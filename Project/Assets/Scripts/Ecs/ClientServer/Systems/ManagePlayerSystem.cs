using Game.ClientServer.Services;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Inventory;
using XFlow.EcsLite;
using XFlow.Modules.Inventory.ClientServer.Components;
using XFlow.Modules.Mech.ClientServer.Components;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class ManagePlayerSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _mainWorld;
        private EcsFilter _filterNewPlayers;
        private EcsFilter _filterDeletedPlayers;
        
        public void Init(EcsSystems systems)
        {
            _mainWorld = systems.GetWorld();
            _filterNewPlayers = _mainWorld.Filter<NewPlayerComponent>().End();
            _filterDeletedPlayers = _mainWorld.FilterMarkedDeleted().Inc<PlayerComponent>().End();
        }
        
        public void Run(EcsSystems systems)
        {
            foreach (var playerEntity in _filterNewPlayers)
            {
                var unitEntity = UnitService.CreateUnitEntity(_mainWorld);

                playerEntity.EntityAdd<ControlledEntityComponent>(_mainWorld).Value =
                    _mainWorld.PackEntity(unitEntity);
                
                playerEntity.EntityAdd<PrimaryUnitEntityComponent>(_mainWorld).Value =
                    _mainWorld.PackEntity(unitEntity);

                var inventory = _mainWorld.NewEntity();
                inventory.EntityAdd<InventoryComponent>(_mainWorld).SlotCapacity = 10;

                var trash = _mainWorld.NewEntity();
                trash.EntityAdd<InventoryComponent>(_mainWorld).SlotCapacity = 10;

                unitEntity.EntityAdd<InventoryLinkComponent>(_mainWorld).Inventory =
                    _mainWorld.PackEntity(inventory);
                unitEntity.EntityAdd<TrashLinkComponent>(_mainWorld).Trash = _mainWorld.PackEntity(trash);
                
                playerEntity.EntityDel<NewPlayerComponent>(_mainWorld);
            }

            foreach (var playerEntity in _filterDeletedPlayers)
            {
                var packedEntity = playerEntity.EntityGet<PrimaryUnitEntityComponent>(_mainWorld).Value;
                if (packedEntity.Unpack(_mainWorld, out int unitEntity))
                {
                    _mainWorld.MarkEntityAsDeleted(unitEntity);

                    if (unitEntity.EntityTryGet<InventoryLinkComponent>(_mainWorld, out var link))
                    {
                        if (link.Inventory.Unpack(_mainWorld, out int entity))
                        {
                            _mainWorld.MarkEntityAsDeleted(entity);
                        }
                    }
                    
                    if (unitEntity.EntityTryGet<TrashLinkComponent>(_mainWorld, out var trashLink))
                    {
                        if (trashLink.Trash.Unpack(_mainWorld, out int entity))
                        {
                            _mainWorld.MarkEntityAsDeleted(entity);
                        }
                    }
                }
            }
        }

    }
}