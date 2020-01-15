using Spotify4Unity;
using Spotify4Unity.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;

public class SliderInteractionController : UIMouseEvents
{
    [SerializeField]
    private GameObject m_handle = null;

    [SerializeField]
    private UnityEngine.UI.Image m_sliderForeground = null;

    [SerializeField]
    private Material m_sliderIdleMaterial = null, m_sliderActiveMaterial = null;

    private void Start()
    {
        if (m_handle != null)
            m_handle.SetActive(false);
        else
            Analysis.Log($"Handle property hasn't been set on '{name}'", Analysis.LogLevel.All);

        m_sliderForeground.material = m_sliderIdleMaterial;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        m_sliderForeground.material = m_sliderActiveMaterial;
        m_handle.SetActive(true);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        m_sliderForeground.material = m_sliderIdleMaterial;
        m_handle.SetActive(false);
    }
}
