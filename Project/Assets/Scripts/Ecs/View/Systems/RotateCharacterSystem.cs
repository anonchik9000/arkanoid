using Fabros.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Ecs.View.Systems
{
    public class RotateCharacterSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<UnitComponent>()
                .Inc<LookDirectionComponent>()
                .Inc<TransformComponent>()
                .End();

            var poolTransform = world.GetPool<TransformComponent>();
            var poolLookDirection = world.GetPool<LookDirectionComponent>();
            var poolLerp = world.GetPool<LerpComponent>();
            
            foreach (var entity in filter)
            {
                var transform = poolTransform.Get(entity).Transform;
                var lookDirection = poolLookDirection.Get(entity).value;

                if (Mathf.Approximately(lookDirection.magnitude, 0))
                    continue;

                var lerp = poolLerp.GetNullable(entity)?.value??1f;
                
                var quat = Quaternion.LookRotation(lookDirection);
                transform.localRotation = Quaternion.Lerp(transform.localRotation, quat, lerp);
            }
        }
    }
}