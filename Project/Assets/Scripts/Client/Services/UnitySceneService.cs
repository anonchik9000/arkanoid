using System;
using System.Collections.Generic;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.View;
using UnityEngine;
using XFlow.Ecs.Client;
using XFlow.Ecs.Client.Components;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.Client;
using XFlow.Modules.Box2D.ClientServer;
using XFlow.Modules.Box2D.ClientServer.Components.Joints;
using XFlow.Modules.Fire.ClientServer.Components;
using XFlow.Utils;
using Object = UnityEngine.Object;

namespace Game.Client.Services
{
    public class UnitySceneService
    {
        private static void ForEachObject<T>(Action<T> fn) where T : Component//it is UNITY Mono Component
        {
            try
            {
                var items = Object.FindObjectsOfType<T>();
                foreach (var item in items)
                {
                    fn(item);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public static void InitializeNewWorldFromScene(EcsWorld world)
        {
            var entities = new Dictionary<GameObject, int>();

            //int NameId = 0;
            int GetOrCreateGameEntity(GameObject go)
            {
                if (entities.TryGetValue(go, out var entity)) return entity;

                entity = world.NewEntity();
                //go.name = $"{go.name}[{entity}]";
                
                
                entity.EntityAdd<DebugNameComponent>(world).Value = go.name;
                
                
                ref var gameObjectNameComponent = ref entity.EntityAdd<GameObjectNameComponent>(world);
                gameObjectNameComponent.Name = go.name;
                //NameId++;

                ref var transformComponent = ref entity.EntityAdd<TransformComponent>(world);
                transformComponent.Transform = go.transform;

                entities.Add(go, entity);

                return entity;
            }


            ForEachObject<BushView>(view =>
            {
                var bushEntity = GetOrCreateGameEntity(view.gameObject);
                bushEntity.EntityAdd<BushComponent>(world);

                ref var positionComponent = ref bushEntity.EntityAdd<PositionComponent>(world);
                positionComponent.Value = view.transform.position;

                bushEntity.EntityAdd<InteractableComponent>(world);

                ref var radiusComponent = ref bushEntity.EntityAdd<RadiusComponent>(world);
                radiusComponent.Value = view.transform.lossyScale.x / 2f;

                ref var resourceComponent = ref bushEntity.EntityAdd<ResourceComponent>(world);
                resourceComponent.count = 1;

                ref var collectableComponent = ref bushEntity.EntityAdd<CollectableComponent>(world);
                collectableComponent.IsCollected = false;

                ref var collectableTargetComponent = ref bushEntity.EntityAdd<CollectableTargetComponent>(world);
                collectableTargetComponent.GameObject = view.Berries.gameObject;

                bushEntity.EntityAdd<FlammableComponent>(world).Power = 3;
            });

            ForEachObject<BoxView>(view =>
            {
                var boxEntity = GetOrCreateGameEntity(view.gameObject);
                boxEntity.EntityAdd<BoxComponent>(world);

                var forward = view.transform.forward;
                var position = view.transform.position + forward / 2;

                ref var positionComponent = ref boxEntity.EntityAdd<PositionComponent>(world);
                positionComponent.Value = position;

                boxEntity.EntityAdd<InteractableComponent>(world);

                ref var radiusComponent = ref boxEntity.EntityAdd<RadiusComponent>(world);
                radiusComponent.Value = view.transform.lossyScale.x / 2f;
                
                
                view.LinkEntity(world, boxEntity);
                
            });

            ForEachObject<ButtonView>(view =>
            {
                var buttonEntity = GetOrCreateGameEntity(view.gameObject);

                ref var buttonComponent = ref buttonEntity.EntityAdd<ButtonStateComponent>(world);
                buttonComponent.isActivated = false;
                
                ref var positionComponent = ref buttonEntity.EntityAdd<PositionComponent>(world);
                positionComponent.Value = view.transform.position;

                ref var radiusComponent = ref buttonEntity.EntityAdd<RadiusComponent>(world);
                radiusComponent.Value = view.transform.lossyScale.x / 2f;

                buttonEntity.EntityAdd<InteractableComponent>(world);
                

                ref var progressComponent = ref buttonEntity.EntityAdd<ProgressComponent>(world);
                progressComponent.Value = 0;

                ref var moveInfoComponent = ref buttonEntity.EntityAdd<MoveInfoComponent>(world);
                moveInfoComponent.StartPosition = view.StartPosition;
                moveInfoComponent.EndPosition = view.EndPosition;
                
                if (view.Spawner)
                    buttonEntity.EntityGetOrCreateRef<ButtonCustomComponent>(world).Spawn = true;
                if (view.Shake)
                    buttonEntity.EntityGetOrCreateRef<ButtonCustomComponent>(world).Shake = true;
            });

            ForEachObject<GateView>(view =>
            {
                var gateEntity = GetOrCreateGameEntity(view.gameObject);
                ref var gateComponent = ref gateEntity.EntityAdd<GateComponent>(world);

                ref var buttonLinkComponent = ref gateEntity.EntityAdd<ButtonLinkComponent>(world);
                buttonLinkComponent.Entities = new int[view.OpenedBy.Count];

                for (var i = 0; i < view.OpenedBy.Count; i++)
                    if (entities.TryGetValue(view.OpenedBy[i], out var buttonEntity))
                        buttonLinkComponent.Entities[i] = buttonEntity;

                ref var positionComponent = ref gateEntity.EntityAdd<PositionComponent>(world);
                positionComponent.Value = view.transform.position;

                ref var radiusComponent = ref gateEntity.EntityAdd<RadiusComponent>(world);
                radiusComponent.Value = 1f;

                ref var progressComponent = ref gateEntity.EntityAdd<ProgressComponent>(world);
                progressComponent.Value = 0;

                ref var moveInfoComponent = ref gateEntity.EntityAdd<MoveInfoComponent>(world);
                moveInfoComponent.StartPosition = view.StartPosition;
                moveInfoComponent.EndPosition = view.EndPosition;
            });

            /*
            ForEachObject<CharacterView>(view =>
            {
                var characterEntity = GetOrCreateGameEntity(view.gameObject);

                ref var playerComponent = ref characterEntity.EntityAdd<PlayerComponent>(world);
                playerComponent.id = Random.Range(-9999, -1111);
                characterEntity.EntityAdd<UnitComponent>(world);

                characterEntity.EntityAdd<MoveDirectionComponent>(world);
                characterEntity.EntityAdd<PositionComponent>(world);
                
                ref var radiusComponent = ref characterEntity.EntityAdd<RadiusComponent>(world);
                radiusComponent.radius = 0.4f;
            });*/

            ForEachObject<DestructibleView>(view =>
            {
                var entity = GetOrCreateGameEntity(view.gameObject);
                
                entity.EntityAdd<HpComponent>(world).Value = 2;
                entity.EntityAdd<MaxHpComponent>(world).Value = 10;
            });

            
            ForEachObject<Rigidbody2D>(body =>
            {
                var entity = GetOrCreateGameEntity(body.gameObject);
                ClientBox2DServices.AddRigidBodyDefinitionWithColliders(world, entity, body);
            });
            
            ForEachObject<Rigidbody>(body =>
            {
                var entity = GetOrCreateGameEntity(body.gameObject);
                ClientBox2DServices3D.AddRigidBodyDefinitionWithColliders(world, entity, body);
            });

            ForEachObject<DistanceJointConnectView>(joint =>
            {
                var entityA = GetOrCreateGameEntity(joint.gameObject);
                var entityB = GetOrCreateGameEntity(joint.Target);
                
                Box2DServices.AddDistanceJointToDefinition(world, entityA, entityB);
            });

            /*ForEachObject<RevoluteJointView>(joint =>
            {
                var entityA = GetOrCreateGameEntity(joint.gameObject);
                var entityB = GetOrCreateGameEntity(joint.ConnectedBody.gameObject);
                
                var data = new Box2DRevoluteJoint();
                data.Entity = world.PackEntity(entityB);

                if (joint.AutoCalculateOffsets)
                {
                    data.ConnectedJointOffset = Vector2.zero;
                    var offset = Vector3.Scale(joint.transform.InverseTransformPoint(joint.ConnectedBody.transform.position), joint.transform.lossyScale);
                    data.JointOffset = new Vector2(offset.x, offset.z);
                }
                else
                {
                    data.JointOffset = joint.JointOffset;
                    data.ConnectedJointOffset = joint.ConnectedJointOffset;
                }

                data.EnableLimits = joint.EnableLimits;
                data.LowerAngleLimit = joint.LowerAngleLimit * Mathf.Deg2Rad;
                data.UpperAngleLimit = joint.UpperAngleLimit * Mathf.Deg2Rad;
                data.CollideConnected = joint.CollideConnected;

                Box2DServices.AddRevoluteJointToDefinition(world, entityA, data);
            });*/
            
            ForEachObject<SpawnGunView>(view =>
            {
                var entity = GetOrCreateGameEntity(view.gameObject);
                entity.EntityAdd<SpawnGunComponent>(world);
                entity.EntityAdd<InteractableComponent>(world);
                entity.EntityAdd<PositionComponent>(world).Value = view.transform.position;
                
                ref var collectableComponent = ref entity.EntityAdd<CollectableComponent>(world);
                collectableComponent.IsCollected = false;
                
                entity.EntityAdd<CollectableTargetComponent>(world).GameObject = view.gameObject;
            });
            
            ForEachObject<AmmoView>(view =>
            {
                var entity = GetOrCreateGameEntity(view.gameObject);
                entity.EntityAdd<AmmoComponent>(world);
                entity.EntityAdd<InteractableComponent>(world);
                entity.EntityAdd<PositionComponent>(world).Value = view.transform.position;
                entity.EntityAdd<CollectableComponent>(world).IsCollected = false;;
                entity.EntityAdd<CollectableTargetComponent>(world).GameObject = view.gameObject;
                entity.EntityAdd<RadiusComponent>(world).Value = view.transform.lossyScale.x / 2f;
            });
                
            var unit = Object.FindObjectOfType<Global>().CharacterPrefab;
            var clips = unit.Animator.runtimeAnimatorController.animationClips;
            //var clip = clips.First(clip => clip.name == "Walking");
            //todo calculate exact speed

            world.AddUnique<AverageSpeedComponent>().Value = 8f; //clip.averageSpeed;
            
        }
    }
}
