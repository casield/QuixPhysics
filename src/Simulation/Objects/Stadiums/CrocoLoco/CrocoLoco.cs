using System;
using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;

namespace QuixPhysics
{
    public class CrocoLoco : PhyObject
    {

        Vehicle vehicle;
        Vector3 target = new Vector3(0, 0, 0);
        private User player;

        public CrocoLoco()
        {
            QuixConsole.WriteLine("Crocoloco");

        }
        public override void Load(BodyHandle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state)
        {
            base.Load(bodyHandle, connectionState, simulator, state);
            vehicle = new Vehicle(this);

            PhyInterval worker = new PhyInterval(1, simulator);
            //worker.Completed += Tick;

        }

        private void Tick()
        {
            FindPlayer();
            if (player != null)
            {
                target = player.player.reference.Pose.Position;
            }
            vehicle.Seek(target);
            vehicle.Update();
        }

        private void FindPlayer()
        {
            if (simulator != null && player == null)
            {
                if (simulator.users.Count > 0)
                {

                    List<User> arr = new List<User>(simulator.users.Values);
                    player = arr[0];

                }
            }
        }
    }
}