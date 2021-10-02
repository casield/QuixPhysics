using System;
using System.Numerics;

namespace QuixPhysics
{
    public class MeshBox : PhyObject
    {

        private Quaternion oldQuat;
        private bool updatedHalfSize = false;


        public override void BeforeLoad(ObjectState state)
        {

             oldQuat = state.quaternion;
             state.quaternion =new Quaternion(-state.quaternion.X,state.quaternion.Z,-state.quaternion.Y,state.quaternion.W);
            state.quaternion *=new Quaternion(00.707f,0,0,-0.707f);

            ((BoxState)state).halfSize /= new Vector3(-2,2,2);;

        }
        public override void ChangeStateBeforeSend()
        {
            state.quaternion =GetQuaternion()*new Quaternion(00.707f,0,0,-0.707f);
            if(!updatedHalfSize){
               ((BoxState)state).halfSize *= new Vector3(-2,2,2); 
               updatedHalfSize = true;
            }
            

        }
    }
}