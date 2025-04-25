using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json; 
using System.IO;

namespace cli_life
{
    public class SettingsJSON
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int CellSize { get; set; }
        public double LiveDensity { get; set; }
    }

    public class Cell
    {
        public bool IsAlive;
        public readonly List<Cell> neighbors = new List<Cell>();
        private bool IsAliveNext;
        public void DetermineNextLiveState()
        {
            int liveNeighbors = neighbors.Where(x => x.IsAlive).Count();
            if (IsAlive)
                IsAliveNext = liveNeighbors == 2 || liveNeighbors == 3;
            else
                IsAliveNext = liveNeighbors == 3;
        }
        public void Advance()
        {
            IsAlive = IsAliveNext;
        }
    }
    public class Board
    {
        public readonly Cell[,] Cells;
        public readonly int CellSize;

        public int Columns { get { return Cells.GetLength(0); } }
        public int Rows { get { return Cells.GetLength(1); } }
        public int Width { get { return Columns * CellSize; } }
        public int Height { get { return Rows * CellSize; } }

        public Board(int width, int height, int cellSize, double liveDensity = .1)
        {
            CellSize = cellSize;

            Cells = new Cell[width / cellSize, height / cellSize];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    Cells[x, y] = new Cell();

            ConnectNeighbors();
            Randomize(liveDensity);
        }

        readonly Random rand = new Random();
        public void Randomize(double liveDensity)
        {
            foreach (var cell in Cells)
                cell.IsAlive = rand.NextDouble() < liveDensity;
        }

        public void Advance()
        {
            foreach (var cell in Cells)
                cell.DetermineNextLiveState();
            foreach (var cell in Cells)
                cell.Advance();
        }
        private void ConnectNeighbors()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int xL = (x > 0) ? x - 1 : Columns - 1;
                    int xR = (x < Columns - 1) ? x + 1 : 0;

                    int yT = (y > 0) ? y - 1 : Rows - 1;
                    int yB = (y < Rows - 1) ? y + 1 : 0;

                    Cells[x, y].neighbors.Add(Cells[xL, yT]);
                    Cells[x, y].neighbors.Add(Cells[x, yT]);
                    Cells[x, y].neighbors.Add(Cells[xR, yT]);
                    Cells[x, y].neighbors.Add(Cells[xL, y]);
                    Cells[x, y].neighbors.Add(Cells[xR, y]);
                    Cells[x, y].neighbors.Add(Cells[xL, yB]);
                    Cells[x, y].neighbors.Add(Cells[x, yB]);
                    Cells[x, y].neighbors.Add(Cells[xR, yB]);
                }
            }
        }
    }
    class Program
    {
        static Board board;
        static private void Reset(string colonyFile = null)
        {
            string json = File.ReadAllText("settings.json");
            SettingsJSON settings = JsonSerializer.Deserialize<SettingsJSON>(json);

            if (!string.IsNullOrEmpty(colonyFile)) {
                bool isLoaded = LoadColony(colonyFile, settings.Width, settings.Height);
                if(isLoaded) {
                    return;
                }
            }

            string loadFileName = "BoardCondition.txt";
            if (File.Exists(loadFileName)) {
                bool isFullBoardLoaded = LoadFullBoard(loadFileName);

                if(isFullBoardLoaded) {
                    return;
                }
            }

            board = new Board(
                width: settings.Width,
                height: settings.Height,
                cellSize: settings.CellSize,
                liveDensity: settings.LiveDensity);
        }

        static private bool LoadFullBoard(string filename) {
            string[] lines = File.ReadAllLines(filename);

            if (lines.Length == 0 || lines[0].Length == 0) {
                Console.WriteLine("Empty load file");
                return false;
            }

            int height = lines.Length;
            int width = lines[0].Length;

            board = new Board(width, height, 1, 0);
            for (int row = 0; row < height; row++) {
                for (int column = 0; column < width; column++) {
                    board.Cells[column, row].IsAlive = lines[row][column] == '*' ? true : false;
                }
            }
            return true;
        }
        static private bool LoadColony (string colonyFile, int boardWidth, int boardHeight) {
            if (!File.Exists(colonyFile)) {
                return false;
            }

            string[] lines = File.ReadAllLines(colonyFile);

            if (lines.Length == 0 || lines[0].Length == 0) {
                Console.WriteLine("Empty load file");
                return false;
            }
            
            int colonyHeight = lines.Length;
            int colonyWidth = lines[0].Length;
            for (int i = 1; i < colonyHeight; i++)
            {
                if (lines[i].Length != colonyWidth)
                {
                    return false;
                }
            }
            if(colonyHeight > boardHeight || colonyWidth > boardWidth) {
                return false;
            }

            int offsetCenterX = (boardWidth - colonyWidth) / 2;
            int offsetCenterY = (boardHeight - colonyHeight) / 2;
            board = new Board(boardWidth, boardHeight, 1, 0);

            for (int y = 0; y < colonyHeight; y++)
            {
                for (int x = 0; x < colonyWidth; x++)
                {
                    if (lines[y][x] == '*')
                    {
                        int targetX = offsetCenterX + x;
                        int targetY = offsetCenterY + y;
                        board.Cells[targetX, targetY].IsAlive = true;
                    }
                }
            }
            return true;
        }
        static void Render()
        {
            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Columns; col++)   
                {
                    var cell = board.Cells[col, row];
                    if (cell.IsAlive)
                    {
                        Console.Write('*');
                    }
                    else
                    {
                        Console.Write(' ');
                    }
                }
                Console.Write('\n');
            }
        }

        static void SaveBoardCondition(string filename) {
            StreamWriter streamWriter = new StreamWriter(filename);
            for (int row = 0; row < board.Rows; row++) {
                for (int column = 0; column < board.Columns; column++) {
                    streamWriter.Write(board.Cells[column, row].IsAlive ? '*' : ' ');
                }
                streamWriter.WriteLine();
            }
            streamWriter.Close();
        }
        static void Main(string[] args)
        {
            string colonyFile = null;

            if(args.Length > 0) {
                colonyFile = args[0];
            }

            Reset(colonyFile);
            while(true)
            {
                Console.Clear();
                Render();
                board.Advance();
                if (Console.KeyAvailable){
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                    if(keyInfo.Key == ConsoleKey.S) {
                        SaveBoardCondition("BoardCondition.txt");
                    }
                }
                Thread.Sleep(1000);
            }
        }
    }
}