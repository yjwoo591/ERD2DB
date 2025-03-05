using System;

namespace ERD2DB.CsBase.CsBase1CB
{
    public class GeometryBase1CB
    {
        public struct Point1CB
        {
            public int X { get; set; }
            public int Y { get; set; }

            public Point1CB(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        public struct Size1CB
        {
            public int Width { get; set; }
            public int Height { get; set; }

            public Size1CB(int width, int height)
            {
                Width = width;
                Height = height;
            }
        }

        public struct Rectangle1CB
        {
            public Point1CB Location { get; set; }
            public Size1CB Size { get; set; }

            public Rectangle1CB(Point1CB location, Size1CB size)
            {
                Location = location;
                Size = size;
            }

            public int X => Location.X;
            public int Y => Location.Y;
            public int Width => Size.Width;
            public int Height => Size.Height;
            public int Left => Location.X;
            public int Top => Location.Y;
            public int Right => Location.X + Size.Width;
            public int Bottom => Location.Y + Size.Height;
        }
    }
}