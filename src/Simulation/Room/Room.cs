using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace QuixPhysics{

    public struct RoomInfo{
        public Vector3 position;
        public string roomId;
        public int maxPlayers;
    }
    public class Room{

        public RoomInfo info;
        private Simulator simulator;
        Gamemode gamemode;

        public Dictionary<string,User> users = new Dictionary<string, User>();
        
        public Room(Simulator simulator,RoomInfo info){
            this.info = info;
            this.simulator = simulator;
            
        }

        public void SetGameMode(Gamemode gamemode){
            this.gamemode = gamemode;
        }
        public void AddUser(User user){
            users.Add(user.sessionId,user);
        }

        public void Create(){

        }
    }
}