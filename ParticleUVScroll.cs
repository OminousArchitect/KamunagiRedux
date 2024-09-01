using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Kamunagi
{

    [ExecuteInEditMode]
    public class ParticleUVScroll : MonoBehaviour
    {
        [SerializeField]
        ParticleSystem particleSystem;
        [SerializeField]
        bool isSubEmmitter;
        //[SerializeField]
        //Vector2 beginningUVs;
        //[SerializeField]
        //Vector2 endUVs;
        [SerializeField]
        float endOffsetX;
        [SerializeField]
        float endOffsetY = -0.6f ;
        [SerializeField]
        Material material;
        [SerializeField]
        List<int> shaderIDs;
        [SerializeField]
        float duration;
        [SerializeField]
        float offX;
        [SerializeField]
        float offY;
        [SerializeField]
        float delay;
        [SerializeField]
        float ltime;
        [SerializeField]
        float ptime;
        ParticleSystem parentSystem;
        float lifetime;
        [SerializeField]
        bool useLifetime = true;
        [SerializeField]
        AnimationCurve xCurve,yCurve;
        [SerializeField]
        bool useCurve = false;
        float curveTime;
        [SerializeField]
        float randomXStartPerc;
        [SerializeField]
        float randomYStartPerc;

        void Start()
        {
           
        }
        void OnEnable()
        {
            particleSystem = gameObject.GetComponent<ParticleSystem>();
            var main = particleSystem.main;
            ltime = 0;
            if (isSubEmmitter)
            {
                parentSystem = gameObject.transform.parent.GetComponent<ParticleSystem>();
                var parentSub = parentSystem.subEmitters;
                int index = 0;
                for (int i = 0; i < parentSub.subEmittersCount; i++)
                {
                    if (parentSub.GetSubEmitterSystem(i) == particleSystem)
                    {
                        index = i;
                    }

                }
                ParticleSystemSubEmitterProperties parentProp = parentSub.GetSubEmitterProperties(index);
                if (useLifetime)
                {
                    if (parentProp.HasFlag(ParticleSystemSubEmitterProperties.InheritLifetime))
                    {
                        lifetime = parentSystem.main.startLifetime.constant * main.startLifetime.constant;
                        delay = parentSystem.main.startDelay.constant + main.startDelay.constant;
                    }
                    else
                    {
                        lifetime = main.startLifetime.constant;
                        delay = parentSystem.main.startDelay.constant + main.startDelay.constant;
                    }
                }
                else
                {


                    if (parentProp.HasFlag(ParticleSystemSubEmitterProperties.InheritDuration))
                    {
                        duration = (parentSystem.main.duration * main.duration);
                        delay = parentSystem.main.startDelay.constant + main.startDelay.constant;
                    }
                    else
                    {
                        duration = main.duration;
                        delay = parentSystem.main.startDelay.constant + main.startDelay.constant;
                    }
                }

            }
            else
            {
                if (useLifetime)
                {
                    lifetime = main.startLifetime.constant;
                    delay = main.startDelay.constant;
                }
                else
                {


                    duration = main.duration;
                    delay = main.startDelay.constant;
                }
            }


            
                

            material = gameObject.GetComponent<ParticleSystemRenderer>().sharedMaterial;
            shaderIDs = new List<int>();
            if (material.HasProperty(Shader.PropertyToID("_MainTex")))
                shaderIDs.Add(Shader.PropertyToID("_MainTex"));
            if (material.HasProperty(Shader.PropertyToID("_BumpMap")))
                shaderIDs.Add(Shader.PropertyToID("_BumpMap"));
            if (material.HasProperty(Shader.PropertyToID("_MaskTex")))
                shaderIDs.Add(Shader.PropertyToID("_MaskTex"));
            if (material.HasProperty(Shader.PropertyToID("_EmissionMap")))
                shaderIDs.Add(Shader.PropertyToID("_EmissionMap"));


        }
        
        void Update()
        {
            if (particleSystem.isPlaying)
            {

                    if (isSubEmmitter)
                    ptime = parentSystem.time;
                else
                    ptime = particleSystem.time;

                //if (particleSystem.time >= delay)
                //{

                    if (useLifetime)
                    ltime = (ptime) / lifetime;
                else
                    ltime = (ptime) / (duration);
                if (useCurve)
                {
                    offX = xCurve.Evaluate(ltime);
                    if (randomXStartPerc != 0)
                        offX += AddStartOffset(randomXStartPerc);
                    offY = yCurve.Evaluate(ltime);
                    if (randomYStartPerc != 0)
                        offY += AddStartOffset(randomYStartPerc);
                    foreach (int ID in shaderIDs)
                    {
                        if (material.GetTexture(ID))
                        {
                            material.SetTextureOffset(ID, new Vector2(offX, offY));
                        }
                    }
                }
                else
                {


                    if (endOffsetX != 0)
                    {
                        offX = Mathf.Lerp(0, endOffsetX, ltime);
                        if (randomXStartPerc != 0)
                            offX += AddStartOffset(randomXStartPerc);
                    }
                        
                    else
                        offX = 0;
                    if (endOffsetY != 0)
                    {
                        offY = Mathf.Lerp(0, endOffsetY, ltime);
                        if(randomYStartPerc != 0)
                            offY += AddStartOffset(randomYStartPerc);
                    }
                        
                    else
                        offY = 0;
                    foreach (int ID in shaderIDs)
                    {
                        if (material.GetTexture(ID))
                        {
                            material.SetTextureOffset(ID, new Vector2(offX, offY));
                        }
                    }
              //  }
                }

            }
            else if(material.mainTextureOffset!=Vector2.zero)
            {
                offX = 0;
                offY = 0;
                foreach (int ID in shaderIDs)
                {
                    if (material.GetTexture(ID))
                    {
                        material.SetTextureOffset(ID, new Vector2(offX, offY));
                    }
                }
            }
        }

        void OnDisable()
        {
            foreach (int ID in shaderIDs)
            {
                if (material.GetTexture(ID))
                    material.SetTextureOffset(ID, Vector2.zero);
            }
            shaderIDs.Clear();
        }

        float AddStartOffset(float offsetPerc)
        {
            float whole = 1 / offsetPerc;
            float rand = Random.Range(0, (int)whole-1);
            return (rand % whole)*offsetPerc;
        }
    }
}
