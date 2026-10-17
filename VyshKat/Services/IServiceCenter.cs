using VyshKat.Domain;

namespace VyshKat.Services;

public interface IServiceCenter
{
    bool Inspect(Transport transport);
}