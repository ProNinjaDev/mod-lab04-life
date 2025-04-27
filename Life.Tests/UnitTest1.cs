using cli_life;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;


namespace Life.Tests 
{

    public class LifeUnitTests
    {
        private Cell CreateCellWithLiveNeighbors(bool isAlive, int liveNeighbors)
        {
            Cell cell = new Cell { IsAlive = isAlive };
            for (int i = 0; i < liveNeighbors; i++)
            {
                cell.neighbors.Add(new Cell { IsAlive = true });
            }
            for (int i = 0; i < 8 - liveNeighbors; i++)
            {
                cell.neighbors.Add(new Cell { IsAlive = false });
            }
            return cell;
        }

        [TestCase(false, 0, false, TestName = "DeadCell_ZeroNeighbors_StaysDead")]
        [TestCase(false, 2, false, TestName = "DeadCell_TwoNeighbors_StaysDead")]
        [TestCase(false, 3, true,  TestName = "DeadCell_ThreeNeighbors_BecomesAlive")]
        [TestCase(false, 4, false, TestName = "DeadCell_FourNeighbors_StaysDead")]
        [TestCase(true,  1, false, TestName = "LiveCell_OneNeighbor_Dies")]
        [TestCase(true,  2, true,  TestName = "LiveCell_TwoNeighbors_StaysAlive")]
        [TestCase(true,  3, true,  TestName = "LiveCell_ThreeNeighbors_StaysAlive")]
        [TestCase(true,  4, false, TestName = "LiveCell_FourNeighbors_Dies")]
        public void Cell_NextState_IsCorrect(bool initialIsAlive, int liveNeighbors, bool expectedIsAlive)
        {
            Cell cell = CreateCellWithLiveNeighbors(initialIsAlive, liveNeighbors);

            cell.DetermineNextLiveState();
            cell.Advance();

            Assert.That(cell.IsAlive, Is.EqualTo(expectedIsAlive));
        }

        private Board CreateBoardFromPattern(string[] pattern)
        {
            if (pattern == null || pattern.Length == 0 || pattern[0].Length == 0)
            {
                return new Board(1, 1, 1, 0);
            }

            int height = pattern.Length;
            int width = pattern[0].Length;

            Board board = new Board(width, height, 1, 0);

            for (int y = 0; y < height; y++)
            {
                if (pattern[y].Length != width)
                    throw new ArgumentException("Pattern rows must have the same length");

                for (int x = 0; x < width; x++)
                {
                    board.Cells[x, y].IsAlive = (pattern[y][x] == '*');
                }
            }
            return board;
        }

        private bool BoardsAreSame(Board board1, Board board2)
        {
            if (board1.Columns != board2.Columns || board1.Rows != board2.Rows)
                return false;

            for (int x = 0; x < board1.Columns; x++)
            {
                for (int y = 0; y < board1.Rows; y++)
                {
                    if (board1.Cells[x, y].IsAlive != board2.Cells[x, y].IsAlive)
                        return false;
                }
            }
            return true;
        }


        [Test]
        public void Block_CountLivingCells_ReturnsFour()
        {
            string[] blockPattern = {
                "....",
                ".**.",
                ".**.",
                "...."};
            Board board = CreateBoardFromPattern(blockPattern);

            int livingCells = board.CntLivingCells();

            Assert.That(livingCells, Is.EqualTo(4));
        }

        [Test]
        public void Block_Advance_StaysSame()
        {
            string[] blockPattern = {
                "....",
                ".**.",
                ".**.",
                "...."};
            Board initialBoard = CreateBoardFromPattern(blockPattern);
            Board expectedBoard = CreateBoardFromPattern(blockPattern);

            initialBoard.Advance();

            Assert.That(BoardsAreSame(expectedBoard, initialBoard), Is.True);
        }

        [Test]
        public void Hive_CountLivingCells_ReturnsSix()
        {
            string[] hivePattern = {
                "......",
                "..**..",
                ".*..*.",
                "..**..",
                "......"};
            Board board = CreateBoardFromPattern(hivePattern);
            int livingCells = board.CntLivingCells();
            Assert.That(livingCells, Is.EqualTo(6));
        }

        [Test]
        public void Hive_Advance_StaysSame()
        {
            string[] hivePattern = {
                "......",
                "..**..",
                ".*..*.",
                "..**..",
                "......"};
            Board initialBoard = CreateBoardFromPattern(hivePattern);
            Board expectedBoard = CreateBoardFromPattern(hivePattern);
            initialBoard.Advance();
            Assert.That(BoardsAreSame(expectedBoard, initialBoard), Is.True);
        }

        [Test]
        public void Boat_CountLivingCells_ReturnsFive()
        {
            string[] boatPattern = {
                ".....",
                ".**..",
                ".*.*.",
                "..*..",
                "....."};
            Board board = CreateBoardFromPattern(boatPattern);
            int livingCells = board.CntLivingCells();
            Assert.That(livingCells, Is.EqualTo(5));
        }

        [Test]
        public void Boat_Advance_StaysSame()
        {
            string[] boatPattern = {
                ".....",
                ".**..",
                ".*.*.",
                "..*..",
                "....."};
            Board initialBoard = CreateBoardFromPattern(boatPattern);
            Board expectedBoard = CreateBoardFromPattern(boatPattern);
            initialBoard.Advance();
            Assert.That(BoardsAreSame(expectedBoard, initialBoard), Is.True);
        }

        [Test]
        public void NormalizeShape_EmptyInput_ReturnsEmptySet()
        {
            var emptyList = new List<(int x, int y)>();
            var normalized = Program.NormalizeShape(emptyList);
            Assert.That(normalized, Is.Empty);
        }

        [Test]
        public void NormalizeShape_SinglePoint_ReturnsPointAtOrigin()
        {
            var singlePoint = new List<(int x, int y)> { (5, 10) };
            var normalized = Program.NormalizeShape(singlePoint);
            var expected = new HashSet<(int, int)> { (0, 0) };
            Assert.That(normalized, Is.EqualTo(expected));
        }

        [Test]
        public void NormalizeShape_BlockAtOrigin_ReturnsCorrectShape()
        {
            var blockCells = new List<(int x, int y)> { (0, 0), (1, 0), (0, 1), (1, 1) };
            var normalized = Program.NormalizeShape(blockCells);
            var expected = new HashSet<(int, int)> { (0, 0), (1, 0), (0, 1), (1, 1) };
            Assert.That(normalized, Is.EqualTo(expected));
            Assert.That(normalized.SetEquals(expected), Is.True);
        }

        [Test]
        public void NormalizeShape_ShiftedBlock_ReturnsShapeAtOrigin()
        {
            var blockCells = new List<(int x, int y)> { (3, 5), (4, 5), (3, 6), (4, 6) };
            var normalized = Program.NormalizeShape(blockCells);
            var expected = new HashSet<(int, int)> { (0, 0), (1, 0), (0, 1), (1, 1) };
            Assert.That(normalized.SetEquals(expected), Is.True);
        }


         [Test]
        public void ClassifyShape_IncompleteBlock_ReturnsUndefined()
        {
            var incompleteBlock = new HashSet<(int, int)> { (0, 0), (1, 0), (0, 1) };

            string shapeName = Program.ClassifyShape(incompleteBlock);

            Assert.That(shapeName, Is.EqualTo("Undefined"));
        }

        [Test]
        public void ClassifyShape_EmptyShape_ReturnsUndefined()
        {
            var emptyShape = new HashSet<(int, int)>();
            string shapeName = Program.ClassifyShape(emptyShape);
            Assert.That(shapeName, Is.EqualTo("Undefined"));
        }

    }

}