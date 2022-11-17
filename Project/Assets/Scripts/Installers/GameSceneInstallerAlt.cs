using Game.Client;
using Game.Client.Services;
using Game.ClientServer;
using Game.ClientServer.Services;
using Game.Dev;
using Game.State;
using Game.UI;
using Game.UI.Mono;
using Game.UIView;
using Game.View;
using UnityEngine;
using UnityEngine.Rendering;
using XFlow.Modules.Box2D.ClientServer.Systems;
using XFlow.Modules.Inventory.Client.Demo.Interfaces;
using XFlow.Modules.Inventory.Client.Demo.Services;
using XFlow.Modules.Mech.ClientServer;
using XFlow.Modules.States;
using XFlow.Net.Client;
using XFlow.Net.Client.Services;
using XFlow.Net.ClientServer;

namespace Game.Installers
{
    public class GameSceneInstallerAlt : XFlowEscInstaller
    {
        protected override void DoInstallBindings(CommonEcsGameSettings gameSettings)
        {
            /*
             * URP? Android exception workaround  2021.3.10f
             * NullReferenceException: Object reference not set to an instance of an object.
             * at UnityEngine.Rendering.DebugManager.UpdateActions () [0x00000] in <
            */
            DebugManager.instance.enableRuntimeUI = false;
            
            
            Container.Bind<Camera>().FromComponentsOn(GameObject.Find("Main Camera")).AsSingle();
            
            var global = GameObject.Find("Global").GetComponent<Global>();
            Container.Bind<Global>().FromInstance(global).AsSingle();

            Container.Bind<CharacterView>().FromInstance(global.CharacterPrefab).AsSingle();
            Container.Bind<BulletView>().FromInstance(global.BulletPrefab).AsSingle();
            Container.Bind<ParticleSystem>().FromInstance(global.FireParticles).AsSingle();
            Container.Bind<IInventoryStaticData>().FromInstance(global.InventoryStaticData).AsSingle();
            
            Container.Bind<FootprintView>().WithId("left").FromInstance(global.LeftFootprintPrefab).AsCached();
            Container.Bind<FootprintView>().WithId("right").FromInstance(global.RightFootprintPrefab).AsCached();

            Container.Bind<Objectives>().AsSingle().NonLazy();

            var mainUI = FindObjectOfType<MainUI>();
            Container.Bind<MainUI>().FromInstance(mainUI).AsSingle();
            Container.Bind<Joystick>().FromInstance(mainUI.Joystick).AsSingle();
            Container.Bind<UI.UI>().AsSingle().NonLazy();
            Container.Bind<Canvas>().WithId("HpViewCanvas").FromInstance(global.HpViewCanvas).AsSingle();

            Container.Bind<NameView>().FromInstance(global.HpViewPrefab).AsSingle();
            Container.Bind<NameViewManager>().AsSingle().NonLazy();

            //register states and view
            Container.Bind<MechInfoView>().FromInstance(global.MechInfoView).AsSingle();
             
            Container.Bind<States>().AsSingle();
            Container.Bind<RootState>().AsSingle();
            Container.Bind<MechInfoState>().AsSingle();
            Container.Bind<InventoryOpenedState>().AsSingle();

            Container.BindInterfacesAndSelfTo<EcsSystemsFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<EcsViewSystemsFactory>().AsSingle();

            Container.Bind<DevPanelController>().FromComponentInNewPrefabResource("DEV/DevPanel").AsSingle();
            Container.Bind<DevPanel>().AsSingle().NonLazy();
            
            Container.Bind<Box2DUpdateSystem.Options>().FromInstance(new Box2DUpdateSystem.Options());
            Container.Bind<MechService>().AsSingle();
            
            
            Container.BindInterfacesAndSelfTo<InventoryInputService>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerControlService>().AsSingle();

            Container.Bind<AssetInstantiator>().AsSingle();
            Container.BindInterfacesAndSelfTo<MyInventoryService>().AsSingle();
            Container.Bind<InventoryFactory>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<EntityDestroyedListener>().AsSingle();

            
            Container.BindInterfacesAndSelfTo<ClientInputService>().AsSingle();

            var starter = FindObjectOfType<GameStarter>().gameObject;
            if (gameSettings.MultiPlayer)
            {
                Container.Bind<string>().WithId("tmpHashesPath").FromInstance(Config.TMP_HASHES_PATH).AsCached();
                Container.Bind<NetClient>().AsSingle();
                
                if (gameSettings.SelectedContainer != null)
                {
                    Container.Bind<IServerConnector>()
                        .FromInstance(new FacadeServerConnector(gameSettings.SelectedContainer.ContainerId))
                        .AsSingle();
                }
                else if (gameSettings.SelectedHost != null)
                {
                    Container.Bind<IServerConnector>()
                        .FromInstance(new LocalServerConnector(gameSettings.SelectedHost.Hostname, gameSettings.SelectedHost.TcpPort,
                            gameSettings.SelectedHost.UdpPort))
                        .AsSingle();
                }
                
                var comp = starter.AddComponent<UnityEcsClient>();
                Container.QueueForInject(comp);
            }
            else
            {
                Container.Bind<SingleGame>().AsSingle();

                var comp = starter.AddComponent<UnityEcsSinglePlayer>();
                Container.QueueForInject(comp);
            }
        }
    }
}