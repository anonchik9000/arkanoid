using Game.Client.Services;
using Game.Ecs.ClientServer.Components;
using Game.UI.Mono;
using Game.State;
using XFlow.EcsLite;
using XFlow.Modules.Mech.ClientServer;
using XFlow.Modules.Mech.ClientServer.Components;
using XFlow.Modules.States;
using XFlow.Modules.Tick.ClientServer.Components;
using XFlow.Utils;

namespace Game.UI
{
    public class UI: 
        EventsSystem<FoodCollectedComponent>.IAnyComponentChangedListener,
        EventsSystem<AmmoCollectedComponent>.IAnyComponentChangedListener,
        EventsSystem<WeaponComponent>.IAnyComponentChangedListener, 
        EventsSystem<TickComponent>.IAnyComponentChangedListener,
        EventsSystem<ArkanoidScoresComponent>.IAnyComponentChangedListener
    {
        public MainUI View { get; private set; }
        private EcsWorld _world;
        
        private MechService _mechService;
        private NameViewManager _hpViewManager;
        private States _states;
        public UI(
            EcsWorld world, 
            MainUI view, 
            States states,
            PlayerControlService controlService,
            NameViewManager hpViewManager,
            MechService mechService
            )
        {
            View = view;
            _world = world;
            _mechService = mechService;

            

            view.OnLateUpdate = () =>
            {
                _hpViewManager.LateUpdate();
            };

            _hpViewManager = hpViewManager;

            view.InteractionButton.onClick.AddListener(() =>
            {
                controlService.Interact();
            });
            
            view.KickButton.onClick.AddListener(() =>
            {
                controlService.Kick();
            });
            
            view.MechButton.onClick.AddListener(() =>
            {
                states.Push<MechInfoState>();
            });

            if (view.InventoryButton != null)
            {
                view.InventoryButton.onClick.AddListener(() =>
                {
                    states.Push<InventoryOpenedState>();
                });
            }

            view.ShotButton.onClick.AddListener(() =>
            {
                controlService.Shot();
            });

            view.FoodText.text = "";
            view.AmmoText.text = "0";
            view.ShotButton.gameObject.SetActive(false);

            var listener = world.CreateAnyListener();
            listener.SetAnyChangedListener<WeaponComponent>(this);
            listener.SetAnyChangedListener<FoodCollectedComponent>(this);
            listener.SetAnyChangedListener<AmmoCollectedComponent>(this);
            listener.SetAnyChangedListener<TickComponent>(this);
            listener.SetAnyChangedListener<ArkanoidScoresComponent>(this);
        }

        

        
        public void OnAnyComponentChanged(EcsWorld _, int entity, FoodCollectedComponent data, bool added)
        {
            if (!ClientPlayerService.IsControlledEntity(_world, entity))
                return;
            
            View.FoodText.text = $"Food Collected {data.Value}";
        }
        
        public void OnAnyComponentChanged(EcsWorld world, int entity, AmmoCollectedComponent data, bool added)
        {
            if (!ClientPlayerService.IsControlledEntity(_world, entity))
                return;

            View.AmmoText.text = data.Value.ToString();
        }

        public void OnAnyComponentChanged(EcsWorld _, int entity, WeaponComponent data, bool added)
        {
            if (!ClientPlayerService.IsControlledEntity(_world, entity))
                return;
            
            //var transform = entity.EntityGet<TransformComponent>(world).Transform
            View.ShotButton.gameObject.SetActive(true);
        }

        public void OnAnyComponentChanged(EcsWorld _, int tickEntity, TickComponent data, bool added)
        {
            //in case if anyone added not unique TickComponent
            //skip it
            if (tickEntity != 0)
                return;
            
            if (!ClientPlayerService.TryGetControlledEntity(_world, out int controlledEntity))
                return;

            if (controlledEntity.EntityHas<MechComponent>(_world))
            {
                //leave mech
                View.MechButton.gameObject.SetActive(true);
                return;
            }
            
            bool hasMech = _mechService.TryGetInteractableMechEntity(_world, controlledEntity, out int mechEntity);
            View.MechButton.gameObject.SetActive(hasMech);
        }

        public void OnAnyComponentChanged(EcsWorld world, int entity, ArkanoidScoresComponent data, bool added)
        {
            View.Score.text = data.Value.ToString();
        }
    }
}