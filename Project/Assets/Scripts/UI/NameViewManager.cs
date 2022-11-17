using DG.Tweening;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.View;
using UnityEngine;
using XFlow.Ecs.Client.Components;
using XFlow.Ecs.ClientServer;
using XFlow.EcsLite;
using Zenject;

namespace Game.UI
{
    public class NameViewManager :
        EventsSystem<ArkanoidPlayerNameComponent>.IAnyComponentChangedListener,
        EventsSystem<ArkanoidPlayerNameComponent>.IAnyComponentRemovedListener,
        EventsSystem<DeletedEntityComponent>.IAnyComponentChangedListener,
        EventsSystem<DeletedEntityComponent>.IAnyComponentRemovedListener
    {
        private NameView _hpViewPrefab;
        private Canvas _canvas;
        private Camera _camera;

        private AnyListener _listener;

        private EcsPool<NameViewComponent> _poolView;

        private EcsPool<ArkanoidPlayerNameComponent> _poolName;
        private EcsPool<TransformComponent> _poolTransform;

        private EcsFilter _filter;

        public NameViewManager(EcsWorld world, [Inject(Id = EcsWorlds.Dead)] EcsWorld deadWorld, NameView hpViewPrefab,
            [Inject(Id = "HpViewCanvas")] Canvas canvas, Camera camera)
        {
            _hpViewPrefab = hpViewPrefab;
            _canvas = canvas;
            _camera = camera;

            _poolView = world.GetPool<NameViewComponent>();
            _poolName = world.GetPool<ArkanoidPlayerNameComponent>();
            _poolTransform = world.GetPool<TransformComponent>();
            _filter = world.Filter<NameViewComponent>().End();

            _listener = world.CreateAnyListener();
            _listener.SetAnyChangedListener<ArkanoidPlayerNameComponent>(this);
            _listener.SetAnyChangedListener<DeletedEntityComponent>(this);

        }


        public void OnAnyComponentChanged(EcsWorld world, int entity, ArkanoidPlayerNameComponent data, bool added)
        {
            UpdateHp(entity, data);
        }

        public void UpdateHp(int entity, in ArkanoidPlayerNameComponent data)
        {
            NameView view;

            if (_poolView.TryGet(entity, out NameViewComponent viewComponent))
            {
                view = viewComponent.View;
            }
            else
            {
                view = GameObject.Instantiate(_hpViewPrefab, _canvas.transform);
                _poolView.Add(entity).View = view;
            }


            view.SetValue(data.Name);
        }


        public void OnAnyComponentRemoved(EcsWorld world, int entity, AlwaysNull<DeletedEntityComponent> alwaysNull)
        {
            if (!_poolName.TryGet(entity, out ArkanoidPlayerNameComponent component))
                return;
            UpdateHp(entity, component);
        }
        

        public void OnAnyComponentChanged(EcsWorld world, int entity, DeletedEntityComponent data, bool added)
        {
            DestroyView(_poolView, entity);
        }

        private void DestroyView(EcsPool<NameViewComponent> pool, int entity)
        {
            if (!pool.TryGet(entity, out NameViewComponent viewComponent))
                return;
            var view = viewComponent.View;
            view.transform.DOScaleY(0, 0.3f).OnComplete(() => { GameObject.Destroy(view.gameObject); });
            pool.Del(entity);
        }

        public void OnAnyComponentRemoved(EcsWorld world, int entity, AlwaysNull<ArkanoidPlayerNameComponent> alwaysNull)
        {
            DestroyView(_poolView, entity);
        }

        public void LateUpdate()
        {
            foreach (var entity in _filter)
            {
                var view = _poolView.Get(entity).View;
                var pos = _poolTransform.Get(entity).Transform.position;

                var screenPoint = _camera.WorldToScreenPoint(pos);
                view.transform.position = screenPoint;
            }
        }
    }
}