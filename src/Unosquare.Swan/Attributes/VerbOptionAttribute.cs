using System;
 
namespace Unosquare.Swan.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class VerbOptionAttribute : Attribute
    {
        public VerbOptionAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; }
    }
}
