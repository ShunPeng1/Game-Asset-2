
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Shun_Card_System
{
    [RequireComponent(typeof(Collider2D))]
    public class BaseCardGameObject : MonoBehaviour, IMouseInteractable
    {
        [SerializeField]
        private bool _interactable;
        public bool Interactable { get => _interactable; protected set => _interactable = value; }
        public bool IsHovering { get; protected set; }
        
        [SerializeField] protected bool ActivateOnValidate = false;
        

        private void OnValidate()
        {
            if (ActivateOnValidate) ValidateInformation();
        }
        
        protected virtual void ValidateInformation()
        {
            
        }

        

        public virtual void Select()
        {
            
        }

        public virtual void Deselect()
        {
            
        }

        public virtual void StartHover()
        {
            IsHovering = true;
        }

        public virtual void EndHover()
        {
            IsHovering = false;
        }
        
        public virtual void DisableInteractable()
        {
            if (!Interactable) return;
            Interactable = false;
            if (IsHovering) EndHover();
        }
        
        public virtual void EnableInteractable()
        {
            if (Interactable) return;
            Interactable = true;
        }

        
    }
}
