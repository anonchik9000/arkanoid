using Game.Ecs.ClientServer.Components;

using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Modules.Mech.ClientServer.Components;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class ArkanoidGameSystem : IEcsInitSystem,IEcsRunSystem
    {
        private EcsFilter _filterBall;
        private EcsFilter _filterBlock;
        private EcsFilter _filterCreateBall;
        private EcsPool<ArkanoidBallResetComponent> _poolBalls;
        private EcsPool<PositionComponent> _poolPositions;
        private EcsPool<Box2DBodyComponent> _poolBody;
        private EcsPool<ArkanoidCreateBallComponent> _poolCreatedBall;
        
        private EcsWorld _world;
        
        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();

            _world.AddUnique<ArkanoidScoresComponent>().Value = 0;

            CreateBall(new Vector3(-3, 0.6f, 1.8f));

            _filterCreateBall = _world.Filter<ArkanoidCreateBallComponent>().End();
            _filterBall = _world.Filter<ArkanoidBallResetComponent>().End();
            _filterBlock = _world.Filter<ArkanoidBlockComponent>().Exc<Box2DBodyComponent>().End();
            _poolBalls = _world.GetPool<ArkanoidBallResetComponent>();
            _poolPositions = _world.GetPool<PositionComponent>();
            _poolBody = _world.GetPool<Box2DBodyComponent>();
            _poolCreatedBall = _world.GetPool<ArkanoidCreateBallComponent>();

            _world.GetOrCreateUniqueRef<AverageSpeedComponent>().Value = 8f; //clip.averageSpeed;
            
            
        }

        private void CreateBall(Vector3 position)
        {
            var arkanoidBall = _world.NewEntity();
            arkanoidBall.EntityAdd<ArkanoidBallComponent>(_world);
            arkanoidBall.EntityAdd<Rotation2DComponent>(_world);
            arkanoidBall.EntityAdd<PositionComponent>(_world).Value = position;
            arkanoidBall.EntityAdd<ArkanoidBallResetComponent>(_world);
            arkanoidBall.EntityAdd<AverageSpeedComponent>(_world).Value = 20;
            Box2DServices.AddRigidbodyDefinition(_world, arkanoidBall, BodyType.Dynamic).
                SetAngularDamping(0).
                SetFriction(0).
                SetDensity(1).
                SetLinearDamping(0).
                SetRestitution(1);
            Box2DServices.AddCircleColliderToDefinition(_world, arkanoidBall, 0.5f, new Vector3(0, 0f));

        }
        

        public void Run(EcsSystems systems)
        {
            foreach(var entity in _filterBall)
            {
                System.IntPtr body;
                float forceValue = 200;
                if (_poolBody.Has(entity))
                {
                    body = _poolBody.Get(entity).BodyReference;
                }
                else
                {
                    body = Box2DServices.CreateBodyNow(_world, entity);
                }
                var dir = new Vector3(Random.Range(-0.5f,0.5f),0, 1).normalized;
                var force = dir.ToVector2XZ() * forceValue;
                var pos = _poolPositions.Get(entity).Value;
                Box2DApiSafe.SetLinearVelocity(body, Vector2.zero);
                Box2DApiSafe.SetPosition(body, pos.ToVector2XZ(), true);
                Box2DApiSafe.ApplyForce(body, force, pos);
                _poolBalls.Del(entity);
            }
            foreach(var entity in _filterBlock)
            {
                ref var pos = ref _poolPositions.GetRef(entity);
                var body = Box2DServices.CreateBodyNow(_world, entity);
                Box2DApiSafe.SetPosition(body, pos.Value.ToVector2XZ(), true);
            }
            foreach(var entity in _filterCreateBall)
            {
                var contact = _poolCreatedBall.Get(entity);
                int ballCount = contact.Count;
                for (int i = 0; i < ballCount; i++)
                {
                    CreateBall(contact.Positon);
                }
                _world.DelEntity(entity);
            }
        }
    }
}