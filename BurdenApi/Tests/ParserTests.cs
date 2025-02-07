using BurdenApi.Models;

namespace BurdenApiTests;

[TestFixture]
public class ParserTests
{
    [TestCase("0x00", "0x00", 0, 0)]
    [TestCase("0x7f", "0x00", 127, 0)]
    [TestCase("0x00", "0x7f", 0, 127)]
    [TestCase("0x7f", "0x7f", 127, 127)]
    [TestCase("0x1A", "0x6C", 26, 108)]
    [TestCase("0xA", "0x3", 10, 3)]
    [TestCase("0x2", "0xb", 2, 11)]
    public void Can_Parse_Calibration_Pair(string coarse, string fine, byte expectedCoarse, byte expectedFine)
    {
        var pair = CalibrationPair.Parse(coarse, fine);

        Assert.Multiple(() =>
        {
            Assert.That(pair.Coarse, Is.EqualTo(expectedCoarse));
            Assert.That(pair.Fine, Is.EqualTo(expectedFine));
        });
    }

    [TestCase("0")]
    [TestCase("0x")]
    [TestCase("0x123")]
    [TestCase("1")]
    [TestCase("x1")]
    [TestCase("")]
    public void Can_Not_Parse_Calibration_Pair_Argument(string value)
    {
        Assert.Throws<ArgumentException>(() => CalibrationPair.Parse(value, "0x00"));
        Assert.Throws<ArgumentException>(() => CalibrationPair.Parse("0x00", value));
        Assert.Throws<ArgumentException>(() => CalibrationPair.Parse(value, value));
    }

    [TestCase("0xr")]
    [TestCase("0xrb")]
    public void Can_Not_Parse_Calibration_Pair_Format(string value)
    {
        Assert.Throws<FormatException>(() => CalibrationPair.Parse(value, "0x00"));
        Assert.Throws<FormatException>(() => CalibrationPair.Parse("0x00", value));
        Assert.Throws<FormatException>(() => CalibrationPair.Parse(value, value));
    }

    [TestCase("0xff")]
    [TestCase("0x80")]
    public void Can_Not_Parse_Calibration_Pair_Ranget(string value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CalibrationPair.Parse(value, "0x00"));
        Assert.Throws<ArgumentOutOfRangeException>(() => CalibrationPair.Parse("0x00", value));
        Assert.Throws<ArgumentOutOfRangeException>(() => CalibrationPair.Parse(value, value));
    }

    [Test]
    public void Can_Parse_Disabled_Calibration()
    {
        Assert.That(Calibration.Parse("0"), Is.Null);
    }

    [TestCase("2")]
    [TestCase("0;")]
    [TestCase("")]
    [TestCase("1;0x00;0x00;0x00;0x00")]
    [TestCase("2;0x71;0x2f;0x33;0x00;0.0000")]
    public void Can_Not_Parse_Calibration(string values)
    {
        Assert.Throws<ArgumentException>(() => Calibration.Parse(values));
    }

    [TestCase("1;0x71;0x2f;0x33;0x00;0.0000", 113, 47, 51, 0, "0.0000")]
    [TestCase("1;0x17;0x1a;0x22;0x5f;0.0000;5;9", 23, 26, 34, 95, "0.0000;5;9")]
    public void Can_Parse_Calibration(string values, byte rCoarse, byte rFine, byte lCoarse, byte lFine, string extra)
    {
        var calibration = Calibration.Parse(values);

        Assert.That(calibration, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(calibration.Item1.Resistive.Coarse, Is.EqualTo(rCoarse));
            Assert.That(calibration.Item1.Resistive.Fine, Is.EqualTo(rFine));
            Assert.That(calibration.Item1.Inductive.Coarse, Is.EqualTo(lCoarse));
            Assert.That(calibration.Item1.Inductive.Fine, Is.EqualTo(lFine));
            Assert.That(calibration.Item2, Is.EqualTo(extra));
        });
    }
}