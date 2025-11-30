namespace HexMapRandom.Tests;

[TestClass]
public sealed class SerializableRandomTests
{
    [TestMethod]
    public void Constructor_Default_CreatesInstance()
    {
        var random = new SerializableRandom();
        
        Assert.IsNotNull(random);
    }

    [TestMethod]
    public void Constructor_WithSeed_CreatesInstance()
    {
        var random = new SerializableRandom(12345);
        
        Assert.IsNotNull(random);
    }

    [TestMethod]
    public void Next_ReturnsNonNegativeInteger()
    {
        var random = new SerializableRandom(42);
        
        var result = random.Next();
        
        Assert.IsGreaterThanOrEqualTo(0, result, "Next() should return a non-negative integer");
        Assert.IsLessThan(int.MaxValue, result, "Next() should return a value less than Int32.MaxValue");
    }

    [TestMethod]
    public void Next_WithMaxValue_ReturnsValueInRange()
    {
        var random = new SerializableRandom(42);
        int maxValue = 100;
        
        for (int i = 0; i < 10; i++)
        {
            var result = random.Next(maxValue);
            Assert.IsGreaterThanOrEqualTo(0, result, $"Next({maxValue}) should return a value >= 0, got {result}");
            Assert.IsLessThan(maxValue, result, $"Next({maxValue}) should return a value < {maxValue}, got {result}");
        }
    }

    [TestMethod]
    public void Next_WithMinAndMaxValue_ReturnsValueInRange()
    {
        var random = new SerializableRandom(42);
        int minValue = 50;
        int maxValue = 100;
        
        for (int i = 0; i < 10; i++)
        {
            var result = random.Next(minValue, maxValue);
            Assert.IsGreaterThanOrEqualTo(minValue, result, $"Next({minValue}, {maxValue}) should return a value >= {minValue}, got {result}");
            Assert.IsLessThan(maxValue, result, $"Next({minValue}, {maxValue}) should return a value < {maxValue}, got {result}");
        }
    }

    [TestMethod]
    public void NextDouble_ReturnsValueInRange()
    {
        var random = new SerializableRandom(42);
        
        for (int i = 0; i < 10; i++)
        {
            var result = random.NextDouble();
            Assert.IsGreaterThanOrEqualTo(0.0, result, $"NextDouble() should return a value >= 0.0, got {result}");
            Assert.IsLessThan(1.0, result, $"NextDouble() should return a value < 1.0, got {result}");
        }
    }

    [TestMethod]
    public void SameSeed_ProducesSameSequence()
    {
        var random1 = new SerializableRandom(12345);
        var random2 = new SerializableRandom(12345);
        
        for (int i = 0; i < 10; i++)
        {
            var result1 = random1.Next();
            var result2 = random2.Next();
            Assert.AreEqual(result1, result2, $"Same seed should produce same sequence at iteration {i}");
        }
    }

    [TestMethod]
    public void SameSeed_ProducesSameSequence_WithMaxValue()
    {
        var random1 = new SerializableRandom(12345);
        var random2 = new SerializableRandom(12345);
        
        for (int i = 0; i < 10; i++)
        {
            var result1 = random1.Next(100);
            var result2 = random2.Next(100);
            Assert.AreEqual(result1, result2, $"Same seed should produce same sequence at iteration {i}");
        }
    }

    [TestMethod]
    public void SameSeed_ProducesSameSequence_WithMinMaxValue()
    {
        var random1 = new SerializableRandom(12345);
        var random2 = new SerializableRandom(12345);
        
        for (int i = 0; i < 10; i++)
        {
            var result1 = random1.Next(50, 100);
            var result2 = random2.Next(50, 100);
            Assert.AreEqual(result1, result2, $"Same seed should produce same sequence at iteration {i}");
        }
    }

    [TestMethod]
    public void SameSeed_ProducesSameSequence_NextDouble()
    {
        var random1 = new SerializableRandom(12345);
        var random2 = new SerializableRandom(12345);
        
        for (int i = 0; i < 10; i++)
        {
            var result1 = random1.NextDouble();
            var result2 = random2.NextDouble();
            Assert.AreEqual(result1, result2, $"Same seed should produce same sequence at iteration {i}");
        }
    }

    [TestMethod]
    public void DifferentSeeds_ProduceDifferentSequences()
    {
        var random1 = new SerializableRandom(12345);
        var random2 = new SerializableRandom(54321);
        
        var result1 = random1.Next();
        var result2 = random2.Next();
        
        Assert.AreNotEqual(result1, result2, "Different seeds should produce different sequences");
    }

    [TestMethod]
    public void MixedMethodCalls_MaintainCallCount()
    {
        var random1 = new SerializableRandom(12345);
        var random2 = new SerializableRandom(12345);
        
        // Call different methods but same number of times
        random1.Next();
        random1.Next(100);
        random1.NextDouble();
        random1.Next(10, 50);
        
        random2.Next();
        random2.Next(100);
        random2.NextDouble();
        random2.Next(10, 50);
        
        // The next call should produce the same result
        var result1 = random1.Next();
        var result2 = random2.Next();
        
        Assert.AreEqual(result1, result2, "Call count should be maintained regardless of method type");
    }

    [TestMethod]
    public void MultipleCallsToNext_ProducesDifferentResults()
    {
        var random = new SerializableRandom(42);
        var results = new HashSet<int>();
        
        for (int i = 0; i < 100; i++)
        {
            results.Add(random.Next(1000));
        }
        
        // Should have generated multiple different values
        Assert.IsGreaterThan(50, results.Count, $"Should generate diverse random numbers, got {results.Count} unique values out of 100 calls");
    }

    [TestMethod]
    public void NextDouble_ProducesDiverseValues()
    {
        var random = new SerializableRandom(42);
        var results = new HashSet<double>();
        
        for (int i = 0; i < 100; i++)
        {
            results.Add(random.NextDouble());
        }
        
        // Should have generated many different values
        Assert.IsGreaterThan(90, results.Count, $"Should generate diverse random doubles, got {results.Count} unique values out of 100 calls");
    }

    [TestMethod]
    public void StatePreservation_AfterManyCalls()
    {
        var random1 = new SerializableRandom(999);
        var random2 = new SerializableRandom(999);
        
        // Make many calls on first instance
        for (int i = 0; i < 1000; i++)
        {
            random1.Next();
        }
        
        // Make same number of calls on second instance
        for (int i = 0; i < 1000; i++)
        {
            random2.Next();
        }
        
        // Next calls should still match
        Assert.AreEqual(random1.Next(), random2.Next(), "Sequence should remain deterministic after many calls");
        Assert.AreEqual(random1.NextDouble(), random2.NextDouble(), "Sequence should remain deterministic for different methods");
    }
}
