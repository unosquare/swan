namespace Swan.Test.ExtensionsTest
{
    using Mocks;
    using NUnit.Framework;
    using Diagnostics;
    using Net;
    using System;
    using System.Collections.Generic;

    [TestFixture]
    public class BenchmarkTest : TestFixtureBase
    {
        [Test]
        public void WithAction_ReturnsTimeSpan()
        {
            const int total = 0;
            var action = new Action(() =>
            {
                if (total < 2)
                    throw new Exception();
            });

            var result = Benchmark.BenchmarkAction(action);

            Assert.IsNotNull(result);
        }

        [Test]
        public void WithEmptyAction_ReturnsTimeSpan()
        {
            var action = new Action(() => { });

            var result = Benchmark.BenchmarkAction(action);

            Assert.IsNotNull(result);
        }

        [Test]
        public void WithNullAction_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Benchmark.BenchmarkAction(NullAction));
        }
    }

    [TestFixture]
    public class Retry : TestFixtureBase
    {
        [Test]
        public void WithNewFunction_RetryAction()
        {
            var total = 0;

            var action = new Func<int>(() => total++ < 2 ? throw new Exception() : total);

            var result = action.Retry();
            Assert.AreEqual(3, result);
        }

        [Test]
        public void WithInvalidAction_ThrowsAggregateException()
        {
            Assert.Throws<AggregateException>(() =>
            {
                var action =
                    new Action(() => JsonClient.GetString(new Uri("http://accesscore.azurewebsites.net/api/token")).Wait());

                action.Retry();
            });
        }

        [Test]
        public void WithValidAction_DoesNotThrowException()
        {
            var total = 0;

            Assert.DoesNotThrow(() =>
            {
                var action = new Action(() =>
                {
                    if (total++ < 2)
                        throw new Exception();
                });

                action.Retry();
            });
        }

        [Test]
        public void WithNullAction_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => NullAction.Retry());
        }

        [Test]
        public void WithNullFunction_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ((Func<int>) null).Retry());
        }
    }

    [TestFixture]
    public class CopyPropertiesTo
    {
        [Test]
        public void WithValidObjectAttr_CopyPropertiesToTarget()
        {
            var source = ObjectAttr.GetDefault();
            var target = new ObjectAttr();

            source.CopyPropertiesTo(target);

            Assert.AreEqual(source.Name, target.Name);
            Assert.AreEqual(source.IsActive, target.IsActive);
        }

        [Test]
        public void WithValidBasicJson_CopyPropertiesToTarget()
        {
            var source = BasicJson.GetDefault();
            var destination = new BasicJson();

            source.CopyPropertiesTo(destination);

            Assert.AreEqual(source.BoolData, destination.BoolData);
            Assert.AreEqual(source.DecimalData, destination.DecimalData);
            Assert.AreEqual(source.StringData, destination.StringData);
            Assert.AreEqual(source.StringNull, destination.StringNull);
        }

        [Test]
        public void WithNullObjectAttr_CopyPropertiesToTarget()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectAttr.GetDefault().CopyPropertiesTo(null));
        }

        [Test]
        public void WithValidParamsAndNewProperty_CopyPropertiesToTarget()
        {
            var source = BasicJson.GetDefault();
            source.StringNull = "1";

            var destination = new BasicJsonWithNewProperty();

            source.CopyPropertiesTo(destination);

            Assert.AreEqual(source.BoolData, destination.BoolData);
            Assert.AreEqual(source.DecimalData, destination.DecimalData);
            Assert.AreEqual(source.StringData, destination.StringData);
            Assert.AreEqual(source.StringNull, destination.StringNull.ToString());
        }

        [Test]
        public void WithValidBasicJson_CopyNotIgnoredPropertiesToTarget()
        {
            var source = BasicJson.GetDefault();
            var destination = new BasicJson();

            source.CopyPropertiesTo(destination, nameof(BasicJson.NegativeInt), nameof(BasicJson.BoolData));

            Assert.AreNotEqual(source.BoolData, destination.BoolData);
            Assert.AreNotEqual(source.NegativeInt, destination.NegativeInt);
            Assert.AreEqual(source.StringData, destination.StringData);
        }

        [Test]
        public void WithValidObjectWithArray_CopyPropertiesToTarget()
        {
            var source = new ObjectWithArray {Data = new[] {"HOLA", "MUNDO "}};
            var target = new ObjectWithArray();

            source.CopyPropertiesTo(target);

            Assert.AreEqual(source.Data, target.Data);
        }
    }

    [TestFixture]
    public class CopyPropertiesToNew
    {
        [Test]
        public void WithObjectWithCopyableAttribute_CopyPropertiesToNewObjectAttr()
        {
            var source = ObjectAttr.GetDefault();

            var destination = source.CopyPropertiesToNew<ObjectAttr>();

            Assert.IsNotNull(destination);
            Assert.AreSame(source.GetType(), destination.GetType());
            Assert.AreNotEqual(source.Id, destination.Id);
            Assert.AreEqual(source.Name, destination.Name);
            Assert.AreEqual(source.IsActive, destination.IsActive);
        }

        [Test]
        public void WithValidParams_CopyPropertiesToNewObject()
        {
            var source = new ObjectEnum
            {
                Id = 1,
                MyEnum = MyEnum.Two,
            };

            var result = source.CopyPropertiesToNew<ObjectEnum>();
            Assert.AreEqual(source.MyEnum, result.MyEnum);
        }

        [Test]
        public void WithValidBasicJson_CopyPropertiesToNewBasicJson()
        {
            var source = BasicJson.GetDefault();
            var destination = source.CopyPropertiesToNew<BasicJson>();

            Assert.IsNotNull(destination);
            Assert.AreSame(source.GetType(), destination.GetType());
            Assert.AreEqual(source.BoolData, destination.BoolData);
            Assert.AreEqual(source.DecimalData, destination.DecimalData);
            Assert.AreEqual(source.StringData, destination.StringData);
            Assert.AreEqual(source.StringNull, destination.StringNull);
        }

        [Test]
        public void WithNullSource_ThrowsArgumentNullException()
        {
            ObjectEnum? source = null;

            Assert.Throws<ArgumentNullException>(() => source.CopyPropertiesToNew<ObjectEnum>());
        }

        [Test]
        public void WithValidDictionary_CopyPropertiesToTarget()
        {
            var source = new Dictionary<string, object>
            {
                {nameof(UserDto.Name), "Thrall"},
                {nameof(UserDto.Email), "Warchief.Thrall@horde.com"},
                {nameof(UserDto.Role), "Warchief"},
                {nameof(UserDto.IsAdmin), 1},
            };

            var target = source.CopyKeyValuePairToNew<UserDto>();

            Assert.AreEqual(source[nameof(UserDto.Name)].ToString(), target.Name);
            Assert.AreEqual(source[nameof(UserDto.Email)], target.Email);
            Assert.IsTrue(target.IsAdmin);
        }

        [Test]
        public void WithNullDictionary_ThrowsArgumentNullException()
        {
            Dictionary<string, object>? source = null;

            Assert.Throws<ArgumentNullException>(() => source.CopyKeyValuePairToNew<ObjectEnum>());
        }

        [Test]
        public void WithValidObjectAttr_CopyPropertiesToTarget()
        {
            var source = ObjectAttr.GetDefault();
            var target = new ObjectAttr();

            source.CopyPropertiesTo(target);

            Assert.AreEqual(source.Name, target.Name);
            Assert.AreEqual(source.IsActive, target.IsActive);
        }
    }

    [TestFixture]
    public class CopyOnlyPropertiesTo
    {
        [Test]
        public void WithValidBasicJson_CopyOnlyPropertiesToTarget()
        {
            var source = BasicJson.GetDefault();
            var destination = new BasicJson {NegativeInt = 800, BoolData = false};
            source.CopyOnlyPropertiesTo(destination, nameof(BasicJson.NegativeInt), nameof(BasicJson.BoolData));

            Assert.AreEqual(source.BoolData, destination.BoolData);
            Assert.AreEqual(source.NegativeInt, destination.NegativeInt);
            Assert.AreNotEqual(source.StringData, destination.StringData);
        }
    }

    [TestFixture]
    public class CopyOnlyPropertiesToNew : TestFixtureBase
    {
        [Test]
        public void WithValidParams_CopyOnlyPropertiesToNewObject()
        {
            var source = ObjectAttr.GetDefault();
            var target = source.CopyOnlyPropertiesToNew<ObjectAttr>(nameof(ObjectAttr.Name));
            Assert.AreEqual(source.Name, target.Name);
        }

        [Test]
        public void WithNullSource_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                NullObj.CopyOnlyPropertiesToNew<ObjectAttr>(nameof(ObjectAttr.Name)));
        }

        [Test]
        public void WithValidBasicJson_CopyOnlyPropertiesToNewBasicJson()
        {
            var source = BasicJson.GetDefault();
            var destination = source.CopyOnlyPropertiesToNew<BasicJson>(nameof(BasicJson.BoolData), nameof(BasicJson.DecimalData));

            Assert.IsNotNull(destination);
            Assert.AreSame(source.GetType(), destination.GetType());

            Assert.AreEqual(source.BoolData, destination.BoolData);
            Assert.AreEqual(source.DecimalData, destination.DecimalData);
        }
    }
}
