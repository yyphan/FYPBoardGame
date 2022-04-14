using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoardGame
{
    public class TileNode
    {
        public int xCoord { get; set; }
        public int yCoord { get; set; }
        public int parentX { get; set; }
        public int parentY { get; set; }
        public int f { get; set; }
        public int g { get; set; }
        public int h { get; set; }

        public TileNode(int x, int y, int pX = -1, int pY = -1, int ff = Int32.MaxValue, int gg = Int32.MaxValue, int hh = Int32.MaxValue)
        {
            xCoord = x;
            yCoord = y;
            parentX = pX;
            parentY = pY;
            f = ff;
            g = gg;
            h = hh;
        }
    }
}
