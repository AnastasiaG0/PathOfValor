using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Настройки следования")]
    public Transform target;              // Цель (персонаж)
    public Vector3 offset = new Vector3(0, 1.5f, -3f); // Смещение камеры
    public float followSpeed = 10f;       // Скорость следования

    [Header("Настройки поворота мыши")]
    public float mouseSensitivity = 2f;   // Чувствительность мыши
    public float maxVerticalAngle = 60f;  // Максимальный угол поворота вверх
    public float minVerticalAngle = -40f; // Минимальный угол поворота вниз

    private float horizontalAngle = 0f;   // Горизонтальный угол
    private float verticalAngle = 20f;    // Вертикальный угол

    void Start()
    {
        // Блокируем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Ищем персонажа
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        // Начальная позиция камеры
        UpdateCameraPosition();
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Ввод мыши
        if (!IsGamePaused())
        {
            horizontalAngle += Input.GetAxis("Mouse X") * mouseSensitivity;
            verticalAngle -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);
        }

        // Обновляем позицию камеры
        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        // ВАЖНО: Создаём вращение ТОЛЬКО по горизонтали (Y) и вертикали (X)
        // НИКАКОГО вращения по Z (крена) - это сохраняет горизонт ровным!
        Quaternion rotation = Quaternion.Euler(verticalAngle, horizontalAngle, 0);

        // Вычисляем позицию: позиция персонажа + поворот * смещение
        Vector3 desiredPosition = target.position + rotation * offset;

        // Мгновенное перемещение (без плавности, чтобы не было "пьяного" эффекта)
        transform.position = desiredPosition;

        // Камера всегда смотрит на персонажа (это сохраняет горизонт ровным)
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }

    private bool IsGamePaused()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return true;
        return false;
    }

    public void SetCursorLock(bool locked)
    {
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ResetCameraAngles()
    {
        horizontalAngle = 0f;
        verticalAngle = 20f;
        UpdateCameraPosition();
    }
}