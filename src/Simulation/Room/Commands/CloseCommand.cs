using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    internal class CloseCommand : Command
    {
        public CloseCommand(Simulator _simulator) : base(_simulator)
        {
        }
        public override void OnRead(JObject message, Room room)
        {
          
          room.Dispose();
          //simulator.Close();

        }
    }
}