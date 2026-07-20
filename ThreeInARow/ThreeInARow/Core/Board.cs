namespace ThreeInARow.Core;

public struct Board
{
    public int size;
    public Element[,] cells;
    public Board(int s)
    {
        size = s;
        cells = new Element[size, size];
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++) cells[x, y] = new Element(Element.EMPTY);
    }
}