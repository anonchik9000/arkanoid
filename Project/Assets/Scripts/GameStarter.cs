using Game.State;
using Game.UI;
using Game.View;
using UnityEngine;
using XFlow.Modules.States;
using Zenject;

namespace Game
{
    public class GameStarter : MonoBehaviour
    {
        [Inject]
        private States _states;

        [Inject] private RootState _rootState;
        [Inject] private MechInfoState _mechInfoState;
        [Inject] private InventoryOpenedState _inventoryOpenedState;
    
    
        // Start is called before the first frame update
        void Start()
        {
            RegisterStateWithUI(_mechInfoState);
            _states.RegisterState(_inventoryOpenedState);
            
            _states.RegisterState(_rootState);
        
            _states.StartFrom<RootState>();

            _states.Push<MechInfoState>();
        }

        private void RegisterStateWithUI<T>(StateWithUI<T> state) where T:BaseUIView
        {
            state.GetView().gameObject.SetActive(false);
            _states.RegisterState(state);
        }
    }
}
