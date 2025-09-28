using System.Reflection;
using UnityEngine;
using UnsafeEcs.Core.Bootstrap;

namespace Game.Core
{
    public class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            var gameAssembly = Assembly.GetExecutingAssembly();
            var ecsAssembly = Assembly.Load("UnsafeEcs");
            var assemblies = new[]
            {
                gameAssembly,
                ecsAssembly
            };
            WorldBootstrap.Initialize(assemblies);
        }
    }
}