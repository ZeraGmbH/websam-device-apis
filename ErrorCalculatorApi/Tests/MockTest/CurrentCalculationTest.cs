using ErrorCalculatorApi.Actions.Device;

namespace MockTest.CurrentCalculationTest;

public class CurrentCalculationTest
{

    [Test]
    public void Calculates_Dc_Current()
    {
        double current = 10;
        double voltage = 210;
        var expectedPower = current * voltage;

        var dcPhase = TestLoadpoints.GetDcLoadpointPhase();
        var actualPower = CurrentCalculation.CalculateDcPower(0, dcPhase);

        Assert.That(expectedPower, Is.EqualTo(actualPower));
    }

    [Test]
    public void Calculates_Ac_Current()
    {
        double current = 10;
        double voltage = 210;
        var expectedPower = current * voltage;  // ignore cos, since both angles are 0

        var dcPhase = TestLoadpoints.GetAcLoadpointPhase();
        var actualPower = CurrentCalculation.CalculateAcPower(0, dcPhase);

        Assert.That(expectedPower, Is.EqualTo(actualPower));
    }

    [Test]
    public void Not_Calculates_Ac_Current()
    {
        var expectedPower = 0;

        var acPhase = TestLoadpoints.GetAcLoadpointPhase();
        var actualPower = CurrentCalculation.CalculateDcPower(0, acPhase);

        Assert.That(expectedPower, Is.EqualTo(actualPower));
    }

    [Test]
    public void Not_Calculates_Dc_Current()
    {
        var expectedPower = 0;

        var dcPhase = TestLoadpoints.GetDcLoadpointPhase();
        var actualPower = CurrentCalculation.CalculateAcPower(0, dcPhase);

        Assert.That(expectedPower, Is.EqualTo(actualPower));
    }
}