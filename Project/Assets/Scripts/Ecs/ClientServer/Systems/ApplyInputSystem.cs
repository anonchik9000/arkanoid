using System;
using System.Collections.Generic;
using Game.ClientServer;
using Game.ClientServer.Services;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Input;
using Game.Ecs.ClientServer.Components.Inventory;
using UnityEngine;
using XFlow.Ecs.ClientServer;
using XFlow.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;
using XFlow.Modules.Grid.Other;
using XFlow.Modules.Inventory.ClientServer.Components;
using XFlow.Modules.Mech.ClientServer.Components;
using XFlow.Modules.Tick.Other;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Net.ClientServer.Ecs.Components.Input;
using XFlow.Net.ClientServer.Services;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class ApplyInputSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilter _filter;
        private EcsWorld _world;
        private EcsWorld _inputWorld;

        private MyInventoryService _inventoryService;
        
        private List<int> _entities = new List<int>();

        public ApplyInputSystem(MyInventoryService myInventoryService)
        {
            _inventoryService = myInventoryService;
        }
        
        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _inputWorld = systems.GetWorld(EcsWorlds.Input);
            _filter = _inputWorld.Filter<InputTypeComponent>().Inc<InputTickComponent>().End();
        }
        
        public void Run(EcsSystems systems)
        {
            var poolInputShot   = _inputWorld.GetPool<InputShotComponent>();
            var poolPlayer      = _inputWorld.GetPool<InputPlayerEntityComponent>();
            var poolPlayerName = _inputWorld.GetPool<InputPlayerNameComponent>();
            var poolInputMoveDir= _inputWorld.GetPool<InputMoveDirectionComponent>();
            var poolInputMoveTo = _inputWorld.GetPool<InputMoveToPointComponent>();
            var poolMoveItem = _inputWorld.GetPool<MoveItemComponent>();
            var poolClearInventory = _inputWorld.GetPool<ClearInventoryComponent>();
            var poolInputAction = _inputWorld.GetPool<InputActionComponent>();
            var poolInputKick   = _inputWorld.GetPool<InputKickComponent>();
            var poolInputTick   = _inputWorld.GetPool<InputTickComponent>();
            var poolInputType  = _inputWorld.GetPool<InputTypeComponent>();

            var tick = _world.GetTick();
            
            foreach (var inputEntity in _filter)
            {
                if (poolInputTick.Get(inputEntity).Tick != tick)
                    continue;

                if (!poolPlayer.Get(inputEntity).Value.Unpack(_world, out int playerEntity)) 
                    continue;
                
                if (!PlayerService.TryGetControlledEntity(_world, playerEntity, out int unitEntity))
                    continue;
                
                var inputType = poolInputType.Get(inputEntity).Value;

                if (inputType == typeof(InputShotComponent))
                {
                    Shoot(unitEntity, poolInputShot.Get(inputEntity));
                }
                if(poolPlayerName.Has(inputEntity))
                {
                    SetPlayerName(unitEntity,poolPlayerName.Get(inputEntity).Name.ToString());
                }
                if (poolInputMoveDir.Has(inputEntity))
                {
                    Move(unitEntity, poolInputMoveDir.Get(inputEntity).Direction);
                }

                if (poolInputMoveTo.Has(inputEntity))
                {
                    MoveToPoint(unitEntity, poolInputMoveTo.Get(inputEntity).Value);
                }
                
                if (poolInputAction.Has(inputEntity))
                {
                    Interract(_world, unitEntity);
                }
                
                if (poolInputKick.Has(inputEntity))
                {
                    var dir = poolInputKick.Get(inputEntity).Direction;
                    Kick(unitEntity, dir);
                }

                if (inputEntity.EntityHas<InputMechEnterLeaveComponent>(_inputWorld))
                {
                    EnterLeaveMech(playerEntity, unitEntity);
                }

                if (inputEntity.EntityHas<MoveItemComponent>(_inputWorld))
                {
                    MoveItem(poolMoveItem.Get(inputEntity));
                }

                if (inputEntity.EntityHas<ClearInventoryComponent>(_inputWorld))
                {
                    ClearInventory(poolClearInventory.Get(inputEntity));
                }
            }
        }

       

        public void EnterLeaveMech(int playerEntity, int unitEntity)
        {
            if (unitEntity.EntityHas<MechComponent>(_world))
            {
                playerEntity.EntityGetRef<ControlledEntityComponent>(_world).Value =
                    playerEntity.EntityGet<PrimaryUnitEntityComponent>(_world).Value;
                
                return;
            }
            
            _world.GetNearestEntities(unitEntity,
                unitEntity.EntityGet<PositionComponent>(_world).Value,
                1, ref _entities, entity=> entity.EntityHas<MechComponent>(_world));

            if (_entities.Count == 0)
                return;
            
            var mechEntity = _entities[0];
            playerEntity.EntityGetRef<ControlledEntityComponent>(_world).Value = _world.PackEntity(mechEntity);
        }
        
        public void Interract(EcsWorld world, int unitEntity)
        {
            world.GetNearestEntities(unitEntity,
                unitEntity.EntityGet<PositionComponent>(world).Value,
                1, ref _entities, entity=> entity.EntityHas<InteractableComponent>(world));

            if (_entities.Count == 0)
                return;
            
            var entity = _entities[0];
            if (entity.EntityHas<SpawnGunComponent>(world))
            {
                unitEntity.EntityGetOrCreateRef<WeaponComponent>(world);
                //TryHideCollected(entity);
            }
            
            if (entity.EntityHas<BushComponent>(world))
            {
                entity.EntityDel<InteractableComponent>(world);
                unitEntity.EntityGetOrCreateRef<FoodCollectedComponent>(world).Value += 1;
                ObjectiveService.Triggered(world, entity);


                if (entity.EntityHas<CollectableComponent>(world))
                {
                    entity.EntityGetRef<CollectableComponent>(world).IsCollected = true;
                }

                if (!unitEntity.EntityHas<InventoryLinkComponent>(world))
                {
                    return;
                }

                var inventoryLink = unitEntity.EntityGet<InventoryLinkComponent>(world);
                if (!inventoryLink.Inventory.Unpack(world, out var inventory))
                {
                    return;
                }

                var berryEntity = world.NewEntity();
                berryEntity.EntityAdd<AmountComponent>(world).Value = 1;
                berryEntity.EntityAdd<ItemIdComponent>(world).ItemId = "Berry";

                _inventoryService.Add(world, inventory, berryEntity);
            }

            if (entity.EntityHas<AmmoComponent>(world))
            {
                unitEntity.EntityGetOrCreateRef<AmmoCollectedComponent>(world).Value += 10;
                //entity.EntityDel<InteractableComponent>(world);
                //TryHideCollected(entity);
            }
        }

        void TryHideCollected(int entity)
        {
            if (entity.EntityHas<CollectableComponent>(_world))
            {
                entity.EntityGetRef<CollectableComponent>(_world).IsCollected = true;
            }
        }
        
        public void Kick(int unitEntity, Vector3 dir)
        {
            if (unitEntity.EntityHas<PushingComponent>(_world))
                return;
                
            unitEntity.EntityAdd<PushingComponent>(_world).EndTime = _world.GetTime() + 1.3f;
            unitEntity.EntityGetOrCreateRef<CantMoveComponent>(_world);
            
            if (unitEntity.EntityHas<LookDirectionComponent>(_world) && 
                !unitEntity.EntityHas<ApplyForceComponent>(_world))
            {
                ref var component = ref unitEntity.EntityAdd<ApplyForceComponent>(_world);
                component.Time = _world.GetTime() + 1f;
                var angle = Math.PI / 8f;
                var rotated = new Vector3();
                rotated.x = (float)(dir.x * Math.Cos(angle) - dir.z * Math.Sin(angle));
                rotated.z = (float)(dir.x * Math.Sin(angle) + dir.z * Math.Cos(angle));
                component.Direction = rotated;
            }
        }
        
        private void Shoot(int unitEntity, InputShotComponent shoot)
        {
            if (unitEntity.EntityHas<ShootingComponent>(_world) &&
                !unitEntity.EntityGet<ShootingComponent>(_world).ShootMade)
            {
                //Debug.Log("skip");
                return;
            }
            
            if (unitEntity.EntityHas<AmmoCollectedComponent>(_world) &&
                unitEntity.EntityGet<AmmoCollectedComponent>(_world).Value <= 0)
            {
                return;
            }

            _world.LogVerbose($"input shot");

            //unitEntity.EntityGetOrCreateRef<CantMoveComponent>(world);
            unitEntity.EntityAdd<ShootStartedComponent>(_world);
            unitEntity.EntityReplace(_world, new ShootingComponent
            {
                Direction = shoot.Direction,
                Position = shoot.Position,
                ShootAtTime = _world.GetTime() + 0.2f, 
                TotalTime = _world.GetTime() + 0.5f
            });
        }

        private void SetPlayerName(int entity,string name)
        {
            entity.EntityGetOrCreateRef<ArkanoidPlayerNameComponent>(_world).Name = name;
        }

        private void Move(int entity, Vector3 dir)
        {
            if (entity.EntityHas<CantMoveComponent>(_world))
                return;
                    
            if (dir.sqrMagnitude > 0.001f)
            {
                entity.EntityDel<TargetPositionComponent>(_world);
                entity.EntityGetOrCreateRef<MoveDirectionComponent>(_world).Value = dir;
            }
            else
            {
                if (entity.EntityHas<MoveDirectionComponent>(_world))
                {
                    entity.EntityDel<MoveDirectionComponent>(_world);
                    entity.EntityDel<MovingComponent>(_world);
                }
            }
        }

        private void MoveToPoint(int entity, Vector3 pos)
        {
            ref var targetPositionComponent = ref entity.EntityGetOrCreateRef<TargetPositionComponent>(_world);
            targetPositionComponent.Value = pos;
        }
        
        private void MoveItem(MoveItemComponent data)
        {
            if (!data.Inventory.Unpack(_world, out var inventory))
            {
                return;
            }
            
            if (!data.Item.Unpack(_world, out var item))
            {
                return;
            }

            if (data.Amount == -1)
            {
                _inventoryService.Add(_world, inventory, item);
                return;
            }

            _inventoryService.Add(_world, inventory, item, data.Amount);
        }

        private void ClearInventory(ClearInventoryComponent data)
        {
            if (!data.Inventory.Unpack(_world, out var inventory))
            {
                return;
            }

            _inventoryService.Clear(_world, inventory);
        }
    }
}
