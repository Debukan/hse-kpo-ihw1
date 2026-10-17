using System.Globalization;
using VyshKat.Domain;
using VyshKat.Services;

namespace VyshKat.ConsoleUi;

public sealed class ConsoleApplication
{
    private readonly FleetService _fleet;
    private readonly ConsoleInput _input;
    private readonly TextWriter _output;

    public ConsoleApplication(FleetService fleet, TextReader input, TextWriter output)
    {
        _fleet = fleet;
        _input = new ConsoleInput(input, output);
        _output = output;
    }

    public void Run()
    {
        _output.WriteLine("ВышКат — учёт парка. Данные хранятся до завершения программы.");
        while (true)
        {
            _output.WriteLine("\n1 — Добавить транспорт\n2 — Добавить вещь\n3 — Отчёт" +
                              "\n4 — Транспорт для новичков\n5 — Инвентаризация\n0 — Выход");
            _output.Write("Выберите действие: ");
            string? command = _input.ReadCommand();
            if (command is null or "0")
            {
                _output.WriteLine("Работа завершена.");
                return;
            }

            try
            {
                switch (command)
                {
                    case "1": AddTransport(); break;
                    case "2": AddEquipment(); break;
                    case "3": ShowSummary(); break;
                    case "4": ShowBeginnerTransport(); break;
                    case "5": ShowInventory(); break;
                    default: _output.WriteLine("Неизвестная команда. Введите число от 0 до 5."); break;
                }
            }
            catch (EndOfStreamException)
            {
                _output.WriteLine("\nВвод завершён. Незавершённая операция отменена.");
                return;
            }
        }
    }

    private void AddTransport()
    {
        int kind = _input.ReadInt("Тип: 1 — электросамокат, 2 — велосипед, 3 — электровелосипед: ", 1, 3);
        int number = _input.ReadInt("Инвентарный номер (положительное целое): ", 1, int.MaxValue);
        string name = _input.ReadName();
        int friendliness = _input.ReadInt("Простота для новичка (1–10): ", 1, 10);
        bool operational = _input.ReadInt("Технически исправен? 1 — да, 0 — нет: ", 0, 1) == 1;
        decimal energy = kind == 2 ? 0 : _input.ReadEnergy();

        Transport transport = kind switch
        {
            1 => new Scooter(number, name, friendliness, operational, energy),
            2 => new Bicycle(number, name, friendliness, operational),
            _ => new ElectricBicycle(number, name, friendliness, operational, energy)
        };
        WriteResult(_fleet.AddTransport(transport));
    }

    private void AddEquipment()
    {
        int kind = _input.ReadInt("Тип: 1 — шлем, 2 — зарядная станция, 3 — док-станция: ", 1, 3);
        int number = _input.ReadInt("Инвентарный номер (положительное целое): ", 1, int.MaxValue);
        string name = _input.ReadName();
        Equipment equipment = kind switch
        {
            1 => new Helmet(number, name),
            2 => new ChargingStation(number, name, _input.ReadEnergy()),
            _ => new DockStation(number, name)
        };
        WriteResult(_fleet.AddEquipment(equipment));
    }

    private void WriteResult(RegistrationResult result)
    {
        _output.WriteLine(result switch
        {
            RegistrationResult.Accepted => "Объект принят на баланс.",
            RegistrationResult.RejectedByInspection => "Техосмотр не пройден. Транспорт отклонён.",
            _ => "Инвентарный номер уже занят. Объект не добавлен."
        });
    }

    public void ShowSummary()
    {
        _output.WriteLine($"Транспорт: {_fleet.TransportCount}; вещи: {_fleet.EquipmentCount}; " +
                          $"всего на балансе: {_fleet.TotalCount}.");
        try
        {
            _output.WriteLine($"Суточный расход энергии: {FormatEnergy(_fleet.TotalDailyEnergy)} кВт·ч.");
        }
        catch (OverflowException)
        {
            _output.WriteLine("Суммарный расход энергии слишком велик для отображения.");
        }
    }

    public void ShowBeginnerTransport()
    {
        var transport = _fleet.GetBeginnerTransport();
        _output.WriteLine("Транспорт для новичков (простота ≥ 6):");
        if (transport.Count == 0)
        {
            _output.WriteLine("Подходящего транспорта нет.");
        }

        foreach (var item in transport)
        {
            _output.WriteLine($"№ {item.InventoryNumber}: {item.Name}; простота {item.BeginnerFriendliness}/10.");
        }
    }

    public void ShowInventory()
    {
        var inventory = _fleet.GetInventory();
        _output.WriteLine("Инвентаризация:");
        if (inventory.Count == 0)
        {
            _output.WriteLine("Баланс пуст.");
        }

        foreach (var item in inventory)
        {
            string category = item is Transport ? "Транспорт" : "Вещь";
            string energy = item is IEnergyConsumer consumer
                ? $"; {FormatEnergy(consumer.DailyEnergyConsumption)} кВт·ч/сутки"
                : "";
            _output.WriteLine($"№ {item.InventoryNumber}: {item.Name} [{category}]{energy}");
        }
    }

    private static string FormatEnergy(decimal value) => value.ToString("0.##", CultureInfo.InvariantCulture);
}
