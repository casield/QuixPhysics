using System;
using BepuPhysics;

namespace QuixPhysics{
    public class CrocoLoco : PhyObject{

        Vehicle vehicle;
        public CrocoLoco(){
            Console.WriteLine("Crocoloco");
            
        }
        public override void Load(BodyHandle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state)
        {
            base.Load(bodyHandle, connectionState, simulator, state);
            vehicle = new Vehicle(this);
            
        }
    }
}