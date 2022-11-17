using Game.View;
using UnityEngine;
using XFlow.Ecs.Client.Components;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Mech.ClientServer.Components;
using XFlow.Utils;

namespace XFlow.Modules.Mech.Client.Systems
{
    public class ArkanoidViewSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorld _world;
        private EcsFilter _filterBall;
        private EcsFilter _filterBlock;
        private EcsPool<ArkanoidBlockComponent> _poolBlock;
        private GameObject _ballPrefab;
        private GameObject _blockPrefab;

        public void Init(EcsSystems systems)
        {
            _ballPrefab = Resources.Load<GameObject>("ArkanoidBall");
            _blockPrefab = Resources.Load<GameObject>("ArkanoidBlock");
            _world = systems.GetWorld();
            _filterBall = _world.Filter<ArkanoidBallComponent>()
                .Exc<TransformComponent>().End();

            _filterBlock = _world.Filter<ArkanoidBlockComponent>()
                .Exc<TransformComponent>().End();

            _poolBlock = _world.GetPool<ArkanoidBlockComponent>();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filterBall)
            {
                CreateView(entity, _ballPrefab);
            }
            foreach (var entity in _filterBlock)
            {
                CreateView(entity, _blockPrefab);
            }
        }

        private void CreateView(int entity,GameObject prefab)
        {
            var go = GameObject.Instantiate(prefab);
            entity.EntityAdd<TransformComponent>(_world).Transform = go.transform;
            go.transform.position = entity.EntityGet<PositionComponent>(_world).Value;
            if(_poolBlock.Has(entity) && _poolBlock.Get(entity).BallCreateCount>0)
            {
                go.GetComponent<BlockView>().Init(_poolBlock.Get(entity).BallCreateCount);
            }
        }
    }
}
