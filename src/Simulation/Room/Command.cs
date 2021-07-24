using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    public interface ICommand
    {
        public void OnRead(JObject message);
        public void Send(JObject message);
    }

    public class Command : ICommand
    {
        internal Simulator simulator;

        public Command(Simulator _simulator)
        {
            simulator = _simulator;
        }
        public virtual void OnRead(JObject message)
        {
            throw new System.NotImplementedException();
        }

        public virtual void Send(JObject message)
        {
            throw new System.NotImplementedException();
        }


    }

    
}