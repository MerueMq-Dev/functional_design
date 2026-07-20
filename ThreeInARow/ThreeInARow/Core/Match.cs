namespace ThreeInARow.Core;

public struct Match
{
    public readonly MatchDirection Direction;
    public readonly int Row;
    public readonly int Col;
    public readonly int Length;

    public Match(MatchDirection direction, int row, int col, int length)
    {
        Direction = direction;
        Row = row;
        Col = col;
        Length = length;
    }
}
