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

            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {

                    int month = rnd.Next(1, 13);

                    if (month < 12)
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
                    var gridpos = elem.getXY();
                    bool isOddX = elem.IsOdd((int)gridpos.X);
                    bool isOddY = elem.IsOdd((int)gridpos.Y);

                    QuixConsole.Log("Is odd", isOddX);
                    Hexagon ob = (Hexagon)room.factory.Create(Hexagon.Build(pos, isOddY), room);
                    ob.hextile = elem;
                    arena.navObjects.Add(ob);

                    var wallspos = new List<int>();
                    /*****/

                    for (int a = 0; a < 6; a++)
                    {
                        var posElem = elem.GetSide(a);
                        var tt = hexgrid.GetHextile((int)(gridpos.X + posElem.X), (int)(gridpos.Y + posElem.Y));
                        if (tt == null)
                        {
                            int num = a;
                           /* if (a == 1 && !isOddX)
                            {
                               num = 0;
                            }
                            
                            if (a == 4 && isOddX)
                            {
                                num = 3;

                            }*/

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