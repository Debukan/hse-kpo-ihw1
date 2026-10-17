namespace VyshKat.Domain;

public abstract class Transport : InventoryItem
{
    public int BeginnerFriendliness { get; }
    public bool IsOperational { get; }

    protected Transport(int inventoryNumber, string name, int beginnerFriendliness,
        bool isOperational) : base(inventoryNumber, name)
    {
        if (beginnerFriendliness is < 1 or > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(beginnerFriendliness),
                "Простота для новичка должна быть от 1 до 10.");
        }

        BeginnerFriendliness = beginnerFriendliness;
        IsOperational = isOperational;
    }
}