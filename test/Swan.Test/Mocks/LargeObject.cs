namespace Swan.Test.Mocks
{
    public class LargeObject
    {
        public int InitializedBy { get { return initBy; } }

        int initBy = 0;
        public LargeObject(int initializedBy)
        {
            initBy = initializedBy;
            Console.WriteLine("LargeObject was created on thread id {0}.", initBy);
        }

        public long[] Data = new long[100000000];
    }
}
