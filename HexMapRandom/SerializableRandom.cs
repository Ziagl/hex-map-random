using System.Text.Json;

namespace HexMapRandom;

/// <summary>
/// A serializable random number generator that maintains deterministic sequences
/// through serialization and deserialization cycles.
/// </summary>
/// <remarks>
/// WARNING: This class is NOT thread-safe. Do not access the same instance from multiple threads
/// without external synchronization. This implementation is designed for single-threaded,
/// turn-based game scenarios where deterministic random sequences are required.
/// 
/// The generator maintains its state through a seed value and a call count. When deserialized,
/// it recreates the internal Random instance and replays the exact number of calls to restore
/// the sequence position, ensuring identical random number generation across save/load cycles.
/// </remarks>
public class SerializableRandom : ISerializableRandom
{
    private readonly int _seed;
    private int _callCount;
    private Random _random;

    /// <summary>
    /// Initializes a new instance with a time-dependent default seed value.
    /// </summary>
    public SerializableRandom()
    {
        _seed = Environment.TickCount;
        _callCount = 0;
        _random = new Random(_seed);
    }

    /// <summary>
    /// Initializes a new instance with a specified seed value.
    /// </summary>
    /// <param name="seed">A number used to calculate a starting value for the pseudo-random number sequence.</param>
    public SerializableRandom(int seed)
    {
        _seed = seed;
        _callCount = 0;
        _random = new Random(_seed);
    }

    /// <summary>
    /// Internal constructor used for deserialization.
    /// </summary>
    private SerializableRandom(int seed, int callCount)
    {
        _seed = seed;
        _callCount = callCount;
        _random = new Random(_seed);
        
        // Replay the exact number of calls to restore the sequence position
        for (int i = 0; i < callCount; i++)
        {
            _random.Next();
        }
    }

    /// <inheritdoc/>
    public int Next()
    {
        _callCount++;
        return _random.Next();
    }

    /// <inheritdoc/>
    public int Next(int maxValue)
    {
        _callCount++;
        return _random.Next(maxValue);
    }

    /// <inheritdoc/>
    public int Next(int minValue, int maxValue)
    {
        _callCount++;
        return _random.Next(minValue, maxValue);
    }

    /// <inheritdoc/>
    public double NextDouble()
    {
        _callCount++;
        return _random.NextDouble();
    }

    /// <inheritdoc/>
    public string ToJson()
    {
        var state = new RandomState(_seed, _callCount);
        return JsonSerializer.Serialize(state, new JsonSerializerOptions 
        { 
            WriteIndented = false // Ensure consistent output for idempotent serialization
        });
    }

    /// <summary>
    /// Deserializes a SerializableRandom instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string containing the serialized state.</param>
    /// <returns>A new SerializableRandom instance with the restored state.</returns>
    /// <exception cref="ArgumentNullException">Thrown when json is null.</exception>
    /// <exception cref="JsonException">Thrown when json is invalid.</exception>
    public static SerializableRandom FromJson(string json)
    {
        if (json == null)
            throw new ArgumentNullException(nameof(json));

        var state = JsonSerializer.Deserialize<RandomState>(json);
        if (state == null)
            throw new JsonException("Failed to deserialize random state from JSON.");

        return new SerializableRandom(state.Seed, state.CallCount);
    }

    /// <inheritdoc/>
    public void WriteTo(Stream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        writer.Write(_seed);
        writer.Write(_callCount);
    }

    /// <summary>
    /// Reads a SerializableRandom instance from a stream in binary format.
    /// </summary>
    /// <param name="stream">The stream to read the binary data from.</param>
    /// <returns>A new SerializableRandom instance with the restored state.</returns>
    /// <exception cref="ArgumentNullException">Thrown when stream is null.</exception>
    /// <exception cref="EndOfStreamException">Thrown when the stream does not contain valid binary data.</exception>
    public static SerializableRandom ReadFrom(Stream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        int seed = reader.ReadInt32();
        int callCount = reader.ReadInt32();

        return new SerializableRandom(seed, callCount);
    }

    /// <summary>
    /// Internal record for JSON serialization of the random state.
    /// </summary>
    private record RandomState(int Seed, int CallCount);
}
