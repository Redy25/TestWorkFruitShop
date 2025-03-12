using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    // Ссылки на существующие объекты в сцене для привязки через Zenject
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private TruckZone truckZone;

    public override void InstallBindings()
    {
        // Привязка PlayerController: создаём новый объект с компонентом
        Container.Bind<PlayerController>()
            .FromNewComponentOnNewGameObject()
            .AsSingle()
            .NonLazy();

        // Привязка SceneManager: используем существующий объект
        Container.Bind<SceneManager>()
            .FromInstance(sceneManager)
            .AsSingle()
            .NonLazy();

        // Настройка фабрики для создания PickupItem (фруктов)
        Container.BindFactory<GameObject, PickupItem, PickupItem.Factory>()
            .FromIFactory(x => x.To<PickupItemFactory>().AsSingle());

        // Привязка ScreenOrientationManager: создаём новый объект
        Container.Bind<ScreenOrientationManager>()
            .FromNewComponentOnNewGameObject()
            .AsSingle()
            .NonLazy();

        // Привязка TruckZone: используем существующий объект
        Container.Bind<TruckZone>()
            .FromInstance(truckZone)
            .AsSingle()
            .NonLazy();
    }

    // Внутренний класс-фабрика для создания PickupItem
    private class PickupItemFactory : IFactory<GameObject, PickupItem>
    {
        private readonly DiContainer _container;

        public PickupItemFactory(DiContainer container)
        {
            _container = container;
        }

        // Метод создания нового PickupItem из префаба
        public PickupItem Create(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogError("PickupItemFactory: Prefab is null!");
                return null;
            }

            // Создаём новый объект и добавляем компонент PickupItem
            GameObject fruit = new GameObject("Fruit");
            PickupItem pickup = _container.InstantiateComponent<PickupItem>(fruit);
            pickup.Initialize(prefab);
            return pickup;
        }
    }
}