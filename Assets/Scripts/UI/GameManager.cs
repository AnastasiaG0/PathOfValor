using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Панели")]
    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    public GameObject pausePanel;

    [Header("Кнопки Victory Panel")]
    public Button replayLevelButton;
    public Button nextLevelButton;
    public Button mainMenuFromVictoryButton;

    [Header("Тексты Victory Panel")]
    public TMP_Text victoryResultText;
    public TMP_Text victoryFinalPowerText;
    public TMP_Text victoryEnemiesKilledText;
    public TMP_Text victorylevelCompletedText;
    public TMP_Text nextLevelButtonText;

    [Header("Тексты для поражения")]
    public TMP_Text defeatResultText;
    public TMP_Text defeatEnemiesKilledText;

    [Header("Настройки уровней")]
    public int totalLevels = 5;

    [Header("Состояние игры")]
    public bool IsGameOver { get; private set; } = false;
    public bool IsVictory { get; private set; } = false;

    private int currentPower = 5;
    private int enemiesKilled = 0;
    private int totalEnemies = 0;
    private int currentLevel = 1;

    private bool isInitialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(Instance.gameObject);
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            ResetGameState();
            FindAndAssignUI();
            FindAndAssignButtons();
            LoadCurrentLevel();
        }
    }

    void ResetGameState()
    {
        currentPower = 5;
        enemiesKilled = 0;
        IsGameOver = false;
        IsVictory = false;
        isInitialized = false;
        currentLevel = PlayerPrefs.GetInt("SelectedLevel", 1);

        Time.timeScale = 1f;

        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        FindTotalEnemies();
    }

    void LoadCurrentLevel()
    {
        SceneManager.LoadScene($"Level{currentLevel}", LoadSceneMode.Additive);
    }

    void FindTotalEnemies()
    {
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        totalEnemies = allEnemies.Length;
    }

    void FindAndAssignUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("Canvas не найден!");
            return;
        }

        if (victoryPanel == null)
            victoryPanel = FindInChildren(canvas.gameObject, "VictoryPanel");
        if (gameOverPanel == null)
            gameOverPanel = FindInChildren(canvas.gameObject, "GameOverPanel");
        if (pausePanel == null)
            pausePanel = FindInChildren(canvas.gameObject, "PausePanel");

        if (victoryPanel != null)
        {
            if (victoryResultText == null)
                victoryResultText = FindInChildren(victoryPanel, "VictoryTitle")?.GetComponent<TMP_Text>();
            if (victoryFinalPowerText == null)
                victoryFinalPowerText = FindInChildren(victoryPanel, "VictoryFinalPowerText")?.GetComponent<TMP_Text>();
            if (victoryEnemiesKilledText == null)
                victoryEnemiesKilledText = FindInChildren(victoryPanel, "VictoryEnemiesKilledText")?.GetComponent<TMP_Text>();
            if (victorylevelCompletedText == null)
                victorylevelCompletedText = FindInChildren(victoryPanel, "VictoryLevelCompletedText")?.GetComponent<TMP_Text>();
        }

        if (gameOverPanel != null)
        {
            if (defeatResultText == null)
                defeatResultText = FindInChildren(gameOverPanel, "GameOverTitle")?.GetComponent<TMP_Text>();
            if (defeatEnemiesKilledText == null)
                defeatEnemiesKilledText = FindInChildren(gameOverPanel, "GameOverEnemiesKilledText")?.GetComponent<TMP_Text>();
        }

        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        isInitialized = true;
    }

    void FindAndAssignButtons()
    {
        // Привязка кнопок для панели победы
        if (victoryPanel != null)
        {
            if (replayLevelButton == null)
            {
                GameObject btn = FindInChildren(victoryPanel, "ReplayButton");
                if (btn != null) replayLevelButton = btn.GetComponent<Button>();
            }

            if (nextLevelButton == null)
            {
                GameObject btn = FindInChildren(victoryPanel, "NextLevelButton");
                if (btn != null) nextLevelButton = btn.GetComponent<Button>();
            }

            if (mainMenuFromVictoryButton == null)
            {
                GameObject btn = FindInChildren(victoryPanel, "MainMenuButton");
                if (btn != null) mainMenuFromVictoryButton = btn.GetComponent<Button>();
            }

            if (nextLevelButton != null && nextLevelButtonText == null)
            {
                nextLevelButtonText = nextLevelButton.GetComponentInChildren<TMP_Text>();
            }

            if (replayLevelButton != null)
                replayLevelButton.onClick.AddListener(ReplayCurrentLevel);

            if (nextLevelButton != null)
                nextLevelButton.onClick.AddListener(LoadNextLevel);

            if (mainMenuFromVictoryButton != null)
                mainMenuFromVictoryButton.onClick.AddListener(LoadMainMenu);
        }

        // ===== ПРИВЯЗКА КНОПОК ДЛЯ ПАНЕЛИ ПОРАЖЕНИЯ =====
        if (gameOverPanel != null)
        {
            // Находим кнопку "Главное меню" на панели поражения
            Button gameOverMainMenuButton = FindInChildren(gameOverPanel, "MainMenuButton")?.GetComponent<Button>();
            if (gameOverMainMenuButton != null)
            {
                gameOverMainMenuButton.onClick.RemoveAllListeners();
                gameOverMainMenuButton.onClick.AddListener(LoadMainMenu);
                Debug.Log("GameOver MainMenuButton привязана!");
            }
            else
            {
                Debug.LogWarning("MainMenuButton не найдена на GameOverPanel!");
            }

            // Находим кнопку "Играть заново" на панели поражения
            Button gameOverRestartButton = FindInChildren(gameOverPanel, "RestartButton")?.GetComponent<Button>();
            if (gameOverRestartButton != null)
            {
                gameOverRestartButton.onClick.RemoveAllListeners();
                gameOverRestartButton.onClick.AddListener(RestartGame);
                Debug.Log("GameOver RestartButton привязана!");
            }
            else
            {
                // Если кнопка называется ReplayButton, а не RestartButton
                Button replayBtn = FindInChildren(gameOverPanel, "ReplayButton")?.GetComponent<Button>();
                if (replayBtn != null)
                {
                    replayBtn.onClick.RemoveAllListeners();
                    replayBtn.onClick.AddListener(RestartGame);
                    Debug.Log("GameOver ReplayButton привязана!");
                }
                else
                {
                    Debug.LogWarning("RestartButton/ReplayButton не найдена на GameOverPanel!");
                }
            }
        }
    }

    GameObject FindInChildren(GameObject parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
                return child.gameObject;
        }
        return null;
    }

    void Start()
    {
        ResetGameState();

        if (!isInitialized)
        {
            FindAndAssignUI();
            FindAndAssignButtons();
        }
    }

    public void UpdatePlayerPower(int power)
    {
        currentPower = power;
    }

    public void EnemyKilled(int enemyPower)
    {
        enemiesKilled++;
        UpdateKillsDisplay();
    }

    public void UpdateKillsDisplay()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            foreach (Transform child in canvas.GetComponentsInChildren<Transform>())
            {
                if (child.name == "KillsText")
                {
                    TextMeshProUGUI killsText = child.GetComponent<TextMeshProUGUI>();
                    if (killsText != null)
                    {
                        killsText.text = $"УБИТО: {enemiesKilled}";
                    }
                    break;
                }
            }
        }
    }

    public void GameOver(bool isVictory)
    {
        IsGameOver = true;
        IsVictory = isVictory;
        Time.timeScale = 0f;

        if (isVictory)
        {
            MarkLevelAsCompleted();
            ShowVictoryScreen();
        }
        else
        {
            ShowGameOverScreen();
        }
    }

    void ShowVictoryScreen()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.sendNavigationEvents = true;
        }

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        if (victorylevelCompletedText != null)
            victorylevelCompletedText.text = $"Уровень {currentLevel} пройден";

        if (victoryResultText != null)
            victoryResultText.text = "ПОБЕДА!";

        if (victoryFinalPowerText != null)
            victoryFinalPowerText.text = $"Итоговая сила: {currentPower}";

        if (victoryEnemiesKilledText != null)
            victoryEnemiesKilledText.text = $"Убито врагов: {enemiesKilled}";

        ShowGameComplete();

        // ===== ПРОВЕРКА НА ПОСЛЕДНИЙ УРОВЕНЬ =====
        /*if (currentLevel >= totalLevels)
        {
            ShowGameComplete();
            return;  // Выходим, чтобы не настраивать кнопку следующего уровня
        }*/

        int nextLevel = currentLevel + 1;
        bool hasNextLevel = nextLevel <= totalLevels;

        if (nextLevelButton != null)
        {
            nextLevelButton.interactable = hasNextLevel;
        }

        if (nextLevelButtonText != null)
        {
            if (hasNextLevel)
                nextLevelButtonText.text = $"Уровень {nextLevel}";
            else
                nextLevelButtonText.text = "Конец игры";
        }

        AudioManager.Instance?.PlaySound("Win");
    }

    void ShowGameOverScreen()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.sendNavigationEvents = true;
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (defeatResultText != null)
            defeatResultText.text = "ПОРАЖЕНИЕ...";

        if (defeatEnemiesKilledText != null)
            defeatEnemiesKilledText.text = $"Убито врагов: {enemiesKilled}";

        AudioManager.Instance?.PlaySound("Lose");
    }

    void MarkLevelAsCompleted()
    {
        PlayerPrefs.SetInt($"Level_{currentLevel}_Completed", 1);
        PlayerPrefs.Save();
    }

    bool CheckNextLevelExists()
    {
        int nextLevel = currentLevel + 1;
        return nextLevel <= totalLevels;
    }

    void ReplayCurrentLevel()
    {
        Debug.Log($"Replaying level {currentLevel}");
        PlayerPrefs.SetInt("SelectedLevel", currentLevel);
        PlayerPrefs.Save();
        Time.timeScale = 1f;
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        // ВЫГРУЖАЕМ старую сцену уровня
        Scene oldLevelScene = SceneManager.GetSceneByName($"Level{currentLevel}");
        /*if (oldLevelScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(oldLevelScene);
        }

        // Перезагружаем Game (она заново загрузит уровень)*/
        SceneManager.LoadScene("Game");
    }

    void LoadNextLevel()
    {
        ShowGameComplete();
        return;

        /*int nextLevel = currentLevel + 1;

        if (!CheckNextLevelExists())
        {
            Debug.Log("No more levels! Game complete!");
            ShowGameComplete();
            return;
        }

        Debug.Log($"Loading next level: {nextLevel}");
        PlayerPrefs.SetInt("SelectedLevel", nextLevel);
        PlayerPrefs.Save();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);*/
    }

    void ShowGameComplete()
    {
        Debug.Log("=== SHOW GAME COMPLETE CALLED! ===");

        if (victoryResultText != null)
        {
            victoryResultText.text = "КОНЕЦ ИГРЫ";
            Debug.Log("VictoryResultText updated");
        }
        else
        {
            Debug.LogWarning("victoryResultText is NULL!");
        }

        if (victorylevelCompletedText != null)
        {
            victorylevelCompletedText.text = "";
            Debug.Log("victorylevelCompletedText updated");
        }
        else
        {
            Debug.LogWarning("victorylevelCompletedText is NULL!");
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.interactable = false;
            Debug.Log("nextLevelButton disabled");
        }
        else
        {
            Debug.LogWarning("nextLevelButton is NULL!");
        }

        if (nextLevelButtonText != null)
        {
            nextLevelButtonText.text = "Конец";
            Debug.Log("nextLevelButtonText updated");
        }
        else
        {
            Debug.LogWarning("nextLevelButtonText is NULL!");
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ResumeGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !IsGameOver)
        {
            if (pausePanel != null)
            {
                bool isPaused = pausePanel.activeSelf;
                pausePanel.SetActive(!isPaused);
                Time.timeScale = isPaused ? 1f : 0f;

                if (!isPaused) // Открываем паузу
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    // === КЛЮЧЕВОЕ: принудительно обновляем EventSystem ===
                    if (EventSystem.current != null)
                    {
                        EventSystem.current.SetSelectedGameObject(null);
                        // Сбрасываем драг состояние
                        EventSystem.current.sendNavigationEvents = true;
                    }
                }
                else // Закрываем паузу
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }
    } 
}