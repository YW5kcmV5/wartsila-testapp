
namespace Wartsila.TestApp.TestConsole
{
    public static class ProblemC
    {
        /// <summary>
        /// Represents a cell position in the parking lot grid using zero-based row and column indexes.
        /// </summary>
        public readonly record struct Cell(int Row, int Column);
        
        /// <summary>
        /// Represents a graph transition from one empty-cell position to another by moving a specific car.
        /// </summary>
        public readonly record struct Edge(int TargetCellIndex, int CarCode);
        
        /// <summary>
        /// Represents a two-cell car and exposes its orientation in the grid.
        /// </summary>
        public sealed class Car
        {
            public int Code { get; init; }

            public Cell First { get; init; }

            public Cell Second { get; init; }

            public bool IsHorizontal => (First.Row == Second.Row);

            public bool IsVertical => (First.Column == Second.Column);
        }

        /// <summary>
        /// Stores parsed input data, grid helpers, car locations, the initial empty cell, and the target cell.
        /// </summary>
        public sealed class InputData
        {
            private const int EmptyCode = -1;
            private const int BlockedCode = -2;
            
            public int Rows { get; private set; }
            
            public int Columns { get; private set; }

            public int Size => Rows * Columns;
            
            public Cell Target { get; private set; }
            
            public Cell EmptyCell { get; private set; }

            public int[,] Locations { get; private set; } = new int[0, 0];
            
            public Car[] Cars { get; private set; } = [];

            public static bool IsBlocked(int point)
            {
                return (point == BlockedCode);
            }

            public static bool IsEmpty(int point)
            {
                return (point == EmptyCode);
            }

            public static bool IsCar(int point)
            {
                return (!IsBlocked(point)) && (!IsEmpty(point));
            }

            public bool IsBlocked(int row, int column)
            {
                return IsBlocked(Locations[row, column]);
            }

            public bool IsBlocked(Cell cell)
            {
                int locationCode = GetLocation(cell);
                return IsBlocked(locationCode);
            }

            public bool IsEmpty(int row, int column)
            {
                int locationCode = GetLocation(row, column);
                return IsEmpty(locationCode);
            }

            public bool IsEmpty(Cell cell)
            {
                int locationCode = GetLocation(cell);
                return IsEmpty(locationCode);
            }

            public bool IsCar(int row, int column)
            {
                int locationCode = GetLocation(row, column);
                return IsCar(locationCode);
            }

            public bool IsCar(Cell cell)
            {
                int locationCode = GetLocation(cell);
                return IsCar(locationCode);
            }

            public bool IsTarget(int row, int column)
            {
                return ((Target.Row == row) && (Target.Column == column));
            }

            public bool IsTarget(Cell cell)
            {
                return IsTarget(cell.Row, cell.Column);
            }

            public bool IsInside(int row, int column)
            {
                return
                (
                    (row >= 0) &&
                    (row < Rows) &&
                    (column >= 0) &&
                    (column < Columns)
                );
            }

            public bool IsInside(Cell cell)
            {
                return IsInside(cell.Row, cell.Column);
            }

            public int GetLocation(Cell cell)
            {
                return Locations[cell.Row, cell.Column];
            }

            public int GetLocation(int row, int column)
            {
                return Locations[row, column];
            }

            public int GetIndex(int row, int column)
            {
                return (row * Columns) + column;
            }

            public int GetIndex(Cell cell)
            {
                return GetIndex(cell.Row, cell.Column);
            }

            public void Initialize(int[][] locations, int[] target)
            {
                Rows = locations.Length;
                Columns = locations[0].Length;
                Locations = new int[Rows, Columns];
                
                var carCells = new Dictionary<int, List<Cell>>();

                for (int rowIndex = 0; rowIndex < Rows; rowIndex++)
                {
                    for (int columnIndex = 0; columnIndex < Columns; columnIndex++)
                    {
                        int locationCode = locations[rowIndex][columnIndex];
                        var cell = new Cell(rowIndex, columnIndex);
                        
                        Locations[rowIndex, columnIndex] = locationCode;

                        if (IsEmpty(locationCode))
                        {
                            EmptyCell = cell;
                        }
                        else if (IsCar(locationCode))
                        {
                            if (!carCells.TryGetValue(locationCode, out List<Cell>? cells))
                            {
                                cells = new List<Cell>();
                                carCells[locationCode] = cells;
                            }

                            cells.Add(cell);
                        }
                    }
                }

                int targetRow = target[0] - 1;
                int targetColumn = target[1] - 1;
                
                Target = new Cell(targetRow, targetColumn);
                
                Cars = carCells
                    .Select(cell => new Car
                    {
                        Code = cell.Key,
                        First = cell.Value[0],
                        Second = cell.Value[1]
                    })
                    .OrderBy(car => car.Code)
                    .ToArray();
            }
        }

        /// <summary>
        /// Represents the final solver result and formats it according to the problem output requirements.
        /// </summary>
        public sealed class Result
        {
            public bool Impossible { get; init; }

            public int[] Steps { get; init; } = [];
            
            public string Output => (Impossible)
                ? "impossible"
                : string.Join(" ", Steps);
        }

        /// <summary>
        /// Builds and stores the graph of all possible empty-cell transitions caused by valid car moves.
        /// </summary>
        public sealed class Graph
        {
            private readonly InputData _input;
            private readonly List<Edge>[] _edges;
            
            public Graph(InputData input)
            {
                _input = input;

                _edges = new List<Edge>[input.Size];
                for (int i = 0; i < input.Size; i++)
                {
                    _edges[i] = new List<Edge>();
                }

                Build();
            }
            
            public IReadOnlyList<Edge> this[int index] => _edges[index];
            
            #region Private

            private void AddEdge(int from, int targetCellIndex, int carCode)
            {
                var edge = new Edge(targetCellIndex, carCode);
                _edges[from].Add(edge);
            }

            private void AddBidirectionalEdge(int firstCellIndex, int secondCellIndex, int carCode)
            {
                AddEdge(firstCellIndex, secondCellIndex, carCode);
                AddEdge(secondCellIndex, firstCellIndex, carCode);
            }

            private void AddCarEdges(Car car)
            {
                if (car.IsHorizontal)
                {
                    AddHorizontalCarEdges(car);
                }
                else if (car.IsVertical)
                {
                    AddVerticalCarEdges(car);
                }
            }

            private void AddHorizontalCarEdges(Car car)
            {
                int row = car.First.Row;
                int leftColumn = Math.Min(car.First.Column, car.Second.Column);
                int rightColumn = Math.Max(car.First.Column, car.Second.Column);

                var leftCell = new Cell(row, leftColumn);
                var rightCell = new Cell(row, rightColumn);
                var beforeCar = new Cell(row, leftColumn - 1);
                var afterCar = new Cell(row, rightColumn + 1);

                TryAddBidirectionalTransition(beforeCar, rightCell, car.Code);
                TryAddBidirectionalTransition(afterCar, leftCell, car.Code);
            }

            private void AddVerticalCarEdges(Car car)
            {
                int column = car.First.Column;
                int topRow = Math.Min(car.First.Row, car.Second.Row);
                int bottomRow = Math.Max(car.First.Row, car.Second.Row);

                var topCell = new Cell(topRow, column);
                var bottomCell = new Cell(bottomRow, column);
                var beforeCar = new Cell(topRow - 1, column);
                var afterCar = new Cell(bottomRow + 1, column);

                TryAddBidirectionalTransition(beforeCar, bottomCell, car.Code);
                TryAddBidirectionalTransition(afterCar, topCell, car.Code);
            }

            private void TryAddBidirectionalTransition(Cell first, Cell second, int carCode)
            {
                if (!CanBeEmpty(first) || !CanBeEmpty(second))
                {
                    return;
                }

                int firstCellIndex = _input.GetIndex(first);
                int secondCellIndex = _input.GetIndex(second);

                AddBidirectionalEdge(firstCellIndex, secondCellIndex, carCode);
            }

            private bool CanBeEmpty(Cell cell)
            {
                return ((_input.IsInside(cell)) && (!_input.IsBlocked(cell)));
            }

            private void SortEdges()
            {
                for (int i = 0; i < _edges.Length; i++)
                {
                    _edges[i] = _edges[i]
                        .OrderBy(edge => edge.CarCode)
                        .ThenBy(edge => edge.TargetCellIndex)
                        .ToList();
                }
            }

            private void Build()
            {
                foreach (Car car in _input.Cars)
                {
                    AddCarEdges(car);
                }
                
                SortEdges();
            }
            
            #endregion

            /// <summary>
            /// Creates a transition graph for the provided input.
            /// </summary>
            public static Graph Build(InputData input)
            {
                return new Graph(input);
            }
        }

        /// <summary>
        /// Finds the shortest path through the transition graph and restores the corresponding car move sequence.
        /// </summary>
        public sealed class PathFinder
        {
            /// <summary>
            /// Stores BFS state for a single cell index, including visit status and path reconstruction data.
            /// </summary>
            private sealed class CellPathState
            {
                public bool IsVisited { get; set; }

                public int PreviousCellIndex { get; set; }

                public int PreviousCarCode { get; set; }
            }

            private readonly Graph _graph;
            private readonly CellPathState[] _cellStates;

            /// <summary>
            /// Initializes a pathfinder for the provided graph and number of cell indexes.
            /// </summary>
            public PathFinder(Graph graph, int size)
            {
                _graph = graph;
                _cellStates = new CellPathState[size];

                for (int i = 0; i < size; i++)
                {
                    _cellStates[i] = new CellPathState
                    {
                        PreviousCellIndex = -1,
                        PreviousCarCode = -1
                    };
                }
            }

            /// <summary>
            /// Finds the shortest sequence of car codes that moves the empty cell from the start index to the target index.
            /// </summary>
            /// <returns>
            /// The sequence of car codes, or <c>null</c> when the target cell cannot be reached.
            /// </returns>
            public int[]? FindShortestPath(int startIndex, int targetIndex)
            {
                Search(startIndex, targetIndex);

                return (_cellStates[targetIndex].IsVisited)
                    ? RestoreSteps(startIndex, targetIndex)
                    : null;
            }
            
            #region Private

            private void Search(int startIndex, int targetIndex)
            {
                var queue = new Queue<int>();

                CellPathState startState = _cellStates[startIndex];
                startState.IsVisited = true;
                
                queue.Enqueue(startIndex);

                while (queue.Count > 0)
                {
                    int currentIndex = queue.Dequeue();

                    if (currentIndex == targetIndex)
                    {
                        return;
                    }

                    IReadOnlyList<Edge> currentEdges = _graph[currentIndex];
                    foreach (Edge edge in currentEdges)
                    {
                        int nextIndex = edge.TargetCellIndex;
                        CellPathState nextState = _cellStates[nextIndex];
                        
                        if (nextState.IsVisited)
                        {
                            continue;
                        }

                        nextState.IsVisited = true;
                        nextState.PreviousCellIndex = currentIndex;
                        nextState.PreviousCarCode = edge.CarCode;

                        queue.Enqueue(nextIndex);
                    }
                }
            }

            private int[] RestoreSteps(int startIndex, int targetIndex)
            {
                var steps = new Stack<int>();

                int currentIndex = targetIndex;
                while (currentIndex != startIndex)
                {
                    CellPathState currentState = _cellStates[currentIndex];

                    steps.Push(currentState.PreviousCarCode);
                    currentIndex = currentState.PreviousCellIndex;
                }

                return steps.ToArray();
            }
            
            #endregion
        }
        
        public static void Tests()
        {
            // Basic tests
            Test("4 4\n8 8 -1 9\n4 10 10 9\n4 3 3 -2\n16 16 2 2\n3 3", "8 4 3");
            Test("4 4\n8 8 -2 9\n4 10 10 9\n4 3 3 -1\n16 16 2 2\n3 3", "impossible");
            
            // Etended tests
            Test("1 3\n-1 1 1\n1 3", "1");
            Test("1 3\n1 1 -1\n1 1", "1");
            Test("3 1\n-1\n2\n2\n3 1", "2");
            Test("3 1\n2\n2\n-1\n1 1", "2");
            Test("1 5\n-1 1 1 2 2\n1 5", "1 2");
            Test("1 5\n1 1 2 2 -1\n1 1", "2 1");
            Test("1 4\n-1 1 1 -2\n1 2", "impossible");
            Test("1 4\n-1 1 1 -2\n1 3", "1");
            Test("3 3\n-1 1 1\n2 2 3\n4 4 3\n3 3", "1 3");
        }

        public static void Invoke()
        {
            InputData input = ReadInput();

            Result result = Resolve(input);
            
            Console.WriteLine(result.Output);
        }

        public static Result Resolve(InputData input)
        {
            Graph graph = Graph.Build(input);
            var pathFinder = new PathFinder(graph, input.Size);

            int startIndex = input.GetIndex(input.EmptyCell);
            int targetIndex = input.GetIndex(input.Target);

            int[]? steps = pathFinder.FindShortestPath(startIndex, targetIndex);

            return (steps == null)
                ? new Result { Impossible = true }
                : new Result { Steps = steps };
        }

        #region Private

        private static InputData ReadInput()
        {
            return ReadInput(() => Console.ReadLine() ?? string.Empty);
        }

        private static InputData ReadInput(string input)
        {
            string[] lines = input.Split('\n');
            int line = 0;
            return ReadInput(() => lines[line++]);
        }

        private static InputData ReadInput(Func<string> readLine)
        {
            var input = new InputData();

            string sizeCode = readLine();

            int[] size = sizeCode
                .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToArray();

            int rows = size[0];
            //int columns = size[1];
            var locations = new List<int[]>();

            for (int rowIndex = 0; rowIndex < rows; rowIndex++)
            {
                string row = readLine();

                int[] rowLocations = row
                    .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToArray();
                
                locations.Add(rowLocations);
            }

            string targetCode = readLine();

            int[] target = targetCode
                .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToArray();

            input.Initialize(locations.ToArray(), target);

            return input;
        }

        private static void Assert(string? expected, string? result, string? input)
        {
            bool passed = (result == expected);

            if (passed)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Test PASSED: expected=\"{expected}\", result=\"{result}\", input==\"{input}\".");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Test FAILED: expected=\"{expected}\", result=\"{result}\", input==\"{input}\".");
            }
            
            Console.ResetColor();
        }

        private static void Test(string input, string expected)
        {
            InputData inputData = ReadInput(input);

            Result result = Resolve(inputData);
            
            Assert(expected, result.Output, input);
        }

        #endregion
    }
}
