using Game.ClientServer.Services;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Input;
using Game.UI;
using Game.View;
using System;
using UnityEngine;
using XFlow.Ecs.Client.Components;
using XFlow.Ecs.ClientServer;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Mech.ClientServer.Components;
using XFlow.Modules.Tick.Other;
using XFlow.Net.Client;
using XFlow.Net.Client.Services;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Utils;
using Zenject;

namespace Game.Client.Services
{
    public class PlayerControlService 
    {
        private EcsWorld _inputWorld;
        private EcsWorld _world;

        private IClientInputService _inputService;
        
        public PlayerControlService(
            [Inject(Id = EcsWorlds.Input)] EcsWorld inputWorld,
            [InjectOptional] IClientInputService inputService,
            EcsWorld world)
        {
            this._inputWorld = inputWorld;
            this._world = world;
            _inputService = inputService;
        }
        
        private int unitEntity
        {
            get
            {
                int entity = -1;
                ClientPlayerService.TryGetControlledEntity(_world, out entity);
                return entity;
            }
        }

        public void SetPlayerName(string name)
        {
            int playerEntity = unitEntity;
            if (playerEntity >= 0)
            {
                playerEntity.EntityGetOrCreateRef<ArkanoidPlayerNameComponent>(_world).Name = name;
            }
        }

        private int tick => _world.GetTick();
        
        public void Shot()
        {
            if (unitEntity == -1)
            {
                Debug.LogWarning("unitEntity == -1");
                return;
            }

            var view = unitEntity.EntityGet<TransformComponent>(_world).Transform.GetComponent<CharacterView>();
            //view.
            
            var component = new InputShotComponent();
            var lookDir = _world.EntityGet<LookDirectionComponent>(unitEntity).Value;
            var dir = Quaternion.Euler(0, -0, 0) * lookDir;
            component.Direction = dir;
            component.Position = view.BulletSpawnPos.transform.position; 
            
            Apply(component);
        }
        
        public void Interact()
        {
            if (unitEntity == -1)
            {
                Debug.LogWarning("unitEntity == -1");
                return;
            }
            
            var component = new InputActionComponent();
            Apply(component);
        }
        
        public void Kick()
        {
            if (unitEntity == -1)
            {
                Debug.LogWarning("unitEntity == -1");
                return;
            }
            
            var component = new InputKickComponent();
            component.Direction = _world.EntityGet<LookDirectionComponent>(unitEntity).Value;
            
            Apply(component);
        }
        
        public void MechEnterLeave()
        {
            if (unitEntity == -1)
            {
                Debug.LogWarning("unitEntity == -1");
                return;
            }

            Apply(new InputMechEnterLeaveComponent());
        }

        
        
        public void MoveToDirection(Vector3 dir)
        {
            if (unitEntity == -1)
            {
                Debug.LogWarning("unitEntity == -1");
                return;
            }
            
            var component = new InputMoveDirectionComponent();
            component.Direction = dir;
            
            Apply(component);
        }
        
        public void StopMoveToDirection()
        {
            if (unitEntity == -1)
            {
                Debug.LogWarning("unitEntity == -1");
                return;
            }

            
            if (!unitEntity.EntityHas<MoveDirectionComponent>(_world))
                return;
            
            var component = new InputMoveDirectionComponent();
            component.Direction = Vector3.zero;
            
            Apply(component);
        }
        public void MoveToPoint(Vector3 pos)
        {
            if (unitEntity == -1)
            {
                Debug.LogWarning("unitEntity == -1");
                return;
            }

            var component = new InputMoveToPointComponent();
            component.Value = pos;
            Apply(component);
        }

        public void BeginDrag(int entity)
        {
            Debug.Log($"BeginDrag {entity}");
            var component = new InputBeginMouseDragComponent();
            component.Entity = _world.PackEntity(entity);
            Apply(component);
        }

        public void UpdateDrag(Vector3 pos)
        {
            var component = new InputUpdateMouseDragComponent();
            component.Position = pos.ToVector2XZ();
            Apply(component);
        }
        
        public void EndDrag()
        {
            Debug.Log($"EndDrag");
            var component = new InputEndMouseDragComponent();
            Apply(component);
        }

        public void Apply(IInputComponent component)
        {
            _inputService.Input(_world, _inputWorld, tick, component);
        }
    }
}