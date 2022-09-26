namespace Swan.Test.Mocks
{
    public class LargeObject
    {
        public int InitializedBy => initBy;

        int initBy;
        public LargeObject(int initializedBy)
        {
            initBy = initializedBy;
        }

        public long[] Data = new long[100000000];
    }
}
