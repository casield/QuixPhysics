using System.Numerics;

namespace QuixPhysics
{
    public class ObjectAnimation
    {
        public PhyObject phyObject;

        public Vector3[] Animation { get; set; }
        public int animationPosition =0;

        public ObjectAnimation(PhyObject obj){
            phyObject = obj;
            
        }
    }
}