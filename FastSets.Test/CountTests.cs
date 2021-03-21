using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace SkiDiveDev.FastSets.Test
{
    class CountTests
    {
        ISuperSet<string> _superSet;

        List<string> _population = new List<string>();


        [SetUp]
        public void Setup()
        {
            _superSet = new SuperSet<string>("Test", "Test Superset", _population);
        }


        [TestCase(0, 0, 1, 0)]
        [TestCase(1, 0, 1, 0)]
        [TestCase(64, 0, 1, 0)]
        [TestCase(100, 0, 1, 0)]
        [TestCase(1, 1, 1, 1)]
        [TestCase(64, 10, 1, 10)]
        [TestCase(100, 99, 1, 99)]
        [TestCase(64, 50, 2, 25)]
        [TestCase(100, 99, 2, 50)]
        [TestCase(64, 51, 3, 17)]
        [TestCase(100, 99, 3, 33)]
        public void WhenThereAreXMembers_Count_ShouldReturnX(int populationSize, int numMembers,
            int memberSelector, int expected)
        {
            // Arrange
            var testSet1 = _superSet.AddSet("setA");

            // Set up population
            for (var i = 0; i < populationSize; i++)
            {
                _superSet.AddMember("Test Member " + i);
            }


            // Set up set members
            for (var i = 0; i < numMembers; i += memberSelector)
            {
                testSet1.Add("Test Member " + i);
            }

            // Act
            var result = testSet1.Count;

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

    }
}
