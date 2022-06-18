﻿using System;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct ApplyForceComponent
    {
        public float Time;
        public Vector3 Direction;
    }
    
    [Serializable]
    public struct MakeShotComponent
    {
        public float Time;
        public Vector3 Direction;
    }
    
    [EmptyComponent]
    [Serializable]
    public struct CantMoveComponent
    {
        
    }
}