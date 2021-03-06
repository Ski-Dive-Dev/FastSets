using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace SkiDiveDev.FastSets.Test
{
    class AggregateTests
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


        [TestCase(0, 0, 1)]
        [TestCase(1, 0, 1)]
        [TestCase(64, 0, 1)]
        [TestCase(100, 0, 1)]
        [TestCase(1, 1, 1)]
        [TestCase(64, 10, 1)]
        [TestCase(100, 99, 1)]
        [TestCase(64, 50, 2)]
        [TestCase(100, 99, 2)]
        [TestCase(64, 51, 3)]
        [TestCase(100, 99, 3)]
        public void WhenSetContainsMember_Contains_ShouldReturnTrue(int populationSize, int numMembers,
            int memberSelector)
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


            // Act-Assert
            for (var i = 0; i < numMembers; i += memberSelector)
            {
                var result = testSet1.Contains("Test Member " + i);
                Assert.That(result, Is.True);
            }
        }


        [TestCase(0, 0, 1)]
        [TestCase(1, 0, 1)]
        [TestCase(64, 0, 1)]
        [TestCase(100, 0, 1)]
        [TestCase(1, 1, 1)]
        [TestCase(64, 10, 1)]
        [TestCase(100, 99, 1)]
        [TestCase(64, 50, 2)]
        [TestCase(100, 99, 2)]
        [TestCase(64, 51, 3)]
        [TestCase(100, 99, 3)]
        public void WhenSetDoesNotContainMember_Contains_ShouldReturnFalse(int populationSize, int numMembers,
            int memberSelector)
        {
            // Arrange
            var testSet1 = _superSet.AddSet("setA");
            var members = new List<string>(numMembers);

            // Set up population
            for (var i = 0; i < populationSize; i++)
            {
                _superSet.AddMember("Test Member " + i);
            }


            // Set up set members
            for (var i = 0; i < numMembers; i += memberSelector)
            {
                members.Add("Test Member " + i);
                testSet1.Add("Test Member " + i);
            }


            // Act-Assert
            for (var i = 0; i < numMembers; i++)
            {
                if (!members.Contains("Test Member " + i))
                {
                    var result = testSet1.Contains("Test Member " + i);
                    Assert.That(result, Is.False);
                }
            }
        }


        [TestCase(0, 0)]
        [TestCase(1, 1)]
        [TestCase(64, 64)]
        [TestCase(100, 100)]
        public void WhenAllPopulationIsInSet_All_ShouldReturnTrue(int populationSize, int numMembers)
        {
            // Arrange
            var testSet1 = _superSet.AddSet("setA");
            var expected = (numMembers == populationSize);

            // Set up population
            for (var i = 0; i < populationSize; i++)
            {
                _superSet.AddMember("Test Member " + i);
            }


            // Set up set members
            for (var i = 0; i < numMembers; i++)
            {
                testSet1.Add("Test Member " + i);
            }


            // Act
            var result = testSet1.All();

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }


        [TestCase(1, 0, 1)]
        [TestCase(64, 0, 1)]
        [TestCase(100, 0, 1)]
        [TestCase(64, 10, 1)]
        [TestCase(100, 99, 1)]
        [TestCase(64, 50, 2)]
        [TestCase(100, 99, 2)]
        [TestCase(64, 51, 3)]
        [TestCase(100, 99, 3)]
        public void WhenNotAllPopulationIsInSet_All_ShouldReturnFalse(int populationSize, int numMembers,
            int memberSelector)
        {
            // Arrange
            var testSet1 = _superSet.AddSet("setA");
            var expected = (numMembers == populationSize);

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
            var result = testSet1.All();

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }


        [TestCase(1, 0, 1)]
        [TestCase(64, 0, 1)]
        [TestCase(100, 0, 1)]
        public void WhenThereAreNoMembers_Any_ShouldReturnFalse(int populationSize, int numMembers,
            int memberSelector)
        {
            // Arrange
            var testSet1 = _superSet.AddSet("setA");
            var expected = (numMembers != 0);

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
            var result = testSet1.Any();

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }


        [TestCase(64, 10, 1)]
        [TestCase(100, 99, 1)]
        [TestCase(64, 50, 2)]
        [TestCase(100, 99, 2)]
        [TestCase(64, 51, 3)]
        [TestCase(100, 99, 3)]
        public void WhenThereAreMembers_Any_ShouldReturnTrue(int populationSize, int numMembers,
            int memberSelector)
        {
            // Arrange
            var testSet1 = _superSet.AddSet("setA");
            var expected = (numMembers != 0);

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
            var result = testSet1.Any();

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }



        [TestCase(1, 0, 1)]
        [TestCase(64, 0, 1)]
        [TestCase(100, 0, 1)]
        public void WhenThereAreNoMembers_Clear_ShouldNotAffectMembership(int populationSize, int numMembers,
            int memberSelector)
        {
            // Arrange
            var testSet1 = _superSet.AddSet("setA");
            var expected = new ulong[0];

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
            testSet1.Clear();
            var result = testSet1.ToUlongArray();

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }


        [TestCase(64, 10, 1)]
        [TestCase(100, 99, 1)]
        [TestCase(64, 50, 2)]
        [TestCase(100, 99, 2)]
        [TestCase(64, 51, 3)]
        [TestCase(100, 99, 3)]
        public void WhenThereAreMembers_Clear_ShouldRemoveAllOfThem(int populationSize, int numMembers,
            int memberSelector)
        {
            // Arrange
            var testSet1 = _superSet.AddSet("setA");
            var expected = new ulong[0];

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
            testSet1.Clear();
            var result = testSet1.ToUlongArray();

            // Assert
            Assert.That(result, Is.EquivalentTo(expected));
        }


        [TestCase(64, 10, 1)]
        [TestCase(100, 99, 1)]
        [TestCase(64, 50, 2)]
        [TestCase(100, 99, 2)]
        [TestCase(64, 51, 3)]
        [TestCase(100, 99, 3)]
        public void WhenThereAreMembers_AsEnumerable_ShouldReturnAllOfThem(int populationSize, int numMembers,
            int memberSelector)
        {
            // Arrange
            var testSet1 = _superSet.AddSet("setA");
            var members = new List<string>(numMembers);

            // Set up population
            for (var i = 0; i < populationSize; i++)
            {
                _superSet.AddMember("Test Member " + i);
            }


            // Set up set members
            for (var i = 0; i < numMembers; i += memberSelector)
            {
                members.Add("Test Member " + i);
                testSet1.Add("Test Member " + i);
            }


            // Act-Assert
            var result = testSet1.AsEnumerable();
            Assert.That(result, Is.EqualTo(members));
        }



        [TestCase(1, 0, 1)]
        [TestCase(64, 0, 1)]
        [TestCase(100, 0, 1)]
        public void WhenThereAreNoMembers_AsEnumerable_ShouldReturnEmptyList(int populationSize, int numMembers,
            int memberSelector)
        {
            // Arrange
            var testSet1 = _superSet.AddSet("setA");
            var members = new List<string>(numMembers);

            // Set up population
            for (var i = 0; i < populationSize; i++)
            {
                _superSet.AddMember("Test Member " + i);
            }


            // Set up set members
            for (var i = 0; i < numMembers; i += memberSelector)
            {
                members.Add("Test Member " + i);
                testSet1.Add("Test Member " + i);
            }


            // Act
            var result = testSet1.AsEnumerable();

            // Assert
            Assert.That(result, Is.EqualTo(members));
        }
    }
}
