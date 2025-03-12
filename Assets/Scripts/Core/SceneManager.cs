using UnityEngine;
using Zenject;

public class SceneManager : MonoBehaviour
{
    [Inject] private PickupItem.Factory _pickupItemFactory; // Фабрика для создания фруктов через Zenject

    // Структура для хранения данных о фрукте: префаб, позиция и имя для отладки
    private struct FruitData
    {
        public GameObject Prefab;
        public Vector3 Position;
        public string Name;

        public FruitData(GameObject prefab, Vector3 position, string name)
        {
            Prefab = prefab;
            Position = position;
            Name = name;
        }
    }

    [Inject]
    public void Initialize()
    {
        // Массив фруктов с их префабами, позициями и именами
        FruitData[] fruits = new[]
        {
            new FruitData(applePrefab, new Vector3(-3.65f, 2.33f, 1.26f), "Apple"),
            new FruitData(bananaPrefab, new Vector3(-3.54f, 1.60f, 1.26f), "Banana"),
            new FruitData(mangoPrefab, new Vector3(-3.30f, 0.9f, 1), "Mango")
        };

        // Проходим по каждому фрукту и создаём его в сцене
        foreach (var fruit in fruits)
        {
            // Проверяем, что префаб не равен null
            if (fruit.Prefab == null)
            {
                Debug.LogError($"{fruit.Name} Prefab is null in SceneManager");
                continue;
            }

            // Создаём фрукт через фабрику и устанавливаем его позицию
            var pickupItem = _pickupItemFactory.Create(fruit.Prefab);
            if (pickupItem != null)
            {
                pickupItem.SetPosition(fruit.Position);
            }
            else
            {
                Debug.LogError($"Failed to create {fruit.Name} PickupItem");
            }
        }
    }

    // Сериализованные поля для привязки префабов в инспекторе
    [SerializeField] private GameObject applePrefab;
    [SerializeField] private GameObject bananaPrefab;
    [SerializeField] private GameObject mangoPrefab;
}