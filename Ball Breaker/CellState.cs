using System;
using System.Collections.Generic;
using System.Drawing;

namespace Ball_Breaker
{
    internal class CellState
    {
        private readonly Brush brush;
        private double ballWidth;

        public static readonly Random Random = new Random();

        public readonly Color BallColor;
        public readonly List<Direction> DifferentColorAdjacentBallDirections;

        public bool HasBall { get; set; }

        public CellState(int cellSize)
        {
            BallColor = GetRandomColor();
            brush = new SolidBrush(BallColor);
            DifferentColorAdjacentBallDirections = new List<Direction>();
            ballWidth = cellSize * 0.8;
            HasBall = true;
        }

        public CellState(Color ballColor, int cellSize, bool hasBall)
        {
            BallColor = ballColor;
            brush = new SolidBrush(BallColor);
            DifferentColorAdjacentBallDirections = new List<Direction>();
            ballWidth = cellSize * 0.8;
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
            if (!HasBall)
                return;

            int positionX = cellX * cellSize;
            int positionY = cellY * cellSize;
            int offsetToCenter = cellSize / 12;

            graphics.FillEllipse(brush, positionX + offsetToCenter, positionY + offsetToCenter, (float)ballWidth, (float)ballWidth);
        }
    }
}
