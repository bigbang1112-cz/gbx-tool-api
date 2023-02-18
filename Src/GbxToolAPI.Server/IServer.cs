using Microsoft.Extensions.DependencyInjection;

namespace GbxToolAPI.Server;

public interface IServer
{
    void Services(IServiceCollection services);
}
