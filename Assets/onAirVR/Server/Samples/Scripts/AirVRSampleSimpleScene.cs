﻿/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class AirVRSampleSimpleScene : MonoBehaviour, AirVRCameraRigManager.EventHandler {
    private const string PointerSampleSceneName = "B. Event System (experimental)";

    private bool _sceneBeingUnloaded;
    private AirVRStereoCameraRig _stereoRig;
    private AirVRMonoCameraRig _monoRig;

    public AudioSource music;

    private IEnumerator loadScene(string sceneName) {
        yield return StartCoroutine(AirVRCameraFade.FadeAllCameras(this, false, 0.5f));
        SceneManager.LoadScene(sceneName);
    }

    void Awake() {
        _stereoRig = FindObjectOfType<AirVRStereoCameraRig>();
        _monoRig = FindObjectOfType<AirVRMonoCameraRig>();

        AirVRCameraRigManager.managerOnCurrentScene.Delegate = this;
    }

    IEnumerator Start() {
        yield return StartCoroutine(AirVRCameraFade.FadeAllCameras(this, true, 0.5f));
    }

    void Update() {
        if (_sceneBeingUnloaded == false && AirVRInput.GetDown(_stereoRig, AirVRInput.Button.Back)) {
            _sceneBeingUnloaded = true;
            StartCoroutine(loadScene(PointerSampleSceneName));
        }

        var touchCount = AirVRInput.GetScreenTouchCount(_monoRig);
        if (touchCount > 0) {
            var trace = " ";
            for (var index = 0; index < touchCount; index++) {
                var touch = AirVRInput.GetScreenTouch(_monoRig, index);
                trace += string.Format("[id: {0}, phase: {1}, x: {2}, y: {3}] ", touch.fingerID, touch.phase, touch.position.x, touch.position.y);
            }

            Debug.Log(trace);
        }
    }

    // implements AirVRCameraRigManager.EventHandler
    public void AirVRCameraRigWillBeBound(int clientHandle, AirVRClientConfig config, List<AirVRCameraRig> availables, out AirVRCameraRig selected) {
        selected = availables.Count > 0 ? availables[0] : null;

        if (selected) {
            AirVRSamplePlayer player = selected.GetComponentInParent<AirVRSamplePlayer>();
            if (player != null) {
                player.EnableInteraction(true);
            }
            music.Play();
        }
    }

    public void AirVRCameraRigActivated(AirVRCameraRig cameraRig) {
        string pingMessage = "ping on camera rig activated, from " + System.Environment.MachineName;
        cameraRig.SendUserData(System.Text.Encoding.UTF8.GetBytes(pingMessage));
    }

    public void AirVRCameraRigDeactivated(AirVRCameraRig cameraRig) {}

    public void AirVRCameraRigHasBeenUnbound(AirVRCameraRig cameraRig) {
        // NOTE : This event occurs in OnDestroy() of AirVRCameraRig during unloading scene.
        //        You should be careful because some objects in the scene might be destroyed already on this event.
        AirVRSamplePlayer player = cameraRig.GetComponentInParent<AirVRSamplePlayer>();
        if (player != null) {
            player.EnableInteraction(false);
        }
        if (music != null) {
            music.Stop();
        }
    }

    public void AirVRCameraRigUserDataReceived(AirVRCameraRig cameraRig, byte[] userData) {
        Debug.Log("User data received: " + System.Text.Encoding.UTF8.GetString(userData));
    }
}
