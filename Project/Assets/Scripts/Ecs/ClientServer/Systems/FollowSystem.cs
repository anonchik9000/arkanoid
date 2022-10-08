﻿using Game.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class FollowSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        
        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
        }

        public void Run(EcsSystems systems)
        {
            var filterFollow = _world.Filter<FollowComponent>().End();

            var poolFollow = _world.GetPool<FollowComponent>();
            var poolPosition = _world.GetPool<PositionComponent>();

            foreach (var entity in filterFollow)
            {
                var follow = poolFollow.Get(entity);
                if (!follow.Entity.Unpack(_world, out var entityToFollow))
                {
                    continue;
                }

                if (!poolPosition.Has(entityToFollow))
                {
                    continue;
                }

                poolPosition.GetOrCreateRef(entity).Value = poolPosition.Get(entityToFollow).Value;
            }
        }
    }
}