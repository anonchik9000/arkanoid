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
        private EcsFilter _ballFilter;
        private EcsFilter _blockFilter;
        private EcsFilter _createBallFilter;
        private EcsPool<ArkanoidBallResetComponent> _ballsPool;
        private EcsPool<PositionComponent> _positionsPool;
        private EcsPool<Box2DBodyComponent> _bodyPool;
        private EcsPool<ArkanoidCreateBallComponent> _createdBallPool;
        
        private EcsWorld _world;
        
        public void Init(EcsSystems systems)
        {
            CreateBall(new Vector3(-3, 0.6f, 1.8f));

            _createBallFilter = _world.Filter<ArkanoidCreateBallComponent>().End();
            _ballFilter = _world.Filter<ArkanoidBallResetComponent>().End();
            _blockFilter = _world.Filter<ArkanoidBlockComponent>().Exc<Box2DBodyComponent>().End();
            _ballsPool = _world.GetPool<ArkanoidBallResetComponent>();
            _positionsPool = _world.GetPool<PositionComponent>();
            _bodyPool = _world.GetPool<Box2DBodyComponent>();
            _createdBallPool = _world.GetPool<ArkanoidCreateBallComponent>();

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
            foreach(var entity in _ballFilter)
            {
                System.IntPtr body;
                float forceValue = 200;
                if (_bodyPool.Has(entity))
                {
                    body = _bodyPool.Get(entity).BodyReference;
                    forceValue = 350;
                }
                else
                {
                    body = Box2DServices.CreateBodyNow(_world, entity);
                }
                var dir = new Vector3(Random.Range(-0.5f,0.5f),0, 1).normalized;
                var force = dir.ToVector2XZ() * forceValue;
                var pos = _positionsPool.Get(entity).Value;
                Box2DApiSafe.SetLinearVelocity(body, Vector2.zero);
                Box2DApiSafe.SetPosition(body, pos.ToVector2XZ(), true);
                Box2DApiSafe.ApplyForce(body, force, pos);
                _ballsPool.Del(entity);
            }
            foreach(var entity in _blockFilter)
            {
                ref var pos = ref _positionsPool.GetRef(entity);
                var body = Box2DServices.CreateBodyNow(_world, entity);
                Box2DApiSafe.SetPosition(body, pos.Value.ToVector2XZ(), true);
            }
            foreach(var entity in _createBallFilter)
            {
                var contact = _createdBallPool.Get(entity);
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