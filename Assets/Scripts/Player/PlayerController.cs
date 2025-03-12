using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PlayerController : MonoBehaviour
{
    // Компоненты игрока
    private CharacterController controller; // Контроллер для движения
    private Camera playerCamera; // Камера от первого лица

    // Параметры движения и гравитации
    private float moveSpeed = 4.0f; // Скорость движения игрока
    private float gravity = -9.81f; // Сила гравитации
    private Vector3 velocity; // Вектор скорости для гравитации

    // Параметры управления камерой
    [SerializeField, Range(0.01f, 0.2f)] private float touchSensitivity = 0.07f; // Чувствительность камеры к касаниям
    [SerializeField, Range(5f, 100f)] private float rotationSmoothSpeed = 25.0f; // Скорость плавного поворота камеры
    private float targetYaw = 0.0f; // Целевой угол поворота по Y (рысканье)
    private float targetPitch = 0.0f; // Целевой угол поворота по X (тангаж)
    private float currentYaw = 0.0f; // Текущий угол поворота по Y
    private float currentPitch = 0.0f; // Текущий угол поворота по X

    // Объекты и UI
    private GameObject heldItem; // Текущий удерживаемый фрукт
    private GameObject dropButton; // Кнопка для выброса фрукта
    private GameObject fruitCountText; // Текст для отображения счёта фруктов
    private Joystick joystick; // Джойстик для движения
    private int joystickTouchId = -1; // ID касания для джойстика
    private bool isCameraMoving = false; // Флаг движения камеры
    private int doorTouchId = -1; // ID касания для взаимодействия с дверью

    [Inject] private TruckZone truckZone; // Инъекция TruckZone для отслеживания счёта

    [Inject]
    public void Initialize()
    {
        // Настройка игрока
        gameObject.name = "Player";
        transform.position = new Vector3(0, 1.5f, 4);
        controller = gameObject.AddComponent<CharacterController>();
        controller.height = 2.0f;
        controller.center = new Vector3(0, 1, 0);

        // Создание камеры от первого лица
        GameObject cameraObj = new GameObject("PlayerCamera");
        cameraObj.transform.SetParent(transform);
        cameraObj.transform.localPosition = new Vector3(0, 1.5f, 0);
        playerCamera = cameraObj.AddComponent<Camera>();
        playerCamera.tag = "MainCamera";
        playerCamera.clearFlags = CameraClearFlags.Skybox;

        // Начальная ориентация игрока
        transform.rotation = Quaternion.Euler(0, 180, 0);
        targetYaw = transform.eulerAngles.y;
        currentYaw = targetYaw;
        targetPitch = 0.0f;
        currentPitch = targetPitch;

        // Инициализация UI и джойстика
        SetupDropButton();
        SetupFruitCountText();
        joystick = FindObjectOfType<Joystick>();

        // Подписка на событие TruckZone для обновления счёта
        if (truckZone != null)
        {
            truckZone.OnFruitCollected += UpdateFruitCount;
        }
    }

    // Настройка кнопки "Drop" для выброса фрукта
    private void SetupDropButton()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        dropButton = new GameObject("DropButton");
        dropButton.transform.SetParent(canvas.transform, false);
        RectTransform rect = dropButton.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.85f, 0.1f);
        rect.anchorMax = new Vector2(0.95f, 0.2f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Button button = dropButton.AddComponent<Button>();
        Image image = dropButton.AddComponent<Image>();
        image.color = Color.red;

        Text text = new GameObject("Text").AddComponent<Text>();
        text.transform.SetParent(dropButton.transform, false);
        text.text = "Drop";
        text.color = Color.black;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;

        button.onClick.AddListener(DropItem);
        dropButton.SetActive(false);
    }

    // Настройка текста для отображения количества собранных фруктов
    private void SetupFruitCountText()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        fruitCountText = new GameObject("FruitCountText");
        fruitCountText.transform.SetParent(canvas.transform, false);
        RectTransform rect = fruitCountText.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(-150f, -50f);
        rect.offsetMax = new Vector2(150f, -10f);

        Text text = fruitCountText.AddComponent<Text>();
        text.text = "Fruits: 0";
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.alignment = TextAnchor.UpperCenter;
    }

    // Обновление текста счёта при сборе фруктов
    private void UpdateFruitCount(int count)
    {
        if (fruitCountText != null)
        {
            Text textComponent = fruitCountText.GetComponent<Text>();
            if (textComponent != null)
            {
                textComponent.text = $"Fruits: {count}";
            }
        }
    }

    private void Update()
    {
        if (controller == null || playerCamera == null) return;

        // Обрабатываем движение игрока
        HandleMovement();
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Обрабатываем поворот камеры
        HandleLook();
        currentYaw = Mathf.Lerp(currentYaw, targetYaw, rotationSmoothSpeed * Time.deltaTime);
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, rotationSmoothSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(0, currentYaw, 0);
        playerCamera.transform.localEulerAngles = new Vector3(currentPitch, 0, 0);

        // Обрабатываем взаимодействие с объектами
        HandleInteraction();
    }

    // Движение игрока с помощью джойстика
    private void HandleMovement()
    {
        Vector2 joystickInput = joystick?.inputVector ?? Vector2.zero;
        Vector3 moveDirection = transform.TransformDirection(new Vector3(joystickInput.x, 0, joystickInput.y).normalized) * moveSpeed * Time.deltaTime;
        controller.Move(moveDirection);
    }

    // Поворот камеры через касание
    private void HandleLook()
    {
        bool hasLookTouch = false;
        foreach (Touch touch in Input.touches)
        {
            if (joystick != null && touch.fingerId == joystickTouchId) continue;

            if (touch.position.x > Screen.width / 2)
            {
                float deltaX = Mathf.Clamp(touch.deltaPosition.x, -50f, 50f);
                float deltaY = Mathf.Clamp(touch.deltaPosition.y, -50f, 50f);

                if (Mathf.Abs(deltaX) > 1f || Mathf.Abs(deltaY) > 1f)
                {
                    isCameraMoving = true;
                }
                else if (touch.fingerId == doorTouchId && touch.phase == TouchPhase.Stationary)
                {
                    isCameraMoving = false;
                }

                targetYaw += deltaX * touchSensitivity;
                targetPitch = Mathf.Clamp(targetPitch - deltaY * touchSensitivity, -90f, 90f);
                hasLookTouch = true;
            }
        }

        if (!hasLookTouch)
        {
            isCameraMoving = false;
            doorTouchId = -1;
        }
    }

    // Обработка взаимодействия с фруктами и дверью через касание
    private void HandleInteraction()
    {
        foreach (Touch touch in Input.touches)
        {
            if (joystick != null && touch.fingerId == joystickTouchId) continue;

            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = playerCamera.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out RaycastHit hit, 3.0f))
                {
                    if (hit.collider.CompareTag("Pickup"))
                    {
                        PickUpItem(hit.collider.gameObject);
                    }
                    else if (hit.collider.CompareTag("Door") && !isCameraMoving)
                    {
                        doorTouchId = touch.fingerId;
                        OpenDoor(hit.collider.gameObject);
                    }
                }
            }
            else if (touch.phase == TouchPhase.Moved && touch.fingerId == doorTouchId)
            {
                if (Mathf.Abs(touch.deltaPosition.x) > 1f || Mathf.Abs(touch.deltaPosition.y) > 1f)
                {
                    isCameraMoving = true;
                }
            }
            else if (touch.phase == TouchPhase.Ended && touch.fingerId == doorTouchId)
            {
                doorTouchId = -1;
            }
        }
    }

    // Открытие/закрытие двери
    private void OpenDoor(GameObject door)
    {
        if (door == null) return;
        door.GetComponent<DoorController>()?.ToggleDoor();
    }

    // Поднятие фрукта
    private void PickUpItem(GameObject item)
    {
        if (item == null || heldItem != null) return;

        heldItem = item;
        Rigidbody rb = item.GetComponent<Rigidbody>() ?? item.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.detectCollisions = false; // Отключаем коллизии для фрукта в руках
        heldItem.transform.SetParent(playerCamera.transform);
        heldItem.transform.localPosition = new Vector3(0, -0.5f, 1);
        heldItem.transform.localRotation = Quaternion.identity;
        dropButton.SetActive(true);

        // Игнорируем коллизии с другими фруктами
        Collider heldCollider = heldItem.GetComponent<Collider>();
        if (heldCollider != null)
        {
            PickupItem[] allPickups = FindObjectsOfType<PickupItem>();
            foreach (PickupItem pickup in allPickups)
            {
                if (pickup.gameObject != heldItem)
                {
                    Collider pickupCollider = pickup.GetComponent<Collider>();
                    if (pickupCollider != null)
                    {
                        Physics.IgnoreCollision(heldCollider, pickupCollider, true);
                    }
                }
            }
        }
    }

    // Выброс фрукта
    private void DropItem()
    {
        if (heldItem == null) return;

        Rigidbody rb = heldItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true; // Включаем коллизии обратно
            rb.AddForce(playerCamera.transform.forward * 5.0f, ForceMode.Impulse);
        }

        // Прекращаем игнорирование коллизий
        Collider heldCollider = heldItem.GetComponent<Collider>();
        if (heldCollider != null)
        {
            PickupItem[] allPickups = FindObjectsOfType<PickupItem>();
            foreach (PickupItem pickup in allPickups)
            {
                if (pickup.gameObject != heldItem)
                {
                    Collider pickupCollider = pickup.GetComponent<Collider>();
                    if (pickupCollider != null)
                    {
                        Physics.IgnoreCollision(heldCollider, pickupCollider, false);
                    }
                }
            }
        }

        heldItem.transform.SetParent(null);
        heldItem = null;
        dropButton.SetActive(false);
    }

    // Установка ID касания для джойстика
    public void SetJoystickTouchId(int touchId) => joystickTouchId = touchId;
    public void ClearJoystickTouchId() => joystickTouchId = -1;

    // Очистка подписки при уничтожении объекта
    private void OnDestroy()
    {
        if (truckZone != null)
        {
            truckZone.OnFruitCollected -= UpdateFruitCount;
        }
    }
}