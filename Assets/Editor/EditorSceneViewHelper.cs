#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class EditorSceneViewHelper
{
    private static GameObject observerCamera;
    private static bool isObserverActive = false;
    
    [MenuItem("Tools/Observer Camera/Activate %#o")]  // Ctrl+Shift+O
    public static void ActivateObserverCamera()
    {
        if (observerCamera == null)
        {
            // Создаём камеру-наблюдателя
            observerCamera = new GameObject("EditorObserverCamera");
            Camera cam = observerCamera.AddComponent<Camera>();
            cam.depth = 100;
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.fieldOfView = 60;
            
            // Добавляем контроллер для камеры
            observerCamera.AddComponent<ObserverCameraController>();
        }
        
        observerCamera.SetActive(true);
        isObserverActive = true;
        
        // Отключаем все другие камеры
        Camera[] allCameras = GameObject.FindObjectsOfType<Camera>();
        foreach (Camera cam in allCameras)
        {
            if (cam.gameObject != observerCamera)
            {
                cam.enabled = false;
                AudioListener listener = cam.GetComponent<AudioListener>();
                if (listener != null)
                    listener.enabled = false;
            }
        }
        
        // Включаем AudioListener на камере-наблюдателе
        AudioListener observerListener = observerCamera.GetComponent<AudioListener>();
        if (observerListener == null)
            observerListener = observerCamera.AddComponent<AudioListener>();
        observerListener.enabled = true;
            
        Debug.Log("Камера-наблюдатель АКТИВИРОВАНА (перетаскивание мышью)");
    }
    
    [MenuItem("Tools/Observer Camera/Deactivate %#p")]  // Ctrl+Shift+P
    public static void DeactivateObserverCamera()
    {
        if (observerCamera != null)
        {
            observerCamera.SetActive(false);
        }
        
        isObserverActive = false;
        
        // Включаем обратно основную камеру
        Camera[] allCameras = GameObject.FindObjectsOfType<Camera>();
        foreach (Camera cam in allCameras)
        {
            if (cam.gameObject != observerCamera && cam.gameObject.name != "EditorObserverCamera")
            {
                cam.enabled = true;
                AudioListener listener = cam.GetComponent<AudioListener>();
                if (listener != null)
                    listener.enabled = true;
                break;
            }
        }
            
        Debug.Log("Камера-наблюдатель ДЕАКТИВИРОВАНА");
    }
    
    [MenuItem("Tools/Scene Camera/Top View %t")]  // Ctrl+T
    public static void SetTopView()
    {
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.orthographic = true;
            SceneView.lastActiveSceneView.LookAt(
                SceneView.lastActiveSceneView.pivot,
                Quaternion.Euler(90, 0, 0),
                20f
            );
            SceneView.lastActiveSceneView.Repaint();
            Debug.Log("Scene View переключена на вид сверху");
        }
    }
    
    [MenuItem("Tools/Scene Camera/Reset View %r")]  // Ctrl+R
    public static void ResetView()
    {
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.orthographic = false;
            SceneView.lastActiveSceneView.LookAt(
                Vector3.zero,
                Quaternion.Euler(45, 0, 0),
                20f
            );
            SceneView.lastActiveSceneView.Repaint();
            Debug.Log("Scene View сброшена");
        }
    }
}

public class ObserverCameraController : MonoBehaviour
{
    [Header("Настройки движения мышью")]
    public float dragSpeed = 0.5f;
    public float scrollSpeed = 10f;
    public float rotationSpeed = 2f;
    
    [Header("Ограничения")]
    public float minZoom = 5f;
    public float maxZoom = 30f;
    public float minX = -25f;
    public float maxX = 25f;
    public float minZ = -25f;
    public float maxZ = 25f;
    
    private Vector3 dragOrigin;
    private Vector3 cameraTargetPosition;
    private bool isDragging = false;
    private Vector2 rotation = Vector2.zero;
    private float currentZoom = 15f;
    private Camera thisCamera;
    
    void Start()
    {
        thisCamera = GetComponent<Camera>();
        
        // Установка начальной позиции под 45 градусов
        transform.position = new Vector3(0, 15, -20);
        transform.rotation = Quaternion.Euler(35, 0, 0);
        cameraTargetPosition = transform.position;
        currentZoom = 15f;
        
        Debug.Log($"Камера настроена: позиция {transform.position}");
    }
    
    void Update()
    {
        if (thisCamera == null) return;
        
        // ========== ПЕРЕТАСКИВАНИЕ МЫШЬЮ ==========
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = GetMouseWorldPosition();
            isDragging = true;
        }
        
        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 currentMousePos = GetMouseWorldPosition();
            Vector3 delta = dragOrigin - currentMousePos;
            
            cameraTargetPosition += delta;
            
            cameraTargetPosition.x = Mathf.Clamp(cameraTargetPosition.x, minX, maxX);
            cameraTargetPosition.z = Mathf.Clamp(cameraTargetPosition.z, minZ, maxZ);
            
            transform.position = Vector3.Lerp(transform.position, cameraTargetPosition, Time.deltaTime * 5f);
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
        
        // ========== ПРИБЛИЖЕНИЕ ==========
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentZoom -= scroll * scrollSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
            
            Vector3 newPos = cameraTargetPosition;
            newPos.y = currentZoom * 0.5f;
            newPos.z = cameraTargetPosition.z - currentZoom;
            newPos.y = Mathf.Clamp(newPos.y, 1f, 25f);
            
            transform.position = newPos;
        }
        
        // ========== ВРАЩЕНИЕ (Alt + ПКМ) ==========
        if (Input.GetMouseButton(1) && Input.GetKey(KeyCode.LeftAlt))
        {
            rotation.x += Input.GetAxis("Mouse X") * rotationSpeed;
            rotation.y -= Input.GetAxis("Mouse Y") * rotationSpeed;
            rotation.y = Mathf.Clamp(rotation.y, 10f, 80f);
            
            Quaternion yaw = Quaternion.Euler(0, rotation.x, 0);
            Quaternion pitch = Quaternion.Euler(rotation.y, 0, 0);
            transform.rotation = yaw * pitch;
        }
        
        // ========== СБРОС ==========
        if (Input.GetKeyDown(KeyCode.Home))
        {
            ResetCamera();
        }
        
        // ========== ЦЕНТР НА ИГРОКЕ ==========
        if (Input.GetKeyDown(KeyCode.End))
        {
            CenterOnPlayer();
        }
    }
    
    private Vector3 GetMouseWorldPosition()
    {
        if (thisCamera == null) return Vector3.zero;
        
        Ray ray = thisCamera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        float distance;
        
        if (plane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }
        
        return Vector3.zero;
    }
    
    private void ResetCamera()
    {
        cameraTargetPosition = new Vector3(0, 15, -20);
        transform.position = cameraTargetPosition;
        transform.rotation = Quaternion.Euler(35, 0, 0);
        rotation = Vector2.zero;
        currentZoom = 15f;
        Debug.Log("Камера сброшена на начальную позицию");
    }
    
    private void CenterOnPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            cameraTargetPosition = player.transform.position;
            cameraTargetPosition.y = 15;
            cameraTargetPosition.z = player.transform.position.z - 20;
            
            transform.position = cameraTargetPosition;
            Debug.Log($"Камера центрирована на игроке");
        }
        else
        {
            Debug.LogWarning("Игрок не найден!");
        }
    }
    
    void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 320, 140), "=== КАМЕРА-НАБЛЮДАТЕЛЬ ===");
        GUI.Label(new Rect(20, 35, 300, 20), "Ctrl+Shift+P - выключить");
        GUI.Label(new Rect(20, 55, 300, 20), "ЛЕВАЯ КНОПКА + ДВИЖЕНИЕ - панорамирование");
        GUI.Label(new Rect(20, 75, 300, 20), "Колёсико - приближение/отдаление");
        GUI.Label(new Rect(20, 95, 300, 20), "Alt + ПКМ + мышь - вращение");
        GUI.Label(new Rect(20, 115, 300, 20), "Home - сброс | End - центр на игроке");
        GUI.Label(new Rect(20, 135, 300, 20), $"Позиция: ({transform.position.x:F1}, {transform.position.y:F1}, {transform.position.z:F1})");
        
        if (isDragging)
        {
            GUI.Box(new Rect(Screen.width - 120, 10, 110, 30), "ПЕРЕМЕЩЕНИЕ");
        }
    }
}
#endif