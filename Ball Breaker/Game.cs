using System;
using System.Collections.Generic;
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
        private List<CellState> selectedArea;
        private Color selectedAreaColor;

        public event Action Defeat = delegate { };
        public event Action Victory = delegate { };

        public Game(int cellSize)
        {
            this.cellSize = cellSize;
        }

        public void StartGame()
        {
            selectedArea = new List<CellState>();
            fieldCells = new CellState[FieldWidth, FieldHeight];

            for (int x = 0; x < FieldWidth; x++)
                for (int y = 0; y < FieldHeight; y++)
                    fieldCells[x, y] = new CellState(x, y);
        }

        public void SelectArea(int positionX, int positionY)
        {
            if (fieldCells[positionX, positionY].BallColor != selectedAreaColor)
                selectedArea.Clear();

            if (!selectedArea.Contains(fieldCells[positionX, positionY]))
            {
                selectedArea.Add(fieldCells[positionX, positionY]);
                selectedAreaColor = fieldCells[positionX, positionY].BallColor;
                AddCellSelectArea(positionX, positionY);
            }
            else
            {
                if (selectedArea.Count >= 2)
                    foreach (var cell in selectedArea)
                    {
                        fieldCells[cell.X, cell.Y].hasBall = false;
                    }

                selectedArea.Clear();
            }
        }

        private void AddCellSelectArea(int x, int y)
        {
            for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++)
                {
                    if (x + i < 0 || x + i >= FieldWidth || y + j < 0 || y + j >= FieldHeight ||
                        Math.Abs(i) == Math.Abs(j) || selectedArea.Contains(fieldCells[x + i, y + j])) continue;

                    if (fieldCells[x, y].BallColor == fieldCells[x + i, y + j].BallColor)
                        SelectArea(x + i, y + j);
                }
        }

        public void Draw(Graphics graphics)
        {
            DrawField(graphics);
            DrawSelectedArea(graphics);
            DrawFieldCells(graphics);
        }

        private void DrawField(Graphics graphics)
        {
            for (int x = 0; x <= FieldWidth; x++)
                for (int y = 0; y <= FieldHeight; y++)
                {
                    graphics.DrawLine(Pens.LightGray, x * cellSize, y * cellSize, FieldWidth - x * cellSize,
                        y * cellSize);
                    graphics.DrawLine(Pens.LightGray, x * cellSize, y * cellSize, x * cellSize,
                        FieldHeight - y * cellSize);
                }
        }

        private void DrawFieldCells(Graphics graphics)
        {
            for (int x = 0; x < FieldWidth; x++)
                for (int y = 0; y < FieldHeight; y++)
                    fieldCells[x, y].Draw(graphics, x, y, cellSize);
        }

        private void DrawSelectedArea(Graphics graphics)
        {
            if (selectedArea.Count >= 2)
                foreach (var cell in selectedArea)
                {
                    if (cell.hasBall)
                        graphics.FillRectangle(Brushes.LightCoral, cell.X * cellSize, cell.Y * cellSize, cellSize,
                            cellSize);
                }
        }
    }
}
