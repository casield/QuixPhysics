using System;
using BepuPhysics;

namespace QuixPhysics{
    public class Hole :PhyObject{

        public Hole(){
            
        }
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid,Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid,room);
             simulator.OnContactListeners.Add(guid,this);
             
           
        }

        public override void OnContact(PhyObject obj){
            if(obj is GolfBall2){
                QuixConsole.Log("Oh si la pelota entro!");
            }

        }

        
        
        
    }
    
}