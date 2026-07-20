using ThreeInARow.Core;

BoardState bs = Game.InitializeGame();
while (true)
{
    Game.Draw(bs.Board);
    Console.WriteLine("Score: " + bs.Score);
    bs = Game.ReadMove(bs);
    bs = Game.ProcessCascade(bs);
}