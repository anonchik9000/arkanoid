using System.IO;
using Fabros.Ecs.ClientServer.WorldDiff;
using Flow.EcsLite;


namespace Game.Fabros.Net.ClientServer.Ecs.Systems
{
    public class CreateGameSystem : IEcsInitSystem
    {
        private readonly ComponentsCollection pool;

        public CreateGameSystem(ComponentsCollection pool)
        {
            this.pool = pool;
        }

        public void Init(EcsSystems systems)
        {
            var world = systems.GetWorld();

            //var worldJson = File.ReadAllText("world.ecs.json");
            //var dif = WorldDiff.FromJsonString(pool, worldJson);
            //dif.ApplyChanges(world);
        }
    }
}