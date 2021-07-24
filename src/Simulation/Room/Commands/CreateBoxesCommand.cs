using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    class CreateBoxesCommand : Command
    {
        public CreateBoxesCommand(Simulator _simulator) : base(_simulator)
        {
        }

        public override void OnRead(JObject message)
        {
           
            simulator.boxToCreate = 10;
            simulator.createObjects();
        }
    }
}