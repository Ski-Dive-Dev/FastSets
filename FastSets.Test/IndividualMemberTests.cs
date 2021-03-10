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
    }
}