using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

using QuixPhysics.Hextiles;

namespace QuixPhysics
{
    public class HextilesAddon : ArenaAddon
    {

        Hexgrid hexgrid;
        List<Hexagon> addedHexagons = new List<Hexagon>();
        Random rnd = new Random();
        public bool IsOdd(float value)
        {
            return value % 2 != 0;
        }

        public HextilesAddon(Simulator simulator, Arena arena) : base(simulator, arena)
        {
            hexgrid = new Hexgrid(Hexagon._SIZE);

            QuixConsole.Log("Creating Hextile");

            var random = true;

            for (int x = 0; x < Hexgrid._GRID_SIZE; x++)
            {
                for (int y = 0; y < Hexgrid._GRID_SIZE; y++)
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

                    Hexagon ob = (Hexagon)room.factory.Create(Hexagon.Build(pos), room);
                    ob.AddHextile(hextile);
                    arena.navObjects.Add(ob);

                    addedHexagons.Add(ob);

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

        public Hextile GetRandomHextile()
        {
            return hexgrid.GetRandomHextile();
        }

        public override void OnStart()
        {
            for (int i = 0; i < addedHexagons.Count; i++)
            {
                Hexagon hexagon = addedHexagons[i];
                Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                hexagon.ChangeColor(HexConverter(randomColor));
            }
            // throw new NotImplementedException();
        }
        private static String HexConverter(System.Drawing.Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }
    }
}