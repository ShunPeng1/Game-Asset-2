using UnityEngine;

namespace Shun_Utilities
{
    [AddComponentMenu("ServiceLocator/GlobalBootstrapper")]
    public class ServiceLocatorGlobalBootstrapper : Bootstrapper
    {
        [SerializeField] bool _dontDestroyOnLoad = true;
        
        protected override void Bootstrap()
        {
            // configure global services
            Container.ConfigureAsGlobal(_dontDestroyOnLoad);
            
        }
    }
}