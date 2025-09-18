using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ValheimVRM
{
	/// <summary>
	/// Static, thread-safe texture cache manager to eliminate duplicate texture processing during VRM loading and material conversion.
	/// The cache handles reference counting, VRM lifecycle management, and safe eviction of unused textures.
	/// </summary>
	public static class VrmTextureCache
	{
		#region Core Data Structures

		/// <summary>
		/// Primary cache: Texture cache (keyed by content hash)
		/// </summary>
		private static Dictionary<TextureCacheKey, Texture2D> TextureCache = new Dictionary<TextureCacheKey, Texture2D>();

		/// <summary>
		/// Texture info cache (keyed by content hash)
		/// </summary>
		private static Dictionary<TextureCacheKey, TextureInfo> TextureInfoCache = new Dictionary<TextureCacheKey, TextureInfo>();

		/// <summary>
		/// VRM lifecycle tracking
		/// </summary>
		private static Dictionary<string, VrmState> VrmStates = new Dictionary<string, VrmState>();

		/// <summary>
		/// Instance ID to cache key mapping (for Unity texture lookup)
		/// </summary>
		private static Dictionary<int, TextureCacheKey> InstanceToCacheKey = new Dictionary<int, TextureCacheKey>();

		/// <summary>
		/// Texture usage tracking (cache key → set of VRMs using it)
		/// </summary>
		private static Dictionary<TextureCacheKey, HashSet<string>> TextureToVrms = new Dictionary<TextureCacheKey, HashSet<string>>();

		/// <summary>
		/// Thread safety lock
		/// </summary>
		private static readonly object CacheLock = new object();

		/// <summary>
		/// Cache for computed hashes to avoid recomputation
		/// </summary>
		private static Dictionary<int, TextureCacheKey> ComputedHashes = new Dictionary<int, TextureCacheKey>();

		#endregion

		#region Data Structures

		/// <summary>
		/// Key for texture cache based on content hash
		/// </summary>
		public struct TextureCacheKey
		{
			public string ContentHash; // `{color-space}|{length}|{sha256}`

			public TextureCacheKey(string contentHash)
			{
				ContentHash = contentHash;
			}

			public override bool Equals(object obj)
			{
				return obj is TextureCacheKey key && ContentHash == key.ContentHash;
			}

			public override int GetHashCode()
			{
				return ContentHash?.GetHashCode() ?? 0;
			}

			public override string ToString()
			{
				return $"TextureCacheKey({ContentHash})";
			}
		}

		/// <summary>
		/// Texture information for debugging and logging
		/// </summary>
		public class TextureInfo
		{
			public int Width;
			public int Height;
			public TextureFormat Format;
			public string ColorSpace;
			public int DataSize;

			public TextureInfo(int width, int height, TextureFormat format, string colorSpace, int dataSize)
			{
				Width = width;
				Height = height;
				Format = format;
				ColorSpace = colorSpace;
				DataSize = dataSize;
			}

			public override string ToString()
			{
				return $"({Width}x{Height}, {Format}, {ColorSpace}, {DataSize} bytes)";
			}
		}

		/// <summary>
		/// VRM state tracking
		/// </summary>
		public class VrmState
		{
			public byte[] VrmHash;           // sha256 of VRM source bytes
			public HashSet<TextureCacheKey> TextureCacheKeys;  // Textures used by this VRM

			public VrmState(byte[] vrmHash)
			{
				VrmHash = vrmHash;
				TextureCacheKeys = new HashSet<TextureCacheKey>();
			}
		}

		#endregion

		#region Texture Management

		/// <summary>
		/// Get or create cached texture from image data with color space awareness.
		/// Cache key embeds color-space to prevent mixing sRGB textures & linear
		/// </summary>
		public static (Texture2D texture, TextureCacheKey key) GetOrCacheTexture(byte[] imageData, bool linear)
		{
			if (imageData == null || imageData.Length == 0)
			{
				Debug.LogError("[VrmTextureCache] 💽 GetOrCacheTexture: imageData is null or empty");
				return (null, default);
			}

			lock (CacheLock)
			{
				try
				{
					// Generate content hash and include color space qualifier to avoid mixing linear/sRGB instances
					var baseHash = GenerateContentHash(imageData);
					var contentHash = $"{(linear ? "linear" : "srgb")}|{baseHash}";
					var rawKey = new TextureCacheKey(contentHash);


					// Check if already cached
					if (TextureCache.TryGetValue(rawKey, out var existingTexture))
					{
						TextureInfoCache.TryGetValue(rawKey, out var existingInfo);
						Debug.Log($"[VrmTextureCache] 💽 Cache HIT - {existingInfo}");
						return (existingTexture, rawKey);
					}

					// Create new texture
					var texture = Utils.TryLoadImageWithUnity(imageData, linear);
					if (texture == null)
					{
						Debug.LogError($"[VrmTextureCache] 💽 GetOrCacheTexture: Failed to create texture from {imageData.Length} bytes");
						return (null, default);
					}

					// Cache the texture and info
					TextureCache[rawKey] = texture;
					var textureInfo = new TextureInfo(
						texture.width,
						texture.height,
						texture.format,
						linear ? "linear" : "srgb",
						imageData.Length
					);
					TextureInfoCache[rawKey] = textureInfo;
					Debug.Log($"[VrmTextureCache] 💽 Cache MISS - {textureInfo}");

					return (texture, rawKey);
				}
				catch (Exception ex)
				{
					Debug.LogError($"[VrmTextureCache] 💽 GetOrCacheTexture: Exception - {ex.Message}");
					return (null, default);
				}
			}
		}

		/// <summary>
		/// Link texture to a VRM for tracking. Returns true if successfully linked (new reference added).
		/// </summary>
		public static bool LinkTextureToVrm(string vrmName, byte[] vrmHash, TextureCacheKey rawKey)
		{
			if (string.IsNullOrEmpty(vrmName))
			{
				Debug.LogError("[VrmTextureCache] 💽 LinkTextureToVrm: vrmName is null or empty");
				return false;
			}

			if (vrmHash == null)
			{
				Debug.LogError("[VrmTextureCache] 💽 LinkTextureToVrm: vrmHash is required but null");
				return false;
			}

			if (rawKey.ContentHash == null)
			{
				Debug.LogError("[VrmTextureCache] 💽 LinkTextureToVrm: rawKey is invalid");
				return false;
			}

			lock (CacheLock)
			{
				try
				{
					// Ensure VRM state exists
					if (!VrmStates.TryGetValue(vrmName, out var vrmState))
					{
						Debug.LogError($"[VrmTextureCache] 💽 LinkTextureToVrm: VRM state not found for {vrmName}");
						return false;
					}

					// Verify hash matches stored state
					if (!vrmState.VrmHash.SequenceEqual(vrmHash))
					{
						Debug.LogWarning($"[VrmTextureCache] 💽 LinkTextureToVrm: Provided hash doesn't match stored for {vrmName}");
						return false;
					}

					// Add raw key to VRM state
					vrmState.TextureCacheKeys.Add(rawKey);

					// Track texture usage
					if (!TextureToVrms.TryGetValue(rawKey, out var vrmsUsingTexture))
					{
						vrmsUsingTexture = new HashSet<string>();
						TextureToVrms[rawKey] = vrmsUsingTexture;
					}

					bool added = vrmsUsingTexture.Add(vrmName);
					return added;
				}
				catch (Exception ex)
				{
					Debug.LogError($"[VrmTextureCache] 💽 LinkTextureToVrm: Exception - {ex.Message}");
					return false;
				}
			}
		}

		/// <summary>
		/// Get texture cache key from existing texture (material processing path)
		/// First checks ComputedHashes cache, then computes if needed
		/// </summary>
		public static TextureCacheKey? GetTextureKey(Texture2D texture)
		{
			if (texture == null)
			{
				Debug.LogError("[VrmTextureCache] 💽 GetTextureKey: texture is null");
				return null;
			}

			lock (CacheLock)
			{
				try
				{
					var instanceId = texture.GetInstanceID();

					// Check computed hashes cache first
					if (ComputedHashes.TryGetValue(instanceId, out var cachedKey))
					{
						return cachedKey;
					}

					// Check instance mapping
					if (InstanceToCacheKey.TryGetValue(instanceId, out var mappedKey))
					{
						ComputedHashes[instanceId] = mappedKey;
						return mappedKey;
					}

					return null;
				}
				catch (Exception ex)
				{
					Debug.LogError($"[VrmTextureCache] 💽 GetTextureKey: Exception - {ex.Message}");
					return null;
				}
			}
		}

		/// <summary>
		/// Record instance ID mapping for texture lookup
		/// </summary>
		public static void RecordInstanceMapping(int instanceId, TextureCacheKey rawKey)
		{
			lock (CacheLock)
			{
				try
				{
					InstanceToCacheKey[instanceId] = rawKey;
					ComputedHashes[instanceId] = rawKey; // Also cache in computed hashes
				}
				catch (Exception ex)
				{
					Debug.LogError($"[VrmTextureCache] 💽 RecordInstanceMapping: Exception - {ex.Message}");
				}
			}
		}

		#endregion

		#region VRM Lifecycle Management

		/// <summary>
		/// Register VRM for texture tracking (on load start)
		/// </summary>
		public static void RegisterVrm(string vrmName, byte[] vrmHash)
		{
			if (string.IsNullOrEmpty(vrmName))
			{
				Debug.LogError("[VrmTextureCache] 💽 RegisterVrm: vrmName is null or empty");
				return;
			}

			if (vrmHash == null)
			{
				Debug.LogError("[VrmTextureCache] 💽 RegisterVrm: vrmHash is required but null");
				return;
			}

			lock (CacheLock)
			{
				try
				{
					if (VrmStates.ContainsKey(vrmName))
					{
						Debug.LogWarning($"[VrmTextureCache] 💽 RegisterVrm: VRM state already exists for {vrmName}");
						return;
					}

					var vrmState = new VrmState(vrmHash);
					VrmStates[vrmName] = vrmState;
					Debug.Log($"[VrmTextureCache] 💽 RegisterVrm: Created state for {vrmName}");
				}
				catch (Exception ex)
				{
					Debug.LogError($"[VrmTextureCache] 💽 RegisterVrm: Exception - {ex.Message}");
				}
			}
		}

		/// <summary>
		/// Unregister VRM from texture tracking (on unload/player leave). Returns true if state was found and disposed.
		/// </summary>
		public static bool UnregisterVrm(string vrmName, byte[] vrmHash)
		{
			if (string.IsNullOrEmpty(vrmName))
			{
				Debug.LogError("[VrmTextureCache] 💽 UnregisterVrm: vrmName is null or empty");
				return false;
			}

			if (vrmHash == null)
			{
				Debug.LogWarning("[VrmTextureCache] 💽 UnregisterVrm: vrmHash is required but null—skipping");
				return false;
			}

			lock (CacheLock)
			{
				try
				{
					if (!VrmStates.TryGetValue(vrmName, out var vrmState))
					{
						Debug.LogWarning($"[VrmTextureCache] 💽 UnregisterVrm: VRM state not found for {vrmName}");
						return false;
					}

					// Verify hash matches
					if (!vrmState.VrmHash.SequenceEqual(vrmHash))
					{
						Debug.LogWarning($"[VrmTextureCache] 💽 UnregisterVrm: Provided hash doesn't match stored for {vrmName}—skipping");
						return false;
					}

					// Remove VRM from texture usage tracking
					foreach (var rawKey in vrmState.TextureCacheKeys.ToArray()) // ToArray for modification safety
					{
						if (TextureToVrms.TryGetValue(rawKey, out var vrmsUsingTexture))
						{
							if (vrmsUsingTexture.Remove(vrmName))
							{
								Debug.Log($"[VrmTextureCache] 💽 UnregisterVrm: Removed {vrmName} from {rawKey} usage (remaining: {vrmsUsingTexture.Count})");
							}
						}
					}

					// Remove VRM state
					VrmStates.Remove(vrmName);
					Debug.Log($"[VrmTextureCache] 💽 UnregisterVrm: Disposed state for {vrmName}");
					return true;
				}
				catch (Exception ex)
				{
					Debug.LogError($"[VrmTextureCache] 💽 UnregisterVrm: Exception - {ex.Message}");
					return false;
				}
			}
		}

		/// <summary>
		/// Cleanup unused textures
		/// </summary>
		public static void CleanupUnusedTextures()
		{
			lock (CacheLock)
			{
				try
				{
					var texturesToRemove = new List<TextureCacheKey>();

					foreach (var kvp in TextureToVrms.Where(kvp => kvp.Value.Count == 0))
					{
						texturesToRemove.Add(kvp.Key);
					}

					foreach (var rawKey in texturesToRemove)
					{
						if (TextureCache.TryGetValue(rawKey, out var texture))
						{
							// Debug.Log($"[VrmTextureCache] 💽 CleanupUnusedTextures: Destroying unused texture '{texture.name}' ({texture.width}x{texture.height}) for {rawKey}");

							var instanceId = texture.GetInstanceID();
							InstanceToCacheKey.Remove(instanceId);
							ComputedHashes.Remove(instanceId);

							Object.DestroyImmediate(texture);
							TextureCache.Remove(rawKey);
						}
						TextureInfoCache.Remove(rawKey);
						TextureToVrms.Remove(rawKey);
					}

					if (texturesToRemove.Count > 0)
					{
						// Debug.Log($"[VrmTextureCache] 💽 CleanupUnusedTextures: Cleaned up {texturesToRemove.Count} unused textures");
					}
				}
				catch (Exception ex)
				{
					Debug.LogError($"[VrmTextureCache] 💽 CleanupUnusedTextures: Exception - {ex.Message}");
				}
			}
		}

		#endregion

		#region Utility Methods

		/// <summary>
		/// Generate content hash for image data
		/// </summary>
		private static string GenerateContentHash(byte[] imageData)
		{
			try
			{
				using (var sha256 = SHA256.Create())
				{
					var hash = sha256.ComputeHash(imageData);
					return $"{imageData.Length}|{BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant()}";
				}
			}
			catch (Exception ex)
			{
				Debug.LogError($"[VrmTextureCache] 💽 GenerateContentHash: Exception - {ex.Message}");
				return $"error|{imageData?.Length ?? 0}";
			}
		}

		#endregion
	}
}
