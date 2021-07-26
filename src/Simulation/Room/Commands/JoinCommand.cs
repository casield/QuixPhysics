using System.Numerics;
using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    struct JoinCommandParams
    {
        string roomId;
        string clientId;
    }
    internal class JoinCommand : Command
    {
        public RoomManager roomManager;
        public JoinCommand(Simulator _simulator) : base(_simulator)
        {
            roomManager = simulator.roomManager;
        }
        public override void OnRead(JObject message,Room room)
        { 
            var clientId = message["clientId"].ToString();
             CreatePlayer(clientId, room);
        }

        public void CreatePlayer(string clientId, Room room)
        {
            var box = new SphereState();
            box.radius = 10;
            box.instantiate = true;
            box.type = "Player2";
            box.mesh = "Players/Sol/sol_prefab";
            box.quaternion = Quaternion.Identity;
            box.mass = 30;
            box.position = new Vector3();
            box.owner = clientId;


            var player = (Player2)simulator.Create(box);

            User user = new User(clientId, player);
            room.AddUser(user);
            player.user = user;

        }
    }
}