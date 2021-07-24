using System.Numerics;
using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    public class MessageState
    {
        public string type;
        public object data;
    }

    public interface Message
    {
        public string roomId { get; set; }
        public string clientId { get; set; }
    }


    public struct MoveMessage : Message
    {
        public float x;
        public float y;

        public string roomId { get; set; }
        public string clientId { get; set; }
    }
    public class ShootMessage : Message
    {
        public string roomId { get; set; }
        public string clientId { get; set; }
        public float force;
    }

    public class SwipeMessage : Message
    {
        public string roomId { get; set; }
        public string clientId { get; set; }
        public float degree;
        public Vector3 direction;
    }

    public class GauntletMessage : Message
    {
        public string roomId { get; set; }
        public string clientId { get; set; }
        public bool active;
    }
}