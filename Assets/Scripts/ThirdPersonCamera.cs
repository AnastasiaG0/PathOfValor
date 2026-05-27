using UnityEngine;

/*public class TopDownLimitedCamera : MonoBehaviour
{
    [Header("Цель")]
    public Transform target;

    [Header("Смещение")]
    public Vector3 offset = new Vector3(0, 8, -5);

    [Header("Ограничение обзора")]
    public float maxVisibleRadius = 7f;  // Радиус видимости вокруг игрока
    public LayerMask wallLayer;

    [Header("Сглаживание")]
    public float smoothSpeed = 5f;

    private Material fogMaterial;

    void Start()
    {
        // Создаём материал для эффекта "тумана войны" (простым способом)
        RenderSettings.fog = true;
        RenderSettings.fogDensity = 0.03f;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
            else
                return;
        }

        // Камера следует за игроком с плавностью
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // Камера смотрит на игрока
        transform.LookAt(target);
    }

    // Опционально: скрытие стен за пределами видимости
    void Update()
    {
        HideDistantWalls();
    }

    void HideDistantWalls()
    {
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        foreach (GameObject wall in walls)
        {
            float distance = Vector3.Distance(wall.transform.position, target.position);
            Renderer renderer = wall.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Стены далеко от игрока становятся прозрачными
                Color color = renderer.material.color;
                float alpha = Mathf.Clamp01(1 - (distance - maxVisibleRadius) / 3f);
                if (distance > maxVisibleRadius)
                    alpha = 0;
                renderer.material.color = new Color(color.r, color.g, color.b, alpha);
            }
        }
    }
}*/






public class TopDownLimitedCamera : MonoBehaviour
{
    [Header("Целевой игрок")]
    public Transform target;

    [Header("Настройки позиции (фиксированное смещение)")]
    public Vector3 offset = new Vector3(0, 8, -6);  // Смещение относительно игрока

    [Header("Сглаживание (плавное следование)")]
    public float smoothSpeed = 0.1f;   // Плавность следования (меньше = плавнее)

    [Header("Ограничение обзора")]
    public float maxVisibleRadius = 7f;
    public LayerMask wallLayer;

    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        // Настройка тумана
        RenderSettings.fog = true;
        RenderSettings.fogDensity = 0.03f;
        RenderSettings.fogColor = new Color(0.15f, 0.15f, 0.2f);

        // Если цель не назначена, найдём игрока
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

        // Желаемая позиция = позиция игрока + фиксированное смещение
        Vector3 desiredPosition = target.position + offset;

        // Плавное движение камеры к желаемой позиции
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);

        // Камера НЕ ПОВОРАЧИВАЕТСЯ - смотрит строго в одном направлении
        // (оставляем текущий поворот камеры без изменений)
    }

    void Update()
    {
        HideDistantWalls();
    }

    void HideDistantWalls()
    {
        if (target == null) return;

        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        foreach (GameObject wall in walls)
        {
            if (wall == null) continue;

            float distance = Vector3.Distance(wall.transform.position, target.position);
            Renderer renderer = wall.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color color = renderer.material.color;
                float alpha = Mathf.Clamp01(1 - (distance - maxVisibleRadius) / 3f);
                if (distance > maxVisibleRadius)
                    alpha = 0;
                renderer.material.color = new Color(color.r, color.g, color.b, alpha);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.green;
            Vector3 cameraPos = target.position + offset;
            Gizmos.DrawWireSphere(cameraPos, 0.5f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position, maxVisibleRadius);
        }
    }
}













// ПРАВИЛЬНЫЙ, НО НЕ ЗА СПИНОЙ




/*public class TopDownLimitedCamera : MonoBehaviour
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
    //public LayerMask obstacleLayer;      // Слои, которые блокируют обзор (стены)

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
        //desiredPosition = CheckObstacles(desiredPosition);

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

    /*Vector3 CheckObstacles(Vector3 desiredPosition)
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
    }*-/

    // Визуализация луча в редакторе
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(target.position, (target.position + offset - target.position).normalized * maxDistance);
        }
    }
}*/










//КАМЕРА ВСЕГДА ЗА СПИНОЙ




/*public class TopDownLimitedCamera : MonoBehaviour
{
    [Header("Целевой игрок")]
    public Transform target;

    [Header("Настройки камеры от третьего лица")]
    public float distanceFromPlayer = 4f;   // Расстояние за спиной
    public float heightAbovePlayer = 2f;    // Высота над игроком
    public float followSpeed = 10f;         // Скорость следования (увеличена)
    public float rotationSpeed = 15f;       // Скорость поворота камеры (НОВАЯ)

    [Header("Ограничение обзора")]
    public float maxVisibleRadius = 7f;
    public LayerMask wallLayer;

    private Vector3 velocity = Vector3.zero;
    private PlayerController playerController;
    private Quaternion targetRotation;

    void Start()
    {
        // Настройка тумана
        RenderSettings.fog = true;
        RenderSettings.fogDensity = 0.03f;
        RenderSettings.fogColor = new Color(0.15f, 0.15f, 0.2f);
    }

    void LateUpdate()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                playerController = player.GetComponent<PlayerController>();
            }
            else
                return;
        }

        // Получаем направление, куда смотрит персонаж
        Vector3 playerForward = target.forward;
        
        // Если персонаж стоит (forward не изменился), используем его текущий поворот
        // Для этого просто берём forward, который уже установлен поворотом персонажа
        
        // Позиция камеры: ЗА СПИНОЙ персонажа (используем forward персонажа)
        Vector3 cameraOffset = -playerForward * distanceFromPlayer;
        cameraOffset.y = heightAbovePlayer;
        
        Vector3 desiredPosition = target.position + cameraOffset;
        
        // Плавное движение камеры (ускорено)
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, 0.2f, followSpeed);
        
        // БЫСТРЫЙ ПОВОРОТ КАМЕРЫ К ИГРОКУ
        Vector3 lookDirection = target.position - transform.position;
        targetRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void Update()
    {
        HideDistantWalls();
    }

    void HideDistantWalls()
    {
        if (target == null) return;
        
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        foreach (GameObject wall in walls)
        {
            if (wall == null) continue;
            
            float distance = Vector3.Distance(wall.transform.position, target.position);
            Renderer renderer = wall.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color color = renderer.material.color;
                float alpha = Mathf.Clamp01(1 - (distance - maxVisibleRadius) / 3f);
                if (distance > maxVisibleRadius)
                    alpha = 0;
                renderer.material.color = new Color(color.r, color.g, color.b, alpha);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.green;
            Vector3 cameraPos = target.position - target.forward * distanceFromPlayer;
            cameraPos.y += heightAbovePlayer;
            Gizmos.DrawWireSphere(cameraPos, 0.3f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position, maxVisibleRadius);
        }
    }
}*/