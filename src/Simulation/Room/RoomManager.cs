using System.Collections.Generic;

namespace QuixPhysics{
    public class RoomManager {
        private Simulator simulator;
       public Dictionary<string,Room> rooms = new Dictionary<string, Room>();

        public RoomManager(Simulator simulator){
            this.simulator = simulator;
        }

        public void AddRoom(Room room){
            rooms.Add(room.props.roomId,room);
        }

        public void Update(){

        }
    }
}