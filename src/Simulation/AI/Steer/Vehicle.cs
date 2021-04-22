using System;
using System.Numerics;
using BepuPhysics;
using BepuUtilities;

namespace QuixPhysics
{
    public class Vehicle
    {
        public PhyObject obj;
        public Vector3 velocity = new Vector3();
        public Vector3 acceleration = new Vector3();
        private BodyReference reference;

        public float maxSpeed = 2;
        public float maxForce = 100f;
        public bool isActive = true;
        public Vehicle(PhyObject obj)
        {
            this.obj = obj;
            reference = obj.GetReference();

        }

        public void Seek(Vector3 target)
        {
            if (isActive)
            {
                Vector3 desired = Vector3.Subtract(target, reference.Pose.Position);
                desired = Vector3.Normalize(desired);
                desired = Vector3.Multiply(desired, maxSpeed);

                Vector3 steer = Vector3.Subtract(desired, velocity);
                steer = Vehicle.Limit(steer, maxForce);
                Console.Log("Steer", steer);
                applyForce(steer);
            }


        }

        private static float LimitFloat(float f, float min,float max){
            if(f < min){
                return min;
            }
            if(f>max){
                return max;
            }
            return f;
        }

        private static Vector3 Limit(Vector3 limitVector, float limit)
        {
            limitVector.X = LimitFloat(limitVector.X,-limit,limit);
            limitVector.Y = LimitFloat(limitVector.Y,-limit,limit);
            limitVector.Z = LimitFloat(limitVector.Z,-limit,limit);
            return limitVector;
        }

        private void applyForce(Vector3 force)
        {
            acceleration = Vector3.Add(acceleration, force);
        }

        public void Update()
        {
            if (isActive)
            {
                acceleration = Limit(acceleration,maxSpeed);
                Vector3 velocity = Vector3.Add(reference.Velocity.Linear, acceleration);
                
                reference.Velocity.Linear = velocity;

                acceleration = Vector3.Multiply(acceleration, 0);
            }

        }
    }
}