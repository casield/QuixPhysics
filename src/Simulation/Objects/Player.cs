using System;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Constraints;
using BepuUtilities;
using Newtonsoft.Json;

namespace QuixPhysics
{
    public class Player2 : PhyObject
    {
        MoveMessage moveMessage;
        private float friction = 0.95f;
        private float maxVelocity = 300;

        private float moveMultiplier = .05f;
        private MoveMessage rotateMessage;

        private float rotationController = 0;
        private float acceleration = 0;
        private float maxAcceleration = 2;
        private float accelerationPower = .05f;

        public Player2()
        {
            this.updateRotation = false;
            Console.WriteLine("Im inside!");

        }
        public override void Load(BodyHandle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state)
        {
            base.Load(bodyHandle, connectionState, simulator, state);
            PhyInterval worker = new PhyInterval(1, simulator);
            worker.Completed += Tick;

           material = new SimpleMaterial
            {
                FrictionCoefficient = 5,
                MaximumRecoveryVelocity = float.MaxValue,
                SpringSettings = new SpringSettings(1f, 1.5f)
            };
            simulator.collidableMaterials.Allocate(bodyHandle) = material;
        }
        public void Tick()
        {
            TickMove();
            TickRotation();
        }
        public bool onMaxVelocity(BodyDescription bd)
        {
            return Math.Abs(bd.Velocity.Linear.X) >= maxVelocity || Math.Abs(bd.Velocity.Linear.Z) >= maxVelocity;
        }
        public void TickMove()
        {
            if (moveMessage != null)
            {

                if (moveMessage.x != 0 || moveMessage.y != 0)
                {

                    BodyDescription bb;
                    simulator.Simulation.Bodies.GetDescription(bodyHandle, out bb);
                    //Console.WriteLine(JsonConvert.SerializeObject(moveMessage));


                    //Vector3 vel = new Vector3(-(moveMessage.y * moveMultiplier), 0, moveMessage.x * moveMultiplier);
                    var radPad = Math.Atan2(this.moveMessage.x, -this.moveMessage.y);
                    var radian = (this.rotationController);
                    var x = (float)Math.Cos(radian + radPad);
                    var y = (float)Math.Sin(radian + radPad);


                    Vector3 vel = new Vector3(x, 0, y);


                    /* if (onMaxVelocity(bb))
                     {
                         vel *= friction;
                     }*/
                    if (this.acceleration <= this.maxAcceleration)
                    {
                        this.acceleration += this.accelerationPower;
                    }
                    else
                    {
                        this.acceleration += this.accelerationPower / 1000;
                    }
                    vel.X *= acceleration;
                    vel.Z *= acceleration;
                    bb.Velocity.Linear.X += vel.X;
                    bb.Velocity.Linear.Z += vel.Z;


                    //  simulator.Simulation.Bodies.ApplyDescription(bodyHandle, in bb);
                    // Console.WriteLine("Move" + moveMessage.x);

                    simulator.Simulation.Bodies.ApplyDescription(bodyHandle, in bb);
                }


                if (moveMessage.x == 0 && moveMessage.y == 0)
                {
                    acceleration /= 1000;

                }


            }


        }

        private void TickRotation()
        {
            if (rotateMessage != null)
            {

                BodyDescription bb;
                simulator.Simulation.Bodies.GetDescription(bodyHandle, out bb);
                rotationController += rotateMessage.x / 70;
                simulator.Simulation.Awakener.AwakenBody(bodyHandle);
                state.quaternion = QuaternionEx.CreateFromYawPitchRoll(-rotationController, 0, 0);
                // simulator.Simulation.Bodies.ApplyDescription(bodyHandle, bb);


            }
        }


        public override void Move(MoveMessage message)
        {
            base.Move(message);
            moveMessage = message;
        }
        public void Rotate(MoveMessage message)
        {
            rotateMessage = message;
        }
    }
}