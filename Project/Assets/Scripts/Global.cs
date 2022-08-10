﻿using Game.UIView;
using Game.View;
using UnityEngine;
using XFlow.Modules.Inventory.Client.Demo.StaticData;

namespace Game
{
    public class Global : MonoBehaviour
    {
        public CharacterView CharacterPrefab;
        public BulletView BulletPrefab;

        [Header("Footprint")] 
        public FootprintView LeftFootprintPrefab;
        public FootprintView RightFootprintPrefab;

        [Space] public HighlightView highlightView;

        public ParticleSystem FireParticles;

        public MechInfoView MechInfoView;

        public InventoryStaticData InventoryStaticData;
    }
}