using Microsoft.Extensions.DependencyInjection;

namespace Domain.App.Core.FastFailing;

public class FailFastTask
{
    private readonly IServiceCollection _services;
    private readonly IServiceProvider _provider;


    public FailFastTask(IServiceCollection services, IServiceProvider provider)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }


    public void Execute()
    {
        using IServiceScope scope = _provider.CreateScope();

        foreach (Type serviceType in FailFastTask.GetServices(_services))
        {
            scope.ServiceProvider.GetServices(serviceType);
        }
    }


    private static IEnumerable<Type> GetServices(IServiceCollection services)
    {
        return services
            .Where(descriptor => descriptor.ImplementationType != typeof(FailFastTask))
            .Where(descriptor => !descriptor.ServiceType.ContainsGenericParameters)
            .Select(descriptor => descriptor.ServiceType)
            .Distinct();
    }
}