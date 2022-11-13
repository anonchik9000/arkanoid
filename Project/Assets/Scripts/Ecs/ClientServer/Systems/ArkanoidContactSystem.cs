using Game.Ecs.ClientServer.Components;
using UnityEngine;
using XFlow.Ecs.ClientServer;
using XFlow.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Components.Other;
using XFlow.Modules.Mech.ClientServer.Components;

namespace Game.Ecs.ClientServer.Systems
{
    public class ArkanoidContactSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorld _world;
        private EcsWorld _eventWorld;

        private EcsPool<ArkanoidBallComponent> _ballPool;
        private EcsPool<ArkanoidBlockComponent> _blockPool;
        private EcsPool<Box2DBeginContactComponent> _poolContacts;
        private EcsFilter _filter;

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _eventWorld = systems.GetWorld(EcsWorlds.Event);

            _ballPool = _world.GetPool<ArkanoidBallComponent>();
            _blockPool = _world.GetPool<ArkanoidBlockComponent>();
            _poolContacts = _eventWorld.GetPool<Box2DBeginContactComponent>();
            _filter = _eventWorld.Filter<Box2DBeginContactComponent>().End();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filter)
            {
                var contact = _poolContacts.Get(entity);
                if (!contact.Data.EntityA.Unpack(_world, out var entityA))
                {
                    _world.Log($"contact {entity} entityA dead {contact.Data.EntityA.ToString()}");
                    continue;
                }

                if (!contact.Data.EntityB.Unpack(_world, out var entityB))
                {
                    _world.Log($"contact {entity} entityB dead {contact.Data.EntityA.ToString()}");
                    continue;
                }

                if (entityA == entityB)
                {
                    //контакт сам с собой???
                    _world.LogError($"self contact wtf {entityA}");
                    continue;
                }


                if(_ballPool.Has(entityA) && _blockPool.Has(entityB))
                {
                    BlockDamage(entityB);
                }else
                {
                    if (_ballPool.Has(entityB) && _blockPool.Has(entityA))
                    {
                        BlockDamage(entityA);
                    }
                }

            }
        }

        private void BlockDamage(int entity)
        {
            if(_blockPool.Has(entity))
            {
                var cmp = _blockPool.Get(entity);
                if (cmp.BallCreated > 0)
                {
                    int eventEntity = _world.NewEntity();
                    ref var contact = ref _world.GetPool<ArkanoidCreateBallComponent>().GetOrCreateRef(eventEntity);
                    contact.Count = cmp.BallCreated;
                    contact.Positon = _world.GetPool<PositionComponent>().Get(entity).Value;
                }
            }
            _world.GetPool<DeletedEntityComponent>().Add(entity);
        }
    }
}
