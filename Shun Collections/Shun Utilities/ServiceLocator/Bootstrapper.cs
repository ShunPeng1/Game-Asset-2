using UnityEngine;

namespace Shun_Utilities
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ServiceLocator))]
    public abstract class Bootstrapper : MonoBehaviour
    {
        private ServiceLocator _container;
        internal ServiceLocator Container => _container.OrNull() ?? (_container = GetComponent<ServiceLocator>());
        
        bool _hasBeenBootstrapped;
        
        public void BootstrapOnDemand()
        {
            if (_hasBeenBootstrapped) return;
            
            Bootstrap();
            _hasBeenBootstrapped = true;
        }
        
        protected abstract void Bootstrap(); 
    }
}