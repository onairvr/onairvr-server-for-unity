﻿/***********************************************************

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
    private Feedback _feedback = null;

    private Vector3 _lastRaycastHitOrigin = Vector3.zero;
    private Vector3 _lastRaycastHitPosition = Vector3.zero;
    private Vector3 _lastRaycastHitNormal = Vector3.zero;
    
    public void Configure(AirVRStereoCameraRig cameraRig, AirVRInputDeviceID srcDevice) {
        _cameraRig = cameraRig;
        _feedback = new Feedback(this, srcDevice);
    }

    private void Start() {
        pointers.Add(this);

        _cameraRig.inputStream.RegisterInputSender(_feedback);
    }

    private void OnDestroy() {
        _cameraRig.inputStream.UnregisterInputSender(_feedback);

        pointers.Remove(this);
    }

    public AirVRCameraRig cameraRig {
        get {
            return _cameraRig;
        }
    }

    public bool interactable {
        get {
            if (_cameraRig == null) { return false; }
            if (_cameraRig.renderControllersOnClient == false) { return true; }

            return _cameraRig.inputStream.GetByteAxis(_feedback.id, (byte)AirVRHandTrackerControl.Status) != 0;
        }
    }

    public bool primaryButtonPressed {
        get {
            if (_cameraRig == null) { return false; }

            switch ((AirVRInputDeviceID)_feedback.id) {
                case AirVRInputDeviceID.LeftHandTracker:
                    return _cameraRig.inputStream.GetActivated((byte)AirVRInputDeviceID.Controller, (byte)AirVRControllerControl.AxisLIndexTrigger) ||
                           _cameraRig.inputStream.GetActivated((byte)AirVRInputDeviceID.Controller, (byte)AirVRControllerControl.ButtonX);
                case AirVRInputDeviceID.RightHandTracker:
                    return _cameraRig.inputStream.GetActivated((byte)AirVRInputDeviceID.Controller, (byte)AirVRControllerControl.AxisRIndexTrigger) ||
                           _cameraRig.inputStream.GetActivated((byte)AirVRInputDeviceID.Controller, (byte)AirVRControllerControl.ButtonA);
                default:
                    return false;
            }
        }
    }

    public bool primaryButtonReleased {
        get {
            if (_cameraRig == null) { return false; }

            switch ((AirVRInputDeviceID)_feedback.id) {
                case AirVRInputDeviceID.LeftHandTracker:
                    return _cameraRig.inputStream.GetDeactivated((byte)AirVRInputDeviceID.Controller, (byte)AirVRControllerControl.AxisLIndexTrigger) ||
                           _cameraRig.inputStream.GetDeactivated((byte)AirVRInputDeviceID.Controller, (byte)AirVRControllerControl.ButtonX);
                case AirVRInputDeviceID.RightHandTracker:
                    return _cameraRig.inputStream.GetDeactivated((byte)AirVRInputDeviceID.Controller, (byte)AirVRControllerControl.AxisRIndexTrigger) ||
                           _cameraRig.inputStream.GetDeactivated((byte)AirVRInputDeviceID.Controller, (byte)AirVRControllerControl.ButtonA);
                default:
                    return false;
            }
        }
    }

    public Ray GetWorldRay() {
        if (_cameraRig) {
            switch ((AirVRInputDeviceID)_feedback.id) {
                case AirVRInputDeviceID.LeftHandTracker:
                    return new Ray(_cameraRig.leftHandAnchor.position, _cameraRig.leftHandAnchor.forward);
                case AirVRInputDeviceID.RightHandTracker:
                    return new Ray(_cameraRig.rightHandAnchor.position, _cameraRig.rightHandAnchor.forward);
                default:
                    break;
            }
        }
        return new Ray();
    }

    public void UpdateRaycastResult(Ray ray, RaycastResult raycastResult) {
        if (_cameraRig == null || _cameraRig.renderControllersOnClient == false) { return; }

        if (raycastResult.isValid) {
            _lastRaycastHitOrigin = _cameraRig.clientSpaceToWorldMatrix.inverse.MultiplyPoint(ray.origin);
            _lastRaycastHitPosition = _cameraRig.clientSpaceToWorldMatrix.inverse.MultiplyPoint(raycastResult.worldPosition);
            _lastRaycastHitNormal = _cameraRig.clientSpaceToWorldMatrix.inverse.MultiplyVector(raycastResult.worldNormal);
        }
        else {
            _lastRaycastHitOrigin = _lastRaycastHitPosition = _lastRaycastHitNormal = Vector3.zero;
        }
    }

    private class Feedback : AirVRInputSender {
        private AirVRPointer _owner;
        private AirVRInputDeviceID _device;

        public Feedback(AirVRPointer owner, AirVRInputDeviceID device) {
            _owner = owner;
            _device = device;
        }

        // implements AirVRInputSender
        public override byte id => (byte)_device;

        public override void PendInputsPerFrame(AirVRInputStream inputStream) {
            if (_owner._cameraRig == null) { return; }

            inputStream.PendState(this, (byte)AirVRHandTrackerFeedbackControl.RenderOnClient, _owner._cameraRig.renderControllersOnClient ? (byte)1 : (byte)0);
            inputStream.PendRaycastHit(this, (byte)AirVRHandTrackerFeedbackControl.RaycastHit, _owner._lastRaycastHitOrigin, _owner._lastRaycastHitPosition, _owner._lastRaycastHitNormal);
        }
    }
}
