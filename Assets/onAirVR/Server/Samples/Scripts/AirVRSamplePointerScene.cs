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
    private const string BasicSampleSceneName = "A. Basic";
    private const float IndicatorDuration = 0.25f;

    private AirVRStereoCameraRig _stereoCameraRig;
    private AirVRMonoCameraRig _monoCameraRig;
    private bool _loadingBasicScene;
    private Button _button;
    private Image _indicator;
    private float _remainingToStopIndicating = -1.0f;

    [SerializeField] private Transform _pointerForMonoClient;

    public void GoToBasicScene() {
        if (_loadingBasicScene == false) {
            _loadingBasicScene = true;

            StartCoroutine(loadScene(BasicSampleSceneName));
        }
    }

    private void Awake() {
        _stereoCameraRig = FindObjectOfType<AirVRStereoCameraRig>();
        _monoCameraRig = FindObjectOfType<AirVRMonoCameraRig>();

        _button = transform.Find("Canvas/Panel/Button").GetComponent<Button>();
        _indicator = transform.Find("Canvas/Panel/Indicator").GetComponent<Image>();

        _indicator.gameObject.SetActive(false);
    }

    private IEnumerator Start() {
        _button.onClick.AddListener(() => {
            AirVRInput.RequestVibration(_stereoCameraRig, AirVRInput.Device.RightHandTracker, AirVRHapticVibration.OneTime_Short);
            _remainingToStopIndicating = IndicatorDuration;
            _indicator.gameObject.SetActive(true);
        });

        yield return StartCoroutine(AirVRCameraFade.FadeAllCameras(this, true, 0.5f));
    }

    private void Update() {
        updateMonoClientPointer();
        updateIndicator();
    }

    private IEnumerator loadScene(string sceneName) {
        yield return StartCoroutine(AirVRCameraFade.FadeAllCameras(this, false, 0.5f));
        SceneManager.LoadScene(sceneName);
    }

    private void updateMonoClientPointer() {
        var touchCount = AirVRInput.GetScreenTouchCount(_monoCameraRig);
        if (touchCount <= 0) {
            _pointerForMonoClient.gameObject.SetActive(false);
            return;
        }

        var touch = AirVRInput.GetScreenTouch(_monoCameraRig, 0);
        var ray = _monoCameraRig.ScreenTouchToRay(touch);

        RaycastHit raycastHit;
        if (Physics.Raycast(ray, out raycastHit, _monoCameraRig.camera.farClipPlane, _monoCameraRig.camera.cullingMask)) {
            _pointerForMonoClient.gameObject.SetActive(true);
            _pointerForMonoClient.position = raycastHit.point;
        }
        else {
            _pointerForMonoClient.gameObject.SetActive(false);
        }
    }

    private void updateIndicator() {
        if (_remainingToStopIndicating <= 0.0f) { return; }

        _remainingToStopIndicating -= Time.deltaTime;
        if (_remainingToStopIndicating <= 0.0f) {
            _indicator.gameObject.SetActive(false);
        }
    }
}
