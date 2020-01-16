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
        private readonly Font font = new Font("Arial", 10);

        private GamePhase gamePhase;
        private CellState[,] fieldCells;
        private List<CellState> selectedArea;
        private int selectedAreaPointsCount;
        private int delayOfShift;

        public event Action Defeat = delegate { };
        public event Action Victory = delegate { };

        public Game(int cellSize)
        {
            this.cellSize = cellSize;
        }

        public void NewGame()
        {
            selectedArea = new List<CellState>();
            fieldCells = new CellState[FieldWidth, FieldHeight];
            selectedAreaPointsCount = 0;
            delayOfShift = 0;

            for (int x = 0; x < FieldWidth; x++)
                for (int y = 0; y < FieldHeight; y++)
                    fieldCells[x, y] = new CellState();
        }

        public void Update()
        {
            switch (gamePhase)
            {
                case GamePhase.ShiftDownFieldCell:
                    if (delayOfShift < 15)
                    {
                        delayOfShift++;
                        ShiftDownFieldCells();
                    }
                    else
                    {
                        gamePhase = GamePhase.ShiftRightFieldCells;
                        delayOfShift = 0;
                    }
                    break;
                case GamePhase.ShiftRightFieldCells:
                    ShiftRightFieldCellS();
                    gamePhase = GamePhase.ChooseSelectedArea;
                    break;
            }
        }

        private void ShiftDownFieldCells()
        {
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

        private void ShiftRightFieldCellS()
        {
            for (int j = 0; j < fieldCells.GetLength(1); j++)
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

        public void PressOnGameField(int positionX, int positionY)
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

                    selectedArea.Clear();
                    selectedAreaPointsCount = 0;

                    gamePhase = GamePhase.ShiftDownFieldCell;
                }

                if (!selectedArea.Contains(fieldCells[positionX, positionY]) && gamePhase != GamePhase.ShiftDownFieldCell)
                {
                    selectedAreaPointsCount = 0;
                    selectedArea.Clear();

                    selectedArea.Add(fieldCells[positionX, positionY]);
                    AddCellToSelectArea(positionX, positionY);

                    gamePhase = GamePhase.ConfirmSelectedArea;
                }
            }
            else
            {
                selectedArea.Clear();
                selectedAreaPointsCount = 0;

                gamePhase = GamePhase.ChooseSelectedArea;
            }
        }

        private void AddCellToSelectArea(int x, int y)
        {
            for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++)
                {
                    if (x + i < 0 || x + i >= FieldWidth || y + j < 0 || y + j >= FieldHeight ||
                        Math.Abs(i) == Math.Abs(j) || selectedArea.Contains(fieldCells[x + i, y + j]))
                        continue;

                    if (fieldCells[x, y].BallColor == fieldCells[x + i, y + j].BallColor)
                    {
                        selectedArea.Add(fieldCells[x + i, y + j]);
                        AddCellToSelectArea(x + i, y + j);

                        selectedAreaPointsCount = selectedArea.Count * (selectedArea.Count - 1);
                    }
                }
        }

        public void Draw(Graphics graphics)
        {
            DrawField(graphics);
            DrawSelectedArea(graphics);
            DrawFieldCells(graphics);
            DrawPointsCount(graphics);
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
            if (selectedArea.Count > 1)
                for (int i = 0; i < fieldCells.GetLength(0); i++)
                {
                    for (int j = 0; j < fieldCells.GetLength(1); j++)
                    {
                        if(selectedArea.Contains(fieldCells[i,j]) && fieldCells[i,j].HasBall)
                            graphics.FillRectangle(Brushes.LightCoral, i * cellSize, j * cellSize, cellSize,
                                cellSize);
                    }
                }
        }

        private void DrawPointsCount(Graphics graphics)
        {
            int positionX = 0;
            int positionY = 0;

            for (int j = 0; j < fieldCells.GetLength(1); j++)

            {
                for (int i = 0; i < fieldCells.GetLength(0); i++)
                {
                    if (!selectedArea.Contains(fieldCells[i, j]) || !fieldCells[i, j].HasBall)
                        continue;

                    positionX = i * cellSize;
                    positionY = j * cellSize;

                    i = fieldCells.GetLength(0);
                    j = fieldCells.GetLength(1);

                }
            }

            positionX = positionX - 15 >= 0
                ? positionX - 15
                : positionX;
            positionY = positionY - 15 >= 0
                ? positionY - 15
                : positionY;

            if (selectedAreaPointsCount > 0 && gamePhase == GamePhase.ConfirmSelectedArea)
            {
                graphics.FillEllipse(Brushes.LightGreen, positionX, positionY, 30, 25);
                graphics.DrawString(selectedAreaPointsCount.ToString(), font, Brushes.Black,
                    positionX + 5, positionY + 5);
            }
        }
    }
}
