namespace BBCodeParser
{
    public interface IBBParser
    {
        string Parse(string input, DirectionMode directionMode);
    }
}