using VContainer;

namespace Game.Core.Content.Converters.Interfaces
{
    public interface IInjectableConverter
    {
        void InjectDependencies(IObjectResolver resolver);
    }
}