﻿using System;
using System.Collections.Generic;
using Fabros.EcsModules.Grid.Other;
using Fabros.EcsModules.Mech.ClientServer.Components;
using Fabros.EcsModules.Tick.Other;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Game.ClientServer.Services;
using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Input;
using XFlow.Net.ClientServer.Input.proto;
using XFlow.Utils;
using Random = System.Random;

namespace Game.Ecs.ClientServer.Systems
{
    public class ApplyInputSystem : IEcsInitSystem, IEcsRunSystem
    {
        EcsFilter filter;
        private EcsWorld world;
        private EcsWorld inputWorld;
        
        List<int> entities = new List<int>();
        
        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            inputWorld = systems.GetWorld("input");
            filter = inputWorld.Filter<InputComponent>().End();
        }
        
        public void Run(EcsSystems systems)
        {
            var mainPlayerId = -1;
            if (world.HasUnique<MainPlayerIdComponent>())//если это мир на клиенте
                mainPlayerId = world.GetUnique<MainPlayerIdComponent>().value;
           
            
            var poolInputShot   = inputWorld.GetPool<InputShotComponent>();
            var poolPlayer      = inputWorld.GetPool<InputPlayerComponent>();
            var poolInputMoveDir= inputWorld.GetPool<InputMoveDirectionComponent>();
            var poolInputMoveTo = inputWorld.GetPool<InputMoveToPointComponent>();
            var poolInputAction = inputWorld.GetPool<InputActionComponent>();
            var poolInputKick   = inputWorld.GetPool<InputKickComponent>();
            var poolInputTick   = inputWorld.GetPool<InputTickComponent>();
            
            
            

            var tick = world.GetTick();
            
            foreach (var inputEntity in filter)
            {
                if (poolInputTick.GetNullable(inputEntity)?.Tick != tick)
                    continue;
                
                var playerId = mainPlayerId;
                if (poolPlayer.Has(inputEntity))
                    playerId = poolPlayer.Get(inputEntity).PlayerID;

                var unitEntity = BaseServices.GetUnitEntityByPlayerId(world, playerId);
                if (!world.IsEntityAliveInternal(unitEntity))
                {
                    Debug.LogError($"unit entity {unitEntity} is not alive");
                    continue;
                }

                if (!unitEntity.EntityHas<UnitComponent>(world))
                {
                    Debug.LogError($"entity {unitEntity} is not unit");
                    continue;
                }
                
                if (poolInputShot.Has(inputEntity))
                {
                    Shoot(unitEntity, poolInputShot.Get(inputEntity));
                }

                if (poolInputMoveDir.Has(inputEntity))
                {
                    Move(unitEntity, poolInputMoveDir.Get(inputEntity).Dir);
                }

                if (poolInputMoveTo.Has(inputEntity))
                {
                    MoveToPoint(unitEntity, poolInputMoveTo.Get(inputEntity).Value);
                }
                
                if (poolInputAction.Has(inputEntity))
                {
                    Interract(world, unitEntity);
                }
                
                if (poolInputKick.Has(inputEntity))
                {
                    var dir = poolInputKick.Get(inputEntity).dir;
                    Kick(unitEntity, dir);
                }

                if (inputEntity.EntityHas<InputMechEnterLeaveComponent>(inputWorld))
                {
                    EnterLeaveMech(unitEntity);
                }
            }
        }

        public void EnterLeaveMech(int unitEntity)
        {
            if (unitEntity.EntityHas<ControlsMechComponent>(world))
            {
                unitEntity.EntityDel<ControlsMechComponent>(world);
                return;
            }
            
            world.GetNearestEntities(unitEntity,
                unitEntity.EntityGet<PositionComponent>(world).value,
                1, ref entities, entity=> entity.EntityHas<MechComponent>(world));

            if (entities.Count == 0)
                return;
            
            var entity = entities[0];
            ref var packedEntity = ref unitEntity.EntityAdd<ControlsMechComponent>(world).PackedEntity;
            packedEntity = world.PackEntity(entity);
        }
        
        public void Interract(EcsWorld world, int unitEntity)
        {
            world.GetNearestEntities(unitEntity,
                unitEntity.EntityGet<PositionComponent>(world).value,
                1, ref entities, entity=> entity.EntityHas<InteractableComponent>(world));

            if (entities.Count == 0)
                return;
            
            var entity = entities[0];

            if (entity.EntityHas<SpawnGunComponent>(world))
            {
                unitEntity.EntityGetOrCreateRef<WeaponComponent>(world);
            }
            
            if (entity.EntityHas<BushComponent>(world))
            {
                entity.EntityDel<InteractableComponent>(world);
                unitEntity.EntityGetOrCreateRef<FoodCollectedComponent>(world).Value += 1;
                ObjectiveService.Triggered(world, entity);

                if (entity.EntityHas<CollectableComponent>(world))
                {
                    entity.EntityGetRefComponent<CollectableComponent>(world).isCollected = true;
                }
            }
        }
        
        public void Kick(int unitEntity, Vector3 dir)
        {
            if (unitEntity.EntityHas<PushingComponent>(world))
                return;
                
            unitEntity.EntityAdd<PushingComponent>(world).EndTime = world.GetTime() + 1.3f;
            unitEntity.EntityGetOrCreateRef<CantMoveComponent>(world);
            
            if (unitEntity.EntityHas<LookDirectionComponent>(world) && 
                !unitEntity.EntityHas<ApplyForceComponent>(world))
            {
                ref var component = ref unitEntity.EntityAdd<ApplyForceComponent>(world);
                component.Time = world.GetTime() + 1f;
                var angle = Math.PI / 8f;
                var rotated = new Vector3();
                rotated.x = (float)(dir.x * Math.Cos(angle) - dir.z * Math.Sin(angle));
                rotated.z = (float)(dir.x * Math.Sin(angle) + dir.z * Math.Cos(angle));
                component.Direction = rotated;
            }
        }
        
        private void Shoot(int unitEntity, InputShotComponent shoot)
        {
            if (unitEntity.EntityHas<ShootingComponent>(world) &&
                !unitEntity.EntityGet<ShootingComponent>(world).ShootMade)
            {
                //Debug.Log("skip");
                return;
            }

            world.Log($"input shot");

            //unitEntity.EntityGetOrCreateRef<CantMoveComponent>(world);
            unitEntity.EntityAdd<ShootStartedComponent>(world);
            unitEntity.EntityReplace(world, new ShootingComponent
            {
                Direction = shoot.dir,
                Position = shoot.pos,
                ShootAtTime = world.GetTime() + 0.2f, 
                TotalTime = world.GetTime() + 0.5f
            });
        }


        private int GetControlledEntity(int unitEntity)
        {
            if (unitEntity.EntityHas<ControlsMechComponent>(world))
            {
                int mechEntity;
                if (unitEntity.EntityGet<ControlsMechComponent>(world).PackedEntity.Unpack(world, out mechEntity))
                    return mechEntity;
            }

            return unitEntity;
        }
        
        private void Move(int unitEntity, Vector3 dir)
        {
            var entity = GetControlledEntity(unitEntity);

            if (entity.EntityHas<CantMoveComponent>(world))
                return;
                    
            if (dir.sqrMagnitude > 0.001f)
            {
                entity.EntityDel<TargetPositionComponent>(world);
                entity.EntityGetOrCreateRef<MoveDirectionComponent>(world).value = dir;
            }
            else
            {
                if (entity.EntityHas<MoveDirectionComponent>(world))
                {
                    entity.EntityDel<MoveDirectionComponent>(world);
                    entity.EntityDel<MovingComponent>(world);
                }
            }
        }

        private void MoveToPoint(int unitEntity, Vector3 pos)
        {
            var entity = GetControlledEntity(unitEntity);
            
            ref var targetPositionComponent = ref entity.EntityGetOrCreateRef<TargetPositionComponent>(world);
            targetPositionComponent.Value = pos;
            
        }
    }
}