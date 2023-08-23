namespace Shun_Card_System
{
    public interface IMouseInteractable
    {
        public bool Interactable { get; }
        public bool IsHovering { get; }
        public void Select();

        public void Deselect();

        public void StartHover();

        public void EndHover();

        public void DisableInteractable();

        public void EnableInteractable();
    }
}