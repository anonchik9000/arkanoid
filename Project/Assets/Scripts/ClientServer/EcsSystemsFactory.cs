using Game.ClientServer.Services;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Objective;
using Game.Ecs.ClientServer.Systems;
using XFlow.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Systems;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Systems;
using XFlow.Modules.Fire.ClientServer.Components;
using XFlow.Modules.Fire.ClientServer.Systems;
using XFlow.Modules.Grid.Systems;
using XFlow.Modules.Inventory.ClientServer.Systems;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Net.ClientServer.Ecs.Systems;
using XFlow.Modules.Mech.ClientServer.Systems;

#if CLIENT
using Game.Ecs.Client.Systems;
using Game.Ecs.Client.Systems.Inventory;
using XFlow.Modules.Box2D.Client.Systems;
using XFlow.Modules.Fire.Client.Systems;
using XFlow.Modules.Inventory.ClientServer.Components;
using XFlow.Modules.Tick.ClientServer.Components;
#endif

using Zenject;

namespace Game.ClientServer
{
    public class EcsSystemsFactory : IEcsSystemsFactory
    {
        private EcsSystemsContainer _container;
        
        public EcsSystemsFactory(DiContainer di)
        {
            _container = new EcsSystemsContainer(di);

            /*
             * регистрация аналогична прямым вызовам через systems.AddSystem, за тем исключением,
             * что система будет создана через Zenject 
             */

            /*
             * системы регистрируются в порядке следования
             * RegisterServer - система, которая будет создана и на стороне сервера (или синглплеера)
             * Register - система, регистрируется для всех случаев
             */

            /*
             ПРИМЕРЫ
             
             container.RegisterServer<JoinPlayerSystem>();
             Система создается только на сервере и вход игрока может быть произведен только на сервере, никак не стартует в юнити             
             

             container.RegisterServer<AIPlayerSystem>();
             АИ бота, который ходит по случ. точкам работает отрабатывает только на сервере
             
             container.Register<GateSystem>();
             система открытия ворот нужна везде, чтоб клиент мог на опережение начинать открывать ворота локально
             а позже результат ее работы долетит и с сервера
            
             */

            /*
             #if SERVER - Определено на сервере        
                               
             нужны только для того чтобы избежать ошибок компиляции, 
             на сервере, например, физически нет систем, завязанных на GameObject
             потому даже их упоминания нужно "физически" исключить из кода
             
             #if CLIENT - Определено в юнити клиенте  
             */
#if SERVER
            _container.Register<CreateGameSystem>();
#endif
          
            _container.RegisterServer<ArkanoidGameSystem>();
            _container.RegisterServer<ManagePlayerSystem>();
           
           
            //_container.RegisterServer<AIPlayerSystem>();
            //_container.Register<AIPlayerSystem>();

            //_container.Register<MoveToTargetPositionSystem>();
            _container.Register<MoveSystem>();
            //_container.Register<LookDirectionSystem>();
            _container.Register<SimpleMoveSystem>();
            _container.Register<UnitMoveSystem>();
            _container.Register<FollowSystem>();
            _container.Register<PushingSystem>();

            _container.Register<MechAdapterSystem>();
            
            _container.Register<EntitiesLifeTimeSystem>();
            
            _container.Register<GridSystem>();
            
            _container.Register<DeleteComponentHereSystem<ShootStartedComponent>>();//если ставить в конец, то на клиент этот компонент даже придет
            
            _container.Register<ApplyInputSystem>();
            
            _container.Register<ApplyDragInputSystem>();


            _container.RegisterServer<FootprintSystem>();


            // gates and buttons
            _container.Register<ButtonsInteractionSystem>();
            _container.RegisterServer<ButtonCustomSystem>();
            _container.Register<GateSystem>();
            _container.Register<MoveByProgressSystem>();

            _container.Register<FireSystem>();

            _container.Register<ApplyForceSystem>();

            


            //_container.Register<DeleteEntityHereSystem<DestroyComponent>>();

            _container.RegisterServer<DeleteInputEntitiesSystem>();
            _container.RegisterServer<DeleteOutdatedInputEntitiesSystem>();

            _container.Register<FireDestroyEntitySystem>();

            _container.RegisterServer<ObjectivesSystem>();

            _container.Register<DestroyAtTimeSystem>();

            _container.Register<ShootSystem>();
            
            //Основная Box2dSystem должна быть в конце после всех основных систем,
            //иначе в мультиплеере предсказание не будет работать правильно
            _container.Register<Box2DInitSystem>();
            _container.Register<Box2DCreateBodiesSystem>();
            _container.Register<Box2DCreateEventContactsSystem>();
            //container.Register<Box2DCreateContactsSystem>();
            _container.Register<Box2DUpdateInternalObjectsSystem>();
            _container.Register<Box2DUpdateSystem>();
            //_container.Register<BulletContactSystem>();
            _container.Register<ArkanoidContactSystem>();
            _container.Register<DamageApplySystem>();
            _container.Register<SetVelocitySystem>();
            _container.Register<ArkanoidReinitSystem>();
            //уничтожение объектов сделаем серверным чтоб не было ошибок предсказания
            _container.RegisterServer<DestroyDamagedSystem>();

            //container.Register<Box2DDeleteContactsSystem>();

            
            
            _container.Register<DeleteEventEntitiesSystem>();
            _container.Register<InventoryRemoveEntitySystem>();
            _container.Register<Ecs.ClientServer.Systems.Box2DDestroyBodiesSystem>();
            
            //write final Box2d transforms to components
            _container.Register<Box2DWriteBodiesToComponentsSystem>();
        }
        
        public void AddNewSystems(EcsSystems systems, IEcsSystemsFactory.Settings settings)
        {
            systems.Add(_container.CreateNewSystems(systems.GetWorld(), settings.AddServerSystems));
        }

        public IEcsSystem CreateSyncDebugSystem(bool pre)
        {
            return new DebugMeSystem(pre);
        }

        public EcsSystemsContainer GetContainer()
        {
            return _container;
        }
    }
}