using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Shun_Card_System
{
    [RequireComponent(typeof(Collider2D))]
    public class BaseCardRegion : MonoBehaviour, IMouseInteractable
    {
        public enum MiddleInsertionStyle
        {
            AlwaysBack,
            InsertInMiddle,
            Cannot,
            Swap,
        }
        [SerializeField] protected BaseCardHolder CardHolderPrefab;
        [SerializeField] protected Transform SpawnPlace;
        [SerializeField] protected Vector3 CardOffset = new Vector3(5f, 0 ,0);

        
        [SerializeField] protected List<BaseCardHolder> _cardPlaceHolders = new();
        [SerializeField] protected int MaxCardHold;
        public MiddleInsertionStyle CardMiddleInsertionStyle = MiddleInsertionStyle.InsertInMiddle;
        protected BaseCardHolder TemporaryBaseCardHolder;
        public int CardHoldingCount { get; private set; } 
        
        
        [SerializeField]
        private bool _interactable;
        public bool Interactable { get => _interactable; protected set => _interactable = value;}
        public bool IsHovering { get; protected set; }

        #region INITIALIZE

        protected virtual void Awake()
        {
            InitializeCardPlaceHolder();
        }

        protected void InitializeCardPlaceHolder()
        {
            if (_cardPlaceHolders.Count != 0)
            {
                MaxCardHold = _cardPlaceHolders.Count;
                for (int i = 0; i < MaxCardHold; i++)
                {
                    _cardPlaceHolders[i].InitializeRegion(this, i);
                }
            }
            else
            {
                for (int i = 0; i < MaxCardHold; i++)
                {
                    var cardPlaceHolder = Instantiate(CardHolderPrefab, SpawnPlace.position + i * CardOffset,
                        Quaternion.identity, SpawnPlace);
                    _cardPlaceHolders.Add(cardPlaceHolder);
                    cardPlaceHolder.InitializeRegion(this, i);
                }    
            }
            
        }
        
        #endregion

        #region OPERATION

        
        public List<BaseCardGameObject> GetAllCardGameObjects(bool getNull = false)
        {
            List<BaseCardGameObject> result = new();
            for (int i = 0; i < CardHoldingCount; i++)
            {
                if ((!getNull && _cardPlaceHolders[i].CardGameObject != null) || getNull) result.Add(_cardPlaceHolders[i].CardGameObject);
            }

            return result;
        }

        public void DestroyAllCardGameObject()
        {
            foreach (var cardHolder in _cardPlaceHolders)
            {
                if (cardHolder.CardGameObject == null) continue;
                Destroy(cardHolder.CardGameObject.gameObject);
                cardHolder.CardGameObject = null;
            }
            
            CardHoldingCount = 0;
        }

        protected BaseCardHolder FindEmptyCardPlaceHolder()
        {
            if (CardHoldingCount >= MaxCardHold) return null;
            return _cardPlaceHolders[CardHoldingCount];
        }
        
        public BaseCardHolder FindCardPlaceHolder(BaseCardGameObject baseCardGameObject)
        {
            foreach (var cardPlaceHolder in _cardPlaceHolders)
            {
                if (cardPlaceHolder.CardGameObject == baseCardGameObject) return cardPlaceHolder;
            }

            return null;
        }

        public bool AddCard(BaseCardGameObject cardGameObject, BaseCardHolder cardHolder = null)
        {
            if ( cardHolder == null || cardHolder.IndexInRegion >= CardHoldingCount)
            {
                return AddCardAtBack(cardGameObject);
            }

            return CardMiddleInsertionStyle switch
            {
                MiddleInsertionStyle.AlwaysBack => AddCardAtBack(cardGameObject),
                MiddleInsertionStyle.InsertInMiddle => AddCardAtMiddle(cardGameObject, cardHolder.IndexInRegion),
                MiddleInsertionStyle.Cannot => false,
                _ => false
            };
        }

        private bool AddCardAtBack(BaseCardGameObject cardGameObject)
        {
            if (CardHoldingCount >= MaxCardHold)
            {
                return false;
            }

            var index = CardHoldingCount;
            var cardPlaceHolder = _cardPlaceHolders[index];
            cardPlaceHolder.AttachCardGameObject(cardGameObject);
            
            CardHoldingCount ++;
            
            OnSuccessfullyAddCard(cardGameObject, cardPlaceHolder, index);
                
            return true;
        }
        
        private  bool AddCardAtMiddle(BaseCardGameObject cardGameObject, int index)
        {
            if (CardHoldingCount >= MaxCardHold)
            {
                return false;
            }
            
            ShiftRight(index);

            var cardPlaceHolder = _cardPlaceHolders[index];
            cardPlaceHolder.AttachCardGameObject(cardGameObject);
            
            CardHoldingCount++;
            
            OnSuccessfullyAddCard(cardGameObject, cardPlaceHolder, index);
            
            return true;
        }
        
        
        protected virtual void ShiftRight(int startIndex)
        {
            for (int i = _cardPlaceHolders.Count - 1; i > startIndex; i--)
            {
                var card = _cardPlaceHolders[i - 1].DetachCardGameObject();
                
                if (card == null) continue;
                _cardPlaceHolders[i].AttachCardGameObject(card);
                
                //SmoothMove(card.transform, _cardPlaceHolders[i].transform.position);

            }
        }
        
        
        protected virtual void ShiftLeft(int startIndex)
        {
            for (int i = startIndex; i < _cardPlaceHolders.Count - 1; i++)
            {
                var card = _cardPlaceHolders[i + 1].DetachCardGameObject();
                
                if (card == null) continue;
                
                _cardPlaceHolders[i].AttachCardGameObject(card);
                
                
                //SmoothMove(card.transform, _cardPlaceHolders[i].transform.position);

            }
        }
        
        public virtual bool RemoveCard(BaseCardGameObject cardGameObject)
        {

            for (int i = 0; i < _cardPlaceHolders.Count; i++)
            {
                if (_cardPlaceHolders[i].CardGameObject != cardGameObject) continue;
                _cardPlaceHolders[i].DetachCardGameObject();
                
                ShiftLeft(i);
                CardHoldingCount--;
                
                OnSuccessfullyRemoveCard(cardGameObject, _cardPlaceHolders[i], i);
                return true;
            }
            return false;
        }
        
        public virtual bool RemoveCard(BaseCardGameObject cardGameObject,BaseCardHolder cardHolder)
        {
            if (cardHolder.CardGameObject != cardGameObject) return false;

            cardHolder.DetachCardGameObject();

            var index = _cardPlaceHolders.IndexOf(cardHolder);
            ShiftLeft(index);
            CardHoldingCount--;

            OnSuccessfullyRemoveCard(cardGameObject, cardHolder, index);
            return true;
        }
        
        
        #endregion

        #region MOUSE_INPUT
        
        public virtual bool TryAddCard(BaseCardGameObject cardGameObject, BaseCardHolder cardHolder = null)
        {
            if (!Interactable) return false;
            return AddCard(cardGameObject, cardHolder);
        }
        
        public virtual bool TakeOutTemporary(BaseCardGameObject cardGameObject,BaseCardHolder cardHolder)
        {
            if (!Interactable) return false;

            if (!RemoveCard(cardGameObject, cardHolder)) return false;
            
            TemporaryBaseCardHolder = cardHolder;
            return true;
        }
        
        public virtual void ReAddTemporary(BaseCardGameObject baseCardGameObject)
        {
            AddCard(baseCardGameObject, TemporaryBaseCardHolder);
            
            TemporaryBaseCardHolder = null;
        }

        public virtual void RemoveTemporary(BaseCardGameObject baseCardGameObject)
        {
            TemporaryBaseCardHolder = null;
        }
        
        
        #endregion

        protected virtual void SmoothMove(Transform movingObject, Vector3 toPosition)
        {
            movingObject.position = toPosition;
        }

        protected virtual void OnSuccessfullyAddCard(BaseCardGameObject baseCardGameObject, BaseCardHolder baseCardHolder, int index)
        {
            
        }
        protected virtual void OnSuccessfullyRemoveCard(BaseCardGameObject baseCardGameObject, BaseCardHolder baseCardHolder, int index)
        {
            
        }

        public void Select()
        {
            throw new NotImplementedException();
        }

        public void Deselect()
        {
            throw new NotImplementedException();
        }

        public void StartHover()
        {
            IsHovering = true;
            
        }

        public void EndHover()
        {
            IsHovering = false;
            
        }

        public virtual void DisableInteractable()
        {
            
            if (!Interactable) return;
            Interactable = false;
            if(IsHovering) EndHover();
        }
        
        public virtual void EnableInteractable()
        {
            if (Interactable) return;
            Interactable = true;
        }
    }
}