using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Pool;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Architecture")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject playerFollowerPrefab;
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
    public float highestGeneratedY = 0f;
    private bool isGameOver = false;
    private float currentScore = 0f;
    private float originPointY = 0f; // The Y position where the player starts, used to calculate score as height climbed
    private Transform playerTransform;
    private CinemachineCamera currentPlayerFollower;

    // Standard Lists to track active objects in the scene
    private List<GameObject> activePlatforms = new List<GameObject>();
    private List<GameObject> activeEnemies = new List<GameObject>();
    private List<GameObject> activeCollectibles = new List<GameObject>();

    // Dictionaries to hold Object Pools for different prefab types
    private Dictionary<int, ObjectPool<GameObject>> platformPools;
    private Dictionary<int, ObjectPool<GameObject>> enemyPools;
    private ObjectPool<GameObject> collectiblePool;

    public void Initialize()
    {
        Time.timeScale = 0f;
        DestroyAllActiveObjects();
        isGameOver = false;
        currentScore = 0f;
        originPointY = freedomPoint.position.y + 4f;

        if(playerTransform == null)
        {
            playerTransform = Instantiate(playerPrefab, freedomPoint.position + Vector3.up * 5f, Quaternion.identity).transform;
            playerTransform.GetComponent<PlayerController>().SetBoundaries(leftBoundry, rightBoundry, freedomPoint);
            Instantiate(platformPools[0].Get(), freedomPoint.position + Vector3.up * 2f, Quaternion.identity, transform); // Spawn an initial platform for the player to stand on
            cameraTransform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, cameraTransform.position.z);
            currentPlayerFollower = Instantiate(playerFollowerPrefab, playerTransform.position, Quaternion.identity).GetComponent<CinemachineCamera>();
            currentPlayerFollower.Follow = playerTransform;



        }
        highestGeneratedY = originPointY;

        Time.timeScale = 1f;
    }

     private void Awake() {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        InitializePools();
        InitializeLevel();
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

        if (playerTransform == null) return; // Player might not be initialized yet

        currentScore = Mathf.Max(currentScore, playerTransform.position.y - freedomPoint.position.y);

        // 3. Check for Game Over (Player falls below the death zone)
        if (playerTransform.position.y < freedomPoint.position.y)
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
        DestroyPlayer();
        DestroyAllActiveObjects();
        GameManager.Instance.ChangeState(GameState.GameOver);
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

    public float GetCurrentScore()
    {
        return currentScore;
    } 

    public void DestroyAllActiveObjects()
    {
        DestroyObjectsOfType(activePlatforms, platformPools);
        DestroyObjectsOfType(activeEnemies, enemyPools);
        RecycleLevelCollectibles(); // This will handle all active collectibles
    }

    private void DestroyObjectsOfType(List<GameObject> activeList, Dictionary<int, ObjectPool<GameObject>> pools)
    {
        for (int i = activeList.Count - 1; i >= 0; i--)
        {
            GameObject obj = activeList[i];
            int typeIndex = obj.GetComponent<LevelEntity>().EntityTypeIndex;
            pools[typeIndex].Release(obj);
            activeList.RemoveAt(i);       
        }
    }

    public void DestroyPlayer()
    {
        isGameOver = true;
        if(playerTransform != null)
        {
            Destroy(playerTransform.gameObject);
            playerTransform = null;
        }
        if(currentPlayerFollower != null)
        {
                Destroy(currentPlayerFollower.gameObject);
        }
    }
}
