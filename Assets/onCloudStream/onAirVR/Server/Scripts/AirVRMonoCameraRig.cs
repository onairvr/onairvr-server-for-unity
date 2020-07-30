﻿/***********************************************************

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

    public Ray ScreenTouchToRay(AirVRInput.Touch touch) {
        var viewportPos = new Vector3((touch.position.x + 1.0f) / 2.0f, 
                                      (touch.position.y + 1.0f) / 2.0f, 
                                      0.0f);
        return camera.ViewportPointToRay(viewportPos);
    }

    // implements AirVRCameraRig
    protected override void ensureGameObjectIntegrity(bool create) {
        if (_thisTransform == null) {
            _thisTransform = transform;
        }

        bool updateCamera = false;
        if (_cameras == null) {
            _cameras = new Camera[1];
            updateCamera = true;
        }

        if (_cameraAnchor == null) {
            _cameraAnchor = getOrCreateGameObject(CameraAnchorName, transform, create);
        }

        if (_cameraAnchor == null && create == false) { return; }

        if (_cameraAnchor.GetComponent<Camera>() == null) {
            _cameraAnchor.gameObject.AddComponent<Camera>();
            updateCamera = true;
        }

        if (updateCamera) {
            _cameras[0] = _cameraAnchor.GetComponent<Camera>();
        }
    }

    protected override void setupCamerasOnBound(AirVRClientConfig config) {
        var projection = config.GetCameraProjectionMatrix(camera.nearClipPlane, camera.farClipPlane);
        if (projection == Matrix4x4.zero) { return; }

#if UNITY_2018_2_OR_NEWER
        var props = config.physicalCameraProps;        

        camera.usePhysicalProperties = true;
        camera.focalLength = props.focalLength;
        camera.sensorSize = props.sensorSize;
        camera.lensShift = props.lensShift;
        camera.aspect = props.aspect;
        camera.gateFit = Camera.GateFitMode.None;
#else
        camera.projectionMatrix = projection;
#endif
    }

    protected override void updateCameraProjection(AirVRClientConfig config, float[] projection) {
        var projectionMatrix = AirVRClientConfig.CalcCameraProjectionMatrix(projection, camera.nearClipPlane, camera.farClipPlane);

#if UNITY_2018_2_OR_NEWER
        var props = AirVRClientConfig.CalcPhysicalCameraProps(projection);

        camera.usePhysicalProperties = true;
        camera.focalLength = props.focalLength;
        camera.sensorSize = props.sensorSize;
        camera.lensShift = props.lensShift;
        camera.aspect = props.aspect;
        camera.gateFit = Camera.GateFitMode.None;
#else
        camera.projectionMatrix = projectionMatrix;
#endif
    }

    protected override void updateCameraTransforms(AirVRClientConfig config, Vector3 centerEyePosition, Quaternion centerEyeOrientation) {
        _cameraAnchor.localRotation = centerEyeOrientation;
        _cameraAnchor.localPosition = centerEyePosition;
    }

    internal override bool raycastGraphic => false;
    internal override Matrix4x4 clientSpaceToWorldMatrix => _thisTransform.localToWorldMatrix;
    internal override Transform headPose => _cameras != null ? _cameras[0].transform : null;
    internal override Camera[] cameras => _cameras;
}
