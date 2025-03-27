using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ParticleType
{
    DamageExplosion,
    WeaponFire,
    WeaponSmoke
}

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance { get; private set; }

    private Dictionary<ParticleType, GameObject> particleSystemDic = new Dictionary<ParticleType, GameObject>();
    private Dictionary<ParticleType, Queue<GameObject>> particlePools = new Dictionary<ParticleType, Queue<GameObject>>();

    public GameObject weaponexplosionParticle;
    public GameObject weaponFireParticle;
    //public GameObject weaponSmokeParticle;

    public int poolSize = 30;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        particleSystemDic.Add(ParticleType.DamageExplosion, weaponexplosionParticle);
        particleSystemDic.Add(ParticleType.WeaponFire, weaponFireParticle);
        //particleSystemDic.Add(ParticleType.WeaponSmoke, weaponSmokeParticle);

        foreach (var type in particleSystemDic.Keys)
        {
            Queue<GameObject> pool = new Queue<GameObject>();
            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(particleSystemDic[type]);
                obj.gameObject.SetActive(false);
                pool.Enqueue(obj);
            }
            particlePools.Add(type, pool);
        }
    }

    public void ParticlePlay(ParticleType type, Vector3 position, Vector3 scale)
    {
        //if (particleSystemDic.ContainsKey(ParticleType.DamageExplosion))
        //{
        //    ParticleSystem particle = Instantiate(particleSystemDic[type], position, Quaternion.identity);
        //    // particle.gameObject.transform.localScale =(Vector3)scale; 스케일 조정 등 여러 기능 추가 가능
        //    Transform playerTransform = PlayerManager.Instance.transform;
        //    Vector2 directionToPlayer = playerTransform.position - position;
        //    Quaternion rotation = Quaternion.LookRotation(directionToPlayer);
        //    particle.Play();
        //    Destroy(particle.gameObject, particle.main.duration);
        //}

        // object pool
        if (particlePools.ContainsKey(type))
        {
            GameObject particleObj = particlePools[type].Dequeue();

            if (particleObj != null)
            {
                particleObj.transform.position = position;
                ParticleSystem particleSystem = particleObj.GetComponentInChildren<ParticleSystem>();

                if (particleSystem.isPlaying)
                {
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                particleObj.transform.localScale = scale;
                particleObj.SetActive(true);
                particleSystem.Play();
                StartCoroutine(particleEnd(type, particleObj, particleSystem));
            }
        }
    }

    IEnumerator particleEnd(ParticleType type, GameObject particleObj, ParticleSystem particleSystem)
    {
        while (particleSystem.isPlaying)
        {
            yield return null;
        }

        particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particleObj.SetActive(false);
        particlePools[type].Enqueue(particleObj);
    }
}
