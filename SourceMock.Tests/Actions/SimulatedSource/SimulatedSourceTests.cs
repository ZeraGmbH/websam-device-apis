
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Moq;

using SourceMock.Actions.Source;
using SourceMock.Model;
using SourceMock.Tests.Actions.LoadpointValidator;

namespace SourceMock.Tests.Actions.Source
{
    internal class SimulatedSourceTests
    {
        const string CONFIG_KEY_NUMBER_OF_PHASES = "SourceProperties:NumberOfPhases";

        #region PositiveTestCases
        [Test]
        [TestCaseSource(typeof(LoadpointValidatorTestData), nameof(LoadpointValidatorTestData.ValidLoadpoints))]
        public void TestValidTurnOn(Loadpoint loadpoint)
        {
            // Arrange
            var capabilities = SimulatedSourceTestData.GetSourceCapabilitiesForNumberOfPhases(loadpoint.Phases.Count);

            ISource source = GenerateSimulatedSource(capabilities: capabilities);

            // Act
            var result = source.SetLoadpoint(loadpoint);

            // Assert
            var currentLoadpoint = source.GetCurrentLoadpoint();

            Assert.AreEqual(SourceResult.SUCCESS, result);
            Assert.AreEqual(currentLoadpoint, currentLoadpoint);
        }

        [Test]
        [TestCaseSource(typeof(LoadpointValidatorTestData), nameof(LoadpointValidatorTestData.ValidLoadpoints))]
        public void TestValidTurnOff(Loadpoint loadpoint)
        {
            // Arrange
            var capabilities = SimulatedSourceTestData.GetSourceCapabilitiesForNumberOfPhases(loadpoint.Phases.Count);

            ISource source = GenerateSimulatedSource(capabilities: capabilities);

            source.SetLoadpoint(loadpoint);

            // Act
            var result = source.TurnOff();

            // Assert
            var currentLoadpoint = source.GetCurrentLoadpoint();

            Assert.AreEqual(SourceResult.SUCCESS, result);
            Assert.AreEqual(null, currentLoadpoint);
        }
        #endregion

        #region LoadpointIssues
        [Test]
        [TestCaseSource(typeof(SimulatedSourceTestData), nameof(SimulatedSourceTestData.ValidLoadpointsWithOneOrThreePhases))]
        public void TestTurnOnWithInvalidLoadpoint(Loadpoint loadpoint)
        {
            // Arrange
            var capabilities = SimulatedSourceTestData.DefaultTwoPhaseSourceCapabilities;

            ISource source = GenerateSimulatedSource(capabilities: capabilities);

            // Act
            var result = source.SetLoadpoint(loadpoint);

            // Assert
            var currentLoadpoint = source.GetCurrentLoadpoint();

            Assert.AreEqual(SourceResult.LOADPOINT_NOT_SUITABLE_DIFFERENT_NUMBER_OF_PHASES, result);
            Assert.AreEqual(null, currentLoadpoint);
        }
        #endregion

        #region CapabilityIssues
        [Test]
        public void TestTooHighVoltage()
        {
            // Arrange 
            ISource source = GenerateSimulatedSource();
            Loadpoint lp = LoadpointValidatorTestData.Loadpoint001_3AC_valid;
            lp.Phases[0].Voltage.Rms = 500;

            // Act
            var result = source.SetLoadpoint(lp);

            // Assert
            var currentLoadpoint = source.GetCurrentLoadpoint();

            Assert.AreEqual(SourceResult.LOADPOINT_NOT_SUITABLE_VOLTAGE_INVALID, result);
            Assert.AreEqual(null, currentLoadpoint);
        }

        [Test]
        public void TestTooHighCurrent()
        {
            // Arrange 
            ISource source = GenerateSimulatedSource();
            Loadpoint lp = LoadpointValidatorTestData.Loadpoint001_3AC_valid;
            lp.Phases[0].Current.Rms = 100;

            // Act
            var result = source.SetLoadpoint(lp);

            // Assert
            var currentLoadpoint = source.GetCurrentLoadpoint();

            Assert.AreEqual(SourceResult.LOADPOINT_NOT_SUITABLE_CURRENT_INVALID, result);
            Assert.AreEqual(null, currentLoadpoint);
        }
        #endregion

        #region HelperFunctions
        private static ISource GenerateSimulatedSource(Mock<ILogger<SimulatedSource>>? loggerMock = null, Mock<IConfiguration>? configMock = null, SourceCapabilities? capabilities = null)
        {
            loggerMock ??= new();
            configMock ??= new();

            if (capabilities == null)
                return new SimulatedSource(loggerMock.Object, configMock.Object);

            return new SimulatedSource(loggerMock.Object, configMock.Object, capabilities);
        }
    }
    #endregion
}
