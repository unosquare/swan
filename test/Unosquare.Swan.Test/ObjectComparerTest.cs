namespace Unosquare.Swan.Test.ObjectComparerTests
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Components;
    using Mocks;

    [TestFixture]
    public class AreObjectsEqual : TestFixtureBase
    {
        [Test]
        public void SameObjects_ReturnsTrue()
        {
            Assert.IsTrue(ObjectComparer.AreObjectsEqual(DefaultObject, DefaultObject));
        }

        [Test]
        public void EqualsObjects_ReturnsTrue()
        {
            var left = new BasicJson
            {
                BoolData = true,
                DecimalData = 1,
                IntData = 1,
                NegativeInt = -1,
                StringData = "A",
                StringNull = null
            };

            var right = new BasicJson
            {
                BoolData = true,
                DecimalData = 1,
                IntData = 1,
                NegativeInt = -1,
                StringData = "A",
                StringNull = null
            };

            Assert.IsTrue(ObjectComparer.AreObjectsEqual(left, right));
        }

        [Test]
        public void DifferentObjects_ReturnsFalse()
        {
            Assert.IsFalse(ObjectComparer.AreObjectsEqual(DefaultObject, new DateBasicJson()));
        }

        [Test]
        public void ObjectsWithDifferentProps_ReturnsFalse()
        {
            var leftArray = new {Numero = new Array[1, 2, 3], Letra = "A"};
            var rightArray = new {Numero = new Array[1, 5, 3], Letra = "A"};

            Assert.IsFalse(ObjectComparer.AreObjectsEqual(leftArray, rightArray));
        }

        [Test]
        public void NullType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                ObjectComparer.AreObjectsEqual(DefaultObject, DefaultObject, null));
        }
    }

    [TestFixture]
    public class AreStructsEqual : TestFixtureBase
    {
        [Test]
        public void EqualStructs_ReturnsTrue()
        {
            Assert.IsTrue(ObjectComparer.AreStructsEqual(DefaultStruct, DefaultStruct));
        }

        [Test]
        public void DifferentStructs_ReturnsFalse()
        {
            var rightStruct = new SampleStruct
            {
                Name = "Kadosh",
                Value = 2
            };

            Assert.IsFalse(ObjectComparer.AreStructsEqual(DefaultStruct, rightStruct));
        }

        [Test]
        public void NullType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                ObjectComparer.AreStructsEqual(DefaultStruct, DefaultStruct, null));
        }
    }

    [TestFixture]
    public class AreArraysEqual
    {
        [Test]
        public void EqualArrays_ReturnsTrue()
        {
            var leftArray = new[] {1, 2, 3, 4, 5};
            var rightArray = new[] {1, 2, 3, 4, 5};

            Assert.IsTrue(ObjectComparer.AreEnumerationsEquals(leftArray, rightArray));
        }

        [Test]
        public void DifferentArrays_ReturnsFalse()
        {
            var leftArray = new[] {1, 2, 3};
            var rightArray = new[] {7, 1, 9};

            Assert.IsFalse(ObjectComparer.AreEnumerationsEquals(leftArray, rightArray));
        }
    }

    [TestFixture]
    public class AreArrayObjectsEqual
    {
        [Test]
        public void EqualObjectsWithArrayProperties_ReturnsTrue()
        {
            var leftObject = new AdvArrayJson {Id = 1, Properties = new[] {BasicJson.GetDefault()}};
            var rightObject = new AdvArrayJson {Id = 1, Properties = new[] {BasicJson.GetDefault()}};

            Assert.IsTrue(ObjectComparer.AreEqual(leftObject, rightObject));
        }

        [Test]
        public void EqualArraysWithObjects_ReturnsTrue()
        {
            var leftArrayObject = new[] {BasicJson.GetDefault(), BasicJson.GetDefault()};
            var rightArrayObject = new[] {BasicJson.GetDefault(), BasicJson.GetDefault()};

            Assert.IsTrue(ObjectComparer.AreEnumerationsEquals(leftArrayObject, rightArrayObject));
        }
    }

    [TestFixture]
    public class AreEnumerationsEquals : TestFixtureBase
    {
        [Test]
        public void EnumsWithDifferentLengths_ReturnsFalse()
        {
            var right = new List<string> {"Unosquare"};

            Assert.IsFalse(
                ObjectComparer.AreEnumerationsEquals(DefaultStringList.AsEnumerable(), right.AsEnumerable()));
        }

        [Test]
        public void LeftEnumNull_ThrowsArgumentNullException()
        {
            var right = new List<string> {"Unosquare"};

            Assert.Throws<ArgumentNullException>(() => ObjectComparer.AreEnumerationsEquals(NullStringList, right));
        }

        [Test]
        public void RightEnumNull_ThrowsArgumentNullException()
        {
            var left = new List<string> {"Unosquare"};

            Assert.Throws<ArgumentNullException>(() => ObjectComparer.AreEnumerationsEquals(left, NullStringList));
        }
    }

    [TestFixture]
    public class AreStructsEqualsInProps
    {
        [Test]
        public void StructsSameProps_ReturnsTrue()
        {
            var leftStruct = new SampleStructWithProps
            {
                StudentId = 1,
                Average = 98.10,
                Notes = "Good"
            };

            var rightStruct = new SampleStructWithProps
            {
                StudentId = 1,
                Average = 98.10,
                Notes = "Good"
            };

            Assert.IsTrue(ObjectComparer.AreStructsEqual(leftStruct, rightStruct));
        }

        [Test]
        public void StructsDifferentProps_ReturnsFalse()
        {
            var leftStruct = new SampleStructWithProps
            {
                StudentId = 1,
                Average = 98.10,
                Notes = "Good"
            };

            var rightStruct = new SampleStructWithProps
            {
                StudentId = 2,
                Average = 79.78,
                Notes = "Ehmm, it could be better"
            };

            Assert.IsFalse(ObjectComparer.AreStructsEqual(leftStruct, rightStruct));
        }
    }

    [TestFixture]
    public class AreEqual
    {
        [Test]
        public void StructsEquals_ReturnsTrue()
        {
            var leftStruct = new SampleStruct
            {
                Name = "ArCiGo",
                Value = 1
            };

            var rightStruct = new SampleStruct
            {
                Name = "ArCiGo",
                Value = 1
            };

            Assert.IsTrue(ObjectComparer.AreEqual(leftStruct, rightStruct));
        }

        [Test]
        public void NullType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                ObjectComparer.AreEqual(new SampleStruct(), new SampleStruct(), null));
        }
    }
}