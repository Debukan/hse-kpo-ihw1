using System.Globalization;
using VyshKat.ConsoleUi;
using VyshKat.Services;

namespace VyshKat.Tests;

public sealed class ConsoleApplicationTests
{
    [Fact]
    public void InvalidInput_IsRetriedAndCorrectScooterIsRegistered()
    {
        var (fleet, output) = Run("1\nне число\n1\n0\n101\n   \nСамокат\n11\n6\n2\n1\n-2\n2,5\n3\n4\n5\n0\n");

        Assert.Equal(1, fleet.TransportCount);
        Assert.Equal(2.5m, fleet.TotalDailyEnergy);
        Assert.Equal("Самокат", Assert.Single(fleet.GetBeginnerTransport()).Name);
        Assert.Contains("Наименование не должно быть пустым", output);
        Assert.Contains("Введите неотрицательное число", output);
        Assert.Contains("Суточный расход энергии: 2.5 кВт·ч", output);
        Assert.Contains("№ 101: Самокат", output);
    }

    [Fact]
    public void Menu_SupportsAllKindsRejectionAndDuplicates()
    {
        string input = "1\n2\n1\nВелосипед\n6\n1\n" +
                       "1\n3\n2\nЭлектровелосипед\n5\n1\n3.5\n" +
                       "1\n1\n3\nНеисправный\n8\n0\n5\n" +
                       "2\n1\n4\nШлем\n2\n2\n5\nЗарядка\n7,5\n" +
                       "2\n3\n6\nДок-станция\n2\n1\n1\nДубликат\n3\n4\n5\n0\n";

        var (fleet, output) = Run(input);

        Assert.Equal(2, fleet.TransportCount);
        Assert.Equal(3, fleet.EquipmentCount);
        Assert.Equal(11m, fleet.TotalDailyEnergy);
        Assert.Single(fleet.GetBeginnerTransport());
        Assert.Contains("Техосмотр не пройден", output);
        Assert.Contains("Инвентарный номер уже занят", output);
        Assert.Contains("№ 6: Док-станция", output);
        Assert.DoesNotContain("№ 3: Неисправный", output);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1\n1\n101\nСамокат\n6\n1\n")]
    [InlineData("2\n2\n101\nЗарядка\n")]
    public void EndOfInput_ExitsWithoutPartialRegistration(string input)
    {
        var (fleet, output) = Run(input);

        Assert.Empty(fleet.GetInventory());
        Assert.True(output.Contains("Работа завершена") || output.Contains("операция отменена"));
    }

    [Fact]
    public void EmptyReportsAndUnknownCommand_AreExplained()
    {
        var (_, output) = Run("99\n3\n4\n5\n0\n");

        Assert.Contains("Неизвестная команда", output);
        Assert.Contains("всего на балансе: 0", output);
        Assert.Contains("Подходящего транспорта нет", output);
        Assert.Contains("Баланс пуст", output);
    }

    [Fact]
    public void FileInput_UsesTheSameMenuAndProvidedData()
    {
        string path = Path.GetTempFileName();
        var fleet = new FleetService(new ServiceCenter());
        using var writer = new StringWriter();
        try
        {
            File.WriteAllText(path, "1\n1\n908\nСамокат из файла\n7\n1\n1,25\n3\n4\n5\n0\n");
            using var reader = File.OpenText(path);

            new ConsoleApplication(fleet, reader, writer).Run();

            Assert.Equal(1, fleet.TransportCount);
            Assert.Equal(1.25m, fleet.TotalDailyEnergy);
            Assert.Equal(908, Assert.Single(fleet.GetInventory()).InventoryNumber);
            Assert.Contains("Суточный расход энергии: 1.25 кВт·ч", writer.ToString());
            Assert.Contains("№ 908: Самокат из файла; простота 7/10", writer.ToString());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Summary_WhenEnergyOverflows_ShowsMessageAndContinuesMenu()
    {
        string maximumEnergy = decimal.MaxValue.ToString(CultureInfo.InvariantCulture);
        string input = $"2\n2\n1\nПервая станция\n{maximumEnergy}\n" +
                       "2\n2\n2\nВторая станция\n1\n3\n5\n0\n";

        var (fleet, output) = Run(input);

        Assert.Equal(2, fleet.EquipmentCount);
        Assert.Equal(2, fleet.GetInventory().Count);
        Assert.Contains("Суммарный расход энергии слишком велик для отображения.", output);
        Assert.DoesNotContain("Суточный расход энергии:", output);
        Assert.Contains("№ 1: Первая станция", output);
        Assert.Contains("№ 2: Вторая станция", output);
        Assert.Contains("Работа завершена.", output);
    }

    private static (FleetService Fleet, string Output) Run(string input)
    {
        var fleet = new FleetService(new ServiceCenter());
        using var reader = new StringReader(input);
        using var writer = new StringWriter();
        new ConsoleApplication(fleet, reader, writer).Run();
        return (fleet, writer.ToString());
    }
}
