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

        private Texture2D LoadIndexedPngWithUnity(byte[] pngData)
        {
            try
            {
                var texture = new Texture2D(2, 2);
                if (texture.LoadImage(pngData))
                {
                    // Unity's LoadImage handles indexed PNGs automatically
                    // Convert to RGBA32 for consistency with other textures
                    var rgbaTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
                    rgbaTexture.SetPixels32(texture.GetPixels32());
                    rgbaTexture.Apply();

                    Object.DestroyImmediate(texture);
                    return rgbaTexture;
                }
                Object.DestroyImmediate(texture);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ValheimVRM] Unity PNG decoder failed: {ex.Message}");
            }
            return null;
        }

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
                texture = LoadIndexedPngWithUnity(textureInfo.ImageData);
                if (texture != null)
                {
                    LogTextureSuccess(textureInfo, currentTextureIndex);
                    texture.wrapModeU = textureInfo.WrapModeU;
                    texture.wrapModeV = textureInfo.WrapModeV;
                    texture.filterMode = textureInfo.FilterMode;
                    await awaitCaller.NextFrame();
                    return texture;
                }
            }

            // Use AsyncImageLoader for non-indexed textures
            var settings = new AsyncImageLoader.LoaderSettings();
            settings.linear = textureInfo.ColorSpace == UniGLTF.ColorSpace.Linear;

            switch (textureInfo.DataMimeType)
            {
                case "image/png":
                    settings.format = AsyncImageLoader.FreeImage.Format.FIF_PNG;
                    break;
                case "image/jpeg":
                    settings.format = AsyncImageLoader.FreeImage.Format.FIF_JPEG;
                    break;
                default:
                    if (string.IsNullOrEmpty(textureInfo.DataMimeType))
                    {
                        Debug.LogWarning("[ValheimVRM] Texture image MIME type is empty.");
                    }
                    else
                    {
                        Debug.LogWarning($"[ValheimVRM] Texture image MIME type `{textureInfo.DataMimeType}` is not supported.");
                    }
                    break;
            }

            try
            {
                texture = await AsyncImageLoader.CreateFromImageAsync(textureInfo.ImageData, settings);

                if (texture == null)
                {
                    // Try synchronous method as fallback
                    try
                    {
                        texture = AsyncImageLoader.CreateFromImage(textureInfo.ImageData, settings);
                        if (texture != null)
                        {
                            LogTextureSuccess(textureInfo, currentTextureIndex);
                        }
                    }
                    catch (System.Exception)
                    {
                        // Fallback failed, continue
                    }

                    // If still null and PNG, try Unity's decoder (handles indexed PNGs)
                    if (texture == null && textureInfo.DataMimeType == "image/png")
                    {
                        Debug.Log($"[ValheimVRM] Attempting Unity PNG decoder fallback for texture #{currentTextureIndex}");
                        var unityTex = LoadIndexedPngWithUnity(textureInfo.ImageData);
                        if (unityTex != null)
                        {
                            texture = unityTex;
                            LogTextureSuccess(textureInfo, currentTextureIndex);
                        }
                    }

                    // If still null, create a fallback texture
                    if (texture == null)
                    {
                        LogTextureFailure(textureInfo, currentTextureIndex);
                        texture = CreateFallbackTexture();
                    }
                }
                else
                {
                    LogTextureSuccess(textureInfo, currentTextureIndex);
                }

                if (texture != null)
                {
                    texture.wrapModeU = textureInfo.WrapModeU;
                    texture.wrapModeV = textureInfo.WrapModeV;
                    texture.filterMode = textureInfo.FilterMode;
                }
            }
            catch (System.Exception)
            {
                LogTextureFailure(textureInfo, currentTextureIndex);
                texture = CreateFallbackTexture();
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

        private Texture2D LoadIndexedPngWithUnity(byte[] pngData)
        {
            try
            {
                var texture = new Texture2D(2, 2);
                if (texture.LoadImage(pngData))
                {
                    // Unity's LoadImage handles indexed PNGs automatically
                    // Convert to RGBA32 for consistency with other textures
                    var rgbaTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
                    rgbaTexture.SetPixels32(texture.GetPixels32());
                    rgbaTexture.Apply();

                    Object.DestroyImmediate(texture);
                    return rgbaTexture;
                }
                Object.DestroyImmediate(texture);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ValheimVRM] Unity PNG decoder failed: {ex.Message}");
            }
            return null;
        }

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
                texture = LoadIndexedPngWithUnity(textureInfo.ImageData);
                if (texture != null)
                {
                    LogTextureSuccess(textureInfo, currentTextureIndex);
                    texture.wrapModeU = textureInfo.WrapModeU;
                    texture.wrapModeV = textureInfo.WrapModeV;
                    texture.filterMode = textureInfo.FilterMode;
                    await awaitCaller.NextFrame();
                    return texture;
                }
            }

            // Use AsyncImageLoader for non-indexed textures
            var settings = new AsyncImageLoader.LoaderSettings();
            settings.linear = textureInfo.ColorSpace == UniGLTF.ColorSpace.Linear;

            switch (textureInfo.DataMimeType)
            {
                case "image/png":
                    settings.format = AsyncImageLoader.FreeImage.Format.FIF_PNG;
                    break;
                case "image/jpg":
                case "image/jpeg":
                    settings.format = AsyncImageLoader.FreeImage.Format.FIF_JPEG;
                    break;
                default:
                    if (string.IsNullOrEmpty(textureInfo.DataMimeType))
                    {
                        Debug.LogWarning("[ValheimVRM] Texture image MIME type is empty.");
                    }
                    else
                    {
                        Debug.LogWarning($"[ValheimVRM] Texture image MIME type `{textureInfo.DataMimeType}` is not supported.");
                    }
                    break;
            }

            try
            {
                texture = AsyncImageLoader.CreateFromImage(textureInfo.ImageData, settings);

                if (texture == null)
                {
                    LogTextureFailure(textureInfo, currentTextureIndex);
                    texture = CreateFallbackTexture();
                }
                else
                {
                    LogTextureSuccess(textureInfo, currentTextureIndex);
                }

                if (texture != null)
                {
                    texture.wrapModeU = textureInfo.WrapModeU;
                    texture.wrapModeV = textureInfo.WrapModeV;
                    texture.filterMode = textureInfo.FilterMode;
                }
            }
            catch (System.Exception)
            {
                LogTextureFailure(textureInfo, currentTextureIndex);
                texture = CreateFallbackTexture();
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



