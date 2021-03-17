using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace SkiDiveDev.FastSets.Test
{
    class SetOperationsTests
    {
        ISuperSet<string> _superSet;

        [SetUp]
        public void Setup()
        {
            var population = new List<string>
            {
                "Allison",
                "Bobby",
                "Charlie",
                "Dorothy",
                "Elaine",
                "Fester",
                "Gordan",
                "Hillary",
                "Iris",
                "Jane" };
            _superSet = new SuperSet<string>("Test", "Test Superset", population);
        }


        [Test]
        public void TestIntersection()
        {
            // Arrange
            var testSet1 = _superSet.AddSet("setA");
            testSet1.Add("Allison")
                .Add("Bobby")
                .Add("Charlie");

            var testSet2 = _superSet.AddSet("setB");
            testSet2.Add("Charlie")
                .Add("Dorothy")
                .Add("Elaine");

            // Act
            var result = testSet1.IntersectedWith(testSet2);

            // Assert
            var bits = result.ToUlongArray()[0];
            Assert.That(bits, Is.EqualTo(0b0100));
        }


        [Test]
        public void TestUnion()
        {
            // Arrange
            var testSet1 = _superSet.AddSet("setA");
            testSet1.Add("Allison")
                .Add("Bobby")
                .Add("Charlie");

            var testSet2 = _superSet.AddSet("setB");
            testSet2.Add("Charlie")
                .Add("Dorothy")
                .Add("Elaine");

            // Act
            var result = testSet1.UnionedWith(testSet2);

            // Assert
            var bits = result.ToUlongArray()[0];
            Assert.That(bits, Is.EqualTo(0b0001_1111));
        }


        [Test]
        public void UnionWithVeryLongSetNames_ShouldTruncate()
        {
            // Arrange
            var testSet1Name = new String('A', 125);
            var testSet1 = _superSet.AddSet(testSet1Name);
            testSet1.Add("Allison")
                .Add("Bobby")
                .Add("Charlie");

            var testSet2Name = new String('B', 125);
            var testSet2 = _superSet.AddSet(testSet2Name);
            testSet2.Add("Charlie")
                .Add("Dorothy")
                .Add("Elaine");

            const int maxNameLength_chars = 255;
            const int fixedNumCharactersToAddToName = 9;                        // "('' ∪ '')".Length
            const int remainingAvailableNumChars = maxNameLength_chars - fixedNumCharactersToAddToName;
            var truncatedTestSet1Name = new String('A', remainingAvailableNumChars - testSet2Name.Length - 1);
            var expected = $"('…{truncatedTestSet1Name}' ∪ '{testSet2Name}')";

            // Act
            var unionSet = testSet1.UnionedWith(testSet2);
            var result = unionSet.Name;

            // Assert
            Assert.That(expected.Length, Is.EqualTo(maxNameLength_chars));
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
