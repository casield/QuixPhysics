namespace QuixPhysics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Numerics;
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
        public Dictionary<BodyHandle, PhyObject> objects = new Dictionary<BodyHandle, PhyObject>();
        public StateObject stateObject;
        private Server server;

        private int tickB = 0;
        private int timer = 60*3;
        public Simulator(StateObject state, Server server)
        {
            this.stateObject = state;
            this.server = server;
            bufferPool = new BufferPool();
            var targetThreadCount = Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
            ThreadDispatcher = new SimpleThreadDispatcher(targetThreadCount);
            Simulation = Simulation.Create(bufferPool, new QuixNarrowPhaseCallbacks(), new QuixPoseIntegratorCallbacks(new Vector3(0, -10, 0)), new PositionFirstTimestepper());


            CreateMap();
            // Server.Send(state.workSocket, "Hola desde simulator");




            gameLoop = new GameLoop();
            gameLoop.Load(this);
            gameLoop.Start().Wait();



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
            for (int a = 0; a < 10; a++)
            {
                var box = new Box(1, 1, 1);
                createBox(box);
            }
        }
        private void createBox(Box box)
        {
            var ringBoxShape = box;
            ringBoxShape.ComputeInertia(1, out var ringBoxInertia);
            var boxDescription = BodyDescription.CreateDynamic(new Vector3(), ringBoxInertia,
                new CollidableDescription(Simulation.Shapes.Add(ringBoxShape), 0.1f),
                new BodyActivityDescription(0.01f));

            boxDescription.Pose = new RigidPose(new Vector3(1, 9, 10), new Quaternion(0, 0, 0, 1));
            var bodyHandle = Simulation.Bodies.Add(boxDescription);
            PhyObject phy = new PhyObject(bodyHandle, boxDescription, stateObject, this);
            objects.Add(bodyHandle, phy);
        }

        private void createFloor()
        {
            Simulation.Statics.Add(new StaticDescription(new Vector3(0, -0.5f, 0), new CollidableDescription(Simulation.Shapes.Add(new Box(500, 1, 500)), 0.1f)));
        }

        internal void Update(TimeSpan gameTime)
        {
            Simulation.Timestep(1 / 60f, ThreadDispatcher);
            if (timer == tickB)
            {
                //createObjects();
                tickB = 0;
            }


            tickB++;

            ArrayList bodies = new ArrayList();


           // Console.WriteLine(Simulation.Bodies.Sets[0].Count);

            var set = Simulation.Bodies.Sets[0];

            for (var bodyIndex = 0; bodyIndex < set.Count; ++bodyIndex)
            {
                var handle = set.IndexToHandle[bodyIndex];
                 bodies.Add(objects[handle].getJSON());
            }
           // Console.WriteLine("Updating "+bodies.Count);

            SendMessage("update", JsonConvert.SerializeObject(bodies));
        }
        public void SendMessage(string type, JObject message)
        {
            SendMessage(type, message.ToString());
        }
        public void SendMessage(string type, string message)
        {
            JObject mess = new JObject();
            mess["type"] = type;
            mess["message"] = message;

            Server.Send(stateObject.workSocket, mess.ToString());
        }
    }
}