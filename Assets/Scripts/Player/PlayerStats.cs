using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    private int score = 0;
    private int enemiesDefeated = 0;

    public void AddScore(int points)
    {
        score += points;
        Debug.Log($"Score: {score}");
    }

    public void AddEnemyDefeated()
    {
        enemiesDefeated++;
        Debug.Log($"Enemies defeated: {enemiesDefeated}");
    }

    public int GetScore() => score;
    public int GetEnemiesDefeated() => enemiesDefeated;
}
