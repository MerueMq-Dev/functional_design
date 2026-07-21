namespace ThreeInARow.Core;

public static partial class Game
{
    static Random r = new Random();
    static char[] symbols = { 'A', 'B', 'C', 'D', 'E', 'F' };

    public static BoardState InitializeGame(int boardSize)
    {
        return
            new Board(boardSize)
            .FillWithoutTriples()
            .ToStartState();
    }

    public static Board FillWithoutTriples(this Board board)
    {
        // Двойным циклом обходим все клетки доски
        for (int row = 0; row < board.size; row++)
            for (int col = 0; col < board.size; col++)
                board.cells[row, col] = new Element(board.PickSymbol(row, col));

        return board;
    }

    private static char PickSymbol(this Board board, int row, int col)
    {
        while (true)
        {
            char c = symbols[r.Next(symbols.Length)];
            if (!board.CreatesTriple(row, col, c)) return c;
        }
    }

    private static bool CreatesTriple(this Board board, int row, int col, char c)
    {
        // Проверяем двух соседей слева и двух сверху (они уже заполнены)
        bool left = col >= 2
            && board.cells[row, col - 1].Symbol == c
            && board.cells[row, col - 2].Symbol == c;

        bool up = row >= 2
            && board.cells[row - 1, col].Symbol == c
            && board.cells[row - 2, col].Symbol == c;

        return left || up;
    }

    public static BoardState ToStartState(this Board board)
    {
        return new BoardState(board, 0);
    }

    public static BoardState ProcessCascade(this BoardState state)
    {
        List<Match> matches = state.Board.FindMatches();

        if (matches.Count == 0)
            return state;

        return
            state.RemoveMatches(matches)
            .FillEmptySpaces()
            .ProcessCascade();
    }

    public static List<Match> FindMatches(this Board board)
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
                    matches.AddIfValid(row, startCol, col - startCol, MatchDirection.Horizontal);
                    startCol = col + 1;
                    continue;
                }

                // Проверяем совпадение символов для непустых ячеек
                if (board.cells[row, col].Symbol != board.cells[row, startCol].Symbol)
                {
                    matches.AddIfValid(row, startCol, col - startCol, MatchDirection.Horizontal);
                    startCol = col;
                }
                else if (col == board.size - 1)
                {
                    matches.AddIfValid(row, startCol, col - startCol + 1, MatchDirection.Horizontal);
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
                    matches.AddIfValid(startRow, col, row - startRow, MatchDirection.Vertical);
                    startRow = row + 1;
                    continue;
                }

                // Проверяем совпадение символов для непустых ячеек
                if (board.cells[row, col].Symbol != board.cells[startRow, col].Symbol)
                {
                    matches.AddIfValid(startRow, col, row - startRow, MatchDirection.Vertical);
                    startRow = row;
                }
                else if (row == board.size - 1)
                {
                    matches.AddIfValid(startRow, col, row - startRow + 1, MatchDirection.Vertical);
                }
            }
        }

        return matches;
    }

    private static void AddIfValid(this List<Match> matches, int row, int col,
            int length, MatchDirection direction)
    {
        // Учитываем только комбинации из 3 и более элементов (ТЗ)
        if (length >= 3)
        {
            matches.Add(new Match(direction, row, col, length));
        }
    }

    public static BoardState RemoveMatches(this BoardState currentState, List<Match> matches)
    {
        if (matches == null || matches.Count == 0)
            return currentState;

        int newScore = currentState.Score + matches.Sum(m => m.Length).CalculateScore();

        return            
            currentState.Board.cells.MarkForRemoval(matches)            
            .ApplyGravity(currentState.Board.size)            
            .ToBoard(currentState.Board.size)
            .ToState(newScore);
    }

    private static Element[,] MarkForRemoval(this Element[,] cells, List<Match> matches)
    {
        Element[,] newCells = (Element[,])cells.Clone();

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

    private static Element[,] ApplyGravity(this Element[,] cells, int size)
    {
        Element[,] newCells = new Element[size, size];

        for (int row = 0; row < size; row++)
            for (int col = 0; col < size; col++)
                newCells[row, col] = new Element { Symbol = Element.EMPTY };

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

    private static int CalculateScore(this int removedCount)
    {
        // Базовая система подсчета очков: 10 за каждый элемент
        return removedCount * 10;
    }


    public static BoardState FillEmptySpaces(this BoardState currentState)
    {
        if (currentState.Board.cells == null)
            return currentState;

        return currentState.Board.cells
            .FillRandom()
            .ToBoard(currentState.Board.size)
            .ToState(currentState.Score);
    }

    private static Element[,] FillRandom(this Element[,] cells)
    {
        Element[,] newCells = (Element[,])cells.Clone();

        for (int row = 0; row < newCells.GetLength(0); row++)
        {
            for (int col = 0; col < newCells.GetLength(1); col++)
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

        return newCells;
    }

    private static Board ToBoard(this Element[,] cells, int size)
    {
        return new Board { size = size, cells = cells };
    }

    private static BoardState ToState(this Board board, int score)
    {
        return new BoardState(board, score);
    }

    public static Board Draw(this Board board)
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
        return board;
    }

    public static BoardState Show(this BoardState state)
    {
        state.Board.Draw();
        Console.WriteLine("Score: " + state.Score);
        return state;
    }

    public static Board CloneBoard(this Board board)
    {
        Board b = new Board(board.size);
        for (int row = 0; row < board.size; row++)
            for (int col = 0; col < board.size; col++)
                b.cells[row, col] = board.cells[row, col];
        return b;
    }

    public static BoardState ReadMove(this BoardState bs)
    {
        Console.WriteLine(">");
        string input = Console.ReadLine();
        if (input == "q")
            Environment.Exit(0);

        return input
            .ParseCoords()
            .ApplyTo(bs);
    }

    private static int[] ParseCoords(this string input)
    {
        return input.Split(' ').Select(int.Parse).ToArray();
    }

    private static BoardState ApplyTo(this int[] c, BoardState bs)
    {
        Board board = bs.Board.CloneBoard();
        int x = c[1], y = c[0], x1 = c[3], y1 = c[2];
        Element e = board.cells[x, y];
        board.cells[x, y] = board.cells[x1, y1];
        board.cells[x1, y1] = e;
        return board.ToState(bs.Score);
    }

    public static BoardState PlayTurn(this BoardState state)
    {
        return state
            .Show()
            .ReadMove()
            .ProcessCascade();
    }
}