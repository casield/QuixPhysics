using System.Numerics;
using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    public struct MessageState
    {
        public string type;
        public object data;
    }

    public interface Message
    {
        public string roomId { get; set; }
        public string clientId { get; set; }
    }


    public struct XYMessage : Message
    {
        public float x;
        public float y;

        public string roomId { get; set; }
        public string clientId { get; set; }
    }
    public struct ShootMessage : Message
    {
        public string roomId { get; set; }
        public string clientId { get; set; }
        public float force;
    }

    public struct SwipeMessage : Message
    {
        public string roomId { get; set; }
        public string clientId { get; set; }
        public float degree;
        public Vector3 direction;
    }

    public struct GauntletMessage : Message
    {
        public string roomId { get; set; }
        public string clientId { get; set; }
        public bool active;
    }
}