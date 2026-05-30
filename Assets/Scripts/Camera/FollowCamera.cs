using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    /*[Header("Цель (игрок)")]
    public Transform target;              // Игрок, за которым следит камера

    [Header("Расстояние от игрока")]
    public float distanceFromPlayer = 4f;  // Дистанция позади игрока
    public float heightAbovePlayer = 2f;   // Высота над игроком

    [Header("Сглаживание")]
    public float smoothSpeed = 5f;         // Скорость следования
    public float rotationSmoothSpeed = 8f; // Скорость поворота

    [Header("Ограничения")]
    public LayerMask obstacleLayer;        // Слои, которые блокируют камеру
    public float minDistance = 1.5f;       // Минимальная дистанция при препятствии

    [Header("Поворот камеры")]
    public bool rotateWithPlayer = true;   // Поворачиваться за игроком
    public float cameraTiltAngle = 15f;    // Наклон камеры вниз

    private Vector3 currentVelocity;
    private Quaternion targetRotation;

    void Start()
    {
        // Если цель не назначена, ищем игрока
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Получаем направление, куда смотрит игрок
        Vector3 playerForward = target.forward;

        // Вычисляем желаемую позицию камеры (позади игрока)
        Vector3 desiredPosition = target.position
            - playerForward * distanceFromPlayer
            + Vector3.up * heightAbovePlayer;

        // Проверяем, не упирается ли камера в стену
        desiredPosition = CheckCameraCollision(desiredPosition);

        // Плавно перемещаем камеру в желаемую позицию
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Настраиваем поворот камеры
        if (rotateWithPlayer)
        {
            // Камера смотрит на игрока
            Vector3 lookDirection = target.position - transform.position;
            targetRotation = Quaternion.LookRotation(lookDirection);

            // Добавляем небольшой наклон вниз (чтобы лучше видеть пол)
            Quaternion tilt = Quaternion.Euler(cameraTiltAngle, 0, 0);
            targetRotation = targetRotation * tilt;

            // Плавный поворот камеры
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
        }
    }

    Vector3 CheckCameraCollision(Vector3 desiredPosition)
    {
        // Направление от игрока к желаемой позиции камеры
        Vector3 direction = desiredPosition - target.position;
        float distance = direction.magnitude;

        RaycastHit hit;
        // Проверяем, нет ли стены между камерой и игроком
        if (Physics.Raycast(target.position, direction.normalized, out hit, distance, obstacleLayer))
        {
            // Если есть стена, ставим камеру перед стеной
            float collisionDistance = hit.distance - 0.3f;
            collisionDistance = Mathf.Max(collisionDistance, minDistance);
            return target.position + direction.normalized * collisionDistance;
        }

        return desiredPosition;
    }

    // Для отладки - визуализируем луч в редакторе
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.green;
            Vector3 camPos = target.position - target.forward * distanceFromPlayer + Vector3.up * heightAbovePlayer;
            Gizmos.DrawWireSphere(camPos, 0.3f);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(target.position, camPos);
        }
    }*/
}
