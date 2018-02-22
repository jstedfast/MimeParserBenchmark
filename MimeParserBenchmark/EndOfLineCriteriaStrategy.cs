namespace MimeParserBenchmark
{
    public class EndOfLineCriteriaStrategy : MIMER.IEndCriteriaStrategy
    {
        public bool IsEndReached(char[] data, int size)
        {
            return data[0] == '\n';
        }
    }
}