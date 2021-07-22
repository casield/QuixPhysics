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
        private float friction = .998f;
        private MoveMessage rotateMessage;

        public float rotationController = 0;
        private float acceleration = 0;
        public float shootForce = 30;
        public Agent Agent;

        private JumpState jumpState;
        private SnappedState snappedState;
        private Not_SnappedState notSnappedState;
        private ShootState shotState;

        internal delegate void OnContactAction(PhyObject obj);
        internal event OnContactAction ContactListeners;

        public GolfBall2 golfball;
        private float maxDistanceWithBall = 20;
        private bool canShoot;

        private IGauntlet gauntlet;

        public Player2()
        {
            this.updateRotation = false;
            Agent = new Agent(this);

        }
        public override void Load(BodyHandle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state)
        {
            base.Load(bodyHandle, connectionState, simulator, state);
            PhyInterval worker = new PhyInterval(1, simulator);
            worker.Completed += Tick;


            material = new SimpleMaterial
            {
                FrictionCoefficient = .97f,
                MaximumRecoveryVelocity = float.MaxValue,
                SpringSettings = new SpringSettings(1f, 1.5f)
            };
            simulator.collidableMaterials.Allocate(bodyHandle) = material;

            reference = simulator.Simulation.Bodies.GetBodyReference(bodyHandle);
            simulator.OnContactListeners.Add(this.bodyHandle, this);


            SetPositionToStartPoint();
            CreateBall();
            CreateUser();
            CreateGauntlet();
            

            jumpState = new JumpState(this);
            snappedState = new SnappedState(this);
            notSnappedState = new Not_SnappedState(this);
            shotState = new ShootState(this);

        }

        public void SetPositionToStartPoint(){
           QuixConsole.Log("Start position",(simulator.map.startPositions[0].AsBsonDocument["x"])) ;
           reference.Pose.Position.X =(float) simulator.map.startPositions[0].AsBsonDocument["x"].AsDouble;
           reference.Pose.Position.Y =(float) simulator.map.startPositions[0].AsBsonDocument["y"].AsDouble;
           reference.Pose.Position.Z =(float) simulator.map.startPositions[0].AsBsonDocument["z"].AsDouble;
        }

        private void CreateGauntlet()
        {
            this.gauntlet = new AtractGauntlet();
            this.gauntlet.AddPlayer(this);
        }


        private void CreateUser()
        {
            User s = new User(state.owner, this);

            if (!simulator.users.ContainsKey(state.owner))
            {
                simulator.users.Add(state.owner, s);
            }
            else
            {
                QuixConsole.Log("User has been already added");
            }


        }



        private void CreateBall()
        {
            SphereState ball = new SphereState();
            ball.radius = 3;
            ball.position = state.position + new Vector3(0, 0, 100);
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
            if (moveMessage != null /*&& !IsSnapped()*/)
            {
                // Console.WriteLine(bb.Velocity.Linear.Y);
                if ((moveMessage.x != 0 || moveMessage.y != 0) && reference.Velocity.Linear.Y < 4)
                {
                    var radPad = Math.Atan2(this.moveMessage.x, -this.moveMessage.y);
                    var radian = (this.rotationController);
                    var x = (float)Math.Cos(radian + radPad);
                    var y = (float)Math.Sin(radian + radPad);
                    Vector3 vel = new Vector3(x, 0, y);

                    vel.X *= .3f;
                    vel.Z *= .3f;
                    reference.Velocity.Linear.X += vel.X;
                    reference.Velocity.Linear.Z += vel.Z;
                    reference.Awake = true;
                }
                // Console.WriteLine(" / " +reference.Velocity.Linear.Y);
                if (moveMessage.x == 0 && moveMessage.y == 0 && reference.Velocity.Linear.Y > -0.06)
                {
                    // Console.WriteLine(moveMessage.x+" / " +moveMessage.y);
                    reference.Velocity.Linear.X *= friction;
                    reference.Velocity.Linear.Z *= friction;
                    acceleration = 0;
                }




            }


        }

        private void TickRotation()
        {
            if (rotateMessage != null)
            {
                rotationController += rotateMessage.x / 50;
                //simulator.Simulation.Awakener.AwakenBody(bodyHandle);
                if (Math.Abs(rotateMessage.x) > 0)
                {
                    reference.Awake = true;
                }

                state.quaternion = QuaternionEx.CreateFromYawPitchRoll(-rotationController, 0, 0);
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
            float distance = 10;
            reference.Pose.Position = golfball.GetReference().Pose.Position;
            reference.Pose.Position.Y += distance;
            var x = (float)Math.Cos(rotationController);
            var y = (float)Math.Sin(rotationController);

            reference.Pose.Position.X +=x*distance;
            reference.Pose.Position.Z +=y*distance;
        }

        public Vector2 GetXYRotation()
        {
            var x = (float)Math.Cos(this.rotationController);
            var y = (float)Math.Sin(this.rotationController);
            return new Vector2(x, y);
        }

        private void CheckIfFall()
        {

            if (this.reference.Pose.Position.Y < -50)
            {
                this.SetPositionToBall();
            }

        }


        public override void Move(MoveMessage message)
        {
            base.Move(message);
            moveMessage = message;
            if (Agent.ActualState() != notSnappedState)
            {
                Agent.ChangeState(notSnappedState);
                Agent.Lock(20);
            }

        }
        public void Rotate(MoveMessage message)
        {
            rotateMessage = message;
        }
        public void Jump(MoveMessage message)
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
                QuixConsole.WriteLine("Shotting");
                shotState.message = message;
                Agent.ChangeState(shotState);
                Agent.Lock(130);

            }
            else
            {

            }

        }
        public void UseGauntlet(bool activate)
        {
            gauntlet.Activate(activate);
        }
        public void Swipe(double degree,Vector3 direction){
            gauntlet.Swipe(degree,direction);
        }
    }

}