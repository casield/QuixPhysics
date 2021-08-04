using System;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Constraints;
using BepuUtilities;
using Newtonsoft.Json;

namespace QuixPhysics
{
    public delegate void OnContact(BodyHandle contact);
    public struct PlayerStats
    {
        public float force;
        public float friction;
        public float speed;
        public float maxSpeed;
    }
    public struct OverBoardStats
    {
        public float acceleration;
    }
    public class Player2 : PhyObject
    {
        XYMessage moveMessage;
        XYMessage forceMoveMessage;

        private XYMessage rotateMessage;

        public float rotationController = 0;


        public PlayerStats playerStats = new PlayerStats { force = 30, friction = .99f, speed = .15f, maxSpeed = 1f };
        public OverBoardStats overStats = new OverBoardStats { acceleration = .06f };

        private float moveAcceleration = 0;
        public Agent Agent;

        private JumpState jumpState;
        private SnappedState snappedState;
        private Not_SnappedState notSnappedState;
        private ShootState shotState;

        internal delegate void OnContactAction(PhyObject obj);
        internal event OnContactAction ContactListeners;

        internal delegate void OnShootAction(ShootMessage message);
        internal event OnShootAction ShootListeners;

        public GolfBall2 golfball;
        private float maxDistanceWithBall = 20;
        private bool canShoot;

        private IGauntlet gauntlet;
        internal User user;

        public bool cameraLocked = false;

        private float rotationAcceleration = 0f;
        private float maxAcc = .5f;

        private float rotationSpeed = 2.6f;
        public LookObject lookObject;

        public Player2()
        {
            this.updateRotation = false;
            Agent = new Agent(this);

        }
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            PhyInterval worker = new PhyInterval(1, simulator);
            worker.Completed += Tick;


            simulator.collidableMaterials[bodyHandle.bodyHandle].collidable = true;
            simulator.collidableMaterials[bodyHandle.bodyHandle].SpringSettings = new SpringSettings(1f, .5f);

            reference = simulator.Simulation.Bodies.GetBodyReference(bodyHandle.bodyHandle);
            simulator.OnContactListeners.Add(this.guid, this);


            SetPositionToStartPoint();
            CreateBall();
            CreateGauntlet();
            CreateLookObject();


            jumpState = new JumpState(this);
            snappedState = new SnappedState(this);
            notSnappedState = new Not_SnappedState(this);
            shotState = new ShootState(this);

        }


        private void CreateGauntlet()
        {
            this.gauntlet = new AtractGauntlet();
            this.gauntlet.AddPlayer(this);
            this.gauntlet.Init();
        }
        private void CreateLookObject()
        {
            lookObject = (LookObject)simulator.Create(new BoxState()
            {
                instantiate = true,
                mass = 0,

                type = "LookObject",
                owner = state.uID
            }, room);
            lookObject.SetPlayer(this);
        }
        private void CreateBall()
        {
            SphereState ball = new SphereState();
            ball.radius = 3;
            ball.position = state.position;
            ball.quaternion = Quaternion.Identity;
            ball.type = "GolfBall2";
            ball.mass = 1;
            ball.instantiate = true;
            ball.owner = state.owner;
            ball.mesh = "Objects/Balls/Vanilla/Vanilla";

            golfball = (GolfBall2)simulator.Create(ball, room);
            golfball.SetPlayer(this);
        }
        public bool IsSnapped()
        {
            return Agent.ActualState() == snappedState;
        }
        public void SetNotSnapped()
        {
            this.Agent.ChangeState(notSnappedState);
        }

        public override void OnContact(PhyObject obj)
        {
            base.OnContact(obj);
            ContactListeners?.Invoke(obj);
        }
        public void Tick()
        {
            if (reference.Exists)
            {
                CheckIfFall();
                TickSnapped();
                TickMove();
                TickRotation();

                Agent.Tick();
                lookObject.Update();
            }

        }
        public void TickMove()
        {
            if (moveMessage.clientId != null /*&& !IsSnapped()*/)
            {
                // Console.WriteLine(bb.Velocity.Linear.Y);
                if ((moveMessage.x != 0 || moveMessage.y != 0))
                {
                    var radPad = Math.Atan2(this.moveMessage.x, -(this.moveMessage.y));
                    var radian = (this.rotationController);
                    var x = (float)Math.Cos(radian + radPad);
                    var y = (float)Math.Sin(radian + radPad);
                    Vector3 vel = new Vector3(x, 0, y);

                    float force = Vector2.Distance(new Vector2(), new Vector2(forceMoveMessage.x, forceMoveMessage.y)) / 100;

                    vel.X *= (float)playerStats.speed * force;
                    vel.Z *= (float)playerStats.speed * force;


                    moveAcceleration += overStats.acceleration;

                    moveAcceleration = (float)Math.Clamp(moveAcceleration, 0, playerStats.maxSpeed);



                    reference.Velocity.Linear.X += ((vel.X) * moveAcceleration);
                    reference.Velocity.Linear.Z += ((vel.Z) * moveAcceleration);

                    reference.Awake = true;

                }
                else
                {
                    moveAcceleration = 0;
                    reference.Velocity.Linear.X *= playerStats.friction;
                    reference.Velocity.Linear.Z *= playerStats.friction;
                }



            }


        }


        private void TickRotation()
        {
            if (!cameraLocked)
            {
                if (rotateMessage.clientId != null)
                {
                    reference.Awake = true;

                    if (Math.Abs(rotateMessage.x) > 0)
                    {
                        reference.Awake = true;
                        rotationAcceleration += rotationSpeed * rotateMessage.x;
                        rotationAcceleration = Math.Clamp(rotationAcceleration, -maxAcc, maxAcc);
                        rotationAcceleration /= 100;


                    }
                    if (rotateMessage.y > .05)
                    {
                        lookObject.AddY(rotateMessage.y);
                        lookObject.Lock();
                    }

                    if (rotateMessage.y == 0)
                    {
                        lookObject.Release();
                    }

                    if (rotateMessage.x == 0)
                    {
                        rotationAcceleration *= .9f;
                    }
                    if (lookObject.watching is Player2)
                    {
                        rotationController += rotationAcceleration;
                    }
                    else
                    {


                        var prod = AngleBetweenVector2(lookObject.GetStaticReference().Pose.Position, reference.Pose.Position);
                        rotationController = prod;
                    }



                    state.quaternion = QuaternionEx.CreateFromYawPitchRoll(-(rotationController), 0, 0);


                }
            }
        }

        float AngleBetweenVector2(Vector3 vec1, Vector3 vec2)
        {
            var deltx = vec1.X - vec2.X;
            var delty = vec1.Z - vec2.Z;


            var rot = MathF.Atan2(-delty, -deltx);
            return rot;
        }
        private void TickSnapped()
        {
            if (this.Agent.ActualState() != snappedState)
            {
                var distance = Vector3.Distance(reference.Pose.Position, golfball.GetReference().Pose.Position);
                //Console.WriteLine(distance);
                if (distance < this.maxDistanceWithBall)
                {
                    this.canShoot = true;
                    this.Agent.ChangeState(snappedState);
                }
                else
                {
                    this.canShoot = false;
                }
            }

        }
        public void SetPositionToBall()
        {
            simulator.Simulation.Awakener.AwakenBody(golfball.reference.Handle);
            float distance = maxDistanceWithBall - .5f;
            var newPos = reference.Pose.Position;
            var x = -(float)Math.Cos(rotationController);
            var y = -(float)Math.Sin(rotationController);

            newPos.X += x * distance;
            newPos.Z += y * distance;

            golfball.reference.Pose.Position = newPos;
        }
        public void SetPositionToStartPoint()
        {
            if (room != null && room.gamemode != null)
            {
                reference.Pose.Position = room.gamemode.GetStartPoint(this.user);
            }

        }

        public Vector2 GetXYRotation()
        {
            var x = (float)Math.Cos(this.rotationController);
            var y = (float)Math.Sin(this.rotationController);
            return new Vector2(x, y);
        }

        public void OnFall()
        {
            if (this.user != null)
            {
                this.user.gems.Update(((int)this.user.gems.value) / 2);
            }

        }

        private void CheckIfFall()
        {

            if (this.reference.Pose.Position.Y < -50)
            {
                this.SetPositionToStartPoint();
                OnFall();
            }

            if (this.golfball.reference.Pose.Position.Y < -50)
            {
                this.golfball.reference.Pose.Position = reference.Pose.Position;
                OnFall();
            }

        }


        public void Move(XYMessage message)
        {

            //QuixConsole.Log("fake", fx, fy);
            moveMessage = message;
            forceMoveMessage.x = MathF.Abs(message.x);
            forceMoveMessage.y = MathF.Abs(message.y);


            if (message.x != 0 && message.y != 0)
            {
                var number_of_chunks = 16;
                var size_of_chunk = (360 / number_of_chunks);

                var angle = (float)Math.Atan2(moveMessage.x, moveMessage.y);
                var fx = (float)MathF.Cos(angle);
                var fy = (float)MathF.Sin(angle);
                message.x = fx * moveMessage.x;
                message.y = fy * moveMessage.y;
            }


            // moveMessage.y = moveMessage.y >

            if (Agent.ActualState() != notSnappedState)
            {
                Agent.ChangeState(notSnappedState);
                Agent.Lock(20);
            }

        }
        public void Rotate(XYMessage message)
        {
            rotateMessage = message;
        }
        public void Jump(XYMessage message)
        {

            if (!Agent.IsLocked())
            {

                Agent.ChangeState(jumpState);
                Agent.Lock(130);
            }

        }
        public void Shoot(ShootMessage message)
        {
            if (Agent.ActualState() == snappedState)
            {
                shotState.message = message;
                Agent.ChangeState(shotState);
                Agent.Lock(130);
                ShootListeners?.Invoke(message);

            }
            else
            {

            }


        }
        public void UseGauntlet(bool activate)
        {
            gauntlet.Activate(activate);
        }
        public void Swipe(double degree, Vector3 direction)
        {
            gauntlet.Swipe(degree, direction);
        }
    }

}