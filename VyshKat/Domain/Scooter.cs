namespace VyshKat.Domain;

public sealed class Scooter : Transport, IEnergyConsumer
{
    public decimal DailyEnergyConsumption { get; }

    public Scooter(int inventoryNumber, string name, int beginnerFriendliness,
        bool isOperational, decimal dailyEnergyConsumption)
        : base(inventoryNumber, name, beginnerFriendliness, isOperational)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(dailyEnergyConsumption);
        DailyEnergyConsumption = dailyEnergyConsumption;
    }
}