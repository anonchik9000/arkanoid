﻿using System;
using Fabros.Ecs.Utils;
using Game.Ecs.Client.Components;
using Game.Ecs.Client.Systems;
using Game.Ecs.ClientServer.Components;
using Game.Fabros.EcsModules.Fire.Client.Components;
using Game.Fabros.Net.Client;
using Game.Fabros.Net.ClientServer;
using Game.Fabros.Net.ClientServer.Protocol;
using Game.UI;
using Game.Utils;
using Leopotam.EcsLite;
using UnityEngine;
using Zenject;

namespace Game.Client
{
    public class UnityEcsClient : MonoBehaviour, EventsSystem<FoodCollectedComponent>.IAnyComponentChangedListener
    {
        private NetClient client;

        [Inject] private Camera camera;
        [Inject] private Global global;
        [Inject] private PlayerInput.PlayerInput playerInput;
        [Inject] private MainUI ui;
        [Inject] private EcsWorld world;

        private EcsSystems viewSystems;

        private void Start()
        {
            client = new NetClient(world);
            
            viewSystems = new EcsSystems(world);
            viewSystems.Add(new SyncTransformSystem());
            viewSystems.Add(new RotateCharacterSystem());
            viewSystems.Add(new CameraFollowSystem());


            client.ConnectedAction = () =>
            {
                viewSystems.Init();
            };
            
            client.InitWorldAction = world =>
            {
                var viewComponent = new ClientViewComponent();
                viewComponent.Camera = Camera.main;
                viewComponent.MainUI = ui;
                viewComponent.Global = global;

                world.AddUnique<ClientViewComponent>() = viewComponent;
            };

            client.LinkUnitsAction = world => { ClientServices.LinkUnits(world); };

            client.DeleteEntitiesAction = (world, entities) =>
            {
                entities.ForEach(entity =>
                {
                    if (entity.EntityHasComponent<GameObjectComponent>(world))
                    {
                        var go = entity.EntityGetComponent<GameObjectComponent>(world).GameObject;
                        Destroy(go);
                    }

                    if (entity.EntityHasComponent<FireViewComponent>(world))
                    {
                        var go = entity.EntityGetComponent<FireViewComponent>(world).view.gameObject;
                        Destroy(go);
                    }
                });
            };

            client.Start();


            ui.InteractionButton.onClick.AddListener(() =>
            {
                var input = new UserInput
                {
                    hasInteraction = true,
                    action = new UserInput.Action()
                };

                client.AddUserInput(input);
            });

            ui.FoodText.text = "";

            int globalListenerEntity = 0;
            globalListenerEntity.AddAnyChangedListener<FoodCollectedComponent>(world, this);
        }

        private void Update()
        {
            if (!client.Connected)
                return;

            client.Update();
            
            var unitEntity = BaseServices.GetUnitEntityByPlayerId(world, client.GetPlayerID());
            CheckInput(world, 
                unitEntity, playerInput, camera,
                input => client.AddUserInput(input));
            
            viewSystems.Run();
        }

        private void OnDestroy()
        {
            client.OnDestroy();
        }

        private void OnGUI()
        {
            client.OnGUI();
        }


        public static void CheckInput(EcsWorld world, 
            int unitEntity, 
            PlayerInput.PlayerInput playerInput,
            Camera camera, Action<UserInput> addUserInput
            )
        {
            if (unitEntity == -1 )
                return;
            
            var forward = camera.transform.forward;
            forward.y = 0;
            forward.Normalize();

            var right = camera.transform.right;
            right.y = 0;
            right.Normalize();

            var moveDirection = playerInput.Movement;
            moveDirection = forward * moveDirection.z + right * moveDirection.x;


            
            if (playerInput.HasTouch)
            {
                var ray = camera.ScreenPointToRay(playerInput.TouchPosition);
                var plane = new Plane(new Vector3(0, 1, 0), 0);
                plane.Raycast(ray, out var dist);

                var point = ray.GetPoint(dist);

                var input = new UserInput
                {
                    hasMove = true,
                    move = new UserInput.Move {value = point, moveType = UserInput.MoveType.MoveToPoint}
                };

                addUserInput(input);
                return;
            }

            var lastDirection = unitEntity.EntityGetComponent<MoveDirectionComponent>(world).value;

            if (moveDirection != lastDirection)
            {
                if (unitEntity.EntityHas<TargetPositionComponent>(world))
                    if (moveDirection.magnitude < 0.001f)
                        return;

                var input = new UserInput
                {
                    hasMove = true,
                    move = new UserInput.Move {value = moveDirection, moveType = UserInput.MoveType.MoveToDirection}
                };

                addUserInput(input);
            }
        }

        public void OnAnyComponentChanged(EcsWorld world, int entity, FoodCollectedComponent data, bool added)
        {
            if (!world.HasUnique<ClientPlayerComponent>())
                return;
            
            var unitEntity = world.GetUnique<ClientPlayerComponent>().entity;
            if (unitEntity != entity)
                return;

            ui.FoodText.text = $"Food Collected {data.Value}";       
        }
    }
}