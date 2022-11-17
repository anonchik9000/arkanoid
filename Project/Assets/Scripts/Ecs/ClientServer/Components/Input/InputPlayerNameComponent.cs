using XFlow.Ecs.ClientServer.WorldDiff;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Components.Input
{
    public struct InputPlayerNameComponent : IComplexComponent<InputPlayerNameComponent>,IInputComponent
    {
        public string Name;

        public void CopyRefsTo(ref InputPlayerNameComponent to)
        {
            to.Name = Name;
        }

        public void ReadTo(HGlobalReader reader, ref InputPlayerNameComponent result)
        {
            result.Name = reader.ReadString();
        }

        public void Write(HGlobalWriter writer)
        {
            writer.WriteString(Name);
        }
    }
}
