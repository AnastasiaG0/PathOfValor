using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("Панели меню")]
    public GameObject mainPanel;
    public GameObject controlsPanel;
    public GameObject settingsPanel;
    public GameObject levelSelectPanel;  // Панель выбора уровней

    [Header("Кнопки")]
    public Button startButton;
    public Button controlsButton;
    public Button settingsButton;
    public Button quitButton;
    public Button backButton;

    [Header("Настройки анимации")]
    public float fadeInDuration = 0.8f;
    public float buttonDelay = 0.1f;
    public float scaleAnimationDuration = 0.3f;
    public float panelTransitionDuration = 0.3f;

    [Header("Аудио")]
    public AudioClip buttonClickSound;
    private AudioSource audioSource;

    private CanvasGroup mainPanelGroup;
    private Button[] menuButtons;
    private Vector3[] originalButtonScales;
    private LevelSelectUI levelSelectUI;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (levelSelectPanel != null)
            levelSelectUI = levelSelectPanel.GetComponent<LevelSelectUI>();

        SetupCanvasGroups();
        SetupButtons();
        StartCoroutine(AnimateMenuAppearance());
        AddHoverEventsToButtons();

        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(false);
    }

    void SetupCanvasGroups()
    {
        if (mainPanel != null)
        {
            mainPanelGroup = mainPanel.GetComponent<CanvasGroup>();
            if (mainPanelGroup == null)
                mainPanelGroup = mainPanel.AddComponent<CanvasGroup>();
            mainPanelGroup.alpha = 0;
            mainPanelGroup.interactable = false;
            mainPanelGroup.blocksRaycasts = false;
        }

        if (controlsPanel != null)
        {
            CanvasGroup controlsGroup = controlsPanel.GetComponent<CanvasGroup>();
            if (controlsGroup == null)
                controlsGroup = controlsPanel.AddComponent<CanvasGroup>();
            controlsGroup.alpha = 0;
            controlsPanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            CanvasGroup settingsGroup = settingsPanel.GetComponent<CanvasGroup>();
            if (settingsGroup == null)
                settingsGroup = settingsPanel.AddComponent<CanvasGroup>();
            settingsGroup.alpha = 0;
            settingsPanel.SetActive(false);
        }

        if (levelSelectPanel != null)
        {
            CanvasGroup levelGroup = levelSelectPanel.GetComponent<CanvasGroup>();
            if (levelGroup == null)
                levelGroup = levelSelectPanel.AddComponent<CanvasGroup>();
            levelGroup.alpha = 0;
            levelSelectPanel.SetActive(false);
        }
    }

    void SetupButtons()
    {
        if (mainPanel != null)
        {
            menuButtons = mainPanel.GetComponentsInChildren<Button>();
            originalButtonScales = new Vector3[menuButtons.Length];
            for (int i = 0; i < menuButtons.Length; i++)
            {
                if (menuButtons[i] != null)
                {
                    originalButtonScales[i] = menuButtons[i].transform.localScale;
                    menuButtons[i].transform.localScale = Vector3.zero;
                }
            }
        }

        if (startButton != null)
        {
            startButton.onClick.AddListener(() => ShowLevelSelect());
            // Для одного уровня !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            startButton.onClick.AddListener(() => StartGameWithLevel1());
        }


        if (controlsButton != null)
            controlsButton.onClick.AddListener(() => StartCoroutine(ShowControlsWithAnimation()));

        if (settingsButton != null)
            settingsButton.onClick.AddListener(() => StartCoroutine(ShowSettingsWithAnimation()));

        if (quitButton != null)
            quitButton.onClick.AddListener(() => StartCoroutine(QuitGameWithAnimation()));

        if (backButton != null)
            backButton.onClick.AddListener(() => StartCoroutine(BackToMenuWithAnimation()));
    }

    // Для одного уровня !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    void StartGameWithLevel1()
    {
        PlayClickSound();
        PlayerPrefs.SetInt("SelectedLevel", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Game");
    }

    public void ShowLevelSelect()
    {
        PlayClickSound();
        StartCoroutine(ShowLevelSelectWithAnimation());
    }

    IEnumerator ShowLevelSelectWithAnimation()
    {
        if (levelSelectPanel == null) yield break;

        if (levelSelectUI != null)
            levelSelectUI.RefreshPanel();

        if (mainPanel != null)
        {
            CanvasGroup fromGroup = mainPanel.GetComponent<CanvasGroup>();
            if (fromGroup == null)
                fromGroup = mainPanel.AddComponent<CanvasGroup>();

            float elapsed = 0;
            float duration = panelTransitionDuration;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1, 0, elapsed / duration);
                fromGroup.alpha = alpha;
                yield return null;
            }
            fromGroup.alpha = 0;
            mainPanel.SetActive(false);
        }

        levelSelectPanel.SetActive(true);
        CanvasGroup toGroup = levelSelectPanel.GetComponent<CanvasGroup>();
        if (toGroup == null)
            toGroup = levelSelectPanel.AddComponent<CanvasGroup>();

        toGroup.alpha = 0;
        toGroup.interactable = false;

        float elapsed2 = 0;
        float duration2 = panelTransitionDuration;

        while (elapsed2 < duration2)
        {
            elapsed2 += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, elapsed2 / duration2);
            toGroup.alpha = alpha;
            yield return null;
        }

        toGroup.alpha = 1;
        toGroup.interactable = true;
        toGroup.blocksRaycasts = true;
    }

    public void CloseLevelSelect()
    {
        StartCoroutine(CloseLevelSelectWithAnimation());
    }

    IEnumerator CloseLevelSelectWithAnimation()
    {
        if (levelSelectPanel == null) yield break;

        CanvasGroup fromGroup = levelSelectPanel.GetComponent<CanvasGroup>();
        if (fromGroup == null)
            fromGroup = levelSelectPanel.AddComponent<CanvasGroup>();

        float elapsed = 0;
        float duration = panelTransitionDuration;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, elapsed / duration);
            fromGroup.alpha = alpha;
            yield return null;
        }

        fromGroup.alpha = 0;
        levelSelectPanel.SetActive(false);

        ShowMainPanel();
    }

    IEnumerator AnimateButtonScale(Transform buttonTransform, Vector3 targetScale, float duration)
    {
        Vector3 startScale = buttonTransform.localScale;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            buttonTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        buttonTransform.localScale = targetScale;
    }

    public void OnButtonHover(Button button, bool isHovering)
    {
        if (button == null) return;
        int index = System.Array.IndexOf(menuButtons, button);
        if (index < 0) return;
        Vector3 targetScale = isHovering ? originalButtonScales[index] * 1.1f : originalButtonScales[index];
        StopCoroutine("AnimateButtonScale");
        StartCoroutine(AnimateButtonScale(button.transform, targetScale, 0.15f));
    }

    IEnumerator AnimateButtonClick(Transform buttonTransform)
    {
        Vector3 originalScale = buttonTransform.localScale;
        buttonTransform.localScale = originalScale * 0.95f;
        yield return new WaitForSecondsRealtime(0.05f);
        buttonTransform.localScale = originalScale;
    }

    void AddHoverEventsToButtons()
    {
        foreach (Button button in menuButtons)
        {
            if (button == null) continue;
            var trigger = button.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (trigger == null)
                trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            trigger.triggers.Clear();

            var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
            enterEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => OnButtonHover(button, true));
            trigger.triggers.Add(enterEntry);

            var exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
            exitEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => OnButtonHover(button, false));
            trigger.triggers.Add(exitEntry);

            var clickEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
            clickEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerClick;
            clickEntry.callback.AddListener((data) => StartCoroutine(AnimateButtonClick(button.transform)));
            trigger.triggers.Add(clickEntry);
        }
    }

    IEnumerator AnimateMenuAppearance()
    {
        float elapsed = 0;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, elapsed / fadeInDuration);
            if (mainPanelGroup != null)
                mainPanelGroup.alpha = alpha;
            yield return null;
        }
        if (mainPanelGroup != null)
        {
            mainPanelGroup.alpha = 1;
            mainPanelGroup.interactable = true;
            mainPanelGroup.blocksRaycasts = true;
        }
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] != null && menuButtons[i].gameObject.activeSelf)
            {
                StartCoroutine(AnimateButtonAppearance(menuButtons[i].transform, i * buttonDelay));
            }
        }
    }

    IEnumerator AnimateButtonAppearance(Transform buttonTransform, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        float elapsed = 0;
        Vector3 startScale = Vector3.zero;
        int index = System.Array.IndexOf(menuButtons, buttonTransform.GetComponent<Button>());
        if (index < 0) yield break;
        Vector3 targetScale = originalButtonScales[index];
        while (elapsed < scaleAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / scaleAnimationDuration);
            buttonTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        buttonTransform.localScale = targetScale;
    }

    IEnumerator AnimatePanelTransition(GameObject fromPanel, GameObject toPanel)
    {
        CanvasGroup fromGroup = fromPanel.GetComponent<CanvasGroup>();
        CanvasGroup toGroup = toPanel.GetComponent<CanvasGroup>();
        if (fromGroup == null) fromGroup = fromPanel.AddComponent<CanvasGroup>();
        if (toGroup == null) toGroup = toPanel.AddComponent<CanvasGroup>();
        float elapsed = 0;
        float duration = panelTransitionDuration;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, elapsed / duration);
            fromGroup.alpha = alpha;
            yield return null;
        }
        fromGroup.alpha = 0;
        fromPanel.SetActive(false);
        toPanel.SetActive(true);
        toGroup.alpha = 0;
        toGroup.interactable = false;
        elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, elapsed / duration);
            toGroup.alpha = alpha;
            yield return null;
        }
        toGroup.alpha = 1;
        toGroup.interactable = true;
        toGroup.blocksRaycasts = true;
    }

    IEnumerator ShowControlsWithAnimation()
    {
        PlayClickSound();
        yield return StartCoroutine(AnimatePanelTransition(mainPanel, controlsPanel));
    }

    IEnumerator ShowSettingsWithAnimation()
    {
        PlayClickSound();
        if (settingsPanel != null)
            yield return StartCoroutine(AnimatePanelTransition(mainPanel, settingsPanel));
    }

    IEnumerator BackToMenuWithAnimation()
    {
        PlayClickSound();
        GameObject currentPanel = null;
        if (controlsPanel != null && controlsPanel.activeSelf)
            currentPanel = controlsPanel;
        else if (settingsPanel != null && settingsPanel.activeSelf)
            currentPanel = settingsPanel;
        else if (levelSelectPanel != null && levelSelectPanel.activeSelf)
            currentPanel = levelSelectPanel;
        if (currentPanel != null && mainPanel != null)
            yield return StartCoroutine(AnimatePanelTransition(currentPanel, mainPanel));
    }

    IEnumerator QuitGameWithAnimation()
    {
        PlayClickSound();
        if (mainPanelGroup != null)
        {
            float elapsed = 0;
            float fadeOutDuration = 0.5f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1, 0, elapsed / fadeOutDuration);
                mainPanelGroup.alpha = alpha;
                yield return null;
            }
        }
        Debug.Log("Выход из игры...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    IEnumerator StartGameWithDelay()
    {
        PlayClickSound();
        float elapsed = 0;
        float fadeOutDuration = 0.5f;
        if (mainPanelGroup != null)
        {
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1, 0, elapsed / fadeOutDuration);
                mainPanelGroup.alpha = alpha;
                yield return null;
            }
        }
        SceneManager.LoadScene("Game");
    }

    public void StartGame()
    {
        StartCoroutine(StartGameWithDelay());
    }

    public void ShowControls()
    {
        StartCoroutine(ShowControlsWithAnimation());
    }

    public void ShowMainPanel()
    {
        StartCoroutine(ShowMainPanelWithAnimation());
    }

    IEnumerator ShowMainPanelWithAnimation()
    {
        if (mainPanel == null) yield break;
        GameObject currentPanel = null;
        if (controlsPanel != null && controlsPanel.activeSelf)
            currentPanel = controlsPanel;
        else if (settingsPanel != null && settingsPanel.activeSelf)
            currentPanel = settingsPanel;
        else if (levelSelectPanel != null && levelSelectPanel.activeSelf)
            currentPanel = levelSelectPanel;
        if (currentPanel != null)
        {
            CanvasGroup fromGroup = currentPanel.GetComponent<CanvasGroup>();
            if (fromGroup == null) fromGroup = currentPanel.AddComponent<CanvasGroup>();
            float elapsed = 0;
            float duration = panelTransitionDuration;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1, 0, elapsed / duration);
                fromGroup.alpha = alpha;
                yield return null;
            }
            fromGroup.alpha = 0;
            currentPanel.SetActive(false);
        }
        mainPanel.SetActive(true);
        CanvasGroup toGroup = mainPanel.GetComponent<CanvasGroup>();
        if (toGroup == null) toGroup = mainPanel.AddComponent<CanvasGroup>();
        toGroup.alpha = 0;
        toGroup.interactable = false;
        float elapsed2 = 0;
        while (elapsed2 < fadeInDuration)
        {
            elapsed2 += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, elapsed2 / fadeInDuration);
            toGroup.alpha = alpha;
            yield return null;
        }
        toGroup.alpha = 1;
        toGroup.interactable = true;
        toGroup.blocksRaycasts = true;
    }

    public void BackToMenu()
    {
        StartCoroutine(BackToMenuWithAnimation());
    }

    public void QuitGame()
    {
        StartCoroutine(QuitGameWithAnimation());
    }

    void PlayClickSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
}