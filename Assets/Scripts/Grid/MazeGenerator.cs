using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Header("Префабы")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject playerPrefab;
    public GameObject castlePrefab;
    public GameObject[] enemyPrefabs;

    [Header("Размеры лабиринта")]
    public int width = 15;
    public int height = 15;
    public float cellSize = 2f;

    [Header("Стены")]
    public Material wallMaterial;
    public Material floorMaterial;

    // ФИКСИРОВАННЫЙ SEED ДЛЯ ОДИНАКОВОЙ ГЕНЕРАЦИИ
    private const int FIXED_SEED = 1;

    private int[,] maze;
    private Vector2Int playerStartPos = new Vector2Int(1, 1);
    private Vector2Int castlePos;

    void Start()
    {
        // Устанавливаем фиксированный seed для одинаковой генерации
        Random.InitState(FIXED_SEED);

        GenerateMaze();
        BuildMazeVisuals();
        SpawnPlayerAndCastle();
        SpawnFixedEnemies();
    }

    void GenerateMaze()
    {
        maze = new int[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                maze[x, y] = 1;

        int startX = 1;
        int startY = 1;
        maze[startX, startY] = 0;

        CarvePaths(startX, startY);
        ConnectIsolatedAreas();

        castlePos = new Vector2Int(width - 2, height - 2);
        maze[castlePos.x, castlePos.y] = 0;

        ConnectCastleToMaze();
        RemoveExtraPathsToCastle();

        Debug.Log($"Лабиринт сгенерирован. Размер: {width}x{height}");
    }

    void CarvePaths(int x, int y)
    {
        int[] dirX = { 0, 0, -2, 2 };
        int[] dirY = { -2, 2, 0, 0 };

        int[] order = { 0, 1, 2, 3 };
        for (int i = 0; i < order.Length; i++)
        {
            int rand = Random.Range(i, order.Length);
            int temp = order[i];
            order[i] = order[rand];
            order[rand] = temp;
        }

        foreach (int i in order)
        {
            int nx = x + dirX[i];
            int ny = y + dirY[i];

            if (nx > 0 && nx < width - 1 && ny > 0 && ny < height - 1 && maze[nx, ny] == 1)
            {
                maze[nx, ny] = 0;
                maze[x + dirX[i] / 2, y + dirY[i] / 2] = 0;
                CarvePaths(nx, ny);
            }
        }
    }

    void ConnectIsolatedAreas()
    {
        List<Vector2Int> allPassages = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (maze[x, y] == 0)
                    allPassages.Add(new Vector2Int(x, y));

        if (allPassages.Count == 0) return;

        List<HashSet<Vector2Int>> components = new List<HashSet<Vector2Int>>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        foreach (Vector2Int cell in allPassages)
        {
            if (visited.Contains(cell)) continue;
            HashSet<Vector2Int> component = GetConnectedComponent(cell);
            components.Add(component);
            foreach (Vector2Int c in component) visited.Add(c);
        }

        while (components.Count > 1)
        {
            HashSet<Vector2Int> comp1 = components[0];
            float minDist = float.MaxValue;
            Vector2Int closestPoint1 = Vector2Int.zero;
            Vector2Int closestPoint2 = Vector2Int.zero;
            HashSet<Vector2Int> comp2ToConnect = null;

            for (int i = 1; i < components.Count; i++)
            {
                foreach (Vector2Int p1 in comp1)
                {
                    foreach (Vector2Int p2 in components[i])
                    {
                        float dist = Vector2Int.Distance(p1, p2);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            closestPoint1 = p1;
                            closestPoint2 = p2;
                            comp2ToConnect = components[i];
                        }
                    }
                }
            }

            if (comp2ToConnect != null)
            {
                CreateCorridor(closestPoint1, closestPoint2);
                foreach (Vector2Int cell in comp2ToConnect) comp1.Add(cell);
                components.Remove(comp2ToConnect);
            }
        }
    }

    HashSet<Vector2Int> GetConnectedComponent(Vector2Int start)
    {
        HashSet<Vector2Int> component = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(start);
        component.Add(start);

        int[] dirX = { 0, 0, -1, 1 };
        int[] dirY = { -1, 1, 0, 0 };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            for (int i = 0; i < 4; i++)
            {
                Vector2Int neighbor = new Vector2Int(current.x + dirX[i], current.y + dirY[i]);
                if (neighbor.x >= 0 && neighbor.x < width && neighbor.y >= 0 && neighbor.y < height &&
                    maze[neighbor.x, neighbor.y] == 0 && !component.Contains(neighbor))
                {
                    component.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
        return component;
    }

    void CreateCorridor(Vector2Int p1, Vector2Int p2)
    {
        int x1 = Mathf.Min(p1.x, p2.x);
        int x2 = Mathf.Max(p1.x, p2.x);
        for (int x = x1; x <= x2; x++)
            if (maze[x, p1.y] == 1) maze[x, p1.y] = 0;

        int y1 = Mathf.Min(p1.y, p2.y);
        int y2 = Mathf.Max(p1.y, p2.y);
        for (int y = y1; y <= y2; y++)
            if (maze[p2.x, y] == 1) maze[p2.x, y] = 0;
    }

    void ConnectCastleToMaze()
    {
        List<Vector2Int> neighbors = GetNeighbors(castlePos);
        if (neighbors.Count == 0)
        {
            Vector2Int nearestPassage = FindNearestPassage(castlePos);
            CreateCorridor(castlePos, nearestPassage);
        }
    }

    List<Vector2Int> GetNeighbors(Vector2Int cell)
    {
        int[] dirX = { 0, 0, -1, 1 };
        int[] dirY = { -1, 1, 0, 0 };
        List<Vector2Int> neighbors = new List<Vector2Int>();
        for (int i = 0; i < 4; i++)
        {
            Vector2Int neighbor = new Vector2Int(cell.x + dirX[i], cell.y + dirY[i]);
            if (neighbor.x >= 0 && neighbor.x < width && neighbor.y >= 0 && neighbor.y < height &&
                maze[neighbor.x, neighbor.y] == 0)
                neighbors.Add(neighbor);
        }
        return neighbors;
    }

    Vector2Int FindNearestPassage(Vector2Int pos)
    {
        for (int radius = 1; radius < Mathf.Max(width, height); radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
                for (int dy = -radius; dy <= radius; dy++)
                    if (Mathf.Abs(dx) == radius || Mathf.Abs(dy) == radius)
                    {
                        Vector2Int check = new Vector2Int(pos.x + dx, pos.y + dy);
                        if (check.x > 0 && check.x < width - 1 && check.y > 0 && check.y < height - 1 &&
                            maze[check.x, check.y] == 0 && !(check.x == 1 && check.y == 1))
                            return check;
                    }
        }
        return new Vector2Int(1, 1);
    }

    void RemoveExtraPathsToCastle()
    {
        List<Vector2Int> neighbors = GetNeighbors(castlePos);
        if (neighbors.Count > 1)
        {
            for (int i = 1; i < neighbors.Count; i++)
                maze[neighbors[i].x, neighbors[i].y] = 1;
        }
    }

    void BuildMazeVisuals()
    {
        Vector3 startPos = new Vector3(-width * cellSize / 2, 0, -height * cellSize / 2);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = startPos + new Vector3(x * cellSize, 0, y * cellSize);
                if (floorPrefab != null)
                {
                    GameObject floor = Instantiate(floorPrefab, pos, Quaternion.identity);
                    floor.transform.localScale = new Vector3(cellSize, 0.1f, cellSize);
                    if (floorMaterial != null)
                        floor.GetComponent<Renderer>().material = floorMaterial;
                }
                if (maze[x, y] == 1 && wallPrefab != null)
                {
                    GameObject wall = Instantiate(wallPrefab, pos + Vector3.up * 1f, Quaternion.identity);
                    wall.transform.localScale = new Vector3(cellSize, 2f, cellSize);
                    if (wallMaterial != null)
                        wall.GetComponent<Renderer>().material = wallMaterial;
                }
            }
        }
    }

    void SpawnPlayerAndCastle()
    {
        Vector3 startPos = new Vector3(-width * cellSize / 2, 0.5f, -height * cellSize / 2);
        Vector3 playerPos = startPos + new Vector3(playerStartPos.x * cellSize, 0.5f, playerStartPos.y * cellSize);
        if (playerPrefab != null) Instantiate(playerPrefab, playerPos, Quaternion.identity);

        Vector3 castleWorldPos = startPos + new Vector3(castlePos.x * cellSize, 0.5f, castlePos.y * cellSize);
        if (castlePrefab != null)
        {
            GameObject castle = Instantiate(castlePrefab, castleWorldPos, Quaternion.identity);
            castle.tag = "Castle";
            castle.transform.localScale = new Vector3(cellSize, 2f, cellSize);
        }
    }

    List<Vector2Int> GetFreeCells()
    {
        List<Vector2Int> freeCells = new List<Vector2Int>();
        for (int x = 2; x < width - 2; x++)
            for (int y = 2; y < height - 2; y++)
                if (maze[x, y] == 0 && !(x == playerStartPos.x && y == playerStartPos.y) && !(x == castlePos.x && y == castlePos.y))
                    freeCells.Add(new Vector2Int(x, y));
        return freeCells;
    }

    /// <summary>
    /// Загружает все доступные силы из префабов врагов
    /// </summary>
    List<int> LoadAvailablePowersFromPrefabs()
    {
        List<int> powers = new List<int>();

        foreach (GameObject prefab in enemyPrefabs)
        {
            if (prefab == null) continue;

            GameObject tempEnemy = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            tempEnemy.SetActive(false);
            Enemy enemyComponent = tempEnemy.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                if (!powers.Contains(enemyComponent.power))
                {
                    powers.Add(enemyComponent.power);
                }
            }
            DestroyImmediate(tempEnemy);
        }

        powers.Sort();
        Debug.Log($"Доступные силы из префабов: {string.Join(", ", powers)}");
        return powers;
    }

    /// <summary>
    /// Находит префаб врага по силе (точное совпадение)
    /// </summary>
    GameObject FindPrefabByPowerExact(int requiredPower)
    {
        foreach (GameObject prefab in enemyPrefabs)
        {
            if (prefab == null) continue;

            GameObject tempEnemy = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            tempEnemy.SetActive(false);
            Enemy enemyComponent = tempEnemy.GetComponent<Enemy>();
            int prefabPower = enemyComponent != null ? enemyComponent.power : 0;
            DestroyImmediate(tempEnemy);

            if (prefabPower == requiredPower)
            {
                return prefab;
            }
        }

        Debug.LogError($"Не найден префаб с точной силой {requiredPower}! Доступные силы: {string.Join(", ", LoadAvailablePowersFromPrefabs())}");
        return null;
    }

    /// <summary>
    /// Проверяет, не слишком ли много врагов в одной области
    /// </summary>
    bool IsTooManyEnemiesInArea(Vector2Int position, List<KeyValuePair<Vector2Int, int>> spawnList, int radius = 3)
    {
        int enemiesInArea = 0;
        for (int x = position.x - radius; x <= position.x + radius; x++)
        {
            for (int y = position.y - radius; y <= position.y + radius; y++)
            {
                if (spawnList.Any(s => s.Key.x == x && s.Key.y == y))
                {
                    enemiesInArea++;
                    if (enemiesInArea >= 3) return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Разрежает скопления врагов (не более 2 в радиусе)
    /// </summary>
    void ThinEnemyClusters(List<KeyValuePair<Vector2Int, int>> spawnList, List<Vector2Int> allFreeCells)
    {
        bool changed = true;
        int iterations = 0;
        int maxIterations = 10;

        while (changed && iterations < maxIterations)
        {
            changed = false;

            for (int i = 0; i < spawnList.Count; i++)
            {
                var enemy1 = spawnList[i];

                for (int j = i + 1; j < spawnList.Count; j++)
                {
                    var enemy2 = spawnList[j];
                    float dist = Vector2Int.Distance(enemy1.Key, enemy2.Key);

                    if (dist < 3)
                    {
                        Vector2Int newPos = FindFarPosition(enemy2.Key, spawnList, allFreeCells);
                        if (newPos != Vector2Int.zero)
                        {
                            spawnList[j] = new KeyValuePair<Vector2Int, int>(newPos, enemy2.Value);
                            Debug.Log($"Враг с силой {enemy2.Value} перемещён с ({enemy2.Key.x},{enemy2.Key.y}) на ({newPos.x},{newPos.y})");
                            changed = true;
                            break;
                        }
                    }
                }
            }
            iterations++;
        }
    }

    /// <summary>
    /// Находит позицию подальше от других врагов
    /// </summary>
    Vector2Int FindFarPosition(Vector2Int currentPos, List<KeyValuePair<Vector2Int, int>> spawnList, List<Vector2Int> allFreeCells)
    {
        List<Vector2Int> freeCells = new List<Vector2Int>();
        foreach (var cell in allFreeCells)
        {
            if (!spawnList.Any(s => s.Key == cell))
            {
                freeCells.Add(cell);
            }
        }

        if (freeCells.Count == 0) return Vector2Int.zero;

        Vector2Int bestCell = freeCells[0];
        float maxMinDist = 0;

        foreach (var cell in freeCells)
        {
            float minDistToEnemy = float.MaxValue;
            foreach (var enemy in spawnList)
            {
                float dist = Vector2Int.Distance(cell, enemy.Key);
                if (dist < minDistToEnemy) minDistToEnemy = dist;
            }

            if (minDistToEnemy > maxMinDist)
            {
                maxMinDist = minDistToEnemy;
                bestCell = cell;
            }
        }

        return bestCell;
    }

    /// <summary>
    /// Находит путь от старта до замка
    /// </summary>
    List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        queue.Enqueue(start);
        visited.Add(start);
        cameFrom[start] = start;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == end)
            {
                List<Vector2Int> path = new List<Vector2Int>();
                Vector2Int node = end;
                while (node != start) { path.Add(node); node = cameFrom[node]; }
                path.Add(start);
                path.Reverse();
                return path;
            }
            Vector2Int[] neighbors = { new Vector2Int(current.x + 1, current.y), new Vector2Int(current.x - 1, current.y),
                                       new Vector2Int(current.x, current.y + 1), new Vector2Int(current.x, current.y - 1) };
            foreach (Vector2Int neighbor in neighbors)
                if (neighbor.x >= 0 && neighbor.x < width && neighbor.y >= 0 && neighbor.y < height &&
                    maze[neighbor.x, neighbor.y] == 0 && !visited.Contains(neighbor))
                { visited.Add(neighbor); cameFrom[neighbor] = current; queue.Enqueue(neighbor); }
        }
        return null;
    }

    /// <summary>
    /// ФИКСИРОВАННЫЙ СПАВН ВРАГОВ (только силы из префабов)
    /// </summary>
    void SpawnFixedEnemies()
    {
        // Загружаем доступные силы из префабов
        List<int> availablePowers = LoadAvailablePowersFromPrefabs();

        if (availablePowers.Count == 0)
        {
            Debug.LogError("Нет доступных префабов врагов!");
            return;
        }

        // Находим путь от старта до замка
        List<Vector2Int> mainPath = FindPath(playerStartPos, castlePos);

        if (mainPath == null || mainPath.Count == 0)
        {
            Debug.LogError("Не найден путь от старта до замка!");
            return;
        }

        Debug.Log($"Найден путь от старта до замка. Длина пути: {mainPath.Count}");
        Debug.Log($"Доступные силы из префабов: {string.Join(", ", availablePowers)}");

        Vector3 startPos = new Vector3(-width * cellSize / 2, 0.5f, -height * cellSize / 2);

        // Создаём список для спавна
        List<KeyValuePair<Vector2Int, int>> spawnList = new List<KeyValuePair<Vector2Int, int>>();

        // ========== ВЫИГРЫШНАЯ ЦЕПОЧКА ==========
        // Используем ТОЛЬКО доступные силы из префабов
        List<int> winChainPowers = new List<int>();
        int currentPower = 5;

        // Сортируем доступные силы по возрастанию
        List<int> sortedPowers = new List<int>(availablePowers);
        sortedPowers.Sort();

        // Строим цепочку из доступных сил
        foreach (int power in sortedPowers)
        {
            if (power < currentPower)
            {
                winChainPowers.Add(power);
                currentPower += power;
            }
        }

        if (winChainPowers.Count == 0)
        {
            Debug.LogError("Не удалось построить выигрышную цепочку из доступных сил!");
            return;
        }

        Debug.Log($"Выигрышная цепочка из доступных сил: {string.Join(" → ", winChainPowers)} (итоговая сила: {currentPower})");

        // Размещаем врагов цепочки на пути через равные промежутки
        int step = mainPath.Count / (winChainPowers.Count + 1);
        if (step < 1) step = 1;

        for (int i = 0; i < winChainPowers.Count; i++)
        {
            int pathIndex = Mathf.Min((i + 1) * step, mainPath.Count - 1);
            Vector2Int cell = mainPath[pathIndex];
            spawnList.Add(new KeyValuePair<Vector2Int, int>(cell, winChainPowers[i]));
            Debug.Log($"Враг цепочки: сила={winChainPowers[i]}, позиция=({cell.x},{cell.y}) (шаг {pathIndex})");
        }

        // ========== ДОПОЛНИТЕЛЬНЫЕ ВРАГИ (только из доступных сил) ==========
        List<Vector2Int> sideCells = new List<Vector2Int>();
        for (int x = 2; x < width - 2; x++)
        {
            for (int y = 2; y < height - 2; y++)
            {
                if (maze[x, y] == 0 &&
                    !mainPath.Contains(new Vector2Int(x, y)) &&
                    !(x == playerStartPos.x && y == playerStartPos.y) &&
                    !(x == castlePos.x && y == castlePos.y))
                {
                    sideCells.Add(new Vector2Int(x, y));
                }
            }
        }

        // Берём дополнительные силы из доступных (исключая уже использованные в цепочке)
        List<int> extraPowers = new List<int>();
        foreach (int power in availablePowers)
        {
            if (!winChainPowers.Contains(power))
            {
                extraPowers.Add(power);
            }
        }

        // Если дополнительных сил нет, используем самые слабые
        if (extraPowers.Count == 0 && availablePowers.Count > 0)
        {
            extraPowers.Add(availablePowers[0]);
        }

        for (int i = 0; i < sideCells.Count && i < extraPowers.Count; i++)
        {
            spawnList.Add(new KeyValuePair<Vector2Int, int>(sideCells[i], extraPowers[i]));
            Debug.Log($"Дополнительный враг: сила={extraPowers[i]}, позиция=({sideCells[i].x},{sideCells[i].y})");
        }

        // ========== РАЗРЕЖАЕМ СКОПЛЕНИЯ ВРАГОВ ==========
        List<Vector2Int> allFreeCells = GetFreeCells();
        ThinEnemyClusters(spawnList, allFreeCells);

        // ========== СОЗДАЁМ ВРАГОВ ==========
        foreach (var spawn in spawnList)
        {
            Vector3 pos = startPos + new Vector3(spawn.Key.x * cellSize, 0.5f, spawn.Key.y * cellSize);
            GameObject prefab = FindPrefabByPowerExact(spawn.Value);
            if (prefab != null)
            {
                GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);
                Enemy enemyComponent = enemy.GetComponent<Enemy>();
                if (enemyComponent != null)
                {
                    enemyComponent.power = spawn.Value;
                }
                Debug.Log($"Создан враг: сила={spawn.Value}, позиция=({spawn.Key.x},{spawn.Key.y})");
            }
        }

        Debug.Log($"Всего создано врагов: {spawnList.Count}. Выигрышная цепочка: {string.Join(" → ", winChainPowers)}");
    }
}