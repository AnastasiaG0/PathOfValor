using UnityEngine;
using UnityEngine.UI;

public class VictoryButtonsController : MonoBehaviour
{
    [Header("Кнопки")]
    public Button mainMenuButton;
    public Button restartButton;

    void Start()
    {
        // Привязываем обработчики к кнопкам
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClick);
            Debug.Log("MainMenuButton привязана!");
        }
        else
        {
            Debug.LogError("MainMenuButton не назначена в VictoryButtonsController!");
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClick);
            Debug.Log("RestartButton привязана!");
        }
        else
        {
            Debug.LogError("RestartButton не назначена в VictoryButtonsController!");
        }
    }

    void OnMainMenuClick()
    {
        Debug.Log("Нажата кнопка 'В главное меню'");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
        else
        {
            Debug.LogError("GameManager.Instance не найден!");
        }
    }

    void OnRestartClick()
    {
        Debug.Log("Нажата кнопка 'Играть заново'");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
        else
        {
            Debug.LogError("GameManager.Instance не найден!");
        }
    }

    // Опционально: очистка слушателей при отключении
    void OnDisable()
    {
        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(OnMainMenuClick);

        if (restartButton != null)
            restartButton.onClick.RemoveListener(OnRestartClick);
    }
}