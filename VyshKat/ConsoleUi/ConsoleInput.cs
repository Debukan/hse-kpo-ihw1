using System.Globalization;

namespace VyshKat.ConsoleUi;

internal sealed class ConsoleInput
{
    private readonly TextReader _reader;
    private readonly TextWriter _writer;

    public ConsoleInput(TextReader reader, TextWriter writer)
    {
        _reader = reader;
        _writer = writer;
    }

    public string? ReadCommand() => _reader.ReadLine()?.Trim();

    public int ReadInt(string prompt, int minimum, int maximum)
    {
        while (true)
        {
            _writer.Write(prompt);
            if (int.TryParse(ReadLine(), NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out int value) && value >= minimum && value <= maximum)
            {
                return value;
            }

            _writer.WriteLine($"Введите целое число от {minimum} до {maximum}.");
        }
    }

    public string ReadName()
    {
        while (true)
        {
            _writer.Write("Наименование: ");
            string name = ReadLine().Trim();
            if (name.Length > 0)
            {
                return name;
            }

            _writer.WriteLine("Наименование не должно быть пустым.");
        }
    }

    public decimal ReadEnergy()
    {
        while (true)
        {
            _writer.Write("Суточный расход энергии, кВт·ч (например, 2,5): ");
            string text = ReadLine().Trim().Replace(',', '.');
            if (decimal.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out decimal energy) && energy >= 0)
            {
                return energy;
            }

            _writer.WriteLine("Введите неотрицательное число с точкой или запятой.");
        }
    }

    private string ReadLine() => _reader.ReadLine() ?? throw new EndOfStreamException();
}