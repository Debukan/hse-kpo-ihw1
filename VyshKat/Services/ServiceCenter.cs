using VyshKat.Domain;

namespace VyshKat.Services;

public sealed class ServiceCenter : IServiceCenter
{
    public bool Inspect(Transport transport)
    {
        ArgumentNullException.ThrowIfNull(transport);
        return transport.IsOperational;
    }
}