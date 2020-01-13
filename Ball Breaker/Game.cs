using System;
using System.Drawing;

namespace Ball_Breaker
{
    class Game
    {
        private const int FieldWidth = 12;
        private const int FieldHeight = 12;

        private readonly int cellSize;
        private readonly Font font = new Font("Arial", 15);

        private CellState[,] fieldCells;

        public event Action Defeat = delegate { };
        public event Action Victory = delegate { };

        public Game(int cellSize)
        {
            this.cellSize = cellSize;
        }

        public void StartGame()
        {
            fieldCells = new CellState[FieldWidth, FieldHeight];

            for (int x = 0; x < FieldWidth; x++)
                for (int y = 0; y < FieldHeight; y++)
                    fieldCells[x, y] = new CellState();
        }

        public void Draw(Graphics graphics)
        {
            DrawField(graphics);
            DrawFieldCells(graphics);
        }

        private void DrawField(Graphics graphics)
        {
            for (int x = 0; x <= FieldWidth; x++)
                for (int y = 0; y <= FieldHeight; y++)
                {
                    graphics.DrawLine(Pens.LightGray, x * cellSize, y * cellSize, FieldWidth - x * cellSize, y * cellSize);
                    graphics.DrawLine(Pens.LightGray, x * cellSize, y * cellSize, x * cellSize, FieldHeight - y * cellSize);
                }
        }

        private void DrawFieldCells(Graphics graphics)
        {
            for (int x = 0; x < FieldWidth; x++)
                for (int y = 0; y < FieldHeight; y++)
                    fieldCells[x, y].Draw(graphics, x, y, cellSize);
        }
    }
}
