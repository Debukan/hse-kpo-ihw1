using VyshKat.Domain;

namespace VyshKat.Services;

public sealed class FleetService
{
    public const int MinimumBeginnerFriendliness = 6;

    private readonly IServiceCenter _serviceCenter;
    private readonly Dictionary<int, IInventoryItem> _items = new();

    public FleetService(IServiceCenter serviceCenter)
    {
        ArgumentNullException.ThrowIfNull(serviceCenter);
        _serviceCenter = serviceCenter;
    }

    public int TransportCount => _items.Values.OfType<Transport>().Count();
    public int EquipmentCount => _items.Values.OfType<Equipment>().Count();
    public int TotalCount => _items.Count;

    public decimal TotalDailyEnergy => _items.Values.OfType<IEnergyConsumer>()
        .Sum(item => item.DailyEnergyConsumption);

    public RegistrationResult AddTransport(Transport transport)
    {
        ArgumentNullException.ThrowIfNull(transport);
        if (_items.ContainsKey(transport.InventoryNumber))
        {
            return RegistrationResult.DuplicateInventoryNumber;
        }

        if (!_serviceCenter.Inspect(transport))
        {
            return RegistrationResult.RejectedByInspection;
        }

        _items.Add(transport.InventoryNumber, transport);
        return RegistrationResult.Accepted;
    }

    public RegistrationResult AddEquipment(Equipment equipment)
    {
        ArgumentNullException.ThrowIfNull(equipment);
        return _items.TryAdd(equipment.InventoryNumber, equipment)
            ? RegistrationResult.Accepted
            : RegistrationResult.DuplicateInventoryNumber;
    }

    public IReadOnlyList<IInventoryItem> GetInventory() => _items.Values
        .OrderBy(item => item.InventoryNumber).ToArray();

    public IReadOnlyList<Transport> GetBeginnerTransport() => _items.Values
        .OfType<Transport>()
        .Where(transport => transport.BeginnerFriendliness >= MinimumBeginnerFriendliness)
        .OrderBy(transport => transport.InventoryNumber).ToArray();
}