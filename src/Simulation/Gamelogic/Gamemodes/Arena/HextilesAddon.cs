using System;
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
                        hexgrid.AddHextile(x,y);
                    }

                }
            }

            foreach (var elem in hexgrid.hextiles)
            {
                if (elem != null)
                {
                    Vector3 pos = elem.GetPosition();
                   PhyObject ob = room.factory.Create(Hexagon.Build(pos), room);
                   arena.navObjects.Add(ob);
                }
            }

        }

        public Vector3 GetRandomHextile(){
            Hextile tile = null;
            foreach (var item in hexgrid.hextiles)
            {
                if(item!=null){
                    tile = item;
                    break;
                }
            }
            QuixConsole.Log("Tile",tile.getXY());
            return tile.GetPosition();
        }

        public override void OnStart()
        {
           // throw new NotImplementedException();
        }
    }
}