using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using RoR2.Audio;
using RoR2.Orbs;
using ThreeEyedGames;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class Prefabs 
{
        internal static GameObject soeiMusouGhost;
        internal static GameObject soeiMusouProjectile;
        internal static GameObject voidTracer;
        internal static GameObject mothMoth;
        internal static GameObject boulderGhost;
        internal static GameObject boulderProjectile;
        internal static GameObject boulderChargeEffect;
        internal static GameObject gravityVoidballProjectile;
        internal static GameObject forceField;
        internal static GameObject chargeSunEffect;
        internal static GameObject sunStreamEffect;
        internal static GameObject naturesAxiom;
        internal static GameObject sunExplosion;
        internal static GameObject teleportEffect;
        internal static GameObject wispMaster;
        internal static GameObject gupMaster;
        internal static GameObject customWisp1Body;

        internal static GameObject nightshadePrisonDotZone;
        internal static GameObject hoverMuzzleFlames;
        internal static GameObject altMusouProjectile;
        internal static GameObject altMusouGhost;
        internal static GameObject tickingFuseObelisk;
        internal static GameObject antimatterVoidballEffect;
        internal static GameObject curseBurnFx;
        internal static GameObject onkamiSealPhase2;
        internal static GameObject yamatoWinds;
        internal static GameObject customFrostNova;
        internal static GameObject fireWalkerProjectile;
        internal static GameObject denebokshiriPillar;
        internal static GameObject frostMuzzleFlash;
        internal static GameObject frostChargeFx;
        internal static GameObject MikazuchiLightningStrike;
        internal static GameObject MikazuchiLightningStrikeSilent;
        internal static GameObject MikazuchiLightningOrb;
        internal static GameObject MikazuchiLightningOrbGhost;
        internal static GameObject MikazuchiLightningSeekerProjectile;
        internal static GameObject MikazuchiLightningSeekerGhost;
        internal static GameObject MikazuchiStakeNova;
        internal static GameObject reaverMusouProjectile;
        internal static GameObject reaverMuzzleFlash;
        internal static GameObject reaverMusouGhost;
        internal static GameObject reaverMusouGhostExplosion;
        internal static GameObject testProjectile;
        internal static GameObject tidalProjectile;
        internal static GameObject tidalProjectileGhost;
        internal static GameObject tidalEruptionProjectile;
        internal static GameObject tidalEruptionEffect;
        internal static GameObject tidalImpactEffect;
        internal static GameObject luckyTidalProjectile;
        internal static GameObject windBoomerang;
        internal static GameObject windBoomerangGhost;
        internal static GameObject windHitEffect;
        internal static GameObject miniSunProjectile;
        internal static GameObject fireHitEffect;
        internal static GameObject miniSunChargeEffect;
        internal static GameObject miniSunGhost;
        internal static GameObject altMusouMuzzle;
        internal static GameObject altMusouChargeballGhost;
        internal static GameObject altMusouChargeballProjectile;
        internal static GameObject windBoomerangChargeEffect;
        internal static GameObject explodingObelisk;
        internal static GameObject theObelisk;
        internal static GameObject onkamiObelisk;
        internal static GameObject primedObeliskGhost;
        internal static GameObject primedObelisk;
        internal static GameObject blankslateObelisk;
        internal static GameObject laserSigil;
        internal static GameObject voidTracerSphere;
        internal static GameObject lightningMuzzle;
        internal static GameObject curseParticles;
        internal static GameObject voidSphereAsset;
        internal static GameObject woshisWard;
        internal static GameObject boulderChild;
        internal static GameObject JachdwaltStrikeEffect;
        internal static GameObject passiveGlowingLight;
        internal static GameObject ascensionSparks;
        internal static GameObject soStupid;
        internal static GameObject VacuumProjectile;
        internal static GameObject VacuumGhost;
        internal static GameObject tmpCurseParticles;
        internal static GameObject mithrixPreBossBillboard;
        internal static GameObject ImpOverlordParticles;
        internal static GameObject kamunagiChains;
        internal static GameObject cherryPetals;
        internal static GameObject electricOrbPink;
        
        internal static Color wispNeonGreen = new Color(0.14f, 1f, 0f);
        internal static Color windSpellColor = new Color(0.175f, 0.63f, 0.086f);
        internal static Color oceanColor = new Color(0.13f, 0.79f, 0.85f);
        internal static Color twinsLightPurple = new Color(0.5411765f, 0.1176471f, 1f);
        internal static Color twinsDarkPurple = new Color(0.04742989f, 0.01059096f, 0.3207547f);
        internal static Color sealingColor = new Color(0.05098039f, 1f, 0.9058824f);
        internal static Color mashiroColor = new Color(0.98f, 1, 0.58f);
        //internal static DamageAPI.ModdedDamageType RekkaSohazen;
        internal static DamageAPI.ModdedDamageType Uitsalnemetia;
        internal static DamageAPI.ModdedDamageType Denebokshiri;
        internal static DamageColorIndex MashiroPrayer;
        internal static Material purpleFireOverlay;
        internal static Material woshisGhostOverlay;
        internal static Material redWispMat;
        internal static ItemDef customGhostItem;
        internal static ItemDef MashiroBlessing;
        internal static Texture2D purpleRamp;
        
    internal static void CreatePrefabs()
    {
        redWispMat = new Material(Load<Material>("RoR2/Base/Wisp/matWispFire.mat"));
        redWispMat.SetFloat("_BrightnessBoost", 2.63f);
        redWispMat.SetFloat("_AlphaBoost", 1.2f);
        redWispMat.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampWispSoul.png"));
        redWispMat.SetColor("_TintColor", Color.red);

        var wispBody = Load<GameObject>("RoR2/Base/Wisp/WispBody.prefab");
        Material wispMat1 = new Material(Load<Material>("RoR2/Base/Wisp/matWispFire.mat"));
        wispMat1.SetFloat("_BrightnessBoost", 2.63f);
        wispMat1.SetFloat("_AlphaBoost", 1.2f);
        wispMat1.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampWispSoul.png"));
        wispMat1.SetColor("_TintColor", wispNeonGreen);
        // hahahahahha
        customWisp1Body = PrefabAPI.InstantiateClone(wispBody, "CustomWisp1", true);
        customWisp1Body.GetComponent<CharacterBody>().baseNameToken = "NUGWISOMKAMI1_BODY_NAME";
        var wispPs = customWisp1Body.GetComponentsInChildren<ParticleSystemRenderer>();
        var firePs = wispPs[0]; //this is verified to be "Fire"
        firePs.material = wispMat1;
        var wtfModel = customWisp1Body.GetComponentInChildren<CharacterModel>();
        wtfModel.baseRendererInfos[1].defaultMaterial = wispMat1;
        wtfModel.baseLightInfos[0].defaultColor = wispNeonGreen;
        var stuffIdk = customWisp1Body.transform.GetChild(1).gameObject;
        var moreParticles = stuffIdk.GetComponentsInChildren<ParticleSystemRenderer>();
        moreParticles[0].material = wispMat1;
        stuffIdk.GetComponentInChildren<Light>().color = wispNeonGreen;
        ContentAddition.AddBody(customWisp1Body);

        wispMaster = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Wisp/WispMaster.prefab"), "TwinsWispMaster", true);
        wispMaster.AddComponent<TwinsDeployableBehaviour>().deployableType = TwinsDeployableBehaviour.DeployableType.Wisp;
        wispMaster.GetComponent<CharacterMaster>().bodyPrefab = customWisp1Body;
        ContentAddition.AddMaster(wispMaster);

        Load<GameObject>("RoR2/DLC1/Gup/GeepMaster.prefab").AddComponent<AIOwnership>();
        Load<GameObject>("RoR2/DLC1/Gup/GipMaster.prefab").AddComponent<AIOwnership>();
        gupMaster = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/Gup/GupMaster.prefab"), "TwinsGupMaster", true);
        gupMaster.GetComponent<CharacterMaster>().destroyOnBodyDeath = false;
        gupMaster.AddComponent<TwinsDeployableBehaviour>().deployableType = TwinsDeployableBehaviour.DeployableType.Gup;
        ContentAddition.AddMaster(gupMaster);

        purpleRamp = Modules.KamunagiAssets.assetBundle.LoadAsset<Texture2D>("purpleramp");
        #region NinesCustomPrefabs
        //TODO Nines prefabs
        //start rotation for particle fx
        //RekkaSohazen = DamageAPI.ReserveDamageType();
        //Uitsalnemetia = DamageAPI.ReserveDamageType();
        //Denebokshiri = DamageAPI.ReserveDamageType();

        purpleFireOverlay = new Material(Load<Material>("RoR2/Base/BurnNearby/matOnHelfire.mat"));
        purpleFireOverlay.SetTexture("_RemapTex", Prefabs.Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
        purpleFireOverlay.SetFloat("_FresnelPower", -15.8f);

        var voidfog = Prefabs.Load<GameObject>("RoR2/Base/Common/VoidFogMildEffect.prefab");

        fireHitEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Merc/MercExposeConsumeEffect.prefab"), "TwinsFireHitEffect", false);
        UnityEngine.Object.Destroy(fireHitEffect.GetComponent<OmniEffect>());
        foreach (ParticleSystemRenderer r in fireHitEffect.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            r.gameObject.SetActive(true);
            r.material.SetColor("_TintColor", new Color(0.7264151f, 0.1280128f, 0f));
            if (r.name == "PulseEffect, Ring (1)")
            {
                var mat = r.material;
                mat.mainTexture = Load<Texture2D>("RoR2/Base/Common/VFX/texArcaneCircleProviMask.png");
            }
        }

        Utils.RegisterEffect(fireHitEffect, 1, "Play_item_use_molotov_throw"); //"Play_fireballsOnHit_impact"); //new

        windHitEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Merc/MercExposeConsumeEffect.prefab"), "TwinsWindHitEffect", false);
        UnityEngine.Object.Destroy(windHitEffect.GetComponent<OmniEffect>());
        foreach (ParticleSystemRenderer r in windHitEffect.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            r.gameObject.SetActive(true);
            r.material.SetColor("_TintColor", Color.green);
            if (r.name == "PulseEffect, Ring (1)")
            {
                var mat = r.material;
                mat.mainTexture = Load<Texture2D>("RoR2/Base/Common/VFX/texArcaneCircleProviMask.png");
            }
        }
        Utils.RegisterEffect(windHitEffect, 1, "Play_huntress_R_snipe_shoot"); //new

        frostChargeFx = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/EliteIce/AffixWhiteExplosion.prefab"), "TwinsFrostChargeFx", false);
        foreach (ParticleSystemRenderer r in frostChargeFx.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            var name = r.name;

            if (name == "Nova Sphere")
            {
                r.enabled = false;
            }
        }
        UnityEngine.Object.Destroy(frostChargeFx.GetComponent<RoR2.EffectComponent>());
        //Utils.RegisterEffect(frostChargeFx, 2f, ""); //this one already has a sound

        frostMuzzleFlash = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamMuzzleflash.prefab"), "TwinsFrostMuzzleFlash", false);
        foreach (ParticleSystemRenderer r in frostMuzzleFlash.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            var name = r.name;

            if (name == "Ring")
            {
                r.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
            }

            if (name == "OmniSparks")
            {
                r.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
            }
        }
        frostMuzzleFlash.GetComponentInChildren<Light>().color = Color.cyan;
        ContentAddition.AddEffect(frostMuzzleFlash);

        customFrostNova = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/EliteIce/AffixWhiteExplosion.prefab"), "TwinsFrostNovaEffect", false);
        customFrostNova.transform.localScale = Vector3.one * 10f;
        Utils.RegisterEffect(customFrostNova, -1f, "Play_item_proc_iceRingSpear");

        yamatoWinds = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Mage/MuzzleflashMageLightningLargeWithTrail.prefab"), "TwinsFlyUpEffect", false);
        foreach (ParticleSystemRenderer r in yamatoWinds.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            var name = r.name;

            if (name == "Matrix, Billboard")
            {
                r.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
                //r.materials[1].SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampHealingMedkit.png"));
            }
            //todo remove the connected lines stuff
            if (name == "Matrix, Mesh")
            {
                r.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
            }
        }
        foreach (ParticleSystem p in yamatoWinds.GetComponentsInChildren<ParticleSystem>(true))
        {
            var name = p.name;
            var main = p.main;

            if (name == "Flash")
            {
                main.startColor = twinsLightPurple;
            }
        }
        yamatoWinds.GetComponentInChildren<Light>().color = twinsLightPurple;
        var trailRenderer = yamatoWinds.GetComponentInChildren<TrailRenderer>(); 
        trailRenderer.materials[0].SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
        Utils.RegisterEffect(yamatoWinds, 2f, ""); //ascension already has a sound

        curseBurnFx = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/GreaterWisp/GreaterWispDeath.prefab"), "TwinsCurseFx", false);
        curseBurnFx.transform.localScale = Vector3.one * 0.4f; //0.65
        foreach (ParticleSystemRenderer r in curseBurnFx.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            var name = r.name;

            if (name == "Ring")
            {
                r.material = new Material(r.material);
                r.material.SetColor("_TintColor", new Color(0.45f, 0, 1));
                r.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
            }

            if (name == "Chunks")
            {
                r.enabled = false;
            }

            if (name == "Mask")
            {
                r.enabled = false;
            }

            if (name == "Chunks, Sharp")
            {
                r.enabled = false;
            }

            if (name == "Flames")
            {
                r.enabled = false;
            }

            if (name == "Flash")
            {
                r.enabled = false;
            }

            if (name == "Distortion")
            {
                r.enabled = false;
            }

            if (name == "Chunks")
            {
                r.enabled = false;
            }
        }
        foreach (ParticleSystem r in curseBurnFx.GetComponentsInChildren<ParticleSystem>(false))
        {
            var name = r.name;
            var main = r.main;

            if (name == "Ring")
            {
                main.simulationSpeed = 3.5f;
            }
        }
        UnityEngine.Object.Destroy(curseBurnFx.GetComponent<ShakeEmitter>());
        curseBurnFx.GetComponentInChildren<Light>().color = Modules.Survivors.KamunagiSurvivor.kamunagiColor;
        Utils.RegisterEffect(curseBurnFx, -1,"");

        antimatterVoidballEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidMegaCrab/MegaCrabBlackCannonGhost.prefab"), "AntimatterMuzzleEffect", false);
        UnityEngine.Object.Destroy(antimatterVoidballEffect.GetComponent<ProjectileGhostController>());
        antimatterVoidballEffect.transform.localScale = Vector3.one * 0.04f;
        var scaler = antimatterVoidballEffect.transform.GetChild(0).gameObject;
        var blackSphere = scaler.transform.GetChild(1).gameObject;
        var crabMats = blackSphere.GetComponent<MeshRenderer>().materials;
        crabMats[0].SetTexture("_Emission", Load<Texture2D>("RoR2/Base/ElectricWorm/ElectricWormBody.png"));
        crabMats[1].SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLunarElectric.png"));
        scaler.GetComponentInChildren<Light>().range = 20f;
        var destroyThis = scaler.transform.GetChild(3).gameObject;
        UnityEngine.Object.Destroy(destroyThis);

        #region MithrixBossArenaFX
        altMusouMuzzle = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegacrabBlackSphere.prefab"), "ChargedMusouEffect", false);
        Utils.AddScaleComponent(altMusouMuzzle, 0.5f);
        var altPP = altMusouMuzzle.AddComponent<PostProcessVolume>();
        altPP.profile = Load<PostProcessProfile>("RoR2/Base/title/ppLocalBrotherImpact.asset");
        altPP.sharedProfile = altPP.profile;

        Material musouInstance = new Material(Load<Material>("RoR2/Base/Brother/matBrotherPreBossSphere.mat"));
        musouInstance.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampMoonPreBoss.png"));
        musouInstance.SetColor("_TintColor", twinsLightPurple);
        var coolSphere = altMusouMuzzle.GetComponent<MeshRenderer>();
        coolSphere.materials = new Material[1] { musouInstance };
        coolSphere.shadowCastingMode = ShadowCastingMode.On;

        altMusouMuzzle.transform.localScale = Vector3.one * 0.5f;
        var pointLight = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/bazaar/Bazaar_Light.prefab").transform.GetChild(1).gameObject, "Point Light", false);
        pointLight.transform.parent = altMusouMuzzle.transform;
        pointLight.transform.localPosition = Vector3.zero;
        pointLight.transform.localScale = Vector3.one * 0.5f;
        pointLight.GetComponent<Light>().range = 0.5f;
        var altSparks = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Blackhole/GravSphere.prefab").transform.GetChild(1).gameObject, "Sparks, Blue", false);
        var altP = altSparks.GetComponent<ParticleSystem>();
        var altPMain = altP.main;
        altPMain.simulationSpeed = 2f;
        altPMain.startColor = twinsLightPurple;
        altSparks.GetComponent<ParticleSystemRenderer>().material = Load<Material>("RoR2/Base/Common/VFX/matTracerBrightTransparent.mat");
        ascensionSparks = PrefabAPI.InstantiateClone(altSparks, "Ascension Sparks", false);
        altSparks.transform.parent = altMusouMuzzle.transform;
        altSparks.transform.localPosition = Vector3.zero;
        altSparks.transform.localScale = Vector3.one * 0.05f;

        var altCoreP = altMusouMuzzle.AddComponent<ParticleSystem>();
        var coreR = altMusouMuzzle.GetComponent<ParticleSystemRenderer>();
        Material decalMaterial = new Material(Load<Material>("RoR2/Base/Brother/matLunarShardImpactEffect.mat"));
        decalMaterial.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
        coreR.material = decalMaterial;
        coreR.renderMode = ParticleSystemRenderMode.Billboard;
        var coreM = altCoreP.main;
        coreM.duration = 1f;
        coreM.simulationSpeed = 1.1f;
        coreM.loop = true;
        coreM.startLifetime = 0.13f;
        coreM.startSpeed = 5f;
        coreM.startSize3D = false;
        coreM.startSizeY = 0.6f;
        coreM.startRotation3D = false;
        coreM.startRotationZ = 0.1745f;
        coreM.startSpeed = 0f;
        coreM.maxParticles = 30;
        var coreS = altCoreP.shape;
        coreS.enabled = false;
        coreS.shapeType = ParticleSystemShapeType.Circle;
        coreS.radius = 0.67f;
        coreS.arcMode = ParticleSystemShapeMultiModeValue.Random;
        var sparkleSize = altCoreP.sizeOverLifetime;
        sparkleSize.enabled = true;
        sparkleSize.separateAxes = true;
        //sparkleSize.sizeMultiplier = 0.75f;
        sparkleSize.xMultiplier = 1.3f;
        #region UnusedLightFlickerValues
        /*var altLight = pointLight.GetComponent<FlickerLight>();
            var flicker0 = altLight.sinWaves[0];
            flicker0.period = 0.08333334f;
            flicker0.amplitude = 0.2f;
            flicker0.frequency = 12f;
            flicker0.cycleOffset = 61.35653f; 
            var flicker1 = altLight.sinWaves[1];
            flicker1.period = 0.1666667f;
            flicker1.amplitude = 0.1f;
            flicker1.frequency = 6f;
            flicker1.cycleOffset = 96.17653f;
            var flicker2 = altLight.sinWaves[2];
            flicker2.period = 0.1111111f;
            flicker2.amplitude = 0.1f;
            flicker2.frequency = 9f;
            flicker2.cycleOffset = 51.90653f;*/
        #endregion
        #endregion MithrixBossArenaFx

        altMusouChargeballGhost = PrefabAPI.InstantiateClone(altMusouMuzzle, "TwinsAltChargeBallGhost", false);
        UnityEngine.Object.Destroy(altMusouChargeballGhost.GetComponent<ObjectScaleCurve>());
        altMusouChargeballGhost.AddComponent<ProjectileGhostController>();

        altMusouChargeballProjectile = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterBigProjectile.prefab"), "TwinsAltChargeBallProjectile", true);
        altMusouChargeballProjectile.GetComponent<ProjectileController>().ghostPrefab = altMusouChargeballGhost;
        ContentAddition.AddProjectile(altMusouChargeballProjectile);

        miniSunChargeEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Grandparent/ChannelGrandParentSunHands.prefab"), "TwinsChargeMiniSun", false);
        Utils.AddScaleComponent(miniSunChargeEffect, 2f);
        var theball = miniSunChargeEffect.transform.GetChild(1);
        theball.gameObject.SetActive(true);

        reaverMuzzleFlash = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Nullifier/NullifierExplosion.prefab"), "ReaverMusouMuzzleFlash", false);
        UnityEngine.Object.Destroy(reaverMuzzleFlash.GetComponent<ShakeEmitter>());
        //UnityEngine.Object.Destroy(reaverMuzzleFlash.GetComponent<Rigidbody>());
        reaverMuzzleFlash.transform.GetChild(1).gameObject.SetActive(false);
        reaverMuzzleFlash.transform.GetChild(4).gameObject.SetActive(false);
        reaverMuzzleFlash.transform.GetChild(6).gameObject.SetActive(false);
        reaverMuzzleFlash.transform.localScale = Vector3.one * 0.4f;
        var dist = reaverMuzzleFlash.transform.GetChild(3).gameObject;
        var distP = dist.GetComponentInChildren<ParticleSystem>().shape;
        distP.scale = Vector3.one * 0.5f;
        Utils.RegisterEffect(reaverMuzzleFlash, -1, "");

        teleportEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterExplosion.prefab"), "TwinsTeleportEffect", false);
        teleportEffect.transform.localScale = new Vector3(5, 5, 5);
        Utils.RegisterEffect(teleportEffect, -1, "Play_voidman_m2_explode");

        hoverMuzzleFlames = Modules.KamunagiAssets.assetBundle.LoadAsset<GameObject>("ShadowFlame.prefab");
        hoverMuzzleFlames.transform.localPosition = Vector3.zero;
        hoverMuzzleFlames.transform.localScale = Vector3.one * 0.6f;
        //Utils.RegisterEffect(hoverMuzzleFlames, -1, "");

        electricOrbPink = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/ElectricWorm/ElectricOrbGhost.prefab"), "TwinsPinkHandEnergy", false);
        UnityEngine.Object.Destroy(electricOrbPink.GetComponent<ProjectileGhostController>());
        UnityEngine.Object.Destroy(electricOrbPink.GetComponent<VFXAttributes>());
        var pinkChild = electricOrbPink.transform.GetChild(0);
        pinkChild.transform.localScale = Vector3.one * 0.1f;
        var pinkTransform = pinkChild.transform.GetChild(0);
        pinkTransform.transform.localScale = Vector3.one * 0.25f;
        var pink = new Color(1f, 0f, 0.34f);
        var pinkAdditive = new Color(0.91f, 0.3f, 0.84f);
        foreach (ParticleSystemRenderer r in electricOrbPink.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            var name = r.name;

            if (name == "SpitCore")
            {
                r.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/DLC1/Common/ColorRamps/texRampVoidRing.png"));
                r.material.SetFloat("_AlphaBoost", 3.2f);
                r.material.SetColor("_TintColor", pinkAdditive);
            }
        }
        var pinkTrails = electricOrbPink.GetComponentsInChildren<TrailRenderer>();
        pinkTrails[0].material.SetColor("_TintColor", pink);
        pinkTrails[1].material.SetColor("_TintColor", pink);
        electricOrbPink.GetComponentInChildren<Light>().color = twinsDarkPurple;

        denebokshiriPillar = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Junk/Mage/MageFirewallPillarProjectile.prefab"), "TwinsFirePillarProjectile", true);
        denebokshiriPillar.GetComponent<ProjectileDamage>().damageType = DamageType.IgniteOnHit;
        //denebokshiriPillar.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(Denebokshiri);
        var overlap = denebokshiriPillar.GetComponent<ProjectileOverlapAttack>();
        overlap.impactEffect = fireHitEffect;
        overlap.fireFrequency = 20f;
        overlap.resetInterval = 1.2f;
        ContentAddition.AddProjectile(denebokshiriPillar);

        fireWalkerProjectile = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Mage/MageIcewallWalkerProjectile.prefab"), "TwinsFlameWallWalkerProjectile", true);
        var walkerComponent = fireWalkerProjectile.GetComponent<ProjectileMageFirewallWalkerController>();
        walkerComponent.firePillarPrefab = denebokshiriPillar;
        walkerComponent.dropInterval = 0.1f;
        ContentAddition.AddProjectile(fireWalkerProjectile);

        #region Onkami Seal, it's kind of a mess
        Material sealingProjectileMat = new Material(Load<Material>("RoR2/Base/Common/matVoidDeathBombAreaIndicatorFront.mat"));
        Material onkamiMat1 = new Material(Load<Material>("RoR2/Base/artifactworld/matArtifactPortalCenter.mat"));
        onkamiMat1.SetFloat("_AlphaBoost", 1.3f);
        onkamiMat1.SetColor("_TintColor", new Color(0f, 0.1843f, 1f));
        Material onkamiMat2 = new Material(Load<Material>("RoR2/Base/artifactworld/matArtifactPortalEdge.mat"));
        onkamiMat2.SetColor("_TintColor", new Color(0f, 0.1843f, 1f));
        onkamiMat2.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampMoonArenaWall.png"));
        onkamiMat2.SetFloat("_BrightnessBoost", 4.67f);
        onkamiMat2.SetFloat("_AlphaBoost", 1.2f);
        //onkamiMat2.SetFloat();

        explodingObelisk = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathExplosion.prefab"), "OnkamiSealPhase3BlastEffect", false);
        foreach (ParticleSystemRenderer r in explodingObelisk.GetComponentsInChildren<ParticleSystemRenderer>())
        {
            var name = r.name;

            if (name == "AreaIndicator")
            {
                r.material.SetTexture("_Cloud1Tex", Load<Texture2D>("RoR2/DLC1/MajorAndMinorConstruct/texMajorConstructShield.png"));
                r.material.SetTexture("_Cloud2Tex", Load<Texture2D>("RoR2/DLC1/ancientloft/texAncientLoft_TempleDecal.tga"));
                r.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampMoonArenaWall.png"));
                r.material.SetColor("_TintColor", sealingColor); //new Color(0f, 0.9137255f, 1f));

                r.material.SetFloat("_IntersectionStrength", 0.08f);
                r.material.SetFloat("_AlphaBoost", 20f);
                r.material.SetFloat("_RimStrength", 1.050622f);
                r.material.SetFloat("_RimPower", 1.415718f);
            }

            if (name == "Vacuum Radial")
            {
                r.material.SetTexture("_MainTex", Load<Texture2D>("RoR2/DLC1/ancientloft/texAncientLoft_TempleDecal.tga"));
                r.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampBrotherPillar.png"));
                r.material.SetFloat("_AlphaBoost", 6.483454f);
                r.material.SetColor("_TintColor",new Color(0f, 0.1843f, 1f));
            }

            if (name == "Vacuum Stars, Trails")
            {
                r.enabled = false;
            }
            if (name == "Goo, Medium")
            {
                r.enabled = false;
            }
            if (name == "AreaIndicator (1)")
            {
                r.enabled = false;
            }

            if (name == "Vacuum Stars")
            {
                r.enabled = false;
            }
        }
        explodingObelisk.transform.localScale = Vector3.one * 11.25f;
        explodingObelisk.GetComponentInChildren<Light>().color = sealingColor;
        //explodingObelisk.transform.position = new Vector3(explodingObelisk.transform.position.x, 4f, explodingObelisk.transform.position.z);
        var sealMeshR = explodingObelisk.GetComponentInChildren<MeshRenderer>();
        sealMeshR.materials = new [] { onkamiMat1, onkamiMat2 };
        var sealingMeshObject = explodingObelisk.transform.GetChild(7).gameObject;
        //obtaining the perfect obelisk mesh
        var loftPrefab = Load<GameObject>("RoR2/DLC1/ancientloft/AL_LightStatue_On.prefab");
        var obelisk = loftPrefab.transform.GetChild(4).gameObject;
        theObelisk = PrefabAPI.InstantiateClone(obelisk, "TwinsObelisk", false);
        theObelisk.transform.position = Vector3.zero;
        var obeliskChildRotation = theObelisk.transform.rotation;
        var sealingObelisk = theObelisk.GetComponent<MeshFilter>().mesh;
        //
        sealingMeshObject.GetComponent<MeshFilter>().mesh = sealingObelisk;
        sealingMeshObject.transform.rotation = obeliskChildRotation; //I should get an award for this
        sealingMeshObject.transform.position = new Vector3(sealingMeshObject.transform.position.x, -8f, sealingMeshObject.transform.position.z); //todo obelisk third (3) position
        //the detonation and priming obelisk use the same Vector3
        sealingMeshObject.GetComponent<MeshRenderer>().materials = new[] { onkamiMat1, onkamiMat2 };
        sealingMeshObject.GetComponent<ObjectScaleCurve>().baseScale = Vector3.one * 0.7f;
        onkamiObelisk = PrefabAPI.InstantiateClone(sealingMeshObject, "OnkamiObelisk", false);
        UnityEngine.Object.Destroy(onkamiObelisk.GetComponent<ObjectScaleCurve>());
        onkamiObelisk.transform.localScale = Vector3.one;
        var blastIndicator = explodingObelisk.transform.GetChild(10).gameObject;
        blastIndicator.transform.localScale = Vector3.one * 1.4f; //blast indicator
        blastIndicator.transform.position = new Vector3(blastIndicator.transform.position.x, 0.5f, blastIndicator.transform.position.z);
        Utils.RegisterEffect(explodingObelisk, -1f, "Play_item_void_bleedOnHit_explo"); 

        onkamiSealPhase2 = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombGhost.prefab"), "OnkamiSealPhase2Ghost", false);
        onkamiSealPhase2.transform.localScale = Vector3.one * 2f;
        var sealingScale = onkamiSealPhase2.transform.GetChild(0).gameObject;
        sealingScale.transform.localScale = Vector3.one * 5.625f;
        foreach (ParticleSystemRenderer r in onkamiSealPhase2.GetComponentsInChildren<ParticleSystemRenderer>())
        {
            var name = r.name;

            if (name == "AreaIndicator, Front")
            {
                sealingProjectileMat.SetTexture("_Cloud1Tex", Load<Texture2D>("RoR2/DLC1/MajorAndMinorConstruct/texMajorConstructShield.png"));
                sealingProjectileMat.SetTexture("_Cloud2Tex", Load<Texture2D>("RoR2/Base/Common/VFX/texArcaneCircleWispMask.png"));
                sealingProjectileMat.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampMoonPreBoss.png"));
                sealingProjectileMat.SetTexture("_MainTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texLunarWispTracer 1.png"));
                sealingProjectileMat.SetFloat("_SrcBlendFloat", 5f);
                sealingProjectileMat.SetFloat("_DstBlendFloat", 1f);

                sealingProjectileMat.SetFloat("_IntersectionStrength", 0.4f);
                sealingProjectileMat.SetFloat("_AlphaBoost", 9.041705f);
                sealingProjectileMat.SetFloat("_RimStrength", 9.041705f);
                sealingProjectileMat.SetFloat("_RimPower", 0.1f);
                sealingProjectileMat.SetColor("_TintColor", sealingColor);
                r.material = sealingProjectileMat;
            }

            if (name == "AreaIndicator, Back")
            {
                r.material.SetColor("_TintColor", new Color(0f, 0.01960784f, 1f));
            }

            if (name == "Vacuum Stars")
            {
                r.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampMoonArenaWall.png"));
            }
        }
        foreach (ParticleSystem p in onkamiSealPhase2.GetComponentsInChildren<ParticleSystem>())
        {
            var name = p.name;
            var main = p.main;
            var sizeLife = p.sizeOverLifetime;

            if (name == "AreaIndicator, Front")
            {
                sizeLife.enabled = false;
            }

            if (name == "Vacuum Radial")
            {
                sizeLife.sizeMultiplier = 1.6f;
            }

            if (name == "AreaIndicator, Back")
            {
                sizeLife.enabled = false;
            }

            if (name == "Vacuum Stars, Trails")
            {
                main.startColor = sealingColor;
            }
        } 
        var scaleChild = onkamiSealPhase2.transform.GetChild(0);
        scaleChild.transform.position = new Vector3(0f, 4f, 0f);
        onkamiObelisk.transform.SetParent(scaleChild);
        onkamiObelisk.transform.localScale = Vector3.one * 0.6f;
        onkamiObelisk.transform.localPosition = onkamiSealPhase2.transform.position;
        onkamiObelisk.transform.position = new Vector3(0f, -10f, 0f);                                                                     //todo obelisk second (2) position
        var frontIndicator = scaleChild.GetChild(8).gameObject;
        var backIndicator = scaleChild.GetChild(9).gameObject;
        frontIndicator.transform.localScale = Vector3.one * 1.3f; // sealing frontIndicator
        backIndicator.transform.localScale = Vector3.one * 1.3f; // sealing backIndicator
        var onkamiMeshR = onkamiObelisk.GetComponent<MeshRenderer>();
        onkamiMeshR.materials = new [] { onkamiMat1, onkamiMat2 }; //this is how you make a completely new array

        tickingFuseObelisk = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombProjectile.prefab"), "OnkamiSealPhase2", true);
        tickingFuseObelisk.GetComponent<ProjectileController>().ghostPrefab = onkamiSealPhase2;
        var sealingImpact = tickingFuseObelisk.GetComponent<ProjectileImpactExplosion>();
        sealingImpact.lifetime = 1.5f;
        sealingImpact.blastRadius = 15f;
        sealingImpact.fireChildren = false;
        sealingImpact.impactEffect = explodingObelisk;
        sealingImpact.blastDamageCoefficient = 6f; //todo you dont set the damage here
        tickingFuseObelisk.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;
        //tickingFuseObelisk.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(Uitsalnemetia);
        ContentAddition.AddProjectile(tickingFuseObelisk);

        primedObeliskGhost = PrefabAPI.InstantiateClone(explodingObelisk, "OnkamiSealPhase1Ghost", false);
        UnityEngine.Object.Destroy(primedObeliskGhost.GetComponent<EffectComponent>());
        UnityEngine.Object.Destroy(primedObeliskGhost.GetComponent<VFXAttributes>());
        primedObeliskGhost.transform.GetChild(0).gameObject.SetActive(false);
        primedObeliskGhost.transform.GetChild(2).gameObject.SetActive(false);
        primedObeliskGhost.transform.GetChild(4).gameObject.SetActive(false);
        primedObeliskGhost.transform.GetChild(8).gameObject.SetActive(false);
        primedObeliskGhost.transform.GetChild(9).gameObject.SetActive(false);
        primedObeliskGhost.transform.GetChild(10).gameObject.SetActive(false);
        primedObeliskGhost.transform.GetChild(11).gameObject.SetActive(false);
        primedObeliskGhost.transform.localScale = Vector3.one * 13.6f;
        var onkamiMesh = primedObeliskGhost.transform.GetChild(7).gameObject;
        UnityEngine.Object.Destroy(onkamiMesh.GetComponent<ObjectScaleCurve>());
        onkamiMesh.GetComponent<MeshRenderer>().materials = new[] { onkamiMat1, onkamiMat2 };
        primedObeliskGhost.AddComponent<ProjectileGhostController>();

        primedObelisk = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Nullifier/NullifierPreBombProjectile.prefab"), "OnkamiSealPhase1", true);
        primedObelisk.GetComponent<ProjectileController>().ghostPrefab = primedObeliskGhost;
        UnityEngine.Object.Destroy(primedObelisk.transform.GetChild(0).gameObject);
        primedObelisk.transform.position = new Vector3(sealingMeshObject.transform.position.x, -8f, sealingMeshObject.transform.position.z); //todo first (1) position
        primedObelisk.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        var onkamiImpact = primedObelisk.GetComponent<ProjectileImpactExplosion>();
        onkamiImpact.blastRadius = 1f;
        onkamiImpact.fireChildren = true;
        onkamiImpact.blastDamageCoefficient = 0f;
        onkamiImpact.childrenProjectilePrefab = tickingFuseObelisk;
        onkamiImpact.impactEffect = null;
        onkamiImpact.lifetimeExpiredSound = null;
        ContentAddition.AddProjectile(primedObelisk);
        #endregion OnkamiSeal

        altMusouGhost = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterSmallGhost.prefab"), "TwinsTrackingGhost", false);
        altMusouGhost.GetComponentInChildren<Light>().color = twinsLightPurple;

        altMusouProjectile = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabMissileProjectile.prefab"), "TwinsTrackingProjectile", true);
        altMusouProjectile.GetComponent<ProjectileController>().ghostPrefab = altMusouGhost;
        //altMusouProjectile.GetComponent<ProjectileDirectionalTargetFinder>().lookRange = 15f;
        altMusouProjectile.GetComponent<ProjectileDamage>().damage = 1f;
        ContentAddition.AddProjectile(altMusouProjectile);

        /*nightshadePrisonDotZone = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabMultiBeamDotZone.prefab"), "TwinsPrisonDotZone", true); //"RoR2/Base/LunarExploder/LunarExploderProjectileDotZone.prefab"
            nightshadePrisonDotZone.GetComponentInChildren<Light>().color = Color.red;
            nightshadePrisonDotZone.transform.localScale = Vector3.one * 0.7f;
            nightshadePrisonDotZone.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(RekkaSohazen);
            var dotZone = nightshadePrisonDotZone.GetComponent<ProjectileDotZone>();
            dotZone.damageCoefficient = 0.5f;
            dotZone.resetFrequency = 10f;
            dotZone.lifetime = 5f;
            ContentAddition.AddProjectile(nightshadePrisonDotZone);*/

        MikazuchiLightningStrike = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Lightning/LightningStrikeImpact.prefab"), "MikazuchiLightningStrikeImpact", false);
        MikazuchiLightningStrike.GetComponentInChildren<Light>().color = Color.yellow;
        var mPP = MikazuchiLightningStrike.GetComponentInChildren<PostProcessVolume>();
        mPP.profile = Load<PostProcessProfile>("RoR2/Base/title/ppLocalGrandparent.asset");
        mPP.sharedProfile = mPP.profile;
        foreach (ParticleSystemRenderer r in MikazuchiLightningStrike.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            var name = r.name;

            if (name == "Ring")
            {
                r.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampParentTeleportIndicator.png"));
                r.material.SetColor("_TintColor", Color.yellow);
            }
            if (name == "LightningRibbon")
            {
                r.trailMaterial = new Material(r.trailMaterial);
                r.trailMaterial.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampParentTeleportIndicator.png"));
            }
            if (name == "Sphere")
            {
                r.material = Load<Material>("RoR2/Base/Loader/matLightningLongYellow.mat");
            }

            if (name == "Flash")
            {
                r.material.DisableKeyword("VERTEXCOLOR");
            }
        }
        Utils.RegisterEffect(MikazuchiLightningStrike, -1f, "Play_item_use_lighningArm");

        MikazuchiLightningStrikeSilent = PrefabAPI.InstantiateClone(MikazuchiLightningStrike, "MikazuchiSilentImpact", false);
        Utils.RegisterEffect(MikazuchiLightningStrikeSilent, -1f, "");

        MikazuchiLightningOrbGhost = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/ElectricWorm/ElectricOrbGhost.prefab"), "MikazuchiLightningOrbGhost", false);
        foreach (ParticleSystemRenderer r in MikazuchiLightningOrbGhost.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            var name = r.name;

            if (name == "SpitCore")
            {
                r.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampWarbanner2.png"));
                r.material.SetColor("_TintColor", Color.yellow);
            }
        }
        var trailR = MikazuchiLightningOrbGhost.GetComponentsInChildren<TrailRenderer>();
        trailR[0].material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampParentTeleportIndicator.png"));
        trailR[1].material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampParentTeleportIndicator.png"));
        MikazuchiLightningOrbGhost.GetComponentInChildren<Light>().color = Color.yellow;

        MikazuchiLightningSeekerGhost = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/ElectricWorm/ElectricWormSeekerGhost.prefab"), "Mikazuch iLightningSeekerGhost", false);
        MikazuchiLightningSeekerGhost.GetComponentInChildren<TrailRenderer>().startColor = Color.yellow;

        MikazuchiStakeNova = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/EliteLightning/LightningStakeNova.prefab"), "MikazuchiStakeNova", false);
        MikazuchiStakeNova.transform.localScale = Vector3.one * 2;
        var novaPr = MikazuchiStakeNova.GetComponentsInChildren<ParticleSystemRenderer>();
        novaPr[1].material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampParentTeleportIndicator.png"));
        //novaPr[3].material.DisableKeyword("VERTEXCOLOR");
        foreach (ParticleSystem p in MikazuchiStakeNova.GetComponentsInChildren<ParticleSystem>())
        {
            var name = p.name;
            var main = p.main;

            if (name == "AreaIndicatorRing, Billboard")
            {
                main.startColor = Color.yellow;
            }

            if (name == "UnscaledHitsparks 1")
            {
                main.startColor = Color.yellow;
            }

            if (name == "Flash")
            {
                main.startColor = Color.yellow;
            }
        }
        MikazuchiStakeNova.GetComponentInChildren<Light>().color = Color.yellow;
        Utils.RegisterEffect(MikazuchiStakeNova, -1f, "Play_mage_m1_impact");

        MikazuchiLightningSeekerProjectile = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/ElectricWorm/ElectricWormSeekerProjectile.prefab"), "MikazuchiLightningSeekerProjectile", true);
        MikazuchiLightningSeekerProjectile.GetComponent<ProjectileController>().ghostPrefab = MikazuchiLightningSeekerGhost;
        MikazuchiLightningSeekerProjectile.GetComponent<ProjectileImpactExplosion>().impactEffect = MikazuchiStakeNova;
        ContentAddition.AddProjectile(MikazuchiLightningSeekerProjectile);

        MikazuchiLightningOrb = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/ElectricWorm/ElectricOrbProjectile.prefab"), "MikazuchiLightningOrbProjectile", true);
        MikazuchiLightningOrb.GetComponent<ProjectileController>().ghostPrefab = MikazuchiLightningOrbGhost;
        MikazuchiLightningOrb.GetComponent<ProjectileDamage>().damageType = DamageType.Shock5s;
        var lightningImpact = MikazuchiLightningOrb.GetComponent<ProjectileImpactExplosion>(); 
        lightningImpact.impactEffect = MikazuchiLightningStrikeSilent;
        lightningImpact.childrenProjectilePrefab = MikazuchiLightningSeekerProjectile;
        lightningImpact.childrenDamageCoefficient = 1.7f;
        var lightpact = MikazuchiLightningOrb.GetComponent<ProjectileImpactExplosion>(); 
        lightpact.falloffModel = BlastAttack.FalloffModel.None;
        lightpact.blastDamageCoefficient = 5f;
        ContentAddition.AddProjectile(MikazuchiLightningOrb);

        reaverMusouGhost = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombletsGhost.prefab"), "ReaverMusouGhost", false);
        reaverMusouGhost.transform.localScale = Vector3.one;
        var reaveMesh = reaverMusouGhost.GetComponentInChildren<MeshRenderer>();
        reaveMesh.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampBanditSplatter.png"));
        reaveMesh.material.SetColor("_TintColor", twinsLightPurple); //this has a great FlickerLight component to copy from
        var flickerPurple = reaverMusouGhost.transform.GetChild(1).gameObject;
        var lightC = flickerPurple.GetComponent<Light>();
        lightC.color = new Color(0.4333f, 0.0726f, 0.8925f);
        //lightC.intensity = 30f;
        flickerPurple.transform.localPosition = new Vector3(0f, 0.7f, 0f);

        kamunagiChains = Modules.KamunagiAssets.assetBundle.LoadAsset<GameObject>("KamunagiChains");
        kamunagiChains.transform.localPosition = new Vector3(0.02f, -0.4f, 0f);
        kamunagiChains.transform.localScale = new Vector3(0.2f, 0.4f, 0.2f);
        var hmm = kamunagiChains.GetComponent<ParticleUVScroll>();
        if (!hmm)
        {
            kamunagiChains.AddComponent<ParticleUVScroll>();
        }

        passiveGlowingLight = PrefabAPI.InstantiateClone(reaverMusouGhost.transform.GetChild(1).gameObject, "TwinsPurpleFlicker", false);
        var cooldownLight = passiveGlowingLight.GetComponent<Light>();
        cooldownLight.intensity = 6f;
        cooldownLight.range = 2f;
        var waves = passiveGlowingLight.GetComponent<RoR2.FlickerLight>().sinWaves;
        waves[0].frequency = 0.7f;
        waves[1].frequency = 0.7f;

        var impBoss = Load<GameObject>("RoR2/Base/ImpBoss/ImpBossBody.prefab");
        var impModelBase = impBoss.transform.GetChild(0).gameObject;
        var impmdl = impModelBase.transform.GetChild(0).gameObject;
        var impDustCenter = impmdl.transform.GetChild(0).gameObject;
        ImpOverlordParticles = PrefabAPI.InstantiateClone(impDustCenter, "VeilParticles", false);
        UnityEngine.Object.Destroy(ImpOverlordParticles.transform.GetChild(0).gameObject);
        var veilLight = ImpOverlordParticles.GetComponentInChildren<Light>();
        veilLight.range = 4f;
        veilLight.color = twinsLightPurple;
        foreach (ParticleSystem p in ImpOverlordParticles.GetComponentsInChildren<ParticleSystem>())
        {
            var name = p.name;
            var main = p.main;

            if (name == "LocalRing")
            {
                //Debug.LogError("this code was ran haha");
                //main.startSpeed = new ParticleSystem.MinMaxCurve(8f);
                main.startColor = twinsLightPurple;
            }
        }
        foreach (ParticleSystemRenderer r in ImpOverlordParticles.GetComponentsInChildren<ParticleSystemRenderer>())
        {
            var name = r.name; ;

            if (name == "LocalRing")
            {
                //Debug.LogError("this code was ran haha");
                //main.startSpeed = new ParticleSystem.MinMaxCurve(8f);
                r.material = new Material(r.material);
                r.material.SetTexture("_RemapTex", purpleRamp);
                r.material.SetFloat("_AlphaBias", 0.1f);
                r.material.SetColor("_TintColor", new Color(0.42f, 0f, 1f));
            }
        }

        reaverMusouGhostExplosion = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Nullifier/NullifierExplosion.prefab"), "ReaverMusouExplosion", false);
        reaverMusouGhostExplosion.transform.localScale = Vector3.one * 4f;
        var exSphere = reaverMusouGhostExplosion.transform.GetChild(6).gameObject;
        exSphere.transform.localScale = Vector3.one * 2f;
        var starrySphere = reaverMusouGhostExplosion.transform.GetChild(5).gameObject;
        var starryMats = starrySphere.GetComponent<MeshRenderer>().materials;
        starryMats[1].SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampNullifierOffset.png"));
        Utils.RegisterEffect(reaverMusouGhostExplosion, -1f);

        reaverMusouProjectile = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Nullifier/NullifierPreBombProjectile.prefab"), "ReaverMusouProjectile", true);
        var impact = reaverMusouProjectile.GetComponent<ProjectileImpactExplosion>();
        impact.lifetime = 0.5f;
        impact.impactEffect = reaverMusouGhostExplosion;
        //impact.blastRadius = 5f;
        reaverMusouProjectile.GetComponent<ProjectileController>().ghostPrefab = reaverMusouGhost;
        reaverMusouProjectile.transform.GetChild(0).gameObject.SetActive(false);
        ContentAddition.AddProjectile(reaverMusouProjectile);

        windBoomerangGhost = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/LunarSkillReplacements/LunarSecondaryGhost.prefab"), "TwinsWindBoomerangGhost", false);
        //windBoomerangGhost.transform.localScale = Vector3.one * 5f;
        var windPsr = windBoomerangGhost.GetComponentsInChildren<ParticleSystemRenderer>();
        windPsr[0].material.SetColor("_TintColor", windSpellColor);
        windPsr[2].enabled = false;
        windPsr[3].enabled = false;
        windPsr[3].material.SetColor("_TintColor", windSpellColor);
        windPsr[3].material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAntler.png"));
        windPsr[4].enabled = false;
        windPsr[5].enabled = false;
        var boomerangTrail = windBoomerangGhost.GetComponentInChildren<TrailRenderer>();
        boomerangTrail.material = new Material(boomerangTrail.material);
        boomerangTrail.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAntler.png"));
        boomerangTrail.material.SetColor("_TintColor", windSpellColor);
        var windLight = windBoomerangGhost.GetComponentInChildren<Light>();
        windLight.color = windSpellColor;
        windLight.intensity = 20f;
        var windMR = windBoomerangGhost.GetComponentsInChildren<MeshRenderer>();
        windMR[0].material.SetColor("_TintColor", windSpellColor);
        windMR[1].material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAntler.png"));

        windBoomerangChargeEffect = PrefabAPI.InstantiateClone(windBoomerangGhost, "WindChargeEffect", false);
        UnityEngine.Object.Destroy(windBoomerangChargeEffect.GetComponent<ProjectileGhostController>());

        windBoomerang = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Saw/Sawmerang.prefab"), "TwinsWindBoomerang", true);
        UnityEngine.Object.Destroy(windBoomerang.GetComponent<BoomerangProjectile>());
        UnityEngine.Object.Destroy(windBoomerang.GetComponent<ProjectileOverlapAttack>());
        var windDamage = windBoomerang.GetComponent<ProjectileDotZone>();
        windDamage.damageCoefficient = 0.5f;
        windDamage.overlapProcCoefficient = 0.2f;
        windDamage.fireFrequency = 25f;
        windDamage.resetFrequency = 10f;
        windDamage.impactEffect = windHitEffect;
        var itjustworks = windBoomerang.AddComponent<WindBoomerangProjectile>();
        //haha hopefully
        var windSounds = windBoomerang.GetComponent<ProjectileController>();
        windSounds.startSound = "Play_merc_m2_uppercut";
        windSounds.flightSoundLoop = Load<LoopSoundDef>("RoR2/Base/LunarSkillReplacements/lsdLunarSecondaryProjectileFlight.asset");
        windBoomerang.GetComponent<ProjectileController>().ghostPrefab = windBoomerangGhost;
        ContentAddition.AddProjectile(windBoomerang);

        tidalEruptionEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/ClayGrenadier/ClayGrenadierMortarExplosion.prefab"), "TwinsGeyserEruptionEffect", false);
        var eruptionDecal = tidalEruptionEffect.GetComponentInChildren<Decal>();
        eruptionDecal.Material = new Material(eruptionDecal.Material);
        tidalEruptionEffect.GetComponentInChildren<Light>().color = oceanColor;
        eruptionDecal.Material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
        foreach (ParticleSystemRenderer r in tidalEruptionEffect.GetComponentsInChildren<ParticleSystemRenderer>())
        {
            Material sharedMat1 = new Material(Load<Material>("RoR2/DLC1/ClayGrenadier/matClayGrenadierShockwave.mat"));
            sharedMat1.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampBombOrb.png"));
            //sharedMat1.SetEmission(0.5f, Color.white);
            var name = r.name;

            if (name == "Billboard, Directional")
            {
                //r.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampBombOrb.png")); //no texRamp on this one according to Addressables
            }

            if (name == "Billboard, Big Splash")
            {
                //r.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
                r.enabled = false;
            }

            if (name == "Billboard, Splash")
            {
                r.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
                r.material.SetFloat("_AlphaBoost", 2.45f);
                r.material.SetFloat("_NormalStrength", 5f);
                r.material.SetFloat("_Cutoff", 0.45f);
            }

            if (name == "Billboard, Dots")
            {
                r.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
            }

            if (name == "Sparks, Collision")
            {
                r.enabled = false;
            }

            if (name == "Lightning, Spark Center")
            {
                r.enabled = false;
            }

            if (name == "Ring")
            {
                r.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
            }

            if (name == "Debris")
            {
                r.enabled = false;
            }

            if (name == "Dust,Edge")
            {
                //r.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
                //r.material.SetFloat("_AlphaBoost", 2.45f);
                r.enabled = false;
            }

            if (name == "Ring, Out")
            {
                r.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
            }
        }
        foreach (ParticleSystem p in tidalEruptionEffect.GetComponentsInChildren<ParticleSystem>())
        {
            var main = p.main;
            var name = p.name;

            if (name == "Billboard, Directional")
            {
                main.startColor = oceanColor;
            }

            if (name == "Billboard, Big Splash")
            {
                main.startColor = oceanColor;
            }

            if (name == "Billboard, Splash")
            {
                main.startColor = oceanColor;
            }

            if (name == "Billboard, Dots")
            {

            }

            if (name == "Sparks, Collision")
            {

            }

            if (name == "Lightning, Spark Center")
            {

            }

            if (name == "Ring")
            {

            }

            if (name == "Debris")
            {

            }

            if (name == "Dust,Edge")
            {
                main.startColor = oceanColor;
            }

            if (name == "Ring, Out")
            {
                main.startColor = oceanColor;
            }
        }
        Utils.RegisterEffect(tidalEruptionEffect, -1f, "Play_clayGrenadier_attack1_explode");

        tidalProjectileGhost = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/ClayGrenadier/ClayGrenadierBarrelGhost.prefab"), "TwinsGeyserGhost", false);
        tidalProjectileGhost.transform.localScale = Vector3.one * 0.5f;
        var gPsr = tidalProjectileGhost.GetComponentInChildren<ParticleSystemRenderer>();
        gPsr.material.SetTexture("_RemapTex", Modules.KamunagiAssets.assetBundle.LoadAsset<Texture2D>("geyserRemapTex"));
        gPsr.material.SetColor(oceanColor);
        gPsr.material.SetFloat("_AlphaCutoff", 0.13f);
        var gMr = tidalProjectileGhost.GetComponentInChildren<MeshRenderer>(true);
        gMr.material = Load<Material>("RoR2/Base/Common/VFX/matDistortion.mat");
        var blueLight = tidalProjectileGhost.AddComponent<Light>();
        blueLight.type = LightType.Point;
        blueLight.range = 8.43f;
        blueLight.color = oceanColor;
        blueLight.intensity = 20f;
        blueLight.lightShadowCasterMode = LightShadowCasterMode.NonLightmappedOnly;
        tidalProjectileGhost.GetComponent<VFXAttributes>().optionalLights = new Light[] { blueLight };

        tidalImpactEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/ClayGrenadier/ClayGrenadierBarrelExplosion.prefab"), "TwinsGeyserGhostImpact", false);
        tidalImpactEffect.transform.localScale = Vector3.one * 3f;
        var giDecal = tidalImpactEffect.GetComponentInChildren<Decal>();
        giDecal.Material = new Material(giDecal.Material);
        giDecal.Material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
        var geyserParticles = tidalImpactEffect.GetComponentsInChildren<ParticleSystemRenderer>();
        geyserParticles[0].enabled = false;
        geyserParticles[1].material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
        geyserParticles[1].material = new Material(geyserParticles[1].material);
        geyserParticles[1].material.SetFloat("_Cutoff", 0.38f);

        geyserParticles[2].material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png"));
        geyserParticles[3].enabled = false;
        Utils.RegisterEffect(tidalImpactEffect, -1f, "Play_acrid_m2_explode");

        tidalEruptionProjectile = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/ClayGrenadier/ClayGrenadierMortarProjectile.prefab"), "TwinsGeyserEruptionProjectile", true);
        /*geyserEruptionProjectile.GetComponent<ProjectileController>().ghostPrefab = geyserEruptionEffect;*/ //These kinds of projectiles don't have ghosts?????? Why are they even projectiles then???? This is just functionally a blast attack???
        tidalEruptionProjectile.GetComponent<ProjectileImpactExplosion>().impactEffect = tidalEruptionEffect;
        var gParticles = tidalEruptionProjectile.GetComponentsInChildren<ParticleSystemRenderer>();
        gParticles[0].enabled = false;
        gParticles[1].enabled = false;
        var healImpact = tidalEruptionProjectile.AddComponent<ProjectileHealOwnerOnDamageInflicted>();
        healImpact.fractionOfDamage = 0.5f;
        ProjectileImpactExplosion geyserImpact = tidalEruptionProjectile.GetComponent<ProjectileImpactExplosion>();
        geyserImpact.falloffModel = BlastAttack.FalloffModel.None;
        geyserImpact.blastDamageCoefficient = 3f;
        tidalEruptionProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.SlowOnHit;
        ContentAddition.AddProjectile(tidalEruptionProjectile);

        tidalProjectile = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/ClayGrenadier/ClayGrenadierBarrelProjectile.prefab"), "TwinsGeyserProjectile", true);
        var almostdoneBro = tidalProjectile.GetComponent<ProjectileController>(); //.ghostPrefab = tidalProjectileGhost;
        almostdoneBro.ghostPrefab = tidalProjectileGhost;
        almostdoneBro.startSound = null;
        tidalProjectile.GetComponent<Rigidbody>().useGravity = false;
        tidalProjectile.GetComponent<ProjectileSimple>().desiredForwardSpeed = 80f;
        tidalProjectile.GetComponent<ProjectileImpactExplosion>().impactEffect = tidalImpactEffect;
        tidalProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.SlowOnHit;
        ContentAddition.AddProjectile(tidalProjectile);

        luckyTidalProjectile = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/ClayGrenadier/ClayGrenadierBarrelProjectile.prefab"), "TwinsGeyserSpawnChild", true);
        var gImpact = luckyTidalProjectile.GetComponent<ProjectileImpactExplosion>();
        gImpact.impactEffect = tidalImpactEffect;
        gImpact.fireChildren = true;
        gImpact.childrenCount = 1;
        gImpact.childrenDamageCoefficient = 1;
        gImpact.childrenProjectilePrefab = tidalEruptionProjectile;
        luckyTidalProjectile.GetComponent<ProjectileController>().ghostPrefab = tidalProjectileGhost;
        luckyTidalProjectile.GetComponent<Rigidbody>().useGravity = false;
        luckyTidalProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.SlowOnHit;
        ContentAddition.AddProjectile(luckyTidalProjectile);

        miniSunGhost = PrefabAPI.InstantiateClone(miniSunChargeEffect, "TwinsMiniSunGhost", false);
        UnityEngine.Object.Destroy(miniSunGhost.GetComponent<ObjectScaleCurve>());
        var minisunMesh = miniSunGhost.GetComponentInChildren<MeshRenderer>();
        minisunMesh.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/FireballsOnHit/texFireballsOnHitIcon.png"));
        minisunMesh.material.SetFloat("_AlphaBoost", 6.351971f);
        miniSunGhost.AddComponent<ProjectileGhostController>();
        miniSunGhost.AddComponent<MeshFilter>().mesh = Load<Mesh>("RoR2/Base/Common/VFX/mdlVFXIcosphere.fbx");
        var miniSunIndicator = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Grandparent/GrandparentGravSphere.prefab").transform.GetChild(0).gameObject, "MiniSunIndicator", false);
        miniSunIndicator.transform.parent = miniSunGhost.transform; //this was the first time I figured this out
        miniSunIndicator.transform.localPosition = Vector3.zero;
        miniSunIndicator.transform.localScale = Vector3.one * 25f;

        var uhhh = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Grandparent/GrandparentGravSphereGhost.prefab").transform.GetChild(0).gameObject, "Indicator", false);
        var gooDrops = PrefabAPI.InstantiateClone(uhhh.transform.GetChild(3).gameObject, "MiniSunGoo", false);
        gooDrops.transform.parent = miniSunGhost.transform; // adding the indicator sphere to DenebokshiriBrimstone
        gooDrops.transform.localPosition = Vector3.zero;

        miniSunProjectile = PrefabAPI.InstantiateClone(windBoomerang, "TwinsMiniSun", true);
        UnityEngine.Object.Destroy(miniSunProjectile.GetComponent<WindBoomerangProjectile>());
        UnityEngine.Object.Destroy(miniSunProjectile.GetComponent<BoomerangProjectile>()); //bro what is this spaghetti
        UnityEngine.Object.Destroy(miniSunProjectile.GetComponent<ProjectileOverlapAttack>());
        UnityEngine.Object.Destroy(miniSunProjectile.GetComponent<ProjectileDotZone>());
        var minisunController = miniSunProjectile.GetComponent<ProjectileController>();
        minisunController.ghostPrefab = miniSunGhost;
        minisunController.flightSoundLoop = null;
        minisunController.startSound = "Play_fireballsOnHit_impact";
        //miniSunProjectile.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(Denebokshiri);
        var minisunSimple = miniSunProjectile.AddComponent<ProjectileSimple>();
        minisunSimple.desiredForwardSpeed = 20f;
        minisunSimple.lifetime = 5f;
        minisunSimple.lifetimeExpiredEffect = fireHitEffect;
        var singleI = miniSunProjectile.AddComponent<ProjectileSingleTargetImpact>();
        singleI.impactEffect = fireHitEffect;
        singleI.destroyOnWorld = true;
        var proxBeam = miniSunProjectile.AddComponent<ProjectileProximityBeamController>();
        proxBeam.attackFireCount = 1;
        proxBeam.attackInterval = 0.15f;
        proxBeam.listClearInterval = 1;
        proxBeam.attackRange = 15f; //radius
        proxBeam.minAngleFilter = 0;
        proxBeam.maxAngleFilter = 180;
        proxBeam.procCoefficient = 0.3f;
        proxBeam.damageCoefficient = 0.1f;
        proxBeam.bounces = 0;
        proxBeam.lightningType = LightningOrb.LightningType.Loader;
        proxBeam.inheritDamageType = true;
        /*var soundWorkaround = miniSunProjectile.AddComponent<ProjectileDotZone>();
            soundWorkaround.damageCoefficient = 0;
            soundWorkaround.attackerFiltering = AttackerFiltering.NeverHitSelf;
            soundWorkaround.impactEffect = null;
            soundWorkaround.overlapProcCoefficient = 0.1f;
            soundWorkaround.fireFrequency = 1f;
            soundWorkaround.resetFrequency = 0.01f;
            soundWorkaround.lifetime = 2f;
            //space
            soundWorkaround.soundLoopString = "Play_fireballsOnHit_pool_aliveLoop";
            soundWorkaround.soundLoopStopString = "Stop_fireballsOnHit_pool_aliveLoop";*/
        ContentAddition.AddProjectile(miniSunProjectile);

        blankslateObelisk = PrefabAPI.InstantiateClone(onkamiMesh, "BlankSlateObelisk", false);
        UnityEngine.Object.Destroy(blankslateObelisk.GetComponent<ObjectScaleCurve>());
        blankslateObelisk.AddComponent<ProjectileGhostController>();

        testProjectile = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Mage/MageFireboltBasic.prefab"), "TwinsTestProjectile", true);
        testProjectile.GetComponent<ProjectileController>().ghostPrefab = blankslateObelisk;
        ContentAddition.AddProjectile(testProjectile);

        laserSigil = Modules.KamunagiAssets.assetBundle.LoadAsset<GameObject>("LaserMuzzle.prefab");
        laserSigil.transform.localScale = Vector3.one * 0.6f;
        Utils.RegisterEffect(laserSigil, 5f, "");

        lightningMuzzle = Modules.KamunagiAssets.assetBundle.LoadAsset<GameObject>("MikazuchiMuzzle.prefab");
        //Utils.AddScaleComponent(lightningMuzzle, 1f);

        var voidFogMild = Prefabs.Load<GameObject>("RoR2/Base/Common/VoidFogMildEffect.prefab");
        curseParticles = PrefabAPI.InstantiateClone(voidFogMild.transform.GetChild(0).gameObject, "CurseParticles", false);
        curseParticles.transform.position = new Vector3(0f, 1f, 0f);
        var tempParticles = PrefabAPI.InstantiateClone(curseParticles, "CurseTempParticles", false);
        var gaySmoke = curseParticles.GetComponentsInChildren<ParticleSystemRenderer>();
        gaySmoke[1].enabled = false;

        mithrixPreBossBillboard = PrefabAPI.InstantiateClone(curseParticles.transform.GetChild(3).gameObject, "TwinsVeilDistortionBB", false);
        mithrixPreBossBillboard.transform.localScale = Vector3.one * 1.5f;
        UnityEngine.Object.Destroy(mithrixPreBossBillboard.GetComponent<Light>());
        UnityEngine.Object.Destroy(mithrixPreBossBillboard.GetComponent<RoR2.FlickerLight>());
        var flickerBB = mithrixPreBossBillboard.AddComponent<ParticleSystem>();
        var flickerPSR = mithrixPreBossBillboard.GetComponent<ParticleSystemRenderer>();
        flickerPSR.material = Load<Material>("RoR2/Base/Common/VFX/matInverseDistortion.mat");
        flickerPSR.renderMode = ParticleSystemRenderMode.Billboard;
        var bbMain = flickerBB.main;
        bbMain.duration = 1f;
        bbMain.simulationSpeed = 0.9f;
        bbMain.loop = true;
        bbMain.startLifetime = 0.13f;
        bbMain.startSpeed = 5f;
        bbMain.startSize3D = false;
        bbMain.startSizeY = 0.6f;
        bbMain.startRotation3D = false;
        bbMain.startRotationZ = 0.1745f;
        bbMain.startSpeed = 0f;
        bbMain.maxParticles = 30;
        var bbShape = flickerBB.shape;
        bbShape.enabled = false;
        bbShape.shapeType = ParticleSystemShapeType.Circle;
        bbShape.radius = 0.67f;
        bbShape.arcMode = ParticleSystemShapeMultiModeValue.Random;
        var bbSizeOverLifetime = flickerBB.sizeOverLifetime;
        bbSizeOverLifetime.enabled = true;
        bbSizeOverLifetime.separateAxes = true;
        //sparkleSize.sizeMultiplier = 0.75f;
        bbSizeOverLifetime.xMultiplier = 1.3f;

        tmpCurseParticles = tempParticles;
        tmpCurseParticles.AddComponent<DestroyOnTimer>().duration = 2f;

        var voidstageThing = Load<GameObject>("RoR2/DLC1/voidstage/VoidXYZMesh1OpenVariant.prefab");
        voidSphereAsset = PrefabAPI.InstantiateClone(voidstageThing.transform.GetChild(2).gameObject, "TwinsVoidSphereAsset", false);



        JachdwaltStrikeEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Merc/OmniImpactVFXSlashMercEvis.prefab"), "JachdwaltStrikeEffect", false);
        Material soDifficult0 = new Material(Load<Material>("RoR2/Base/Common/VFX/matOmniHitspark3.mat"));
        soDifficult0.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampEngi.png"));
        Material soDifficult1 = new Material(Load<Material>("RoR2/Base/Common/VFX/matOmniHitspark4.mat"));
        soDifficult1.SetColor("_TintColor", new Color(0.61f, 1f, 0.55f));
        Material soDifficult2 = new Material(Load<Material>("RoR2/Base/Common/VFX/matOmniRing2.mat"));
        soDifficult2.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAntler.png"));
        Material soDifficult3 = new Material(Load<Material>("RoR2/Base/Common/VFX/matOmniHitspark2.mat"));
        soDifficult3.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampCaptainAirstrike.png"));

        var whatIsThis = JachdwaltStrikeEffect.GetComponent<OmniEffect>();
        var array = whatIsThis.omniEffectGroups;
        array[6].omniEffectElements[0].particleSystemOverrideMaterial = soDifficult3;
        array[3].omniEffectElements[0].particleSystemOverrideMaterial = soDifficult2;
        array[3].omniEffectElements[1].particleSystemOverrideMaterial = soDifficult2;
        array[1].omniEffectElements[0].particleSystemOverrideMaterial = soDifficult1;
        array[4].omniEffectElements[0].particleSystemOverrideMaterial = soDifficult0;
        array[4].omniEffectElements[1].particleSystemOverrideMaterial = soDifficult0;
        foreach (ParticleSystemRenderer r in JachdwaltStrikeEffect.GetComponentsInChildren<ParticleSystemRenderer>())
        {
            var name = r.name;
            var greenWithLessAlpha = new Color(0.01712415f, 0.3615091f, 0);

            if (name == "Hologram")
            {
                r.material.SetColor("_TintColor", windSpellColor);
            }

            if (name == "Scaled Hitspark 3, Radial (Random Color)")
            {
                //this one is being stubborn 0
                var mat = r.material;

                mat.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampEngi.png"));
            }

            if (name == "Scaled Hitspark 4, Directional (Random Color) (1)")
            {
                //this one is being stubborn 1
                var mat = r.material;
                mat.SetColor("_TintColor", new Color(0.61f, 1f, 0.55f));
            }

            if (name == "Impact Slash")
            {
                //this one is being stubborn 2
                r.material = soDifficult2;
            }

            if (name == "Scaled Hitspark 2 (Random Color)")
            {
                //this one is being stubborn 3
                var mat = r.material;
                mat.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampCaptainAirstrike.png"));
            }
        }

        foreach (ParticleSystem p in JachdwaltStrikeEffect.GetComponentsInChildren<ParticleSystem>(false))
        {
            var name = p.name;
            var main = p.main;

            if (name == "Scaled Hitspark 3, Radial (Random Color)")
            {
                main.startColor = new ParticleSystem.MinMaxGradient(Color.white);
            }

            if (name == "Scaled Hitspark 4, Directional (Random Color)")
            {
                main.startColor = new ParticleSystem.MinMaxGradient(Color.white);
            }

            if (name == "Dash, Bright")
            {
                main.startColor = Color.white;
            }

            if (name == "Flash, Hard")
            {
                main.startColor = Color.white;
            }
        }
        Utils.RegisterEffect(JachdwaltStrikeEffect, -1f, "");

        ascensionSparks.transform.localScale = Vector3.one * 0.25f;
        var ascP = ascensionSparks.GetComponent<ParticleSystem>();
        var sparkEmit = ascP.emission;
        sparkEmit.rate = new ParticleSystem.MinMaxCurve(120f); //holy fuck

        VacuumGhost = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Blackhole/GravSphere.prefab").transform.GetChild(2).gameObject, "TwinsVacuumGhost", false);
        ascensionSparks.transform.SetParent(VacuumGhost.transform);
        var innerSphere = VacuumGhost.transform.GetChild(0).gameObject;
        innerSphere.GetComponent<MeshRenderer>().material = Load<Material>("RoR2/Base/Nullifier/matNullifierGemPortal.mat");
        VacuumGhost.AddComponent<ProjectileGhostController>();

        soStupid = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Grandparent/GrandparentGravSphereTether.prefab"), "ImTooLazyToDoThis", false);
        soStupid.GetComponent<LineRenderer>().enabled = false;

        VacuumProjectile = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Grandparent/GrandparentGravSphere.prefab"), "TwinsVacuumSphere", true);
        var vacuumSimple = VacuumProjectile.GetComponent<ProjectileSimple>();
        vacuumSimple.desiredForwardSpeed = 0f;
        vacuumSimple.lifetime = 1f;
        VacuumProjectile.GetComponent<TetherVfxOrigin>().tetherPrefab = soStupid;
        UnityEngine.Object.Destroy(VacuumProjectile.transform.GetChild(0).gameObject);
        VacuumProjectile.GetComponent<ProjectileController>().ghostPrefab = VacuumGhost;
        ContentAddition.AddProjectile(VacuumProjectile);

        //end my (Nines) own prefabs
        #endregion NinesCustomPrefabs

        chargeSunEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Grandparent/ChargeGrandParentSunHands.prefab"), "TwinsUltHands", false);
        var shutTheFuckUp = chargeSunEffect.GetComponentInChildren<RoR2.FlickerLight>();
        UnityEngine.Object.Destroy(shutTheFuckUp);
        chargeSunEffect.transform.localScale = Vector3.one * 0.35f;
        chargeSunEffect.GetComponentInChildren<ObjectScaleCurve>().transform.localScale = Vector3.one * 1.5f;
        UnityEngine.Object.Destroy(chargeSunEffect.GetComponentInChildren<Light>());
        var Sunmesh = chargeSunEffect.GetComponentInChildren<MeshRenderer>(true);
        Sunmesh.gameObject.SetActive(true);
        Sunmesh.material = new Material(Sunmesh.material); //todo this is probably why your remap ramps aren't working, do it the way Dragonyck does it like right here
        Sunmesh.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/DLC1/Common/ColorRamps/texRampBottledChaos.png"));
        var sunP = chargeSunEffect.GetComponentsInChildren<ParticleSystemRenderer>(true);
        //sunP[0].material = new Material(sunP[0].material);
        sunP[0].material.SetColor("_TintColor", new Color(0.45f, 0, 1));
        sunP[0].material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
        var lol = chargeSunEffect.GetComponentsInChildren<ParticleSystem>();
        var mainMain = lol[1].main;
        mainMain.startColor = new Color(0.45f, 0, 1);

        sunStreamEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Grandparent/GrandParentSunChannelStartStream.prefab"), "AxiomStream", false);
        sunStreamEffect.GetComponent<ChildLocator>().FindChild("EndPoint").gameObject.AddComponent<DestroyOnDestroy>().target = sunStreamEffect;
        UnityEngine.Object.Destroy(sunStreamEffect.GetComponentInChildren<MeshRenderer>(true).gameObject);
        var sunSP = sunStreamEffect.GetComponentsInChildren<ParticleSystemRenderer>(true)[1];
        sunSP.transform.localScale = Vector3.one * 0.25f;
        var m = sunSP.trailMaterial;
        sunSP.trailMaterial = new Material(m);
        sunSP.trailMaterial.SetColor("_TintColor", new Color(0.45f, 0, 1));
        sunSP.trailMaterial.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));

        #region LightOfNaturesAxiom
        naturesAxiom = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Grandparent/GrandParentSun.prefab"), "TwinsUltSun", true);
        UnityEngine.Object.Destroy(naturesAxiom.GetComponent<EntityStateMachine>());
        UnityEngine.Object.Destroy(naturesAxiom.GetComponent<NetworkStateMachine>());
        UnityEngine.Object.Destroy(naturesAxiom.GetComponent<GrandParentSunController>());
        naturesAxiom.AddComponent<UmbralSunController>();
        var vfxRoot = naturesAxiom.transform.GetChild(0).gameObject;
        vfxRoot.transform.localScale = Vector3.one * 0.5f;
        var sunL = naturesAxiom.GetComponentInChildren<Light>();
        sunL.intensity = 100;
        sunL.range = 70;
        sunL.color = new Color(0.45f, 0, 1);
        var sunMeshes = naturesAxiom.GetComponentsInChildren<MeshRenderer>(true);
        var sunIndicator = sunMeshes[0];
        sunIndicator.material = new Material(sunIndicator.material);
        sunIndicator.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/DLC1/Common/ColorRamps/texRampPortalVoid.png"));
        sunIndicator.material.SetColor("_TintColor", new Color(0.45f, 0, 1));
        sunIndicator.transform.localScale = Vector3.one * 85f; //visual indicator
        var Sunmesh2 = sunMeshes[1];
        Sunmesh2.material = Sunmesh.material;
        sunMeshes[2].enabled = false;
        var sunPP = naturesAxiom.GetComponentInChildren<PostProcessVolume>();
        sunPP.profile = Load<PostProcessProfile>("RoR2/Base/Common/ppLocalVoidFogMild.asset");
        sunPP.sharedProfile = sunPP.profile;
        sunPP.gameObject.AddComponent<SphereCollider>().radius = 40;
        foreach (ParticleSystemRenderer r in naturesAxiom.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            var name = r.name;
            if (name == "GlowParticles, Fast")
            {
                r.material = sunP[0].material;
                r.transform.localScale = Vector3.one * 0.6f;
            }

            if (name == "GlowParticles")
            {
                r.enabled = false;
            }
            if (name == "SoftGlow, Backdrop")
            {
                r.material = new Material(Load<Material>("RoR2/Junk/Common/VFX/matTeleportOutBodyGlow.mat"));
                r.material.SetColor("_TintColor", new Color(0f, 0.4F, 1)); //todo example of new Material()
                r.transform.localScale = Vector3.one * 0.5f;
            }
            if (name == "Donut" || name == "Trails")
            {
                r.material = new Material(r.material);
                r.material.SetColor("_TintColor", new Color(0.45f, 0, 1));
                r.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
                r.trailMaterial = r.material;
            }
            if (name == "Goo, Drip")
            {
                r.enabled = false;
            }
            if (name == "Sparks")
            {
                r.enabled = false;
            }
        }
        #endregion LightOfNaturesAxiom

        sunExplosion = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Grandparent/GrandParentSunSpawn.prefab"), "SunExplosion", false);
        var sunePP = sunExplosion.GetComponentInChildren<PostProcessVolume>();
        sunePP.profile = sunPP.profile;
        sunePP.sharedProfile = sunPP.profile;
        var suneL = sunExplosion.GetComponentInChildren<Light>();
        suneL.intensity = 100;
        suneL.range = 40;
        suneL.color = new Color(0.45f, 0, 1);
        foreach (ParticleSystemRenderer r in sunExplosion.GetComponentsInChildren<ParticleSystemRenderer>())
        {
            var name = r.name;
            if (r.material)
            {
                r.material = new Material(r.material);
                r.material.SetColor("_TintColor", new Color(0.45f, 0, 1));
                r.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampAncientWisp.png"));
                r.trailMaterial = r.material;
            }
        }
        ContentAddition.AddEffect(sunExplosion);

        forceField = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/MajorAndMinorConstruct/MajorConstructBubbleShield.prefab"), "ForceField", true);
        forceField.GetComponentInChildren<MeshCollider>().gameObject.layer = 3;
        forceField.transform.localScale = Vector3.one * 0.7f;
        UnityEngine.Object.Destroy(forceField.GetComponent<NetworkedBodyAttachment>());
        UnityEngine.Object.Destroy(forceField.GetComponent<VFXAttributes>());
        var ffScale = forceField.GetComponentInChildren<ObjectScaleCurve>();
        ffScale.useOverallCurveOnly = true;
        ffScale.overallCurve = AnimationCurve.Linear(0, 0.35f, 1, 1);
        forceField.AddComponent<DestroyOnTimer>().duration = XinZhao.forcefieldDuration;
        var forceMesh = forceField.GetComponentInChildren<MeshRenderer>();
        var forceMaterials = forceMesh.sharedMaterials;
        forceMesh.sharedMaterials[0] = new Material(forceMaterials[0]);
        forceMesh.sharedMaterials[0].SetColor("_TintColor", new Color(0.07843f, 0, 1));
        forceMesh.sharedMaterials[0].SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampMoonLighting.png"));
        forceMesh.sharedMaterials[1] = new Material(forceMaterials[1]);
        forceMesh.sharedMaterials[1].SetColor("_TintColor", new Color(0.39215f, 0, 1));
        forceMesh.sharedMaterials[1].SetTexture("_RemapTex", Load<Texture2D>("RoR2/DLC1/Common/ColorRamps/texRampHippoVoidEye.png"));
        var forceP = forceField.GetComponentInChildren<ParticleSystemRenderer>();
        forceP.material = new Material(forceP.material);
        forceP.material.SetColor("_TintColor", new Color(0.07843f, 0.02745f, 1));
        forceP.material.DisableKeyword("VERTEXCOLOR");

        soeiMusouGhost = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterBigGhost.prefab"), "VoidProjectileSimpleGhost", false);
        var simple1Mesh = soeiMusouGhost.GetComponentInChildren<MeshRenderer>();
        //Solid Parallax
        simple1Mesh.materials[0].SetTexture("_EmissionTex", Load<Texture2D>("RoR2/DLC1/voidraid/texRampVoidRaidSky.png"));
        simple1Mesh.materials[0].SetFloat("_EmissionPower", 1.5f);
        simple1Mesh.materials[0].SetFloat("_HeightStrength", 4.1f);
        simple1Mesh.materials[0].SetFloat("_HeightBias", 0.35f);
        simple1Mesh.materials[0].SetFloat("_Parallax", 1f);
        simple1Mesh.materials[0].SetColor(twinsLightPurple);
        //Cloud Remap 
        //simple1Mesh.materials[1].SetFloat("_SrcBlend", 8f); //One - dst alpha
        simple1Mesh.materials[1].SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampIce.png"));
        simple1Mesh.materials[1].SetColor("_TintColor", new Color(0.525f, 0f, 1f));
        simple1Mesh.materials[1].SetFloat("_AlphaBoost", 3.88f);
        var simple1Scale = soeiMusouGhost.AddComponent<ObjectScaleCurve>();
        simple1Scale.useOverallCurveOnly = true;
        simple1Scale.timeMax = 0.12f;
        simple1Scale.overallCurve = AnimationCurve.Linear(0, 0, 1, 1);
        simple1Scale.baseScale = Vector3.one * 0.6f;

        gravityVoidballProjectile = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterBigProjectile.prefab"), "GravityVoidProjectileSimple", true);
        var GvbiMPACT = gravityVoidballProjectile.GetComponent<ProjectileImpactExplosion>();
        GvbiMPACT.fireChildren = true;
        GvbiMPACT.childrenCount = 1;
        GvbiMPACT.childrenDamageCoefficient = 1;
        GvbiMPACT.childrenProjectilePrefab = nightshadePrisonDotZone;
        GvbiMPACT.blastRadius = 1f;
        var gVBSimple = gravityVoidballProjectile.GetComponent<ProjectileSimple>();
        gVBSimple.desiredForwardSpeed = 40;
        gVBSimple.lifetime = 10;
        gVBSimple.updateAfterFiring = false;
        gravityVoidballProjectile.GetComponent<Rigidbody>().useGravity = true;
        var antiGravity = gravityVoidballProjectile.AddComponent<AntiGravityForce>();
        antiGravity.rb = gravityVoidballProjectile.GetComponent<Rigidbody>();
        antiGravity.antiGravityCoefficient = 0.7f;
        ContentAddition.AddProjectile(gravityVoidballProjectile);

        soeiMusouProjectile = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterBigProjectile.prefab"), "VoidProjectileSimple", true);
        soeiMusouProjectile.GetComponent<ProjectileController>().ghostPrefab = soeiMusouGhost;
        var rb = soeiMusouProjectile.GetComponent<Rigidbody>();
        rb.useGravity = true;
        var antiGrav = soeiMusouProjectile.AddComponent<AntiGravityForce>();
        antiGrav.rb = rb;
        antiGrav.antiGravityCoefficient = 0.7f;
        ContentAddition.AddProjectile(soeiMusouProjectile);

        voidTracerSphere = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabSpinBeamChargeUp.prefab"), "VoidTracerSphere", false);
        voidTracerSphere.GetComponentInChildren<Light>().range = 30f;

        voidTracer = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabSpinBeamVFX.prefab"), "VoidTracer", false);
        var particles = voidTracer.GetComponentsInChildren<ParticleSystemRenderer>();
        particles[particles.Length - 1].transform.localScale = new Vector3(0, 0, 0.25f);
        UnityEngine.Object.Destroy(voidTracer.GetComponentInChildren<ShakeEmitter>());
        var sofuckingbright = voidTracer.transform.GetChild(3).gameObject;
        sofuckingbright.SetActive(false);
        var laserC = voidTracer.transform.GetChild(4).gameObject;
        var rarted = laserC.transform.GetChild(0).gameObject;
        rarted.SetActive(true);
        rarted.GetComponent<PostProcessVolume>().blendDistance = 19f;

        mothMoth = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Beetle/BeetleWard.prefab"), "Mothmoth", true);
        Material impMat = new Material(Load<Material>("RoR2/Base/Imp/matImpBoss.mat"));
        impMat.SetFloat("_Cull", 0);
        impMat.SetColor("_Color", new Color(0.2588235f, 0.2705882f, 0.6352941f));
        impMat.SetColor("_EmColor", new Color(0.07058824f, 0.07058824f, 0.8823529f)); //you will probably need the shader stub for hgstandard here.
        impMat.SetTexture("_FresnelRamp", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLunarElectric.png"));
        impMat.SetTexture("_PrintRamp", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampHuntressSoft.png"));
        //Main color, Emission color, Fresnel ramp, and Print Ramp are your 4 big ticket items here
        var mothLight = mothMoth.GetComponentInChildren<Light>();
        mothLight.color = new Color(0f, 0.391f, 0.9f);
        mothLight.range = 4f;
        mothMoth.GetComponentInChildren<SkinnedMeshRenderer>().material = impMat;
        mothMoth.AddComponent<DestroyOnTimer>().duration = 10;
        var mothWParticles = mothMoth.GetComponentsInChildren<ParticleSystemRenderer>();
        mothWParticles[0].material = Load<Material>("RoR2/DLC1/PortalVoid/matPortalVoid.mat");
        mothWParticles[0].material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Captain/texRampCrosshair2.png"));
        mothWParticles[0].material.SetColor("_TintColor", new Color(0f, 0.6784314f,1f));
        mothWParticles[0].transform.localPosition = new Vector3(0f, 0.3f, 0f);
        mothWParticles[0].transform.localScale = Vector3.one * 0.3f;
        mothWParticles[1].enabled = false;
        Material outlineMaterial = new Material(Load<Material>("RoR2/Base/Nullifier/matNullifierZoneAreaIndicatorLookingIn.mat"));
        outlineMaterial.SetColor("_TintColor", new Color(0f, 0.274509804f, 1f));
        mothMoth.GetComponentInChildren<MeshRenderer>().material = outlineMaterial;
        /*var mothSMat = mothMoth.GetComponentInChildren<MeshRenderer>().material;
            mothSMat = new Material(Load<Material>("RoR2/Base/Nullifier/matNullifierZoneAreaIndicatorLookingIn.mat"));
            mothSMat.SetColor("_TintColor", new Color(0f, 0.274509804f, 1f));*/

        //wispWard = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/EliteHaunted/BeetleWard.prefab"), "SpiritWard", true);
        //UnityEngine.Object.Destroy(wispWard.transform.GetChild(0));
        //var wispMdl = 

        woshisWard = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/EliteHaunted/AffixHauntedWard.prefab"), "WoshisWard", true);
        Material woshisEnergy = new Material(Load<Material>("RoR2/Base/BleedOnHitAndExplode/matBleedOnHitAndExplodeAreaIndicator.mat"));
        woshisEnergy.SetFloat("_DstBlendFloat", 3f);
        woshisEnergy.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampImp2.png"));
        woshisEnergy.SetFloat("_Boost", 0.1f);
        woshisEnergy.SetFloat("_RimPower", 0.48f);
        woshisEnergy.SetFloat("_RimStrength", 0.12f);
        woshisEnergy.SetFloat("_AlphaBoost", 6.55f);
        woshisEnergy.SetFloat("_IntersectionStrength", 5.12f);

        woshisWard.GetComponentInChildren<MeshRenderer>().material = woshisEnergy;
        woshisWard.GetComponent<BuffWard>().radius = 10f;
        woshisWard.AddComponent<DestroyOnTimer>().duration = 8f;

        boulderGhost = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Grandparent/GrandparentBoulderGhost.prefab"), "BoulderProjectileGhost", false);
        boulderGhost.transform.localScale = Vector3.one * 0.3f; 

        boulderChargeEffect = PrefabAPI.InstantiateClone(boulderGhost, "BoulderChargeEffect", false);
        UnityEngine.Object.Destroy(boulderChargeEffect.GetComponent<ProjectileGhostController>());
        Utils.AddScaleComponent(boulderChargeEffect, 0.2f);

        boulderChild = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Grandparent/GrandparentMiniBoulder.prefab"), "BoulderChild", true);
        boulderChild.GetComponent<ProjectileImpactExplosion>().falloffModel = BlastAttack.FalloffModel.None;
        ContentAddition.AddProjectile(boulderChild);

        boulderProjectile = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Grandparent/GrandparentBoulder.prefab"), "BoulderProjectile", true);
        boulderProjectile.transform.localScale = Vector3.one * 0.3f;
        boulderProjectile.GetComponent<ProjectileController>().ghostPrefab = boulderGhost;
        var boulderImpact = boulderProjectile.GetComponent<ProjectileImpactExplosion>();
        boulderImpact.bonusBlastForce = new Vector3(20, 20, 20);
        boulderImpact.blastRadius = 5f;
        boulderImpact.childrenProjectilePrefab = boulderChild;
        boulderImpact.blastDamageCoefficient = 1f;
        boulderImpact.childrenDamageCoefficient = 0.5f;
        boulderImpact.falloffModel = BlastAttack.FalloffModel.None;
        boulderProjectile.GetComponent<Rigidbody>().useGravity = false;
        boulderProjectile.GetComponent<SphereCollider>().radius = 3.5f;
        ContentAddition.AddProjectile(boulderProjectile);
    }
        
        [SystemInitializer(typeof(BuffCatalog))]
        private static void BuffCatalogInit()
        {
            mothMoth.GetComponent<BuffWard>().buffDef = RoR2Content.Buffs.LifeSteal;
            woshisWard.GetComponent<BuffWard>().buffDef = Buffs.WoshisDebuff;
        } 
}