namespace Swan.Test.Mocks
{
    public class LargeObject
    {
        public int InitializedBy { get { return initBy; } }

        int initBy = 0;
        public LargeObject(int initializedBy)
        {
            initBy = initializedBy;
        }

        public long[] Data = new long[100000000];
    }
}
