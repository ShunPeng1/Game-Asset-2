using System.Collections.Generic;

namespace Shun_Utilities
{
    public class EventBus<T> where T : IEventBus
    {
        private static readonly HashSet<IEventBinding<T>> Bindings = new HashSet<IEventBinding<T>>();
        
        public static void Register(IEventBinding<T> binding) => Bindings.Add(binding);
        public static void Unregister(IEventBinding<T> binding) => Bindings.Remove(binding);
        public static void UnregisterAll() => Bindings.Clear();
        
        public static void Raise(T @eventArgs)
        {
            foreach (var binding in Bindings)
            {
                binding.OnEvent.Invoke(@eventArgs);
                binding.OnEventNoArgs.Invoke();
            }
        }
        
        
    }
}