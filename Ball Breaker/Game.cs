using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Ball_Breaker
{
    internal class Game
    {
        private const int FieldWidth = 12;
        private const int FieldHeight = 12;

        private readonly Font pointCountFont = new Font("Arial", 10);
        private readonly Font scoreFont = new Font("Arial", 15);
        private readonly Pen selectedAreaPen = new Pen(Color.Black, 3);
        private readonly int cellSize;

        private GamePhase gamePhase;
        private ShiftDirection shiftDirection;
        private CellState[,] gameField;
        private CellState[,] gameFieldsOnLastTurn;
        private List<CellState> selectedArea;
        private int selectedAreaPointsCount;
        private int delayOfShift;
        private int score;
        private bool canUndoLastTurn;

        public event Action Defeat = delegate { };

        public Game(int cellSize)
        {
            this.cellSize = cellSize;
        }

        public void StartNewGame()
        {
            gameField = new CellState[FieldWidth, FieldHeight];
            gameFieldsOnLastTurn = new CellState[FieldWidth, FieldHeight];
            selectedArea = new List<CellState>();

            gamePhase = GamePhase.StartGame;
            selectedAreaPointsCount = 0;
            delayOfShift = 0;
            score = 0;
            canUndoLastTurn = true;

            for (int x = 0; x < FieldWidth; x++)
                for (int y = 0; y < FieldHeight; y++)
                    gameField[x, y] = new CellState();

            CalculateSimilarBallsAroundCell();
        }

        public void ChooseCell(int positionX, int positionY)
        {
            if (positionX < 0 || positionX >= FieldWidth ||
                positionY < 0 || positionY >= FieldHeight ||
                gamePhase == GamePhase.ShiftDownFieldCell ||
                gamePhase == GamePhase.ShiftRightFieldCells)
                return;

            if (gameField[positionX, positionY].HasBall)
            {
                if (selectedArea.Contains(gameField[positionX, positionY]) && selectedArea.Count > 1)
                {
                    DeleteSelectedArea();
                    gamePhase = GamePhase.ShiftDownFieldCell;
                }

                else if (gamePhase != GamePhase.ShiftDownFieldCell)
                {
                    selectedAreaPointsCount = 0;
                    selectedArea.Clear();

                    selectedArea.Add(gameField[positionX, positionY]);
                    AddCellToSelectArea(positionX, positionY);

                    gamePhase = GamePhase.ConfirmSelectedArea;
                }
            }
            else
            {
                selectedArea.Clear();
                gamePhase = GamePhase.ChooseSelectedArea;
            }
        }

        private void DeleteSelectedArea()
        {
            CopyArray(gameField, gameFieldsOnLastTurn);

            foreach (var cell in gameField)
                if (selectedArea.Contains(cell))
                    cell.HasBall = false;

            selectedArea.Clear();
            score += selectedAreaPointsCount;

            ShiftGameField();
        }

        private void AddCellToSelectArea(int x, int y)
        {
            for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++)
                {
                    if (x + i < 0 || x + i >= FieldWidth || y + j < 0 || y + j >= FieldHeight ||
                        Math.Abs(i) == Math.Abs(j) || selectedArea.Contains(gameField[x + i, y + j]))
                        continue;

                    if (gameField[x, y].BallColor == gameField[x + i, y + j].BallColor &&
                        gameField[x + i, y + j].HasBall)
                    {
                        selectedArea.Add(gameField[x + i, y + j]);
                        AddCellToSelectArea(x + i, y + j);

                        selectedAreaPointsCount = selectedArea.Count * (selectedArea.Count - 1);
                    }
                }
        }

        public void Update()
        {
            switch (gamePhase)
            {
                case GamePhase.ShiftDownFieldCell:
                    if (delayOfShift >= 1)
                    {
                        gamePhase = GamePhase.ShiftRightFieldCells;
                        shiftDirection = ShiftDirection.Right;
                        delayOfShift = 0;
                    }
                    else
                        delayOfShift++;

                    break;
                case GamePhase.ShiftRightFieldCells:
                    ShiftGameField();
                    gamePhase = GamePhase.ChooseSelectedArea;
                    shiftDirection = ShiftDirection.Down;
                    canUndoLastTurn = true;
                    break;
            }
        }

        private void ShiftGameField()
        {
            switch (shiftDirection)
            {
                case ShiftDirection.Down:
                    ShiftDownGameField();
                    break;
                case ShiftDirection.Right:
                    ShiftRightGameField();
                    CalculateSimilarBallsAroundCell();
                    break;
            }

            while (IsFirstColumnEmpty())
            {
                AddBallToFirstColumn();
                ShiftRightGameField();
                CalculateSimilarBallsAroundCell();
            }

            CheckDefeat();
        }

        private void ShiftDownGameField()
        {
            for (int x = 0; x < FieldWidth; x++)
            {
                int cellWithOutBallPosition = GetCellWithOutBallPosition(x);

                if (cellWithOutBallPosition < 0)
                    continue;

                for (int y = FieldHeight - 1; y > 0;)
                {
                    if (gameField[x, y].HasBall)
                    {
                        y--;
                    }
                    else
                    {
                        (gameField[x, y], gameField[x, cellWithOutBallPosition]) = (
                            gameField[x, cellWithOutBallPosition], gameField[x, y]);

                        cellWithOutBallPosition--;
                        if (cellWithOutBallPosition < 0)
                            y = 0;
                    }
                }
            }
        }

        private void ShiftRightGameField()
        {
            for (int y = 0; y < gameField.GetLength(1); y++)
            {
                int cellWithOutBallPosition = GetCellWithOutBallPosition(y);

                if (cellWithOutBallPosition < 0)
                    continue;

                for (int x = FieldWidth - 1; x > 0;)
                {
                    if (gameField[x, y].HasBall)
                    {
                        x--;
                    }
                    else
                    {
                        (gameField[x, y], gameField[cellWithOutBallPosition, y]) = (
                            gameField[cellWithOutBallPosition, y], gameField[x, y]);

                        cellWithOutBallPosition--;
                        if (cellWithOutBallPosition < 0)
                            x = 0;
                    }
                }
            }
        }

        private int GetCellWithOutBallPosition(int numberPartOfArray)
        {
            int position = -1;

            switch (shiftDirection)
            {
                case ShiftDirection.Down:

                    for (int y = 0; y < FieldWidth; y++)
                        if (!gameField[numberPartOfArray, y].HasBall)
                            position = y;

                    break;
                case ShiftDirection.Right:

                    for (int x = 0; x < FieldHeight; x++)
                        if (!gameField[x, numberPartOfArray].HasBall)
                            position = x;

                    break;
            }

            return position;
        }

        private bool IsFirstColumnEmpty()
        {
            int cellWithBallsCount = 0;

            for (int y = 0; y < FieldHeight; y++)
                if (gameField[0, y].HasBall)
                    cellWithBallsCount++;

            return cellWithBallsCount == 0;
        }

        private void CalculateSimilarBallsAroundCell()
        {
            for (int x = 0; x < FieldWidth; x++)
                for (int y = 0; y < FieldHeight; y++)
                {
                    gameField[x, y].SimilarBallList.Clear();

                    if (!gameField[x, y].HasBall)
                        continue;

                    if (x - 1 >= 0 && gameField[x - 1, y].HasBall
                                   && gameField[x, y].BallColor == gameField[x - 1, y].BallColor)
                        gameField[x, y].SimilarBallList.Add(Direction.Left);

                    if (y - 1 >= 0 && gameField[x, y - 1].HasBall
                                   && gameField[x, y].BallColor == gameField[x, y - 1].BallColor)
                        gameField[x, y].SimilarBallList.Add(Direction.Top);

                    if (x + 1 < FieldWidth && gameField[x + 1, y].HasBall
                                           && gameField[x, y].BallColor == gameField[x + 1, y].BallColor)
                        gameField[x, y].SimilarBallList.Add(Direction.Right);

                    if (y + 1 < FieldHeight && gameField[x, y + 1].HasBall
                                            && gameField[x, y].BallColor == gameField[x, y + 1].BallColor)
                        gameField[x, y].SimilarBallList.Add(Direction.Bottom);
                }
        }

        private void CheckDefeat()
        {
            bool isPotentialSelectedArea =
                gameField.Cast<CellState>().Count(cell => cell.SimilarBallList.Count > 0) > 0;

            if (!isPotentialSelectedArea)
                Defeat();
        }

        private void AddBallToFirstColumn()
        {
            int ballsCount = CellState.Random.Next(1, FieldHeight);

            for (int y = ballsCount; y >= 0; y--)
                gameField[0, FieldHeight - y - 1] = new CellState();

            gamePhase = GamePhase.ShiftDownFieldCell;
        }

        public void UndoDeleteSelectedArea()
        {
            if (gamePhase == GamePhase.StartGame || gamePhase == GamePhase.ConfirmSelectedArea)
                return;

            CopyArray(gameFieldsOnLastTurn, gameField);
            CalculateSimilarBallsAroundCell();
            canUndoLastTurn = false;
        }

        public bool CanUndoLastTurn()
        {
            return canUndoLastTurn;
        }

        private void CopyArray(CellState[,] sourceArray, CellState[,] destinationArray)
        {
            for (int x = 0; x < FieldWidth; x++)
                for (int y = 0; y < FieldHeight; y++)
                    destinationArray[x, y] = new CellState(sourceArray[x, y].BallColor, sourceArray[x, y].HasBall);
        }

        public void Draw(Graphics graphics)
        {
            DrawField(graphics);
            DrawFieldCells(graphics);
            DrawSelectedArea(graphics);
            DrawPointsCount(graphics);
            DrawScore(graphics);
        }

        private void DrawField(Graphics graphics)
        {
            for (int x = 0; x <= FieldWidth; x++)
                for (int y = 0; y <= FieldHeight; y++)
                {
                    graphics.DrawLine(Pens.LightGray, x * cellSize, y * cellSize,
                        FieldWidth - x * cellSize, y * cellSize);
                    graphics.DrawLine(Pens.LightGray, x * cellSize, y * cellSize,
                        x * cellSize, FieldHeight - y * cellSize);
                }
        }

        private void DrawFieldCells(Graphics graphics)
        {
            for (int x = 0; x < FieldWidth; x++)
                for (int y = 0; y < FieldHeight; y++)
                    gameField[x, y].Draw(graphics, x, y, cellSize);
        }

        private void DrawSelectedArea(Graphics graphics)
        {
            if (selectedArea.Count <= 1)
                return;

            for (int x = 0; x < FieldWidth; x++)
                for (int y = 0; y < FieldHeight; y++)
                    if (selectedArea.Contains(gameField[x, y]) && gameField[x, y].HasBall)
                    {
                        if (!gameField[x, y].SimilarBallList.Contains(Direction.Left))
                            graphics.DrawLine(selectedAreaPen, x * cellSize, y * cellSize,
                                x * cellSize, (y + 1) * cellSize);

                        if (!gameField[x, y].SimilarBallList.Contains(Direction.Top))
                            graphics.DrawLine(selectedAreaPen, x * cellSize, y * cellSize
                                , (x + 1) * cellSize, y * cellSize);

                        if (!gameField[x, y].SimilarBallList.Contains(Direction.Right))
                            graphics.DrawLine(selectedAreaPen, (x + 1) * cellSize, y * cellSize,
                                (x + 1) * cellSize, (y + 1) * cellSize);

                        if (!gameField[x, y].SimilarBallList.Contains(Direction.Bottom))
                            graphics.DrawLine(selectedAreaPen, x * cellSize, (y + 1) * cellSize,
                                (x + 1) * cellSize, (y + 1) * cellSize);
                    }

        }

        private void DrawPointsCount(Graphics graphics)
        {
            int positionX = 0;
            int positionY = 0;

            for (int y = 0; y < FieldHeight; y++)
                for (int x = 0; x < FieldWidth; x++)
                {
                    if (!selectedArea.Contains(gameField[x, y]) || !gameField[x, y].HasBall)
                        continue;

                    positionX = x * cellSize;
                    positionY = y * cellSize;

                    x = FieldWidth;
                    y = FieldHeight;
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
                graphics.DrawString(selectedAreaPointsCount.ToString(), pointCountFont, Brushes.Black,
                    positionX + 5, positionY + 5);
            }
        }

        private void DrawScore(Graphics graphics)
        {
            graphics.DrawString("Score: " + score, scoreFont, Brushes.Black, 0, 440);
        }
    }
}
