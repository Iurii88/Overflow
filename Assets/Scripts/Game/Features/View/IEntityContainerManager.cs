using System;
using UnityEngine;

namespace Game.Features.View
{
    public interface IEntityContainerManager : IDisposable
    {
        Transform GetOrCreateEntityContainer();
    }
}