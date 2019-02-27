namespace Unosquare.Swan.Test
{
    using NUnit.Framework;
    using Components;

    [TestFixture]
    public class PaginatorTest
    {
        [Test]
        public void WithValidData_ReturnPageSize()
        {
            var stu = new Paginator(100, 10);
            Assert.AreEqual(10, stu.PageSize);
        }

        [Test]
        public void WithValidData_ReturnPageCount()
        {
            var stu = new Paginator(100, 10);
            Assert.AreEqual(10, stu.PageCount);
        }

        [Test]
        public void WithValidData_ReturnTotalCount()
        {
            var stu = new Paginator(100, 10);
            Assert.AreEqual(100, stu.TotalCount);
        }

        [Test]
        public void WithValidDataAtIndexNine_ReturnGetItemCount()
        {
            var stu = new Paginator(99, 10);
            Assert.AreEqual(9, stu.GetItemCount(9));
        }
        
        [Test]
        public void WithValidDataAtIndexNine_ReturnGetFirstItemIndex()
        {
            var stu = new Paginator(99, 10);
            Assert.AreEqual(90, stu.GetFirstItemIndex(9));
        }

        [Test]
        public void WithValidDataAtIndexNine_ReturnGetLastItemIndex()
        {
            var stu = new Paginator(99, 10);
            Assert.AreEqual(98, stu.GetLastItemIndex(9));
        }
    }
}
