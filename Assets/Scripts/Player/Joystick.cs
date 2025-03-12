using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Joystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [HideInInspector] public Vector2 inputVector;
    private RectTransform background;
    private RectTransform handle;
    private PlayerController playerController;
    private int currentTouchId = -1;

    private void Awake()
    {
        background = GetComponent<RectTransform>();
        handle = transform.childCount > 0 ? transform.GetChild(0).GetComponent<RectTransform>() : null;
        playerController = FindObjectOfType<PlayerController>();
        if (handle == null) Debug.LogError("Joystick: No handle found!");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.position.x > Screen.width / 2) return;

        foreach (Touch touch in Input.touches)
        {
            if (touch.position.x <= Screen.width / 2)
            {
                currentTouchId = touch.fingerId;
                playerController?.SetJoystickTouchId(currentTouchId);
                OnDrag(eventData);
                break;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out Vector2 pos))
        {
            pos.x = (pos.x / background.sizeDelta.x) * 2;
            pos.y = (pos.y / background.sizeDelta.y) * 2;
            inputVector = Vector2.ClampMagnitude(new Vector2(pos.x, pos.y), 1.0f);
            handle.anchoredPosition = new Vector2(inputVector.x * (background.sizeDelta.x / 2), inputVector.y * (background.sizeDelta.y / 2));
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
        playerController?.ClearJoystickTouchId();
        currentTouchId = -1;
    }
}