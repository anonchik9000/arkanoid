using Game.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Modules.Tick.Other;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class SetVelocitySystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsPool<SetVelocityComponent> _setVelocityPool;
        private EcsFilter _filter;
        private EcsWorld _world;

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _setVelocityPool = _world.GetPool<SetVelocityComponent>();
            _filter = _world.Filter<SetVelocityComponent>().Inc<Box2DBodyComponent>().End();
        }

        public void Run(EcsSystems systems)
        {

            foreach (var entity in _filter)
            {
                
                var body = Box2DServices.GetBodyRefFromEntity(_world, entity);

                var velocity = _setVelocityPool.Get(entity).Velocity;
                
                Box2DApiSafe.SetLinearVelocity(body, velocity);

                _setVelocityPool.Del(entity);
            }
        }
    }
}
