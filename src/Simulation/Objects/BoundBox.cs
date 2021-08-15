using System.Numerics;

namespace QuixPhysics
{
    public class BoundBox : PhyObject
    {

        //NOTE> IF OBJECT IS DYNAMC CENTER SHOULD ALWAYS BE 0,0,0;
        private Vector3 oldHalfSize;
        private void SetBoundBox(ObjectState state)
        {
            if (state.boundBox != null)
            {
                oldHalfSize = ((BoxState)state).halfSize;
                //state.mesh = null;
                var halfSize = ((BoxState)state).halfSize;
                halfSize.X = state.boundBox.extents.X * 2;
                halfSize.Y = state.boundBox.extents.Z * 2;
                halfSize.Z = state.boundBox.extents.Y * 2;
                ((BoxState)state).halfSize = halfSize;

                state.position += state.boundBox.center;
                QuixConsole.Log("BoundingBox", state.boundBox.center);
            }
        }
        public override void BeforeLoad(ObjectState state)
        {
            SetBoundBox(state);
        }
        public override void ChangeStateBeforeSend()
        {
            if (oldHalfSize != null && state!=null && state.boundBox != null)
            {
                state.position = GetPosition()-(new Vector3(state.boundBox.center.X/2,state.boundBox.center.Y,state.boundBox.center.Z/2));
               // state.quaternion = new Quaternion(new Vector3(0,1,0),1.57f);
                //

                ((BoxState)state).halfSize = oldHalfSize;
            }

        }
    }
}