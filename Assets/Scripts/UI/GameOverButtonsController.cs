using UnityEngine;
using UnityEngine.UI;

public class GameOverButtonsController : MonoBehaviour
{
    [Header("Кнопки")]
    public Button mainMenuButton;
    public Button restartButton;

    void Start()
    {
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(OnMainMenuClick);
            Debug.Log("GameOver MainMenuButton привязана!");
        }
        else
        {
            Debug.LogError("MainMenuButton не назначена в GameOverButtonsController!");
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartClick);
            Debug.Log("GameOver RestartButton привязана!");
        }
        else
        {
            Debug.LogError("RestartButton не назначена в GameOverButtonsController!");
        }
    }

    void OnMainMenuClick()
    {
        Debug.Log("Нажата кнопка 'В главное меню' (поражение)");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
    }

    void OnRestartClick()
    {
        Debug.Log("Нажата кнопка 'Играть заново' (поражение)");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
}