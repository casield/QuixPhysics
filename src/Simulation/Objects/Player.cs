using System;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Constraints;
using BepuUtilities;
using Newtonsoft.Json;

namespace QuixPhysics
{
    public delegate void OnContact(BodyHandle contact);
    public struct PlayerStruct
    {
        public BodyHandle handle;
        //public char[] uID;
        // public string uID;
        //public event OnContact onContact;

    }
    public class Player2 : PhyObject
    {
        MoveMessage moveMessage;
        private float friction = 0.90f;
        private float maxVelocity = 300;

        private float moveMultiplier = .05f;
        private MoveMessage rotateMessage;

        private float rotationController = 0;
        private float acceleration = 0;
        private float maxAcceleration = 15;
        private float accelerationPower = .8f;
        private BodyReference reference;
        private Waiter jumpWaiter;
        private bool canJump = true;

        public Player2()
        {
            this.updateRotation = false;

        }
        public override void Load(BodyHandle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state)
        {
            base.Load(bodyHandle, connectionState, simulator, state);
            PhyInterval worker = new PhyInterval(1, simulator);
            worker.Completed += Tick;

            jumpWaiter = new Waiter(300);

            material = new SimpleMaterial
            {
                FrictionCoefficient = .97f,
                MaximumRecoveryVelocity = float.MaxValue,
                SpringSettings = new SpringSettings(1f, 1.5f)
            };
            simulator.collidableMaterials.Allocate(bodyHandle) = material;

            reference = simulator.Simulation.Bodies.GetBodyReference(bodyHandle);
            //simulator.Simulation.Bodies.
            simulator.OnContactListeners.Add(this.bodyHandle, this);
            // simulator.narrowPhaseCallbacks.AddListener(bodyHandle);
            // simulator.narrowPhaseCallbacks.onContactListeners.Allocate(handle,simulation.BufferPool);
            //simulator.narrowPhaseCallbacks.onContactListeners.Add(bodyHandle, simulator.Simulation.BufferPool);

            // simulator.Simulation.Timestepper.CollisionsDetected+=collitionDetected;
            // Console.WriteLine(simulator.narrowPhaseCallbacks.onContactListeners.Count);
            //  simulator.narrowPhaseCallbacks;
        }

        private void collitionDetected(float dt, IThreadDispatcher threadDispatcher)
        {
            throw new NotImplementedException();
        }

        public override void OnContact(PhyObject obj)
        {
            base.OnContact(obj);
            // Console.WriteLine("Contacted with "+obj.state.type + " - "+obj.state.uID);
        }
        public void Tick()
        {
            if (reference.Exists)
            {
                TickMove();
                TickRotation();
                TickJump();
            }

        }

        private void TickJump()
        {
            if (!canJump)
            {
                if (jumpWaiter.Wait())
                {
                    canJump = true;
                }
            }
        }

        public bool onMaxVelocity(BodyDescription bd)
        {
            return Math.Abs(bd.Velocity.Linear.X) >= maxVelocity || Math.Abs(bd.Velocity.Linear.Z) >= maxVelocity;
        }
        public void TickMove()
        {
            if (moveMessage != null)
            {
                // Console.WriteLine(bb.Velocity.Linear.Y);
                if ((moveMessage.x != 0 || moveMessage.y != 0))
                {
                    var radPad = Math.Atan2(this.moveMessage.x, -this.moveMessage.y);
                    var radian = (this.rotationController);
                    var x = (float)Math.Cos(radian + radPad);
                    var y = (float)Math.Sin(radian + radPad);
                    Vector3 vel = new Vector3(x, 0, y);
                    if (this.acceleration <= maxAcceleration)
                    {
                        this.acceleration += accelerationPower;
                    }
                    else
                    {
                        this.acceleration += 0.01f;
                    }

                    vel.X *= acceleration;
                    vel.Z *= acceleration;
                    reference.Velocity.Linear.X += vel.X;
                    reference.Velocity.Linear.Z += vel.Z;
                    reference.Awake = true;

                    //simulator.Simulation.Bodies.ApplyDescription(bodyHandle, in bb);
                }
                else
                {
                    //reference.Awake = false;
                }


                if ((moveMessage.x == 0 && moveMessage.y == 0))
                {
                    acceleration /= 1000;
                }
            }

            if (Math.Abs(reference.Velocity.Linear.Y) > 0.3)
            {
                reference.Velocity.Linear.X *= 0.99f;
                reference.Velocity.Linear.Z *= 0.99f;
            }
            reference.Velocity.Linear.X *= friction;
            reference.Velocity.Linear.Z *= friction;



        }

        private void TickRotation()
        {
            if (rotateMessage != null)
            {
                rotationController += rotateMessage.x / 70;
                //simulator.Simulation.Awakener.AwakenBody(bodyHandle);
                if (Math.Abs(rotateMessage.x) > 0)
                {
                    reference.Awake = true;
                }

                state.quaternion = QuaternionEx.CreateFromYawPitchRoll(-rotationController, 0, 0);
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
        public void Jump(MoveMessage message)
        {
            if (canJump)
            {
                reference.Awake = true;
                reference.Velocity.Linear.Y += 50;

                canJump = false;
            }

        }
    }
}