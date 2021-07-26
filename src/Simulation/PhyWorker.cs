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
        public bool destroy = false;
        public bool paused = false;
        public PhyWorker(int time, Simulator _simulator)
        {
            this.time = time;
            //Console.WriteLine("Created a worker");
            this.simulator = _simulator;
            lock (_simulator.workers)
            {
                _simulator.workersToAdd.Add(this);
            }


        }
        protected virtual void OnTick()
        {
            Tick?.Invoke();
        }
        protected virtual void OnCompleted()
        {
            Completed?.Invoke();
        }
        protected void Destroy()
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
            }else{
                paused = true;
            }

        }


    }
    public class Waiter
    {
        public int time;
        private int tickTime;
        public Waiter(int time)
        {
            this.time = time;
        }
        public bool Wait()
        {
            tickTime++;
            if (time == tickTime)
            {
                time = 0;
                return true;
            }
            return false;
        }
    }
}