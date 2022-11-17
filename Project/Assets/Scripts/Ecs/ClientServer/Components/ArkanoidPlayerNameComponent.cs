using XFlow.Ecs.ClientServer.WorldDiff;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Components
{
    public struct ArkanoidPlayerNameComponent:IComplexComponent<ArkanoidPlayerNameComponent>
    {
        public string Name;

        public void CopyRefsTo(ref ArkanoidPlayerNameComponent to)
        {
            to.Name = Name;
        }

        public void ReadTo(HGlobalReader reader, ref ArkanoidPlayerNameComponent result)
        {
            result.Name = reader.ReadString();
        }

        public void Write(HGlobalWriter writer)
        {
            writer.WriteString(Name);
        }
    }

}
