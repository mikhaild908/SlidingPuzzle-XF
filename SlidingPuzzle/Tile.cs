using System;
namespace SlidingPuzzle
{
    public class Tile
    {
        public int Id
        {
            get;
            set;
        }

        public int X
        {
            get;
            set;
        }

        public int Y
        {
            get;
            set;
        }

        public int XBeforeMoving
        {
            get;
            set;
        }

        public int YBeforeMoving
        {
            get;
            set;
        }
    }
}
