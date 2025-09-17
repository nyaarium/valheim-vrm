using System.Threading.Tasks;
using UnityEngine;
using VRMShaders;
using UniGLTF;
using System.IO;
using System;
using Object = UnityEngine.Object;


namespace ValheimVRM
{
    public sealed class TextureDeserializerAsync : UniGLTF.ITextureDeserializer
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

            // Debug PNG header detection
            if (textureInfo.DataMimeType == "image/png" && textureInfo.ImageData.Length >= 25)
            {
                byte colorType = textureInfo.ImageData[25];
                Debug.Log($"[ValheimVRM] DEBUG: PNG #{currentTextureIndex} colorType={colorType}, isIndexed={colorType == 3}");
            }

            // Check if this is an indexed PNG (color type 3)
            bool isIndexedPng = textureInfo.DataMimeType == "image/png" &&
                               textureInfo.ImageData.Length >= 25 &&
                               textureInfo.ImageData[25] == 3;

            if (isIndexedPng)
            {
                Debug.Log($"[ValheimVRM] Detected indexed PNG texture #{currentTextureIndex}, using Unity decoder");
            }

            // Load with Unity's decoder via reflection for PNG/JPEG (handles indexed PNGs)
            bool isPng = textureInfo.DataMimeType == "image/png";
            bool isJpeg = textureInfo.DataMimeType == "image/jpeg" || textureInfo.DataMimeType == "image/jpg";
            bool isKnownImage = isPng || isJpeg || string.IsNullOrEmpty(textureInfo.DataMimeType);

            if (isKnownImage)
            {
                var linear = textureInfo.ColorSpace == UniGLTF.ColorSpace.Linear;
                texture = Utils.TryLoadImageWithUnity(textureInfo.ImageData, linear);
                if (texture == null)
                {
                    LogTextureFailure(textureInfo, currentTextureIndex);
                    texture = CreateFallbackTexture();
                }

                if (texture != null)
                {
                    LogTextureSuccess(textureInfo, currentTextureIndex);
                    texture.wrapModeU = textureInfo.WrapModeU;
                    texture.wrapModeV = textureInfo.WrapModeV;
                    texture.filterMode = textureInfo.FilterMode;
                }
            }

            await awaitCaller.NextFrame();

            return texture;
        }

        private void LogTextureSuccess(UniGLTF.DeserializingTextureInfo textureInfo, int textureIndex)
        {
            var format = GetTextureFormat(textureInfo);
            Debug.Log($"[ValheimVRM] ✅ Loaded texture #{textureIndex} | {textureInfo.ImageData.Length} bytes | {format}");
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
            // Create a simple 1x1 white texture as fallback
            var fallbackTexture = new Texture2D(1, 1);
            fallbackTexture.SetPixel(0, 0, Color.magenta);
            fallbackTexture.Apply();
            return fallbackTexture;
        }

    }

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

            // Debug PNG header detection
            if (textureInfo.DataMimeType == "image/png" && textureInfo.ImageData.Length >= 25)
            {
                byte colorType = textureInfo.ImageData[25];
                Debug.Log($"[ValheimVRM] DEBUG: PNG #{currentTextureIndex} colorType={colorType}, isIndexed={colorType == 3}");
            }

            // Check if this is an indexed PNG (color type 3)
            bool isIndexedPng = textureInfo.DataMimeType == "image/png" &&
                               textureInfo.ImageData.Length >= 25 &&
                               textureInfo.ImageData[25] == 3;

            if (isIndexedPng)
            {
                Debug.Log($"[ValheimVRM] Detected indexed PNG texture #{currentTextureIndex}, using Unity decoder");
            }

            // Load with Unity's decoder via reflection for PNG/JPEG (handles indexed PNGs)
            bool isPng = textureInfo.DataMimeType == "image/png";
            bool isJpeg = textureInfo.DataMimeType == "image/jpeg" || textureInfo.DataMimeType == "image/jpg";
            bool isKnownImage = isPng || isJpeg || string.IsNullOrEmpty(textureInfo.DataMimeType);

            if (isKnownImage)
            {
                var linear = textureInfo.ColorSpace == UniGLTF.ColorSpace.Linear;
                texture = Utils.TryLoadImageWithUnity(textureInfo.ImageData, linear);
                if (texture == null)
                {
                    LogTextureFailure(textureInfo, currentTextureIndex);
                    texture = CreateFallbackTexture();
                }

                if (texture != null)
                {
                    LogTextureSuccess(textureInfo, currentTextureIndex);
                    texture.wrapModeU = textureInfo.WrapModeU;
                    texture.wrapModeV = textureInfo.WrapModeV;
                    texture.filterMode = textureInfo.FilterMode;
                }
            }

            await awaitCaller.NextFrame();

            return texture;
        }

        private void LogTextureSuccess(UniGLTF.DeserializingTextureInfo textureInfo, int textureIndex)
        {
            var format = GetTextureFormat(textureInfo);
            Debug.Log($"[ValheimVRM] ✅ Loaded texture #{textureIndex} | {textureInfo.ImageData.Length} bytes | {format}");
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
            // Create a simple 1x1 white texture as fallback
            var fallbackTexture = new Texture2D(1, 1);
            fallbackTexture.SetPixel(0, 0, Color.white);
            fallbackTexture.Apply();
            return fallbackTexture;
        }
    }

}
