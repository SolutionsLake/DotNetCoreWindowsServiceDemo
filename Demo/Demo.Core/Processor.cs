using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Demo.Core
{
    public interface IProcessor
    {
        void Run();
    }

    public class Processor : IProcessor
    {
        private readonly ILogger<Processor> _logger;
        private readonly AppSettings _settings;

        public Processor(ILogger<Processor> logger, IOptions<AppSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
        }

        public void Run()
        {
            _logger.LogInformation("Begin Process");
            _logger.LogInformation(_settings.InputFolder);
        }
    }
}
