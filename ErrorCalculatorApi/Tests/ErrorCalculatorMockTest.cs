using ErrorCalculatorApi.Actions.Device;
using ErrorCalculatorApi.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SourceApi.Actions.Source;
using SourceApi.Model;

namespace ErrorCalculatorApiTests;

public class ErrorCalculatorMockTest
{
    private ServiceProvider Services;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();

        Mock<ISource> sourceMock = new();

        /* Total energy is 66kW. */
        Loadpoint loadpoint = new()
        {
            Phases = {
                new(){
                    Current = new(){Angle=0, Rms=100, On=true},
                    Voltage = new(){Angle=0, Rms=220, On=true}
                },
                new(){
                    Current = new(){Angle=120, Rms=100, On=true},
                    Voltage = new(){Angle=120, Rms=220, On=true}
                },
                new(){
                    Current = new(){Angle=240, Rms=100, On=true},
                    Voltage = new(){Angle=240, Rms=220, On=true}
                }
            }
        };

        sourceMock.Setup(s => s.GetCurrentLoadpoint()).Returns(loadpoint);

        services.AddSingleton(sourceMock.Object);

        services.AddTransient<IErrorCalculatorMock, ErrorCalculatorMock>();

        Services = services.BuildServiceProvider();
    }

    [TearDown]
    public void Teardown()
    {
        Services?.Dispose();
    }

    [Test]
    public async Task Returns_Correct_Error_Status()
    {
        var cut = Services.GetRequiredService<IErrorCalculatorMock>();

        /* 200 impulses at 10000/kWh is equivalent to 20. */
        await cut.SetErrorMeasurementParameters(10000, 200);
        await cut.StartErrorMeasurement(false);

        /* 100ms delay generates ~1.8W. */
        Thread.Sleep(100);

        var result = await cut.GetErrorStatus();

        Assert.Multiple(() =>
        {
            Assert.That(result.State, Is.EqualTo(ErrorMeasurementStates.Running));
            Assert.That(result.ErrorValue, Is.Null);
            Assert.That(result.Energy, Is.GreaterThan(0));
        });

        /* 1.5s delay generates 27.5W. */
        Thread.Sleep(1500);

        result = await cut.GetErrorStatus();

        Assert.Multiple(() =>
        {
            Assert.That(result.State, Is.EqualTo(ErrorMeasurementStates.Finished));
            Assert.That(result.ErrorValue, Is.GreaterThanOrEqualTo(-5).And.LessThanOrEqualTo(+7));
            Assert.That(result.Energy, Is.EqualTo(20));
        });
    }

    [Test]
    public async Task Can_Do_Continuous_Measurement()
    {
        var cut = Services.GetRequiredService<IErrorCalculatorMock>();

        /* 200 impulses at 10000/kWh is equivalent to 20W. */
        await cut.SetErrorMeasurementParameters(10000, 200);
        await cut.StartErrorMeasurement(true);

        /* 1.5s delay generates 27.5W. */
        Thread.Sleep(1500);

        var result = await cut.GetErrorStatus();
        var error = result.ErrorValue;

        Assert.Multiple(() =>
        {
            Assert.That(result.State, Is.EqualTo(ErrorMeasurementStates.Running));
            Assert.That(error, Is.GreaterThanOrEqualTo(-5).And.LessThanOrEqualTo(+7));
            Assert.That(result.Energy, Is.EqualTo(20));
        });

        Thread.Sleep(100);

        result = await cut.GetErrorStatus();

        /* There should be no full cycle be done and the error value keeps its value. */
        Assert.That(error, Is.EqualTo(result.ErrorValue));

        /* 1.5s delay generates 27.5W. */
        Thread.Sleep(1500);

        result = await cut.GetErrorStatus();

        /* Now the second iteration should be complete and the error value "should" change - indeed there is a tin chance of random generation clashes. */
        Assert.That(error, Is.Not.EqualTo(result.ErrorValue));
    }
}
