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

        private readonly Point[] directions = { new Point(0, -1), new Point(0, 1), new Point(-1, 0), new Point(1, 0) };
        private readonly Font pointCountFont = new Font("Arial", 10);
        private readonly Font scoreFont = new Font("Arial", 15);
        private readonly Pen selectedAreaPen = new Pen(Color.Black, 3);
        private readonly int cellSize;

        private GamePhase gamePhase;
        private CellState[,] gameField;
        private CellState[,] gameFieldOnPreviousTurn;
        private List<CellState> selectedArea;
        private int delayOfShift;
        private int score;
        private int deletedAreaCount;

        public bool CanUndoLastTurn { get; private set; }

        public event Action Defeat = delegate { };

        public Game(int cellSize)
        {
            this.cellSize = cellSize;
        }

        public void StartNewGame()
        {
            gameField = new CellState[FieldWidth, FieldHeight];
            gameFieldOnPreviousTurn = new CellState[FieldWidth, FieldHeight];
            selectedArea = new List<CellState>();

            delayOfShift = 0;
            score = 0;
            deletedAreaCount = 0;
            CanUndoLastTurn = false;

            for (int x = 0; x < FieldWidth; x++)
                for (int y = 0; y < FieldHeight; y++)
                    gameField[x, y] = new CellState(cellSize);

            CalculateDifferentBallsAroundCell();
        }

        public void ChooseCell(int positionX, int positionY)
        {
            if (positionX < 0 || positionX >= FieldWidth ||
                positionY < 0 || positionY >= FieldHeight)
                return;

            CellState choosenCell = gameField[positionX, positionY];

            if (gamePhase == GamePhase.WaitingSelectArea)
            {
                if(!choosenCell.HasBall)
                    return;

                selectedArea = GetSimilarColorArea(positionX, positionY);

                if (selectedArea.Count >= 2)
                    gamePhase = GamePhase.WaitingConfirmSelectedArea;

                return;
            }

            if (gamePhase == GamePhase.WaitingConfirmSelectedArea)
            {
                if (!choosenCell.HasBall
                    || !selectedArea.Contains(choosenCell))
                {
                    selectedArea.Clear();
                    gamePhase = GamePhase.WaitingSelectArea;
                    return;
                }

                gamePhase = GamePhase.ShiftDownFieldCells;
                DeleteSelectedArea();
            }
        }

        private void DeleteSelectedArea()
        {
            CopyArray(gameField, gameFieldOnPreviousTurn);

            foreach (var cell in selectedArea)
                cell.HasBall = false;

            deletedAreaCount = selectedArea.Count * (selectedArea.Count - 1);
            score += deletedAreaCount;

            selectedArea.Clear();
        }

        private List<CellState> GetSimilarColorArea(int positionX, int positionY)
        {
            return AddCellToSimilarColorArea(positionX, positionY, new List<CellState>());

            List<CellState> AddCellToSimilarColorArea(int x, int y, List<CellState> accumulatedSimilarColorArea)
            {
                accumulatedSimilarColorArea.Add(gameField[x, y]);

                foreach (Point direction in directions)
                {
                    if (x + direction.X < 0 || x + direction.X >= FieldWidth ||
                        y + direction.Y < 0 || y + direction.Y >= FieldHeight)
                        continue;


                    if (!gameField[x + direction.X, y + direction.Y].HasBall)
                        continue;

                    if (gameField[x, y].BallColor != gameField[x + direction.X, y + direction.Y].BallColor)
                        continue;

                    if (accumulatedSimilarColorArea.Contains(gameField[x + direction.X, y + direction.Y]))
                        continue;

                    AddCellToSimilarColorArea(x + direction.X, y + direction.Y, accumulatedSimilarColorArea);
                }

                return accumulatedSimilarColorArea;
            }
        }

        public void Update()
        {
            switch (gamePhase)
            {
                case GamePhase.ShiftDownFieldCells:

                    if (delayOfShift >= 1)
                    {
                        delayOfShift = 0;
                        ShiftDownGameField();
                    }
                    else
                        delayOfShift++;

                    break;
                case GamePhase.ShiftRightFieldCells:
                    ShiftRightGameField();
                    break;
                case GamePhase.AddBallToEmptyColumns:
                    AddBallToEmptyColumns(GetEmptyFirstColumnCount());
                    break;
            }
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

            gamePhase = GamePhase.ShiftRightFieldCells;
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

            if (GetEmptyFirstColumnCount() == 0)
            {
                gamePhase = GamePhase.WaitingSelectArea;
                CanUndoLastTurn = true;

                CalculateDifferentBallsAroundCell();
                CheckDefeat();
            }
            else
                gamePhase = GamePhase.AddBallToEmptyColumns;
        }

        private int GetCellWithOutBallPosition(int numberPartOfArray)
        {
            int position = -1;

            switch (gamePhase)
            {
                case GamePhase.ShiftDownFieldCells:

                    for (int y = 0; y < FieldWidth; y++)
                        if (!gameField[numberPartOfArray, y].HasBall)
                            position = y;

                    break;
                case GamePhase.ShiftRightFieldCells:

                    for (int x = 0; x < FieldHeight; x++)
                        if (!gameField[x, numberPartOfArray].HasBall)
                            position = x;

                    break;
            }

            return position;
        }

        private int GetEmptyFirstColumnCount()
        {
            int emptyColumnCount = 0;

            for (int x = 0; x < FieldWidth; x++)
            {
                int emptyCellCount = 0;

                for (int y = 0; y < FieldHeight; y++)
                    if (!gameField[x, y].HasBall)
                        emptyCellCount++;

                if (emptyCellCount == FieldHeight)
                    emptyColumnCount++;
            }

            return emptyColumnCount;
        }

        private void CalculateDifferentBallsAroundCell()
        {
            for (int x = 0; x < FieldWidth; x++)
                for (int y = 0; y < FieldHeight; y++)
                {
                    gameField[x, y].DifferentColorAdjacentBallDirections.Clear();

                    if (!gameField[x, y].HasBall)
                        continue;


                    foreach (Point direction in directions)
                    {
                        Point nearPoint = new Point(x + direction.X, y + direction.Y);

                        if (nearPoint.X < 0 || nearPoint.X >= FieldWidth ||
                            nearPoint.Y < 0 || nearPoint.Y >= FieldHeight ||
                            !gameField[nearPoint.X, nearPoint.Y].HasBall ||
                            gameField[x, y].BallColor != gameField[nearPoint.X, nearPoint.Y].BallColor)
                            gameField[x, y].DifferentColorAdjacentBallDirections.Add(direction);
                    }
                }
        }

        private void CheckDefeat()
        {
            bool isPotentialSelectedArea = gameField.Cast<CellState>()
                                                    .Any(cell => cell.HasBall && cell.DifferentColorAdjacentBallDirections.Count < 4);

            if (!isPotentialSelectedArea)
                Defeat();
        }

        private void AddBallToEmptyColumns(int columnCount)
        {
            for (int i = 0; i < columnCount; i++)
            {
                int ballsCount = CellState.Random.Next(1, FieldHeight);

                for (int y = 0; y <= ballsCount; y++)
                    gameField[i, FieldHeight - y - 1] = new CellState(cellSize);
            }

            gamePhase = GamePhase.ShiftDownFieldCells;
        }

        public void UndoDeleteSelectedArea()
        {
            if (!CanUndoLastTurn)
                return;

            CopyArray(gameFieldOnPreviousTurn, gameField);
            CalculateDifferentBallsAroundCell();
            score -= deletedAreaCount;
            selectedArea.Clear();
            CanUndoLastTurn = false;
        }

        private void CopyArray(CellState[,] sourceArray, CellState[,] destinationArray)
        {
            for (int x = 0; x < FieldWidth; x++)
                for (int y = 0; y < FieldHeight; y++)
                    destinationArray[x, y] =
                        new CellState(sourceArray[x, y].BallColor, cellSize, sourceArray[x, y].HasBall);
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
                    if (gameField[x, y].HasBall && selectedArea.Contains(gameField[x, y]))
                    {
                        foreach (var direction in gameField[x, y].DifferentColorAdjacentBallDirections)
                        {
                            if (direction.X < 0 || direction.Y < 0)
                                graphics.DrawLine(selectedAreaPen, x * cellSize, y * cellSize,
                                    (x - direction.Y) * cellSize, (y - direction.X) * cellSize);

                            if( direction.X > 0 || direction.Y > 0)
                                graphics.DrawLine(selectedAreaPen, (x + 1) * cellSize, (y + 1) * cellSize,
                                    (x + 1 - direction.Y) * cellSize, (y + 1 - direction.X) * cellSize);
                        }
                    }

        }

        private void DrawPointsCount(Graphics graphics)
        {
            int selectedAreaPointsCount = selectedArea.Count * (selectedArea.Count - 1);

            if (selectedAreaPointsCount <= 0 || gamePhase != GamePhase.WaitingConfirmSelectedArea)
                return;

            Point pointsCountPosition = GetCountPosition();

            graphics.FillEllipse(Brushes.LightGreen, pointsCountPosition.X, pointsCountPosition.Y, 30, 25);
            graphics.DrawString(selectedAreaPointsCount.ToString(), pointCountFont, Brushes.Black,
                pointsCountPosition.X + 5, pointsCountPosition.Y + 5);

            Point GetCountPosition()
            {
                for (int y = 0; y < FieldHeight; y++)
                    for (int x = 0; x < FieldWidth; x++)
                    {
                        if (gameField[x, y].HasBall && selectedArea.Contains(gameField[x, y]))
                        { 
                            var position = new Point(x * cellSize, y * cellSize);

                            if (position.X - 15 >= 0)
                                position.X -= 15;

                            if (position.Y - 15 >= 0)
                                position.Y -= 15;

                            return position;
                        }
                    }

                return new Point(0, 0);
            }
        }

        private void DrawScore(Graphics graphics)
        {
            graphics.DrawString("Score: " + score, scoreFont, Brushes.Black, 0, 440);
        }
    }
}
