using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ValheimVRM
{
	public static class Utils
	{
		public static Tout GetField<Tin, Tout>(this Tin self, string fieldName)
		{
			return AccessTools.FieldRefAccess<Tin, Tout>(fieldName).Invoke(self);
		}

		public static string GetHaxadecimalString(this IEnumerable<byte> self)
		{
			if (self == null) return "none";

			StringBuilder hex = new StringBuilder(self.Count() * 2);
			foreach (byte b in self) hex.AppendFormat("{0:x2}", b);
			return hex.ToString();
		}

		public static V GetOrCreateDefault<K, V>(this IDictionary<K, V> self, K key) where V : new()
		{
			if (self.ContainsKey(key)) return self[key];

			var newVal = new V();
			self[key] = newVal;
			return newVal;
		}

		public static bool CompareArrays<T>(IEnumerable<T> a, IEnumerable<T> b) => ((a == null) == (b == null)) &&
			((a != null && b != null) ? Enumerable.SequenceEqual(a, b) : true);


		public static FieldInfo GetField<T>(string name) =>
			typeof(T).GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

		public static MethodInfo GetMethod<T>(string name) =>
			typeof(T).GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

		public static int FindOp(this List<CodeInstruction> self, OpCode code, int from = 0) =>
			self.FindIndex(from, inst => inst.opcode == code);
		public static int FindOp(this List<CodeInstruction> self, OpCode code, object operand, int from = 0) =>
			self.FindIndex(from, inst => inst.opcode == code && inst.operand.Equals(operand));

		public static bool IsOp(CodeInstruction self, OpCode code) =>
			self.opcode == code;
		public static bool IsOp(this CodeInstruction self, OpCode code, object operand) =>
			self.opcode == code && self.operand.Equals(operand);

		/// <summary>
		/// Sends a notification message to the local player's message HUD
		/// </summary>
		public static void SendNotification(string message, MessageHud.MessageType messageType = MessageHud.MessageType.Center)
		{
			if (MessageHud.instance != null)
			{
				MessageHud.instance.ShowMessage(messageType, message);
			}
		}

		/// <summary>
		/// Tries to load image data into a Unity Texture2D using reflection for weird textures that crash AsyncImageLoader
		/// </summary>
		public static Texture2D TryLoadImageWithUnity(byte[] imageData, bool linear)
		{
			try
			{
				var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, linear);
				var imageConversionType = System.Type.GetType("UnityEngine.ImageConversion, UnityEngine.ImageConversionModule")
					?? System.Type.GetType("UnityEngine.ImageConversion, UnityEngine");
				if (imageConversionType == null)
				{
					Object.DestroyImmediate(texture);
					return null;
				}
				var loadImage = imageConversionType.GetMethod(
					"LoadImage",
					new System.Type[] { typeof(Texture2D), typeof(byte[]), typeof(bool) }
				);
				if (loadImage == null)
				{
					Object.DestroyImmediate(texture);
					return null;
				}
				var ok = (bool)loadImage.Invoke(null, new object[] { texture, imageData, false });
				if (ok)
				{
					return texture;
				}
				Object.DestroyImmediate(texture);
			}
			catch (System.Exception ex)
			{
				Debug.LogWarning($"[ValheimVRM] Unity reflection image load failed: {ex.Message}");
			}
			return null;
		}

		/// <summary>
		/// Applies all material properties for Valheim player shader
		/// </summary>
		public static void ApplyMaterialProperties(Material mat, Shader shader, Texture2D tex, Texture2D bumpMap, Color color)
		{
			mat.shader = shader;
			mat.SetTexture("_MainTex", tex);
			mat.SetTexture("_SkinBumpMap", bumpMap);
			mat.SetColor("_SkinColor", color);
			mat.SetTexture("_ChestTex", tex);
			mat.SetTexture("_ChestBumpMap", bumpMap);
			mat.SetTexture("_LegsTex", tex);
			mat.SetTexture("_LegsBumpMap", bumpMap);
			mat.SetFloat("_Glossiness", 0.2f);
			mat.SetFloat("_MetalGlossiness", 0.0f);
		}
	}
}
