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
    using BepuUtilities.Memory;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class Simulator
    {
        private BufferPool bufferPool;

        public SimpleThreadDispatcher ThreadDispatcher { get; }
        public Simulation Simulation { get; }
        public GameLoop gameLoop = null;
        public Dictionary<BodyHandle, PhyObject> objectsHandlers = new Dictionary<BodyHandle, PhyObject>();
        public Dictionary<string, PhyObject> objects = new Dictionary<string, PhyObject>();
        public ConnectionState connectionState;
        private Server server;

        private int t = 0;
        private int tMax = 15000;
        private ArrayList commandsList = new ArrayList();
        private int boxToCreate = 100;

        public Simulator(ConnectionState state, Server server)
        {
            this.connectionState = state;
            this.server = server;
            bufferPool = new BufferPool();
            var targetThreadCount = Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
            ThreadDispatcher = new SimpleThreadDispatcher(targetThreadCount);
            Simulation = Simulation.Create(bufferPool, new QuixNarrowPhaseCallbacks(), new QuixPoseIntegratorCallbacks(new Vector3(0, -10, 0)), new PositionFirstTimestepper());

            CreateMap();
            // Server.Send(state.workSocket, "Hola desde simulator");
            gameLoop = new GameLoop();
            gameLoop.Load(this);

            Thread t = new Thread(new ThreadStart(gameLoop.Start));
            try
            {
                t.Start();
            }
            catch (SocketException e)
            {
                Console.WriteLine("Exception in Simulator Start - " + e.ToString());
            }
        }

        internal void Load()
        {

        }

        private void CreateMap()
        {
            createFloor();
            createObjects();
        }

        private void createObjects()
        {
            for (int a = 0; a < boxToCreate; a++)
            {
                var box = new BoxState();
                box.uID = PhyObject.createUID();
                box.mass = 1;
                box.position = new Vector3(11 * a, 630, 30);
                box.halfSize = new Vector3(10, 10, 10);
                box.quaternion = Quaternion.Identity;
                Create(box);
            }
        }

        private void createFloor()
        {
            Simulation.Statics.Add(new StaticDescription(new Vector3(0, -0.5f, 0), new CollidableDescription(Simulation.Shapes.Add(new Box(5000, 1, 5000)), 0.1f)));
        }

        internal void Update(TimeSpan gameTime)
        {
            // Console.WriteLine(gameTime.TotalSeconds);
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
                    var handle = set.IndexToHandle[bodyIndex];
                    bodies.Add(objectsHandlers[handle].getJSON());
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

            ReadCommand();


        }
        public void SendMessage(string type, JObject message, Socket socket)
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

            objectsHandlers.Add(phy.bodyHandle, phy);
            objects.Add(state.uID, phy);
        }
        private PhyObject CreateBox(BoxState state)
        {
            var box = new Box(state.halfSize.X, state.halfSize.Y, state.halfSize.Z);
            var ringBoxShape = box;

            ringBoxShape.ComputeInertia(state.mass, out var ringBoxInertia);
            var boxDescription = BodyDescription.CreateDynamic(new Vector3(), ringBoxInertia,
                new CollidableDescription(Simulation.Shapes.Add(ringBoxShape), 0.1f),
                new BodyActivityDescription(0.01f));

            boxDescription.Pose = new RigidPose(state.position, state.quaternion);
            var bodyHandle = Simulation.Bodies.Add(boxDescription);
            var phy = SetUpPhyObject(bodyHandle, boxDescription, state);

            return phy;
        }

        private PhyObject SetUpPhyObject(BodyHandle bodyHandle, BodyDescription description, ObjectState state)
        {
            System.Type t = System.Type.GetType("QuixPhysics." + state.type + ", QuixPhysics");
            PhyObject phy = null;
            if (t != null)
            {
                phy = (PhyObject)Activator.CreateInstance(t);
            }
            else
            {
                phy = new PhyObject();
            }
            phy.Load(bodyHandle, description, connectionState, this, state);

            return phy;
        }

        internal void AddCommandToBeRead(string v)
        {
            commandsList.Add(v);
        }
        internal void ReadCommand()
        {
            //Console.WriteLine("Read command: {0}", v);
            try
            {
                foreach (var item in commandsList)
                {
                    MessageState message = JsonConvert.DeserializeObject<MessageState>((string)item);

                    if (message.type == "create")
                    {
                        string typ = message.data.ToString().Split("type")[1].Split(",")[0].Trim(new Char[] { '"', ':' }).Remove(0, 2);


                        Type t = System.Type.GetType("QuixPhysics." + typ + ", QuixPhysics");
                        BoxState ob = JsonConvert.DeserializeObject<BoxState>(message.data.ToString()); ;
                        Create(ob);
                    }

                    if (message.type == "createBoxes")
                    {
                        Console.WriteLine("Create boxes");
                        createObjects();
                    }

                    if (message.type == "move")
                    {
                        try
                        {
                            MoveMessage j = JsonConvert.DeserializeObject<MoveMessage>(message.data.ToString());
                            //objects[]
                            PhyObject ob = objects[j.uID];
                            // Simulation.Awakener.AwakenBody(ob.bodyHandle);
                            BodyDescription bb;
                            Simulation.Bodies.GetDescription(ob.bodyHandle, out bb);
                            //Simulation.Bodies.
                            j.x /= 10;
                            j.y /= 10;

                            bb.Velocity.Linear += new Vector3(-j.y, 0, j.x);
                            Simulation.Bodies.ApplyDescription(ob.bodyHandle, in bb);
                            Console.WriteLine("Move" + j.x);
                        }
                        catch (NullReferenceException e)
                        {

                        }
                        catch (ArgumentOutOfRangeException e)
                        {

                        }

                    }
                }

                commandsList.Clear();
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("Collection was modifyes");
            }


        }
    }
}