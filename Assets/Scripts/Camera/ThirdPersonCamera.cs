using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Целевой игрок")]
    public Transform target;           // Игрок, за которым следит камера

    [Header("Настройки позиции")]
    public Vector3 offset = new Vector3(0, 3, -5);  // Смещение относительно игрока
    public float followSpeed = 5f;      // Скорость следования

    [Header("Настройки поворота")]
    public float rotationSpeed = 3f;    // Скорость поворота камеры
    public bool rotateWithPlayer = true; // Поворачиваться ли за игроком

    [Header("Ограничения обзора")]
    public float maxDistance = 8f;       // Максимальная дистанция от игрока
    public float minDistance = 2f;       // Минимальная дистанция от игрока
    public LayerMask obstacleLayer;      // Слои, которые блокируют обзор (стены)

    [Header("Сглаживание")]
    public float smoothTime = 0.3f;      // Время сглаживания
    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null)
        {
            // Поиск игрока, если не назначен
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
            else
                return;
        }

        // Расчёт желаемой позиции камеры
        Vector3 desiredPosition = target.position + offset;

        // Проверка на препятствия
        desiredPosition = CheckObstacles(desiredPosition);

        // Плавное движение камеры
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

        // Поворот камеры к игроку
        if (rotateWithPlayer)
        {
            Vector3 lookDirection = target.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    Vector3 CheckObstacles(Vector3 desiredPosition)
    {
        Vector3 direction = desiredPosition - target.position;
        float distance = direction.magnitude;

        // Ограничиваем дистанцию
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        RaycastHit hit;
        if (Physics.Raycast(target.position, direction.normalized, out hit, distance, obstacleLayer))
        {
            // Если есть препятствие, ставим камеру перед ним
            return hit.point - direction.normalized * 0.3f;
        }

        return target.position + direction.normalized * distance;
    }

    // Визуализация луча в редакторе
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(target.position, (target.position + offset - target.position).normalized * maxDistance);
        }
    }
}