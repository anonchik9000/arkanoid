﻿using Fabros.Ecs;
using Fabros.Ecs.Systems;
using Fabros.EcsModules.Base.Components;
using Fabros.EcsModules.Grid;
using Fabros.EcsModules.Tick;
using Game.ClientServer.Physics.Components;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Events;
using Game.Ecs.ClientServer.Components.Input;
using Game.Ecs.ClientServer.Components.Inventory;
using Game.Ecs.ClientServer.Components.Objective;
using Game.Ecs.ClientServer.Components.Physics;
using Game.Ecs.ClientServer.Systems;
using Game.Ecs.ClientServer.Systems.Physics;
using Game.Fabros.EcsModules.Fire.ClientServer.Components;
using Game.Fabros.EcsModules.Fire.ClientServer.Systems;
using Game.Fabros.Net.ClientServer.Ecs.Systems;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedSystems;
#if CLIENT
using Game.Ecs.Client.Physics;
using Game.Ecs.Client.Systems;
using Game.Ecs.Client.Systems.Inventory;
using Game.Fabros.EcsModules.Fire.Client.Systems;
using Game.Fabros.Net.Client;
#endif

namespace Game.ClientServer
{
    public class SystemsAndComponents
    {
        //public LeoContexts leo { get; set; }

        public static ComponentsPool CreateComponentsPool()
        {
            /*
             * общий код для клиента и сервера
             * ComponentsPool необходим, чтобы компоненты правильно передавались через сеть
             * если компонента в списке нет, то он не будет попадать в diff
            */
#if CLIENT
            var pool = new ComponentsPool(new UnityJsonComponentSerializer());
#else
            var pool = new ComponentsPool(new JsonComponentSerializer());
#endif

            pool.AddComponent<MoveDirectionComponent>();
            pool.AddComponent<PlayerComponent>();
            pool.AddComponent<TickrateConfigComponent>();

            TickModule.AddSerializableComponents(pool);
            GridModule.AddSerializableComponents(pool);

            pool.AddComponent<FireComponent>();
            pool.AddComponent<BurnedOutComponent>();
            pool.AddComponent<FlammableComponent>();

            pool.AddComponent<GameObjectNameComponent>();
            pool.AddComponent<BoxComponent>();
            pool.AddComponent<BushComponent>();
            pool.AddComponent<ButtonComponent>();
            pool.AddComponent<ButtonLinkComponent>();
            pool.AddComponent<UnitComponent>();
            pool.AddComponent<GateComponent>();
            pool.AddComponent<MoveInfoComponent>();
            
            pool.AddComponent<FoodCollectedComponent>();

            pool.AddComponent<HealthComponent>();
            pool.AddComponent<InteractableComponent>();
            pool.AddComponent<CollectableComponent>();
            pool.AddComponent<TargetPositionComponent>();
            pool.AddComponent<ProgressComponent>();
            pool.AddComponent<PositionComponent>();
            pool.AddComponent<RadiusComponent>();
            pool.AddComponent<ResourceComponent>();
            pool.AddComponent<SpeedComponent>();

            pool.AddComponent<InventoryItemComponent>();
            pool.AddComponent<InventorySlotComponent>();

            pool.AddComponent<FootprintComponent>();
            pool.AddComponent<LastFootprintComponent>();
            pool.AddComponent<LifeTimeComponent>();
            pool.AddComponent<AIPlayerComponent>();
            pool.AddComponent<DestroyComponent>();


            pool.AddComponent<ObjectiveCompletedComponent>();
            pool.AddComponent<ObjectiveOpenedComponent>();
            pool.AddComponent<ObjectiveComponent>();
            pool.AddComponent<ObjectiveDescriptionComponent>();

            pool.AddComponent<ButtonPushCompleted>();
            pool.AddComponent<GateOpenedComponent>();
            pool.AddComponent<LookDirectionComponent>();
            
            pool.AddComponent<RigidbodyDefinitionComponent>();
            pool.AddComponent<RigidbodyComponent>();
            pool.AddComponent<BoxColliderComponent>();
            pool.AddComponent<CircleColliderComponent>();
            pool.AddComponent<PolygonColliderComponent>();
            pool.AddComponent<ChainColliderComponent>();
            pool.AddComponent<RotationComponent>();
            
            //pool.AddComponent<BeginContactComponent>();
            //pool.AddComponent<EndContactComponent>();
            //pool.AddComponent<PreSolveComponent>();
            //pool.AddComponent<PostSolveComponent>();
            
            pool.AddComponent<AverageSpeedComponent>();

            return pool;
        }
        
        public static void AddSystems(ComponentsPool pool, EcsSystems systems, bool client, bool server)
        {
#if CLIENT
            void AddClient(IEcsSystem system)
            {
                //иногда на клиенте нужно получить список систем
                //куда не входят по настоящему клиентские системы (привязанные к Unity) 
                if (client)
                    systems.Add(system);
            }
#endif

            void AddServer(IEcsSystem system)
            { 
                //для сингл плеера нужны все игровые системы
                if (server)
                    systems.Add(system);
            }
       
            
#if SERVER
            systems.Add(new CreateGameSystem(pool));
#endif
            
#if CLIENT
            AddClient(new DetectPlayerIdChangesSystem());
#endif
            
            AddServer(new SpawnBotSystem());
            AddServer(new JoinPlayerSystem());

#if CLIENT
            AddClient(new InitSceneSystem());
#endif
            
            
            systems.Add(new InitPhysicsSystem());
#if CLIENT
            systems.Add(new InitBox2dDebug());
#endif
            systems.Add(new PopulatePhysicsWorldSystem());
            systems.Add(new SyncPhysicsWorldSystem());

            AddServer(new AIPlayerSystem());

            systems.Add(new MoveToTargetPositionSystem());
            systems.Add(new MoveSystem());
            systems.Add(new UnitMoveSystem());
            
            
            systems.DelHere<BeginContactComponent>();
            systems.DelHere<EndContactComponent>();
            systems.DelHere<PreSolveComponent>();
            systems.DelHere<PostSolveComponent>();
            
            systems.Add(new UpdatePhysicsWorldSystem());
            systems.Add(new EntitiesLifeTimeSystem());
            
#if CLIENT
            AddClient(new AnimateCharacterSystem());
#endif
            
            systems.Add(GridModule.GetSystems());
            systems.Add(new ApplyUserInputSystem());


            AddServer(new FootprintSystem());

#if CLIENT
            AddClient(new FootprintViewSystem());
            AddClient(new HighlightInteractableSystem());
#endif
            //systems.Add(new BushInteractionSystem());
            //systems.Add(new BoxInteractionSystem());

#if CLIENT
            AddClient(new CollectableSystem());
#endif
            // gates and buttons
            systems.Add(new ButtonsInteractionSystem());
            systems.Add(new GateSystem());
            systems.Add(new MoveByProgressSystem());

#if CLIENT
            AddClient(new UseInventoryItemSystem());
#endif

            systems.Add(new AddInventoryItemSystem());
            systems.Add(new RemoveInventoryItemSystem());

            systems.Add(new FireSystem());

#if CLIENT
            AddClient(new FireViewSystem());
            AddClient(new InventorySystem());
#endif

            systems.Add(new DeleteEntitiesSystemWith<DestroyComponent>());
            systems.DelHere<InputComponent>("input");
            systems.Add(new DeleteEntitiesSystemWith<EventComponent>());


            systems.Add(new FireDestroyEntitySystem());

            AddServer(new ObjectivesSystem());

            systems.Add(TickModule.GetSystems());
            
#if CLIENT
            AddClient(new AnimateCharacterSystem());
#endif


            systems.Add(new EventsSystem<PlayerComponent>());
            systems.Add(new EventsSystem<ButtonPushCompleted>());
            systems.Add(new EventsSystem<ObjectiveOpenedComponent>());
            systems.Add(new EventsSystem<ObjectiveCompletedComponent>());
            systems.Add(new EventsSystem<GateOpenedComponent>());
            systems.Add(new EventsSystem<FoodCollectedComponent>());
            
#if CLIENT
            systems.Add(new EventsSystem<AnimationStateComponent>());
            systems.Add(new EventsSystem<MovingComponent>());
#endif
        }
    }
}