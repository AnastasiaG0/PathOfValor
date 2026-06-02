using UnityEngine;
using System.Collections;

public class CubeRotator : MonoBehaviour
{
    [Header("Настройки вращения")]
    public float rotationSpeed = 30f;

    [Header("Локальная позиция (над головой врага)")]
    public Vector3 localPosition = new Vector3(0, 1.5f, 0);

    [Header("Эффект лопания")]
    public float growDuration = 0.15f;
    public float shrinkDuration = 0.1f;
    public float growScale = 1.5f;

    private Vector3 originalScale;
    private bool isPositionSet = false;
    private bool isExploding = false;

    private CanvasGroup[] canvasGroups;
    private Renderer cubeRenderer;

    void Start()
    {
        transform.localPosition = localPosition;
        originalScale = transform.localScale;
        isPositionSet = true;

        canvasGroups = GetComponentsInChildren<CanvasGroup>();
        cubeRenderer = GetComponent<Renderer>();

        // Изначально куб прозрачный и выключен
        SetAlpha(0);
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (isExploding) return;

        transform.Rotate(transform.up, rotationSpeed * Time.deltaTime);
    }

    void LateUpdate()
    {
        if (isExploding) return;

        if (isPositionSet)
        {
            transform.localPosition = localPosition;
        }
    }

    /// <summary>
    /// Мгновенное появление куба
    /// </summary>
    public void FadeIn()
    {
        if (isExploding) return;

        gameObject.SetActive(true);
        SetAlpha(1);
    }

    /// <summary>
    /// Мгновенное исчезновение куба
    /// </summary>
    public void FadeOut()
    {
        if (isExploding) return;

        SetAlpha(0);
        gameObject.SetActive(false);
    }

    private void SetAlpha(float alpha)
    {
        if (canvasGroups != null)
        {
            foreach (CanvasGroup cg in canvasGroups)
            {
                if (cg != null) cg.alpha = alpha;
            }
        }

        if (cubeRenderer != null)
        {
            Material[] materials = cubeRenderer.materials;
            foreach (Material mat in materials)
            {
                if (mat != null && mat.HasProperty("_Color"))
                {
                    Color color = mat.color;
                    color.a = alpha;
                    mat.color = color;
                }
            }
        }
    }

    public void Pop(System.Action onComplete = null)
    {
        if (isExploding) return;
        StartCoroutine(PopCoroutine(onComplete));
    }

    private IEnumerator PopCoroutine(System.Action onComplete)
    {
        isExploding = true;

        // Скрываем UI
        if (canvasGroups != null)
        {
            foreach (CanvasGroup cg in canvasGroups)
            {
                if (cg != null) cg.alpha = 0;
            }
        }

        // Увеличиваемся
        float elapsed = 0;
        Vector3 targetScale = originalScale * growScale;
        Vector3 startScale = transform.localScale;

        while (elapsed < growDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / growDuration;
            float easedT = Mathf.SmoothStep(0, 1, t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, easedT);
            yield return null;
        }

        transform.localScale = targetScale;

        // Уменьшаемся до нуля
        elapsed = 0;
        startScale = transform.localScale;

        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shrinkDuration;
            float easedT = Mathf.SmoothStep(0, 1, t);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, easedT);
            yield return null;
        }

        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);

        onComplete?.Invoke();
        Destroy(gameObject, 0.1f);
    }

    public void SetLocalPosition(Vector3 newPosition)
    {
        localPosition = newPosition;
        transform.localPosition = localPosition;
    }
}