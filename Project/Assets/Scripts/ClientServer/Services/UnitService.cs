using Game.Ecs.ClientServer.Components;
using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Modules.Box2D.ClientServer.Components.Colliders;
using XFlow.Utils;

namespace Game.ClientServer.Services
{
    public static class UnitService
    {
        public static int CreateUnitEntity(EcsWorld world)
        {
            var entity = world.NewEntity();
            
            entity.EntityAdd<UnitComponent>(world);
            entity.EntityAdd<LookDirectionComponent>(world).Value = new Vector3(0,0,1);
            entity.EntityAdd<PositionComponent>(world).Value = new Vector3(0, 0, 0);
            entity.EntityAdd<FoodCollectedComponent>(world).Value = 0;
            entity.EntityAdd<AmmoCollectedComponent>(world).Value = 0;
            entity.EntityAdd<Rotation2DComponent>(world);


            ref var radiusComponent = ref entity.EntityAdd<RadiusComponent>(world);
            radiusComponent.Value = 0.4f;

            /*
            ref var healthComponent = ref entity.EntityAdd<HpComponent>(world);
            healthComponent.maxHealth = 100;
            healthComponent.health = 50;
            */
            Box2DServices.AddRigidbodyDefinition(world, entity, BodyType.Dynamic).SetDensity(700f).SetFriction(0f)
                .SetRestitution(0).SetRestitutionThreshold(0.5f).SetSleepingAllowed(false);//.SetLinearDamping(0);
            Box2DServices.AddBoxColliderToDefinition(world, entity, new Vector2(2.1f,0.2f), Vector2.zero,0);

            entity.EntityAdd<AverageSpeedComponent>(world) = world.GetUnique<AverageSpeedComponent>();

            world.Log($"GenerateUnitEntity {entity}");

            return entity;
        }
    }
}