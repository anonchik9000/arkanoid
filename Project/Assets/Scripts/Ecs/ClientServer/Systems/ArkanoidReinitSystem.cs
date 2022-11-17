using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Modules.Mech.ClientServer.Components;
using XFlow.Utils;

namespace XFlow.Modules.Mech.ClientServer.Systems
{
    public class ArkanoidReinitSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorld _world;
        private EcsFilter _filterBall;
        private EcsFilter _filterCreated;
        private EcsPool<PositionComponent> _poolPositions;
        private const float _deadLine = -3;

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _filterBall = _world.Filter<ArkanoidBallComponent>()
                                .Inc<PositionComponent>()
                                .Exc<ArkanoidBallResetComponent>()
                                .End();

            _poolPositions = _world.GetPool<PositionComponent>();

            _filterCreated = _world.Filter<ArkanoidBlockComponent>().End();
        }

        public void Run(EcsSystems systems)
        {
            
            foreach (var entity in _filterBall)
            {
                if(_poolPositions.Get(entity).Value.z < _deadLine)
                {
                    if(_filterBall.GetEntitiesCount()==1)
                        _world.GetPool<ArkanoidBallResetComponent>().Add(entity);
                    else
                        _world.GetPool<DeletedEntityComponent>().Add(entity);
                }
            }
            if(_filterCreated.IsEmpty())
            {
                CreateBlocks();
            }
        }

        private void CreateBlocks()
        {
            Vector3 startPositon = new Vector3(-8.88f, 0, 7.13f);
            var position = startPositon;
            int countCreatedBallBlocks = 1;
            for (int x = 0; x < 3; x++)
            {
                position.z = startPositon.z;
                for (int z = 0; z < 3; z++)
                {
                    var block = _world.NewEntity();
                    block.EntityAdd<Rotation2DComponent>(_world);
                    ref var blockCmp =ref block.EntityAdd<ArkanoidBlockComponent>(_world);
                    blockCmp.BallCreateCount = countCreatedBallBlocks>0 ? 3 : 0;
                    countCreatedBallBlocks--;
                    block.EntityAdd<PositionComponent>(_world).Value = position;
                    Box2DServices.AddRigidbodyDefinition(_world, block, BodyType.Static).SetFriction(0).SetDensity(1000);
                    Box2DServices.AddBoxColliderToDefinition(_world, block, new Vector2(2.1f, 0.2f), Vector2.zero, 0);
                    position.z += 6;
                }
                position.x += 5;
            }
        }
    }
}



