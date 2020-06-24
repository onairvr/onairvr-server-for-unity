/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;

public class AirVRSamplePlayer : MonoBehaviour {
    private const float ThrowSpeed = 12.0f;
    private const float ThrowTorqueSpeed = 20.0f;

    private AirVRStereoCameraRig _cameraRig;
    private MeshRenderer _head;
    private MeshRenderer _leftController;
    private MeshRenderer _rightController;
    private AudioSource _soundShot;

    public AirVRSampleCan canPrefab;

    private void Awake() {
        _cameraRig = GetComponentInChildren<AirVRStereoCameraRig>();

        var head = _cameraRig.transform.Find("TrackingSpace/CenterEyeAnchor/Head");
        if (head != null) {
            _head = head.GetComponent<MeshRenderer>();
        }
        _leftController = _cameraRig.transform.Find("TrackingSpace/LeftHandAnchor/Controller").GetComponent<MeshRenderer>();
        _rightController = _cameraRig.transform.Find("TrackingSpace/RightHandAnchor/Controller").GetComponent<MeshRenderer>();
        _soundShot = transform.Find("SoundShot").GetComponent<AudioSource>();
    }

    private void Update() {
        if (_head != null) {
            _head.enabled = _cameraRig.isActive;
        }
        _leftController.enabled = _cameraRig.isActive && AirVRInput.IsDeviceAvailable(_cameraRig, AirVRInput.Device.LeftHandTracker);
        _rightController.enabled = _cameraRig.isActive && AirVRInput.IsDeviceAvailable(_cameraRig, AirVRInput.Device.RightHandTracker);

        if (AirVRInput.GetDown(_cameraRig, AirVRInput.Button.X) ||
            AirVRInput.GetDown(_cameraRig, AirVRInput.Button.LIndexTrigger)) {
            throwCan(_cameraRig.leftHandAnchor);
        }
        if (AirVRInput.GetDown(_cameraRig, AirVRInput.Button.A) ||
            AirVRInput.GetDown(_cameraRig, AirVRInput.Button.RIndexTrigger)) {
            throwCan(_cameraRig.rightHandAnchor);
        }
    }

    private void throwCan(Transform hand) {
        AirVRSampleCan can = Instantiate(canPrefab, hand.position, hand.rotation) as AirVRSampleCan;
        can.Throw(hand.forward * ThrowSpeed, Vector3.right * ThrowTorqueSpeed);

        _soundShot.Play();
    }
}
