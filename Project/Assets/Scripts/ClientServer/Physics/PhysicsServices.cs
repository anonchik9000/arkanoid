﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Fabros.Ecs.Utils;
using Game.Ecs.ClientServer.Components.Physics;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.ClientServer.Physics
{
    public static class PhysicsServices
    {
#if UNITY_IPHONE && !UNITY_EDITOR
    private const string DllName = "__Internal";
#else
        private const string DllName = "libbox2d";
#endif

        // Clone current physicsWorld, delete old, change all old body references to the new ones, return cloned world
        public static void ReplicateBox2D(EcsWorld src, EcsWorld dest)
        {
            var srcEntities = new List<int>();
            src.GetPool<BodyReferenceComponent>().GetEntities(srcEntities);
            var srcPool = src.GetPool<BodyReferenceComponent>();
            
            IntPtr[] arrayOfReferences = new IntPtr[srcEntities.Count + 111];
            for (int i = 0; i < srcEntities.Count; i++)
                arrayOfReferences[i] = srcPool.Get(srcEntities[i]).BodyReference;

            var srcWorld = src.GetUnique<PhysicsWorldComponent>().WorldReference;
            if (srcWorld == default)
                return;
            var newDestWorld = Box2DPhysics.CloneWorld(ref arrayOfReferences, srcEntities.Count, srcWorld);

            
            var destPool = dest.GetPool<BodyReferenceComponent>();
            var destPoolBodyCreated = dest.GetPool<BodyCreatedComponent>();
            
            
            if (dest.HasUnique<PhysicsWorldComponent>())
            {
                var oldDestWorld = dest.ReplaceUnique<PhysicsWorldComponent>().WorldReference;
                if (oldDestWorld != default)
                    Box2DPhysics.DestroyWorld(oldDestWorld);
                
                var destEntities = new List<int>();
                destPool.GetEntities(destEntities);

                for (int i = 0; i < destEntities.Count; i++)
                {
                    var entity = destEntities[i];
                    destPool.Del(entity);
                    destPoolBodyCreated.Del(entity);
                }
            }
            
            dest.ReplaceUnique<PhysicsWorldComponent>().WorldReference = newDestWorld;

            for (int i = 0; i < srcEntities.Count; i++)
            {
                var entity = srcEntities[i];
                destPool.Replace(entity).BodyReference = arrayOfReferences[i];
                destPoolBodyCreated.Replace(entity);
            }
            
        }

        public static IntPtr GetBodyRefFromEntity(this EcsWorld world, int entity)
        {
            IntPtr bodyRef = IntPtr.Zero;

            var ecsPool = world.GetPool<BodyReferenceComponent>();

            if (ecsPool.Has(entity))
            {
                bodyRef = ecsPool.Get(entity).BodyReference;
            }
                        
            return bodyRef;
        }
        
        [DllImport(DllName)]
        public static extern int GetEntityFromBody(IntPtr body);

        [DllImport(DllName)]
        public static extern IntPtr GetWorldFromBody(this IntPtr body);
    }
}