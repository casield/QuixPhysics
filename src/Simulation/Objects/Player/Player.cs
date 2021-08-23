using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
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


        public PlayerStats playerStats = new PlayerStats { force = 60, friction = .99f, speed = .15f, maxSpeed = 1f };
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

        private IGauntlet activeGauntlet;
        private Dictionary<string, IGauntlet> gauntlets = new Dictionary<string, IGauntlet>();
        internal User user;

        public bool cameraLocked = false;

        private float rotationAcceleration = 0f;
        private float maxAcc = .5f;

        private float rotationSpeed = 2.6f;
        public LookObject lookObject;
        private PhyInterval worker;

        public Player2()
        {
            this.updateRotation = false;
            Agent = new Agent(this);

        }
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            worker = new PhyInterval(1, simulator);
            worker.Completed += Tick;


            simulator.collidableMaterials[bodyHandle.bodyHandle].collidable = true;
            simulator.collidableMaterials[bodyHandle.bodyHandle].SpringSettings = new SpringSettings(1f, .5f);

            bodyReference = simulator.Simulation.Bodies.GetBodyReference(bodyHandle.bodyHandle);
            room.factory.OnContactListeners.Add(this.guid, this);

            CreateBall();
            CreateGauntlets();
            CreateLookObject();
            CreateTestObject();


            jumpState = new JumpState(this);
            snappedState = new SnappedState(this);
            notSnappedState = new Not_SnappedState(this);
            shotState = new ShootState(this);

            QuixConsole.Log("Loading Player");

        }

        private void CreateTestObject()
        {
           var t=new TestsObject();
           t.player = this;
          // t.Instantiate(room,GetPosition()+new Vector3(50,50,50));
        }

        private void CreateGauntlets()
        {
            AddGauntletToAvailable(new AtractGauntlet());
            AddGauntletToAvailable(new ItemGauntlet());

            ChangeGauntlet("item");
        }
        private void AddGauntletToAvailable(IGauntlet gauntlet)
        {

            gauntlet.AddPlayer(this);
            gauntlet.Init();
            gauntlets.Add(gauntlet.name, gauntlet);
        }
        public void ChangeGauntlet(string type)
        {
            if (activeGauntlet != null)
            {
                activeGauntlet.OnChange();
            }

            activeGauntlet = gauntlets[type];
            activeGauntlet.OnActivate();

        }
        private void CreateLookObject()
        {
            lookObject = (LookObject)room.Create(new BoxState()
            {
                instantiate = true,
                mass = 0,
                //halfSize=new Vector3(10,10,10),
                type = "LookObject",
                owner = state.owner
            });
            lookObject.SetPlayer(this);
        }
        private void CreateBall()
        {
            QuixConsole.Log("Create ball "+state.owner);
            SphereState ball = new SphereState();
            ball.radius = 3;
            ball.position = state.position;
            ball.quaternion = Quaternion.Identity;
            ball.type = "GolfBall2";
            ball.mass = 1;
            ball.instantiate = true;
            ball.owner = state.owner;
            ball.mesh = "Objects/Balls/Vanilla/Vanilla";

            golfball = (GolfBall2)room.Create(ball);
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
            //base.OnContact(obj);
            ContactListeners?.Invoke(obj);
        }
        public void Tick()
        {
            if (bodyReference.Exists)
            {
                CheckIfFall();
                TickSnapped();
                TickMove();
                TickRotation();

                Agent.Tick();
                if (lookObject != null)
                {
                    lookObject.Update();
                }

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



                    bodyReference.Velocity.Linear.X += ((vel.X) * moveAcceleration);
                    bodyReference.Velocity.Linear.Z += ((vel.Z) * moveAcceleration);

                    bodyReference.Awake = true;

                }
                else
                {
                    moveAcceleration = 0;
                    bodyReference.Velocity.Linear.X *= playerStats.friction;
                    bodyReference.Velocity.Linear.Z *= playerStats.friction;
                }



            }


        }


        private void TickRotation()
        {
            if (!cameraLocked)
            {
                if (rotateMessage.clientId != null)
                {
                    bodyReference.Awake = true;

                    if (Math.Abs(rotateMessage.x) > 0)
                    {
                        bodyReference.Awake = true;
                        rotationAcceleration += rotationSpeed * rotateMessage.x;
                        rotationAcceleration = Math.Clamp(rotationAcceleration, -maxAcc, maxAcc);
                        rotationAcceleration /= 100;


                    }

                    lookObject.AddY(rotateMessage.y);

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


                        var prod = AngleBetweenVector2(lookObject.GetStaticReference().Pose.Position, bodyReference.Pose.Position);
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
            if (golfball != null)
            {
                if (this.Agent.ActualState() != snappedState)
                {
                    var distance = Vector3.Distance(bodyReference.Pose.Position, golfball.GetBodyReference().Pose.Position);
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


        }
        public void SetPositionToBall()
        {
            simulator.Simulation.Awakener.AwakenBody(golfball.bodyReference.Handle);
            float distance = maxDistanceWithBall - .5f;
            var newPos = bodyReference.Pose.Position;
            var x = -(float)Math.Cos(rotationController);
            var y = -(float)Math.Sin(rotationController);

            newPos.X += x * distance;
            newPos.Z += y * distance;

            golfball.bodyReference.Pose.Position = newPos;
        }
        public void SetPositionToStartPoint()
        {
            bodyReference.Pose.Position = room.gamemode.GetStartPoint(this.user);
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

            if (this.bodyReference.Pose.Position.Y < -50)
            {
                this.SetPositionToStartPoint();
                OnFall();
            }

            if (golfball != null)
            {
                if (this.golfball.bodyReference.Pose.Position.Y < -50)
                {
                    this.golfball.bodyReference.Pose.Position = bodyReference.Pose.Position;
                    OnFall();
                }
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
            if (activeGauntlet != null)
            {
                activeGauntlet.Activate(activate);
            }
        }
        public void Swipe(double degree, Vector3 direction)
        {
            if (activeGauntlet != null)
            {
                activeGauntlet.Swipe(degree, direction);
            }

        }

        public override void Destroy()
        {
            base.Destroy();
            worker.Destroy();

        }
    }

}