namespace VyshKat.Domain;

public sealed class ChargingStation : Equipment, IEnergyConsumer
{
    public decimal DailyEnergyConsumption { get; }

    public ChargingStation(int inventoryNumber, string name, decimal dailyEnergyConsumption)
        : base(inventoryNumber, name)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(dailyEnergyConsumption);
        DailyEnergyConsumption = dailyEnergyConsumption;
    }
}