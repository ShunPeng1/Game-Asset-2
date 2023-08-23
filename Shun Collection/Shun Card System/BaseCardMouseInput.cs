using System.Collections.Generic;
using Shun_Card_System;
using Shun_Utility;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Shun_Card_System
{
    public class BaseCardMouseInput 
    {
        protected Vector3 MouseWorldPosition;
        protected RaycastHit2D[] MouseCastHits;
    
        [Header("Hover Objects")]
        protected List<IMouseInteractable> LastHoverMouseInteractableGameObjects = new();
        public bool IsHoveringCard => LastHoverMouseInteractableGameObjects.Count != 0;

        [Header("Drag Objects")]
        protected Vector3 CardOffset;
        protected BaseCardGameObject DraggingCard;
        protected BaseCardRegion LastCardRegion;
        protected BaseCardHolder LastCardHolder;
        protected BaseCardButton LastCardButton;

        public bool IsDraggingCard
        {
            get;
            private set;
        }

    
        public virtual void UpdateMouseInput()
        {
            UpdateMousePosition();
            CastMouse();
            if(!IsDraggingCard) UpdateHoverObject();
        
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                StartDragCard();
            }

            if (Input.GetMouseButton(0))
            {
                DragCard();
            }

            if (Input.GetMouseButtonUp(0))
            {
                EndDragCard();
            }
        }

        #region CAST

        protected void UpdateMousePosition()
        {
            Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            MouseWorldPosition = new Vector3(worldMousePosition.x, worldMousePosition.y, 0);
        }

        protected void CastMouse()
        {
            MouseCastHits = Physics2D.RaycastAll(MouseWorldPosition, Vector2.zero);
        }

        #endregion
    

        #region HOVER

        protected virtual void UpdateHoverObject()
        {
            var hoveringMouseInteractableGameObject = FindAllIMouseInteractableInMouseCast();

            var endHoverInteractableGameObjects = SetOperations.SetDifference(LastHoverMouseInteractableGameObjects, hoveringMouseInteractableGameObject);
            var startHoverInteractableGameObjects =  SetOperations.SetDifference(hoveringMouseInteractableGameObject, LastHoverMouseInteractableGameObjects);

            foreach (var interactable in endHoverInteractableGameObjects)
            {
                if (interactable.IsHovering) interactable.EndHover();
            }
            foreach (var interactable in startHoverInteractableGameObjects)
            {
                if (!interactable.IsHovering) interactable.StartHover();
            }

            LastHoverMouseInteractableGameObjects = hoveringMouseInteractableGameObject;
        }
    
    
        protected virtual IMouseInteractable FindFirstIMouseInteractableInMouseCast()
        {
            foreach (var hit in MouseCastHits)
            {
                var characterCardButton = hit.transform.gameObject.GetComponent<BaseCardButton>();
                if (characterCardButton != null && characterCardButton.Interactable)
                {
                    //Debug.Log("Mouse find "+ gameObject.name);
                    return characterCardButton;
                }
            
                var characterCardGameObject = hit.transform.gameObject.GetComponent<BaseCardGameObject>();
                if (characterCardGameObject != null && characterCardGameObject.Interactable)
                {
                    //Debug.Log("Mouse find "+ gameObject.name);
                    return characterCardGameObject;
                }
            }

            return null;
        }
    
        protected virtual List<IMouseInteractable> FindAllIMouseInteractableInMouseCast()
        {
            List<IMouseInteractable> mouseInteractableGameObjects = new(); 
            foreach (var hit in MouseCastHits)
            {
                var characterCardButton = hit.transform.gameObject.GetComponent<BaseCardButton>();
                if (characterCardButton != null && characterCardButton.Interactable)
                {
                    mouseInteractableGameObjects.Add(characterCardButton);
                }
            
                var characterCardGameObject = hit.transform.gameObject.GetComponent<BaseCardGameObject>();
                if (characterCardGameObject != null && characterCardGameObject.Interactable)
                {
                    //Debug.Log("Mouse find "+ gameObject.name);
                    mouseInteractableGameObjects.Add(characterCardGameObject);
                }
            }

            return mouseInteractableGameObjects;
        }
    
        #endregion
    
    
        protected TResult FindFirstInMouseCast<TResult>()
        {
            foreach (var hit in MouseCastHits)
            {
                var result = hit.transform.gameObject.GetComponent<TResult>();
                if (result != null)
                {
                    //Debug.Log("Mouse find "+ gameObject.name);
                    return result;
                }
            }

            //Debug.Log("Mouse cannot find "+ typeof(TResult));
            return default;
        }

    
        protected bool StartDragCard()
        {
            // Check for button first
            LastCardButton = FindFirstInMouseCast<BaseCardButton>();

            if (LastCardButton != null && LastCardButton.Interactable)
            {
                LastCardButton.Select();
                return true;
            } 

            // Check for card game object second
            DraggingCard = FindFirstInMouseCast<BaseCardGameObject>();

            if (DraggingCard == null || !DraggingCard.Interactable || !DetachCardToHolder())
            {
                DraggingCard = null;
                return false;
            }
            
            // Successfully detach card
            CardOffset = DraggingCard.transform.position - MouseWorldPosition;
            IsDraggingCard = true;

            DraggingCard.Select();
    
            return true;
        
        }

        protected void DragCard()
        {
            if (!IsDraggingCard) return; 
        
            DraggingCard.transform.position = MouseWorldPosition + CardOffset;
        
        }

        protected void EndDragCard()
        {
            if (!IsDraggingCard) return;
        
            DraggingCard.Deselect();
            AttachCardToHolder();

            DraggingCard = null;
            LastCardHolder = null;
            LastCardRegion = null;
            IsDraggingCard = false;

        }
    
        protected virtual bool DetachCardToHolder()
        {
            // Check the card region base on card game object or card holder, to TakeOutTemporary
            LastCardRegion = FindFirstInMouseCast<BaseCardRegion>();
            if (LastCardRegion == null)
            {
                LastCardHolder = FindFirstInMouseCast<BaseCardHolder>();
                if (LastCardHolder == null)
                {
                    return true;
                }

                LastCardRegion = LastCardHolder.CardRegion;
            }
            else
            {
                LastCardHolder = LastCardRegion.FindCardPlaceHolder(DraggingCard);
            }

            // Having got the region and holder, take the card out temporary
            if (LastCardRegion.TakeOutTemporary(DraggingCard, LastCardHolder)) return true;
        
            LastCardHolder = null;
            LastCardRegion = null;

            return false;

        }

        protected void AttachCardToHolder()
        {
        
            var dropRegion = FindFirstInMouseCast<BaseCardRegion>();
            var dropHolder = FindFirstInMouseCast<BaseCardHolder>();
        
            if (dropHolder == null)
            {
                if (dropRegion != null && dropRegion != LastCardRegion &&
                    dropRegion.TryAddCard(DraggingCard, dropHolder)) // Successfully add to the drop region
                {
                    if (LastCardHolder != null) // remove the temporary in last region
                    {
                        LastCardRegion.RemoveTemporary(DraggingCard);
                        return;
                    }
                }
            
                if (LastCardRegion != null) // Unsuccessfully add to drop region or it is the same region
                    LastCardRegion.ReAddTemporary(DraggingCard);
            }
            else
            {
                if (dropRegion == null) 
                    dropRegion = dropHolder.CardRegion;
                
                if (dropRegion == null) // No region to drop anyway
                {
                    if(LastCardRegion != null) LastCardRegion.ReAddTemporary(DraggingCard);
                }

                if (dropRegion.CardMiddleInsertionStyle == BaseCardRegion.MiddleInsertionStyle.Swap)
                {
                    var targetCard = dropHolder.CardGameObject;
                    if (targetCard != null && LastCardRegion != null && dropRegion.TakeOutTemporary(targetCard, dropHolder))
                    {
                        LastCardRegion.ReAddTemporary(targetCard);
                        dropRegion.ReAddTemporary(DraggingCard);
                        
                        return;
                    }
                    
                }
                
                if (!dropRegion.TryAddCard(DraggingCard, dropHolder))
                {
                    if(LastCardRegion != null) LastCardRegion.ReAddTemporary(DraggingCard);
                }
                
                if (LastCardHolder != null)
                {
                    LastCardRegion.RemoveTemporary(DraggingCard);
                }

            }

        }
    
    }
}
