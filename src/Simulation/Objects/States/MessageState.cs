using Newtonsoft.Json.Linq;

namespace QuixPhysics{
    public class MessageState{
        public string type;
        public object data;
    }


    public class MoveMessage{
        public string uID;
        public float x;
        public float y;
    }
}