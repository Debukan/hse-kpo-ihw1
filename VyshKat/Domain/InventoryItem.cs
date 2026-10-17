namespace VyshKat.Domain;

public abstract class InventoryItem : IInventoryItem
{
    public int InventoryNumber { get; }
    public string Name { get; }

    protected InventoryItem(int inventoryNumber, string name)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(inventoryNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        InventoryNumber = inventoryNumber;
        Name = name.Trim();
    }
}