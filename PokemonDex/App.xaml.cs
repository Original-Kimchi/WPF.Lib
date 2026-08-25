using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PokemonDex.Core.Services;
using PokemonDex.Infrastructure;
using PokemonDex.ViewModels;

namespace PokemonDex;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var services = new ServiceCollection();
        services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri("https://pokeapi.co/api/v2/"),
            Timeout = TimeSpan.FromSeconds(30)
        });
        services.AddSingleton<IPokemonService, PokeApiPokemonService>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();
        _serviceProvider = services.BuildServiceProvider(
            new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        MainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        MainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        MainWindow = null;
        _serviceProvider?.Dispose();
        _serviceProvider = null;
        base.OnExit(e);
    }
}
