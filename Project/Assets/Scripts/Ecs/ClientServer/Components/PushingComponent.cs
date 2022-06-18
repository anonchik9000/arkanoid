﻿using System;
using Leopotam.EcsLite;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct PushingComponent
    {
        public float EndTime;
    }
    
    [EmptyComponent]
    [Serializable]
    public struct ShootingComponent
    {
        //public float EndTime;
    }
}