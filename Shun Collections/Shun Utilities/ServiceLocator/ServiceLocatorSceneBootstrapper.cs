using UnityEngine;

namespace Shun_Utilities
{
    [AddComponentMenu("ServiceLocator/SceneBootstrapper")]
    public class ServiceLocatorSceneBootstrapper : Bootstrapper
    {
        protected override void Bootstrap()
        {
            // configure scene services
            Container.ConfigureForScene();
        }
    }
}