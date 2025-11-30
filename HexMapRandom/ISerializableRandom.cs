namespace HexMapRandom;

/// <summary>
/// Represents a serializable random number generator that produces deterministic sequences.
/// </summary>
/// <remarks>
/// This interface is NOT thread-safe. If multiple threads access the same instance,
/// external synchronization is required to ensure consistent behavior.
/// Designed for turn-based games where deterministic random sequences are needed.
/// </remarks>
public interface ISerializableRandom
{
    /// <summary>
    /// Returns a non-negative random integer.
    /// </summary>
    /// <returns>A 32-bit signed integer that is greater than or equal to 0 and less than Int32.MaxValue.</returns>
    int Next();

    /// <summary>
    /// Returns a non-negative random integer that is less than the specified maximum.
    /// </summary>
    /// <param name="maxValue">The exclusive upper bound of the random number to be generated. Must be greater than or equal to 0.</param>
    /// <returns>A 32-bit signed integer that is greater than or equal to 0 and less than maxValue.</returns>
    int Next(int maxValue);

    /// <summary>
    /// Returns a random integer that is within a specified range.
    /// </summary>
    /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
    /// <param name="maxValue">The exclusive upper bound of the random number returned. Must be greater than or equal to minValue.</param>
    /// <returns>A 32-bit signed integer greater than or equal to minValue and less than maxValue.</returns>
    int Next(int minValue, int maxValue);

    /// <summary>
    /// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
    /// </summary>
    /// <returns>A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
    double NextDouble();

    /// <summary>
    /// Serializes the current state of the random generator to a JSON string.
    /// </summary>
    /// <returns>A JSON string representing the current state (seed and call count).</returns>
    /// <remarks>
    /// Multiple calls to this method without intervening random number generation
    /// will produce identical JSON strings, ensuring idempotent serialization.
    /// </remarks>
    string ToJson();

    /// <summary>
    /// Writes the current state of the random generator to a stream in binary format.
    /// </summary>
    /// <param name="stream">The stream to write the binary data to.</param>
    void Write(Stream stream);
}
