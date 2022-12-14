using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.View;

using UnityEngine;
using XFlow.Ecs.Client.Components;
using XFlow.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;
using XFlow.Utils;
using Zenject;

namespace Game.Ecs.Client.Systems
{
    public class CreateViewSystem : IEcsInitSystem, IEcsRunSystem
    {
        private static int N = 0;
        private CharacterView _viewPrefab;
        private BulletView _bulletPrefab;
        private DiContainer _container;
        
        private EcsWorld world;
        private EcsFilter filter;
        
        public CreateViewSystem(CharacterView viewPrefab, BulletView bulletPrefab, DiContainer container)
        {
            _container = container;
            this._bulletPrefab = bulletPrefab;
            this._viewPrefab = viewPrefab;
        }
        
        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            filter = world.Filter<GameObjectNameComponent>()
                .Exc<TransformComponent>().End();
        }
        
        public void Run(EcsSystems systems)
        {

            foreach (var entity in filter)
            {
                var name = entity.EntityGet<GameObjectNameComponent>(world).Name.ToString();
                var go = GameObject.Find(name);
                if (go == null)
                {
                    Debug.LogError($"not found gameobject {name} for entity {entity.e2name(world)}");
                    continue;
                }
                go.SetActive(true);


                if (entity.EntityTryGet(world, out PositionComponent pos))
                {
                    entity.EntityAdd<TransformComponent>(world).Transform = go.transform;
                    go.transform.position = pos.Value;
                }
                
                
                if (entity.EntityTryGet(world, out Rotation2DComponent rot))
                {
                    
                    var angle = rot.AngleRadians * -Mathf.Rad2Deg;
                    go.transform.eulerAngles = go.transform.eulerAngles.WithY(angle);
                }

                if (entity.EntityHas<CollectableComponent>(world))
                {
                    if (entity.EntityHas<BushComponent>(world))
                    {
                        var view = go.GetComponent<BushView>();
                        ref var collectableTargetComponent =
                            ref entity.EntityAdd<CollectableTargetComponent>(world);
                        collectableTargetComponent.GameObject = view.Berries.gameObject;
                    }
                    else
                    {
                        ref var collectableTargetComponent =
                            ref entity.EntityAdd<CollectableTargetComponent>(world);
                        collectableTargetComponent.GameObject = go;
                    }
                }
            }

            var filterUnits = world.Filter<UnitComponent>()
                .Exc<TransformComponent>().End();

            foreach (var entity in filterUnits)
            {
                var view = Object.Instantiate(_viewPrefab);
                view.transform.position = entity.EntityGet<PositionComponent>(world).Value;
                view.Gun.gameObject.SetActive(false);

                ref var component = ref entity.EntityAdd<TransformComponent>(world);
                component.Transform = view.transform;

                ref var animatorComponent = ref entity.EntityAdd<AnimatorComponent>(world);
                animatorComponent.Animator = view.Animator;

                entity.EntityGetOrCreateRef<LerpComponent>(world).Value = 0.5f;
            }
            
            var filterBullets = world.Filter<BulletDamageComponent>()
                .Exc<TransformComponent>().End();

            foreach (var entity in filterBullets)
            {
                var viewGo = _container.InstantiatePrefab(_bulletPrefab);
                var view = viewGo.GetComponent<BulletView>();
                var pos = entity.EntityGet<PositionComponent>(world).Value;
                view.transform.position = pos;
                view.name = $"Bullet{entity.e2name(world)}-{N}";
                //view.PackedEntity = world.PackEntity(entity);

                ref var component = ref entity.EntityAdd<TransformComponent>(world);
                component.Transform = view.transform;

                entity.EntityGetOrCreateRef<LerpComponent>(world).Value = 0.5f;
                
                world.Log($"create view '{view.name}'  {entity.e2name(world)} at {pos}");

                ++N;
            }
        }
    }
}