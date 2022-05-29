﻿using Fabros.Ecs.Utils;
using Fabros.EcsModules.Tick.Other;
using Leopotam.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class PushingSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world.Filter<PushingComponent>().End();
            foreach (var entity in filter)
            {
                if (entity.EntityGet<PushingComponent>(world).EndTime < world.GetTime())
                    entity.EntityDel<PushingComponent>(world);
            }
        }
    }
}