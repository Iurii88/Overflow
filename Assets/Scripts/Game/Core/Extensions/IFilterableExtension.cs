using System.Collections.Generic;
using Game.Core.Extensions.Filters;

namespace Game.Core.Extensions
{
    public interface IFilterableExtension : IExtension
    {
        IReadOnlyList<IExtensionFilter> Filters { get; }
    }
}
