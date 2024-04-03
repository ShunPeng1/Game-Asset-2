using System;

namespace Shun_Utilities
{
    public class EventBinding<T> : IEventBinding<T>
    {
        public Action<T> OnEvent { get; set; } = _ => { };
        public Action OnEventNoArgs { get; set; } = () => { };
        
        Action<T> IEventBinding<T>.OnEvent { get => OnEvent; set => OnEvent = value; }
        Action IEventBinding<T>.OnEventNoArgs { get => OnEventNoArgs; set => OnEventNoArgs = value; }
        
        public EventBinding(Action<T> onEvent, Action onEventNoArgs)
        {
            OnEvent = onEvent;
            OnEventNoArgs = onEventNoArgs;
        }
        
        public EventBinding(Action<T> onEvent) => OnEvent = onEvent;

        public EventBinding(Action onEventNoArgs) => OnEventNoArgs = onEventNoArgs;
        
        public void Add(Action<T> onEvent) => OnEvent += onEvent;
        public void Add(Action onEventNoArgs) => OnEventNoArgs += onEventNoArgs;
        
        public void Remove(Action<T> onEvent) => OnEvent -= onEvent;
        public void Remove(Action onEventNoArgs) => OnEventNoArgs -= onEventNoArgs;
    }
}