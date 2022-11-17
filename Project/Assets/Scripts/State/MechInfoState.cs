using Game.Client.Services;
using Game.Ecs.ClientServer.Components;
using Game.UI;
using Game.UIView;
using UnityEngine;
using XFlow.EcsLite;
using XFlow.Modules.Mech.ClientServer.Components;
using XFlow.Modules.States;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Utils;

namespace Game.State
{
    public class MechInfoState : StateWithUI<MechInfoView> 
        //,EventsSystem<ControlledEntityComponent>.IComponentChangedListener
    {
        private EcsWorld _world;
        private PlayerControlService _playerControlService;

        private EcsPackedEntity _playerEntity;
        
        public MechInfoState(
            States states,
            EcsWorld world, 
            MechInfoView view,
            PlayerControlService playerControlService):base(states)
        {
            this._view = view;
            this._world = world;
            this._playerControlService = playerControlService;
        }

        protected override void DoInitialize()
        {
            
            _view.ButtonSetName.onClick.AddListener(() =>
            {
                _playerControlService.SetPlayerName(_view.NameInput.text);
                Close();
            });
            
        }

        //protected override void DoEnter()
        //{
        //    if (!ClientPlayerService.TryGetPlayerEntity(_world, out int playerEntity))
        //        return;
        //    playerEntity.AddChangedListener<ControlledEntityComponent>(_world, this);
            
        //    _playerEntity = _world.PackEntity(playerEntity);
        //}

        //protected override void DoExit()
        //{
        //    if (!_playerEntity.Unpack(_world, out int playerEntity))
        //        return;
        //    playerEntity.DelChangedListener<ControlledEntityComponent>(_world, this);
        //}



        //public void OnComponentChanged(EcsWorld world, int entity, ControlledEntityComponent data, bool newComponent)
        //{
        //    Close();
        //}
    }
}