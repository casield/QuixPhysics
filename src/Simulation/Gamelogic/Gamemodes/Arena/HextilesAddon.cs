using System;
using System.Collections.Generic;
using System.Numerics;

using QuixPhysics.Hextiles;

namespace QuixPhysics
{
    public class HextilesAddon : ArenaAddon
    {

        Hexgrid hexgrid;

        public HextilesAddon(Simulator simulator, Arena arena) : base(simulator, arena)
        {
            hexgrid = new Hexgrid(Hexagon._SIZE);
            Random rnd = new Random();
            QuixConsole.Log("Creating Hextile");

            for (int x = 0; x < Hexgrid._GRID_SIZE; x++)
            {
                for (int y = 0; y < Hexgrid._GRID_SIZE; y++)
                {

                    int month = rnd.Next(1, 13);

                    if (month < 10)
                    {
                        hexgrid.AddHextile(x, y);
                    }

                }
            }

            foreach (var elem in hexgrid.hextiles)
            {
                if (elem != null)
                {
                    Vector3 pos = elem.GetPosition();
                    Hexagon ob = (Hexagon)room.factory.Create(Hexagon.Build(pos), room);
                    ob.hextile = elem;
                    arena.navObjects.Add(ob);
                    /* ob.AddWorker(new PhyTimeOut(100,simulator,true)).Completed+=()=>{
                         ob.AddWalls(new int[]{1,2,3,4,5,0});
                     };*/

                    var walls = ob.AddWalls(new int[] { 1, 2, 3, 4, 5, 0 });

                    arena.navObjects.AddRange(walls);
                    // ob.AddWalls(new int[]{1,2});
                }
            }

        }

        public Vector3 GetRandomHextile()
        {
            Hextile tile = null;
            foreach (var item in hexgrid.hextiles)
            {
                if (item != null)
                {
                    tile = item;
                    break;
                }
            }
            QuixConsole.Log("Tile", tile.getXY());
            return tile.GetPosition() + new Vector3(0, Hexagon._SIZE, 0);
        }

        public override void OnStart()
        {
            // throw new NotImplementedException();
        }
    }
}