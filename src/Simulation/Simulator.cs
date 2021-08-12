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
    using Newtonsoft.Json.Linq;
    using OVars;

    public class Simulator : IDisposable
    {
        internal BufferPool bufferPool;

        public SimpleThreadDispatcher ThreadDispatcher { get; }
        public Simulation Simulation { get; }
        public int TIME_TO_SEND_TICK = 50;

        public GameLoop gameLoop = null;

        public CollidableProperty<SimpleMaterial> collidableMaterials;
        internal Server server;
        internal List<PhyWorker> workers = new List<PhyWorker>();
        internal List<PhyWorker> workersToAdd = new List<PhyWorker>();
        public CommandReader commandReader;
        internal Thread thread;

        public QuixNarrowPhaseCallbacks narrowPhaseCallbacks;
        public bool Disposed = false;
        public RoomManager roomManager;
        private int updateTick;

        public Simulator(Server server)
        {
            collidableMaterials = new CollidableProperty<SimpleMaterial>();

            // this.connectionState = state;
            this.server = server;
            bufferPool = new BufferPool();
            var targetThreadCount = Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
            ThreadDispatcher = new SimpleThreadDispatcher(targetThreadCount);
            narrowPhaseCallbacks = new QuixNarrowPhaseCallbacks() { CollidableMaterials = collidableMaterials, simulator = this };
            new Tests(this);
            Simulation = Simulation.Create(bufferPool, narrowPhaseCallbacks, new QuixPoseIntegratorCallbacks(new Vector3(0, -5, 0)), new PositionFirstTimestepper());


            gameLoop = new GameLoop();
            gameLoop.Load(this);


            roomManager = new RoomManager(this);
            commandReader = new CommandReader(this);

            thread = new Thread(new ThreadStart(gameLoop.Start));


            thread.Start();



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
            if (Simulation != null && ThreadDispatcher != null)
            {
                handleWorkers();

                Simulation.Timestep(1 / 60f, ThreadDispatcher);
                if (updateTick == TIME_TO_SEND_TICK)
                {
                    SendUpdate();
                    updateTick = 0;
                }
                else
                {
                    updateTick++;
                }
            }
        }
        public void SendUpdate()
        {
            foreach (var room in roomManager.rooms)
            {
                room.Value.factory.SendUpdate();
            }
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

        public void Close()
        {
            QuixConsole.WriteLine("Closing Simulator");

            collidableMaterials.Dispose();


            foreach (var room in roomManager.rooms)
            {
                room.Value.Dispose();
            }

            Simulation.Dispose();
            bufferPool.Clear();
            ThreadDispatcher.Dispose();

            Disposed = true;
            gameLoop.Stop();

        }

        public void Dispose()
        {
            GC.Collect();

            // Wait for all finalizers to complete before continuing.
            GC.WaitForPendingFinalizers();
        }
    }
}