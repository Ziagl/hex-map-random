# hex-map-random

A lightweight, deterministic random number generator for .NET that maintains consistent sequences across serialization and deserialization cycles. Perfect for turn-based games, procedural generation, and any scenario requiring reproducible randomness.

## Features

- **Deterministic Sequences**: Same seed always produces the same sequence of random numbers
- **Serialization Support**: Save and restore random generator state via JSON or binary format
- **State Preservation**: Maintains exact position in random sequence across save/load cycles
- **Simple API**: Familiar interface matching `System.Random`
- **Lightweight**: Minimal dependencies, only uses `System.Text.Json`
- **Type-Safe**: Fully documented with nullable reference types enabled

## Installation

### NuGet Package Manager
```bash
Install-Package HexMapRandom
```

### .NET CLI
```bash
dotnet add package HexMapRandom
```

### PackageReference
```xml
<PackageReference Include="HexMapRandom" Version="1.0.0" />
```

## Usage

### Basic Usage

```csharp
using HexMapRandom;

// Create a new random generator with a specific seed
var random = new SerializableRandom(12345);

// Generate random numbers
int value1 = random.Next();              // Random integer
int value2 = random.Next(100);           // Random integer from 0 to 99
int value3 = random.Next(50, 100);       // Random integer from 50 to 99
double value4 = random.NextDouble();     // Random double from 0.0 to 1.0
```

### JSON Serialization

```csharp
// Create and use a random generator
var random = new SerializableRandom(12345);
int firstValue = random.Next(100);
int secondValue = random.Next(100);

// Serialize to JSON
string json = random.ToJson();

// Later... deserialize and continue the exact same sequence
var restored = SerializableRandom.FromJson(json);
int thirdValue = restored.Next(100);  // Continues from where we left off
```

### Binary Serialization

```csharp
// Save to a stream
var random = new SerializableRandom(12345);
random.Next(100);
random.Next(100);

using (var stream = new MemoryStream())
{
    random.WriteTo(stream);
    
    // Reset stream position for reading
    stream.Position = 0;
    
    // Restore from stream
    var restored = SerializableRandom.ReadFrom(stream);
    int nextValue = restored.Next(100);  // Continues the sequence
}
```

### Save Game Example

```csharp
public class GameState
{
    public ISerializableRandom Random { get; set; }
    public int Score { get; set; }
    
    public string SaveToJson()
    {
        return JsonSerializer.Serialize(new
        {
            RandomState = Random.ToJson(),
            Score
        });
    }
    
    public static GameState LoadFromJson(string json)
    {
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        return new GameState
        {
            Random = SerializableRandom.FromJson(data.GetProperty("RandomState").GetString()!),
            Score = data.GetProperty("Score").GetInt32()
        };
    }
}
```

## How It Works

The `SerializableRandom` class wraps .NET's standard `Random` class and tracks two pieces of state:
- **Seed**: The initial value used to create the random sequence
- **Call Count**: The number of times random generation methods have been called

When deserializing, the generator:
1. Creates a new `Random` instance with the saved seed
2. Replays the exact number of calls to restore the sequence position
3. Resumes generating numbers from the correct point in the sequence

This ensures that after deserialization, the random generator produces the exact same sequence as if it had never been serialized.

## Thread Safety

?? **WARNING**: `SerializableRandom` is **NOT thread-safe**. If you need to access the same instance from multiple threads, you must provide external synchronization (e.g., using `lock` statements).

This library is designed for single-threaded, turn-based scenarios where deterministic randomness is required.

## Requirements

- .NET 10.0 or later
- System.Text.Json (included in .NET)

## License

This project is licensed under the GNU Lesser General Public License v3.0 (LGPL-3.0) - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## Repository

[https://github.com/Ziagl/hex-map-random](https://github.com/Ziagl/hex-map-random)
