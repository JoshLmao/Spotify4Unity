using Spotify4Unity.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;

public class TrackSliderToController : SliderInteractionController
{
    [SerializeField]
    private ExamplePlayerController m_controller = null;

    public override void OnPointerDown(PointerEventData eventData)
    {
        m_controller.OnMouseDownTrackTimeSlider();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        m_controller.OnMouseUpTrackTimeSlider();
    }
}
