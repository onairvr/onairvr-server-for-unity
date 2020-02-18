/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;

public sealed class AirVRMonoCameraRig : AirVRCameraRig {
    private readonly string CameraAnchorName = "CameraAnchor";

    private Transform _thisTransform;
    private Transform _cameraAnchor;
    private Camera[] _cameras;

    public new Camera camera {
        get {
            return _cameras[0];
        }
    }

    // implements AirVRCameraRig
    protected override void ensureGameObjectIntegrity() {
        if (_thisTransform == null) {
            _thisTransform = transform;
        }

        bool updateCamera = false;
        if (_cameras == null) {
            _cameras = new Camera[1];
            updateCamera = true;
        }

        if (_cameraAnchor == null) {
            _cameraAnchor = getOrCreateGameObject(CameraAnchorName, transform);
        }
        if (_cameraAnchor.GetComponent<Camera>() == null) {
            _cameraAnchor.gameObject.AddComponent<Camera>();
            updateCamera = true;
        }

        if (updateCamera) {
            _cameras[0] = _cameraAnchor.GetComponent<Camera>();
        }
    }

    protected override void init() {
        inputStream.AddInputDevice(new AirVRHeadTrackerInputDevice());
        inputStream.AddInputDevice(new AirVRScreenTouchInputDevice());
    }

    protected override void setupCamerasOnBound(AirVRClientConfig config) {
        var projection = config.GetCameraProjection(camera.nearClipPlane, camera.farClipPlane);
        if (projection == Matrix4x4.zero) { return; }

#if UNITY_2018_2_OR_NEWER
        camera.usePhysicalProperties = true;
        camera.focalLength = config.cameraFocalLength;
        camera.sensorSize = config.cameraSensorSize;
        camera.lensShift = config.cameraLeftLensShift;
        camera.aspect = config.cameraAspect;
        camera.gateFit = Camera.GateFitMode.None;
#else
        camera.projectionMatrix = projection;
#endif
    }

    protected override void updateCameraTransforms(AirVRClientConfig config, Vector3 centerEyePosition, Quaternion centerEyeOrientation) {
        _cameraAnchor.localRotation = centerEyeOrientation;
        _cameraAnchor.localPosition = centerEyePosition;
    }

    internal override Matrix4x4 clientSpaceToWorldMatrix {
        get {
            return _thisTransform.localToWorldMatrix;
        }
    }

    internal override Transform headPose {
        get {
            return _cameras != null ? _cameras[0].transform : null;
        }
    }

    internal override Camera[] cameras {
        get {
            return _cameras;
        }
    }
}
