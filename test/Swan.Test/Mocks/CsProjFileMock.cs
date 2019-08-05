namespace Swan.Test.Mocks
{
    using System;
    using Formatters;

    public class CsMetadataMock : CsProjMetadataBase
    {
        public string Copyright => FindElement(nameof(Copyright))?.Value;

        public string NonExistentProp => FindElement(nameof(NonExistentProp))?.Value;

        public override void ParseCsProjTags(ref string[] args)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class CsAbstractMetadataMock : CsProjMetadataBase
    {
        public override void ParseCsProjTags(ref string[] args)
        {
            throw new NotImplementedException();
        }
    }
}
