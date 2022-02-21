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
        public Layout layout;
        int offset = OffsetCoord.EVEN;
        public bool IsOdd(float value)
        {
            return value % 2 != 0;
        }

        public HextilesAddon(Simulator simulator, Arena arena) : base(simulator, arena)
        {
            layout = new Layout(Layout.pointy, new Point(250, 250), new Point(0, 0));

            QuixConsole.Log("Creating Hextile");

            var random = true;
            var size = 6;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    if (!random || rnd.Next(10) < 8)
                    {
                        var off = OffsetCoord.RoffsetToCube(offset, new OffsetCoord(x, y));
                        var posPoint = layout.HexToPixel(off);
                        var pos = new Vector3((float)posPoint.x, 100, (float)posPoint.y);
                        Hexagon ob = (Hexagon)room.factory.Create(Hexagon.Build(pos), room);
                        // ob.AddHextile(hextile);
                        ob.AddHex(off);
                        arena.navObjects.Add(ob);

                        addedHexagons.Add(ob);
                    }


                }
            }

            foreach (var hex in addedHexagons)
            {
                List<int> wallpos = new List<int>();
                for (int a = 0; a < 6; a++)
                {
                    int num = a;
                    var neighbor = hex.hex.Neighbor(a);
                    var find = addedHexagons.Find(e =>
                    {
                        var offE = OffsetCoord.QoffsetFromCube(offset, e.hex);
                        var offNei = OffsetCoord.QoffsetFromCube(offset, neighbor);

                        return (offE.col == offNei.col && offE.row == offNei.row);
                    });

                    if (find == null)
                    {
                        wallpos.Add(num);
                    }
                }
                var walls = hex.AddWalls(wallpos.ToArray(), this);

                arena.navObjects.AddRange(walls);
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

        public Vector3 GetPositionFromHex(Hex hex)
        {
            var pos = layout.HexToPixel(hex);

            return new Vector3((float)pos.x, 800, (float)pos.y);
        }

        private static String HexConverter(System.Drawing.Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }
    }
}