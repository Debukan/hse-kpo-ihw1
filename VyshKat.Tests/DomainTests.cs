using VyshKat.Domain;
using VyshKat.Services;

namespace VyshKat.Tests;

public sealed class DomainTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void NonPositiveInventoryNumber_IsRejected(int number)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Helmet(number, "Шлем"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Bicycle(number, "Велосипед", 6, true));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyName_IsRejected(string? name)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Helmet(1, name!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void FriendlinessOutsideScale_IsRejected(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Bicycle(1, "Велосипед", value, true));
    }

    [Fact]
    public void NegativeEnergy_IsRejectedByEveryElectricType()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Scooter(1, "Самокат", 6, true, -1m));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ElectricBicycle(1, "Велосипед", 6, true, -1m));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChargingStation(1, "Зарядка", -1m));
    }

    [Fact]
    public void ZeroEnergy_IsAllowedAndNamesAreTrimmed()
    {
        Assert.Equal(0m, new Scooter(1, "Самокат", 1, true, 0m).DailyEnergyConsumption);
        Assert.Equal(0m, new ElectricBicycle(2, "Велосипед", 10, true, 0m).DailyEnergyConsumption);
        var station = new ChargingStation(3, "  Зарядка  ", 0m);
        Assert.Equal(0m, station.DailyEnergyConsumption);
        Assert.Equal("Зарядка", station.Name);
        Assert.Equal(3, station.InventoryNumber);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ServiceCenter_AcceptsOnlyOperationalTransport(bool operational)
    {
        var center = new ServiceCenter();
        Assert.Equal(operational, center.Inspect(new Bicycle(1, "Велосипед", 6, operational)));
    }
}