using System.Text.Json.Serialization;

namespace SharedLibrary.DomainSpecific;

/// <summary>
/// Active energy (in Wh) as domain specific number.
/// </summary>
public readonly struct ActiveEnergy(double value) : IInternalDomainSpecificNumber<ActiveEnergy>
{
    /// <summary>
    /// The real value is always represented as a double.
    /// </summary>
    private readonly double _Value = value;

    /// <inheritdoc/>
    public double GetValue() => _Value;

    /// <inheritdoc/>
    [JsonIgnore]
    public readonly string Unit => "Wh";

    /// <summary>
    /// No energy at all.
    /// </summary>
    public static readonly ActiveEnergy Zero = new(0);

    /// <summary>
    /// Only explicit casting out the pure number is allowed.
    /// </summary>
    /// <param name="energy">Some active energy.</param>
    public static explicit operator double(ActiveEnergy energy) => energy._Value;

    /// <summary>
    /// Add to active energies.
    /// </summary>
    /// <param name="left">One energy.</param>
    /// <param name="right">Another energy.</param>
    /// <returns>New active energy instance representing the sum of the parameters.</returns>
    public static ActiveEnergy operator +(ActiveEnergy left, ActiveEnergy right) => new(left._Value + right._Value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static ActiveEnergy operator -(ActiveEnergy left, ActiveEnergy right) => new(left._Value - right._Value);

    /// <summary>
    /// Scale energy by a factor.
    /// </summary>
    /// <param name="energy">Some energy.</param>
    /// <param name="factor">Factor to apply to the energy.</param>
    /// <returns>New energy with scaled value.</returns>
    public static ActiveEnergy operator *(ActiveEnergy energy, double factor) => new(energy._Value * factor);

    /// <summary>
    /// Scale energy by a factor.
    /// </summary>
    /// <param name="energy">Some energy.</param>
    /// <param name="factor">Factor to apply to the energy.</param>
    /// <returns>New energy with scaled value.</returns>
    public static ActiveEnergy operator *(double factor, ActiveEnergy energy) => energy * factor;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="energy"></param>
    /// <param name="meterConstant"></param>
    /// <returns></returns>
    public static Impulses operator *(ActiveEnergy energy, MeterConstant meterConstant) => meterConstant * energy;

    /// <summary>
    /// Get time from power and energy
    /// </summary>
    /// <param name="power">Some energy.</param>
    /// <param name="energy">Some power.</param>
    /// <returns>time in seconds.</returns>
    public static Time? operator /(ActiveEnergy energy, ActivePower power) => (double)power == 0 ? null : new(energy._Value / (double)power * 3600);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static double operator /(ActiveEnergy left, ActiveEnergy right) => left._Value / right._Value;

    #region Comparable

    /// <summary>
    /// See if the number is exactly zero.
    /// </summary>
    /// <param name="number">Some number.</param>
    /// <returns>Set if number is zero.</returns>
    public static bool operator !(ActiveEnergy number) => number._Value == 0;

    /// <summary>
    /// Compare with any other object.
    /// </summary>
    /// <param name="obj">Some other object.</param>
    /// <returns>Set if this number is identical to the other object.</returns>
    public override bool Equals(object? obj) => obj is ActiveEnergy angle && _Value == angle._Value;

    /// <summary>
    /// Get a hashcode.
    /// </summary>
    /// <returns>Hashcode for this number.</returns>
    public override int GetHashCode() => _Value.GetHashCode();

    /// <summary>
    /// Compare to another number.
    /// </summary>
    /// <param name="other">The other number.</param>
    /// <returns>Comparision result of the number.</returns>
    public int CompareTo(ActiveEnergy other) => _Value.CompareTo(other._Value);

    /// <summary>
    /// Compare two numbers.
    /// </summary>
    /// <param name="left">First number.</param>
    /// <param name="right">Second number.</param>
    /// <returns>Set if the numbers are exactly identical.</returns>
    public static bool operator ==(ActiveEnergy left, ActiveEnergy right) => left._Value == right._Value;

    /// <summary>
    /// Compare two numbers.
    /// </summary>
    /// <param name="left">First number.</param>
    /// <param name="right">Second number.</param>
    /// <returns>Set if the numbers are not exactly identical.</returns>
    public static bool operator !=(ActiveEnergy left, ActiveEnergy right) => left._Value != right._Value;

    /// <summary>
    /// Compare two numbers.
    /// </summary>
    /// <param name="left">First number.</param>
    /// <param name="right">Second number.</param>
    /// <returns>Set if the first number is less than the second number.</returns>
    public static bool operator <(ActiveEnergy left, ActiveEnergy right) => left._Value < right._Value;

    /// <summary>
    /// Compare two numbers.
    /// </summary>
    /// <param name="left">First number.</param>
    /// <param name="right">Second number.</param>
    /// <returns>Set if the first number is not greater than the second number.</returns>
    public static bool operator <=(ActiveEnergy left, ActiveEnergy right) => left._Value <= right._Value;

    /// <summary>
    /// Compare two numbers.
    /// </summary>
    /// <param name="left">First number.</param>
    /// <param name="right">Second number.</param>
    /// <returns>Set if the first number is greater than the second number.</returns>
    public static bool operator >(ActiveEnergy left, ActiveEnergy right) => left._Value > right._Value;

    /// <summary>
    /// Compare two numbers.
    /// </summary>
    /// <param name="left">First number.</param>
    /// <param name="right">Second number.</param>
    /// <returns>Set if the first number is not less than the second number.</returns>
    public static bool operator >=(ActiveEnergy left, ActiveEnergy right) => left._Value >= right._Value;

    #endregion
}
