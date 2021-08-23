using System;
using System.Numerics;
using BepuPhysics;
using BepuUtilities;

namespace QuixPhysics
{

    public class VehicleProps
    {
        public Vector3 maxSpeed = new Vector3(0.2f, .5f, 0.2f);
        
        public Vector3 velocity = new Vector3();
        public Vector3 acceleration = new Vector3();
        public float slowingDistance = 100;
    }
    public class Vehicle
    {
        public PhyObject obj;

        public bool isActive = true;
        public VehicleProps props;
        public Vehicle(PhyObject phyObject, VehicleProps props){
            this.obj = phyObject;
       
            this.props = props;
        }

        public void SeekFlee(Vector3 target,Boolean seeking)
        {
            if (isActive)
            {
                Vector3 desired = Vector3.Subtract(target, obj.GetPosition());
                desired = Vector3.Normalize(desired);
                desired = Vector3.Multiply(desired, props.maxSpeed);

                Vector3 steer = Vector3.Subtract(desired, props.velocity);
                //steer = Vector3.Clamp(steer, -props.maxForce, props.maxForce);
                
                ApplyForce(seeking? steer:-steer);
            }
        }
        public void Arrive(Vector3 target)
        {

            if (isActive)
            {
                Vector3 target_offset = Vector3.Subtract(target, obj.GetPosition());
                float distance = target_offset.Length();
                var ramped_speed = Vector3.Multiply(props.maxSpeed, (distance /props.slowingDistance));
                var clipped_speed = Vector3.Min(ramped_speed,props.maxSpeed);
                var desired_velocity = Vector3.Multiply(Vector3.Divide(clipped_speed,distance),target_offset);
                var steer = Vector3.Subtract(desired_velocity,props.velocity);
                ApplyForce(steer);
            }
        }
        private void ApplyForce(Vector3 force)
        {
            props.acceleration = Vector3.Add(props.acceleration, force);
        }

        public void Update()
        {
            if (isActive&& obj.bodyReference.Exists)
            {
                obj.Awake();
                props.acceleration = Vector3.Clamp(props.acceleration, -props.maxSpeed, props.maxSpeed);// Limit(acceleration,maxSpeed);
                Vector3 velocity = Vector3.Add(obj.bodyReference.Velocity.Linear, props.acceleration);

                obj.bodyReference.Velocity.Linear = velocity;

                props.acceleration = Vector3.Multiply(props.acceleration, 0);
            }

        }
    }
}