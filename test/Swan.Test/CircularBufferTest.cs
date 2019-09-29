using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Swan.Test
{
    [TestFixture]
    public class CircularBufferTest
    {
        [Test]
        public void CircularBufferInstance()
        {
            var buffer = new CircularBuffer(10);
            Assert.That(buffer, Is.Not.Null);
        }

        [Test]
        public void WriteTest()
        {
            var buffer = new CircularBuffer(10);
            var tag = new TimeSpan();

            buffer.Write(new IntPtr(1), 1, tag);

            Assert.That(tag.Ticks, Is.EqualTo(buffer.WriteTag.Ticks));
        }

        [Test]
        public void ReadInvalidTest()
        {
            var buffer = new CircularBuffer(10);

            Assert.Throws<InvalidOperationException>(
                () => buffer.Read(10, new byte[10], 0));
        }

        [Test]
        public void CapacityPercent()
        {
            var buffer = new CircularBuffer(10);
            Assert.That(buffer.CapacityPercent, Is.EqualTo(0));
        }

        [Test]
        public void ReadableCount()
        {
            var buffer = new CircularBuffer(10);
            Assert.That(buffer.ReadableCount, Is.EqualTo(0));
        }

        [Test]
        public void Clear()
        {
            var buffer = new CircularBuffer(10);
            var tag = new TimeSpan();

            // buffer.Write(new IntPtr(1), 1, tag); // Fix reference to DLL
            buffer.Clear();

            Assert.That(buffer.WriteIndex, Is.EqualTo(0));
            Assert.That(buffer.ReadIndex, Is.EqualTo(0));
            Assert.That(buffer.WriteTag, Is.EqualTo(TimeSpan.MinValue));
            Assert.That(buffer.ReadableCount, Is.EqualTo(0));
        }
    }
}
