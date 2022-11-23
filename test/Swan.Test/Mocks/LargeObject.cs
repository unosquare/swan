namespace Swan.Test.Mocks;

public class LargeObject
{
    public int InitializedBy { get; }

    public LargeObject(int initializedBy)
    {
        InitializedBy = initializedBy;
    }

    public long[] Data = new long[100000000];
}
