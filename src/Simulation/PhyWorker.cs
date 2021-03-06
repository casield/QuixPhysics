using System;

namespace QuixPhysics
{
    public delegate void PhyAction();
    public class PhyWorker
    {
        internal int time = 0;
        internal int tickTime = 0;

        public event PhyAction Tick;
        public event PhyAction Completed;
        private Simulator simulator;
        private bool destroy = false;
        public bool paused = false;
        public PhyWorker(int time, Simulator _simulator)
        {
            this.time = time;
            //Console.WriteLine("Created a worker");
            this.simulator = _simulator;

            _simulator.workersToAdd.Add(this);

        }

        public bool ShouldDestroy()
        {
            return destroy;
        }
        protected virtual void OnTick()
        {
            Tick?.Invoke();
        }
        protected virtual void OnCompleted()
        {
            Completed?.Invoke();
        }
        public void Destroy()
        {
            destroy = true;
            RemoveAllListeners();

        }
        public void RemoveAllListeners()
        {
            if (Tick != null)
            {
                foreach (PhyAction d in Tick.GetInvocationList())
                {
                    Tick -= d;
                }
            }
            if (Completed != null)
            {
                foreach (PhyAction d in Completed.GetInvocationList())
                {
                    Completed -= d;
                }
            }

        }

        public bool tick()
        {
            if (!paused)
            {
                if (time == tickTime)
                {
                    OnCompleted();
                    return true;
                }
                tickTime++;
                OnTick();
            }
            return false;
        }
        public void Reset()
        {
            tickTime = 0;
            paused = false;
        }

    }
    public class PhyInterval : PhyWorker
    {
        public PhyInterval(int time, Simulator simulator) : base(time, simulator)
        {
        }
        protected override void OnCompleted()
        {
            base.OnCompleted();
            this.tickTime = 0;
        }
    }
    public class PhyTimeOut : PhyWorker
    {
        private bool shouldDestroy = true;

        public PhyTimeOut(int time, Simulator _simulator, bool shouldDestroy) : base(time, _simulator)
        {
            this.shouldDestroy = shouldDestroy;
        }
        protected override void OnCompleted()
        {
            base.OnCompleted();
            if (shouldDestroy)
            {
                Destroy();
            }
            else
            {
                paused = true;
            }

        }


    }

    /// <summary>
    /// Waits an amount of time. Then it return true on the Tick method.
    /// </summary>
    public class PhyWaiter
    {
        private int tick = 0;
        private int time = 1;
        public event PhyAction OnCompleteListeners;
        public PhyWaiter(int time)
        {
            this.time = time;
        }
        public bool Tick()
        {
            tick++;
            if (tick == time)
            {
                OnTimeCompleted();
                Reset();
                return true;
            }
            return false;
        }
        public int GetTick()
        {
            return tick;
        }
        public void Reset()
        {
            tick = 0;
        }
        public void SetTime(int newTime)
        {
            time = newTime;
        }
        private void OnTimeCompleted()
        {
            OnCompleteListeners?.Invoke();
        }
    }
}