
using TBEasyWebCam;
using UnityEngine;
using UnityEngine.UI;

public class QRCodeScanner : MonoBehaviour
{
    public RawImage raw_Scanner;
    public Toggle tg_torch;
    public Button btn_back;
    public DeviceCameraController deviceCameraController;
    public QRCodeDecodeController decodeController;

    public void Init()
    {
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 0);
        raw_Scanner.texture = rt;
        deviceCameraController.GetComponent<Camera>().targetTexture = rt;
        decodeController.onQRScanFinished += DecodeController_onQRScanFinished;
        btn_back.onClick.AddListener(StopWork);
    }

    private void DecodeController_onQRScanFinished(string str)
    {
        StopWork();
    }

    public void StartWork()
    {
        decodeController.Reset();
        gameObject.SetActive(true);
        deviceCameraController.StartWork();
    }

    public void StopWork()
    {
        deviceCameraController.StopWork();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 开启手电筒
    /// </summary>
    public bool Torch
    {
        set
        {
#if UNITY_ANDROID && !UNITY_EDITOR
		if (EasyWebCam.isActive) {
			if (!value) {
				EasyWebCam.setTorchMode (TBEasyWebCam.Setting.TorchMode.Off);
			} else {
				EasyWebCam.setTorchMode (TBEasyWebCam.Setting.TorchMode.On);
			}
		}
#endif

        }
    }

}
