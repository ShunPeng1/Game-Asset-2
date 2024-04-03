using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shun_Utilities
{
    public class ServiceManager
    {
        readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        public IEnumerable<object> Services => _services.Values;
        
        public bool TryGet<T> (out T service) where T : class
        {
            Type type = typeof(T);
            if (_services.TryGetValue(type, out var obj))
            {
                service = obj as T;
                return true;
            }
            else
            {
                service = default;
                return false;
            }
            
        }
        
        public T Get<T>() where T : class
        {
            Type type = typeof(T);
            if (_services.TryGetValue(type, out var obj))
            {
                return obj as T;
            }
            else
            {
                throw new ArgumentException("ServiceManager.Get: Service of type " + type.FullName + " does not registered");
            }
        }
        
        public ServiceManager Register<T>(T service, bool replace = false)
        {
            Type type = typeof(T);

            if (_services.ContainsKey(type))
            {
                if (replace)
                {
                    _services[type] = service;
                }
                else
                {
                    Debug.LogError("ServiceManager.Register: Service of type " + type + " already exists");
                }
            }
            else
            {
                _services.Add(type, service);
            }
            
            return this;
        }
        
        public ServiceManager Register(Type type, object service, bool replace = false)
        {
            if (!type.IsInstanceOfType(service))
            {
                throw new ArgumentException("ServiceManager.Register: Service of type " + type + " is not of type " + type + "");
            }
            
            if (_services.ContainsKey(type))
            {
                if (replace)
                {
                    _services[type] = service;
                }
                else
                {
                    Debug.LogError("ServiceManager.Register: Service of type " + type + " already exists");
                }
            }
            else
            {
                _services.Add(type, service);
            }
            
            return this;
        }

        
    }
}