namespace VyshKat.Domain;

public sealed class Bicycle : Transport
{
    public Bicycle(int inventoryNumber, string name, int beginnerFriendliness,
        bool isOperational) : base(inventoryNumber, name, beginnerFriendliness, isOperational)
    {
    }
}