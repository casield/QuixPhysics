using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
namespace QuixPhysics
{
    public class GameLoop
    {
        private Simulator _mySimulator;
        private double TICKS_PER_UPDATE = 10000;//.01;
        private double current_Time = 0;


        /// <summary>
        /// Status of GameLoop
        /// </summary>
        public bool Running { get; private set; }

        /// <summary>
        /// Load Game into GameLoop
        /// </summary>
        public void Load(Simulator gameObj)
        {
            _mySimulator = gameObj;
        }

        /// <summary>
        /// Start GameLoop
        /// </summary>
        public void Start()
        {
            if (_mySimulator == null)
                throw new ArgumentException("Simulator not loaded!");

            // Load game content
            //_mySimulator.Load();

            // Set gameloop state
            Running = true;

            // Set previous game time
            double previous = getCurrentTime();
            double lag = 0.0;

            while (Running)
            {
                // Calculate the time elapsed since the last game loop cycle
                double current = getCurrentTime();
                double elapsed = current - previous;
                previous = current;
                lag += elapsed;

                // Update the game
                //QuixConsole.Log(".....");
                int repeat = 0;
                if (_mySimulator != null && !_mySimulator.Disposed)
                {


                    while (lag >= TICKS_PER_UPDATE)
                    {
                        //  QuixConsole.Log("Inside",lag); 
                        repeat += 1;
                        _mySimulator.commandReader.ReadCommand();
                        _mySimulator.Update();
                        lag -= TICKS_PER_UPDATE;
                    }
                   // QuixConsole.Log("repeated ",repeat);


                }


                if (_mySimulator.Disposed)
                {
                    Running = false;
                }
                //QuixConsole.Log("End", DateTime.Now.Ticks - start);

                // Update Game at 60fps
                //Thread.Sleep((int)(1));
            }


        }

        private double getCurrentTime()
        {
            current_Time += 1;
            return DateTime.Now.Ticks;
        }

        /// <summary>
        /// Stop GameLoop
        /// </summary>
        public void Stop()
        {
            Running = false;
            //_myGame?.Unload();
        }

        /// <summary>
        /// Draw Game Graphics
        /// </summary>
    }
}