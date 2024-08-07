using Microsoft.Extensions.Logging;
using ZERA.WebSam.Shared.DomainSpecific;
using ZERA.WebSam.Shared.Models.Logging;
using SourceApi.Actions.Source;
using SourceApi.Model;

namespace SourceApi.Actions.VeinSource
{
    /// <summary>
    /// Communicates with a ZENUX/Vein Source
    /// </summary>
    public class VeinSource : ISource
    {
        private readonly LoadpointInfo _info = new();
        private readonly ILogger<VeinSource> _logger;
        private readonly VeinClient _veinClient;

        public VeinSource(ILogger<VeinSource> logger, VeinClient veinClient)
        {
            _logger = logger;
            _veinClient = veinClient;
        }

        public bool GetAvailable(IInterfaceLogger interfaceLogger) => true;

        public Task CancelDosage(IInterfaceLogger logger)
        {
            throw new NotImplementedException();
        }

        public LoadpointInfo GetActiveLoadpointInfo(IInterfaceLogger interfaceLogger) => _info;

        public Task<SourceCapabilities> GetCapabilities(IInterfaceLogger interfaceLogger) => Task.FromException<SourceCapabilities>(new NotImplementedException());

        public Task<DosageProgress> GetDosageProgress(IInterfaceLogger logger, MeterConstant meterConstant) => throw new NotImplementedException();

        public Task SetDosageEnergy(IInterfaceLogger logger, ActiveEnergy value, MeterConstant meterConstant) => throw new NotImplementedException();

        public Task SetDosageMode(IInterfaceLogger logger, bool on) => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task<SourceApiErrorCodes> SetLoadpoint(IInterfaceLogger logger, TargetLoadpoint loadpoint)
        {
            var veinRequest = VeinLoadpointMapper.ConvertToZeraJson(loadpoint);

            _logger.LogInformation(veinRequest.ToString());

            return Task.FromResult(SourceApiErrorCodes.SUCCESS);
        }

        public Task StartDosage(IInterfaceLogger logger) => throw new NotImplementedException();

        public Task<bool> CurrentSwitchedOffForDosage(IInterfaceLogger logger) => throw new NotImplementedException();

        public Task<SourceApiErrorCodes> TurnOff(IInterfaceLogger logger) => Task.FromException<SourceApiErrorCodes>(new NotImplementedException());

        public TargetLoadpoint? GetCurrentLoadpoint(IInterfaceLogger logger)
            => VeinLoadpointMapper.ConvertToLoadpoint(_veinClient.GetLoadpoint().Value);
    }
}