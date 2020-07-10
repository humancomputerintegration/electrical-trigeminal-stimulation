using Pixyz.Import;
using Pixyz.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Pixyz.Plugin4Unity;

namespace Pixyz.Interface
{

    public static class InterfaceConversions
    {

        #region Settings
        public static NativeInterface.ImportSettings ToInterfaceObject(this ImportSettings settings, ImportSettingsTemplate template)
        {
            NativeInterface.ImportSettings nsettings = new NativeInterface.ImportSettings();
            nsettings.importLines = (template.importLines.status == ParameterAvailability.Available) ? settings.importLines : template.importLines.defaultValue;
            nsettings.importPatchContours = (template.importPatchBorders.status == ParameterAvailability.Available) ? settings.importPatchBorders : template.importPatchBorders.defaultValue;
            nsettings.importPoints = (template.importPoints.status == ParameterAvailability.Available) ? settings.importPoints : template.importPoints.defaultValue;
            nsettings.mapUV3dSize = (template.mapUV.status == ParameterAvailability.Available) ? (settings.mapUV ? settings.mapUV3dSize * 1000 : -1) : (template.mapUV.defaultValue ? template.mapUV3dSize.defaultValue : -1);
            nsettings.UVPadding = (template.uvPadding.status == ParameterAvailability.Available) ? settings.uvPadding : template.uvPadding.defaultValue;
            nsettings.lightMapResolution = (template.createLightmapUV.status == ParameterAvailability.Available) ? (settings.createLightmapUV ? settings.lightmapResolution : -1) : (template.createLightmapUV.defaultValue ? template.lightmapResolution.defaultValue : -1);
            nsettings.mergeFinalLevel = (template.mergeFinalLevel.status == ParameterAvailability.Available) ? settings.mergeFinalLevel : template.mergeFinalLevel.defaultValue;
            nsettings.treeProcess = (template.treeProcess.status == ParameterAvailability.Available) ? settings.treeProcess : template.treeProcess.defaultValue; ;
            nsettings.voxelizeGridSize = (template.voxelizeGridSize.status == ParameterAvailability.Available) ? settings.voxelizeGridSize : template.voxelizeGridSize.defaultValue;
            nsettings.orient = (template.orient.status == ParameterAvailability.Available) ? settings.orient : template.orient.defaultValue;
            nsettings.support32BytesIndex = !((template.splitTo16BytesIndex.status == ParameterAvailability.Available) ? settings.splitTo16BytesIndex : template.splitTo16BytesIndex.defaultValue);
            var lodSettings = ((template.qualities.status == ParameterAvailability.Available) ? settings.qualities : template.qualities.defaultValue).ToInterfaceObject(settings, template);
            nsettings.meshQualities = lodSettings.Item1;
            nsettings.lodDistance = lodSettings.Item2;
            nsettings.singularizeSymmetries = (template.singularizeSymmetries.status == ParameterAvailability.Available) ? settings.singularizeSymmetries : template.singularizeSymmetries.defaultValue;
            nsettings.repair = (template.repair.status == ParameterAvailability.Available) ? settings.repair : template.repair.defaultValue;
            nsettings.combinePatchesByMaterial = (template.combinePatchesByMaterial.status == ParameterAvailability.Available) ? settings.combinePatchesByMaterial : template.combinePatchesByMaterial.defaultValue;
            return nsettings;
        }
        #endregion

        #region LODs
        public static (NativeInterface.MeshQualityList, NativeInterface.DoubleList) ToInterfaceObject(this LodsGenerationSettings unityObject, ImportSettings settings, ImportSettingsTemplate template)
        {
            var iLods = new List<MeshQuality>();
            var iDistances = new List<double>();

            if (settings.hasLODs)
            {
                if (template.name == "Point Cloud")
                {
                    for (int l = 0; l < settings.lodCount; l++)
                    {
                        iLods.Add(MeshQuality.MAXIMUM);
                        iDistances.Add(1);
                    }
                }
                else
                {
                    for (int l = 0; l < unityObject.lods.Length; l++)
                    {
                        if (unityObject.lods[l].quality == LodQuality.CULLED)
                            continue;
                        iLods.Add((MeshQuality)unityObject.lods[l].quality);
                        iDistances.Add(unityObject.lods[l].threshold);
                    }
                }
            }
            else
            {
                iLods.Add((MeshQuality)unityObject.lods[0].quality);
                iDistances.Add((6.0 - (int)unityObject.lods[0].quality) / 6.0);
            }

            return (new NativeInterface.MeshQualityList(iLods.ToArray()), new NativeInterface.DoubleList(iDistances.ToArray()));
        }

        #endregion

        #region Materials

        public static bool HasTexture(this Material mat, string property)
        {
            return mat.HasProperty(property) && mat.GetTexture(property) != null;
        }

        public static NativeInterface.MaterialDefinition ToNative(this Material material, ref Dictionary<int, NativeInterface.ImageDefinition> images)
        {

            var nativeMaterial = new NativeInterface.MaterialDefinition();
            nativeMaterial.name = material.name;
            nativeMaterial.id = material.GetInstanceID().ToUInt32();

            Color mainColor = Color.white;
            if (material.HasProperty("_Color")) {
                mainColor = material.GetColor("_Color");
            } else if (material.HasProperty("_BaseColor")) {
                mainColor = material.GetColor("_BaseColor");
            }

            nativeMaterial.opacity = new NativeInterface.CoeffOrTexture();
            nativeMaterial.opacity._type = NativeInterface.CoeffOrTexture.Type.COEFF;
            nativeMaterial.opacity.coeff = mainColor.a;

            nativeMaterial.albedo = new NativeInterface.ColorOrTexture();
            nativeMaterial.albedo._type = material.HasTexture("_MainTex") ? NativeInterface.ColorOrTexture.Type.TEXTURE : NativeInterface.ColorOrTexture.Type.COLOR;
            if (nativeMaterial.albedo._type == NativeInterface.ColorOrTexture.Type.TEXTURE) {
                nativeMaterial.albedo.texture = GetNativeTexture(material, "_MainTex", ref images);
            } else {
                nativeMaterial.albedo.color = mainColor.ToInterfaceObject();
            }

            nativeMaterial.normal = new NativeInterface.ColorOrTexture();
            nativeMaterial.normal._type = material.HasTexture("_BumpMap") ? NativeInterface.ColorOrTexture.Type.TEXTURE : NativeInterface.ColorOrTexture.Type.COLOR;
            if (nativeMaterial.normal._type == NativeInterface.ColorOrTexture.Type.TEXTURE) {
                nativeMaterial.normal.texture = GetNativeTexture(material, "_BumpMap", ref images);
            } else {
                nativeMaterial.normal.color = Color.black.ToInterfaceObject();
            }

            nativeMaterial.metallic = new NativeInterface.CoeffOrTexture();
            nativeMaterial.metallic._type = material.HasTexture("_MetallicGlossMap") ? NativeInterface.CoeffOrTexture.Type.TEXTURE : NativeInterface.CoeffOrTexture.Type.COEFF;
            if (nativeMaterial.metallic._type == NativeInterface.CoeffOrTexture.Type.TEXTURE) {
                nativeMaterial.metallic.texture = GetNativeTexture(material, "_MetallicGlossMap", ref images);
            } else {
                nativeMaterial.metallic.coeff = material.TryGetFloat("_Metallic");
            }

            nativeMaterial.roughness = new NativeInterface.CoeffOrTexture();
            nativeMaterial.roughness._type = NativeInterface.CoeffOrTexture.Type.COEFF;
            nativeMaterial.roughness.coeff = 1 - material.TryGetFloat("_Glossiness");

            nativeMaterial.ao = new NativeInterface.CoeffOrTexture();
            nativeMaterial.ao._type = material.HasTexture("_OcclusionMap") ? NativeInterface.CoeffOrTexture.Type.TEXTURE : NativeInterface.CoeffOrTexture.Type.COEFF;
            if (nativeMaterial.ao._type == NativeInterface.CoeffOrTexture.Type.TEXTURE) {
                nativeMaterial.ao.texture = GetNativeTexture(material, "_OcclusionMap", ref images);
            } else {
                nativeMaterial.ao.coeff = material.TryGetFloat("_OcclusionStrength");
            }

            return nativeMaterial;
        }

        public static NativeInterface.MaterialDefinition ToNativeNoTextures(this Material material)
        {
            var nativeMaterial = new NativeInterface.MaterialDefinition();
            nativeMaterial.name = material.name;
            nativeMaterial.id = material.GetInstanceID().ToUInt32();

            Color mainColor = Color.white;
            if (material.HasProperty("_Color")) {
                mainColor = material.GetColor("_Color");
            } else if (material.HasProperty("_BaseColor")) {
                mainColor = material.GetColor("_BaseColor");
            }

            nativeMaterial.opacity = new NativeInterface.CoeffOrTexture();
            nativeMaterial.opacity._type = NativeInterface.CoeffOrTexture.Type.COEFF;
            nativeMaterial.opacity.coeff = mainColor.a;

            nativeMaterial.albedo = new NativeInterface.ColorOrTexture();
            nativeMaterial.albedo._type = NativeInterface.ColorOrTexture.Type.COLOR;
            nativeMaterial.albedo.color = mainColor.ToInterfaceObject();

            nativeMaterial.normal = new NativeInterface.ColorOrTexture();
            nativeMaterial.normal._type = NativeInterface.ColorOrTexture.Type.COLOR;
            nativeMaterial.normal.color = Color.black.ToInterfaceObject();

            nativeMaterial.metallic = new NativeInterface.CoeffOrTexture();
            nativeMaterial.metallic._type = NativeInterface.CoeffOrTexture.Type.COEFF;
            nativeMaterial.metallic.coeff = material.TryGetFloat("_Metallic");

            nativeMaterial.roughness = new NativeInterface.CoeffOrTexture();
            nativeMaterial.roughness._type = NativeInterface.CoeffOrTexture.Type.COEFF;
            nativeMaterial.roughness.coeff = 1 - material.TryGetFloat("_Glossiness");

            nativeMaterial.ao = new NativeInterface.CoeffOrTexture();
            nativeMaterial.ao._type = NativeInterface.CoeffOrTexture.Type.COEFF;
            nativeMaterial.ao.coeff = material.TryGetFloat("_OcclusionStrength");

            return nativeMaterial;
        }

        public static NativeInterface.Texture GetNativeTexture(Material material, string textureProperty, ref Dictionary<int, NativeInterface.ImageDefinition> images)
        {
            NativeInterface.Texture nativeTexture = null;
            if (!material.HasProperty(textureProperty))
                return null; // Material has no such texture property
            var texture2D = material.GetTexture(textureProperty) as Texture2D;
            if (!texture2D)
                return null; // Material has no texture assigned to that field
            NativeInterface.ImageDefinition image;
            if (!images.TryGetValue(texture2D.GetInstanceID(), out image)) {
                try {
                    image = texture2D.ToImageDefinition();
                } catch (Exception exception) {
                    Debug.LogError($"Texture '{texture2D.name}' on Material '{material.name}'({textureProperty}) conversion failed : {exception}");
                    return null;
                }
                images.Add(texture2D.GetInstanceID(), image);
            }
            Vector2 offset = material.GetTextureOffset(textureProperty);
            Vector2 scale = material.GetTextureScale(textureProperty);
            nativeTexture = image.ToTexture(offset, scale);
            return nativeTexture;
        }

        private static void SetTexture(this Material material, string property, NativeInterface.Texture textureExtract, List<Texture2D> textures)
        {
            int textureIndex = (int)textureExtract.image;
            if (!material.HasProperty(property) || textures.Count <= textureIndex)
                return;
            material.SetTextureOffset(property, new Vector2((float)textureExtract.offset.x, (float)textureExtract.offset.y));
            material.SetTextureScale(property, new Vector2((float)textureExtract.tilling.x, (float)textureExtract.tilling.y));
            material.SetTexture(property, textures[(int)textureExtract.image]);
        }

        public static Material ToUnityObject(this NativeInterface.MaterialDefinition interfaceObject, Shader shader, List<Texture2D> textures)
        {
            shader = shader ?? SceneExtensions.GetDefaultShader();

            Material mat = new Material(shader);

            mat.name = interfaceObject.name;

            float alpha = 1f;
            if (interfaceObject.opacity._type == NativeInterface.CoeffOrTexture.Type.TEXTURE) {
                CombineAlbedoAndOpacity(interfaceObject.albedo.texture, interfaceObject.opacity.texture, textures);
                mat.SetFloat("_Mode", 1);
                mat.SetInt("_SrcBlend", (int)BlendMode.One);
                mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            } else {
                alpha = (float)interfaceObject.opacity.coeff;
                if (alpha < 1f) {
                    mat.SetFloat("_Mode", 3);
                    mat.SetInt("_SrcBlend", (int)BlendMode.One);
                    mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.DisableKeyword("_ALPHABLEND_ON");
                    mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = 3000;
                }
            }

            if (interfaceObject.albedo._type == NativeInterface.ColorOrTexture.Type.COLOR) {
                Color color = new Color((float)interfaceObject.albedo.color.r, (float)interfaceObject.albedo.color.g, (float)interfaceObject.albedo.color.b, alpha);
                mat.SetColor("_Color", color);
                mat.SetColor("_BaseColor", color); // Hdrp and Lwrp
            } else {
                mat.SetTexture("_MainTex", interfaceObject.albedo.texture, textures);
                mat.SetTexture("_BaseColorMap", interfaceObject.albedo.texture, textures);
            }

            if (interfaceObject.normal._type != NativeInterface.ColorOrTexture.Type.COLOR) {
                mat.EnableKeyword("_NORMALMAP");
                mat.SetTexture("_BumpMap", interfaceObject.normal.texture, textures);
            }

            if (interfaceObject.roughness._type == NativeInterface.CoeffOrTexture.Type.TEXTURE) {
                mat.EnableKeyword("_METALLICGLOSSMAP");
                Texture2D roughness = textures[(int)interfaceObject.roughness.texture.image];
                Texture2D specular = roughness;
                if (interfaceObject.metallic._type == NativeInterface.CoeffOrTexture.Type.TEXTURE) {
                    specular = textures[(int)interfaceObject.metallic.texture.image];
                } else {
                    mat.SetFloat("_Metallic", Mathf.Clamp((float)interfaceObject.metallic.coeff, 0, 1));
                }
                mat.SetTexture("_MetallicGlossMap", CreateSpecGlossMap(specular, roughness));
            } else {
                mat.SetFloat("_Glossiness", 1 - Mathf.Clamp((float)interfaceObject.roughness.coeff, 0, 1));
            }

            if (interfaceObject.ao._type == NativeInterface.CoeffOrTexture.Type.COEFF)
                mat.SetFloat("_OcclusionStrength", Mathf.Clamp((float)interfaceObject.ao.coeff, 0, 1));
            else
                mat.SetTexture("_OcclusionMap", interfaceObject.ao.texture, textures);

            return mat;
        }

        public static Color TryGetColor(this Material material, string colorPropertyName, Color defaultColor)
        {
            if (!material.HasProperty(colorPropertyName)) {
                return defaultColor;
            }
            return material.GetColor(colorPropertyName);
        }

        public static float TryGetFloat(this Material material, string floatPropertyName, float defaultValue = 0f)
        {
            if (!material.HasProperty(floatPropertyName)) {
                return defaultValue;
            }
            return material.GetFloat(floatPropertyName);
        }

        public static Texture2D ToUnityObject(this NativeInterface.ImageDefinition imageDefinition)
        {
            TextureFormat format = TextureFormat.Alpha8;
            switch (imageDefinition.componentsCount) {
                case 1:
                    if (imageDefinition.bitsPerComponent == 8)
                        format = TextureFormat.R8;
                    else if (imageDefinition.bitsPerComponent == 16)
                        format = TextureFormat.R16;
                    break;
                case 2:
                    if (imageDefinition.bitsPerComponent == 8)
                        format = TextureFormat.RG16;
                    break;
                case 3:
                    if (imageDefinition.bitsPerComponent == 8)
                        format = TextureFormat.RGB24;
                    break;
                case 4:
                    if (imageDefinition.bitsPerComponent == 8)
                        format = TextureFormat.RGBA32;
                    else if (imageDefinition.bitsPerComponent == 8)
                        format = TextureFormat.BGRA32;
                    else if (imageDefinition.bitsPerComponent == 8)
                        format = TextureFormat.ARGB32;
                    break;
                default:
                    break;
            }
            var texture = new Texture2D(imageDefinition.width, imageDefinition.height, format, false);
            texture.name = imageDefinition.name;
            if (imageDefinition.width <= 0 || imageDefinition.height <= 0) {
                Debug.LogWarning($"Texture '{texture.name}' is 0x0 and was ignored");
                return null;
            }
            if (imageDefinition.width > 8192 || imageDefinition.height > 8192) {
                Debug.LogWarning($"Texture '{texture.name}' is larger than 8192, which is not supported");
                return null;
            }
            try {
                if (imageDefinition.data != null && imageDefinition.data.length > 0) {
                    texture.LoadRawTextureData(imageDefinition.data);
                }
            } catch {
                Debug.LogWarning($"Texture data for '{texture.name}' is corrupted and couldn't be converted properly.");
            }
            texture.Apply();
            //if (tex.name.StartsWith("n_")) {
            //tex = ConvertToNormalMap(tex);
            //}
            return texture;
        }

        internal static Texture2D ConvertToNormalMap(this Texture2D texture)
        {
            Texture2D newTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            newTexture.name = texture.name;

            Color[] colors = texture.GetPixels();
            for (int i = 0; i < colors.Length; i++) {
                Color c = colors[i];
                // This seems to be really close the the "Fix Now" available in the Unity Editor.
                // See "UnpackNormal" methods in UnityCG.cginc
                colors[i] = new Color(c.r, 1f - c.g, c.b, c.a);
                //colors[i] = new Color(color.r, color.g, color.b);
                //colors[i] = new Color(0, 1f - c.g, 0, c.r);
                //colors[i] = new Color(0.5f * (1f + color.r), 0.5f * (1f + color.g), 0.5f * (1f + color.b));
                //colors[i] = new Color(0.5f * (1f + c.r), 0.5f * (1f + c.g), 0f, 0.975f);
                //c.r = c.a * 2 - 1;  //red<-alpha (x<-w)
                //c.g = c.g * 2 - 1; //green is always the same (y)
                //Vector2 xy = new Vector2(c.r, c.g); //this is the xy vector
                //c.b = Mathf.Sqrt(1 - Mathf.Clamp01(Vector2.Dot(xy, xy))); //recalculate the blue channel (z)
                //colors[i] = new Color(c.r * 0.5f + 0.5f, c.g * 0.5f + 0.5f, c.b * 0.5f + 0.5f); //back to 0-1 range
            }
            newTexture.SetPixels(colors);

            newTexture.Apply();
            return newTexture;
        }

        internal static Texture2D CreateSpecGlossMap(Texture2D specular, Texture2D roughness)
        {
            if (specular.width != roughness.width || specular.height != roughness.height) {
                Debug.LogError("Specular/metallic map and roughness map must be of the same size");
                return specular;
            }

            Texture2D specGloss = new Texture2D(specular.width, specular.height, TextureFormat.RGBA32, false);
            specGloss.name = specular.name;

            Color[] colorsSpecular = specular.GetPixels();
            Color[] colorsRoughness = roughness.GetPixels();

            for (int i = 0; i < colorsSpecular.Length; i++) {
                Color colorSpecular = colorsSpecular[i];
                Color colorRoughness = colorsRoughness[i];
                colorsSpecular[i] = new Color(colorSpecular.r, colorSpecular.g, colorSpecular.b, 1f - colorRoughness.r);
            }

            specGloss.SetPixels(colorsSpecular);
            specGloss.Apply();
            return specGloss;
        }

        internal static void CombineAlbedoAndOpacity(NativeInterface.Texture albedo, NativeInterface.Texture opacity, List<Texture2D> textures)
        {
            Texture2D albedotex = textures[(int)albedo.image];
            Texture2D opacitytex = textures[(int)opacity.image];

            Texture2D combined = new Texture2D(albedotex.width, albedotex.height, TextureFormat.RGBA32, false);

            Color[] albedoColors = albedotex.GetPixels();
            Color[] opacityColors = opacitytex.GetPixels();
            for (int i = 0; i < albedoColors.Length; i++) {
                Color c = albedoColors[i];
                albedoColors[i] = new Color(c.r, c.g, c.b, opacityColors[i].r);
            }
            combined.SetPixels(albedoColors);
            combined.Apply();

            textures[(int)albedo.image] = combined;
        }

        public static NativeInterface.ImageDefinition ToImageDefinition(this Texture2D texture)
        {
            var image = new NativeInterface.ImageDefinition();
            image.name = texture.name;
            image.width = texture.width;
            image.height = texture.height;
            image.data = new NativeInterface.ByteList();
            switch (texture.format) {
                case TextureFormat.R8:
                    image.bitsPerComponent = 8;
                    image.componentsCount = 1;
                    image.data.list = texture.GetRawTextureData();
                    break;
                case TextureFormat.R16:
                    image.bitsPerComponent = 8;
                    image.componentsCount = 2;
                    image.data.list = texture.GetRawTextureData();
                    break;
                case TextureFormat.RGB24:
                    image.bitsPerComponent = 8;
                    image.componentsCount = 3;
                    image.data.list = texture.GetRawTextureData();
                    break;
                case TextureFormat.RGBA32:
                case TextureFormat.BGRA32:
                case TextureFormat.ARGB32:
                    image.bitsPerComponent = 8;
                    image.componentsCount = 4;
                    image.data.list = texture.GetRawTextureData();
                    break;
                default:
                    // The texture data format is not handled, so we can't get just copy the raw texture data.
                    // In this case, we simply copy colors (4 bytes) to a flat array of bytes.
                    image.bitsPerComponent = 8;
                    image.componentsCount = 4;
                    Color32[] colors;
                    if (!texture.isReadable) {
                        // If the texture is not readable, we make a copy of it
                        byte[] tmp = texture.GetRawTextureData();
                        var tempTex = new Texture2D(texture.width, texture.height, texture.format, false);
                        tempTex.LoadRawTextureData(tmp);
                        colors = tempTex.GetPixels32();
                        UnityEngine.Object.DestroyImmediate(tempTex);
                    } else {
                        colors = texture.GetPixels32();
                    }
                    image.data.list = new byte[colors.Length * 4];
                    // Copy color data
                    Buffer.BlockCopy(colors, 0, image.data.list, 0, image.data.list.Length);
                    break;
            }
            return image;
        }

        public static NativeInterface.Texture ToTexture(this NativeInterface.ImageDefinition image, Vector2 offset, Vector2 scale)
        {
            var texture = new NativeInterface.Texture();
            texture.image = image.id;
            texture.offset = new NativeInterface.Point2();
            texture.offset.x = offset.x;
            texture.offset.y = offset.y;
            texture.tilling = new NativeInterface.Point2();
            texture.tilling.x = scale.x;
            texture.tilling.y = scale.y;
            return texture;
        }

        private static NativeInterface.Color ToInterfaceObject(this Color unityObject)
        {
            return new NativeInterface.Color { r = unityObject.r, g = unityObject.g, b = unityObject.b };
        }

        private static Color ToUnityObject(this NativeInterface.Color interfaceColor)
        {
            return new Color((float)interfaceColor.r, (float)interfaceColor.g, (float)interfaceColor.b);
        }

        public static Texture2D RgbToDxtnm(this Texture2D texture)
        {
            if (texture == null)
                return null;
            var newTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            newTexture.name = texture.name;
            Color[] colors = texture.GetPixels();
            for (int i = 0; i < colors.Length; i++) {
                Color c = colors[i];
                //colors[i] = new Color(1f, c.g, 1f, c.r);
                colors[i] = new Color(0.5f * (1f + c.r), 0.5f * (1f + c.g), 0f, 0.975f);
            }
            newTexture.SetPixels(colors);
            newTexture.Apply();
            return newTexture;
        }

        public static Texture2D DxtnmToRgb(this Texture2D texture)
        {
            if (texture == null)
                return null;
            var newTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            newTexture.name = texture.name;
            Color[] colors = texture.GetPixels();
            for (int i = 0; i < colors.Length; i++) {
                Color c = colors[i];
                c.r = c.a * 2 - 1; // red <- alpha
                c.g = c.g * 2 - 1; // green is always the same
                var xy = new Vector2(c.r, c.g); // This is the xy vector
                c.b = Mathf.Sqrt(1 - Mathf.Clamp01(Vector2.Dot(xy, xy))); // Recalculate the blue channel (z)
                colors[i] = new Color(c.r * 0.5f + 0.5f, c.g * 0.5f + 0.5f, c.b * 0.5f + 0.5f); // Back to 0-1 range
            }
            newTexture.SetPixels(colors);
            newTexture.Apply();
            return newTexture;
        }

        internal static Texture2D Invert(this Texture2D texture)
        {
            if (texture == null)
                return null;
            var newTexture = new Texture2D(texture.width, texture.height, texture.format, false);
            newTexture.name = texture.name;
            Color[] colors = texture.GetPixels();
            for (int i = 0; i < colors.Length; i++) {
                Color c = colors[i];
                colors[i] = new Color(1 - c.r, 1 - c.g, 1 - c.b, 1 - c.a);
            }
            newTexture.SetPixels(colors);
            newTexture.Apply();
            return newTexture;
        }

        #endregion

        #region Matrix
        public static Matrix4x4 ToUnityObject(this NativeInterface.Matrix4 mat)
        {
            Matrix4x4 matrix = new Matrix4x4();
            matrix.m00 = (float)mat[0][0];
            matrix.m01 = (float)mat[0][1];
            matrix.m02 = (float)mat[0][2];
            matrix.m03 = (float)mat[0][3];
            matrix.m10 = (float)mat[1][0];
            matrix.m11 = (float)mat[1][1];
            matrix.m12 = (float)mat[1][2];
            matrix.m13 = (float)mat[1][3];
            matrix.m20 = (float)mat[2][0];
            matrix.m21 = (float)mat[2][1];
            matrix.m22 = (float)mat[2][2];
            matrix.m23 = (float)mat[2][3];
            matrix.m30 = (float)mat[3][0];
            matrix.m31 = (float)mat[3][1];
            matrix.m32 = (float)mat[3][2];
            matrix.m33 = (float)mat[3][3];
            return matrix;
        }

        public static NativeInterface.Matrix4 ToInterfaceObject(this Matrix4x4 mat)
        {
            NativeInterface.Matrix4 matrix = new NativeInterface.Matrix4();
            matrix.tab = new NativeInterface.Array4[4];
            matrix[0] = new NativeInterface.Array4();
            matrix[0][0] = mat.m00;
            matrix[0][1] = mat.m01;
            matrix[0][2] = mat.m02;
            matrix[0][3] = mat.m03;
            matrix[1] = new NativeInterface.Array4();
            matrix[1][0] = mat.m10;
            matrix[1][1] = mat.m11;
            matrix[1][2] = mat.m12;
            matrix[1][3] = mat.m13;
            matrix[2] = new NativeInterface.Array4();
            matrix[2][0] = mat.m20;
            matrix[2][1] = mat.m21;
            matrix[2][2] = mat.m22;
            matrix[2][3] = mat.m23;
            matrix[3] = new NativeInterface.Array4();
            matrix[3][0] = mat.m30;
            matrix[3][1] = mat.m31;
            matrix[3][2] = mat.m32;
            matrix[3][3] = mat.m33;
            return matrix;
        }
        #endregion

        #region Time
        public static DateTime ToUnityObject(this NativeInterface.Date date)
        {
            return new DateTime(date.year, date.month, date.day);
        }

        public static string ToEndDateRichText(this NativeInterface.Date date)
        {
            var validity = new DateTime(date.year, date.month, date.day);
            int daysRemaining = Math.Max(0, (validity - DateTime.Now).Days + 1);
            if (daysRemaining == 0)
                return date.ToUnityObject().ToString("yyyy-MM-dd") + "   <color='red'><i>Expired</i></color>";
            string remainingTextColor = daysRemaining > 30 ? "green" : daysRemaining > 15 ? "orange" : "red";
            return date.ToUnityObject().ToString("yyyy-MM-dd") + "   <color='" + remainingTextColor + "'><i>" + daysRemaining + " remaining day" + (daysRemaining > 1 ? "s" : "") + "</i></color>";
        }

        public static string GetRemainingDaysText(this NativeInterface.Date date)
        {
            var validity = new DateTime(date.year, date.month, date.day);
            int daysRemaining = Math.Max(0, (validity - DateTime.Now).Days + 1);
            if (daysRemaining == 0)
                return "<color='red'>Expired</color>";
            string remainingTextColor = daysRemaining > 30 ? "green" : daysRemaining > 15 ? "orange" : "red";
            return "<color='" + remainingTextColor + "'>" + daysRemaining + " Day" + (daysRemaining > 1 ? "s" : "") + "</color>";
        }

        #endregion

        #region Ids
        public static UInt32 ToUInt32(this Int32 i)
        {
            return BitConverter.ToUInt32(BitConverter.GetBytes(i), 0);
        }

        public static Int32 ToInt32(this UInt32 i)
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(i), 0);
        }
        #endregion
    }
}