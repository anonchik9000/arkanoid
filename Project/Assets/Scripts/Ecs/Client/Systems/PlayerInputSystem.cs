using System.Collections.Generic;
using Game.Client.Services;
using Game.Ecs.Client.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using XFlow.EcsLite;
using Zenject;

namespace Game.Client
{
    public class PlayerInputSystem : IEcsRunSystem, IEcsInitSystem
    {
        [Inject]private Camera camera;
        [Inject]private Joystick joystick;
        [Inject]private PlayerControlService controlService;

        private EcsWorld _world;
        
        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
        }
        
        void MoveDir(float hor, float ver)
        {
            var forward = -Vector3.Cross(Vector3.up,camera.transform.right) ;
            forward.y = 0;
            forward.Normalize();

            var right = camera.transform.right;
            right.y = 0;
            right.Normalize();

            var dir = forward * ver + right * hor;
                
            controlService.MoveToDirection(dir);
        }
        
        public void Run(EcsSystems systems)
        {
            if (!_world.HasUnique<MouseDownHandledComponent>() && Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
            {
                Debug.Log("MoveToPoint");
                var ray = camera.ScreenPointToRay(Input.mousePosition);
                var plane = new Plane(new Vector3(0, 1, 0), 0);
                plane.Raycast(ray, out var dist);

                var point = ray.GetPoint(dist);
                point.z = 0;
                controlService.MoveToPoint(point);
                return;
            }

            var hor = Input.GetAxis("Horizontal");
            //var ver = Input.GetAxis("Vertical");
            //Debug.Log(joystick.Direction);
            
            if (Mathf.Abs(hor) > 0.01f)// || Mathf.Abs(ver) > 0.01f)
            {
                MoveDir(hor, 0);
            }
            else if (joystick.Direction.magnitude > 0.01f)
            {
                MoveDir(joystick.Direction.x, 0);
            }else
            {
                controlService.StopMoveToDirection();
            }
        }

        
        
        public static bool IsPointerOverUIObject()
        {
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = Input.mousePosition;
 
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, raycastResults);
 
            if (raycastResults.Count > 0)
            {
                if (raycastResults[0].gameObject.GetComponentInParent<Canvas>() != null)
                    return true;

            }

            return false;
        }
    }
}