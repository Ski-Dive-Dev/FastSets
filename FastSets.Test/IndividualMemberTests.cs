using System;
using System.Collections.Generic;
using NUnit.Framework;
using SkiDiveDev.FastSets;

namespace SkiDiveDev.FastSets.Test
{
    public class Tests
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
        public void GivenEmptySet_WhenContainsCalled_ShouldThrowException()
        {
            // Arrange
            var testSet = new FastSet<string>(_superSet, "testSet");

            // Act/Assert
            Assert.Throws<Exception>(() => testSet.Contains("NOT A MEMBER"));
        }


        [Test]
        public void GivenNonEmptySet_WhenContainsCalledWithNonMember_ShouldReturnFalse()
        {
            // Arrange
            var testSet = new FastSet<string>(_superSet, "testSet");
            testSet.Add("Allison");

            // Act
            var result = testSet.Contains("Bobby");

            //Assert
            Assert.That(result, Is.False);
        }


        [Test]
        public void GivenNonEmptySet_WhenContainsCalledWithMember_ShouldReturnTrue()
        {
            // Arrange
            var testSet = new FastSet<string>(_superSet, "testSet");
            testSet.Add("Allison");

            // Act
            var result = testSet.Contains("Allison");

            //Assert
            Assert.That(result, Is.True);
        }


        [Test]
        public void WhenTwoMembersAdded_ToUlongArrayReturnsCorrectValue()
        {
            // Arrange
            var testSet = new FastSet<string>(_superSet, "testSet");
            testSet.Add("Allison")
                .Add("Charlie");

            // Act
            var result = testSet.ToUlongArray();

            // Assert
            Assert.That(result[0], Is.EqualTo(0b101));
        }


        [Test]
        public void When100MembersAdded_ToUlongArrayReturnsCorrectValue()
        {
            // Arrange
            const ulong expected0 = ulong.MaxValue;
            const ulong expected1 = 0b_0011_11111111_11111111_11111111;

            var testSet = new FastSet<string>(_superSet, "testSet");
            _superSet.AddSet(testSet);

            var members = new List<string>
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
            testSet.Add(members);

            for (var i = 10; i < 90; i++)
            {
                _superSet.AddMember("Test Member " + i);
                testSet.Add("Test Member " + i);
            }

            // Act
            var result = testSet.ToUlongArray();

            // Assert
            //var differences = result[1] | expected1;
            var differences = result[0] | expected0;
            var differencesWithHighBitTruncated = differences & 0x_7FFFFFFF_FFFFFFFF;
            var lower63BitsOfDifferences = (long)differencesWithHighBitTruncated;
            var stringOf63BitDifferences = Convert.ToString(lower63BitsOfDifferences, 2);

            Assert.That(result[0], Is.EqualTo(expected0), "1st 64 members mismatch");
            Assert.That(result[1], Is.EqualTo(expected1), stringOf63BitDifferences);
        }
    }
}