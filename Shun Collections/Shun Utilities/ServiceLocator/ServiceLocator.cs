using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shun_Utilities
{
    public class ServiceLocator : MonoBehaviour
    {
        static ServiceLocator global;
        static Dictionary<Scene, ServiceLocator> sceneContainers = new Dictionary<Scene, ServiceLocator>();
        static List<GameObject> tmpSceneGameObjects;
        
        readonly ServiceManager _services = new ServiceManager();
        
        const string GLOBAL_SERVICE_LOCATOR_NAME = "ServiceLocator [Global]";
        const string SCENE_SERVICE_LOCATOR_NAME = "ServiceLocator [Scene]";
        
        internal void ConfigureAsGlobal(bool dontDestroyOnLoad) 
        {
            if (global == this)
            {
                Debug.LogWarning("ServiceLocator.ConfigureAsGlobal: already configured as global", this);
            }
            else if (global != null)
            {
                Debug.LogError("ServiceLocator.ConfigureAsGlobal: Another ServiceLocator is already configured as global", this);
            }
            else
            {
                global = this;
                
                if (dontDestroyOnLoad)
                {
                    DontDestroyOnLoad(gameObject);
                }
                
            }
            
        }

        internal void ConfigureForScene()
        {
            Scene scene = gameObject.scene;
            
            if (sceneContainers.ContainsKey(scene))
            {
                Debug.LogError("ServiceLocator.ConfigureForScene: Another ServiceLocator is already configured for scene " + scene.name, this);
                return;
            }
            
            sceneContainers.Add(scene, this);
        }
        
        public static ServiceLocator Global
        {
            get
            {
                if (global != null) return global;
                
                // bootstrap or initialize the global service locator

                if (FindFirstObjectByType<ServiceLocatorGlobalBootstrapper>() is { } found)
                {
                    found.BootstrapOnDemand();
                    return global;
                }

                var container = new GameObject(GLOBAL_SERVICE_LOCATOR_NAME, typeof(ServiceLocator));
                container.AddComponent<ServiceLocatorGlobalBootstrapper>().BootstrapOnDemand();
        
                
                return global;
            }
        }

        
        public static ServiceLocator For(MonoBehaviour mb) {
            return mb.GetComponentInParent<ServiceLocator>().OrNull() ?? ForSceneOf(mb) ?? Global;
        }
        
        
        public static ServiceLocator ForSceneOf(MonoBehaviour monoBehaviour)
        {
            Scene scene = monoBehaviour.gameObject.scene;
            
            if (sceneContainers.TryGetValue(scene, out ServiceLocator container) && container != monoBehaviour)
            {
                return container;
            }
           
            tmpSceneGameObjects.Clear();
            scene.GetRootGameObjects(tmpSceneGameObjects);
            
            foreach (GameObject go in tmpSceneGameObjects.Where(go1 => go1.GetComponent<ServiceLocatorSceneBootstrapper>() != null))
            {
                if (go.TryGetComponent(out ServiceLocatorSceneBootstrapper bootstrapper) && bootstrapper.Container != monoBehaviour)
                {
                    bootstrapper.BootstrapOnDemand();
                    return bootstrapper.Container;
                }
                
            }

            return Global;
        }
        
        public ServiceLocator Register<T>(T service, bool replace = false)
        {
            _services.Register(service, replace);
            return this;
        }
        
        public ServiceLocator Register(Type type, object service)
        {
            _services.Register(type, service);
            return this;
        }
        
        public ServiceLocator Get<T>(out T service) where T : class
        {
            if (TryGetService(out service))
            {
                return this;
            }
            
            if (TryGetNextInHierarchy(out ServiceLocator container))
            {
                container.Get(out service);
                return this;
            }
            
            throw new ArgumentException("ServiceLocator.Get: Service of type " + typeof(T).FullName + " not registered");
 
            
        }
        
        bool TryGetService<T>(out T service) where T : class
        {
            return _services.TryGet(out service);
        }
        
        
        bool TryGetNextInHierarchy(out ServiceLocator container)
        {
            if (this == global)
            {
                container = null;
                return false;
            }

            container = transform.parent.OrNull()?.GetComponentInParent<ServiceLocator>().OrNull() ?? ForSceneOf(this);
            return container != null;
        }

        private void OnDestroy()
        {
            if (this == global)
            {
                global = null;
            }
            else if (sceneContainers.ContainsValue(this))
            {
                sceneContainers.Remove(gameObject.scene);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            global = null;
            sceneContainers = new Dictionary<Scene, ServiceLocator>();
            tmpSceneGameObjects = new List<GameObject>();
        }
        
# if UNITY_EDITOR
        [MenuItem("GameObject/ServiceLocator/Add Global")]
        static void AddGlobalServiceLocator(MenuCommand menuCommand)
        {
            var go = new GameObject(GLOBAL_SERVICE_LOCATOR_NAME, typeof(ServiceLocatorGlobalBootstrapper));
            
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
        
        [MenuItem("GameObject/ServiceLocator/Add Scene")]
        static void AddSceneServiceLocator(MenuCommand menuCommand)
        {
            var go = new GameObject(SCENE_SERVICE_LOCATOR_NAME, typeof(ServiceLocatorSceneBootstrapper));
            
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
        
# endif
        
    }
}