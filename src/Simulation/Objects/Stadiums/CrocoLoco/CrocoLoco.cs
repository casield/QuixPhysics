using System;
using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;
using FluentBehaviourTree;
namespace QuixPhysics
{
    public class CrocoLoco : PhyObject
    {

        public Vehicle vehicle;
        public PhyObject target;
        CrocoBehaviour behaviour;
        TimeData timeData;

        public CrocoLoco()
        {
            QuixConsole.WriteLine("Crocoloco");
            

        }
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state,Guid guid)
        {
            base.Load(bodyHandle, connectionState, simulator, state,guid);
            vehicle = new Vehicle(this);

            PhyInterval worker = new PhyInterval(1, simulator);
            worker.Completed += Tick;
            behaviour = new CrocoBehaviour(this);

        }

        private void Tick()
        {
            behaviour.Tick();
        }

        private void FindPlayer()
        {
        }
    }
}