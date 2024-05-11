using System.Threading.Tasks;
using UnityEngine;
using VRMShaders;


namespace ValheimVRM
{
    public sealed class TextureDeserializerAsync : ITextureDeserializer
    {
        public async Task<Texture2D> LoadTextureAsync(DeserializingTextureInfo textureInfo, IAwaitCaller awaitCaller)
        {
            
            var settings = new AsyncImageLoader.LoaderSettings();
            settings.linear = textureInfo.ColorSpace == global::VRMShaders.ColorSpace.Linear;

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
                        Debug.Log($"Texture image MIME type is empty.");
                    }
                    else
                    {
                        Debug.Log($"Texture image MIME type `{textureInfo.DataMimeType}` is not supported.");
                    }
                    break;
            }
            Texture2D texture = null;

            texture = await AsyncImageLoader.CreateFromImageAsync(textureInfo.ImageData, settings);

            texture.wrapModeU = textureInfo.WrapModeU;
            texture.wrapModeV = textureInfo.WrapModeV;
            texture.filterMode = textureInfo.FilterMode;
            
            await awaitCaller.NextFrame();
            
            return texture;
        }
        
    }
    
    public sealed class TextureDeserializer: ITextureDeserializer
    {
        public async Task<Texture2D> LoadTextureAsync(DeserializingTextureInfo textureInfo, IAwaitCaller awaitCaller)
        {
            
            var settings = new AsyncImageLoader.LoaderSettings();
            settings.linear = textureInfo.ColorSpace == global::VRMShaders.ColorSpace.Linear;

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
                        Debug.Log($"Texture image MIME type is empty.");
                    }
                    else
                    {
                        Debug.Log($"Texture image MIME type `{textureInfo.DataMimeType}` is not supported.");
                    }
                    break;
            }
            Texture2D texture = null;

            texture = AsyncImageLoader.CreateFromImage(textureInfo.ImageData, settings);

            texture.wrapModeU = textureInfo.WrapModeU;
            texture.wrapModeV = textureInfo.WrapModeV;
            texture.filterMode = textureInfo.FilterMode;
            
            await awaitCaller.NextFrame();
            
            return texture;
        }
    }

    
    
}



 