using Fabros.Library.States;
using Game.State;
using Game.UI;
using Game.View;
using UnityEngine;
using Zenject;

namespace Game
{
    public class GameStarter : MonoBehaviour
    {
        [Inject]
        private States states;

        [Inject] private RootState rootState;
        [Inject] private MechInfoState mechInfoState;
    
    
        // Start is called before the first frame update
        void Start()
        {
            RegisterStateWithUI(mechInfoState);
            states.RegisterState(rootState);
        
            states.StartFrom<RootState>();
        }

        private void RegisterStateWithUI<T>(StateWithUI<T> state) where T:BaseUIView
        {
            state.GetView().gameObject.SetActive(false);
            states.RegisterState(state);
        }
    }
}
