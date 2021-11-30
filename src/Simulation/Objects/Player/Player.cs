using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using BepuPhysics;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using Newtonsoft.Json;
using QuixPhysics.Player;

namespace QuixPhysics
{
    public delegate void OnContact(BodyHandle contact);
    /// <summary>
    /// Defines the stast of this Player
    /// </summary>
    public struct PlayerStats
    {
        public float force;
        public float friction;
        public float speed;
        public float maxSpeed;
        public float maxDistanceWithBall;
        public float height;
    }
    public struct OverBoardStats
    {
        public float acceleration;
    }

    public class Player2 : PhyObject
    {
        public float rotationController = 0;


        public PlayerStats playerStats = new PlayerStats
        {
            force = 60,
            friction = .99f,
            speed = .15f,
            maxSpeed = 1f,
            maxDistanceWithBall = 20,
            height = 30
        };
        public OverBoardStats overStats = new OverBoardStats { acceleration = .06f };
        public ActionsManager actionsManager;


        internal delegate void OnContactAction(PhyObject obj,Vector3 normal);
        internal event OnContactAction ContactListeners ;

        public GolfBall2 golfball;

        private IGauntlet activeGauntlet;
        private Dictionary<string, IGauntlet> gauntlets = new Dictionary<string, IGauntlet>();
        internal User user;

        public bool cameraLocked = false;

        public LookObject lookObject;

        public Player2()
        {
            this.updateRotation = false;

        }
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            AddWorker(new PhyInterval(1, simulator)).Completed += Tick;


            //simulator.collidableMaterials[bodyHandle.bodyHandle].collidable = true;
            //simulator.collidableMaterials[bodyHandle.bodyHandle].SpringSettings = new SpringSettings(10f, 1f);

            bodyReference = simulator.Simulation.Bodies.GetBodyReference(bodyHandle.bodyHandle);
            room.factory.OnContactListeners.Add(this.guid, this);

            StartActionsManager();

            CreateBall();
            CreateGauntlets();
            CreateLookObject();
            CreateDummy();

            QuixConsole.Log("Player loaded - ");

        }

        private void CreateDummy()
        {
           Dummy dummy = (Dummy)room.factory.Create(Dummy.Build(),room);

           dummy.AddToObject(this);
        }

        /// <summary>
        /// Create the actions manager class and initialize the actions that will run by default.
        /// </summary>
        private void StartActionsManager()
        {
            actionsManager = new ActionsManager(this);
            actionsManager.Fall();
            actionsManager.GrabBall();
            //actionsManager.RayCast();
        }

        private void CreateGauntlets()
        {
            AddGauntletToAvailable(new AtractGauntlet());
            AddGauntletToAvailable(new ItemGauntlet());

            ChangeGauntlet("atract");
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
            QuixConsole.Log("Create ball " + state.owner);
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


        public override void OnContact<TManifold>(PhyObject obj, TManifold manifold)
        {
            Vector3 normal = manifold.GetNormal(ref manifold,0);
            
            ContactListeners?.Invoke(obj,normal);
        }
        public void Tick()
        {
            if (bodyReference.Exists)
            {
                actionsManager.Update();

                if (lookObject != null)
                {
                    lookObject.Update();
                }
            }
        }
        public void SetPositionToStartPoint()
        {
            bodyReference.Pose.Position = room.gamemode.GetStartPoint(this.user);
        }

        public Vector2 GetXYRotation()
        {
            var x = (float)Math.Cos(this.rotationController);
            var y = (float)Math.Sin(this.rotationController);
            return -new Vector2(x, y);
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
    }

}