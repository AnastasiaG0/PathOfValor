using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VictoryButtonsController : MonoBehaviour
{
    [Header("Кнопки")]
    public Button mainMenuButton;
    public Button replayButton;

    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.sendNavigationEvents = true;
        }
    }

    void Start()
    {
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(() => {
                Debug.Log("Victory: Главное меню");
                GameManager.Instance?.LoadMainMenu();
            });
        }

        if (replayButton != null)
        {
            replayButton.onClick.RemoveAllListeners();
            replayButton.onClick.AddListener(() => {
                Debug.Log("Victory: Играть заново");
                GameManager.Instance?.RestartGame();
            });
        }
        else
        {
            Debug.LogError("ReplayButton не назначен в VictoryButtonsController!");
        }
    }
}