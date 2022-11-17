using System;
using UnityEngine;

namespace Game.View
{
    public class BlockView : MonoBehaviour
    {
        public GameObject BallCreatedView;

        public void Init(int ballCreated)
        {
            BallCreatedView.SetActive(ballCreated > 0);
        }
    }
}
