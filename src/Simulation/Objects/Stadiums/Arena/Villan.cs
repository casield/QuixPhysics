using System;
using BepuPhysics;

namespace QuixPhysics{
    public class Villan:PhyObject {
        BodyReference reference;
        public Villan(){

        }
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            //state.mesh = "Board/Crocodile/Crocodile";
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            reference = GetReference();
    
            var interval = new PhyInterval(1,simulator);
            interval.Tick+=IntervalTick;
        }

        private void IntervalTick()
        {

        }
    }
}