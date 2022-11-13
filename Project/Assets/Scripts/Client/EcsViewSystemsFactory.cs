using Game.ClientServer;
using Game.Ecs.Client.Systems;
using Game.Ecs.Client.Systems.Inventory;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Objective;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.Client.Systems;
using XFlow.Modules.Fire.Client.Systems;
using XFlow.Modules.Fire.ClientServer.Components;
using XFlow.Modules.Inventory.ClientServer.Components;
using XFlow.Modules.Mech.Client.Systems;
using XFlow.Modules.Tick.ClientServer.Components;
using XFlow.Net.Client;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Net.ClientServer.Ecs.Systems;
using Zenject;

namespace Game.Client
{
    public class EcsViewSystemsFactory : IEcsViewSystemsFactory
    {
        private EcsSystemsContainer _container;
        
        public EcsViewSystemsFactory(DiContainer di)
        {
            _container = new EcsSystemsContainer(di);
            
            
            _container.Register<PlayerDragAndDropInputSystem>();
            _container.Register<PlayerInputSystem>();

            _container.Register<InitSceneSystem>();
            
            _container.Register<DetectPlayerIdChangesSystem>();

            _container.Register<CreateViewSystem>();
            _container.Register<FireViewSystem>();
            _container.Register<InventoryImageSystem>();
            _container.Register<CreateMechViewSystem>();
            _container.Register<MechAnimationSystem>();

            _container.Register<ArkanoidViewSystem>();

            _container.Register<CharacterAnimationSystem>();
            
            _container.Register<FootprintViewSystem>();
            _container.Register<HighlightInteractableSystem>();
            _container.Register<VFXCreationSystem>();
            
            
            _container.Register<CollectableSystem>();
            _container.Register<WeaponEquipSystem>();
            
            
            _container.Register<AddLerpSystem>();
            
            _container.Register<SyncTransformSystem>();
            _container.Register<RotateCharacterSystem>();
            _container.Register<RotateRigidbodySystem>();
            //_container.Register<CameraFollowSystem>();
            
            _container.Register<ResetMouseDownSystem>();

            _container.Register<EventsSystem<WeaponComponent>>();
            _container.Register<EventsSystem<ButtonPressedComponent>>();
            _container.Register<EventsSystem<PlayerComponent>>();
            _container.Register<EventsSystem<ButtonPushCompleted>>();
            _container.Register<EventsSystem<ObjectiveOpenedComponent>>();
            _container.Register<EventsSystem<ObjectiveCompletedComponent>>();
            _container.Register<EventsSystem<GateOpenedComponent>>();
            _container.Register<EventsSystem<FoodCollectedComponent>>();
            _container.Register<EventsSystem<AmmoCollectedComponent>>();
            _container.Register<EventsSystem<PushingComponent>>();
            _container.Register<EventsSystem<CantMoveComponent>>();
            
            _container.Register<EventsSystem<MovingComponent>>();
            _container.Register<EventsSystem<InventorySlotComponent>>();
            _container.Register<EventsSystem<ActiveInventoryCategoryComponent>>();
            _container.Register<EventsSystem<EntityRemoveEventComponent>>();
            _container.Register<EventsSystem<TickComponent>>();
            _container.Register<EventsSystem<ControlledEntityComponent>>();
            _container.Register<EventsSystem<HpComponent>>();
            _container.Register<EventsSystem<DeletedEntityComponent>>();
            
            _container.Register<BulletDestroyFxSystem>();
            
            _container.Register<Box2DDebugViewSystem>();
            
            _container.Register<DestroyViewSystem>();
        }
        
        public void AddNewSystems(EcsSystems systems)
        {
            systems.Add(_container.CreateNewSystems(systems.GetWorld(), true));
        }
    }
}