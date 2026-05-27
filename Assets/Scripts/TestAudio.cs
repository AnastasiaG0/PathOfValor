using UnityEngine;

public class TestAudio : MonoBehaviour
{
    void Start()
    {
        // Тест через 1 секунду после запуска
        Invoke("TestAllSounds", 1f);
    }

    void TestAllSounds()
    {
        Debug.Log("Тестирование звуков...");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("Swing");
            Invoke("TestVictory", 0.5f);
        }
    }

    void TestVictory()
    {
        AudioManager.Instance.PlaySound("Victory");
        Invoke("TestDeath", 0.5f);
    }

    void TestDeath()
    {
        AudioManager.Instance.PlaySound("Death");
        Invoke("TestWin", 0.5f);
    }

    void TestWin()
    {
        AudioManager.Instance.PlaySound("Win");
    }
}
