using System;
using System.Numerics;
using BepuPhysics;
using BepuUtilities;

namespace QuixPhysics{
    public class Vehicle{
        public PhyObject obj;
        public Vector3 velocity = new Vector3();
        public Vector3 acceleration = new Vector3();
        private BodyReference reference;

        public float maxSpeed = 10;
        public float maxForce = 0.1f;
        public Vehicle(PhyObject obj){
            this.obj = obj;
            reference = obj.GetReference();

            Seek(new Vector3(100,0,100));
        }

        public void Seek(Vector3 target){
            Vector3 desired = Vector3.Subtract(target,reference.Pose.Position);
            NormalizeVector(desired);

            Console.Log("Desired",desired);
            
        }

        public void NormalizeVector(Vector3 vector3){
            float m =(float) System.Math.Sqrt(vector3.X*vector3.X + vector3.Y*vector3.Y);

            vector3.X/=m;
            vector3.Y/=m;
            vector3.Z/=m;
            Console.Log("Normilize",vector3);

        }
    }
}