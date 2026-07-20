namespace ThreeInARow.Core
{
    public struct BoardState
    {
        public readonly Board Board;
        public readonly int Score;

        public BoardState(Board board, int score)
        {
            Board = board;
            Score = score;
        }
    }
}
