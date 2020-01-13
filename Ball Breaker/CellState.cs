using System;
using System.Drawing;

namespace Ball_Breaker
{
    class CellState
    {
        private static readonly Random Random = new Random();

        private readonly Brush brush;

        public CellState()
        {
            brush = GetRandomBrush();
        }

        private Brush GetRandomBrush()
        {
            switch (Random.Next(0, 5))
            {
                case 0: return Brushes.Green;
                case 1: return Brushes.Blue;
                case 2: return Brushes.Red;
                case 3: return Brushes.Khaki;
                case 4: return Brushes.BlueViolet;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public void Draw(Graphics graphics, int cellX, int cellY, int cellSize)
        {
            int positionX = cellX * cellSize;
            int positionY = cellY * cellSize;
            int offsetToCenter = cellSize / 7;

            graphics.FillEllipse(brush, positionX + offsetToCenter, positionY + offsetToCenter, 27, 27);
        }
    }
}
