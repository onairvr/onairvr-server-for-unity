/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AirVRSamplePointerScene : MonoBehaviour {
    private const string BasicSampleSceneName = "A. Basic";

    private AirVRMonoCameraRig _monoCameraRig;
    private bool _loadingBasicScene;

    [SerializeField] private Transform _pointerForMonoClient;

    public void GoToBasicScene() {
        if (_loadingBasicScene == false) {
            _loadingBasicScene = true;

            StartCoroutine(loadScene(BasicSampleSceneName));
        }
    }

    private void Awake() {
        _monoCameraRig = FindObjectOfType<AirVRMonoCameraRig>();
    }

    private IEnumerator Start() {
        yield return StartCoroutine(AirVRCameraFade.FadeAllCameras(this, true, 0.5f));
    }

    private void Update() {
        updateMonoClientPointer();
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
}
