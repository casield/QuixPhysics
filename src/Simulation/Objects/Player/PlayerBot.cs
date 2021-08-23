using System;
using System.Numerics;

namespace QuixPhysics{
    public class PlayerBot:Player2 {
        
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            AddWorker(new PhyInterval(1,simulator)).Completed+=TickBot;
            
        }

        public void OnStart(){
            
            AddWorker(new PhyTimeOut(1000,simulator,true)).Completed+=()=>{
                lookObject.SetLookPosition(lookObject.GetPosition()+new Vector3(0,150,0));
                UseGauntlet(true);
            };
        }

        private void TickBot()
        {
            //QuixConsole.Log("Ticl bot");
        }
    }
}