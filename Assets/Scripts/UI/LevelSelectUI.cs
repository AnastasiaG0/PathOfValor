using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class LevelSelectUI : MonoBehaviour
{
    [Header("Компоненты панели")]
    public GameObject levelButtonPrefab;
    public Transform buttonsContainer;
    public Button startGameButton;
    public Button backButton;
    public TextMeshProUGUI selectedLevelText;

    [Header("Настройки цветов")]
    public Color unlockedColor = new Color(0.3f, 0.7f, 0.3f);    // Зелёный - доступен
    public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Серый - заблокирован
    public Color completedColor = new Color(0.2f, 0.5f, 0.2f);    // Тёмно-зелёный - пройден
    public Color selectedColor = new Color(1f, 0.8f, 0.2f);       // Золотой - выбран

    [Header("Настройки")]
    public int totalLevels = 5;  // Общее количество уровней

    private int selectedLevel = 1;
    private List<Button> levelButtons = new List<Button>();
    private List<Image> buttonImages = new List<Image>();
    private List<TextMeshProUGUI> buttonTexts = new List<TextMeshProUGUI>();

    void Start()
    {
        // Загружаем сохранённый прогресс
        /*LoadProgress();

        // Назначаем обработчики
        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartGame);

        if (backButton != null)
            backButton.onClick.AddListener(ClosePanel);

        // Создаём кнопки уровней
        CreateLevelButtons();

        // Обновляем UI
        UpdateSelectedLevelDisplay();
        UpdateStartButtonState();*/

        StartGameDirectly();
    }

    // Сразу запустить игру
    void StartGameDirectly()
    {
        PlayerPrefs.SetInt("SelectedLevel", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Game");
    }

    void LoadProgress()
    {
        // Загружаем, какие уровни пройдены
        for (int i = 1; i <= totalLevels; i++)
        {
            bool isCompleted = PlayerPrefs.GetInt($"Level_{i}_Completed", 0) == 1;
            if (isCompleted)
            {
                Debug.Log($"Level {i} completed");
            }
        }
    }

    void CreateLevelButtons()
    {
        // Очищаем старые кнопки
        foreach (Button btn in levelButtons)
        {
            DestroyImmediate(btn.gameObject);
        }
        levelButtons.Clear();
        buttonImages.Clear();
        buttonTexts.Clear();

        // Проверяем, что префаб назначен
        if (levelButtonPrefab == null)
        {
            Debug.LogError("❌ Level Button Prefab is NULL! Assign it in the inspector!");
            return;
        }

        // Проверяем, что контейнер назначен
        if (buttonsContainer == null)
        {
            Debug.LogError("❌ Buttons Container is NULL! Assign it in the inspector!");
            return;
        }

        Debug.Log($"Creating {totalLevels} level buttons...");

        // Создаём кнопку для каждого уровня
        for (int i = 1; i <= totalLevels; i++)
        {
            int levelNumber = i;
            bool isUnlocked = IsLevelUnlocked(levelNumber);
            bool isCompleted = IsLevelCompleted(levelNumber);

            // Создаём кнопку из префаба
            GameObject buttonObj = Instantiate(levelButtonPrefab, buttonsContainer);
            buttonObj.name = $"LevelButton_{levelNumber}";

            Button button = buttonObj.GetComponent<Button>();
            Image buttonImage = buttonObj.GetComponent<Image>();

            // НАСТРАИВАЕМ ТЕКСТ - ЭТОТ КОД ДОЛЖЕН БЫТЬ!
            TextMeshProUGUI textTMP = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (textTMP != null)
            {
                textTMP.text = $"{levelNumber}";
                textTMP.fontSize = 36;
                textTMP.alignment = TextAlignmentOptions.Center;
                Debug.Log($"✅ Button {levelNumber} text set to: '{textTMP.text}'");
                buttonTexts.Add(textTMP);
            }
            else
            {
                Debug.LogError($"❌ Button {levelNumber}: No TextMeshProUGUI found! Please add TMP Text to the button prefab.");
            }

            // Настраиваем цвет и состояние кнопки
            if (!isUnlocked)
            {
                button.interactable = false;
                if (buttonImage != null) buttonImage.color = lockedColor;
            }
            else if (isCompleted)
            {
                button.interactable = true;
                if (buttonImage != null) buttonImage.color = completedColor;
            }
            else
            {
                button.interactable = true;
                if (buttonImage != null) buttonImage.color = unlockedColor;
            }

            // Добавляем обработчик нажатия
            int capturedLevel = levelNumber;
            button.onClick.AddListener(() => SelectLevel(capturedLevel));

            levelButtons.Add(button);
            buttonImages.Add(buttonImage);
        }

        Debug.Log($"✅ Created {levelButtons.Count} buttons");

        // Выбираем первый доступный уровень по умолчанию
        SelectFirstAvailableLevel();
    }

    void SelectFirstAvailableLevel()
    {
        for (int i = 1; i <= totalLevels; i++)
        {
            if (IsLevelUnlocked(i))
            {
                SelectLevel(i);
                break;
            }
        }
    }

    void SelectLevel(int levelNumber)
    {
        selectedLevel = levelNumber;

        // Обновляем визуальное выделение
        UpdateLevelSelectionHighlight();

        // Обновляем текст выбранного уровня
        UpdateSelectedLevelDisplay();

        // Обновляем состояние кнопки старта
        UpdateStartButtonState();

        Debug.Log($"🎮 Selected level: {selectedLevel}");
    }

    void UpdateLevelSelectionHighlight()
    {
        for (int i = 0; i < levelButtons.Count; i++)
        {
            int levelNumber = i + 1;
            Image img = buttonImages[i];

            if (img == null) continue;

            if (levelNumber == selectedLevel)
            {
                // Выделяем выбранный уровень золотым
                img.color = selectedColor;

                // Добавляем обводку (если есть Outline компонент)
                Outline outline = img.GetComponent<Outline>();
                if (outline == null)
                    outline = img.gameObject.AddComponent<Outline>();
                outline.effectColor = Color.yellow;
                outline.effectDistance = new Vector2(3, 3);
            }
            else
            {
                // Возвращаем обычный цвет
                if (IsLevelCompleted(levelNumber))
                    img.color = completedColor;
                else if (IsLevelUnlocked(levelNumber))
                    img.color = unlockedColor;
                else
                    img.color = lockedColor;

                // Убираем обводку
                Outline outline = img.GetComponent<Outline>();
                if (outline != null)
                    outline.effectDistance = Vector2.zero;
            }
        }
    }

    void UpdateSelectedLevelDisplay()
    {
        if (selectedLevelText != null)
        {
            string status = IsLevelCompleted(selectedLevel) ? " (пройден)" : "";
            selectedLevelText.text = $"Выбран: Уровень {selectedLevel}{status}";
        }
    }

    void UpdateStartButtonState()
    {
        if (startGameButton != null)
        {
            // Кнопка старта активна только если выбран доступный уровень
            startGameButton.interactable = IsLevelUnlocked(selectedLevel);
        }
    }

    bool IsLevelUnlocked(int levelNumber)
    {
        // Уровень 1 всегда открыт
        if (levelNumber == 1) return true;

        // Остальные уровни открываются после прохождения предыдущего
        return IsLevelCompleted(levelNumber - 1);
    }

    bool IsLevelCompleted(int levelNumber)
    {
        return PlayerPrefs.GetInt($"Level_{levelNumber}_Completed", 0) == 1;
    }

    void StartGame()
    {
        if (!IsLevelUnlocked(selectedLevel))
        {
            Debug.LogWarning("This level is locked!");
            return;
        }

        // Сохраняем выбранный уровень
        PlayerPrefs.SetInt("SelectedLevel", selectedLevel);
        PlayerPrefs.Save();

        // Воспроизводим звук кнопки
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null) audio.Play();

        // Загружаем игровую сцену
        SceneManager.LoadScene("Game");
    }

    void ClosePanel()
    {
        // Находим MainMenu и закрываем панель выбора уровней
        MainMenu mainMenu = FindObjectOfType<MainMenu>();
        if (mainMenu != null)
        {
            mainMenu.CloseLevelSelect();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // Метод для обновления панели (вызывается при открытии)
    public void RefreshPanel()
    {
        LoadProgress();
        CreateLevelButtons();
        UpdateSelectedLevelDisplay();
        UpdateStartButtonState();
    }
}