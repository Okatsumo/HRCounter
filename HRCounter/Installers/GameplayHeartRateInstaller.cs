using HRCounter.Configuration;
using HRCounter.Data;
using UnityEngine;
using Zenject;
using IPALogger = IPA.Logging.Logger;

namespace HRCounter.Installers;

public class GameplayHeartRateInstaller : Installer<GameplayHeartRateInstaller>
{
    [Inject]
    private readonly PluginConfig _config = null!;

    [Inject]
    private readonly IPALogger _logger = null!;

    [Inject]
    private readonly DataSourceManager _dataSourceManager = null!;

    public override void InstallBindings()
    {
        if (!_config.ModEnable)
        {
            return;
        }

        var source = _dataSourceManager.GetFromKey(_config.DataSource);
        if (source is null)
        {
            _logger.Error($"Unknown data source: {_config.DataSource}");
            return;
        }

        var sourceType = _dataSourceManager.GetDataSourceType(_config.DataSource);
        if (sourceType is null)
        {
            _logger.Error($"No data source type registered for: {_config.DataSource}");
            return;
        }

        if (!source.PreconditionMet())
        {
            _logger.Warn($"{source} precondition not met! Did you set your link/id/token or install the required dependencies?");
            return;
        }

        _logger.Debug("Binding BPM Downloader");
        if (typeof(Component).IsAssignableFrom(sourceType))
        {
            Container.BindInterfacesAndSelfTo(sourceType)
                .FromNewComponentOnNewGameObject()
                .WithGameObjectName($"HRCounter {source.Key} Data Source")
                .AsSingle();
        }
        else
        {
            Container.BindInterfacesAndSelfTo(sourceType).AsSingle();
        }

        _logger.Debug("Binding hr controller");
        Container.BindInterfacesAndSelfTo<HRDataManager>().AsSingle().NonLazy();
    }
}
