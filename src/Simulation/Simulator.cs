namespace QuixPhysics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Sockets;
    using System.Numerics;
    using System.Threading;
    using BepuPhysics;
    using BepuPhysics.Collidables;
    using BepuPhysics.Constraints;
    using BepuUtilities.Memory;
    using ContentBuilder;
    using ContentLoader;
    using MongoDB.Bson;
    using Newtonsoft.Json;
    using OVars;

    public class Simulator : IDisposable
    {
        internal BufferPool bufferPool;

        public SimpleThreadDispatcher ThreadDispatcher { get; }
        public Simulation Simulation { get; }
        public int TIME_TO_SEND_TICK = 50;

        public GameLoop gameLoop = null;
      //  public Dictionary<BodyHandle, PhyObject> objectsHandlers = new Dictionary<BodyHandle, PhyObject>();
      //  public Dictionary<StaticHandle, PhyObject> staticObjectsHandlers = new Dictionary<StaticHandle, PhyObject>();


       // public Dictionary<string, PhyObject> objects = new Dictionary<string, PhyObject>();
        public CollidableProperty<SimpleMaterial> collidableMaterials;
        public ConnectionState connectionState;
        internal Server server;
     //   internal int boxToCreate = 10;
    //    internal int timesPressedCreateBoxes = 0;
        internal List<PhyWorker> workers = new List<PhyWorker>();
        internal List<PhyWorker> workersToAdd = new List<PhyWorker>();
        public CommandReader commandReader;
        internal Thread thread;

        public QuixNarrowPhaseCallbacks narrowPhaseCallbacks;
      //  public Dictionary<Guid, PhyObject> allObjects = new Dictionary<Guid, PhyObject>();

       // public Dictionary<Guid, PhyObject> OnContactListeners = new Dictionary<Guid, PhyObject>();
    

        public bool Disposed = false;



        public OVarManager oVarManager;

        public RoomManager roomManager;
        private int updateTick;

        public Simulator(ConnectionState state, Server server)
        {

            server.ReloadMeshes();



            collidableMaterials = new CollidableProperty<SimpleMaterial>();

            this.connectionState = state;
            this.server = server;
            bufferPool = new BufferPool();
            var targetThreadCount = Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
            ThreadDispatcher = new SimpleThreadDispatcher(targetThreadCount);
            narrowPhaseCallbacks = new QuixNarrowPhaseCallbacks() { CollidableMaterials = collidableMaterials, simulator = this };
            new Tests(this);
            Simulation = Simulation.Create(bufferPool, narrowPhaseCallbacks, new QuixPoseIntegratorCallbacks(new Vector3(0, -5, 0)), new PositionFirstTimestepper());


            gameLoop = new GameLoop();
            gameLoop.Load(this);


            oVarManager = new OVarManager(this);
            roomManager = new RoomManager(this);


            thread = new Thread(new ThreadStart(gameLoop.Start));

            commandReader = new CommandReader(this);
            thread.Start();



        }

    
        internal void createObjects(Room room)
        {
           /* int width = 10;
            int max = 3000;
            var random = new Random();
            for (int a = 0; a < boxToCreate; a++)
            {
                var box = new SphereState();
                box.uID = PhyObject.createUID();
                box.uID += "" + a;
                box.mass = 10;
                box.type = "Bomb";
                // box.instantiate = false;

                int x = a % width;    // % is the "modulo operator", the remainder of i / width;
                int y = a / width;    // where "/" is an integer division
                //box.position = new Vector3(x * sizeObj, 1050 + (timesPressedCreateBoxes * sizeObj), y * sizeObj);
                box.position = new Vector3(random.Next(-max, max), 2500, random.Next(-max, max));
                box.radius = 10;
                box.mesh = "Board/Bomb";
                box.instantiate = true;
                box.quaternion = Quaternion.Identity;
               // var b = Create(box,room);
            }
            timesPressedCreateBoxes++;
            // Console.WriteLine("Statics size " + Simulation.Statics.Count);*/
        }
        private void handleWorkers()
        {
            foreach (var item in workersToAdd)
            {
                workers.Add(item);
            }
            workersToAdd.Clear();
            List<PhyWorker> toDelete = new List<PhyWorker>();
            foreach (var item in workers)
            {
                item.tick();
                if (item.ShouldDestroy())
                {
                    toDelete.Add(item);
                }
            }
            foreach (var item in toDelete)
            {
                workers.Remove(item);
            }
        }

        internal void Update()
        {
            //Console.WriteLine(gameTime.TotalSeconds);
            //commandReader.ReadCommand();
            if (Simulation != null && ThreadDispatcher != null)
            {
                handleWorkers();
                
                Simulation.Timestep(1 / 60f, ThreadDispatcher);
                
                // ArrayList bodies = new ArrayList();
                if(updateTick ==TIME_TO_SEND_TICK){
                    SendUpdate();
                    updateTick = 0;
                }else{
                    updateTick++;
                }
            }



        }
        public void SendUpdate(){
            foreach (var room in roomManager.rooms)
            {
                room.Value.factory.SendUpdate();
            }
           /*  var set = Simulation.Bodies.Sets[0];
                string[] bodies2 = new string[allObjects.Count];
                int bodiesAdded = 0;



                for (var bodyIndex = 0; bodyIndex < set.Count; ++bodyIndex)
                {
                    try
                    {

                        var handle = set.IndexToHandle[bodyIndex];
                        if (objectsHandlers[handle].state.instantiate)
                        {
                        
                            bodies2[bodyIndex] = objectsHandlers[handle].getJSON();
                            bodiesAdded += 1;
                        }


                    }
                    catch (KeyNotFoundException e)
                    {
                        QuixConsole.Log("Key not found",e);
                    }

                }


                foreach (var item in staticObjectsHandlers)
                {
                    if (item.Value.needUpdate)
                    {
                        //bodies.Add(item.Value.getJSON());
                        bodies2[bodiesAdded] = item.Value.getJSON();
                        //QuixConsole.Log("Updating",item.Value.state.type,item.Value.state.position);
                        item.Value.needUpdate = false;
                        bodiesAdded += 1;
                    }
                }

                if (bodiesAdded > 0)
                {
                    var slice = bodies2[0..bodiesAdded];
                    SendMessage("update", JsonConvert.SerializeObject(slice), connectionState.workSocket);
                }*/
        }
        public void SendMessage(string type, Newtonsoft.Json.Linq.JObject message, Socket socket)
        {
            SendMessage(type, message.ToString(), socket);
        }
        public void SendMessage(string type, string message, Socket socket)
        {
            MessageState mess = new MessageState();
            mess.type = type;
            mess.data = message;

            // Console.WriteLine(JsonConvert.SerializeObject(mess));
            server.Send(socket, JsonConvert.SerializeObject(mess, Formatting.None));
        }


       /* public PhyObject Create(ObjectState state,Room room)
        {
            PhyObject phy = null;
            if (state.uID == null || objects.ContainsKey(state.uID))
            {
                state.uID = PhyObject.createUID();
            }
            if (state is BoxState)
            {
                if (state.isMesh)
                {
                    phy = CreateMesh((BoxState)state,room);
                }
                else
                {
                    phy = CreateBox((BoxState)state,room);
                }

            }
            if (state is SphereState)
            {
                phy = CreateSphere((SphereState)state,room);
            }


            if (!objects.ContainsKey(state.uID))
            {
                objects.Add(state.uID, phy);
            }
            else
            {
                QuixConsole.WriteLine("Objects already had that key");
            }
            return phy;

        }
        private PhyObject CreateVanilla(ObjectState state, CollidableDescription collidableDescription, BodyInertia bodyInertia,Room room)
        {
            PhyObject phy;
            Guid guid = Guid.NewGuid();
            var material = new SimpleMaterial
            {
                FrictionCoefficient = 1f,
                MaximumRecoveryVelocity = float.MaxValue,
                SpringSettings = new SpringSettings(30, 1f),
                collidable = true,
                guid = guid
            };
            if(state.quaternion == new Quaternion()){
                state.quaternion = Quaternion.Identity;
            }
            if (state.mass != 0)
            {
                BodyDescription boxDescription = BodyDescription.CreateDynamic(state.position, bodyInertia,
                     collidableDescription,
                     new BodyActivityDescription(0.01f));

                boxDescription.Pose = new RigidPose(state.position, state.quaternion);
                var bodyHandle = Simulation.Bodies.Add(boxDescription);

                SimpleMaterial allocatedMat = collidableMaterials.Allocate(bodyHandle) = material;

                phy = SetUpPhyObject(new Handle{bodyHandle = bodyHandle}, state, guid,room);

                objectsHandlers.Add(bodyHandle, phy);
                allObjects.Add(guid, phy);

            }
            else
            {

                StaticDescription description = new StaticDescription(state.position, state.quaternion, collidableDescription);
                StaticHandle handle = Simulation.Statics.Add(description);

                collidableMaterials.Allocate(handle) = material;
                phy = SetUpPhyObject(new Handle{staticHandle = handle}, state, guid,room);

                staticObjectsHandlers.Add(handle, phy);
                allObjects.Add(guid, phy);

            }


            return phy;
        }
        private PhyObject CreateBox(BoxState state,Room room)
        {
            var box = new Box(state.halfSize.X, state.halfSize.Y, state.halfSize.Z);

            CollidableDescription collidableDescription = new CollidableDescription(Simulation.Shapes.Add(box), 0.1f);
            BodyInertia bodyInertia;

            box.ComputeInertia(state.mass, out bodyInertia);


            var phy = CreateVanilla(state, collidableDescription, bodyInertia,room);
            return phy;

        }

        private PhyObject CreateSphere(SphereState state,Room room)
        {

            var sphere = new Sphere(state.radius);
            CollidableDescription collidableDescription = new CollidableDescription(Simulation.Shapes.Add(sphere), 0.1f);
            BodyInertia bodyInertia;

            sphere.ComputeInertia(state.mass, out bodyInertia);

            var phy = CreateVanilla(state, collidableDescription, bodyInertia,room);
            return phy;
        }

        private PhyObject CreateMesh(BoxState state,Room room)
        {
            LoadModel(server.GetMesh(state.mesh), out var mesh, state.halfSize);

            //fs.Close();

            TypedIndex shapeIndex = Simulation.Shapes.Add(mesh);

            CollidableDescription collidableDescription = new CollidableDescription(shapeIndex, 0.1f);
            

            mesh.ComputeClosedInertia(state.mass, out var bodyInertia);

            var phy = CreateVanilla(state, collidableDescription, bodyInertia,room);
            phy.shapeIndex = shapeIndex;
            objects.Add(state.uID, phy);

            return phy;
        }

        public void LoadModel(MeshContent meshContent, out Mesh mesh, Vector3 scale)
        {
            bufferPool.Take<Triangle>(meshContent.Triangles.Length, out var triangles);

            for (int i = 0; i < meshContent.Triangles.Length; ++i)
            {
                triangles[i] = new Triangle(meshContent.Triangles[i].A, meshContent.Triangles[i].B, meshContent.Triangles[i].C);
            }

            // scale.Y *=-1;
            mesh = new Mesh(triangles, scale, bufferPool);


        }

        private PhyObject GetPhyClass(string name)
        {
            System.Type t = System.Type.GetType("QuixPhysics." + name + ", QuixPhysics");
            
            PhyObject phy = null;
            if (t != null)
            {
                
                phy = (PhyObject)Activator.CreateInstance(t);
            }
            else
            {
                phy = new PhyObject();
            }
            return phy;
        }

        private PhyObject SetUpPhyObject(Handle bodyHandle, ObjectState state, Guid guid,Room room)
        {

            PhyObject phy = GetPhyClass(state.type);

            phy.Load(bodyHandle, connectionState, this, state,guid,room);
           
            return phy;
        }*/

        public void Close()
        {
            QuixConsole.WriteLine("Closing Simulator");




            collidableMaterials.Dispose();

           

            foreach (var room in roomManager.rooms)
            {
                room.Value.Dispose();
            }

            //Simulation.Bodies.Dispose();
            // Simulation.Statics.Dispose();

            Simulation.Dispose();
            bufferPool.Clear();
            ThreadDispatcher.Dispose();

            //OnContactListeners.Clear();
            // OnStaticContactListeners.Clear();
            connectionState.Dispose();
            //server.isRunning = false;

            Disposed = true;
            gameLoop.Stop();

            //Dispose();


        }

        public void Dispose()
        {
            GC.Collect();

            // Wait for all finalizers to complete before continuing.
            GC.WaitForPendingFinalizers();
        }
    }
}