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
             room.gamemode.OnJoin(CreateUser(clientId, room));
            
        }

        public User CreateUser(string clientId, Room room)
        {
            var box = new SphereState();
            box.radius = 10;
            box.uID = PhyObject.createUID();
            box.instantiate = true;
            box.type = "Player2";
            box.mesh = "Players/Sol/sol_prefab";
            box.quaternion = Quaternion.Identity;
            box.mass = 30;
            box.owner = clientId;
            box.position = new Vector3(0,0,0);


            var player = (Player2)simulator.Create(box,room);


            User user = new User(clientId, player);
            room.AddUser(user);
            player.user = user;

            return user;

        }
    }
}