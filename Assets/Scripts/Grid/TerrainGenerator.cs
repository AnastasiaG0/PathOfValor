using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TerrainGenerator : MonoBehaviour
{
    /*[Header("Настройки уровня")]
    public LevelSettings currentLevelSettings;
    public LevelSettings[] allLevels;
    public int currentLevel = 1;

    [Header("Префабы")]
    public GameObject playerPrefab;
    public GameObject castlePrefab;
    public GameObject[] enemyPrefabs;

    [Header("Префабы растительности")]
    public GameObject[] treePrefabs;
    public GameObject[] rockPrefabs;
    public GameObject bushPrefab;

    [Header("Настройки отображения")]
    public Material groundMaterial;
    public Material waterMaterial;

    private GameObject currentPlayer;
    private GameObject currentCastle;
    private float[,] heightMap;
    private GameObject terrainObject;

    [System.Serializable]
    public class LevelSettings
    {
        [Header("Основные настройки")]
        public int levelNumber = 1;
        public int seed = 0;
        public string levelName = "Level 1";

        [Header("Размеры карты")]
        public int mapSize = 30;
        public float cellSize = 2f;

        [Header("Настройки холмов")]
        [Range(0, 5)] public float heightMultiplier = 2f;
        [Range(0, 1f)] public float noiseScale = 0.1f;
        public int octaves = 3;
        public float persistence = 0.5f;
        public float lacunarity = 2f;

        [Header("Границы карты")]
        public int borderWidth = 3;
        [Range(0, 1)] public float borderTreeDensity = 0.7f;
        [Range(0, 1)] public float borderRockDensity = 0.3f;

        [Header("Спавн игрока")]
        public PlayerSpawnPosition playerSpawn = PlayerSpawnPosition.BottomLeft;
        public float playerSpawnOffsetFromBorder = 2f;

        [Header("Спавн замка")]
        public CastleSpawnMode castleSpawnMode = CastleSpawnMode.FromPlayer;
        public float castleMinDistanceFromPlayer = 15f;
        public float castleMaxDistanceFromPlayer = 30f;
        public float castleMinDistanceFromCenter = 0f;
        public float castleMaxDistanceFromCenter = 30f;

        [Header("Враги")]
        public EnemySetup[] enemiesSetup;

        [Header("Растительность внутри карты")]
        [Range(0, 1)] public float treeDensity = 0.05f;
        [Range(0, 1)] public float rockDensity = 0.02f;
        [Range(0, 1)] public float bushDensity = 0.03f;

        [Header("Вода")]
        public bool hasWater = true;
        public float waterLevel = 0.5f;
    }

    public enum PlayerSpawnPosition
    {
        BottomLeft, BottomMiddle, BottomRight,
        LeftMiddle, Center, RightMiddle,
        TopLeft, TopMiddle, TopRight,
        Random
    }

    public enum CastleSpawnMode
    {
        FromPlayer,
        FromCenter,
        Mixed
    }

    [System.Serializable]
    public class EnemySetup
    {
        public GameObject enemyPrefab;
        public int count = 1;
        public bool useCustomPower = false;
        public int customPower = 5;
    }

    void Start()
    {
        GenerateCurrentLevel();
    }

    public void GenerateCurrentLevel()
    {
        currentLevelSettings = GetLevelSettings(currentLevel);
        if (currentLevelSettings == null)
        {
            Debug.LogError($"No settings for level {currentLevel}!");
            return;
        }
        GenerateLevel(currentLevelSettings);
    }

    LevelSettings GetLevelSettings(int level)
    {
        foreach (var settings in allLevels)
        {
            if (settings.levelNumber == level)
                return settings;
        }
        return null;
    }

    public void GenerateLevel(LevelSettings settings)
    {
        ClearAllObjects();

        int seed = settings.seed;
        if (seed == 0) seed = Random.Range(1, 999999);
        Random.InitState(seed);
        Debug.Log($"Generating level {settings.levelNumber} with seed {seed}");

        GenerateTerrain(settings);
        SpawnBorderVegetation(settings);
        SpawnInnerVegetation(settings);

        if (settings.hasWater)
            SpawnWater(settings);

        SpawnPlayer(settings);
        SpawnCastle(settings);
        SpawnEnemies(settings);
    }

    void GenerateTerrain(LevelSettings settings)
    {
        int size = settings.mapSize;
        float cellSize = settings.cellSize;
        float halfSize = size / 2f;
        float heightMultiplier = settings.heightMultiplier;

        heightMap = new float[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                float height = GenerateTerrainHeight(x, z, settings);
                heightMap[x, z] = height;
            }
        }

        Mesh terrainMesh = GenerateTerrainMesh(settings);

        terrainObject = new GameObject("Terrain");
        MeshFilter meshFilter = terrainObject.AddComponent<MeshFilter>();
        meshFilter.mesh = terrainMesh;

        MeshCollider meshCollider = terrainObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = terrainMesh;

        Renderer renderer = terrainObject.GetComponent<Renderer>();
        if (renderer == null)
            renderer = terrainObject.AddComponent<MeshRenderer>();

        if (groundMaterial != null)
            renderer.material = groundMaterial;

        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer == -1) groundLayer = 0;
        terrainObject.layer = groundLayer;

        Debug.Log($"Terrain generated: {size * size} vertices, smooth surface");
    }

    Mesh GenerateTerrainMesh(LevelSettings settings)
    {
        int size = settings.mapSize;
        float cellSize = settings.cellSize;
        float halfSize = size / 2f;
        float heightMultiplier = settings.heightMultiplier;

        int vertexCountX = size + 1;
        int vertexCountZ = size + 1;
        int vertexCount = vertexCountX * vertexCountZ;

        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];
        int[] triangles = new int[size * size * 6];

        for (int i = 0; i < vertexCountX; i++)
        {
            for (int j = 0; j < vertexCountZ; j++)
            {
                int x = i;
                int z = j;

                float worldX = (i - halfSize) * cellSize;
                float worldZ = (j - halfSize) * cellSize;
                float height = GetSmoothHeight(x, z, settings) * heightMultiplier;

                int index = i + j * vertexCountX;
                vertices[index] = new Vector3(worldX, height, worldZ);
                uv[index] = new Vector2((float)i / size, (float)j / size);
            }
        }

        int triIndex = 0;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                int bottomLeft = i + j * vertexCountX;
                int bottomRight = (i + 1) + j * vertexCountX;
                int topLeft = i + (j + 1) * vertexCountX;
                int topRight = (i + 1) + (j + 1) * vertexCountX;

                triangles[triIndex] = bottomLeft;
                triangles[triIndex + 1] = topLeft;
                triangles[triIndex + 2] = bottomRight;

                triangles[triIndex + 3] = bottomRight;
                triangles[triIndex + 4] = topLeft;
                triangles[triIndex + 5] = topRight;

                triIndex += 6;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    float GetSmoothHeight(int x, int z, LevelSettings settings)
    {
        int maxIndex = settings.mapSize;

        if (x <= 0 || x >= maxIndex || z <= 0 || z >= maxIndex)
        {
            int clampedX = Mathf.Clamp(x, 0, maxIndex - 1);
            int clampedZ = Mathf.Clamp(z, 0, maxIndex - 1);
            return heightMap[clampedX, clampedZ];
        }

        float h00 = heightMap[Mathf.Clamp(x - 1, 0, maxIndex - 1), Mathf.Clamp(z - 1, 0, maxIndex - 1)];
        float h10 = heightMap[Mathf.Clamp(x, 0, maxIndex - 1), Mathf.Clamp(z - 1, 0, maxIndex - 1)];
        float h01 = heightMap[Mathf.Clamp(x - 1, 0, maxIndex - 1), Mathf.Clamp(z, 0, maxIndex - 1)];
        float h11 = heightMap[Mathf.Clamp(x, 0, maxIndex - 1), Mathf.Clamp(z, 0, maxIndex - 1)];

        float fx = x - Mathf.Floor(x);
        float fz = z - Mathf.Floor(z);

        float h0 = Mathf.Lerp(h00, h10, fx);
        float h1 = Mathf.Lerp(h01, h11, fx);

        return Mathf.Lerp(h0, h1, fz);
    }

    float GenerateTerrainHeight(int x, int z, LevelSettings settings)
    {
        float amplitude = 1f;
        float frequency = settings.noiseScale;
        float height = 0f;
        float maxAmplitude = 0f;

        for (int i = 0; i < settings.octaves; i++)
        {
            float sampleX = (x + Random.Range(0f, 10000f)) * frequency;
            float sampleZ = (z + Random.Range(0f, 10000f)) * frequency;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ);
            height += perlinValue * amplitude;
            maxAmplitude += amplitude;

            amplitude *= settings.persistence;
            frequency *= settings.lacunarity;
        }

        height /= maxAmplitude;

        int size = settings.mapSize;
        float edgeFactor = 1f;
        float edgeDist = Mathf.Min(x, z, size - 1 - x, size - 1 - z);
        if (edgeDist < settings.borderWidth)
        {
            edgeFactor = edgeDist / settings.borderWidth;
        }
        height *= edgeFactor;

        return Mathf.Clamp01(height);
    }

    void SpawnBorderVegetation(LevelSettings settings)
    {
        GameObject borderParent = new GameObject("BorderVegetation");
        int size = settings.mapSize;
        float halfSize = size / 2f;
        float cellSize = settings.cellSize;

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                bool isBorder = x < settings.borderWidth || x >= size - settings.borderWidth ||
                                z < settings.borderWidth || z >= size - settings.borderWidth;

                if (!isBorder) continue;

                float rand = Random.value;

                if (rand < settings.borderTreeDensity && treePrefabs.Length > 0)
                {
                    GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                    SpawnObjectOnTerrain(prefab, x, z, settings, borderParent, Random.Range(0.7f, 1.3f));
                }
                else if (rand < settings.borderTreeDensity + settings.borderRockDensity && rockPrefabs.Length > 0)
                {
                    GameObject prefab = rockPrefabs[Random.Range(0, rockPrefabs.Length)];
                    SpawnObjectOnTerrain(prefab, x, z, settings, borderParent, Random.Range(0.5f, 1.2f));
                }
            }
        }
    }

    void SpawnInnerVegetation(LevelSettings settings)
    {
        GameObject vegetationParent = new GameObject("InnerVegetation");
        int size = settings.mapSize;

        for (int x = settings.borderWidth; x < size - settings.borderWidth; x++)
        {
            for (int z = settings.borderWidth; z < size - settings.borderWidth; z++)
            {
                float rand = Random.value;
                float height = heightMap[x, z];

                if (height > 0.8f) continue;

                if (rand < settings.treeDensity && treePrefabs.Length > 0)
                {
                    GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                    SpawnObjectOnTerrain(prefab, x, z, settings, vegetationParent, Random.Range(0.8f, 1.3f));
                }
                else if (rand < settings.treeDensity + settings.rockDensity && rockPrefabs.Length > 0)
                {
                    GameObject prefab = rockPrefabs[Random.Range(0, rockPrefabs.Length)];
                    SpawnObjectOnTerrain(prefab, x, z, settings, vegetationParent, Random.Range(0.6f, 1.1f));
                }
                else if (rand < settings.treeDensity + settings.rockDensity + settings.bushDensity && bushPrefab != null)
                {
                    SpawnObjectOnTerrain(bushPrefab, x, z, settings, vegetationParent, Random.Range(0.7f, 1.2f));
                }
            }
        }
    }

    void SpawnObjectOnTerrain(GameObject prefab, int x, int z, LevelSettings settings, GameObject parent, float scale)
    {
        float halfSize = settings.mapSize / 2f;
        float cellSize = settings.cellSize;
        float height = heightMap[x, z] * settings.heightMultiplier;
        float objHeight = GetObjectHeight(prefab);

        float worldX = (x - halfSize) * cellSize;
        float worldZ = (z - halfSize) * cellSize;
        float worldY = height + objHeight / 2f;

        GameObject obj = Instantiate(prefab, new Vector3(worldX, worldY, worldZ), Quaternion.Euler(0, Random.Range(0, 360), 0));
        obj.transform.localScale = Vector3.one * scale;
        obj.transform.parent = parent.transform;

        if (obj.GetComponent<Collider>() == null && (prefab == treePrefabs.FirstOrDefault() || rockPrefabs.Contains(prefab)))
        {
            BoxCollider col = obj.AddComponent<BoxCollider>();
            col.size = new Vector3(1f, objHeight, 1f);
            obj.layer = LayerMask.NameToLayer("Obstacle");
        }
    }

    void SpawnWater(LevelSettings settings)
    {
        if (waterPlanePrefab == null) return;

        float waterY = settings.waterLevel * settings.heightMultiplier;
        GameObject water = Instantiate(waterPlanePrefab, new Vector3(0, waterY, 0), Quaternion.identity);
        water.transform.localScale = new Vector3(settings.mapSize * settings.cellSize / 10f, 1, settings.mapSize * settings.cellSize / 10f);
        water.tag = "Water";

        if (waterMaterial != null)
        {
            Renderer r = water.GetComponent<Renderer>();
            if (r != null) r.material = waterMaterial;
        }
    }

    void SpawnPlayer(LevelSettings settings)
    {
        Vector2 pos = GetPlayerSpawnPosition(settings);
        float groundHeight = GetGroundHeightAtPosition(pos.x, pos.y, settings);

        if (playerPrefab != null && currentPlayer == null)
        {
            float playerHeight = GetObjectHeight(playerPrefab);
            Vector3 playerPos = new Vector3(pos.x, groundHeight + playerHeight / 2f, pos.y);
            currentPlayer = Instantiate(playerPrefab, playerPos, Quaternion.identity);
            Debug.Log($"Player spawned at {settings.playerSpawn}: {playerPos}");
        }
    }

    Vector2 GetPlayerSpawnPosition(LevelSettings settings)
    {
        float halfSize = settings.mapSize / 2f;
        float cellSize = settings.cellSize;
        float offset = settings.playerSpawnOffsetFromBorder;

        float minX = -halfSize * cellSize + offset;
        float maxX = halfSize * cellSize - offset;
        float minZ = -halfSize * cellSize + offset;
        float maxZ = halfSize * cellSize - offset;

        switch (settings.playerSpawn)
        {
            case PlayerSpawnPosition.BottomLeft: return new Vector2(minX, minZ);
            case PlayerSpawnPosition.BottomMiddle: return new Vector2(0, minZ);
            case PlayerSpawnPosition.BottomRight: return new Vector2(maxX, minZ);
            case PlayerSpawnPosition.LeftMiddle: return new Vector2(minX, 0);
            case PlayerSpawnPosition.Center: return new Vector2(0, 0);
            case PlayerSpawnPosition.RightMiddle: return new Vector2(maxX, 0);
            case PlayerSpawnPosition.TopLeft: return new Vector2(minX, maxZ);
            case PlayerSpawnPosition.TopMiddle: return new Vector2(0, maxZ);
            case PlayerSpawnPosition.TopRight: return new Vector2(maxX, maxZ);
            case PlayerSpawnPosition.Random:
                return new Vector2(Random.Range(minX, maxX), Random.Range(minZ, maxZ));
        }
        return Vector2.zero;
    }

    void SpawnCastle(LevelSettings settings)
    {
        if (castlePrefab == null) return;

        Vector2? castlePos = FindCastlePosition(settings);
        if (castlePos == null)
        {
            Debug.LogWarning("Could not find suitable castle position!");
            return;
        }

        float groundHeight = GetGroundHeightAtPosition(castlePos.Value.x, castlePos.Value.y, settings);
        float castleHeight = GetObjectHeight(castlePrefab);
        Vector3 worldPos = new Vector3(castlePos.Value.x, groundHeight + castleHeight / 2f, castlePos.Value.y);

        if (currentCastle != null) Destroy(currentCastle);
        currentCastle = Instantiate(castlePrefab, worldPos, Quaternion.identity);
        currentCastle.tag = "Castle";
        currentCastle.layer = LayerMask.NameToLayer("Castle");
        Debug.Log($"Castle spawned at {worldPos}");
    }

    Vector2? FindCastlePosition(LevelSettings settings)
    {
        List<Vector2> candidates = new List<Vector2>();
        float halfSize = settings.mapSize / 2f;
        float cellSize = settings.cellSize;

        Vector2 playerPos = currentPlayer != null ? new Vector2(currentPlayer.transform.position.x, currentPlayer.transform.position.z) : Vector2.zero;

        for (int x = settings.borderWidth; x < settings.mapSize - settings.borderWidth; x++)
        {
            for (int z = settings.borderWidth; z < settings.mapSize - settings.borderWidth; z++)
            {
                float worldX = (x - halfSize) * cellSize;
                float worldZ = (z - halfSize) * cellSize;
                float height = heightMap[x, z];

                if (height > 0.7f) continue;

                float distFromPlayer = Vector2.Distance(new Vector2(worldX, worldZ), playerPos);
                float distFromCenter = Vector2.Distance(new Vector2(worldX, worldZ), Vector2.zero);

                bool valid = false;
                switch (settings.castleSpawnMode)
                {
                    case CastleSpawnMode.FromPlayer:
                        valid = distFromPlayer >= settings.castleMinDistanceFromPlayer &&
                                distFromPlayer <= settings.castleMaxDistanceFromPlayer;
                        break;
                    case CastleSpawnMode.FromCenter:
                        valid = distFromCenter >= settings.castleMinDistanceFromCenter &&
                                distFromCenter <= settings.castleMaxDistanceFromCenter;
                        break;
                    case CastleSpawnMode.Mixed:
                        valid = distFromPlayer >= settings.castleMinDistanceFromPlayer &&
                                distFromPlayer <= settings.castleMaxDistanceFromPlayer &&
                                distFromCenter >= settings.castleMinDistanceFromCenter &&
                                distFromCenter <= settings.castleMaxDistanceFromCenter;
                        break;
                }

                if (valid)
                    candidates.Add(new Vector2(worldX, worldZ));
            }
        }

        if (candidates.Count > 0)
            return candidates[Random.Range(0, candidates.Count)];

        return null;
    }

    void SpawnEnemies(LevelSettings settings)
    {
        if (settings.enemiesSetup == null || settings.enemiesSetup.Length == 0)
        {
            Debug.Log("No enemies configured for this level");
            return;
        }

        List<GameObject> enemyQueue = new List<GameObject>();
        foreach (EnemySetup setup in settings.enemiesSetup)
        {
            if (setup.enemyPrefab == null) continue;
            for (int i = 0; i < setup.count; i++)
                enemyQueue.Add(setup.enemyPrefab);
        }

        List<Vector2> spawnPoints = FindEnemySpawnPoints(enemyQueue.Count, settings);

        for (int i = 0; i < spawnPoints.Count && i < enemyQueue.Count; i++)
        {
            GameObject prefab = enemyQueue[i];
            float groundHeight = GetGroundHeightAtPosition(spawnPoints[i].x, spawnPoints[i].y, settings);
            float enemyHeight = GetObjectHeight(prefab);
            Vector3 pos = new Vector3(spawnPoints[i].x, groundHeight + enemyHeight / 2f, spawnPoints[i].y);

            GameObject enemyObj = Instantiate(prefab, pos, Quaternion.identity);
            enemyObj.layer = LayerMask.NameToLayer("Enemy");

            Enemy enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null)
            {
                EnemySetup matchingSetup = settings.enemiesSetup.FirstOrDefault(s => s.enemyPrefab == prefab);
                if (matchingSetup != null && matchingSetup.useCustomPower)
                {
                    enemy.power = matchingSetup.customPower;
                    enemy.UpdatePowerDisplay();
                }
            }
        }

        Debug.Log($"Spawned {Mathf.Min(spawnPoints.Count, enemyQueue.Count)} enemies");
    }

    List<Vector2> FindEnemySpawnPoints(int count, LevelSettings settings)
    {
        List<Vector2> allPoints = new List<Vector2>();
        float halfSize = settings.mapSize / 2f;
        float cellSize = settings.cellSize;

        Vector2 playerPos = currentPlayer != null ? new Vector2(currentPlayer.transform.position.x, currentPlayer.transform.position.z) : Vector2.zero;

        for (int x = settings.borderWidth; x < settings.mapSize - settings.borderWidth; x++)
        {
            for (int z = settings.borderWidth; z < settings.mapSize - settings.borderWidth; z++)
            {
                float worldX = (x - halfSize) * cellSize;
                float worldZ = (z - halfSize) * cellSize;
                float height = heightMap[x, z];

                if (height < 0.2f || height > 0.7f) continue;

                float distFromPlayer = Vector2.Distance(new Vector2(worldX, worldZ), playerPos);
                if (distFromPlayer > 10f && distFromPlayer < 40f)
                {
                    allPoints.Add(new Vector2(worldX, worldZ));
                }
            }
        }

        for (int i = 0; i < allPoints.Count && i < count; i++)
        {
            int randomIndex = Random.Range(i, allPoints.Count);
            Vector2 temp = allPoints[i];
            allPoints[i] = allPoints[randomIndex];
            allPoints[randomIndex] = temp;
        }

        return allPoints.Take(count).ToList();
    }

    float GetGroundHeightAtPosition(float worldX, float worldZ, LevelSettings settings)
    {
        float halfSize = settings.mapSize / 2f;
        float cellSize = settings.cellSize;

        int x = Mathf.RoundToInt((worldX / cellSize) + halfSize);
        int z = Mathf.RoundToInt((worldZ / cellSize) + halfSize);

        x = Mathf.Clamp(x, 0, settings.mapSize - 1);
        z = Mathf.Clamp(z, 0, settings.mapSize - 1);

        return heightMap[x, z] * settings.heightMultiplier;
    }

    float GetObjectHeight(GameObject obj)
    {
        Renderer r = obj.GetComponentInChildren<Renderer>();
        if (r != null) return r.bounds.size.y;

        Collider c = obj.GetComponentInChildren<Collider>();
        if (c != null) return c.bounds.size.y;

        return 1f;
    }

    void ClearAllObjects()
    {
        if (terrainObject != null) DestroyImmediate(terrainObject);

        GameObject obj = GameObject.Find("BorderVegetation");
        if (obj != null) DestroyImmediate(obj);
        obj = GameObject.Find("InnerVegetation");
        if (obj != null) DestroyImmediate(obj);

        obj = GameObject.FindGameObjectWithTag("Water");
        if (obj != null) DestroyImmediate(obj);

        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy e in enemies) DestroyImmediate(e.gameObject);

        if (currentCastle != null) DestroyImmediate(currentCastle);
        if (currentPlayer != null) DestroyImmediate(currentPlayer);

        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (GameObject o in obstacles) DestroyImmediate(o);
    }

    public void NextLevel()
    {
        currentLevel++;
        LevelSettings nextSettings = GetLevelSettings(currentLevel);
        if (nextSettings == null)
        {
            Debug.Log("Congratulations! You completed all levels!");
            return;
        }
        GenerateLevel(nextSettings);
    }

    [ContextMenu("Load Next Level")]
    public void LoadNextLevelTest() => NextLevel();

    public GameObject waterPlanePrefab;*/
}