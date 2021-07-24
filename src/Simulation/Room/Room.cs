using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace QuixPhysics{

    public struct RoomInfo{
        public Vector3 position;
        public string roomId;
        public int maxPlayers;
        public int index;
    }
    public class Room{

        public RoomInfo info;
        private Simulator simulator;

        public List<User> users = new List<User>();
        
        public Room(Simulator simulator,RoomInfo info){
            this.info = info;
            this.simulator = simulator;
            
        }
        public void AddUser(User user){
            users.Add(user);
        }

        public void Create(){

        }
    }
}