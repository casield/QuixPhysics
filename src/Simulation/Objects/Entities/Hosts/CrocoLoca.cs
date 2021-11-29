using System;
using System.Numerics;

namespace QuixPhysics
{
    public class CrocoLoca : Entity
    {
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);

            SendObjectMessage("text_Eat");
            CreateQuixtam();
        }

        public void CreateQuixtam(){
            
            Quixtam quixtam = (Quixtam)room.factory.Create(Quixtam.Build(state.position+new Vector3(150,-300,0)),room);
        }
        
        public override void ChangeStateBeforeSend()
        {
            base.ChangeStateBeforeSend();
            var velocityDirection = GetForward();
            var angle = (float)Math.Atan2(velocityDirection.Z, velocityDirection.X);
            state.quaternion = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), -angle); ;
        }
        public static SphereState Build(Vector3 position)
        {
            
            var state = new SphereState(){
                radius=5,
                position= position,
                type="CrocoLoca",
                instantiate=true,
                mass=1,
                mesh="Board/Monkey",
            };

            return state;

        }
        public override bool OnLastPolygon()
        {
            throw new System.NotImplementedException();
        }

        public override void OnTrailActive()
        {
            throw new System.NotImplementedException();
        }

        public override void OnTrailInactive()
        {
            throw new System.NotImplementedException();
        }

        internal override void OnRayCastHit(PhyObject obj, Vector3 normal)
        {
            throw new System.NotImplementedException();
        }
    }
}