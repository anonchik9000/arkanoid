﻿using System;

namespace Game.Fabros.EcsModules.Box2D.ClientServer.Components
{
    [Serializable]
    public struct PhysicsConfigComponent
    {
        public int PositionIterations;
        public int VelocityIterations;
    }
}