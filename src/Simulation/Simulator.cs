namespace QuixPhysics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Numerics;
    using System.Threading;
    using BepuPhysics;
    using BepuPhysics.Collidables;
    using BepuPhysics.Constraints;
    using BepuUtilities.Memory;
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
        internal List<PhyWorker> workers = new List<PhyWorker>();
        internal CommandReader commandReader;
        internal Thread thread;

        public QuixNarrowPhaseCallbacks narrowPhaseCallbacks;

        public Dictionary<BodyHandle, PhyObject> OnContactListeners = new Dictionary<BodyHandle, PhyObject>();
        public Dictionary<StaticHandle, PhyObject> OnStaticContactListeners = new Dictionary<StaticHandle, PhyObject>();

        public bool Disposed = false;


        public Simulator(ConnectionState state, Server server)
        {

            collidableMaterials = new CollidableProperty<SimpleMaterial>();

            this.connectionState = state;
            this.server = server;
            bufferPool = new BufferPool();
            var targetThreadCount = Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
            ThreadDispatcher = new SimpleThreadDispatcher(targetThreadCount);
            narrowPhaseCallbacks = new QuixNarrowPhaseCallbacks() { CollidableMaterials = collidableMaterials, simulator = this };
            Simulation = Simulation.Create(bufferPool, narrowPhaseCallbacks, new QuixPoseIntegratorCallbacks(new Vector3(0, -20, 0)), new PositionFirstTimestepper());

            CreateMap();
            // Server.Send(state.workSocket, "Hola desde simulator");
            gameLoop = new GameLoop();
            gameLoop.Load(this);

            thread = new Thread(new ThreadStart(gameLoop.Start));

            commandReader = new CommandReader(this);
            thread.Start();


        }

        internal void Load()
        {

        }

        private void CreateMap()
        {
            createFloor();
            createObjects();
        }

        internal void createObjects()
        {
            int width = 30;
            for (int a = 0; a < boxToCreate; a++)
            {
                var box = new BoxState();
                box.uID = PhyObject.createUID();
                box.mass = 1;
                box.type = "QuixBox";
                // box.instantiate = false;

                int x = a % width;    // % is the "modulo operator", the remainder of i / width;
                int y = a / width;    // where "/" is an integer division
                box.position = new Vector3(x * 11, 630, y * 11);
                box.halfSize = new Vector3(10, 10, 10);
                box.mesh = "Board/Bomb";
                box.instantiate = true;
                box.quaternion = Quaternion.Identity;
                Create(box);
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

        internal void Update(TimeSpan gameTime)
        {
            //Console.WriteLine(gameTime.TotalSeconds);
            foreach (var item in workers)
            {
                item.tick();
            }
            Simulation.Timestep(1 / 30f, ThreadDispatcher);

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
                    Console.WriteLine("Key not found");
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

        public void Create(ObjectState state)
        {
            PhyObject phy = null;
            if (state is BoxState)
            {
                phy = CreateBox((BoxState)state);
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
                Console.WriteLine("Objects already had that key");
            }

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
            Console.WriteLine("Closing Simulator");
            


            
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
        internal void Jump(string data)
        {
            MoveMessage j2 = JsonConvert.DeserializeObject<MoveMessage>(data);
            //objects[]
            Player2 onb2 = (Player2)simulator.objects[j2.uID];
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
                    // Console.WriteLine(item);
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
                        case "move":

                            MoveMessage j = JsonConvert.DeserializeObject<MoveMessage>(((object)message["data"]).ToString());
                            //objects[]
                            Player2 onb = (Player2)simulator.objects[j.uID];
                            // Simulation.Awakener.AwakenBody(ob.bodyHandle);
                            onb.Move(j);

                            break;
                        case "rotate":
                            MoveMessage j2 = JsonConvert.DeserializeObject<MoveMessage>(((object)message["data"]).ToString());
                            //objects[]
                            Player2 onb2 = (Player2)simulator.objects[j2.uID];
                            // Simulation.Awakener.AwakenBody(ob.bodyHandle);
                            onb2.Rotate(j2);
                            break;
                        case "jump":
                            Console.WriteLine(((object)message["data"]).ToString());
                            Jump(((object)message["data"]).ToString());
                            break;
                        case "generateMap":

                            var map = this.simulator.server.dataBase.GetMap((string)message["data"]);

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
                                    Console.WriteLine(obj);
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
                            Console.WriteLine("Command not registred " + type);
                            break;

                    }

                }

                commandsList.Clear();
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("Collection was modifyes");
            }
            catch (JsonReaderException e)
            {
                Console.WriteLine("Json Problem " + e);
            }


        }
    }
}