using R2API;
using RoR2;
using UnityEngine;

namespace KamunagiOfChains.Data.GameObjects
{
    public class MothMoth : Asset, INetworkedObject
    {
        public GameObject BuildObject()
        {
            var mothMoth = LoadAsset<GameObject>("addressable:RoR2/Base/Beetle/BeetleWard.prefab")!.InstantiateClone("MothMoth");
            
            var impMat = new Material(LoadAsset<Material>("addressable:RoR2/Base/Imp/matImpBoss.mat"));
            impMat.SetFloat("_Cull", 0);
            impMat.SetColor("_Color", new Color(0.2588235f, 0.2705882f, 0.6352941f));
            impMat.SetColor("_EmColor", new Color(0.07058824f, 0.07058824f, 0.8823529f)); //you will probably need the shader stub for hgstandard here.
            impMat.SetTexture("_FresnelRamp", LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampLunarElectric.png"));
            impMat.SetTexture("_PrintRamp", LoadAsset<Texture2D>("addressable:RoR2/Base/Common/ColorRamps/texRampHuntressSoft.png"));
            mothMoth.GetComponentInChildren<SkinnedMeshRenderer>().material = impMat;
            
            //Main color, Emission color, Fresnel ramp, and Print Ramp are your 4 big ticket items here
            var mothLight = mothMoth.GetComponentInChildren<Light>();
            mothLight.color = new Color(0f, 0.391f, 0.9f);
            mothLight.range = 4f;
            
            var (mothWParticles, garbage) = mothMoth.GetComponentsInChildren<ParticleSystemRenderer>();
            Object.Destroy(garbage);
            var particlesMat = new Material(LoadAsset<Material>("addressable:RoR2/DLC1/PortalVoid/matPortalVoid.mat"));
            particlesMat.SetTexture("_RemapTex", LoadAsset<Texture2D>("addressable:RoR2/Base/Captain/texRampCrosshair2.png"));
            particlesMat.SetColor("_TintColor", new Color(0f, 0.6784314f,1f));
            mothWParticles.material = particlesMat;
            mothWParticles.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            mothWParticles.transform.localScale = Vector3.one * 0.3f;
            
            var outlineMaterial = new Material(LoadAsset<Material>("addressable:RoR2/Base/Nullifier/matNullifierZoneAreaIndicatorLookingIn.mat"));
            outlineMaterial.SetColor("_TintColor", new Color(0f, 0.274509804f, 1f));
            mothMoth.GetComponentInChildren<MeshRenderer>().material = outlineMaterial;

            mothMoth.AddComponent<DestroyOnTimer>().duration = 10;            
            return mothMoth;
        }
    }
}