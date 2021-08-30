using Swan.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Swan.Mapping
{
    /// <summary>
    /// Represents a map to go from a single or multi-level property into a target property.
    /// </summary>
    public sealed class MapPath
    {
        private readonly List<IPropertyProxy> m_SourcePath = new(8);

        internal MapPath(IPropertyProxy targetMember, IEnumerable<IPropertyProxy> sourcePath)
        {
            if (targetMember is null)
                throw new ArgumentNullException(nameof(targetMember));

            if (sourcePath is null || !sourcePath.Any())
                throw new ArgumentException($"The {nameof(sourcePath)} must countain at least 1 item to build a path.");

            TargetMember = targetMember;
            m_SourcePath.AddRange(sourcePath);
        }

        /// <summary>
        /// A property representing a target property into which values are written.
        /// </summary>
        public IPropertyProxy TargetMember { get; }

        /// <summary>
        /// An ordered property path from which values are read.
        /// </summary>
        public IReadOnlyList<IPropertyProxy> SourcePath => m_SourcePath;

        /// <inheridoc />
        public override string ToString()
        {
            return $"{TargetMember.PropertyName} = {string.Join('.', SourcePath.Select(c => c.PropertyName))}";
        }
    }
}
