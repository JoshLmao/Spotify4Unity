using Spotify4Unity.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;

public class VolumeSliderToController : UIMouseEvents
{
    [SerializeField]
    private ExamplePlayerController m_controller = null;

    public override void OnPointerDown(PointerEventData eventData)
    {
        m_controller.OnMouseDownVolumeSlider();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        m_controller.OnMouseUpVolumeSlider();
    }

    public override void OnScrollUp(float yDelta)
    {
        m_controller.OnScrollUpVolume(yDelta);
    }

    public override void OnScrollDown(float yDelta)
    {
        m_controller.OnScrollDownVolume(yDelta);
    }
}
