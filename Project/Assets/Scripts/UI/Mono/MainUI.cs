using System;
using Game.UIView;
using Game.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XFlow.Modules.States;

namespace Game.UI.Mono
{
    public class MainUI : MonoBehaviour
    {
        public TextMeshProUGUI Score;
        public Button ShotButton;
        public Button InteractionButton;
        public Button InventoryButton;
        public Button KickButton;
        public Button MechButton;
        //public FixedJoystick Joystick;
        public FloatingJoystick Joystick;

        public RectTransform ObjectivesRectTransform;
        public GridLayoutGroup InventoryGrid;
        public ConnectView Connect;
        public TMP_Text FoodText;
        public TMP_Text AmmoText;

        public MechInfoView mechInfo;
        public BaseUIView[] UIViews;

        public Action OnLateUpdate;

        public void LateUpdate()
        {
            OnLateUpdate?.Invoke();
        }
    }
}