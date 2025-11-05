using System;
using Cysharp.Threading.Tasks;
using Game.Features.Entities.Content;
using UnsafeEcs.Core.Entities;

namespace Game.Core.Extensions
{
    public interface IExtensionExecutor
    {
        UniTask ExecuteAsync<TExtension>(Entity entity, ContentEntity contentEntity, Func<TExtension, UniTask> action)
            where TExtension : IExtension;

        void Execute<TExtension>(Entity entity, ContentEntity contentEntity, Action<TExtension> action)
            where TExtension : IExtension;
    }
}
