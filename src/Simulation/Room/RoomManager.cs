using System;
using System.Collections.Generic;
using System.Numerics;

namespace QuixPhysics{
    public class RoomManager {
        private Simulator simulator;
       public Dictionary<string,Room> rooms = new Dictionary<string, Room>();

        public RoomManager(Simulator simulator){
            this.simulator = simulator;
        }

        private void AddRoom(Room room){
            rooms.Add(room.props.roomId,room);
        }
        public Room NewRoom(ConnectionState state,string roomId){
            Room room = new Room(simulator,new RoomInfo(){maxPlayers=10,position=new Vector3(),roomId=roomId},state);
            
            AddRoom(room);

            return room;
        }

        public void Update(){

        }

        internal void RoomLeave(Room room)
        {
            rooms.Remove(room.props.roomId);
        }
    }
}