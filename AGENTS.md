# AGENTS.md - AI Agent Context Guide

## Project Overview

**hex-map-random** is a .NET library providing a serializable, deterministic random number generator designed for turn-based games and procedural generation scenarios where reproducible randomness is critical.

### Core Purpose
Enable random number generation that can be saved, loaded, and resumed with perfect consistency across application sessions, ensuring the same random sequence continues from exactly where it left off.

## Project Structure

```
hex-map-random/
├── HexMapRandom/                    # Main library project
│   ├── ISerializableRandom.cs       # Public interface definition
│   ├── SerializableRandom.cs        # Core implementation
│   └── HexMapRandom.csproj          # Project configuration & NuGet metadata
├── HexMapRandom.Tests/              # Unit test project
├── README.md                        # User-facing documentation
├── LICENSE                          # LGPL-3.0 license
├── icon.png                         # NuGet package icon
└── HexMapRandom.slnx                # Solution file
```

## Architecture & Design

### Key Components

#### 1. **ISerializableRandom** Interface
- **Location**: `HexMapRandom/ISerializableRandom.cs`
- **Purpose**: Public contract for the random generator
- **Key Methods**:
  - `int Next()` / `Next(int)` / `Next(int, int)` - Integer generation
  - `double NextDouble()` - Floating-point generation
  - `string ToJson()` - JSON serialization
  - `void WriteTo(Stream)` - Binary serialization
- **Design Philosophy**: Minimal API surface matching `System.Random` for familiarity

#### 2. **SerializableRandom** Implementation
- **Location**: `HexMapRandom/SerializableRandom.cs`
- **State Management**:
  - `_seed` (readonly int): Initial seed value
  - `_callCount` (int): Number of random calls made
  - `_random` (Random): Internal .NET Random instance
- **Key Mechanisms**:
  - **Deterministic Restoration**: On deserialization, recreates `Random` with saved seed and replays exact number of calls to restore sequence position
  - **Idempotent Serialization**: Multiple serializations without state changes produce identical output
  - **Call Tracking**: Every random method increments `_callCount` before delegating to internal `Random`

### Design Decisions

1. **Seed + Call Count Approach**
   - ✅ **Chosen**: Store seed and replay calls
   - ❌ **Rejected**: Reflection-based state extraction (fragile, implementation-dependent)
   - **Rationale**: Simple, portable, and reliable across .NET versions

2. **Thread Safety**
   - **Decision**: NOT thread-safe (documented clearly)
   - **Rationale**: Turn-based game use case is single-threaded; thread-safety overhead unnecessary

3. **Serialization Formats**
   - **JSON**: Human-readable, debugging-friendly, uses `System.Text.Json`
   - **Binary**: Compact storage for production saves, uses `BinaryWriter`/`BinaryReader`

4. **API Surface**
   - **Minimal**: Only core methods (Next variants, NextDouble)
   - **Rationale**: Simplicity for turn-based games; avoid feature creep

## Technical Specifications

### Technology Stack
- **Target Framework**: .NET 10.0
- **Language Features**: C# with nullable reference types, implicit usings
- **Dependencies**: System.Text.Json (included in .NET)
- **Build Tool**: .NET SDK

### State Serialization Format

**JSON Structure**:
```json
{"Seed":12345,"CallCount":42}
```

**Binary Structure**:
```
[4 bytes: seed (int32)]
[4 bytes: callCount (int32)]
```

### Performance Characteristics
- **Serialization**: O(1) - just writes two integers
- **Deserialization**: O(n) where n = callCount (must replay calls)
- **Memory**: Minimal - 3 fields (2 ints + 1 Random instance)

## Usage Patterns & Examples

### Basic Instantiation
```csharp
var random1 = new SerializableRandom();        // Time-based seed
var random2 = new SerializableRandom(12345);   // Explicit seed
```

### Save/Load Cycle
```csharp
// Save
var random = new SerializableRandom(42);
random.Next(100);  // Call count: 1
string json = random.ToJson();  // {"Seed":42,"CallCount":1}

// Load
var restored = SerializableRandom.FromJson(json);
restored.Next(100);  // Continues from call count 2
```

### Integration Pattern
```csharp
public interface IGameRandom : ISerializableRandom { }

public class GameEngine
{
    private readonly ISerializableRandom _random;
    
    public GameEngine(ISerializableRandom random)
    {
        _random = random;
    }
}
```

## Development Guidelines

### When Modifying This Library

1. **Preserve Determinism**: Any changes MUST maintain exact sequence reproduction after deserialization
2. **Call Count Tracking**: Every public random method MUST increment `_callCount` before calling internal `Random`
3. **Documentation**: Update XML docs for all public APIs; maintain thread-safety warnings
4. **Serialization**: Ensure idempotent serialization - same state = same output

### Testing Requirements
- Verify same seed produces same sequence
- Verify deserialized generator continues sequence correctly
- Verify multiple serializations produce identical output
- Test edge cases: callCount overflow, null checks, invalid JSON

### Version Compatibility
- **Current**: 0.1.0
- **Target Users**: Game developers, procedural generation systems
- **Breaking Changes**: Avoid changes to serialization format; maintain backward compatibility

## Common AI Agent Tasks

### Adding New Random Methods
1. Add method to `ISerializableRandom` interface with XML docs
2. Implement in `SerializableRandom` with `_callCount++` before delegation
3. Update README.md usage examples
4. Add tests for new method

### Extending Serialization
1. Update `RandomState` record with new fields
2. Modify serialization methods (ToJson, WriteTo)
3. Modify deserialization (FromJson, ReadFrom)
4. Ensure idempotent serialization
5. Add migration logic if breaking existing format

### Performance Optimization
- **Current bottleneck**: Call replay on deserialization (O(n))
- **Improvement options**: 
  - Snapshot internal Random state (complex, reflection-based)
  - Batch replay optimization (limited benefit)
- **Trade-off**: Simplicity vs performance (current choice: simplicity)

## Key Constraints & Limitations

1. **Call Count Limit**: Int32.MaxValue (~2.1 billion calls) before overflow
2. **Single-Threaded**: Not thread-safe; requires external synchronization for concurrent access
3. **Sequential Replay**: Deserialization performance degrades with high call counts
4. **Method Symmetry**: All Next() variants increment same counter, so mixing methods affects sequence

## NuGet Package Configuration

- **Package ID**: HexMapRandom
- **Version**: 0.1.0
- **Authors**: Werner Ziegelwanger
- **Company**: Hexagon Simulations
- **License**: LGPL-3.0
- **Tags**: hexagon, serializable, random, generator
- **Repository**: https://github.com/Ziagl/hex-map-random
- **Auto-Generate**: Enabled via package properties

## Integration Points

### Expected Consumer Projects
- **hex-map-core**: Hexagonal map generation library
- **Turn-based games**: Strategy, roguelikes, board games
- **Procedural generation**: Terrain, dungeons, content

### Interface Pattern
Consumers should reference `ISerializableRandom` interface, not concrete `SerializableRandom` class, enabling:
- Dependency injection
- Testing with mock implementations
- Future alternative implementations

## Troubleshooting Guide

### Common Issues

**Issue**: Deserialized generator produces different sequence
- **Cause**: Mixing Next() variants inconsistently
- **Solution**: Use same method calls in same order

**Issue**: Large callCount causes slow deserialization
- **Cause**: Replaying thousands/millions of calls
- **Solution**: Serialize more frequently; consider resetting with new seed

**Issue**: JSON deserialization fails
- **Cause**: Invalid JSON format or schema mismatch
- **Solution**: Validate JSON structure matches `{"Seed":int,"CallCount":int}`

## Future Enhancements (Potential)

1. **Advanced Methods**: NextBytes, Shuffle, GetItems
2. **State Snapshots**: Efficient checkpoint/restore without full replay
3. **Multi-Version Support**: Target multiple .NET frameworks
4. **Async Support**: Async serialization methods for large state files
5. **Compression**: Compress binary format for very large call counts

## Contact & Maintenance

- **Repository**: https://github.com/Ziagl/hex-map-random
- **Owner**: Ziagl
- **Primary Use Case**: Hexagon Simulations game development ecosystem
- **Maintenance**: Active development for turn-based game projects

*This document is intended for AI agents working on this codebase. Keep it updated when making architectural changes.*
