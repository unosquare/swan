using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unosquare.Swan.Components;

namespace Unosquare.Swan.Test.Mocks
{
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
