﻿using System;
using System.IO;
using System.Text;
using Fabros.Ecs.ClientServer;
using Fabros.Ecs.ClientServer.Utils;
using Fabros.Ecs.ClientServer.WorldDiff;
using Fabros.EcsModules.Tick.ClientServer.Components;
using Fabros.EcsModules.Tick.Other;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components.Input;
using Game.Fabros.Net.ClientServer.Ecs.Components;
using Game.Fabros.Net.ClientServer.Protocol;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Fabros.Net.ClientServer
{
    public class LeoContexts
    {
        public ComponentsCollection Components { get; }

        //compatibility with old code
        public ComponentsCollection Pool => Components;
        /*
         * позволяет сохранять в отдельный файл игровые данные и сравнивать состояния миров между собой
        */

        
        
        private readonly string hashDir;
        private readonly bool writeHashes;
        private EcsWorld inputWorld;

        public LeoContexts(string hashDir, ComponentsCollection components, EcsWorld inputWorld)
        {
            this.hashDir = hashDir;
            this.inputWorld = inputWorld;

            Components = components;

#if UNITY_EDITOR || UNITY_STANDALONE || SERVER
            //используется для отладки
            writeHashes = Directory.Exists(hashDir);
            if (writeHashes && (hashDir.Contains("temp") || hashDir.Contains("tmp")))
            {
                Debug.Log("writeHashes");
                var files = Directory.GetFiles(hashDir);
                Array.ForEach(files, file => {
                    File.Delete(file);
                });
            }
#endif
        }


        
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public void FilterInputs(Tick time)
        {
            var filter = inputWorld.Filter<InputTickComponent>().End();
            var pool = inputWorld.GetPool<InputTickComponent>();
            foreach (var entity in filter)
            {
                var tick = pool.Get(entity).Tick;
                if (tick < time.Value)
                    inputWorld.DelEntity(entity);
            }
        }

        public void Tick(EcsSystems systems, EcsWorld inputWorld, EcsWorld world, bool writeToLog, string debug="")
        {
            //обновляем мир 1 раз
            
            var currentTick = GetCurrentTick(world);
            var sl = world.GetSyncLogger();
            sl?.BeginTick(world, currentTick.Value);
            
            var strStateDebug = "";
            if (writeHashes)
            {
                strStateDebug += WorldDumpUtils.DumpWorld(Components, inputWorld);
                strStateDebug += "\n\n\n";
            }
            
            
            //тик мира
            systems.ChangeDefaultWorld(world);
            systems.Run();

            sl?.EndTick(world, GetCurrentTick(world).Value);

            if (!writeToLog)
                return;

            currentTick = GetCurrentTick(world);

            //отладочный код, который умеет писать в файлы hash мира и его diff
            //var empty = WorldUtils.CreateWorld("empty", Pool);
            //var dif = WorldUtils.BuildDiff(Pool, empty, world, true);
            
            
            
            world.Log($"<- tick {currentTick.Value}\n");

            if (writeHashes)
            {
                //var str = JsonUtility.ToJson(dif, true);
                var strWorldDebug = WorldDumpUtils.DumpWorld(Components, world);
                var hash = CreateMD5(strWorldDebug);

                //SyncLog.WriteLine($"hash: {hash}\n");

                var str = strStateDebug + "\n>>>>>>\n" +  strWorldDebug;
                var tick = currentTick.Value.ToString("D4");

                //var tt = DateTime.UtcNow.Ticks % 10000000;
                
                using (var file = new StreamWriter($"{hashDir}/{hash}-{tick}-{world.GetDebugName()}-{debug}.txt"))
                {
                    file.Write(str);
                }
                
                using (var file = new StreamWriter($"{hashDir}/{tick}-{world.GetDebugName()}-{hash}-{debug}.txt"))
                {
                    file.Write(str);
                }
            }
        }

        public Tick GetCurrentTick(EcsWorld w)
        {
            return w.GetUnique<TickComponent>().Value;
        }
        

        public TickrateConfigComponent GetConfig(EcsWorld world)
        {
            //конфиг сервера
            return world.GetUnique<TickrateConfigComponent>();
        }
    }
}