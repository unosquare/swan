using NUnit.Framework;
using Swan.Configuration;
using System;

namespace Swan.Test
{
    public class ConfiguredObjectTest
    {
        [Test]
        public void BeforeLock_IsNotLocked()
        {
            var obj = new TestObject();
            Assert.IsFalse(obj.ConfigurationLocked);
        }

        [Test]
        public void AfterLock_IsLocked()
        {
            var obj = new TestObject();
            obj.LockConfiguration();
            Assert.IsTrue(obj.ConfigurationLocked);
        }

        [Test]
        public void OnBeforeLockConfiguration_BeforeLock_HasNotBeenCalled()
        {
            var obj = new TestObject();
            Assert.IsFalse(obj.OnBeforeLockConfigurationCalled);
        }

        [Test]
        public void OnBeforeLockConfiguration_AfterLock_HasBeenCalled()
        {
            var obj = new TestObject();
            obj.LockConfiguration();
            Assert.IsTrue(obj.OnBeforeLockConfigurationCalled);
        }

        [Test]
        public void LockConfiguration_AfterLock_Succeeds()
        {
            var obj = new TestObject();
            obj.LockConfiguration();
            Assert.DoesNotThrow(() => obj.LockConfiguration());
        }

        [Test]
        public void EnsureConfigurationNotLocked_BeforeLock_Succeeds()
        {
            var obj = new TestObject();
            Assert.DoesNotThrow(() => obj.EnsureConfigurationNotLocked());
        }

        [Test]
        public void EnsureConfigurationNotLocked_AfterLock_ThrowsInvalidOperationException()
        {
            var obj = new TestObject();
            obj.LockConfiguration();
            Assert.Throws<InvalidOperationException>(() => obj.EnsureConfigurationNotLocked());
        }

        private class TestObject : ConfiguredObject
        {
            public bool OnBeforeLockConfigurationCalled { get; private set; }

            public new bool ConfigurationLocked => base.ConfigurationLocked;

            public new void LockConfiguration() => base.LockConfiguration();

            public new void EnsureConfigurationNotLocked() => base.EnsureConfigurationNotLocked();

            protected override void OnBeforeLockConfiguration() => OnBeforeLockConfigurationCalled = true;
        }
    }
}