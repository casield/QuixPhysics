using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    public class MessageState
    {
        public string type;
        public object data;
    }


    public class MoveMessage
    {
        public string client;
        public float x;
        public float y;
    }
    public class ShootMessage
    {
        public string client;
        public float force;
    }

    public class SwipeMessage
    {
        public string client;
        public float degree;
    }

    public class GauntletMessage
    {
        public string client;
        public bool active;
    }
}