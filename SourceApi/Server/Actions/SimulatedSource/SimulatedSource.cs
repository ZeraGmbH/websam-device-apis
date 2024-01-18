using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SourceApi.Model;

namespace SourceApi.Actions.Source
{
    public interface ISimulatedSource : ISource
    {

    }
    /// <summary>
    /// Simulatetes the behaviour of a ZERA source.
    /// </summary>
    public class SimulatedSource : ISimulatedSource
    {
        #region ContructorAndDependencyInjection
        private readonly ILogger<SimulatedSource> _logger;
        private readonly IConfiguration _configuration;
        private readonly SourceCapabilities _sourceCapabilities;
        private readonly LoadpointInfo _info = new();
        private DosageProgress _status = new();
        private DateTime _startTime;
        private double _dosageEnergy;
        private bool _dosageMode = false;

        /// <summary>
        /// Constructor that injects logger and configuration and uses default source capablities.
        /// </summary>
        /// <param name="logger">The logger to be used.</param>
        /// <param name="configuration">The configuration o be used.</param>
        public SimulatedSource(ILogger<SimulatedSource> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            _sourceCapabilities = new()
            {
                Phases = new() {
                    new() {
                        Voltage = new(10, 300, 0.01),
                        Current = new(0, 60, 0.01)
                    },
                    new() {
                        Voltage = new(10, 300, 0.01),
                        Current = new(0, 60, 0.01)
                    },
                    new() {
                        Voltage = new(10, 300, 0.01),
                        Current = new(0, 60, 0.01)
                    }
                },
                FrequencyRanges = new() {
                    new(40, 60, 0.1, FrequencyMode.SYNTHETIC)
                }
            };
        }

        /// <summary>
        /// Constructor that injects logger, configuration and capabilities.
        /// </summary>
        /// <param name="logger">The logger to be used.</param>
        /// <param name="configuration">The configuration o be used.</param>
        /// <param name="sourceCapabilities">The capabilities of the source which should be simulated.</param>
        public SimulatedSource(ILogger<SimulatedSource> logger, IConfiguration configuration, SourceCapabilities sourceCapabilities)
        {
            _logger = logger;
            _configuration = configuration;
            _sourceCapabilities = sourceCapabilities;
        }

        #endregion

        private Loadpoint? _loadpoint;

        /// <inheritdoc/>
        public Task<SourceApiErrorCodes> SetLoadpoint(Loadpoint loadpoint)
        {
            var isValid = SourceCapabilityValidator.IsValid(loadpoint, _sourceCapabilities);

            if (isValid == SourceApiErrorCodes.SUCCESS)
            {
                _logger.LogTrace("Loadpoint set, source turned on.");
                _loadpoint = loadpoint;
            }

            _dosageMode = true;

            return Task.FromResult(isValid);
        }

        /// <inheritdoc/>
        public Task<SourceApiErrorCodes> TurnOff()
        {
            _logger.LogTrace("Source turned off.");
            _loadpoint = null;

            return Task.FromResult(SourceApiErrorCodes.SUCCESS);
        }

        /// <inheritdoc/>
        public Loadpoint? GetCurrentLoadpoint()
        {
            return _loadpoint;
        }

        public Task<SourceCapabilities> GetCapabilities()
        {
            return Task.FromResult(_sourceCapabilities);
        }

        public Task SetDosageMode(bool on)
        {
            _dosageMode = on;
            return Task.FromResult(_dosageMode);
        }

        public Task SetDosageEnergy(double value)
        {
            _dosageEnergy = value;
            return Task.FromResult(value);
        }

        public Task StartDosage()
        {
            _startTime = DateTime.Now;
            _status.Active = true;
            _dosageMode = false;

            return Task.FromResult(true);
        }

        public Task CancelDosage()
        {
            _status.Active = false;
            _status.Remaining = 0;
            return Task.FromResult(true);
        }

        public Task<DosageProgress> GetDosageProgress()
        {
            double power = 0;
            foreach (var phase in _loadpoint!.Phases)
            {
                power += phase.Voltage.Rms * phase.Current.Rms * Math.Cos((phase.Voltage.Angle - phase.Current.Angle) * Math.PI / 180d);
            }
            var timeInSeconds = (DateTime.Now - _startTime).TotalSeconds;

            double energy = power * timeInSeconds / 3600;

            if (energy > _dosageEnergy)
            {
                energy = StopDosage();
            }

            _status.Progress = energy;
            _status.Remaining = _dosageEnergy - energy;
            _status.Total = _dosageEnergy;

            return Task.FromResult(_status);
        }

        public Task<bool> CurrentSwitchedOffForDosage()
        {
            _logger.LogTrace("Mock switches off the current for dosage");
            return Task.FromResult(false);
        }

        public LoadpointInfo GetActiveLoadpointInfo() => _info;

        public Task<double[]> GetVoltageRanges() => Task.FromResult<double[]>([50d, 250d]);

        public Task<double[]> GetCurrentRanges() => Task.FromResult<double[]>([1d, 2d, 5d]);

        public bool Available => true;

        private double StopDosage()
        {
            double energy;
            _status.Active = false;
            energy = _dosageEnergy;
            _dosageMode = false;
            return energy;
        }
    }
}