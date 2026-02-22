using System.Threading.Tasks;
using UnityEngine;
using VRMShaders;
using UniGLTF;
using System.IO;
using System;
using Object = UnityEngine.Object;

namespace ValheimVRM
{
    public sealed class TextureDeserializer : UniGLTF.ITextureDeserializer
    {
        private static int textureLoadCounter = 0;

        public async Task<Texture2D> LoadTextureAsync(UniGLTF.DeserializingTextureInfo textureInfo, UniGLTF.IAwaitCaller awaitCaller)
        {
            if (textureInfo?.ImageData == null)
            {
                Debug.LogError("[ValheimVRM] TextureInfo or ImageData is null");
                return CreateFallbackTexture();
            }

            Texture2D texture = null;
            int currentTextureIndex = ++textureLoadCounter;

            // Load with Unity's decoder via reflection for PNG/JPEG (handles indexed PNGs)
            bool isPng = textureInfo.DataMimeType == "image/png";
            bool isJpeg = textureInfo.DataMimeType == "image/jpeg" || textureInfo.DataMimeType == "image/jpg";
            bool isKnownImage = isPng || isJpeg || string.IsNullOrEmpty(textureInfo.DataMimeType);

            if (isKnownImage)
            {
                // Integrate VrmTextureCache: Use cache first for deduplication
                var linear = textureInfo.ColorSpace == UniGLTF.ColorSpace.Linear;
                var (cachedTexture, rawKey) = VrmTextureCache.GetOrCacheTexture(textureInfo.ImageData, linear);
                if (cachedTexture != null)
                {
                    texture = cachedTexture;
                    VrmTextureCache.RecordInstanceMapping(texture.GetInstanceID(), rawKey);
                }
                else
                {
                    // Fallback to original Unity load
                    texture = Utils.TryLoadImageWithUnity(textureInfo.ImageData, linear);
                    if (texture != null)
                    {
                        // Cache the newly loaded texture
                        var (newCached, newKey) = VrmTextureCache.GetOrCacheTexture(textureInfo.ImageData, linear);
                        if (newCached != null)
                        {
                            texture = newCached; // Replace with cached version
                            VrmTextureCache.RecordInstanceMapping(texture.GetInstanceID(), newKey);
                        }
                    }
                    else
                    {
                        LogTextureFailure(textureInfo, currentTextureIndex);
                        texture = CreateFallbackTexture();
                    }
                }

                if (texture != null)
                {
                    texture.wrapModeU = textureInfo.WrapModeU;
                    texture.wrapModeV = textureInfo.WrapModeV;
                    texture.filterMode = textureInfo.FilterMode;
                    texture.anisoLevel = 4;

                    var __sw = System.Diagnostics.Stopwatch.StartNew();
                    var __sha = System.Security.Cryptography.SHA256.Create().ComputeHash(textureInfo.ImageData);
                    __sw.Stop();
                }
            }

            await awaitCaller.NextFrame();

            return texture;
        }

        private void LogTextureFailure(UniGLTF.DeserializingTextureInfo textureInfo, int textureIndex)
        {
            var format = GetTextureFormat(textureInfo);
            Debug.LogWarning($"[ValheimVRM] ❌ Failed to load texture #{textureIndex} | {textureInfo.ImageData.Length} bytes | {format}");
        }

        private string GetTextureFormat(UniGLTF.DeserializingTextureInfo textureInfo)
        {
            if (textureInfo.DataMimeType == "image/png" && textureInfo.ImageData.Length >= 24)
            {
                try
                {
                    var bitDepth = textureInfo.ImageData[24];
                    var colorType = textureInfo.ImageData[25];

                    string colorTypeStr;
                    switch (colorType)
                    {
                        case 0: colorTypeStr = "Grayscale"; break;
                        case 2: colorTypeStr = "RGB"; break;
                        case 3: colorTypeStr = "Indexed"; break;
                        case 4: colorTypeStr = "Grayscale+Alpha"; break;
                        case 6: colorTypeStr = "RGBA"; break;
                        default: colorTypeStr = "Unknown"; break;
                    }

                    return $"{bitDepth}-bit | {colorTypeStr}";
                }
                catch
                {
                    return "Unknown format";
                }
            }
            else if (textureInfo.DataMimeType == "image/jpeg")
            {
                return "JPEG";
            }

            return "Unknown format";
        }

        private Texture2D CreateFallbackTexture()
        {
            // Create a simple 1x1 magenta texture as fallback (consistent with Async version)
            var fallbackTexture = new Texture2D(1, 1);
            fallbackTexture.SetPixel(0, 0, Color.magenta);
            fallbackTexture.Apply();
            return fallbackTexture;
        }
    }
}
