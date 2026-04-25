using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyAmmoBox : MonoBehaviour
{
    public static EnemyAmmoBox Instance { get; private set; }

    [SerializeField] private GameObject enemyProjectilePrefab;
    private ObjectPool<GameObject> ammoPool;

    private List<GameObjectLiveTime> activeProjectiles = new List<GameObjectLiveTime>();
    float currentTime = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        ammoPool = new ObjectPool<GameObject>(() => Instantiate(enemyProjectilePrefab),
             (obj) => obj.SetActive(false), (obj) => obj.SetActive(true), null, true, 10);
        
        currentTime = Time.time;

        InvokeRepeating(nameof(CheckForExpiredProjectiles), 1f, 1f); // Check every second for expired projectiles


    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public GameObject[] getAmmo(int count)
    {
        GameObject[] ammo = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            ammo[i] = ammoPool.Get();
        }
        return ammo;
    }

    private void CheckForExpiredProjectiles()
    {
        float currentTime = Time.time;
        for (int i = activeProjectiles.Count - 1; i >= 0; i--)
        {
            if (currentTime - activeProjectiles[i].liveTime > 5f) // Example lifetime of 5 seconds
            {
                ammoPool.Release(activeProjectiles[i].obj);
                activeProjectiles.RemoveAt(i);
            }
        }
    }

    private struct GameObjectLiveTime
    {
        public GameObject obj;
        public float liveTime;

        public GameObjectLiveTime(GameObject obj, float liveTime)
        {
            this.obj = obj;
            this.liveTime = liveTime;
        }
    }
}
