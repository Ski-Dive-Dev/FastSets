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
            var testSet1 = new FastSet<string>(_superSet, "setA");
            testSet1.Add("Allison")
                .Add("Bobby")
                .Add("Charlie");

            var testSet2 = new FastSet<string>(_superSet, "setB");
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
            var testSet1 = new FastSet<string>(_superSet, "setA");
            testSet1.Add("Allison")
                .Add("Bobby")
                .Add("Charlie");

            var testSet2 = new FastSet<string>(_superSet, "setB");
            testSet2.Add("Charlie")
                .Add("Dorothy")
                .Add("Elaine");

            // Act
            var result = testSet1.UnionedWith(testSet2);

            // Assert
            var bits = result.ToUlongArray()[0];
            Assert.That(bits, Is.EqualTo(0b0001_1111));
        }
    }
}
