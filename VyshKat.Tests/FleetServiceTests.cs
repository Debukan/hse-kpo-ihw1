using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VyshKat.Domain;
using VyshKat.Services;

namespace VyshKat.Tests;

public sealed class FleetServiceTests
{
    [Theory]
    [InlineData(true, RegistrationResult.Accepted, 1)]
    [InlineData(false, RegistrationResult.RejectedByInspection, 0)]
    public void AddTransport_UsesInspectionDecisionFromDi(bool decision,
        RegistrationResult expected, int expectedCount)
    {
        var fake = new FakeServiceCenter(decision);
        using var provider = CreateProvider(fake);
        var fleet = provider.GetRequiredService<FleetService>();
        var scooter = new Scooter(1, "Самокат", 8, !decision, 2m);

        Assert.Equal(expected, fleet.AddTransport(scooter));

        Assert.Equal(expectedCount, fleet.TransportCount);
        Assert.Equal(decision ? 2m : 0m, fleet.TotalDailyEnergy);
        Assert.Equal(expectedCount, fleet.GetInventory().Count);
        Assert.Equal(expectedCount, fleet.GetBeginnerTransport().Count);
        Assert.Same(scooter, fake.LastInspected);
        Assert.Equal(1, fake.Calls);
    }

    [Fact]
    public void AddTransport_RejectedNumberCanBeUsedAgain()
    {
        var fleet = new FleetService(new ServiceCenter());
        Assert.Equal(RegistrationResult.RejectedByInspection,
            fleet.AddTransport(new Bicycle(7, "Неисправный", 6, false)));

        Assert.Equal(RegistrationResult.Accepted,
            fleet.AddTransport(new Bicycle(7, "Исправный", 6, true)));
        Assert.Equal("Исправный", Assert.Single(fleet.GetInventory()).Name);
    }

    [Fact]
    public void DuplicateTransport_DoesNotRunInspectionOrChangeState()
    {
        var fake = new FakeServiceCenter(true);
        var fleet = new FleetService(fake);
        fleet.AddTransport(new Scooter(1, "Первый", 8, true, 2m));

        var result = fleet.AddTransport(new Scooter(1, "Дубликат", 9, true, 5m));

        Assert.Equal(RegistrationResult.DuplicateInventoryNumber, result);
        Assert.Equal(1, fake.Calls);
        Assert.Equal(1, fleet.TotalCount);
        Assert.Equal(2m, fleet.TotalDailyEnergy);
        Assert.Equal("Первый", Assert.Single(fleet.GetInventory()).Name);
    }

    [Fact]
    public void InventoryNumbers_AreUniqueAcrossTransportAndEquipment()
    {
        var fake = new FakeServiceCenter(true);
        var fleet = new FleetService(fake);
        fleet.AddEquipment(new Helmet(1, "Шлем"));
        fleet.AddTransport(new Bicycle(2, "Велосипед", 6, true));

        Assert.Equal(RegistrationResult.DuplicateInventoryNumber,
            fleet.AddTransport(new Scooter(1, "Самокат", 8, true, 2m)));
        Assert.Equal(RegistrationResult.DuplicateInventoryNumber,
            fleet.AddEquipment(new ChargingStation(2, "Зарядка", 10m)));
        Assert.Equal(RegistrationResult.DuplicateInventoryNumber,
            fleet.AddEquipment(new Helmet(1, "Другой шлем")));
        Assert.Equal(2, fleet.TotalCount);
        Assert.Equal(1, fleet.TransportCount);
        Assert.Equal(1, fleet.EquipmentCount);
        Assert.Equal(0m, fleet.TotalDailyEnergy);
        Assert.Equal(1, fake.Calls);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(5, false)]
    [InlineData(6, true)]
    [InlineData(10, true)]
    public void GetBeginnerTransport_UsesInclusiveThreshold(int friendliness, bool included)
    {
        var fleet = new FleetService(new ServiceCenter());
        fleet.AddTransport(new Bicycle(1, "Велосипед", friendliness, true));
        fleet.AddEquipment(new Helmet(2, "Шлем"));

        Assert.Equal(included ? 1 : 0, fleet.GetBeginnerTransport().Count);
    }

    [Fact]
    public void Reports_IncludeAllAcceptedEnergyConsumersAndIgnoreRejectedTransport()
    {
        var fleet = new FleetService(new ServiceCenter());
        fleet.AddTransport(new Scooter(1, "Самокат", 8, true, 2.5m));
        fleet.AddTransport(new ElectricBicycle(2, "Электровелосипед", 5, true, 3m));
        fleet.AddTransport(new Bicycle(3, "Велосипед", 6, true));
        fleet.AddEquipment(new ChargingStation(4, "Зарядка", 7.5m));
        fleet.AddEquipment(new Helmet(5, "Шлем"));
        fleet.AddEquipment(new DockStation(6, "Док-станция"));
        fleet.AddTransport(new Scooter(7, "Неисправный", 10, false, 100m));

        Assert.Equal(13m, fleet.TotalDailyEnergy);
        Assert.Equal(3, fleet.TransportCount);
        Assert.Equal(3, fleet.EquipmentCount);
        Assert.Equal(6, fleet.TotalCount);
        Assert.Equal(new[] { 1, 3 }, fleet.GetBeginnerTransport().Select(t => t.InventoryNumber));
        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6 },
            fleet.GetInventory().Select(item => item.InventoryNumber));
    }

    [Fact]
    public void EmptyFleet_HasZeroReportsAndEmptyLists()
    {
        var fleet = new FleetService(new ServiceCenter());

        Assert.Equal(0, fleet.TotalCount);
        Assert.Equal(0, fleet.TransportCount);
        Assert.Equal(0, fleet.EquipmentCount);
        Assert.Equal(0m, fleet.TotalDailyEnergy);
        Assert.Empty(fleet.GetInventory());
        Assert.Empty(fleet.GetBeginnerTransport());
    }

    [Fact]
    public void NewTransportType_WorksWithoutChangingFleetService()
    {
        var fleet = new FleetService(new ServiceCenter());
        var skateboard = new ElectricSkateboard(3);

        Assert.Equal(RegistrationResult.Accepted, fleet.AddTransport(skateboard));
        Assert.Same(skateboard, Assert.Single(fleet.GetInventory()));
        Assert.Same(skateboard, Assert.Single(fleet.GetBeginnerTransport()));
        Assert.Equal(0.75m, fleet.TotalDailyEnergy);
        Assert.Equal(1, fleet.TransportCount);
    }

    [Fact]
    public void InventorySnapshot_CannotChangeFleetState()
    {
        var fleet = new FleetService(new ServiceCenter());
        fleet.AddEquipment(new Helmet(1, "Шлем"));
        var snapshot = fleet.GetInventory();
        fleet.AddEquipment(new DockStation(2, "Док-станция"));

        Assert.Single(snapshot);
        Assert.Equal(2, fleet.GetInventory().Count);
    }

    [Fact]
    public void ServiceRegistration_UsesOneFleetPerContainer()
    {
        using var provider = CreateProvider(new FakeServiceCenter(true));
        var first = provider.GetRequiredService<FleetService>();
        first.AddEquipment(new Helmet(1, "Шлем"));

        var second = provider.GetRequiredService<FleetService>();

        Assert.Same(first, second);
        Assert.Single(second.GetInventory());
    }

    [Fact]
    public void NullDependenciesAndItems_AreRejected()
    {
        Assert.Throws<ArgumentNullException>(() => new FleetService(null!));
        var fleet = new FleetService(new ServiceCenter());
        Assert.Throws<ArgumentNullException>(() => fleet.AddTransport(null!));
        Assert.Throws<ArgumentNullException>(() => fleet.AddEquipment(null!));
        Assert.Throws<ArgumentNullException>(() => new ServiceCenter().Inspect(null!));
    }

    private static ServiceProvider CreateProvider(IServiceCenter fake)
    {
        var services = new ServiceCollection();
        services.AddVyshKat();
        services.Replace(ServiceDescriptor.Singleton<IServiceCenter>(fake));
        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }

    private sealed class FakeServiceCenter : IServiceCenter
    {
        private readonly bool _decision;
        public int Calls { get; private set; }
        public Transport? LastInspected { get; private set; }

        public FakeServiceCenter(bool decision) => _decision = decision;

        public bool Inspect(Transport transport)
        {
            Calls++;
            LastInspected = transport;
            return _decision;
        }
    }

    private sealed class ElectricSkateboard : Transport, IEnergyConsumer
    {
        public decimal DailyEnergyConsumption => 0.75m;

        public ElectricSkateboard(int number) : base(number, "Электроскейтборд", 7, true)
        {
        }
    }
}