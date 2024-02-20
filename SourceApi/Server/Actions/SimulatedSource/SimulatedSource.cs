using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SourceApi.Model;

namespace SourceApi.Actions.Source
{
    public interface ISourceMock : ISource
    {

    }
    /// <summary>
    /// Simulatetes the behaviour of a ZERA source.
    /// </summary>
    public class SimulatedSource : ISourceMock, ISimulatedSource
    {
        #region ContructorAndDependencyInjection
        private readonly ILogger<SimulatedSource> _logger;
        private readonly IConfiguration _configuration;
        private readonly SourceCapabilities _sourceCapabilities;
        private LoadpointInfo _info = new();
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

        private SimulatedSourceState? _simulatedSourceState;
        private Loadpoint? _loadpoint;

        /// <inheritdoc/>
        public Task<SourceApiErrorCodes> SetLoadpoint(Loadpoint loadpoint)
        {
            var isValid = SourceCapabilityValidator.IsValid(loadpoint, _sourceCapabilities);

            if (isValid == SourceApiErrorCodes.SUCCESS)
            {
                _logger.LogTrace("Loadpoint set, source turned on.");
                _loadpoint = loadpoint;
                _info.IsActive = CheckHasActivePhase();
            }

            _dosageMode = false;

            _info.SavedAt = _info.ActivatedAt = DateTime.Now;

            return Task.FromResult(isValid);
        }

        /// <inheritdoc/>
        public Task<SourceApiErrorCodes> TurnOff()
        {
            _logger.LogTrace("Source turned off.");

            _info = new()
            {
                IsActive = false
            };

            if (_loadpoint != null)
                foreach (var phase in _loadpoint.Phases)
                {
                    phase.Current.On = false;
                    phase.Voltage.On = false;
                }

            return Task.FromResult(SourceApiErrorCodes.SUCCESS);
        }

        /// <inheritdoc/>
        public Loadpoint? GetCurrentLoadpoint() => _loadpoint;

        public Task<SourceCapabilities> GetCapabilities() => Task.FromResult(_sourceCapabilities);

        public Task SetDosageMode(bool on)
        {
            _dosageMode = on;

            return Task.CompletedTask;
        }

        public Task SetDosageEnergy(double value)
        {
            _dosageEnergy = value;

            return Task.CompletedTask;
        }

        public Task StartDosage()
        {
            _startTime = DateTime.Now;
            _status.Active = true;
            _dosageMode = false;

            return Task.CompletedTask;
        }

        public Task CancelDosage()
        {
            _status.Active = false;
            _status.Remaining = 0;

            return Task.CompletedTask;
        }

        public Task<DosageProgress> GetDosageProgress()
        {
            var power = 0d;

            foreach (var phase in _loadpoint!.Phases)
                if (phase.Voltage.On && phase.Current.On)
                    power += phase.Voltage.Rms * phase.Current.Rms * Math.Cos((phase.Voltage.Angle - phase.Current.Angle) * Math.PI / 180d);

            var elapsedHours = (DateTime.Now - _startTime).TotalHours;
            var energy = power * elapsedHours;

            if (energy > _dosageEnergy) energy = StopDosage();

            _status.Progress = energy;
            _status.Remaining = _dosageEnergy - energy;
            _status.Total = _dosageEnergy;

            return Task.FromResult(new DosageProgress
            {
                Active = _status.Active,
                Progress = _status.Progress,
                Remaining = _status.Remaining,
                Total = _status.Total
            });
        }

        public Task<bool> CurrentSwitchedOffForDosage()
        {
            _logger.LogTrace("Mock switches off the current for dosage");

            return Task.FromResult(_dosageMode);
        }

        public LoadpointInfo GetActiveLoadpointInfo() => _info;

        public Task<double[]> GetVoltageRanges() => Task.FromResult<double[]>([50d, 250d]);

        public Task<double[]> GetCurrentRanges() => Task.FromResult<double[]>([1d, 2d, 5d]);

        public bool Available => true;

        /// <inheritdoc/>
        public void SetSimulatedSourceState(SimulatedSourceState simulatedSourceState) =>
            _simulatedSourceState = simulatedSourceState;

        /// <inheritdoc/>
        public SimulatedSourceState? GetSimulatedSourceState() => _simulatedSourceState;

        private double StopDosage()
        {
            _status.Active = false;

            _dosageMode = false;

            return _dosageEnergy;
        }

        private bool CheckHasActivePhase()
        {
            if (_loadpoint != null)
                foreach (var phase in _loadpoint.Phases)
                {
                    if (phase.Current.On == true)
                        return true;
                    if (phase.Voltage.On == true)
                        return true;
                }
            return false;
        }
    }
}