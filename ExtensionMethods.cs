using System.Collections.Generic;
using UnityEngine;

namespace KamunagiOfChains
{
    public static class ExtensionMethods
    {
        static List<Material> _cachedMaterials = new List<Material>();
        static Shader hgStandard = RoR2.LegacyResourcesAPI.Load<Shader>("Shaders/Deferred/HGStandard");
        public static Material SetHopooMaterial(this Material tempMat)
        {
            if (_cachedMaterials.Contains(tempMat))
                return tempMat;

            float? bumpScale = null;
            Color? emissionColor = null;

            //grab values before the shader changes
            if (tempMat.IsKeywordEnabled("_NORMALMAP"))
            {
                bumpScale = tempMat.GetFloat("_BumpScale");
            }
            if (tempMat.IsKeywordEnabled("_EMISSION"))
            {
                emissionColor = tempMat.GetColor("_EmissionColor");
            }

            //set shader
            tempMat.shader = hgStandard;

            //apply values after shader is set
            tempMat.SetColor("_Color", tempMat.GetColor("_Color"));
            tempMat.SetTexture("_MainTex", tempMat.GetTexture("_MainTex"));
            tempMat.SetTexture("_EmTex", tempMat.GetTexture("_EmissionMap"));
            tempMat.EnableKeyword("DITHER");

            if (bumpScale != null)
            {
                tempMat.SetFloat("_NormalStrength", (float)bumpScale);
            }
            if (emissionColor != null)
            {
                tempMat.SetColor("_EmColor", (Color)emissionColor);
                tempMat.SetFloat("_EmPower", 1);
            }

            //set this keyword in unity if you want your model to show backfaces
            //in unity, right click the inspector tab and choose Debug
            if (tempMat.IsKeywordEnabled("NOCULL"))
            {
                tempMat.SetInt("_Cull", 0);
            }
            //set this keyword in unity if you've set up your model for limb removal item displays (eg. goat hoof) by setting your model's vertex colors
            if (tempMat.IsKeywordEnabled("LIMBREMOVAL"))
            {
                tempMat.SetInt("_LimbRemovalOn", 1);
            }

            _cachedMaterials.Add(tempMat);
            return tempMat;
        }
    }
}