using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Demo.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Demo.WindowsService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private FileSystemWatcher _inboundFolderWatcher;
        private FileSystemWatcher _outputFolderWatcher;
        private readonly AppSettings _settings;
        private readonly IServiceProvider _services;

        public Worker(ILogger<Worker> logger, IOptions<AppSettings> settings, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    await Task.Delay(1000, stoppingToken);
            //}
            await Task.CompletedTask;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Service Starting");
                _logger.LogInformation("Binding Inbound Folder Events");
                _inboundFolderWatcher = new FileSystemWatcher(_settings.InputFolder, "*.TXT")
                {
                    NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName |
                                   NotifyFilters.DirectoryName
                };
                _inboundFolderWatcher.Created += InBound_OnChanged;

                _logger.LogInformation("Binding Outbound Folder Events");
                _outputFolderWatcher = new FileSystemWatcher(_settings.OutputFolder, "*.PDF")
                {
                    NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName |
                                   NotifyFilters.DirectoryName
                };
                _outputFolderWatcher.Created += Output_OnChanged;

                _inboundFolderWatcher.EnableRaisingEvents = true;
                _outputFolderWatcher.EnableRaisingEvents = true;

            }
            catch (Exception exception)
            {
                _logger.LogError("Error Encountered", exception);
            }

            return base.StartAsync(cancellationToken);
        }

        protected void InBound_OnChanged(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                _logger.LogInformation("InBound Change Event Triggered");
                _logger.LogInformation($"Converting Files in folder {_settings.InputFolder}");
                using (var scope = _services.CreateScope())
                {
                    var processor = scope.ServiceProvider.GetRequiredService<IProcessor>();
                    processor.Run();
                }
                _logger.LogInformation("Done with Inbound Change Event");
            }
        }

        protected void Output_OnChanged(object source, FileSystemEventArgs e)
        {
            _logger.LogInformation($"{e.ChangeType}");
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                _logger.LogInformation(e.FullPath);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Service");
            _inboundFolderWatcher.EnableRaisingEvents = false;
            _outputFolderWatcher.EnableRaisingEvents = false;
            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _logger.LogInformation("Disposing Service");
            _inboundFolderWatcher.EnableRaisingEvents = false;
            _outputFolderWatcher.EnableRaisingEvents = false;
            base.Dispose();
        }
    }
}
