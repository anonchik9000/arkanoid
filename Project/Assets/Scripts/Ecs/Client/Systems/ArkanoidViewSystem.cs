using Game.Ecs.ClientServer.Components;
using UnityEngine;
using XFlow.Ecs.Client.Components;
using XFlow.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;
using XFlow.Modules.Mech.Client.Components;
using XFlow.Modules.Mech.ClientServer.Components;
using XFlow.Utils;

namespace XFlow.Modules.Mech.Client.Systems
{
    public class ArkanoidViewSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorld _world;
        private EcsFilter _ballFilter;
        private EcsFilter _blockFilter;
        private EcsPool<ArkanoidBlockComponent> _blockPool;
        private GameObject _ballPrefab;
        private GameObject _blockPrefab;

        public void Init(EcsSystems systems)
        {
            _ballPrefab = Resources.Load<GameObject>("ArkanoidBall");
            _blockPrefab = Resources.Load<GameObject>("ArkanoidBlock");
            _world = systems.GetWorld();
            _ballFilter = _world.Filter<ArkanoidBallComponent>()
                .Exc<TransformComponent>().End();

            _blockFilter = _world.Filter<ArkanoidBlockComponent>()
                .Exc<TransformComponent>().End();

            _blockPool = _world.GetPool<ArkanoidBlockComponent>();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _ballFilter)
            {
                CreateView(entity, _ballPrefab);
            }
            foreach (var entity in _blockFilter)
            {
                CreateView(entity, _blockPrefab);
            }
        }

        private void CreateView(int entity,GameObject prefab)
        {
            var go = GameObject.Instantiate(prefab);
            entity.EntityAdd<TransformComponent>(_world).Transform = go.transform;
            go.transform.position = entity.EntityGet<PositionComponent>(_world).Value;
            if(_blockPool.Has(entity) && _blockPool.Get(entity).BallCreated>0)
            {
                go.transform.Find("Bonus").gameObject.SetActive(true);
            }
        }
    }
}
