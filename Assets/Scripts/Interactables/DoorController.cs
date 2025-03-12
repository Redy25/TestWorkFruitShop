using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private float targetAngle = 90f;
    [SerializeField] private float openSpeed = 90f;
    private Quaternion initialRotation;
    private Quaternion targetRotation;
    private Transform pivotTransform;
    private bool isOpening = false;
    private bool isClosing = false;

    void Start()
    {
        pivotTransform = transform.parent;
        if (pivotTransform == null)
        {
            Debug.LogError("Parent DoorPivot not found!");
            return;
        }

        initialRotation = pivotTransform.rotation;
        targetRotation = initialRotation;

        if (!gameObject.GetComponent<MeshCollider>())
        {
            gameObject.AddComponent<MeshCollider>();
        }
        gameObject.tag = "Door";
    }

    public void ToggleDoor()
    {
        if (pivotTransform == null) return;

        if (Quaternion.Angle(pivotTransform.rotation, targetRotation) < 1f)
        {
            if (Quaternion.Angle(pivotTransform.rotation, initialRotation) < 1f)
            {
                targetRotation = initialRotation * Quaternion.Euler(0, targetAngle, 0);
                isOpening = true;
                isClosing = false;
            }
            else
            {
                targetRotation = initialRotation;
                isClosing = true;
                isOpening = false;
            }
        }
    }

    void Update()
    {
        if (pivotTransform == null || (!isOpening && !isClosing)) return;

        pivotTransform.rotation = Quaternion.RotateTowards(pivotTransform.rotation, targetRotation, openSpeed * Time.deltaTime);
        if (Quaternion.Angle(pivotTransform.rotation, targetRotation) < 1f)
        {
            isOpening = false;
            isClosing = false;
        }
    }
}