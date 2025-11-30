using System.Text.Json;

namespace HexMapRandom.Tests;

[TestClass]
public sealed class SerializableRandomSerializationTests
{
    private readonly string TempDir = @"C:\Temp\";

    [TestMethod]
    public void ToJson_ReturnsValidJson()
    {
        var random = new SerializableRandom(12345);
        
        var json = random.ToJson();
        
        Assert.IsFalse(string.IsNullOrWhiteSpace(json), "JSON should not be empty");
    }

    [TestMethod]
    public void ToJson_IsIdempotent()
    {
        var random = new SerializableRandom(12345);
        random.Next(); // Make some calls
        random.Next();
        random.NextDouble();
        
        var json1 = random.ToJson();
        var json2 = random.ToJson();
        
        Assert.AreEqual(json1, json2, "Multiple calls to ToJson() without intervening random calls should produce identical output");
    }

    [TestMethod]
    public void FromJson_ReturnsValidInstance()
    {
        var original = new SerializableRandom(12345);
        var json = original.ToJson();
        
        var deserialized = SerializableRandom.FromJson(json);
        
        Assert.IsNotNull(deserialized, "Deserialized instance should not be null");
    }

    [TestMethod]
    public void FromJson_ThrowsOnNull()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => SerializableRandom.FromJson(null!));
        Assert.IsNotNull(exception, "FromJson should throw ArgumentNullException when json is null");
    }

    [TestMethod]
    public void FromJson_ThrowsOnInvalidJson()
    {
        var exception = Assert.Throws<JsonException>(() => SerializableRandom.FromJson("invalid json"));
        Assert.IsNotNull(exception, "FromJson should throw JsonException when json is invalid");
    }

    [TestMethod]
    public void JsonRoundTrip_PreservesState_NoCallsMade()
    {
        var original = new SerializableRandom(12345);
        
        var json = original.ToJson();
        var deserialized = SerializableRandom.FromJson(json);
        
        // Both should produce the same sequence
        Assert.AreEqual(original.Next(), deserialized.Next(), "First value should match");
        Assert.AreEqual(original.Next(), deserialized.Next(), "Second value should match");
        Assert.AreEqual(original.NextDouble(), deserialized.NextDouble(), "Third value should match");
    }

    [TestMethod]
    public void JsonRoundTrip_PreservesState_AfterCalls()
    {
        var original = new SerializableRandom(12345);
        
        // Make some calls before serialization
        original.Next();
        original.Next();
        original.NextDouble();
        
        var json = original.ToJson();
        var deserialized = SerializableRandom.FromJson(json);
        
        // Both should continue with the same sequence
        Assert.AreEqual(original.Next(), deserialized.Next(), "Fourth value should match");
        Assert.AreEqual(original.Next(100), deserialized.Next(100), "Fifth value should match");
        Assert.AreEqual(original.Next(10, 50), deserialized.Next(10, 50), "Sixth value should match");
    }

    [TestMethod]
    public void JsonRoundTrip_PreservesState_ManyCalls()
    {
        var original = new SerializableRandom(999);
        
        // Make many calls before serialization
        for (int i = 0; i < 100; i++)
        {
            original.Next();
        }
        
        var json = original.ToJson();

#if DEBUG
        File.WriteAllText($"{TempDir}SerializableRandom_ManyCalls.json", json);
#endif

        var deserialized = SerializableRandom.FromJson(json);
        
        // Continue sequence verification
        for (int i = 0; i < 10; i++)
        {
            Assert.AreEqual(original.Next(), deserialized.Next(), $"Value at iteration {i + 100} should match");
        }
    }

    [TestMethod]
    public void WriteTo_ThrowsOnNullStream()
    {
        var random = new SerializableRandom(12345);
        
        var exception = Assert.Throws<ArgumentNullException>(() => random.Write(null!));
        Assert.IsNotNull(exception, "WriteTo should throw ArgumentNullException when stream is null");
    }

    [TestMethod]
    public void ReadFrom_ThrowsOnNullStream()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => SerializableRandom.Read(null!));
        Assert.IsNotNull(exception, "ReadFrom should throw ArgumentNullException when stream is null");
    }

    [TestMethod]
    public void ReadFrom_ThrowsOnInvalidStream()
    {
        using var ms = new MemoryStream();
        ms.WriteByte(1); // Write incomplete data
        ms.Position = 0;
        
        var exception = Assert.Throws<EndOfStreamException>(() => SerializableRandom.Read(ms));
        Assert.IsNotNull(exception, "ReadFrom should throw EndOfStreamException when stream contains incomplete data");
    }

    [TestMethod]
    public void BinaryRoundTrip_PreservesState_NoCallsMade()
    {
        var original = new SerializableRandom(12345);
        
        using var ms = new MemoryStream();
        original.Write(ms);
        ms.Position = 0;
        var deserialized = SerializableRandom.Read(ms);
        
        // Both should produce the same sequence
        Assert.AreEqual(original.Next(), deserialized.Next(), "First value should match");
        Assert.AreEqual(original.Next(), deserialized.Next(), "Second value should match");
        Assert.AreEqual(original.NextDouble(), deserialized.NextDouble(), "Third value should match");
    }

    [TestMethod]
    public void BinaryRoundTrip_PreservesState_AfterCalls()
    {
        var original = new SerializableRandom(12345);
        
        // Make some calls before serialization
        original.Next();
        original.Next();
        original.NextDouble();
        
        using var ms = new MemoryStream();
        original.Write(ms);

#if DEBUG
        File.WriteAllBytes($"{TempDir}SerializableRandom_AfterCalls.bin", ms.ToArray());
#endif

        ms.Position = 0;
        var deserialized = SerializableRandom.Read(ms);
        
        // Both should continue with the same sequence
        Assert.AreEqual(original.Next(), deserialized.Next(), "Fourth value should match");
        Assert.AreEqual(original.Next(100), deserialized.Next(100), "Fifth value should match");
        Assert.AreEqual(original.Next(10, 50), deserialized.Next(10, 50), "Sixth value should match");
    }

    [TestMethod]
    public void BinaryRoundTrip_PreservesState_ManyCalls()
    {
        var original = new SerializableRandom(999);
        
        // Make many calls before serialization
        for (int i = 0; i < 100; i++)
        {
            original.Next();
        }
        
        using var ms = new MemoryStream();
        original.Write(ms);
        ms.Position = 0;
        var deserialized = SerializableRandom.Read(ms);
        
        // Continue sequence verification
        for (int i = 0; i < 10; i++)
        {
            Assert.AreEqual(original.Next(), deserialized.Next(), $"Value at iteration {i + 100} should match");
        }
    }

    [TestMethod]
    public void BinaryRoundTrip_LeavesStreamOpen()
    {
        var original = new SerializableRandom(12345);
        
        using var ms = new MemoryStream();
        original.Write(ms);
        
        Assert.IsTrue(ms.CanWrite, "Stream should remain open after WriteTo");
        
        ms.Position = 0;
        var deserialized = SerializableRandom.Read(ms);
        
        Assert.IsTrue(ms.CanRead, "Stream should remain open after ReadFrom");
    }

    [TestMethod]
    public void JsonAndBinaryRoundTrip_ProduceSameResults()
    {
        var original = new SerializableRandom(12345);
        original.Next();
        original.Next();
        
        // JSON deserialization
        var json = original.ToJson();
        var jsonDeserialized = SerializableRandom.FromJson(json);
        
        // Binary deserialization
        using var ms = new MemoryStream();
        original.Write(ms);
        ms.Position = 0;
        var binaryDeserialized = SerializableRandom.Read(ms);
        
        // Both should produce the same sequence
        for (int i = 0; i < 10; i++)
        {
            var jsonResult = jsonDeserialized.Next();
            var binaryResult = binaryDeserialized.Next();
            Assert.AreEqual(jsonResult, binaryResult, $"JSON and binary deserialization should produce same results at iteration {i}");
        }
    }

    [TestMethod]
    public void MultipleSerializationCycles_MaintainState()
    {
        var random = new SerializableRandom(42);
        random.Next();
        random.Next();
        
        // First serialization cycle
        var json1 = random.ToJson();
        var deserialized1 = SerializableRandom.FromJson(json1);
        deserialized1.Next();
        deserialized1.Next();
        
        // Second serialization cycle
        var json2 = deserialized1.ToJson();
        var deserialized2 = SerializableRandom.FromJson(json2);
        
        // Original should match after same number of calls
        random.Next();
        random.Next();
        
        Assert.AreEqual(random.Next(), deserialized2.Next(), "Multiple serialization cycles should maintain state");
    }

    [TestMethod]
    public void SerializationFormat_ContainsSeedAndCallCount()
    {
        var random = new SerializableRandom(12345);
        random.Next();
        random.Next();
        random.Next();
        
        var json = random.ToJson();
        
        Assert.Contains("12345", json, "JSON should contain seed value");
        Assert.Contains("3", json, "JSON should contain call count");
    }

    [TestMethod]
    public void BinaryFormat_ProducesConsistentOutput()
    {
        var random1 = new SerializableRandom(12345);
        var random2 = new SerializableRandom(12345);
        
        random1.Next();
        random2.Next();
        
        using var ms1 = new MemoryStream();
        using var ms2 = new MemoryStream();
        
        random1.Write(ms1);
        random2.Write(ms2);
        
        CollectionAssert.AreEqual(ms1.ToArray(), ms2.ToArray(), "Same state should produce identical binary output");
    }
}
