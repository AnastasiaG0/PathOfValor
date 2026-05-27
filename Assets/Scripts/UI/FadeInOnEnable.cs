using UnityEngine;
using System.Collections;

public class FadeInOnEnable : MonoBehaviour
{
    [Header("Настройки анимации")]
    public float fadeInDuration = 0.5f;      // Длительность появления
    public float scaleDuration = 0.3f;       // Длительность увеличения
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 originalScale;
    
    void Awake()
    {
        // Получаем компоненты
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        
        // Изначально скрыты
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        rectTransform.localScale = Vector3.zero;
    }
    
    void OnEnable()
    {
        // Запускаем анимацию при включении панели
        StartCoroutine(PlayFadeInAnimation());
    }
    
    IEnumerator PlayFadeInAnimation()
    {
        float timer = 0;
        
        // Анимация появления (fade in)
        while (timer < fadeInDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / fadeInDuration;
            float easedProgress = fadeCurve.Evaluate(progress);
            
            canvasGroup.alpha = easedProgress;
            
            yield return null;
        }
        
        canvasGroup.alpha = 1;
        
        // Анимация увеличения (scale up)
        timer = 0;
        while (timer < scaleDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / scaleDuration;
            float scale = Mathf.Lerp(0, originalScale.x, progress);
            
            rectTransform.localScale = new Vector3(scale, scale, scale);
            
            yield return null;
        }
        
        rectTransform.localScale = originalScale;
        
        // Включаем взаимодействие
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
    
    void OnDisable()
    {
        // Сброс состояния при выключении
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        if (rectTransform != null)
            rectTransform.localScale = Vector3.zero;
        
        // Останавливаем все корутины
        StopAllCoroutines();
    }
}