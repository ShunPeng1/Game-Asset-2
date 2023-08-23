using UnityEngine;

namespace Shun_Card_System
{
    /// <summary>
    /// This class is the card place holder of a card object in card place region.
    /// This can be used to move, animations,...
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BaseCardHolder : MonoBehaviour
    {
        [HideInInspector] public BaseCardRegion CardRegion;
        [HideInInspector] public int IndexInRegion;
        public BaseCardGameObject CardGameObject;

        
        public void InitializeRegion(BaseCardRegion cardRegion, int indexInRegion)
        {
            CardRegion = cardRegion;
            IndexInRegion = indexInRegion;
            transform.parent = cardRegion.transform;
        }
        
        public void AttachCardGameObject(BaseCardGameObject cardGameObject)
        {
            if (cardGameObject == null) return;
            
            CardGameObject = cardGameObject;
            CardGameObject.transform.SetParent(transform, true);
            
            CardGameObject.DisableInteractable();
            AttachCardVisual();
        }

        public BaseCardGameObject DetachCardGameObject()
        {
            if (CardGameObject == null) return null;
            
            BaseCardGameObject detachedCard = CardGameObject;
            
            detachedCard.transform.SetParent(CardRegion.transform.parent, true);
            
            
            CardGameObject = null;

            return detachedCard;
        }

        protected virtual void AttachCardVisual()
        {
            CardGameObject.transform.localPosition = Vector3.zero;
            CardGameObject.EnableInteractable();
        }
        
        
    }
}