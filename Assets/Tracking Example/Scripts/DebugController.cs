using Assets.UnityPharusAPI.Managers;
using TMPro;
using UnityEngine;
using UnityPharusAPI;
using UnityPharusAPI.Enums;

namespace Assets.Tracking_Example.Scripts
{
    public class DebugController : MonoBehaviour
    {
        public GameObject canvasControl;
        public TextMeshProUGUI trackingType;
        public TextMeshProUGUI protocolStatus;
        public TextMeshProUGUI interpolationStatus;
        public TextMeshProUGUI stageStatus;

        private void Update()
        {
            HandleKeyboardInputs();
        }

        private void HandleKeyboardInputs()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (canvasControl != null)
                {
                    canvasControl.SetActive(true);
                    UpdateDebugGUI();
                }
            }
            else if (Input.GetKeyUp(KeyCode.Tab))
            {
                if (canvasControl != null)
                {
                    canvasControl.SetActive(false);
                    UpdateDebugGUI();
                }
            }
        }

        private void UpdateDebugGUI()
        {
            if (ATrackingManager.Instance.Settings != null)
            {
                TrackingSettings settings = ATrackingManager.Instance.Settings;

                if (trackingType != null)
                {
                    trackingType.text = $"Tracking Port: {settings.TracklinkProtocol}";
                }

                if (interpolationStatus != null)
                {
                    interpolationStatus.text =
                        $"Interpolation: {settings.TrackingResolutionX}px x {settings.TrackingResolutionY}px";
                }

                if (stageStatus != null)
                {
                    stageStatus.text = $"Stage Dimensions: {settings.StageSizeX}cm x {settings.StageSizeY}cm";
                }

                if (protocolStatus != null)
                {
                    string ipAddress = settings.TracklinkProtocol == EProtocolType.UDP ? settings.TracklinkMulticastIp : settings.TracklinkTcpIp;
                    string port = settings.TracklinkProtocol == EProtocolType.UDP ? settings.TracklinkUdpPort.ToString() : settings.TracklinkTcpPort.ToString();
                    protocolStatus.text = $"TracklinkProtocol: {settings.TracklinkProtocol} {ipAddress} : {port}";
                }
            }
        }
    }
}