using UnityEngine;
using Zenject;

public class PickupItem : MonoBehaviour
{
    public class Factory : PlaceholderFactory<GameObject, PickupItem> { }

    [SerializeField] private float scaleFactor = 1.0f;

    public void Initialize(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("PickupItem: Prefab is null!");
            return;
        }

        GameObject fruitInstance = Instantiate(prefab, transform);
        fruitInstance.transform.localScale *= scaleFactor;

        Rigidbody rb = fruitInstance.GetComponent<Rigidbody>() ?? fruitInstance.AddComponent<Rigidbody>();
        fruitInstance.tag = "Pickup";

        Transform fruitsParent = GameObject.Find("Fruits")?.transform;
        if (fruitsParent != null)
        {
            transform.SetParent(fruitsParent, false);
        }
        else
        {
            Debug.LogError("PickupItem: Fruits parent not found!");
        }
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }
}