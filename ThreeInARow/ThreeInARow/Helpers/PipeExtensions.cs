namespace ThreeInARow.Helpers;

public static class PipeExtensions
{
   public static TOut Pipe<TIn, TOut>(this TIn input, Func<TIn, TOut> func) => func(input);
}
