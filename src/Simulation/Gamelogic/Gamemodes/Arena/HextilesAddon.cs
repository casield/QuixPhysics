using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

using QuixPhysics.Hextiles;
namespace QuixPhysics
{
    public class HextilesAddon : ArenaAddon
    {

        List<Hexagon> addedHexagons = new List<Hexagon>();
        Random rnd = new Random();
        Layout layout;
        int offset = OffsetCoord.EVEN;
        public bool IsOdd(float value)
        {
            return value % 2 != 0;
        }

        public HextilesAddon(Simulator simulator, Arena arena) : base(simulator, arena)
        {
            layout = new Layout(Layout.pointy, new Point(250, 250), new Point(0, 0));

            QuixConsole.Log("Creating Hextile");

            var random = false;
            var size = 4;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var posPoint = layout.HexToPixel(OffsetCoord.RoffsetToCube(offset, new OffsetCoord(x, y)));
                    var pos = new Vector3((float)posPoint.x, 100, (float)posPoint.y);
                    Hexagon ob = (Hexagon)room.factory.Create(Hexagon.Build(pos), room);
                    // ob.AddHextile(hextile);
                    arena.navObjects.Add(ob);

                    addedHexagons.Add(ob);

                }
            }

            List<int> wallpos = new List<int>();

            foreach (var hex in addedHexagons)
            {
                for (int a = 0; a < 6; a++)
                {
                    int num = a;
                    var neighbor = addedHexagons.Find(e=>{
                        return true;
                    });

                    if (neighbor == null)
                    {
                        wallpos.Add(num);
                    }
                }
                var walls = hex.AddWalls(wallpos.ToArray());

                arena.navObjects.AddRange(walls);
            }

            for (int a = 0; a < 0; a++)
            {

                Vector3 pos = new Vector3(rnd.Next(0, 10), 500, rnd.Next(0, 10));


                Hexagon ob = (Hexagon)room.factory.Create(Hexagon.Build(pos), room);
                // ob.AddHextile(hextile);
                arena.navObjects.Add(ob);

                addedHexagons.Add(ob);

                var wallspos = new List<int>();
                /**

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

                arena.navObjects.AddRange(walls);***/
            }


        }

        public Hextile GetRandomHextile()
        {
            return null;
        }

        public override void OnStart()
        {
            for (int i = 0; i < addedHexagons.Count; i++)
            {
                Hexagon hexagon = addedHexagons[i];
                Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                hexagon.ChangeColor(HexConverter(randomColor));
                QuixConsole.Log("Change color", randomColor);
            }
            // throw new NotImplementedException();
        }
        public Vector3 GetHextilPosition(Vector3 position)
        {
            var hex = layout.PixelToHex(new Point(position.X, position.Z));
            var coord = OffsetCoord.QoffsetFromCube(offset, hex.HexRound());

            var newx = layout.HexToPixel(hex.HexRound());
            return new Vector3((float)newx.x, 800, (float)newx.y);
        }

        private static String HexConverter(System.Drawing.Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }
    }
}