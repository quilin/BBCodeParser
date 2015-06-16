namespace BBCodeParser
{
    internal interface IReadingStrategy
    {
        TagResult Read(string input);
    }
}