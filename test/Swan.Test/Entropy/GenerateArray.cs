using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Swan.Test.Entropy
{
    public static class GenerateArray
    {
        public static (List<T>, List<Mock<T>>) GetMockList<T>(int maxSize = 25)
            where T : class
        {
            var entropy = new Random();
            var objectslist = new List<T>();
            var mockList = new List<Mock<T>>();

            var size = entropy.Next(maxSize);

            for (int i = 0; i < size; i++)
            {
                var o = new Mock<T>();
                mockList.Add(o);
                objectslist.Add(o.Object);
            }

            return (objectslist, mockList);
        }

        public static List<T> GetNullListOf<T>(int maxSize = 25)
        {
            var entropy = new Random();
            var size = entropy.Next(maxSize);

            var list = new List<T>();

            for (int i = 0; i < size; i++)
            {
                T o = default(T);
                list.Add(o);
            }

            return list;
        }
    }
}
