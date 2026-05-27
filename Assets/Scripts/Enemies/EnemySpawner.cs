using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Префабы врагов")]
    public GameObject[] enemyPrefabs;

    [Header("Позиции спавна")]
    public Transform[] spawnPoints;

    [Header("Настройки")]
    public bool spawnOnStart = true;

    private List<Enemy> activeEnemies = new List<Enemy>();

    void Start()
    {
        if (spawnOnStart)
            SpawnAllEnemies();
    }

    public void SpawnAllEnemies()
    {
        // Пример конфигурации врагов
        int[] powers = { 2, 4, 3, 7, 5, 9, 6, 12 };
        AttackType[] attackTypes = {
            AttackType.Sword, AttackType.Arrow, AttackType.Throw, AttackType.Fire,
            AttackType.Sword, AttackType.HeavyStrike, AttackType.Arrow, AttackType.Fire
        };

        for (int i = 0; i < spawnPoints.Length && i < enemyPrefabs.Length; i++)
        {
            GameObject enemyObj = Instantiate(enemyPrefabs[i % enemyPrefabs.Length], spawnPoints[i].position, Quaternion.identity);
            Enemy enemy = enemyObj.GetComponent<Enemy>();

            if (enemy != null && i < powers.Length)
            {
                enemy.power = powers[i % powers.Length];
                enemy.attackType = attackTypes[i % attackTypes.Length];
                activeEnemies.Add(enemy);
            }
        }
    }

    public void SpawnEnemyAtPosition(GameObject enemyPrefab, Vector3 position, int power, AttackType attackType)
    {
        GameObject enemyObj = Instantiate(enemyPrefab, position, Quaternion.identity);
        Enemy enemy = enemyObj.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.power = power;
            enemy.attackType = attackType;
            activeEnemies.Add(enemy);
        }
    }

    public int GetRemainingEnemies()
    {
        activeEnemies.RemoveAll(e => e == null || !e.IsAlive);
        return activeEnemies.Count;
    }
}
