﻿using XFlow.EcsLite;
using XFlow.Modules.Inventory.ClientServer;
using XFlow.Modules.Inventory.ClientServer.Components;
using XFlow.Utils;

namespace Game
{
    public class MyInventoryService : InventoryService
    {
        public override bool IsItemStackable(EcsWorld world, int itemEntity)
        {
            var poolStackable = world.GetPool<StackableComponent>();
            return poolStackable.Has(itemEntity);
        }

        protected override int CreateStackableSlotEntity(EcsWorld world, int fromItemEntity, int storageEntity)
        {
            var itemEntity = world.NewEntity();
            itemEntity.EntityAddComponent<StackableComponent>(world);
            return itemEntity;
        }

        protected override int CreateUniqueSlotEntity(EcsWorld world, int fromItemEntity, int storageEntity)
        {
            var itemEntity = world.NewEntity();
            return itemEntity;
        }
    }
}