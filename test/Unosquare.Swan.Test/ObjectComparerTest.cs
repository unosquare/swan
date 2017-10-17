using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unosquare.Swan.Components;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ObjectComparerTest
    {

        [TestFixture]
        public class AreObjectsEqual : ObjectComparerTest
        {
            [Test]
            public void CompareEqualsObjects_ReturnsTrue()
            {
                var left = DateBasicJson.GetDateDefault();
                var right = DateBasicJson.GetDateDefault();

                Assert.IsTrue(ObjectComparer.AreObjectsEqual(left, right));
            }

            [Test]
            public void CompareDifferentObjects_ReturnsFalse()
            {
                var left = BasicJson.GetDefault();
                var right = new BasicJson();

                Assert.IsFalse(ObjectComparer.AreObjectsEqual(left, right));
            }

            [Test]
            public void WithDifferentObjects_ReturnsFalse()
            {
                List<object> leftListObject = new List<object>
                {
                    "ArCiGo",
                    "ElCiGo",
                    true
                };

                List<object> rightListObject = new List<object>
                {
                    "Néstor",
                    "Florencia",
                    1,
                    false
                };

                Assert.IsFalse(ObjectComparer.AreObjectsEqual(leftListObject, rightListObject));
            }
        }
        
        [TestFixture]
        public class AreStructsEqual : ObjectComparerTest
        {
            [Test]
            public void EqualStructs_ReturnsTrue()
            {
                var leftStruct = new SampleStruct()
                {
                    Name = "Alexey Turpalov",
                    Value = 1
                };

                var rightStruct = new SampleStruct()
                {
                    Name = "Alexey Turpalov",
                    Value = 1
                };

                Assert.IsTrue(ObjectComparer.AreStructsEqual(leftStruct, rightStruct));
            }

            [Test]
            public void DifferentStructs_ReturnsFalse()
            {
                var leftStruct = new SampleStruct()
                {
                    Name = "ArCiGo",
                    Value = 1
                };

                var rightStruct = new SampleStruct()
                {
                    Name = "Kadosh",
                    Value = 2
                };

                Assert.IsFalse(ObjectComparer.AreStructsEqual(leftStruct, rightStruct));
            }
        }

        [TestFixture]
        public class AreArraysEqual : ObjectComparerTest
        {
            [Test]
            public void EqualArrays_ReturnsTrue()
            {
                var leftArray = new[] { 1, 2, 3, 4, 5 };
                var rightArray = new[] { 1, 2, 3, 4, 5 };

                Assert.IsTrue(ObjectComparer.AreEnumsEqual(leftArray, rightArray));
            }

            [Test]
            public void DifferentArrays_ReturnsFalse()
            {
                var leftArray = new[] { 1, 2, 3 };
                var rightArray = new[] { 7, 1, 9 };

                Assert.IsFalse(ObjectComparer.AreEnumsEqual(leftArray, rightArray));
            }
        }

        [TestFixture]
        public class AreArrayObjectsEqual : ObjectComparerTest
        {
            [Test]
            public void EqualObjectsWithArrayProperties_ReturnsTrue()
            {
                var leftObject = new AdvArrayJson { Id = 1, Properties = new[] { BasicJson.GetDefault() } };
                var rightObject = new AdvArrayJson { Id = 1, Properties = new[] { BasicJson.GetDefault() } };

                Assert.IsTrue(ObjectComparer.AreEqual(leftObject, rightObject));
            }

            [Test]
            public void EqualArraysWithObjects_ReturnsTrue()
            {
                var leftArrayObject = new[] { BasicJson.GetDefault(), BasicJson.GetDefault() };
                var rightArrayObject = new[] { BasicJson.GetDefault(), BasicJson.GetDefault() };

                Assert.IsTrue(ObjectComparer.AreEnumsEqual(leftArrayObject, rightArrayObject));
            }
        }

        [TestFixture]
        public class AreEnumsEquals : ObjectComparerTest
        {
            [Test]
            public void EnumsWithDifferentLengths_ReturnsFalse()
            {
                List<string> leftListEnum = new List<string>
                {
                    "ArCiGo",
                    "ElCiGo",
                    "WizardexC137",
                    "DCOW"
                };

                List<string> rightListEnum = new List<string>
                {
                    "Kadosh"
                };

                Assert.IsFalse(ObjectComparer.AreEnumsEqual(leftListEnum.AsEnumerable(), rightListEnum.AsEnumerable()));
            }
        }

        [TestFixture]
        public class AreStructsEqualsInProps : ObjectComparerTest
        {
            [Test]
            public void StructsSameProps_ReturnsTrue()
            {
                var leftStruct = new SampleStructDifferent1()
                {
                    StudentId = 1,
                    Average = 98.10,
                    Notes = "Good"
                };

                var rightStruct = new SampleStructDifferent1()
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
                var leftStruct = new SampleStructDifferent1()
                {
                    StudentId = 1,
                    Average = 98.10,
                    Notes = "Good"
                };

                var rightStruct = new SampleStructDifferent1()
                {
                    StudentId = 2,
                    Average = 79.78,
                    Notes = "Ehmm, it could be better"
                };

                Assert.IsFalse(ObjectComparer.AreStructsEqual(leftStruct, rightStruct));
            }
        }
    }
}