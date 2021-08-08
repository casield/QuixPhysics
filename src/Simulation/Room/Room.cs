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

        public RoomInfo props;
        internal Simulator simulator;
        public Gamemode gamemode;

        public Dictionary<string,User> users = new Dictionary<string, User>();
        internal MapMongo map;

        public Room(Simulator simulator,RoomInfo info){
            this.props = info;
            this.simulator = simulator;
            //simulator.createObjects(this);
            SetGameMode(new Arena(simulator,this));
            
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