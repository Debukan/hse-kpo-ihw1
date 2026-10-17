namespace VyshKat.Domain;

public abstract class Equipment : InventoryItem
{
    protected Equipment(int inventoryNumber, string name) : base(inventoryNumber, name)
    {
    }
}