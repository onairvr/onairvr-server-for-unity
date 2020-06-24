/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AirVRPointer : MonoBehaviour {
    public static List<AirVRPointer> pointers = new List<AirVRPointer>();

    private AirVRStereoCameraRig _cameraRig = null;
    private AirVRInput.Device _inputDevice = AirVRInput.Device.Unknown;
    private AirVRTrackerDeviceFeedback _deviceFeedback;

    [SerializeField] private bool _renderOnClient = false;
    [SerializeField] private Texture2D _cookie = null;
    [SerializeField] private float _depthScaleMultiplier = 0.015f;

    private void Awake() {
        pointers.Add(this);

        var grandparent = transform.parent ? transform.parent.parent : null;
        _cameraRig = grandparent ? grandparent.GetComponent<AirVRStereoCameraRig>() : null;

        if (_cameraRig == null) { return; }

        if (gameObject.name == "LeftHandAnchor") {
            _inputDevice = AirVRInput.Device.LeftHandTracker;
        }
        else if (gameObject.name == "RightHandAnchor") {
            _inputDevice = AirVRInput.Device.RightHandTracker;
        }
        else {
            throw new UnityException("[ERROR] AirVRPointer must be attached LeftHandAnchor or RightHandAnchor of AirVRStereoCameraRig");
        }
    }

    private void Start() {
        switch (_inputDevice) {
            case AirVRInput.Device.LeftHandTracker:
                _deviceFeedback = new AirVRLeftHandTrackerDeviceFeedback(_cookie, _depthScaleMultiplier);
                break;
            case AirVRInput.Device.RightHandTracker:
                _deviceFeedback = new AirVRRightHandTrackerDeviceFeedback(_cookie, _depthScaleMultiplier);
                break;
            default:
                return;
        }

        _cameraRig.inputStream.AddInputDeviceFeedback(_deviceFeedback);
    }

    protected virtual void Update() {
        if (AirVRInput.IsDeviceAvailable(_cameraRig, _inputDevice) == false) { return; }

        if (_renderOnClient && _deviceFeedback.renderOnClient == false) {
            _deviceFeedback.renderOnClient = true;
            _deviceFeedback.EnableRaycastHit(true);
        }
        else if (_renderOnClient == false && _deviceFeedback.renderOnClient) {
            _deviceFeedback.renderOnClient = false;
            _deviceFeedback.EnableRaycastHit(false);
        }
    }

    private void OnDisable() {
        if (AirVRInput.IsDeviceAvailable(_cameraRig, _inputDevice) == false) { return; }

        if (_deviceFeedback.renderOnClient) {
            _deviceFeedback.renderOnClient = false;
            _deviceFeedback.EnableRaycastHit(false);
        }
    }

    private void OnDestroy() {
        pointers.Remove(this);
    }

    public AirVRCameraRig cameraRig {
        get {
            return _cameraRig;
        }
    }

    public bool interactable {
        get {
            if (_renderOnClient == false) { return true; }

            return AirVRInput.IsDeviceFeedbackEnabled(_cameraRig, _inputDevice);
        }
    }

    public bool primaryButtonPressed {
        get {
            if (_cameraRig == null) { return false; }

            switch (_inputDevice) {
                case AirVRInput.Device.LeftHandTracker:
                    return AirVRInput.GetDown(_cameraRig, AirVRInput.Button.LIndexTrigger) ||
                           AirVRInput.GetDown(_cameraRig, AirVRInput.Button.X);
                case AirVRInput.Device.RightHandTracker:
                    return AirVRInput.GetDown(_cameraRig, AirVRInput.Button.RIndexTrigger) ||
                           AirVRInput.GetDown(_cameraRig, AirVRInput.Button.A);
                default:
                    return false;
            }
        }
    }

    public bool primaryButtonReleased {
        get {
            if (_cameraRig == null) { return false; }

            switch (_inputDevice) {
                case AirVRInput.Device.LeftHandTracker:
                    return AirVRInput.GetUp(_cameraRig, AirVRInput.Button.LIndexTrigger) ||
                           AirVRInput.GetUp(_cameraRig, AirVRInput.Button.X);
                case AirVRInput.Device.RightHandTracker:
                    return AirVRInput.GetUp(_cameraRig, AirVRInput.Button.RIndexTrigger) ||
                           AirVRInput.GetUp(_cameraRig, AirVRInput.Button.A);
                default:
                    return false;
            }
        }
    }

    public Ray GetWorldRay() {
        switch (_inputDevice) {
            case AirVRInput.Device.LeftHandTracker:
                return new Ray(_cameraRig.leftHandAnchor.position, _cameraRig.leftHandAnchor.forward);
            case AirVRInput.Device.RightHandTracker:
                return new Ray(_cameraRig.rightHandAnchor.position, _cameraRig.rightHandAnchor.forward);
        }
        return new Ray();
    }

    public void UpdateRaycastResult(Ray ray, RaycastResult raycastResult) {
        if (_renderOnClient == false) { return; }

        if (raycastResult.isValid) {
            AirVRInput.UpdateRaycastHitResult(_cameraRig, _inputDevice, ray.origin, raycastResult.worldPosition, raycastResult.worldNormal);
        }
        else {
            AirVRInput.UpdateRaycastHitResult(_cameraRig, _inputDevice, Vector3.zero, Vector3.zero, Vector3.zero);
        }
    }
}
