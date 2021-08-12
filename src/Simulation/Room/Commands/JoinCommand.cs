using System.Numerics;
using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    internal class JoinCommand : Command
    {
        public RoomManager roomManager;
        public JoinCommand(Simulator _simulator) : base(_simulator)
        {
            roomManager = simulator.roomManager;
        }
        public override void OnRead(JObject message, Room room)
        {
            var clientId = message["clientId"].ToString();
            
            room.gamemode.OnJoin(CreateUser(clientId, room));

        }

        public User CreateUser(string clientId, Room room)
        {
            if (!room.users.ContainsKey(clientId))
            {

                User user = new User(clientId, room);
                room.AddUser(user);
    
                 return user;
            }else{
                return room.users[clientId];
            }


           

        }
    }
}