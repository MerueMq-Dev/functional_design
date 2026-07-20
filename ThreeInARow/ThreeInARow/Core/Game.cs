namespace ThreeInARow.Core;

public static partial class Game
{
    static Random r = new Random();
    static char[] symbols = { 'A', 'B', 'C', 'D', 'E', 'F' };
    const int BOARD_SIZE = 8;

    public static void Draw(Board board)
    {
        Console.WriteLine("  0 1 2 3 4 5 6 7");
        for (int i = 0; i < 8; i++)
        {
            Console.Write(i + " ");
            for (int j = 0; j < 8; j++)
            {
                Console.Write(board.cells[i, j].Symbol + " ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }

    public static Board CloneBoard(Board board)
    {
        Board b = new Board(board.size);
        for (int row = 0; row < board.size; row++)
            for (int col = 0; col < board.size; col++)
                b.cells[row, col] = board.cells[row, col];
        return b;
    }

    // Инициализация 

    public static BoardState InitializeGame()
    {
        // Шаги 1-3: создаём и заполняем доску без готовых комбинаций
        Board board = CreateFilledBoard(BOARD_SIZE);
        // Шаг 4: оборачиваем доску в BoardState с нулевым счётом
        return new BoardState(board, 0);
    }

    private static Board CreateFilledBoard(int size)
    {
        // Шаг 1: доска рождается «с нуля», клонировать нечего
        Board board = new Board(size);

        // Шаг 2: двойным циклом обходим все клетки
        for (int row = 0; row < size; row++)
            for (int col = 0; col < size; col++)
                board.cells[row, col] = new Element(PickSymbol(board, row, col));

        return board;
    }

    private static char PickSymbol(Board board, int row, int col)
    {
        while (true)
        {
            char c = symbols[r.Next(symbols.Length)];
            if (!CreatesTriple(board, row, col, c)) return c;
        }
    }

    private static bool CreatesTriple(Board board, int row, int col, char c)
    {
        // Шаг 3: проверяем двух соседей слева и двух сверху (они уже заполнены)
        bool left = col >= 2
            && board.cells[row, col - 1].Symbol == c
            && board.cells[row, col - 2].Symbol == c;

        bool up = row >= 2
            && board.cells[row - 1, col].Symbol == c
            && board.cells[row - 2, col].Symbol == c;

        return left || up;
    }
   
    public static BoardState ReadMove(BoardState bs)
    {
        Console.WriteLine(">");
        string input = Console.ReadLine();
        if (input == "q")
            Environment.Exit(0);

        Board board = CloneBoard(bs.Board);
        string[] coords = input.Split(' ');
        int x = int.Parse(coords[1]);
        int y = int.Parse(coords[0]);
        int x1 = int.Parse(coords[3]);
        int y1 = int.Parse(coords[2]);
        Element e = board.cells[x, y];
        board.cells[x, y] = board.cells[x1, y1];
        board.cells[x1, y1] = e;
        BoardState bb = new BoardState(board, bs.Score);
        return bb;
    }

    public static List<Match> FindMatches(Board board)
    {
        var matches = new List<Match>();

        // Горизонтальные комбинации
        for (int row = 0; row < board.size; row++)
        {
            int startCol = 0;
            for (int col = 1; col < board.size; col++)
            {
                // Пропускаем пустые ячейки в начале строки
                if (board.cells[row, startCol].Symbol == Element.EMPTY)
                {
                    startCol = col;
                    continue;
                }

                // Если текущая ячейка пустая, обрываем текущую последовательность
                if (board.cells[row, col].Symbol == Element.EMPTY)
                {
                    AddMatchIfValid(matches, row, startCol, col - startCol, MatchDirection.Horizontal);
                    startCol = col + 1;
                    continue;
                }

                // Проверяем совпадение символов для непустых ячеек
                if (board.cells[row, col].Symbol != board.cells[row, startCol].Symbol)
                {
                    AddMatchIfValid(matches, row, startCol, col - startCol, MatchDirection.Horizontal);
                    startCol = col;
                }
                else if (col == board.size - 1)
                {
                    AddMatchIfValid(matches, row, startCol, col - startCol + 1, MatchDirection.Horizontal);
                }
            }
        }

        // Вертикальные комбинации
        for (int col = 0; col < board.size; col++)
        {
            int startRow = 0;
            for (int row = 1; row < board.size; row++)
            {
                // Пропускаем пустые ячейки в начале столбца
                if (board.cells[startRow, col].Symbol == Element.EMPTY)
                {
                    startRow = row;
                    continue;
                }

                // Если текущая ячейка пустая, обрываем текущую последовательность
                if (board.cells[row, col].Symbol == Element.EMPTY)
                {
                    AddMatchIfValid(matches, startRow, col, row - startRow, MatchDirection.Vertical);
                    startRow = row + 1;
                    continue;
                }

                // Проверяем совпадение символов для непустых ячеек
                if (board.cells[row, col].Symbol != board.cells[startRow, col].Symbol)
                {
                    AddMatchIfValid(matches, startRow, col, row - startRow, MatchDirection.Vertical);
                    startRow = row;
                }
                else if (row == board.size - 1)
                {
                    AddMatchIfValid(matches, startRow, col, row - startRow + 1, MatchDirection.Vertical);
                }
            }
        }

        return matches;
    }

    private static void AddMatchIfValid(List<Match> matches, int row, int col,
            int length, MatchDirection direction)
    {
        // Учитываем только комбинации из 3 и более элементов (ТЗ)
        if (length >= 3)
        {
            matches.Add(new Match(direction, row, col, length));
        }
    }
    
    public static BoardState RemoveMatches(BoardState currentState, List<Match> matches)
    {
        if (matches == null || matches.Count == 0)
            return currentState;

        // Шаг 1: Помечаем ячейки для удаления 
        Element[,] markedCells = MarkCellsForRemoval(currentState.Board, matches);

        // Шаг 2: Применяем гравитацию
        Element[,] gravityAppliedCells = ApplyGravity(markedCells, currentState.Board.size);

        // Шаг 3: Подсчитываем очки
        int removedCount = matches.Sum(m => m.Length);
        int newScore = currentState.Score + CalculateScore(removedCount);

        // Возвращаем НОВОЕ состояние
        return new BoardState(
            new Board { size = currentState.Board.size, cells = gravityAppliedCells },
            newScore
        );
    }

    private static Element[,] MarkCellsForRemoval(Board board, List<Match> matches)
    {
        Element[,] newCells = (Element[,])board.cells.Clone();

        foreach (var match in matches)
        {
            for (int i = 0; i < match.Length; i++)
            {
                int row = match.Direction == MatchDirection.Horizontal ? match.Row : match.Row + i;
                int col = match.Direction == MatchDirection.Horizontal ? match.Col + i : match.Col;

                newCells[row, col] = new Element { Symbol = Element.EMPTY };
            }
        }

        return newCells;
    }

    private static Element[,] ApplyGravity(Element[,] cells, int size)
    {
        Element[,] newCells = new Element[size, size];

        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                newCells[row, col] = new Element { Symbol = Element.EMPTY };
            }
        }

        for (int col = 0; col < size; col++)
        {
            int newRow = size - 1;
            for (int row = size - 1; row >= 0; row--)
            {
                if (cells[row, col].Symbol != Element.EMPTY)
                {
                    newCells[newRow, col] = cells[row, col];
                    newRow--;
                }
            }
        }

        return newCells;
    }

    private static int CalculateScore(int removedCount)
    {
        // Базовая система подсчета очков: 10 за каждый элемент
        return removedCount * 10;
    }
   
    public static BoardState FillEmptySpaces(BoardState currentState)
    {
        if (currentState.Board.cells == null)
            return currentState;

        Element[,] newCells = (Element[,])currentState.Board.cells.Clone();

        for (int row = 0; row < currentState.Board.size; row++)
        {
            for (int col = 0; col < currentState.Board.size; col++)
            {
                if (newCells[row, col].Symbol == Element.EMPTY)
                {
                    newCells[row, col] = new Element
                    {
                        Symbol = symbols[r.Next(symbols.Length)]
                    };
                }
            }
        }

        return new BoardState(
            new Board { size = currentState.Board.size, cells = newCells },
            currentState.Score
        );
    }

    public static BoardState ProcessCascade(BoardState currentState)
    {
        // Шаг 1: Находим комбинации
        List<Match> matches = FindMatches(currentState.Board);

        // Шаг 2: Если не найдено ни одной, завершаем работу с текущим состоянием
        if (matches.Count == 0)
            return currentState;

        // Шаг 3: Удаляем комбинации, считаем статистику/бонусы
        BoardState removed = RemoveMatches(currentState, matches);

        // Шаг 4: Заполняем пустые клетки
        BoardState filled = FillEmptySpaces(removed);

        // Шаг 5: Возвращаем рекурсивный вызов с этим локальным состоянием
        return ProcessCascade(filled);
    }
}