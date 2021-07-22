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

    public class Simulator : IDisposable
    {
        private BufferPool bufferPool;

        public SimpleThreadDispatcher ThreadDispatcher { get; }
        public Simulation Simulation { get; }
        public GameLoop gameLoop = null;
        public Dictionary<BodyHandle, PhyObject> objectsHandlers = new Dictionary<BodyHandle, PhyObject>();
        public Dictionary<StaticHandle, StaticPhyObject> staticObjectsHandlers = new Dictionary<StaticHandle, StaticPhyObject>();
        public Dictionary<string, PhyObject> objects = new Dictionary<string, PhyObject>();
        public CollidableProperty<SimpleMaterial> collidableMaterials;
        public ConnectionState connectionState;
        internal Server server;

        private int t = 0;
        private int tMax = 15000;
        internal int boxToCreate = 10;
        internal int timesPressedCreateBoxes = 0;
        internal List<PhyWorker> workers = new List<PhyWorker>();
        internal List<PhyWorker> workersToAdd = new List<PhyWorker>();
        internal CommandReader commandReader;
        internal Thread thread;

        public QuixNarrowPhaseCallbacks narrowPhaseCallbacks;

        public Dictionary<BodyHandle, PhyObject> OnContactListeners = new Dictionary<BodyHandle, PhyObject>();
        public Dictionary<StaticHandle, PhyObject> OnStaticContactListeners = new Dictionary<StaticHandle, PhyObject>();

        public Dictionary<string, User> users = new Dictionary<string, User>();

        public bool Disposed = false;

        public MapMongo map;


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
            Simulation = Simulation.Create(bufferPool, narrowPhaseCallbacks, new QuixPoseIntegratorCallbacks(new Vector3(0, -5, 0)), new PositionFirstTimestepper());

            CreateMap();
            // Server.Send(state.workSocket, "Hola desde simulator");
            gameLoop = new GameLoop();
            gameLoop.Load(this);


            //CreateNewt();


            thread = new Thread(new ThreadStart(gameLoop.Start));

            commandReader = new CommandReader(this);
            thread.Start();



        }

        private void CreateNewt()
        {
            BoxState state = new BoxState();
            state.position = new Vector3(0, 140, 160);
            state.halfSize = new Vector3(10, 10, 10);
            state.quaternion = Quaternion.Identity;
            state.mass = 0;
            state.uID = PhyObject.createUID();
            state.type = "QuixBox";
            state.instantiate = true;
            state.mesh = "Tiles/test";

            // CreateMesh(state);
        }

        private void CreateMap()
        {
            createFloor();
            createObjects();
        }
        internal void createObjects()
        {
            int width = 10;
            int sizeObj = 60;
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
                box.position = new Vector3(x * sizeObj, 1050 + (timesPressedCreateBoxes * sizeObj), y * sizeObj);
                box.radius = 10;
                box.mesh = "Board/Bomb";
                box.instantiate = true;
                box.quaternion = Quaternion.Identity;
                var b = Create(box);
                /* int x = a % width;    // % is the "modulo operator", the remainder of i / width;
               int y = a / width;    // where "/" is an integer division
               var ringBoxShape = new Box(1, 1, 1);
               ringBoxShape.ComputeInertia(1, out var ringBoxInertia);
               var boxDescription = BodyDescription.CreateDynamic(new Vector3(), ringBoxInertia,
                                  new CollidableDescription(Simulation.Shapes.Add(ringBoxShape), 0.1f),
                                  new BodyActivityDescription(0.01f));
               new BodyActivityDescription(0.01f);

           boxDescription.Pose = new RigidPose(new Vector3(x,300,y), Quaternion.Identity);
           Simulation.Bodies.Add(boxDescription);*/
            }
            timesPressedCreateBoxes++;
            // Console.WriteLine("Statics size " + Simulation.Statics.Count);
        }

        private void createFloor()
        {
            // Simulation.Statics.Add(new StaticDescription(new Vector3(0, -0.5f, 0), new CollidableDescription(Simulation.Shapes.Add(new Box(5000, 1, 5000)), 0.1f)));
            BoxState box = new BoxState();
            box.position = new Vector3(0, 0, 0);
            box.uID = PhyObject.createUID();
            box.mass = 0;
            box.quaternion = Quaternion.Identity;
            box.type = "QuixBox";
            box.halfSize = new Vector3(5000, 1, 5000);

            // Create(box);
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
                if (item.destroy)
                {
                    toDelete.Add(item);
                }
            }
            foreach (var item in toDelete)
            {
                workers.Remove(item);
            }
        }

        internal void Update(TimeSpan gameTime)
        {
            //Console.WriteLine(gameTime.TotalSeconds);
            handleWorkers();
            Simulation.Timestep(1/60f, ThreadDispatcher);

            ArrayList bodies = new ArrayList();
            var set = Simulation.Bodies.Sets[0];

            if (t == tMax)
            {
                // createObjects();
                t = 0;
            }
            t++;


            for (var bodyIndex = 0; bodyIndex < set.Count; ++bodyIndex)
            {
                try
                {
                    //var handle = set.IndexToHandle[bodyIndex];
                    // if(objects.ContainsValue())
                    var handle = set.IndexToHandle[bodyIndex];
                    if (objectsHandlers[handle].state.instantiate)
                    {
                        bodies.Add(objectsHandlers[handle].getJSON());
                    }


                }
                catch (KeyNotFoundException e)
                {
                    QuixConsole.WriteLine("Key not found");
                }

            }


            foreach (var item in staticObjectsHandlers)
            {
                if (item.Value.needUpdate)
                {
                    bodies.Add(item.Value.getJSON());
                    item.Value.needUpdate = false;
                }
            }

            if (bodies.Count > 0)
            {
                SendMessage("update", JsonConvert.SerializeObject(bodies), connectionState.workSocket);
            }

            commandReader.ReadCommand();


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

        public PhyObject Create(ObjectState state)
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
                    phy = CreateMesh((BoxState)state);
                }
                else
                {
                    phy = CreateBox((BoxState)state);
                }

            }
            if (state is SphereState)
            {
                phy = CreateSphere((SphereState)state);
            }

            if (phy.material == default(SimpleMaterial))
            {
                collidableMaterials.Allocate(phy.bodyHandle) = new SimpleMaterial
                {
                    FrictionCoefficient = .1f,
                    MaximumRecoveryVelocity = float.MaxValue,
                    SpringSettings = new SpringSettings(1f, 1.5f)
                };

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
        private PhyObject CreateVanilla(ObjectState state, CollidableDescription collidableDescription, BodyInertia bodyInertia)
        {
            PhyObject phy;

            if (state.mass != 0)
            {
                BodyDescription boxDescription = BodyDescription.CreateDynamic(state.position, bodyInertia,
                     collidableDescription,
                     new BodyActivityDescription(0.01f));

                boxDescription.Pose = new RigidPose(state.position, state.quaternion);
                var bodyHandle = Simulation.Bodies.Add(boxDescription);



                phy = SetUpPhyObject(bodyHandle, state);
                objectsHandlers.Add(bodyHandle, phy);

            }
            else
            {

                StaticDescription description = new StaticDescription(state.position, state.quaternion, collidableDescription);
                StaticHandle handle = Simulation.Statics.Add(description);
                //collidableMaterials.Allocate(handle) = new SimpleMaterial { FrictionCoefficient = 1, MaximumRecoveryVelocity = float.MaxValue, SpringSettings = new SpringSettings(1f, 1f) };
                phy = SetUpPhyObject(handle, state);
                staticObjectsHandlers.Add(handle, (StaticPhyObject)phy);
                //objectsHandlers.Add(handle,phy);
            }


            return phy;
        }
        private PhyObject CreateBox(BoxState state)
        {
            var box = new Box(state.halfSize.X, state.halfSize.Y, state.halfSize.Z);

            CollidableDescription collidableDescription = new CollidableDescription(Simulation.Shapes.Add(box), 0.1f);
            BodyInertia bodyInertia;

            box.ComputeInertia(state.mass, out bodyInertia);


            var phy = CreateVanilla(state, collidableDescription, bodyInertia);
            return phy;

        }

        private PhyObject CreateSphere(SphereState state)
        {

            var sphere = new Sphere(state.radius);
            CollidableDescription collidableDescription = new CollidableDescription(Simulation.Shapes.Add(sphere), 0.1f);
            BodyInertia bodyInertia;

            sphere.ComputeInertia(state.mass, out bodyInertia);

            var phy = CreateVanilla(state, collidableDescription, bodyInertia);
            return phy;
        }

        private PhyObject CreateMesh(BoxState state)
        {
            LoadModel(server.GetMesh(state.mesh), out var mesh, state.halfSize);

            //fs.Close();

            CollidableDescription collidableDescription = new CollidableDescription(Simulation.Shapes.Add(mesh), 0.1f);

            mesh.ComputeClosedInertia(state.mass, out var bodyInertia);

            var phy = CreateVanilla(state, collidableDescription, bodyInertia);
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

        private PhyObject SetUpPhyObject(BodyHandle bodyHandle, ObjectState state)
        {

            PhyObject phy = GetPhyClass(state.type);

            phy.Load(bodyHandle, connectionState, this, state);

            return phy;
        }
        private StaticPhyObject SetUpPhyObject(StaticHandle staticHandle, ObjectState state)
        {
            StaticPhyObject phy = (StaticPhyObject)GetPhyClass(state.type);

            phy.Load(staticHandle, connectionState, this, state);

            return phy;
        }

        public PhyObject handleToPhyObject(BodyHandle handle)
        {
            PhyObject obj = objectsHandlers[handle];
            return obj;
        }
        public StaticPhyObject handleToPhyObject(StaticHandle handle)
        {
            StaticPhyObject obj = staticObjectsHandlers[handle];

            return obj;
        }
        public PhyObject collidableToPhyObject(CollidableReference reference)
        {
            if (reference.Mobility == CollidableMobility.Static)
            {
                return handleToPhyObject(reference.StaticHandle);
            }
            else
            {
                return handleToPhyObject(reference.BodyHandle);
            }
        }
        public void Close()
        {
            QuixConsole.WriteLine("Closing Simulator");




            collidableMaterials.Dispose();

            objects.Clear();
            objectsHandlers.Clear();
            staticObjectsHandlers.Clear();

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
    class CommandReader
    {
        private ArrayList commandsList = new ArrayList();
        private Simulator simulator;

        public CommandReader(Simulator _simulator)
        {
            simulator = _simulator;
        }
        internal void AddCommandToBeRead(string v)
        {
            commandsList.Add(v);
        }
        internal void Shoot(string data)
        {
            QuixConsole.WriteLine(data);
            ShootMessage j2 = JsonConvert.DeserializeObject<ShootMessage>(data);
            //objects[]
            Player2 onb2 = (Player2)simulator.users[j2.client].player;
            // Simulation.Awakener.AwakenBody(ob.bodyHandle);
            onb2.Shoot(j2);
        }
        internal void UseGauntlet(string data)
        {
            QuixConsole.Log("Use gauntlet", data);
            GauntletMessage j2 = JsonConvert.DeserializeObject<GauntletMessage>(data);
            //objects[]
            Player2 onb2 = (Player2)simulator.users[j2.client].player;
            // Simulation.Awakener.AwakenBody(ob.bodyHandle);
            onb2.UseGauntlet(j2.active);
        }
         internal void Swipe(string data)
        {
            QuixConsole.Log("Swiping", data);
            SwipeMessage j2 = JsonConvert.DeserializeObject<SwipeMessage>(data);
            //objects[]
            Player2 onb2 = (Player2)simulator.users[j2.client].player;
            // Simulation.Awakener.AwakenBody(ob.bodyHandle);
            onb2.Swipe(j2.degree,j2.direction);
        }
        internal void Jump(string data)
        {
            MoveMessage j2 = JsonConvert.DeserializeObject<MoveMessage>(data);
            //objects[]
            Player2 onb2 = (Player2)simulator.users[j2.client].player;
            // Simulation.Awakener.AwakenBody(ob.bodyHandle);
            onb2.Jump(j2);
        }
        internal void ReadCommand()
        {
            //Console.WriteLine("Read command: {0}", v);

            try
            {
                foreach (var item in commandsList)
                {

                    JsonSerializerSettings setting = new JsonSerializerSettings();
                    setting.CheckAdditionalContent = false;

                    Newtonsoft.Json.Linq.JObject message = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>((string)item, setting);
                    string type = (string)message["type"];
                    switch (type)
                    {
                        case "create":

                            // Console.WriteLine(message);

                            if (message["data"]["halfSize"] != null)
                            {
                                BoxState ob = JsonConvert.DeserializeObject<BoxState>(((object)message["data"]).ToString());
                                simulator.Create(ob);
                            }

                            if (message["data"]["radius"] != null)
                            {
                                SphereState ob = JsonConvert.DeserializeObject<SphereState>(((object)message["data"]).ToString());
                                simulator.Create(ob);
                                break;
                            }

                            break;
                        case "createBoxes":
                            //Console.WriteLine("Create boxes");
                            // simulator.boxToCreate = 10;
                            simulator.createObjects();
                            break;
                        case "gauntlet":

                            UseGauntlet(((object)message["data"]).ToString());
                            break;
                        case "move":

                            MoveMessage j = JsonConvert.DeserializeObject<MoveMessage>(((object)message["data"]).ToString());
                            //objects[]
                            Player2 onb = (Player2)simulator.users[j.client].player;
                            // Simulation.Awakener.AwakenBody(ob.bodyHandle);
                            onb.Move(j);

                            break;
                        case "rotate":
                            MoveMessage j2 = JsonConvert.DeserializeObject<MoveMessage>(((object)message["data"]).ToString());
                            //objects[]
                            Player2 onb2 = (Player2)simulator.users[j2.client].player;
                            // Simulation.Awakener.AwakenBody(ob.bodyHandle);
                            onb2.Rotate(j2);
                            break;
                        case "jump":
                            Jump(((object)message["data"]).ToString());
                            break;
                        case "shoot":
                            Shoot(((object)message["data"]).ToString());
                            break;
                        case "swipe":
                            Swipe(((object)message["data"]).ToString());
                            break;
                        case "generateMap":

                            var map = this.simulator.server.dataBase.GetMap((string)message["data"]);
                            this.simulator.map = map;

                            foreach (var obj in map.objects)
                            {

                                //obj.ToJson();
                                if (obj.Contains("halfSize"))
                                {

                                    obj["halfSize"].AsBsonDocument.Remove("__refId");
                                    obj.Remove("_id");
                                    var stri = JsonConvert.DeserializeObject<BoxState>(obj.ToJson());
                                    stri.quaternion = JsonConvert.DeserializeObject<Quaternion>(obj["quat"].ToJson());

                                    this.simulator.Create(stri);
                                }
                                if (obj.Contains("radius"))
                                {

                                    // obj["radius"].AsBsonDocument.Remove("__refId");
                                    obj.Remove("_id");
                                    var stri = JsonConvert.DeserializeObject<SphereState>(obj.ToJson());
                                    stri.quaternion = JsonConvert.DeserializeObject<Quaternion>(obj["quat"].ToJson());

                                    this.simulator.Create(stri);
                                }
                            }
                            break;
                        case "close":
                            //Console.WriteLine("Close");
                            simulator.Close();

                            break;
                        default:
                            QuixConsole.WriteLine("Command not registred " + type);
                            break;

                    }

                }

                commandsList.Clear();
            }
            catch (InvalidOperationException e)
            {
                QuixConsole.Log("Collection was modifieded",e);
            }
            catch (JsonReaderException e)
            {
                QuixConsole.Log("Json Problem ", e);
            }
            catch(Exception e){
                QuixConsole.WriteLine(e);
            }


        }
    }
}