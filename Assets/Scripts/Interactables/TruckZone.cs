using UnityEngine;

public class TruckZone : MonoBehaviour
{
    private int collectedFruits = 0;

    // Событие для уведомления об изменении количества фруктов
    public delegate void FruitCollectedHandler(int count);
    public event FruitCollectedHandler OnFruitCollected;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pickup"))
        {
            collectedFruits++;
            Destroy(other.gameObject);
            Debug.Log($"Collected fruits: {collectedFruits}");
            OnFruitCollected?.Invoke(collectedFruits); // Уведомляем подписчиков
            if (collectedFruits >= 3)
            {
                Debug.Log("All fruits delivered! Task complete!");
            }
        }
    }

    private void Start()
    {
        gameObject.tag = "Truck";
    }

    // Метод для получения текущего количества (для отладки или тестирования)
    public int GetCollectedFruits()
    {
        return collectedFruits;
    }
}