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
        public double force;
        public double friction;
        public double speed;

    }
    public class Player2 : PhyObject
    {
        XYMessage moveMessage;

        private XYMessage rotateMessage;

        public float rotationController = 0;
        private float acceleration = 0;


        public PlayerStats playerStats = new PlayerStats { force = 30, friction = .99,speed=25 };
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

        public Player2()
        {
            this.updateRotation = false;
            Agent = new Agent(this);

        }
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid);
            PhyInterval worker = new PhyInterval(1, simulator);
            worker.Completed += Tick;


            simulator.collidableMaterials[bodyHandle.bodyHandle].collidable = true;
            simulator.collidableMaterials[bodyHandle.bodyHandle].SpringSettings = new SpringSettings(1f, .5f);

            reference = simulator.Simulation.Bodies.GetBodyReference(bodyHandle.bodyHandle);
            simulator.OnContactListeners.Add(this.guid, this);


            SetPositionToStartPoint();
            CreateBall();
            CreateGauntlet();


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

            golfball = (GolfBall2)simulator.Create(ball);
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
            }

        }
        public void TickMove()
        {
            if (moveMessage.clientId != null /*&& !IsSnapped()*/)
            {
                // Console.WriteLine(bb.Velocity.Linear.Y);
                if ((moveMessage.x != 0 || moveMessage.y != 0) && reference.Velocity.Linear.Y < 4)
                {
                    var radPad = Math.Atan2(this.moveMessage.x, -this.moveMessage.y);
                    var radian = (this.rotationController);
                    var x = (float)Math.Cos(radian + radPad);
                    var y = (float)Math.Sin(radian + radPad);
                    Vector3 vel = new Vector3(x, 0, y);

                    vel.X *= (float)playerStats.speed/100;
                    vel.Z *= (float)playerStats.speed/100;
                    reference.Velocity.Linear.X += vel.X;
                    reference.Velocity.Linear.Z += vel.Z;
                    reference.Awake = true;
                }
                // Console.WriteLine(" / " +reference.Velocity.Linear.Y);
                if (moveMessage.x == 0 && moveMessage.y == 0 && reference.Velocity.Linear.Y > -0.06)
                {
                    // Console.WriteLine(moveMessage.x+" / " +moveMessage.y);
                    reference.Velocity.Linear.X *= (float)playerStats.friction;
                    reference.Velocity.Linear.Z *= (float)playerStats.friction;
                    acceleration = 0;
                }




            }


        }

        private void TickRotation()
        {
            if (!cameraLocked)
            {
                if (rotateMessage.clientId != null)
                {
                    rotationController += rotateMessage.x / 80;
                    //simulator.Simulation.Awakener.AwakenBody(bodyHandle);
                    if (Math.Abs(rotateMessage.x) > 0)
                    {
                        reference.Awake = true;
                    }

                    state.quaternion = QuaternionEx.CreateFromYawPitchRoll(-rotationController, 0, 0);
                }
            }
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
            float distance = maxDistanceWithBall - 1;
            var newPos = reference.Pose.Position;
            var x = -(float)Math.Cos(rotationController);
            var y = -(float)Math.Sin(rotationController);

            newPos.X += x * distance;
            newPos.Z += y * distance;

            golfball.reference.Pose.Position = newPos;
        }
        public void SetPositionToStartPoint()
        {
            reference.Pose.Position.X = (float)simulator.map.startPositions[0].AsBsonDocument["x"].AsDouble;
            reference.Pose.Position.Y = (float)simulator.map.startPositions[0].AsBsonDocument["y"].AsDouble;
            reference.Pose.Position.Z = (float)simulator.map.startPositions[0].AsBsonDocument["z"].AsDouble;
        }

        public Vector2 GetXYRotation()
        {
            var x = (float)Math.Cos(this.rotationController);
            var y = (float)Math.Sin(this.rotationController);
            return new Vector2(x, y);
        }

        public void OnFall()
        {
            this.user.gems.Update(((int)this.user.gems.value) / 2);
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

            moveMessage = message;

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