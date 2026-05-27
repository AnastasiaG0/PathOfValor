using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class NatureMapGenerator : MonoBehaviour
{
    /*[Header("Префабы")]
    public GameObject playerPrefab;
    public GameObject castlePrefab;
    public GameObject[] enemyPrefabs;

    [Header("Ландшафт")]
    public GameObject terrainPrefab;
    public GameObject waterPlanePrefab;
    public GameObject treePrefab;
    public GameObject rockPrefab;
    public GameObject bushPrefab;
    public GameObject grassPrefab;
    public GameObject flowerPrefab;

    [Header("Префабы земли (для разных биомов)")]
    public GameObject[] groundPrefabs;        // Обычная земля (для Plains)
    public GameObject[] grassGroundPrefabs;   // Трава (для Forest)
    public GameObject[] sandGroundPrefabs;    // Песок (для Beach)
    public GameObject[] winterGroundPrefabs;  // Снег/лёд (для Winter) - НОВОЕ
    public GameObject[] mountainGroundPrefabs;// Камень (для Mountains)

    [Header("Размеры карты (по умолчанию)")]
    public int defaultMapSize = 30;
    public float defaultCellSize = 2f;
    public float defaultTerrainHeight = 5f;

    [Header("Границы карты - Лес и валуны")]
    public int borderWidth = 3;
    public float borderObstacleDensity = 0.85f;

    public bool usePrefabsForBorder = false;
    public GameObject[] borderTreePrefabs;
    public GameObject[] borderRockPrefabs;

    public Color[] treeColors = new Color[]
    {
        new Color(0.2f, 0.6f, 0.2f),
        new Color(0.3f, 0.7f, 0.3f),
        new Color(0.15f, 0.5f, 0.15f),
        new Color(0.4f, 0.8f, 0.4f)
    };

    public Color[] rockColors = new Color[]
    {
        new Color(0.5f, 0.5f, 0.5f),
        new Color(0.4f, 0.4f, 0.45f),
        new Color(0.6f, 0.55f, 0.5f)
    };

    [Header("Шум Перлина")]
    public float noiseScale = 0.1f;
    public float heightMultiplier = 3f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;

    [Header("Биомы")]
    public AnimationCurve heightToBiome = AnimationCurve.Linear(0, 0, 1, 3);
    public float forestThreshold = 0.3f;
    public float mountainThreshold = 0.7f;

    [Header("Плотность объектов")]
    public float treeDensity = 0.05f;
    public float rockDensity = 0.02f;
    public float bushDensity = 0.08f;
    public float grassDensity = 0.15f;
    public float flowerDensity = 0.03f;

    [Header("Настройки уровней")]
    public int currentLevel = 1;
    public LevelData[] levels;

    [Header("Враги")]
    public int enemiesCount = 12;
    public bool autoGenerateBalancedPowers = true;

    [Header("Материалы")]
    public Material terrainMaterial;
    public Material waterMaterial;

    public enum TreeType { Pine, Oak, Round, Tall }
    public enum RockType { Round, Sharp, Flat, Huge }
    public enum BiomeType { Beach, Plains, Forest, Winter, Mountains }

    private const float TILE_HEIGHT = 0.2f;
    private float[,] heightMap;
    private List<Vector3> enemySpawnPoints = new List<Vector3>();
    private Vector3 castlePosition;
    private Vector3 playerStartPosition;
    private int obstacleLayer;
    private GameObject currentCastle;
    private GameObject currentPlayer;

    // Текущие размеры карты (могут меняться для каждого уровня)
    private int currentMapSize;
    private float currentCellSize;
    private float currentTerrainHeight;

    [System.Serializable]
    public class LevelData
    {
        public int levelNumber;
        public int seed;

        // ===== РАЗМЕРЫ КАРТЫ ДЛЯ ЭТОГО УРОВНЯ =====
        public bool useCustomMapSize = false;      // Использовать ли свой размер карты
        public int mapSize = 30;                   // Размер карты для этого уровня
        public float cellSize = 2f;                // Размер ячейки для этого уровня
        public float terrainHeight = 5f;           // Высота terrain для этого уровня

        // ===== ВЫБОР РЕЖИМА СПАВНА ЗАМКА =====
        public enum CastleSpawnMode
        {
            FromCenter,      // Замок на расстоянии от ЦЕНТРА карты
            FromPlayer,      // Замок на расстоянии от ИГРОКА
            Mixed            // И то, и другое (должно соблюдаться оба условия)
        }
        public CastleSpawnMode castleSpawnMode = CastleSpawnMode.FromPlayer;

        // ===== ПАРАМЕТРЫ ДЛЯ РАЗНЫХ РЕЖИМОВ =====
        // Для режима FromCenter
        public float castleMinDistanceFromCenter = 0f;
        public float castleMaxDistanceFromCenter = 30f;

        // Для режима FromPlayer
        public float castleMinDistanceFromPlayer = 15f;
        public float castleMaxDistanceFromPlayer = 30f;

        // Общие параметры
        public BiomeType[] allowedBiomes;
        public int enemiesCount;
        public int playerStartPower = 5;
        public string levelName;
        public Color terrainTint = Color.white;

        // Параметры спавна игрока
        public enum PlayerSpawnCorner { TopLeft, TopRight, BottomLeft, BottomRight, Center, Random, TopMiddle, BottomMiddle, LeftMiddle, RightMiddle }
        public PlayerSpawnCorner playerSpawnCorner = PlayerSpawnCorner.BottomLeft;
    }

    void Start()
    {
        GenerateLevel(currentLevel);
    }

    public void GenerateLevel(int level)
    {
        LevelData levelData = GetLevelData(level);
        if (levelData == null)
        {
            Debug.LogError($"No level data found for level {level}!");
            return;
        }

        Random.InitState(levelData.seed);
        enemiesCount = levelData.enemiesCount;

        // Устанавливаем размеры карты для этого уровня
        if (levelData.useCustomMapSize)
        {
            currentMapSize = levelData.mapSize;
            currentCellSize = levelData.cellSize;
            currentTerrainHeight = levelData.terrainHeight;
        }
        else
        {
            currentMapSize = defaultMapSize;
            currentCellSize = defaultCellSize;
            currentTerrainHeight = defaultTerrainHeight;
        }

        Debug.Log($"Level {level}: Map size = {currentMapSize}x{currentMapSize} ({currentMapSize * currentCellSize}x{currentMapSize * currentCellSize} meters)");

        CreateObstacleLayer();

        GenerateTerrain(levelData.terrainTint);
        GenerateVegetation();
        GenerateBorderObstacles();
        SpawnWater();

        // Сначала спавним игрока, потом замок
        SpawnPlayerAtCorner(levelData);
        SpawnCastle(levelData);

        SpawnEnemiesOnTerrain();
        SetupEnvironment();

        Debug.Log($"Level {level} generated with seed {levelData.seed}");
    }

    LevelData GetLevelData(int level)
    {
        foreach (var levelData in levels)
        {
            if (levelData.levelNumber == level)
                return levelData;
        }
        return null;
    }

    void SpawnPlayerAtCorner(LevelData levelData)
    {
        int halfSize = currentMapSize / 2;
        float x = 0;
        float z = 0;

        switch (levelData.playerSpawnCorner)
        {
            case LevelData.PlayerSpawnCorner.TopLeft:
                x = -halfSize * currentCellSize + borderWidth * currentCellSize;
                z = halfSize * currentCellSize - borderWidth * currentCellSize;
                break;
            case LevelData.PlayerSpawnCorner.TopRight:
                x = halfSize * currentCellSize - borderWidth * currentCellSize;
                z = halfSize * currentCellSize - borderWidth * currentCellSize;
                break;
            case LevelData.PlayerSpawnCorner.BottomLeft:
                x = -halfSize * currentCellSize + borderWidth * currentCellSize;
                z = -halfSize * currentCellSize + borderWidth * currentCellSize;
                break;
            case LevelData.PlayerSpawnCorner.BottomRight:
                x = halfSize * currentCellSize - borderWidth * currentCellSize;
                z = -halfSize * currentCellSize + borderWidth * currentCellSize;
                break;
            case LevelData.PlayerSpawnCorner.Center:
                x = 0;
                z = 0;
                break;
            case LevelData.PlayerSpawnCorner.TopMiddle:
                x = 0;
                z = halfSize * currentCellSize - borderWidth * currentCellSize;
                break;
            case LevelData.PlayerSpawnCorner.BottomMiddle:
                x = 0;
                z = -halfSize * currentCellSize + borderWidth * currentCellSize;
                break;
            case LevelData.PlayerSpawnCorner.LeftMiddle:
                x = -halfSize * currentCellSize + borderWidth * currentCellSize;
                z = 0;
                break;
            case LevelData.PlayerSpawnCorner.RightMiddle:
                x = halfSize * currentCellSize - borderWidth * currentCellSize;
                z = 0;
                break;
            case LevelData.PlayerSpawnCorner.Random:
                int randomChoice = Random.Range(0, 9);
                if (randomChoice == 0) { x = -halfSize * currentCellSize + borderWidth * currentCellSize; z = halfSize * currentCellSize - borderWidth * currentCellSize; }
                else if (randomChoice == 1) { x = halfSize * currentCellSize - borderWidth * currentCellSize; z = halfSize * currentCellSize - borderWidth * currentCellSize; }
                else if (randomChoice == 2) { x = -halfSize * currentCellSize + borderWidth * currentCellSize; z = -halfSize * currentCellSize + borderWidth * currentCellSize; }
                else if (randomChoice == 3) { x = halfSize * currentCellSize - borderWidth * currentCellSize; z = -halfSize * currentCellSize + borderWidth * currentCellSize; }
                else if (randomChoice == 4) { x = 0; z = halfSize * currentCellSize - borderWidth * currentCellSize; }
                else if (randomChoice == 5) { x = 0; z = -halfSize * currentCellSize + borderWidth * currentCellSize; }
                else if (randomChoice == 6) { x = -halfSize * currentCellSize + borderWidth * currentCellSize; z = 0; }
                else if (randomChoice == 7) { x = halfSize * currentCellSize - borderWidth * currentCellSize; z = 0; }
                else { x = 0; z = 0; }
                break;
        }

        float groundHeight = GetGroundHeightAtPosition(x, z);
        playerStartPosition = new Vector3(x, 0, z);

        if (playerPrefab != null && currentPlayer == null)
        {
            Vector3 playerPos = new Vector3(x, groundHeight, z);
            currentPlayer = Instantiate(playerPrefab, playerPos, Quaternion.identity);
            PlaceOnGround(currentPlayer, groundHeight); // Добавьте эту строку
            Debug.Log($"Player spawned at {levelData.playerSpawnCorner}: {currentPlayer.transform.position}");
        }
    }

    void SpawnCastle(LevelData levelData)
    {
        List<Vector3> candidates = new List<Vector3>();
        int halfSize = currentMapSize / 2;

        for (int x = borderWidth; x < currentMapSize - borderWidth; x++)
        {
            for (int z = borderWidth; z < currentMapSize - borderWidth; z++)
            {
                float worldX = (x - halfSize) * currentCellSize;
                float worldZ = (z - halfSize) * currentCellSize;
                Vector3 position = new Vector3(worldX, 0, worldZ);

                bool isSuitable = true;

                switch (levelData.castleSpawnMode)
                {
                    case LevelData.CastleSpawnMode.FromCenter:
                        float distanceFromCenter = Vector2.Distance(new Vector2(worldX, worldZ), Vector2.zero);
                        if (distanceFromCenter < levelData.castleMinDistanceFromCenter ||
                            distanceFromCenter > levelData.castleMaxDistanceFromCenter)
                        {
                            isSuitable = false;
                        }
                        break;

                    case LevelData.CastleSpawnMode.FromPlayer:
                        float distanceFromPlayer = Vector3.Distance(position, playerStartPosition);
                        if (distanceFromPlayer < levelData.castleMinDistanceFromPlayer ||
                            distanceFromPlayer > levelData.castleMaxDistanceFromPlayer)
                        {
                            isSuitable = false;
                        }
                        break;

                    case LevelData.CastleSpawnMode.Mixed:
                        float distFromCenter = Vector2.Distance(new Vector2(worldX, worldZ), Vector2.zero);
                        float distFromPlayer = Vector3.Distance(position, playerStartPosition);

                        if (distFromCenter < levelData.castleMinDistanceFromCenter ||
                            distFromCenter > levelData.castleMaxDistanceFromCenter ||
                            distFromPlayer < levelData.castleMinDistanceFromPlayer ||
                            distFromPlayer > levelData.castleMaxDistanceFromPlayer)
                        {
                            isSuitable = false;
                        }
                        break;
                }

                if (isSuitable)
                {
                    float height = heightMap[x, z];
                    string biome = GetBiomeAtHeight(height);

                    if (System.Array.Exists(levelData.allowedBiomes, b => b.ToString() == biome))
                    {
                        candidates.Add(position);
                    }
                }
            }
        }

        if (candidates.Count > 0)
        {
            castlePosition = candidates[Random.Range(0, candidates.Count)];
        }
        else
        {
            Debug.LogWarning($"Could not find suitable castle position for level {currentLevel}, using fallback");
            string[] biomeStrings = levelData.allowedBiomes.Select(b => b.ToString()).ToArray();
            castlePosition = FindSuitableSpawnPoint(5, currentMapSize * currentCellSize / 2, biomeStrings);
        }

        if (castlePrefab != null)
        {
            if (currentCastle != null)
                Destroy(currentCastle);

            float groundHeight = GetGroundHeightAtPosition(castlePosition.x, castlePosition.z);

            // ===== ПРАВИЛЬНЫЙ СПАВН ЗАМКА С КОРРЕКЦИЕЙ ПО ЗЕМЛЕ =====
            GameObject castle = Instantiate(castlePrefab, castlePosition, Quaternion.identity);

            // Корректируем позицию замка на земле
            PlaceOnGround(castle, groundHeight);

            currentCastle = castle;
            currentCastle.tag = "Castle";
            currentCastle.layer = LayerMask.NameToLayer("Castle");

            // Добавляем коллайдер, если его нет
            if (currentCastle.GetComponent<Collider>() == null)
            {
                BoxCollider col = currentCastle.AddComponent<BoxCollider>();
                Renderer rend = currentCastle.GetComponentInChildren<Renderer>();
                if (rend != null)
                    col.size = rend.bounds.size;
            }

            // Выводим информацию о позиции
            float finalY = currentCastle.transform.position.y;
            float castleBottomY = GetBottomY(currentCastle);
            Debug.Log($"Castle spawned: ground={groundHeight}, castle Y={finalY}, bottom Y={castleBottomY}");

            if (levelData.castleSpawnMode == LevelData.CastleSpawnMode.FromCenter)
            {
                float dist = Vector2.Distance(new Vector2(castlePosition.x, castlePosition.z), Vector2.zero);
                Debug.Log($"Castle spawned at distance {dist:F1} from center");
            }
            else if (levelData.castleSpawnMode == LevelData.CastleSpawnMode.FromPlayer)
            {
                float dist = Vector3.Distance(playerStartPosition, castlePosition);
                Debug.Log($"Castle spawned at distance {dist:F1} from player");
            }
            else if (levelData.castleSpawnMode == LevelData.CastleSpawnMode.Mixed)
            {
                float distCenter = Vector2.Distance(new Vector2(castlePosition.x, castlePosition.z), Vector2.zero);
                float distPlayer = Vector3.Distance(playerStartPosition, castlePosition);
                Debug.Log($"Castle spawned: {distCenter:F1} from center, {distPlayer:F1} from player");
            }
        }
    }

    void CreateObstacleLayer()
    {
        obstacleLayer = LayerMask.NameToLayer("Obstacle");
        if (obstacleLayer == -1)
        {
            Debug.LogWarning("Layer 'Obstacle' not found! Using Default layer.");
            obstacleLayer = 0;
        }
    }

    float GetYPosition(float height, float offset = 0f)
    {
        float tileCenterOffset = TILE_HEIGHT / 2f;
        return height * currentTerrainHeight + tileCenterOffset + offset;
    }

    void GenerateTerrain(Color tint)
    {
        heightMap = new float[currentMapSize, currentMapSize];
        GameObject terrainParent = new GameObject("Terrain");

        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer == -1) groundLayer = 0;

        for (int x = 0; x < currentMapSize; x++)
        {
            for (int z = 0; z < currentMapSize; z++)
            {
                float height = GenerateHeight(x, z);
                heightMap[x, z] = height;

                float yPos = height * currentTerrainHeight;
                Vector3 position = new Vector3((x - currentMapSize / 2f) * currentCellSize, yPos, (z - currentMapSize / 2f) * currentCellSize);

                // ===== ПОЛУЧАЕМ БИОМ И ПРЕФАБ ЗЕМЛИ =====
                string biome = GetBiomeAtHeight(height);
                GameObject selectedPrefab = GetGroundPrefabByBiome(biome);

                GameObject tile;

                if (selectedPrefab != null)
                {
                    // Используем ваш префаб земли
                    tile = Instantiate(selectedPrefab, position, Quaternion.identity);

                    // Правильно размещаем префаб на земле
                    float prefabHeight = GetPrefabHeight(selectedPrefab);
                    tile.transform.position = new Vector3(position.x, position.y - prefabHeight / 2f, position.z);
                }
                else
                {
                    // Запасной вариант - куб (если префаб не найден)
                    tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tile.transform.localScale = new Vector3(currentCellSize, TILE_HEIGHT, currentCellSize);
                }

                tile.transform.parent = terrainParent.transform;
                tile.layer = groundLayer;
            }
        }

        // ОПЦИОНАЛЬНО: объединить все тайлы для оптимизации
        // CombineTerrainMeshes(terrainParent);

        Debug.Log($"Terrain generated: {currentMapSize * currentMapSize} tiles using biome prefabs");
    }

    // Добавьте этот вспомогательный метод:
    float GetPrefabHeight(GameObject prefab)
    {
        MeshFilter meshFilter = prefab.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            return meshFilter.sharedMesh.bounds.size.y;
        }

        Renderer renderer = prefab.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.size.y;
        }

        return TILE_HEIGHT;
    }

    float GenerateHeight(int x, int z)
    {
        float amplitude = 1f;
        float frequency = noiseScale;
        float height = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (x + Random.seed) * frequency;
            float sampleZ = (z + Random.seed) * frequency;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ);
            height += perlinValue * amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        height /= (1f - Mathf.Pow(persistence, octaves)) / (1f - persistence);

        float distanceToCenter = Vector2.Distance(new Vector2(x, z), new Vector2(currentMapSize / 2f, currentMapSize / 2f));
        float edgeFactor = Mathf.Clamp01(distanceToCenter / (currentMapSize / 2f * 0.8f));
        height -= edgeFactor * 0.3f;

        return Mathf.Clamp01(height);
    }

    Color GetBiomeColor(float height)
    {
        if (height < 0.2f) return new Color(0.96f, 0.87f, 0.7f);  // Песок
        if (height < 0.4f) return new Color(0.55f, 0.77f, 0.35f); // Зелёная трава
        if (height < 0.6f) return new Color(0.3f, 0.6f, 0.2f);    // Тёмно-зелёный лес
        if (height < 0.8f) return new Color(0.9f, 0.9f, 1f);      // Снежно-белый для зимы
        return new Color(0.95f, 0.95f, 0.95f);                    // Белые горы
    }

    GameObject GetGroundPrefabByBiome(string biome)
    {
        switch (biome)
        {
            case "Beach":
                if (sandGroundPrefabs != null && sandGroundPrefabs.Length > 0)
                    return sandGroundPrefabs[Random.Range(0, sandGroundPrefabs.Length)];
                break;
            case "Plains":
                if (groundPrefabs != null && groundPrefabs.Length > 0)
                    return groundPrefabs[Random.Range(0, groundPrefabs.Length)];
                break;
            case "Forest":
                if (grassGroundPrefabs != null && grassGroundPrefabs.Length > 0)
                    return grassGroundPrefabs[Random.Range(0, grassGroundPrefabs.Length)];
                break;
            case "Winter":  // Изменено с "Hills"
                if (winterGroundPrefabs != null && winterGroundPrefabs.Length > 0)
                    return winterGroundPrefabs[Random.Range(0, winterGroundPrefabs.Length)];
                break;
            case "Mountains":
                if (mountainGroundPrefabs != null && mountainGroundPrefabs.Length > 0)
                    return mountainGroundPrefabs[Random.Range(0, mountainGroundPrefabs.Length)];
                break;
        }

        if (groundPrefabs != null && groundPrefabs.Length > 0)
            return groundPrefabs[Random.Range(0, groundPrefabs.Length)];

        return null;
    }

    void GenerateBorderObstacles()
    {
        GameObject borderParent = new GameObject("BorderForest");
        int halfSize = currentMapSize / 2;

        for (int x = 0; x < currentMapSize; x++)
        {
            for (int z = 0; z < currentMapSize; z++)
            {
                bool isBorder = x < borderWidth || x >= currentMapSize - borderWidth ||
                                z < borderWidth || z >= currentMapSize - borderWidth;

                if (isBorder && Random.value < borderObstacleDensity)
                {
                    float height = heightMap[x, z];
                    float yPos = GetYPosition(height, 0f);
                    Vector3 position = new Vector3((x - halfSize) * currentCellSize, yPos, (z - halfSize) * currentCellSize);

                    bool isTree = Random.value < 0.7f;
                    float scale = isTree ? Random.Range(0.8f, 1.5f) : Random.Range(0.6f, 1.3f);

                    if (isTree)
                        CreateTreeAtPosition(position, scale, borderParent);
                    else
                        CreateRockAtPosition(position, scale, borderParent);
                }
            }
        }
    }

    GameObject CreateTreeAtPosition(Vector3 position, float scale, GameObject parent)
    {
        GameObject tree;

        if (usePrefabsForBorder && borderTreePrefabs != null && borderTreePrefabs.Length > 0)
        {
            GameObject prefab = borderTreePrefabs[Random.Range(0, borderTreePrefabs.Length)];
            tree = Instantiate(prefab, position, Quaternion.Euler(0, Random.Range(0, 360), 0));
            tree.transform.localScale = Vector3.one * scale;
        }
        else
        {
            tree = CreateProceduralTree(position, scale, parent);
        }

        tree.transform.parent = parent.transform;
        tree.layer = obstacleLayer;

        // Корректируем позицию по земле
        float groundHeight = GetGroundHeightAtPosition(position.x, position.z);
        PlaceOnGround(tree, groundHeight);

        if (tree.GetComponent<Collider>() == null)
        {
            BoxCollider col = tree.AddComponent<BoxCollider>();
            col.size = new Vector3(1f, 2f * scale, 1f);
        }

        return tree;
    }

    GameObject CreateRockAtPosition(Vector3 position, float scale, GameObject parent)
    {
        GameObject rock;

        if (usePrefabsForBorder && borderRockPrefabs != null && borderRockPrefabs.Length > 0)
        {
            GameObject prefab = borderRockPrefabs[Random.Range(0, borderRockPrefabs.Length)];
            rock = Instantiate(prefab, position, Quaternion.Euler(0, Random.Range(0, 360), 0));
            rock.transform.localScale = Vector3.one * scale;
        }
        else
        {
            rock = CreateProceduralRock(position, scale, parent);
        }

        rock.transform.parent = parent.transform;
        rock.layer = obstacleLayer;

        // Корректируем позицию по земле
        float groundHeight = GetGroundHeightAtPosition(position.x, position.z);
        PlaceOnGround(rock, groundHeight);

        if (rock.GetComponent<Collider>() == null)
        {
            BoxCollider col = rock.AddComponent<BoxCollider>();
            col.size = new Vector3(1f, 1f * scale, 1f);
        }

        return rock;
    }

    /// <summary>
    /// Устанавливает pivot в нижнюю точку объекта (для земли, деревьев, камней)
    /// </summary>
    private GameObject FixPivotProgrammatically(GameObject originalPrefab, string objectType)
    {
        if (originalPrefab == null) return originalPrefab;

        try
        {
            // Создаём временный объект для анализа
            GameObject tempObj = Instantiate(originalPrefab, Vector3.zero, Quaternion.identity);
            tempObj.SetActive(false);

            // Получаем реальные границы объекта
            Renderer renderer = tempObj.GetComponentInChildren<Renderer>();
            if (renderer == null)
            {
                DestroyImmediate(tempObj);
                return originalPrefab;
            }

            // Вычисляем смещение до нижней точки
            Bounds bounds = renderer.bounds;
            float bottomY = bounds.min.y;
            float currentPivotY = tempObj.transform.position.y;
            float offsetToBottom = currentPivotY - bottomY; // На сколько нужно поднять объект, чтобы низ был на 0

            DestroyImmediate(tempObj);

            // Если pivot уже внизу (offset близок к 0 или высоте)
            if (Mathf.Abs(offsetToBottom) < 0.01f || Mathf.Abs(offsetToBottom - renderer.bounds.size.y) < 0.01f)
            {
                return originalPrefab; // Уже правильно
            }

            // Создаём новый префаб с правильным pivot через родительский объект
            GameObject fixedPrefab = new GameObject($"{originalPrefab.name}_Fixed");

            // Создаём дочерний объект с оригинальной моделью
            GameObject child = Instantiate(originalPrefab, fixedPrefab.transform);

            // Смещаем дочерний объект так, чтобы его низ был на Y=0 родителя
            Renderer childRenderer = child.GetComponentInChildren<Renderer>();
            if (childRenderer != null)
            {
                Bounds childBounds = childRenderer.bounds;
                float childBottomY = childBounds.min.y;
                float childWorldY = child.transform.position.y;
                float neededOffset = childWorldY - childBottomY; // Сдвиг, чтобы низ был на 0

                child.transform.localPosition = new Vector3(0, -neededOffset, 0);
            }

            // Сохраняем как префаб (только в редакторе)
#if UNITY_EDITOR
            string path = $"Assets/FixedPrefabs/{originalPrefab.name}_Fixed.prefab";

            // Создаём папку, если её нет
            if (!System.IO.Directory.Exists("Assets/FixedPrefabs"))
                System.IO.Directory.CreateDirectory("Assets/FixedPrefabs");

            UnityEditor.PrefabUtility.SaveAsPrefabAsset(fixedPrefab, path);
            GameObject loadedPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            DestroyImmediate(fixedPrefab);

            Debug.Log($"✅ Создан исправленный префаб: {path}");
            return loadedPrefab != null ? loadedPrefab : originalPrefab;
#else
        return originalPrefab; // В билде используем оригинал
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка при исправлении pivot для {originalPrefab.name}: {e.Message}");
            return originalPrefab;
        }
    }

    /// <summary>
    /// Получает самую нижнюю точку объекта в мировых координатах
    /// </summary>
    private float GetBottomY(GameObject obj)
    {
        float lowestY = float.MaxValue;

        // Проверяем все Renderer
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            if (rend.bounds.min.y < lowestY)
                lowestY = rend.bounds.min.y;
        }

        // Проверяем все Collider
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            if (col.bounds.min.y < lowestY)
                lowestY = col.bounds.min.y;
        }

        if (lowestY == float.MaxValue)
            return obj.transform.position.y;

        return lowestY;
    }

    /// <summary>
    /// Универсальный метод для правильного размещения любого объекта на земле
    /// </summary>
    private void PlaceOnGround(GameObject obj, float groundHeight)
    {
        if (obj == null) return;

        // Получаем самую нижнюю точку объекта
        float bottomY = GetBottomY(obj);
        float currentY = obj.transform.position.y;
        float offset = currentY - bottomY; // Расстояние от pivot до низа

        // Ставим объект так, чтобы его низ был на уровне земли
        obj.transform.position = new Vector3(obj.transform.position.x, groundHeight + offset, obj.transform.position.z);

        // Дополнительная проверка для очень больших объектов (как замок)
        Renderer renderer = obj.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            float objectHeight = renderer.bounds.size.y;
            Debug.Log($"Object {obj.name}: height={objectHeight}, bottom={bottomY}, offset={offset}, finalY={obj.transform.position.y}");
        }
    }

    GameObject CreateProceduralTree(Vector3 position, float scale, GameObject parent)
    {
        TreeType type = (TreeType)Random.Range(0, System.Enum.GetValues(typeof(TreeType)).Length);
        Color color = treeColors[Random.Range(0, treeColors.Length)];

        GameObject tree = new GameObject($"Tree_{type}_{Random.Range(0, 1000)}");
        tree.transform.position = position;
        tree.transform.parent = parent.transform;

        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.transform.parent = tree.transform;
        trunk.transform.localPosition = Vector3.zero;
        trunk.transform.localScale = new Vector3(0.6f * scale, 1f * scale, 0.6f * scale);
        trunk.GetComponent<Renderer>().material.color = new Color(0.55f, 0.35f, 0.2f);
        DestroyImmediate(trunk.GetComponent<Collider>());

        GameObject crown = null;

        switch (type)
        {
            case TreeType.Pine:
                crown = CreateCone(1.2f * scale, 1.5f * scale);
                crown.transform.parent = tree.transform;
                crown.transform.localPosition = new Vector3(0, 1f * scale, 0);
                break;
            case TreeType.Oak:
                crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                crown.transform.parent = tree.transform;
                crown.transform.localPosition = new Vector3(0, 0.8f * scale, 0);
                crown.transform.localScale = new Vector3(1.3f * scale, 1.1f * scale, 1.3f * scale);
                break;
            case TreeType.Round:
                crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                crown.transform.parent = tree.transform;
                crown.transform.localPosition = new Vector3(0, 0.7f * scale, 0);
                crown.transform.localScale = new Vector3(1.5f * scale, 1.2f * scale, 1.5f * scale);
                break;
            case TreeType.Tall:
                crown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                crown.transform.parent = tree.transform;
                crown.transform.localPosition = new Vector3(0, 1.1f * scale, 0);
                crown.transform.localScale = new Vector3(1.1f * scale, 1.8f * scale, 1.1f * scale);
                break;
        }

        if (crown != null)
        {
            crown.GetComponent<Renderer>().material.color = color;
            Collider crownCollider = crown.GetComponent<Collider>();
            if (crownCollider != null)
            {
                if (crownCollider is SphereCollider sphereCol)
                    sphereCol.radius = 0.7f;
                else if (crownCollider is CapsuleCollider capsuleCol)
                    capsuleCol.radius = 0.6f;
            }
        }

        tree.layer = obstacleLayer;
        foreach (Transform child in tree.transform)
        {
            child.gameObject.layer = obstacleLayer;
        }

        return tree;
    }

    GameObject CreateCone(float radius, float height)
    {
        GameObject cone = new GameObject("Cone");
        Mesh mesh = new Mesh();
        int segments = 24;
        int vertexCount = segments + 1;
        Vector3[] vertices = new Vector3[vertexCount + 1];
        Vector2[] uv = new Vector2[vertexCount + 1];
        int[] triangles = new int[segments * 3];

        vertices[0] = new Vector3(0, height, 0);
        uv[0] = new Vector2(0.5f, 1f);

        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            vertices[i + 1] = new Vector3(x, 0, z);
            uv[i + 1] = new Vector2((float)i / segments, 0);
        }

        vertices[vertexCount] = new Vector3(0, 0, 0);
        uv[vertexCount] = new Vector2(0.5f, 0);

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 1) % segments + 1;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        MeshFilter meshFilter = cone.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        MeshRenderer renderer = cone.AddComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("Standard"));

        return cone;
    }

    GameObject CreateProceduralRock(Vector3 position, float scale, GameObject parent)
    {
        RockType type = (RockType)Random.Range(0, System.Enum.GetValues(typeof(RockType)).Length);
        Color color = rockColors[Random.Range(0, rockColors.Length)];

        GameObject rock = new GameObject($"Rock_{type}_{Random.Range(0, 1000)}");
        rock.transform.position = position;
        rock.transform.parent = parent.transform;

        GameObject rockMesh = null;

        switch (type)
        {
            case RockType.Round:
                rockMesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rockMesh.transform.localScale = new Vector3(0.9f * scale, 0.7f * scale, 0.8f * scale);
                break;
            case RockType.Sharp:
                rockMesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                rockMesh.transform.localScale = new Vector3(0.8f * scale, 0.6f * scale, 0.8f * scale);
                rockMesh.transform.rotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
                break;
            case RockType.Flat:
                rockMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rockMesh.transform.localScale = new Vector3(1.2f * scale, 0.4f * scale, 1.1f * scale);
                break;
            case RockType.Huge:
                rockMesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rockMesh.transform.localScale = new Vector3(1.4f * scale, 0.9f * scale, 1.3f * scale);
                scale *= 1.5f;
                break;
        }

        if (rockMesh != null)
        {
            rockMesh.transform.parent = rock.transform;
            rockMesh.transform.localPosition = Vector3.zero;
            rockMesh.GetComponent<Renderer>().material.color = color;

            Collider rockCollider = rockMesh.GetComponent<Collider>();
            if (rockCollider != null)
            {
                PhysicsMaterial rockMaterial = new PhysicsMaterial("RockMaterial");
                rockMaterial.dynamicFriction = 0.8f;
                rockMaterial.staticFriction = 0.8f;
                rockMaterial.bounciness = 0.1f;
                rockCollider.material = rockMaterial;
            }
        }

        rock.layer = obstacleLayer;
        foreach (Transform child in rock.transform)
        {
            child.gameObject.layer = obstacleLayer;
        }

        return rock;
    }

    void GenerateVegetation()
    {
        if (treePrefab == null && rockPrefab == null && bushPrefab == null) return;

        GameObject vegetationParent = new GameObject("Vegetation");
        int halfSize = currentMapSize / 2;

        for (int x = 0; x < currentMapSize; x++)
        {
            for (int z = 0; z < currentMapSize; z++)
            {
                bool isBorder = x < borderWidth || x >= currentMapSize - borderWidth ||
                                z < borderWidth || z >= currentMapSize - borderWidth;

                if (isBorder) continue;

                float height = heightMap[x, z];
                float yPos = GetYPosition(height, 0f);
                Vector3 position = new Vector3((x - halfSize) * currentCellSize, yPos, (z - halfSize) * currentCellSize);

                string biome = GetBiomeAtHeight(height);

                if (treePrefab != null && (biome == "Forest" || biome == "Hills"))
                {
                    if (Random.value < treeDensity * GetDensityMultiplier(biome))
                    {
                        SpawnObject(treePrefab, position, Random.Range(0.8f, 1.5f), vegetationParent);
                    }
                }

                if (rockPrefab != null && biome != "Beach")
                {
                    if (Random.value < rockDensity * GetDensityMultiplier(biome))
                    {
                        SpawnObject(rockPrefab, position, Random.Range(0.5f, 1.2f), vegetationParent);
                    }
                }

                if (bushPrefab != null && Random.value < bushDensity * GetDensityMultiplier(biome))
                {
                    SpawnObject(bushPrefab, position, Random.Range(0.7f, 1.2f), vegetationParent);
                }
            }
        }
    }

    void SpawnObject(GameObject prefab, Vector3 position, float scale, GameObject parent = null)
    {
        if (prefab == null) return;

        GameObject obj = Instantiate(prefab, position, Quaternion.Euler(0, Random.Range(0, 360), 0));
        obj.transform.localScale = Vector3.one * scale;
        if (parent != null) obj.transform.parent = parent.transform;

        // Корректируем позицию по земле
        float groundHeight = GetGroundHeightAtPosition(position.x, position.z);
        PlaceOnGround(obj, groundHeight);
    }

    string GetBiomeAtHeight(float height)
    {
        if (height < 0.2f) return "Beach";
        if (height < 0.4f) return "Plains";
        if (height < 0.6f) return "Forest";
        if (height < 0.8f) return "Winter";
        return "Mountains";
    }

    float GetDensityMultiplier(string biome)
    {
        switch (biome)
        {
            case "Forest": return 1.5f;
            case "Hills": return 1.2f;
            case "Plains": return 0.8f;
            default: return 1f;
        }
    }

    void SpawnWater()
    {
        if (waterPlanePrefab == null) return;

        float waterHeight = 0.3f * currentTerrainHeight + TILE_HEIGHT / 2f;
        Vector3 waterPos = new Vector3(0, waterHeight, 0);
        GameObject water = Instantiate(waterPlanePrefab, waterPos, Quaternion.identity);
        water.transform.localScale = new Vector3(currentMapSize * currentCellSize / 10f, 1, currentMapSize * currentCellSize / 10f);

        if (waterMaterial != null)
        {
            Renderer renderer = water.GetComponent<Renderer>();
            if (renderer != null) renderer.material = waterMaterial;
        }
    }

    float GetGroundHeightAtPosition(float worldX, float worldZ)
    {
        // Вычисляем индексы с правильным округлением
        float halfSize = currentMapSize / 2f;
        float xFloat = (worldX / currentCellSize) + halfSize;
        float zFloat = (worldZ / currentCellSize) + halfSize;

        int x = Mathf.FloorToInt(xFloat);
        int z = Mathf.FloorToInt(zFloat);

        // Проверяем границы
        if (x < 0) x = 0;
        if (x >= currentMapSize) x = currentMapSize - 1;
        if (z < 0) z = 0;
        if (z >= currentMapSize) z = currentMapSize - 1;

        float height = heightMap[x, z];
        float groundY = height * currentTerrainHeight;

        // Для отладки
        // Debug.Log($"GetGroundHeightAtPosition({worldX}, {worldZ}) -> x={x}, z={z}, height={height}, groundY={groundY}");

        return groundY;
    }

    float GetLowestPoint(GameObject obj)
    {
        float lowestY = float.MaxValue;

        MeshFilter[] meshes = obj.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter mesh in meshes)
        {
            if (mesh.sharedMesh != null)
            {
                float localLowest = mesh.sharedMesh.bounds.min.y;
                Vector3 worldLowest = mesh.transform.TransformPoint(new Vector3(0, localLowest, 0));
                if (worldLowest.y < lowestY)
                    lowestY = worldLowest.y;
            }
        }

        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            float bottomY = collider.bounds.min.y;
            if (bottomY < lowestY)
                lowestY = bottomY;
        }

        if (lowestY == float.MaxValue)
            return 0f;

        return lowestY;
    }

    Vector3 FindSuitableSpawnPoint(float minDistance, float maxDistance, params string[] allowedBiomes)
    {
        List<Vector3> candidates = new List<Vector3>();
        int halfSize = currentMapSize / 2;

        for (int x = borderWidth; x < currentMapSize - borderWidth; x++)
        {
            for (int z = borderWidth; z < currentMapSize - borderWidth; z++)
            {
                float worldX = (x - halfSize) * currentCellSize;
                float worldZ = (z - halfSize) * currentCellSize;
                float distanceFromCenter = Vector2.Distance(new Vector2(worldX, worldZ), Vector2.zero);

                if (distanceFromCenter >= minDistance && distanceFromCenter <= maxDistance)
                {
                    float height = heightMap[x, z];
                    string biome = GetBiomeAtHeight(height);

                    if (allowedBiomes.Contains(biome))
                    {
                        Vector3 position = new Vector3(worldX, 0, worldZ);
                        candidates.Add(position);
                    }
                }
            }
        }

        if (candidates.Count > 0)
        {
            return candidates[Random.Range(0, candidates.Count)];
        }

        return Vector3.zero;
    }

    List<Vector3> FindEnemySpawnPoints(int count)
    {
        List<Vector3> spawnPoints = new List<Vector3>();
        List<Vector3> allValidPoints = new List<Vector3>();
        int halfSize = currentMapSize / 2;

        for (int x = borderWidth; x < currentMapSize - borderWidth; x++)
        {
            for (int z = borderWidth; z < currentMapSize - borderWidth; z++)
            {
                float height = heightMap[x, z];
                float groundHeight = height * currentTerrainHeight;
                Vector3 position = new Vector3((x - halfSize) * currentCellSize, groundHeight + 0.2f, (z - halfSize) * currentCellSize);

                float distanceFromPlayer = Vector3.Distance(position, playerStartPosition);

                if (distanceFromPlayer > 10f && distanceFromPlayer < 40f)
                {
                    string biome = GetBiomeAtHeight(height);
                    if (biome != "Beach" && biome != "Mountains")
                    {
                        allValidPoints.Add(position);
                    }
                }
            }
        }

        for (int i = 0; i < allValidPoints.Count && spawnPoints.Count < count; i++)
        {
            int randomIndex = Random.Range(i, allValidPoints.Count);
            Vector3 temp = allValidPoints[i];
            allValidPoints[i] = allValidPoints[randomIndex];
            allValidPoints[randomIndex] = temp;

            bool tooClose = false;
            foreach (Vector3 existing in spawnPoints)
            {
                if (Vector3.Distance(existing, allValidPoints[i]) < 5f)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                spawnPoints.Add(allValidPoints[i]);
            }
        }

        return spawnPoints;
    }

    void SpawnEnemiesOnTerrain()
    {
        List<EnemyPrefabData> availableEnemies = LoadAllAvailableEnemies();

        if (availableEnemies.Count == 0)
        {
            Debug.LogError("No enemy prefabs available!");
            return;
        }

        List<int> availablePowers = availableEnemies.Select(e => e.power).Distinct().OrderBy(p => p).ToList();
        List<int> enemyPowers = GeneratePowerSet(availablePowers);
        List<Vector3> spawnPoints = FindEnemySpawnPoints(enemyPowers.Count);

        for (int i = 0; i < spawnPoints.Count && i < enemyPowers.Count; i++)
        {
            int power = enemyPowers[i];
            GameObject prefab = FindPrefabByPower(power, availableEnemies);

            if (prefab != null)
            {
                GameObject enemy = Instantiate(prefab, spawnPoints[i], Quaternion.identity);
                PlaceOnGround(enemy, GetGroundHeightAtPosition(spawnPoints[i].x, spawnPoints[i].z));
                Debug.Log($"Enemy spawned: power={power} at {enemy.transform.position}");
            }
        }

        Debug.Log($"Total enemies spawned: {spawnPoints.Count}");
    }

    List<int> GeneratePowerSet(List<int> availablePowers)
    {
        List<int> powers = new List<int>();

        if (autoGenerateBalancedPowers)
        {
            int playerStartPower = 5;
            int currentPower = playerStartPower;

            List<int> winChain = new List<int>();
            List<int> tempAvailable = new List<int>(availablePowers);
            tempAvailable = tempAvailable.OrderBy(p => p).ToList();

            bool added = true;
            while (added && tempAvailable.Count > 0)
            {
                added = false;
                for (int i = 0; i < tempAvailable.Count; i++)
                {
                    if (tempAvailable[i] < currentPower)
                    {
                        winChain.Add(tempAvailable[i]);
                        currentPower += tempAvailable[i];
                        tempAvailable.RemoveAt(i);
                        added = true;
                        break;
                    }
                }
            }

            powers.AddRange(winChain);

            List<int> remainingPowers = new List<int>(availablePowers);
            foreach (int power in winChain) remainingPowers.Remove(power);

            while (powers.Count < enemiesCount && remainingPowers.Count > 0)
            {
                int randomIndex = Random.Range(0, remainingPowers.Count);
                powers.Add(remainingPowers[randomIndex]);
                remainingPowers.RemoveAt(randomIndex);
            }

            if (powers.Count < enemiesCount && availablePowers.Count > 0)
            {
                int weakestPower = availablePowers.Min();
                while (powers.Count < enemiesCount) powers.Add(weakestPower);
            }

            Debug.Log($"Win chain: {string.Join(" → ", winChain)}");
        }

        return powers;
    }

    List<EnemyPrefabData> LoadAllAvailableEnemies()
    {
        List<EnemyPrefabData> availableEnemies = new List<EnemyPrefabData>();

        foreach (GameObject prefab in enemyPrefabs)
        {
            if (prefab == null) continue;

            GameObject tempEnemy = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            tempEnemy.SetActive(false);
            Enemy enemyComponent = tempEnemy.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                availableEnemies.Add(new EnemyPrefabData
                {
                    prefab = prefab,
                    power = enemyComponent.power,
                    attackType = enemyComponent.attackType,
                    prefabName = prefab.name
                });
            }
            DestroyImmediate(tempEnemy);
        }

        return availableEnemies.OrderBy(e => e.power).ToList();
    }

    GameObject FindPrefabByPower(int requiredPower, List<EnemyPrefabData> availableEnemies)
    {
        EnemyPrefabData exactMatch = availableEnemies.FirstOrDefault(e => e.power == requiredPower);
        if (exactMatch != null) return exactMatch.prefab;

        EnemyPrefabData nearestMatch = availableEnemies
            .Where(e => e.power <= requiredPower)
            .OrderByDescending(e => e.power)
            .FirstOrDefault();

        if (nearestMatch != null) return nearestMatch.prefab;

        return availableEnemies.OrderBy(e => e.power).FirstOrDefault()?.prefab;
    }

    void SetupEnvironment()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.01f;
        RenderSettings.fogColor = new Color(0.6f, 0.7f, 0.8f);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.5f, 0.6f, 0.7f);
        RenderSettings.ambientEquatorColor = new Color(0.3f, 0.35f, 0.4f);
        RenderSettings.ambientGroundColor = new Color(0.2f, 0.2f, 0.25f);
    }

    public void NextLevel()
    {
        currentLevel++;

        LevelData nextLevelData = GetLevelData(currentLevel);
        if (nextLevelData == null)
        {
            Debug.Log("Congratulations! You completed all levels!");
            return;
        }

        ClearLevelObjects();
        GenerateLevel(currentLevel);
    }

    void ClearLevelObjects()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }

        if (currentCastle != null)
            Destroy(currentCastle);

        GameObject vegetation = GameObject.Find("Vegetation");
        if (vegetation != null)
            Destroy(vegetation);

        GameObject border = GameObject.Find("BorderForest");
        if (border != null)
            Destroy(border);

        GameObject terrain = GameObject.Find("Terrain");
        if (terrain != null)
            Destroy(terrain);

        GameObject water = GameObject.FindGameObjectWithTag("Water");
        if (water != null)
            Destroy(water);
    }

    [ContextMenu("Load Next Level")]
    public void LoadNextLevelTest()
    {
        NextLevel();
    }

    private class EnemyPrefabData
    {
        public GameObject prefab;
        public int power;
        public AttackType attackType;
        public string prefabName;
    }*/
}