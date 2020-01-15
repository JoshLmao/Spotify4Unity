using UnityEngine;
using UnityEngine.EventSystems;

namespace Spotify4Unity.Helpers
{
    /// <summary>
    /// Used to detect mouse events and pass them up to any inherited class
    /// </summary>
    public class UIMouseEvents : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        IScrollHandler
    {
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
        }

        public void OnScroll(PointerEventData eventData)
        {
            float yScrollDelta = eventData.scrollDelta.y;
            if (yScrollDelta > 0)
            {
                OnScrollUp(yScrollDelta);
            }
            else if (yScrollDelta < 0)
            {
                OnScrollDown(yScrollDelta);
            }
        }

        public virtual void OnScrollUp(float yDelta)
        {

        }

        public virtual void OnScrollDown(float yDelta)
        {

        }
    }
}