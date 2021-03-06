namespace QuixPhysics
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using BepuPhysics;
    using BepuPhysics.Collidables;
    using BepuUtilities.Memory;

    public class Simulator
    {
        private BufferPool bufferPool;

        public SimpleThreadDispatcher ThreadDispatcher { get; }
        public Simulation Simulation { get; }
        public GameLoop gameLoop = null;
        public Dictionary<string,PhyObject> objects = new Dictionary<string, PhyObject>();
        public Simulator()
        {
            bufferPool = new BufferPool();
            var targetThreadCount = Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
            ThreadDispatcher = new SimpleThreadDispatcher(targetThreadCount);
            Simulation = Simulation.Create(bufferPool, new QuixNarrowPhaseCallbacks(), new QuixPoseIntegratorCallbacks(new Vector3(0, -10, 0)), new PositionFirstTimestepper());


            CreateMap();
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
            for (int a = 0; a < 1000; a++)
            {
              var box = new Box(1,1,1);
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
            PhyObject phy = new PhyObject(boxDescription);
        }

        private void createFloor()
        {
            Simulation.Statics.Add(new StaticDescription(new Vector3(0, -0.5f, 0), new CollidableDescription(Simulation.Shapes.Add(new Box(500, 1, 500)), 0.1f)));
        }

        internal void Update(TimeSpan gameTime)
        {
            Simulation.Timestep(1 / 60f, ThreadDispatcher);
        }
    }
}