using SourceMock.Model;

namespace SourceMock.Actions.Source
{
    /// <summary>
    /// Simulatetes the behaviour of a ZERA source.
    /// </summary>
    public class SimulatedSource : ISource, ISimulatedSource
    {
        #region ContructorAndDependencyInjection
        private readonly ILogger<SimulatedSource> _logger;
        private readonly IConfiguration _configuration;
        private readonly SourceCapabilities _sourceCapabilities;

        /// <summary>
        /// Constructor that injects logger and configuration and uses default source capablities.
        /// </summary>
        /// <param name="logger">The logger to be used.</param>
        /// <param name="configuration">The configuration o be used.</param>
        public SimulatedSource(ILogger<SimulatedSource> logger, IConfiguration configuration)
        {
            this._logger = logger;
            this._configuration = configuration;

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
            this._logger = logger;
            this._configuration = configuration;
            this._sourceCapabilities = sourceCapabilities;
        }

        #endregion

        private Loadpoint? _currentLoadpoint, _nextLoadpoint;
        private SimulatedSourceState? _simulatedSourceState;

        /// <inheritdoc/>
        public SourceResult SetLoadpoint(Loadpoint loadpoint)
        {
            var isValid = SourceCapabilityValidator.IsValid(loadpoint, _sourceCapabilities);

            if (isValid == SourceResult.SUCCESS)
            {
                _logger.LogTrace("Loadpoint set.");
                _nextLoadpoint = loadpoint;
            }

            return isValid;
        }

        /// <inheritdoc/>
        public SourceResult TurnOn()
        {
            if (_nextLoadpoint == null)
            {
                return SourceResult.NO_LOADPOINT_SET;
            }

            _logger.LogTrace("Source turned on.");
            _currentLoadpoint = _nextLoadpoint;
            return SourceResult.SUCCESS;
        }

        /// <inheritdoc/>
        public SourceResult TurnOff()
        {
            _logger.LogTrace("Source turned off.");
            _currentLoadpoint = null;
            return SourceResult.SUCCESS;
        }

        /// <inheritdoc/>
        public Loadpoint? GetNextLoadpoint()
        {
            return _nextLoadpoint;
        }

        /// <inheritdoc/>
        public Loadpoint? GetCurrentLoadpoint()
        {
            return _currentLoadpoint;
        }

        /// <inheritdoc/>
        public void SetSimulatedSourceState(SimulatedSourceState simulatedSourceState)
        {
            _simulatedSourceState = simulatedSourceState;
        }

        /// <inheritdoc/>
        public SimulatedSourceState? GetSimulatedSourceState()
        {
            return _simulatedSourceState;
        }

        public SourceCapabilities GetCapabilities()
        {
            return _sourceCapabilities;
        }
    }
}