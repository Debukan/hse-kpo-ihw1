namespace VyshKat.Domain;

public sealed class ElectricBicycle : Transport, IEnergyConsumer
{
    public decimal DailyEnergyConsumption { get; }

    public ElectricBicycle(int inventoryNumber, string name, int beginnerFriendliness,
        bool isOperational, decimal dailyEnergyConsumption)
        : base(inventoryNumber, name, beginnerFriendliness, isOperational)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(dailyEnergyConsumption);
        DailyEnergyConsumption = dailyEnergyConsumption;
    }
}