using System;
using System.Collections.Generic;
using System.Drawing;

namespace Ball_Breaker
{
    internal class CellState
    {
        private readonly Brush brush;

        public static readonly Random Random = new Random();

        public readonly Color BallColor;
        public readonly List<Direction> DifferentColorAdjacentBallDirections;

        public bool HasBall { get; set; }

        public CellState()
        {
            BallColor = GetRandomColor();
            brush = new SolidBrush(BallColor);
            DifferentColorAdjacentBallDirections = new List<Direction>();
            HasBall = true;
        }

        public CellState(Color ballColor, bool hasBall)
        {
            BallColor = ballColor;
            brush = new SolidBrush(BallColor);
            DifferentColorAdjacentBallDirections = new List<Direction>();
            HasBall = hasBall;
        }

        private Color GetRandomColor()
        {
            switch (Random.Next(0, 5))
            {
                case 0: return Color.Green;
                case 1: return Color.Blue;
                case 2: return Color.Red;
                case 3: return Color.Khaki;
                case 4: return Color.BlueViolet;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public void Draw(Graphics graphics, int cellX, int cellY, int cellSize)
        {
            int positionX = cellX * cellSize;
            int positionY = cellY * cellSize;
            int offsetToCenter = cellSize / 7;

            if (!HasBall)
                return;

            graphics.FillEllipse(brush, positionX + offsetToCenter, positionY + offsetToCenter, 27, 27);
        }
    }
}
