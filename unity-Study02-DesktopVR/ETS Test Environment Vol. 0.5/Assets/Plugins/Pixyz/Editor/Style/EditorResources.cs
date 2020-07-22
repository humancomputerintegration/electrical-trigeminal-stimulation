using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Pixyz.Utils;

namespace Pixyz.Editor {

    /// <summary>
    /// Static class for convenient resources (texture primarily) handling in the Unity Editor.
    /// </summary>
    public static class TextureCache {

        private static Dictionary<string, Texture2D> CachedTextures = new Dictionary<string, Texture2D>();

        /// <summary>
        /// Returns a texture from it's resource name. Particularity is that the texture can be downscaled.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lowres"></param>
        /// <returns></returns>
        public static Texture2D GetTexture(string name, bool lowres = false) {
            return GetTexture(name, Color.white, lowres);
        }

        /// <summary>
        /// Returns a texture from it's resource name. Particularity is that the texture can be tinted and downscaled.
        /// All textures returned by this function are cached for optimal performances.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tint"></param>
        /// <param name="lowres"></param>
        /// <returns></returns>
        public static Texture2D GetTexture(string name, Color tint, bool lowres = false) {

            string hash = name + tint.ToInt32() + (lowres? "lr" : null);

            if (CachedTextures.ContainsKey(hash)) {
                var tex = CachedTextures[hash];
                if (!tex) {
                    CachedTextures.Remove(hash); // Repairing if texture is lost
                    tex = GetTexture(name, tint, lowres); 
                }
                return tex;
            }

            Texture2D texture = Resources.Load<Texture2D>(name);
            if (!texture) {
                throw new FileNotFoundException($"File name '{name}' could not be loaded");
            }

            if (tint == Color.clear || tint == Color.white) {
                try {
                    var newTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
                    newTexture.SetPixels32(texture.GetPixels32());
                    newTexture.Apply();
                    texture = newTexture;
                }
                catch (Exception ex) {
                    Debug.LogError(ex.ToString());
                }
            } else {
                try {
                    var newTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
                    Color[] texturePixels = texture.GetPixels();
                    Color[] newTexturePixels = new Color[texturePixels.Length];
                    for (int i = 0; i < texturePixels.Length; i++) {
                        newTexturePixels[i].a = texturePixels[i].a * tint.a;
                        newTexturePixels[i].r = tint.r;
                        newTexturePixels[i].g = tint.g;
                        newTexturePixels[i].b = tint.b;
                    }
                    newTexture.SetPixels(newTexturePixels);
                    newTexture.Apply();
                    texture = newTexture;
                }
                catch (Exception ex) {
                    Debug.LogError(ex.ToString());
                }
            }

            if (lowres) {
                TextureScaler.Point(texture, texture.width / 2, texture.height / 2);
            }

            CachedTextures.Add(hash, texture);

            return texture;
        }
    }
}