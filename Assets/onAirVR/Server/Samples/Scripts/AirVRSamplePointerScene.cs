/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AirVRSamplePointerScene : MonoBehaviour {
    private const float IndicatorDuration = 0.25f;

    private AirVRStereoCameraRig _stereoCameraRig;
    private bool _loadingBasicScene;
    private Button _button;
    private Image _indicator;
    private float _remainingToStopIndicating = -1.0f;

    private void Awake() {
        _stereoCameraRig = FindObjectOfType<AirVRStereoCameraRig>();

        _button = transform.Find("Canvas/Panel/Button").GetComponent<Button>();
        _indicator = transform.Find("Canvas/Panel/Indicator").GetComponent<Image>();

        _indicator.gameObject.SetActive(false);
    }

    private IEnumerator Start() {
        _button.onClick.AddListener(() => {
            _remainingToStopIndicating = IndicatorDuration;
            _indicator.gameObject.SetActive(true);

            AirVRInput.RequestVibration(_stereoCameraRig, AirVRInput.Device.LeftHandTracker, AirVRHapticVibration.OneTime_Short);
            AirVRInput.RequestVibration(_stereoCameraRig, AirVRInput.Device.RightHandTracker, AirVRHapticVibration.OneTime_Short);
        });

        yield return StartCoroutine(AirVRCameraFade.FadeAllCameras(this, true, 0.5f));
    }

    private void Update() {
        updateIndicator();
    }

    private void updateIndicator() {
        if (_remainingToStopIndicating <= 0.0f) { return; }

        _remainingToStopIndicating -= Time.deltaTime;
        if (_remainingToStopIndicating <= 0.0f) {
            _indicator.gameObject.SetActive(false);
        }
    }
}
