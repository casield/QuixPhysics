using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
namespace QuixPhysics
{
    public class GameLoop
    {
        private Simulator _mySimulator;

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
            _mySimulator.Load();

            // Set gameloop state
            Running = true;

            // Set previous game time
            DateTime _previousGameTime = DateTime.Now;
            try
            {
                while (true)
                {
                    // Calculate the time elapsed since the last game loop cycle
                    TimeSpan GameTime = DateTime.Now - _previousGameTime;
                    // Update the current previous game time
                    _previousGameTime = _previousGameTime + GameTime;
                    // Update the game
                    _mySimulator.Update(GameTime);


                    // Update Game at 60fps
                     Thread.Sleep(8);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in gameloop. {0}", e);
            }
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