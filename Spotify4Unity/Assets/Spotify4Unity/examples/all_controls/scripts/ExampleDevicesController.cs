using Spotify4Unity;
using Spotify4Unity.Dtos;
using Spotify4Unity.Events;
using Spotify4Unity.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using S4UEnums = Spotify4Unity.Enums;

/// <summary>
/// Spotify4Unity
/// Example Controller script for displaying all Spotify devices
/// (Make sure you read the Wiki documentation for all information & help!)
/// </summary>
public class ExampleDevicesController : LayoutGroupBase<Device>
{
    [SerializeField, Tooltip("The UI object to show when no devices have been found in the list")]
    private GameObject m_noDevicesUI = null;

    [SerializeField, Tooltip("The color of the currently active device")]
    private Color m_activeDeviceColor = Color.green;

    [SerializeField, Tooltip("The color of inactive devices")]
    private Color m_inactiveDeviceColor = Color.white;

    private List<Device> m_devices = null;

    protected override void Start()
    {
        base.Start();

        m_noDevicesUI.SetActive(true);
    }

    protected override void OnConnectedChanged(ConnectedChanged e)
    {
        base.OnConnectedChanged(e);

        if(m_noDevicesUI != null)
            m_noDevicesUI.SetActive(!e.IsConnected);
        // Remove any UI if not connected
        if (!e.IsConnected)
            UpdateUI(null);
    }

    protected override void OnDevicesChanged(DevicesChanged e)
    {
        if (e.Devices == null)
            return;

        m_devices = e.Devices.OrderBy(x => x.Name).ToList();

        if(m_devices != null && m_devices.Count > 0)
            m_noDevicesUI.SetActive(false);

        // Reorder list to show Active Device at the top of the list
        Device activeDevice = m_devices.FirstOrDefault(x => x.Id == SpotifyService.ActiveDevice.Id);
        m_devices.Remove(activeDevice);
        m_devices.Insert(0, activeDevice);

        UpdateUI(m_devices);
    }

    protected override void SetPrefabInfo(GameObject instantiatedPrefab, Device device)
    {
        // Fill prefab details
        Button btn = instantiatedPrefab.transform.Find("Button").GetComponent<Button>();
        btn.GetComponent<Button>().onClick.AddListener(() => OnChangeDevice(device));

        Text deviceNameText = btn.transform.Find("Text").GetComponent<Text>();
        deviceNameText.text = device.Name;
        deviceNameText.color = device.Id == SpotifyService.ActiveDevice.Id ? m_activeDeviceColor : m_inactiveDeviceColor;

        Image deviceTypeIcon = btn.transform.Find("Icon").GetComponent<Image>();
        deviceTypeIcon.sprite = TypeToIcon(device.Type);
        deviceTypeIcon.color = device.Id == SpotifyService.ActiveDevice.Id ? m_activeDeviceColor : m_inactiveDeviceColor;
    }


    private void OnChangeDevice(Device d)
    {
        if (SpotifyService.ActiveDevice.Id == d.Id)
            return;

        SpotifyService.SetActiveDevice(d);

        UpdateUI(m_devices);
    }

    private Sprite TypeToIcon(S4UEnums.DeviceType type)
    {
        //ToDo: Add your own images to different types of devices
        switch (type)
        {
            case S4UEnums.DeviceType.Computer:
            case S4UEnums.DeviceType.Tablet:
            case S4UEnums.DeviceType.Smartphone:
            case S4UEnums.DeviceType.Speaker:
            case S4UEnums.DeviceType.TV:
            case S4UEnums.DeviceType.AudioVideoReciever:
            case S4UEnums.DeviceType.SetTopBox:
            case S4UEnums.DeviceType.AudioDongle:
            case S4UEnums.DeviceType.GameConsole:
            case S4UEnums.DeviceType.ChromecastVideo:
            case S4UEnums.DeviceType.ChromecastAudio:
            case S4UEnums.DeviceType.Automobile:
                return Resources.Load<Sprite>("sprites/monitor");
            default:
                throw new NotImplementedException($"Missing type '{type}'");
        }
    }
}
