using System.Text;
using Microsoft.Extensions.DependencyInjection;
using VyshKat.ConsoleUi;
using VyshKat.Services;

Console.OutputEncoding = Encoding.UTF8;

if (args.Length != 0 && (args.Length != 2 || args[0] != "--input" || string.IsNullOrWhiteSpace(args[1])))
{
    Console.Error.WriteLine("Запуск: dotnet run --project VyshKat [-- --input <путь к файлу>]");
    return 1;
}

StreamReader? fileInput = null;

try
{
    if (args.Length == 2)
    {
        fileInput = File.OpenText(args[1]);
    }
}
catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
{
    Console.Error.WriteLine($"Не удалось открыть файл ввода: {exception.Message}");
    return 1;
}

using var inputFile = fileInput;
var services = new ServiceCollection();

services.AddVyshKat();
services.AddSingleton<TextReader>(inputFile ?? (TextReader)Console.In);
services.AddSingleton<TextWriter>(Console.Out);
services.AddSingleton<ConsoleApplication>();

using var provider = services.BuildServiceProvider(new ServiceProviderOptions
{
    ValidateOnBuild = true,
    ValidateScopes = true
});
provider.GetRequiredService<ConsoleApplication>().Run();

return 0;