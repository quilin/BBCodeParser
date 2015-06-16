namespace BBCodeParser
{
    internal class Reader
    {
        private readonly IReadingStrategy readingStrategy;

        public Reader(IReadingStrategy readingStrategy)
        {
            this.readingStrategy = readingStrategy;
        }

        public TagResult Read(string input)
        {
            return readingStrategy.Read(input);
        }
    }
}