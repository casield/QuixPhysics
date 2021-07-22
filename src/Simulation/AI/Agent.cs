using System;
using System.Collections.Generic;

namespace QuixPhysics
{
    public class Agent
    {
        private PhyObject phy;
        private Dictionary<string, AgentState> registred = new Dictionary<string, AgentState>();

        private AgentState state;
        private bool isLocked = false;
        private int lockedTick = 0;
        private int lockedTime = -1;

        public Agent(PhyObject phy)
        {
            this.phy = phy;
        }
        public void Register(string name, AgentState state)
        {
            this.registred.Add(name, state);
        }
        public void Unlock()
        {
            this.isLocked = false;
            this.lockedTime = -1;
        }
        public void Lock(int milisec)
        {
            this.isLocked = true;
            this.lockedTime = milisec;
            this.lockedTick = 0;
        }

        public bool IsLocked()
        {
            return isLocked;
        }

        public void Tick()
        {
            if (this.state != null)
            {
                this.state.Tick();
            }
            if (this.isLocked)
            {
                if (this.lockedTick == this.lockedTime)
                {
                    QuixConsole.Log("unLocked",lockedTick);
                    this.Unlock();
                }
                this.lockedTick++;

            }
        }

        private void _checkToChangeState(AgentState ms)
        {
            if (this.state != null)
            {
                //QuixConsole.Log("Check change");
                if (ms == this.state)
                {
                    this.state.OnRepeat();
                }
                else
                {
                    if (this.state != null)
                    {
                        this.state.OnDesactivate();
                    }

                    this.state = ms;
                    QuixConsole.Log("State",ms.GetType().ToString());
                    this.state.OnActivate();
                }
            }else{
                this.state = ms;
            }
        }

        public void ChangeState(AgentState state)
        {
        
            if (!this.isLocked)
            {
                //QuixConsole.Log("Changing state to",state.GetType().ToString());
                this._checkToChangeState(state);
            }

        }


        public AgentState ActualState()
        {
            return this.state;
        }
    }
    public interface AgentState
    {
        void Tick();
        void OnDesactivate();
        void OnRepeat();
        void OnActivate();
    }
}