using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Ball_Breaker
{
    class Game
    {
        private const int FieldWidth = 12;
        private const int FieldHeight = 12;

        private readonly int cellSize;
        private readonly Font font = new Font("Arial", 10);

        private CellState[,] fieldCells;
        private List<CellState> selectedArea;
        private int selectedAreaPointsCount;

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
            selectedAreaPointsCount = 0;

            for (int x = 0; x < FieldWidth; x++)
                for (int y = 0; y < FieldHeight; y++)
                    fieldCells[x, y] = new CellState();
        }

        public void SelectArea(int positionX, int positionY)
        {
            if (fieldCells[positionX, positionY].HasBall)
            {
                if (selectedArea.Contains(fieldCells[positionX, positionY]) && selectedArea.Count > 1)
                {
                    foreach (var cell in fieldCells)
                    {
                        if (selectedArea.Contains(cell))
                            cell.HasBall = false;
                    }
                    ShiftFieldCells();

                    selectedArea.Clear();
                    selectedAreaPointsCount = 0;
                }

                if (!selectedArea.Contains(fieldCells[positionX, positionY]))
                {
                    selectedAreaPointsCount = 0;
                    selectedArea.Clear();

                    selectedArea.Add(fieldCells[positionX, positionY]);
                    AddCellSelectArea(positionX, positionY);
                }
            }
            else
            {
                selectedArea.Clear();
                selectedAreaPointsCount = 0;
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
                    {
                        selectedArea.Add(fieldCells[x + i, y + j]);
                        AddCellSelectArea(x + i, y + j);

                        selectedAreaPointsCount = selectedArea.Count * (selectedArea.Count - 1);
                    }
                }
        }

        private void ShiftFieldCells()
        {
            for(int j = 0; j < fieldCells.GetLength(1); j++)
            {
                int cellWithOutBallPosition = GetCellWithOutBallPositionInRow(j);
                if (cellWithOutBallPosition >= 0)
                    for (int i = fieldCells.GetLength(0) - 1; i > 0;)
                    {
                        if (fieldCells[i, j].HasBall)
                        {
                            i--;
                        }
                        else
                        {
                            (fieldCells[i, j], fieldCells[cellWithOutBallPosition, j]) = (
                                fieldCells[cellWithOutBallPosition, j], fieldCells[i, j]);

                            cellWithOutBallPosition--;
                            if (cellWithOutBallPosition < 0)
                                i = 0;
                        }
                    }
            }

            for (int i = 0; i < fieldCells.GetLength(0); i++)
            {
                int cellWithOutBallPosition = GetCellWithOutBallPositionInColumn(i);
                if (cellWithOutBallPosition >= 0)
                    for (int j = fieldCells.GetLength(1) - 1; j > 0;)
                    {
                        if (fieldCells[i, j].HasBall)
                        {
                            j--;
                        }
                        else
                        {
                            (fieldCells[i, j], fieldCells[i, cellWithOutBallPosition]) = (
                                fieldCells[i, cellWithOutBallPosition], fieldCells[i, j]);

                            cellWithOutBallPosition--;
                            if (cellWithOutBallPosition < 0)
                                j = 0;
                        }
                    }
            }
        }

        private int GetCellWithOutBallPositionInRow(int rowNumber)
        {
            int position = -1;
            for (int i = 0; i < fieldCells.GetLength(0); i++)
            {
                if (!fieldCells[i, rowNumber].HasBall)
                    position = i;
            }

            return position;
        }

        private int GetCellWithOutBallPositionInColumn(int columnNumber)
        {
            int position = -1;
            for (int j = 0; j < fieldCells.GetLength(0); j++)
            {
                if (!fieldCells[columnNumber, j].HasBall)
                    position = j;
            }

            return position;
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

        //private void DrawSelectedArea(Graphics graphics)
        //{
        //    if (selectedArea.Count > 1)
        //        foreach (var cell in selectedArea)
        //        {
        //            if (cell.hasBall)
        //                graphics.FillRectangle(Brushes.LightCoral, cell.X * cellSize, cell.Y * cellSize, cellSize,
        //                    cellSize);
        //        }
        //}

        //private void DrawPointsCount(Graphics graphics)
        //{
        //    if (selectedAreaPointsCount > 0)
        //    {
        //        var pointsCountPosition = selectedArea.OrderBy(cell => cell.Y).ThenBy(cell => cell.X).First();

        //        pointsCountPosition.X = pointsCountPosition.X * cellSize - 15 >= 0
        //            ? pointsCountPosition.X * cellSize - 15
        //            : pointsCountPosition.X * cellSize;
        //        pointsCountPosition.Y = pointsCountPosition.Y * cellSize - 15 >= 0
        //            ? pointsCountPosition.Y * cellSize - 15
        //            : pointsCountPosition.Y * cellSize;

        //        graphics.FillEllipse(Brushes.LightGreen, pointsCountPosition.X, pointsCountPosition.Y, 30, 25);
        //        graphics.DrawString(selectedAreaPointsCount.ToString(), font, Brushes.Black,
        //            pointsCountPosition.X + 5, pointsCountPosition.Y + 5);
        //    }
        //}
    }
}
