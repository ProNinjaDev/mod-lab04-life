using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json; 
using System.IO;
using ScottPlot;

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

        public int CntLivingCells(){
            int cnt = 0;
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    if (Cells[x, y].IsAlive)
                    {
                        cnt++;
                    }
                }
            }
            return cnt;
        }

        public int CntClusters()
        {
            int clusterCnt = 0;
            bool[,] visited = new bool[Columns, Rows];

            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    if (Cells[x, y].IsAlive && !visited[x, y])
                    {
                        clusterCnt++;
                        Queue<(int, int)> queue = new Queue<(int, int)>();

                        queue.Enqueue((x, y));
                        visited[x, y] = true;

                        while (queue.Count > 0)
                        {
                            var (currentX, currentY) = queue.Dequeue();

                            int xL = (currentX > 0) ? currentX - 1 : Columns - 1;
                            int xR = (currentX < Columns - 1) ? currentX + 1 : 0;
                            int yT = (currentY > 0) ? currentY - 1 : Rows - 1;
                            int yB = (currentY < Rows - 1) ? currentY + 1 : 0;

                            int[,] neighborCoords = {
                                { xL, yT }, { currentX, yT }, { xR, yT },
                                { xL, currentY }, { xR, currentY },
                                { xL, yB }, { currentX, yB }, { xR, yB }
                            };

                            for (int i = 0; i < 8; i++)
                            {
                                int nx = neighborCoords[i, 0];
                                int ny = neighborCoords[i, 1];

                                if (Cells[nx, ny].IsAlive && !visited[nx, ny])
                                {
                                    visited[nx, ny] = true;
                                    queue.Enqueue((nx, ny));
                                }
                            }
                        }
                    }
                }
            }

            return clusterCnt;
        }

        public Dictionary<string, int> AllocatorClusters()
        {
            var clusterCounts = new Dictionary<string, int>();
            bool[,] visited = new bool[Columns, Rows];

            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    if (Cells[x, y].IsAlive && !visited[x, y])
                    {
                        var currentClusterCells = new List<(int, int)>();
                        Queue<(int, int)> queue = new Queue<(int, int)>();

                        queue.Enqueue((x, y));
                        visited[x, y] = true;
                        currentClusterCells.Add((x, y));

                        while (queue.Count > 0)
                        {
                            var (currentX, currentY) = queue.Dequeue();

                            int xL = (currentX > 0) ? currentX - 1 : Columns - 1;
                            int xR = (currentX < Columns - 1) ? currentX + 1 : 0;
                            int yT = (currentY > 0) ? currentY - 1 : Rows - 1;
                            int yB = (currentY < Rows - 1) ? currentY + 1 : 0;

                            int[,] neighborCoords = {
                                { xL, yT }, { currentX, yT }, { xR, yT },
                                { xL, currentY },            { xR, currentY },
                                { xL, yB }, { currentX, yB }, { xR, yB }
                            };

                            for (int i = 0; i < 8; i++)
                            {
                                int nx = neighborCoords[i, 0];
                                int ny = neighborCoords[i, 1];

                                if (Cells[nx, ny].IsAlive && !visited[nx, ny])
                                {
                                    visited[nx, ny] = true;
                                    queue.Enqueue((nx, ny));
                                    currentClusterCells.Add((nx, ny));
                                }
                            }
                        }

                        HashSet<(int, int)> normalizedShape = Program.NormalizeShape(currentClusterCells);
                        string patternName = Program.ClassifyShape(normalizedShape);

                        if (clusterCounts.ContainsKey(patternName))
                        {
                            clusterCounts[patternName]++;
                        }
                        else
                        {
                            clusterCounts.Add(patternName, 1);
                        }
                    }
                }
            }

            return clusterCounts;
        }
    }
    class Program
    {
        static Board board;

        private static readonly Dictionary<string, HashSet<(int, int)>> SamplesShapes;

        private const int STAGNATIONCOUNT = 10;
        private static Queue<int> historyLivingCells = new Queue<int>();
        private static int generationCount = 0;
        private static bool stagnationAchieved = false;

        static Program(){
            SamplesShapes = InitializeSamples();
        }

        private static Dictionary<string, HashSet<(int, int)>> InitializeSamples() {
            var library = new Dictionary<string, HashSet<(int, int)>>();

            var samplesFiles = new Dictionary<string, string> {
                { "block.txt", "Block" },
                { "glider.txt", "Glider" },
                { "boat.txt", "Boat" },
                { "hive.txt", "Hive" },
                { "ship.txt", "Ship" },
                { "snake.txt", "Snake" }
            };

            foreach(var sampleFile in samplesFiles) {
                string filename = sampleFile.Key;
                string sampleName = sampleFile.Value;

                HashSet<(int, int)> normalizedShape = LoadNormalizeShape(filename);

                if (normalizedShape.Count > 0) {
                    library.Add(sampleName, normalizedShape);
                }
            }

            return library;
        }

        private static HashSet<(int, int)> LoadNormalizeShape(string filename) {
            if(!File.Exists(filename)) {
                return new HashSet<(int, int)>();
            }

            string[] lines = File.ReadAllLines(filename);
            if (lines.Length == 0) {
                return new HashSet<(int, int)>();
            }

            var cells = new List<(int x, int y)>();
            for (int y = 0; y < lines.Length; y++)
            {
                for (int x = 0; x < lines[y].Length; x++)
                {
                    if (lines[y][x] == '*')
                    {
                        cells.Add((x, y));
                    }
                }
            }

            return NormalizeShape(cells);
        }

        internal static string ClassifyShape(HashSet<(int, int)> normalizedShape) {
            foreach(var sampleLibraryShape in SamplesShapes) {
                string sampleName = sampleLibraryShape.Key;
                HashSet<(int, int)> sampleShape = sampleLibraryShape.Value;

                if (normalizedShape.Count != sampleShape.Count)
                {
                    continue;
                }

                if (normalizedShape.SetEquals(sampleShape))
                {
                    return sampleName;
                }
            }
            return "Undefined";
        }
        static private void Reset(string colonyFile = null, bool forceRandMode = false, double customDensity = 0.5)
        {
            string json = File.ReadAllText("settings.json");
            SettingsJSON settings = JsonSerializer.Deserialize<SettingsJSON>(json);

            if (!forceRandMode) {
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
            }
            double densityToUse = forceRandMode ? customDensity : settings.LiveDensity;

            board = new Board(
                width: settings.Width,
                height: settings.Height,
                cellSize: settings.CellSize,
                liveDensity: densityToUse);
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

            int livingCells = board.CntLivingCells();
            //int clusters = board.CntClusters();
            Dictionary<string, int> clusterInfo = board.AllocatorClusters();

            int clusters = 0;
            foreach (var cnt in clusterInfo.Values) {
                clusters += cnt;
            }

            Console.WriteLine("+-+-+-+-+-+-+-+-+-+-+-+-+");
            Console.WriteLine($"Generation: {generationCount}");
            Console.WriteLine($"Count living cells: {livingCells}");
            Console.WriteLine($"Count clusters: {clusters}");

            foreach (var cluster in clusterInfo) {
                Console.WriteLine($"    {cluster.Key}: {cluster.Value}");
            }

            Console.WriteLine("+-+-+-+-+-+-+-+-+-+-+-+-+");
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
        internal static HashSet<(int, int)> NormalizeShape(List<(int x, int y)> clusterCells)
        {
            if (clusterCells == null || clusterCells.Count == 0)
            {
                return new HashSet<(int, int)>();
            }

            int minX = clusterCells[0].x;
            int minY = clusterCells[0].y;
            foreach (var cell in clusterCells)
            {
                if (cell.x < minX) minX = cell.x;
                if (cell.y < minY) minY = cell.y;
            }

            HashSet<(int, int)> normalizedShape = new HashSet<(int, int)>();
            foreach (var cell in clusterCells)
            {
                normalizedShape.Add((cell.x - minX, cell.y - minY));
            }

            return normalizedShape;
        }

        static void Main(string[] args)
        {
            if (args.Contains("plotmode")) {
                Console.WriteLine("!!!PLOT MODE!!!");
                RunPlotMode();
                Console.WriteLine("PLOT MODE end");
            }

            else {

                string colonyFile = null;

                if(args.Length > 0) {
                    colonyFile = args[0];
                }

                Reset(colonyFile);

                generationCount = 0;
                stagnationAchieved = false;
                historyLivingCells.Clear();
                while(true)
                {
                    generationCount++;
                    Console.Clear();
                    Render();
                    board.Advance();

                    int currentLivingCells = board.CntLivingCells();

                    historyLivingCells.Enqueue(currentLivingCells);
                    if (historyLivingCells.Count > STAGNATIONCOUNT)
                    {
                        historyLivingCells.Dequeue();
                    }

                    if (!stagnationAchieved && historyLivingCells.Count == STAGNATIONCOUNT)
                    {
                        bool isStable = true;
                        int firstCount = historyLivingCells.Peek();
                        foreach (int countInHistory in historyLivingCells)
                        {
                            if (countInHistory != firstCount)
                            {
                                isStable = false;
                                break;
                            }
                        }

                        if (isStable)
                        {
                            Console.WriteLine($"Stagnation achieved on a generation {generationCount}");
                            stagnationAchieved = true;
                            break;
                        }
                    }
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

        static void RunPlotMode() {
            double[] densitiesToTest = { 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9 };
            int maxGenerations = 200;
            string dataName = "data.txt";
            string plotName = "plot.png";

            var simData = new List<(double density, int generation, int population)>();

            foreach (var density in densitiesToTest) {
                Reset(forceRandMode: true, customDensity: density);
                Queue<int> localHistory = new Queue<int>();
                bool localStagnation = false;

                for (int gen = 1; gen <= maxGenerations; gen++) {
                    int  currentPopulation = board.CntLivingCells();
                    simData.Add((density, gen, currentPopulation));

                    localHistory.Enqueue(currentPopulation);
                    if (localHistory.Count > STAGNATIONCOUNT) {
                        localHistory.Dequeue();
                    }

                    if (!localStagnation && localHistory.Count == STAGNATIONCOUNT)
                    {
                        bool isStable = true;
                        int firstCount = localHistory.Peek();
                        foreach (int countInHistory in localHistory)
                        {
                            if (countInHistory != firstCount)
                            {
                                isStable = false;
                                break;
                            }
                        }

                        if (isStable)
                        {
                            localStagnation = true;
                            Console.WriteLine($"Stagnation achieved on a generation {gen}");
                            break;
                        }
                    }

                    board.Advance();

                }
                if (!localStagnation) {
                    Console.WriteLine("Gen limit has been reached");
                }
            }

            StreamWriter writer = new StreamWriter(dataName);

            writer.WriteLine("Generation Density Population");
            foreach (var dataPoint in simData) {
                writer.WriteLine($"{dataPoint.generation} {dataPoint.density:F2} {dataPoint.population}");
            }
            writer.Close();

            Plot plt = new Plot();

            plt.Title("Number of living cells");
            plt.XLabel("Gen number");
            plt.YLabel("Cell count");

            var groupedData = simData.GroupBy(d => d.density);

            foreach (var group in groupedData) {
                double density = group.Key;

                double[] generations = group.Select(d => (double)d.generation).ToArray();
                double[] populations = group.Select(d => (double)d.population).ToArray();

                var scatter = plt.Add.Scatter(generations, populations);
                scatter.Label = $"{density:F1}";
                scatter.MarkerSize = 0;
                scatter.LineWidth = 1.5f;
            }

            plt.Legend.IsVisible = true;

            plt.SavePng(plotName, 1200, 800);
        }
    }
}