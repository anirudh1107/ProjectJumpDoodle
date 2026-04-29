using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class LevelManager : MonoBehaviour
{
    [Header("Architecture")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform leftBoundry;
    [SerializeField] private Transform rightBoundry;
    [SerializeField] private Transform freedomPoint;
    [SerializeField] private GameObject enemyManager;
    [SerializeField] private GameObject audioManager;

    [Header("Generation Settings")]
    [SerializeField] private float levelWidth = 5f;       // Horizontal bounds for spawning (-5 to 5)
    [SerializeField] private float minJumpGap = 1.5f;     // Minimum Y distance between platforms
    [SerializeField] private float maxJumpGap = 3.0f;     // Maximum Y distance between platforms (must be reachable!)
    [SerializeField] private float preGenerateBuffer = 15f; // How far above the camera to generate
    [SerializeField] private float cleanupDistance = 8f;    // Distance below camera to recycle objects
    [SerializeField] private float deathDistance = 6f;      // Distance below camera where player dies

    [Header("Prefabs")]
    [SerializeField] private GameObject[] platformPrefabs; // 0: Normal, 1: Moving, 2: Breakable, 3: Bouncy
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private GameObject collectiblePrefab;

    [Header("Difficulty Scaling")]
    [SerializeField] private float maxDifficultyHeight = 500f; // Height at which game reaches max difficulty
    [SerializeField] [Range(0, 1)] private float collectibleChance = 0.15f; // Constant 15% chance
    [SerializeField] [Range(0, 1)] private float minEnemyChance = 0.05f;
    [SerializeField] [Range(0, 1)] private float maxEnemyChance = 0.40f;

    // --- Internal State ---
    private float highestGeneratedY = 0f;
    private bool isGameOver = false;

    // Standard Lists to track active objects in the scene
    private List<GameObject> activePlatforms = new List<GameObject>();
    private List<GameObject> activeEnemies = new List<GameObject>();
    private List<GameObject> activeCollectibles = new List<GameObject>();

    // Dictionaries to hold Object Pools for different prefab types
    private Dictionary<int, ObjectPool<GameObject>> platformPools;
    private Dictionary<int, ObjectPool<GameObject>> enemyPools;
    private ObjectPool<GameObject> collectiblePool;

    private void Start()
    {
        InitializePools();
        InitializeLevel();
        
        Time.timeScale = 0f;
        // Generate the initial chunk of the level
        while (highestGeneratedY < preGenerateBuffer)
        {
            SpawnNextPlatform();
        }
        Time.timeScale = 1f;
    }

    private void InitializeLevel()
    {
        if(enemyManager != null)
        {
            Instantiate(enemyManager, transform);
        }
        if(audioManager != null)
        {
            Instantiate(audioManager, transform);
        }
    }

    private void Update()
    {
        if (isGameOver) return;

        // 1. Generate new level chunks as the camera moves up
        if (highestGeneratedY < cameraTransform.position.y + preGenerateBuffer)
        {
            SpawnNextPlatform();
        }

        // 2. Recycle objects that have fallen too far below the camera
        RecycleObjects(activePlatforms, platformPools);
        RecycleObjects(activeEnemies, enemyPools);
        RecycleLevelCollectibles();

        // 3. Check for Game Over (Player falls below the death zone)
        if (playerTransform.position.y < cameraTransform.position.y - deathDistance)
        {
            TriggerGameOver();
        }
    }

    private void SpawnNextPlatform()
    {
        // Calculate procedural position
        float randomX = Random.Range(-levelWidth, levelWidth);
        float randomY = highestGeneratedY + Random.Range(minJumpGap, maxJumpGap);
        Vector2 spawnPos = new Vector2(randomX, randomY);

        // Determine platform type based on current height
        int platformTypeIndex = GetPlatformTypeForHeight(randomY);
        
        // Spawn Platform from its specific pool
        GameObject platform = platformPools[platformTypeIndex].Get();
        if (platform.TryGetComponent<MoveAcrossScreen>(out MoveAcrossScreen mover))
        {
            // Set the left and right bounds for the moving platform
            mover.SetLeftAndRightPoints(leftBoundry, rightBoundry);
        }
        platform.transform.position = spawnPos;
        // Store the type index in the object's name/tag or a small component so we know which pool to return it to later
        platform.GetComponent<LevelEntity>().EntityTypeIndex = platformTypeIndex; 
        activePlatforms.Add(platform);

        // Try spawning a Collectible (Constant chance)
        bool spawnedCollectible = false;
        if (Random.value <= collectibleChance)
        {
            GameObject collectible = collectiblePool.Get();
            // Spawn slightly above the platform
            collectible.transform.position = spawnPos + Vector2.up * 1f; 
            activeCollectibles.Add(collectible);
            spawnedCollectible = true;
        }

        // Try spawning an Enemy (Chance scales with height, don't spawn if a collectible is already there)
        if (!spawnedCollectible)
        {
            float currentEnemyChance = Mathf.Lerp(minEnemyChance, maxEnemyChance, randomY / maxDifficultyHeight);
            if (Random.value <= currentEnemyChance)
            {
                int enemyTypeIndex = Random.Range(0, enemyPrefabs.Length); // Could also scale enemy type with height
                GameObject enemy = enemyPools[enemyTypeIndex].Get();
                if (enemy.TryGetComponent<MoveAcrossScreen>(out MoveAcrossScreen enemyMover))
                {
                    enemyMover.SetLeftAndRightPoints(leftBoundry, rightBoundry);
                }
                if (enemy.TryGetComponent<ShootAtTarget>(out ShootAtTarget shootTarget))
                {
                    shootTarget.SetTarget(playerTransform);
                }
                enemy.transform.position = spawnPos + Vector2.up * 1f;
                enemy.GetComponent<LevelEntity>().EntityTypeIndex = enemyTypeIndex;
                activeEnemies.Add(enemy);
            }
        }

        highestGeneratedY = randomY;
    }

    // --- Difficulty Logic ---

    private int GetPlatformTypeForHeight(float height)
    {
        // Example logic:
        // 0-50m: Only Normal (0)
        // 50-100m: Normal (0) and Moving (1)
        // 100-200m: Normal (0), Moving (1), Breakable (2)
        // 200m+: All types

        if (height < 50f) return 0;
        if (height < 100f) return Random.Range(0, 2);
        if (height < 200f) return Random.Range(0, 3);
        
        return Random.Range(0, platformPrefabs.Length);
    }

    // --- Recycling Logic ---

    private void RecycleObjects(List<GameObject> activeList, Dictionary<int, ObjectPool<GameObject>> pools)
    {
        // Loop backwards when removing from a list
        for (int i = activeList.Count - 1; i >= 0; i--)
        {
            GameObject obj = activeList[i];
            if (obj.transform.position.y < cameraTransform.position.y - cleanupDistance)
            {
                // Get the index we assigned earlier so we know which pool it belongs to
                int typeIndex = obj.GetComponent<LevelEntity>().EntityTypeIndex;
                pools[typeIndex].Release(obj);
                activeList.RemoveAt(i);
            }
        }
    }

    private void RecycleLevelCollectibles()
    {
        for (int i = activeCollectibles.Count - 1; i >= 0; i--)
        {
            GameObject obj = activeCollectibles[i];
            if (obj.transform.position.y < cameraTransform.position.y - cleanupDistance || !obj.activeInHierarchy)
            {
                // Note: If the player collects it, it might disable itself. 
                // We catch inactive ones here and put them back in the pool.
                collectiblePool.Release(obj);
                activeCollectibles.RemoveAt(i);
            }
        }
    }

    // --- End Game Logic ---

    private void TriggerGameOver()
    {
        isGameOver = true;
        // if (onGameOverEvent != null)
        //     onGameOverEvent.RaiseEvent();
    }

    private void InitializePools()
    {
        platformPools = new Dictionary<int, ObjectPool<GameObject>>();
        for (int i = 0; i < platformPrefabs.Length; i++)
        {
            int index = i; // Capture for closure
            platformPools[i] = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(platformPrefabs[index], transform),
                actionOnGet: (obj) => obj.SetActive(true),
                actionOnRelease: (obj) => obj.SetActive(false),
                actionOnDestroy: (obj) => Destroy(obj),
                defaultCapacity: 15, maxSize: 30);
        }

        enemyPools = new Dictionary<int, ObjectPool<GameObject>>();
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            int index = i; 
            enemyPools[i] = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(enemyPrefabs[index], transform),
                actionOnGet: (obj) => obj.SetActive(true),
                actionOnRelease: (obj) => obj.SetActive(false),
                defaultCapacity: 5, maxSize: 15);
        }

        collectiblePool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(collectiblePrefab, transform),
            actionOnGet: (obj) => obj.SetActive(true),
            actionOnRelease: (obj) => obj.SetActive(false),
            defaultCapacity: 10, maxSize: 20);
    }
}
