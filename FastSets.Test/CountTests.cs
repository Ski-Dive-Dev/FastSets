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
        public void WhenThereAreXMembers_Count_ShouldReturnX(int populationSize, int numMembers,
            int memberSelector, int expected)
        {
            // Arrange

            // Set up population
            for (var i = 0; i < populationSize; i++)
            {
                _superSet.AddMember("Test Member " + i);
            }


            var testSet1 = _superSet.AddSet("setA");

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
