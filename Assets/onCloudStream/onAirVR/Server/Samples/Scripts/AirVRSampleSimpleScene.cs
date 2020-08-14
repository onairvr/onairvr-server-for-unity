/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AirVRSampleSimpleScene : MonoBehaviour, AirVRCameraRigManager.EventHandler {
    private AirVRCameraRig _lastCameraRig;

    public AudioSource music;

    void Awake() {
        AirVRCameraRigManager.managerOnCurrentScene.Delegate = this;
    }

    IEnumerator Start() {
        yield return StartCoroutine(AirVRCameraFade.FadeAllCameras(this, true, 0.5f));
    }

    void Update() {
        if (_lastCameraRig != null && AirVRInput.GetScreenTouchCount(_lastCameraRig) > 0) {
            Debug.Log("touch count: " + AirVRInput.GetScreenTouchCount(_lastCameraRig));
        }
    }

    // implements AirVRCameraRigManager.EventHandler
    public void AirVRCameraRigWillBeBound(int clientHandle, AirVRClientConfig config, List<AirVRCameraRig> availables, out AirVRCameraRig selected) {
        selected = availables.Count > 0 ? availables[0] : null;

        if (selected) {
            music.Play();
        }

        _lastCameraRig = selected;
    }

    public void AirVRCameraRigActivated(AirVRCameraRig cameraRig) {
        // The sample onairvr client app just sends back what it receives.
        // (https://github.com/onairvr/onairvr-client-for-oculus-mobile)

        string pingMessage = "ping from " + System.Environment.MachineName;
        cameraRig.SendUserData(System.Text.Encoding.UTF8.GetBytes(pingMessage));
    }

    public void AirVRCameraRigDeactivated(AirVRCameraRig cameraRig) {}

    public void AirVRCameraRigHasBeenUnbound(AirVRCameraRig cameraRig) {
        // NOTE : This event occurs in OnDestroy() of AirVRCameraRig during unloading scene.
        //        You should be careful because some objects in the scene might be destroyed already on this event.
        if (music != null) {
            music.Stop();
        }
    }

    public void AirVRCameraRigUserDataReceived(AirVRCameraRig cameraRig, byte[] userData) {
        Debug.Log("User data received: " + System.Text.Encoding.UTF8.GetString(userData));
    }
}
