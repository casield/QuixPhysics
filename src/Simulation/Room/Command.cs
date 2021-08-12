using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    public interface ICommand
    {
        public void OnRead(JObject message,Room room);
        public void Send(JObject message);
    }

    public abstract class Command : ICommand
    {
        internal Simulator simulator;
        public ConnectionState connectionState;

        public Command(Simulator _simulator)
        {
            simulator = _simulator;
        }
        public void SetConnectionState(ConnectionState state){
            this.connectionState = state;
        }
        public abstract void OnRead(JObject message,Room room);

        public virtual void Send(JObject message)
        {
            throw new System.NotImplementedException();
        }


    }

    
}