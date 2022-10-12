﻿using Gaming.ContainerManager.ImageContracts.V1;
using XFlow.EcsLite;

namespace XFlow.Server
{
    struct ClientComponent
    {
        //public int ID;
        
        public int Delay;
        public int DelayMs;
        
        
        public int LastPingTick;
        public int LastClientTick;
        public int LastServerTick;
        public string UserAddressId;
        public IUserAddress ReliableAddress;
        public IUserAddress UnreliableAddress;

        public EcsWorld SentWorld;
        public EcsWorld SentWorldRelaible;
    }
}