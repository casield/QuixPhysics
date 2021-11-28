using System;
using System.Collections.Generic;
using System.Numerics;

using QuixPhysics.Hextiles;

namespace QuixPhysics
{
    public class HextilesAddon : ArenaAddon
    {

        Hexgrid hexgrid;
        public bool IsOdd(float value)
        {
            return value % 2 != 0;
        }

        public HextilesAddon(Simulator simulator, Arena arena) : base(simulator, arena)
        {
            hexgrid = new Hexgrid(Hexagon._SIZE);
            Random rnd = new Random();
            QuixConsole.Log("Creating Hextile");

            var random = true;

            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {

                    int month = rnd.Next(1, 13);

                    if (month < 10 || !random)
                    {

                        hexgrid.AddHextile(x, y);


                    }

                }
            }

            foreach (var hextile in hexgrid.hextiles)
            {
                if (hextile != null)
                {
                    Vector3 pos = hextile.GetPosition();
                    var gridpos = hextile.getXY();
                    bool isOddX = hextile.IsOdd((int)gridpos.X);
                    bool isOddY = hextile.IsOdd((int)gridpos.Y);

                    Hexagon ob = (Hexagon)room.factory.Create(Hexagon.Build(pos, isOddY), room);
                    ob.AddHextile(hextile);
                    arena.navObjects.Add(ob);

                    var wallspos = new List<int>();
                    /*****/

                    for (int a = 0; a < 6; a++)
                    {
                        int num = a;
                        var posElem = hextile.GetSide(num);
                        var tt = hexgrid.GetHextile((int)(gridpos.X + posElem.X), (int)(gridpos.Y + posElem.Y));

                        if (tt == null)
                        {

                            wallspos.Add(num);
                        }
                    }




                    var walls = ob.AddWalls(wallspos.ToArray());

                    arena.navObjects.AddRange(walls);
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