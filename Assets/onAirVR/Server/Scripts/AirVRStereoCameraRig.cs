/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using UnityEngine.Audio;

public sealed class AirVRStereoCameraRig : AirVRCameraRig, IAirVRTrackingModelContext {
    private readonly string TrackingSpaceName = "TrackingSpace";
    private readonly string LeftEyeAnchorName = "LeftEyeAnchor";
    private readonly string RightEyeAnchorName = "RightEyeAnchor";
    private readonly string CenterEyeAnchorName = "CenterEyeAnchor";
    private readonly string LeftHandAnchorName = "LeftHandAnchor";
    private readonly string RightHandAnchorName = "RightHandAnchor";
    private readonly int CameraLeftIndex = 0;
    private readonly int CameraRightIndex = 1;

    public enum HandSelect {
        None,
        Left,
        Right,
        Both
    }

    public enum TrackingModel {
        Head,
        InterpupillaryDistanceOnly,
        ExternalTracker,
        NoPositionTracking
    }

    private Matrix4x4 _worldToHMDSpaceMatrix;

    private Camera[] _cameras;

    private AirVRTrackingModel _trackingModelObject;

    [SerializeField] private HandSelect _eventSystemResponsive = HandSelect.None;
    [SerializeField] private bool _renderControllersOnClient = false;
    [SerializeField] private bool _raycastGraphic = true;
    [SerializeField] private bool _raycastPhysics = true;
    [SerializeField] private LayerMask _physicsRaycastEventMask = -1;

    [SerializeField] private bool _sendAudio = true;
    [SerializeField] private AirVRServerAudioOutputRouter.Input _audioInput = AirVRServerAudioOutputRouter.Input.AudioListener;
    [SerializeField] private AudioMixer _targetAudioMixer = null;
    [SerializeField] private string _exposedRendererIDParameterName = null;

    [SerializeField] private TrackingModel _trackingModel = TrackingModel.Head;
    [SerializeField] private Transform _externalTrackingOrigin = null;
    [SerializeField] private Transform _externalTracker = null;

    internal Transform trackingSpace { get; private set; }
    internal bool renderControllersOnClient { get { return _renderControllersOnClient; } }

    public Camera leftEyeCamera {
        get {
            return _cameras[CameraLeftIndex];
        }
    }

    public Camera rightEyeCamera {
        get {
            return _cameras[CameraRightIndex];
        }
    }

    public Transform leftEyeAnchor { get; private set; }
    public Transform centerEyeAnchor { get; private set; }
    public Transform rightEyeAnchor { get; private set; }
    public Transform leftHandAnchor { get; private set; }
    public Transform rightHandAnchor { get; private set; }

    private TrackingModel trackingModelOf(AirVRTrackingModel trackingModelObject) {
        return trackingModelObject.GetType() == typeof(AirVRHeadTrackingModel)             ? TrackingModel.Head :
               trackingModelObject.GetType() == typeof(AirVRIPDOnlyTrackingModel)          ? TrackingModel.InterpupillaryDistanceOnly :
               trackingModelObject.GetType() == typeof(AirVRExternalTrackerTrackingModel)  ? TrackingModel.ExternalTracker :
               trackingModelObject.GetType() == typeof(AirVRNoPotisionTrackingModel)       ? TrackingModel.NoPositionTracking : TrackingModel.Head;
    }

    private AirVRTrackingModel createTrackingModelObject(TrackingModel model) {
        return model == TrackingModel.InterpupillaryDistanceOnly ?  new AirVRIPDOnlyTrackingModel(this, leftEyeAnchor, centerEyeAnchor, rightEyeAnchor) :
               model == TrackingModel.ExternalTracker            ?  new AirVRExternalTrackerTrackingModel(this, leftEyeAnchor, centerEyeAnchor, rightEyeAnchor, _externalTrackingOrigin, _externalTracker) :
               model == TrackingModel.NoPositionTracking         ?  new AirVRNoPotisionTrackingModel(this, leftEyeAnchor, centerEyeAnchor, rightEyeAnchor) :
                                                                    new AirVRHeadTrackingModel(this, leftEyeAnchor, centerEyeAnchor, rightEyeAnchor) as AirVRTrackingModel;
    }

    private void updateTrackingModel() {
        if (_trackingModelObject == null || trackingModelOf(_trackingModelObject) != _trackingModel) {
            _trackingModelObject = createTrackingModelObject(_trackingModel);
        }
        if (trackingModelOf(_trackingModelObject) == TrackingModel.ExternalTracker) {
            AirVRExternalTrackerTrackingModel model = _trackingModelObject as AirVRExternalTrackerTrackingModel;
            model.trackingOrigin = _externalTrackingOrigin;
            model.tracker = _externalTracker;
        }
    }

    // implements AirVRCameraRig
    private bool ensureCameraObjectIntegrity(Transform xform) {
        if (xform.gameObject.GetComponent<Camera>() == null) {
            xform.gameObject.AddComponent<Camera>();
            return false;
        }
        return true;
    }

    protected override void ensureGameObjectIntegrity() {
        if (trackingSpace == null) {
            trackingSpace = getOrCreateGameObject(TrackingSpaceName, transform);
        }
        if (leftEyeAnchor == null) {
            leftEyeAnchor = getOrCreateGameObject(LeftEyeAnchorName, trackingSpace);
        }
        if (centerEyeAnchor == null) {
            centerEyeAnchor = getOrCreateGameObject(CenterEyeAnchorName, trackingSpace);
        }
        if (rightEyeAnchor == null) {
            rightEyeAnchor = getOrCreateGameObject(RightEyeAnchorName, trackingSpace);
        }
        if (leftHandAnchor == null) {
            leftHandAnchor = getOrCreateGameObject(LeftHandAnchorName, trackingSpace);
        }
        if (rightHandAnchor == null) {
            rightHandAnchor = getOrCreateGameObject(RightHandAnchorName, trackingSpace);
        }

        bool updateCamera = false;
        if (_cameras == null) {
            _cameras = new Camera[2];
            updateCamera = true;
        }

        if (ensureCameraObjectIntegrity(leftEyeAnchor) == false || updateCamera) {
            _cameras[CameraLeftIndex] = leftEyeAnchor.GetComponent<Camera>();
        }
        if (ensureCameraObjectIntegrity(rightEyeAnchor) == false || updateCamera) {
            _cameras[CameraRightIndex] = rightEyeAnchor.GetComponent<Camera>();
        }
    }

    protected override void onAwake() {
        switch (_eventSystemResponsive) {
            case HandSelect.Left:
                prepareForEventSystem(leftHandAnchor.gameObject.AddComponent<AirVRPointer>(), AirVRInputDeviceID.LeftHandTracker);
                break;
            case HandSelect.Right:
                prepareForEventSystem(rightHandAnchor.gameObject.AddComponent<AirVRPointer>(), AirVRInputDeviceID.RightHandTracker);
                break;
            case HandSelect.Both:
                prepareForEventSystem(leftHandAnchor.gameObject.AddComponent<AirVRPointer>(), AirVRInputDeviceID.LeftHandTracker);
                prepareForEventSystem(rightHandAnchor.gameObject.AddComponent<AirVRPointer>(), AirVRInputDeviceID.RightHandTracker);
                break;
        }

        if (_sendAudio) {
            var router = centerEyeAnchor.gameObject.AddComponent<AirVRServerAudioOutputRouter>();
            router.input = _audioInput;
            router.targetAudioMixer = _targetAudioMixer;
            router.exposedRendererIDParameterName = _exposedRendererIDParameterName;
            router.targetCameraRig = this;

            if (_audioInput == AirVRServerAudioOutputRouter.Input.AudioListener) {
                router.output = AirVRServerAudioOutputRouter.Output.All;

                centerEyeAnchor.gameObject.AddComponent<AudioListener>();
            }
            else {
                router.output = AirVRServerAudioOutputRouter.Output.One;
            }
        }
    }

    private void prepareForEventSystem(AirVRPointer pointer, AirVRInputDeviceID srcDevice) {
        pointer.Configure(this, srcDevice);

        if (_raycastPhysics) {
            var raycaster = pointer.gameObject.AddComponent<AirVRPhysicsRaycaster>();
            raycaster.eventMask = _physicsRaycastEventMask;
        }
    }

    protected override void onStart() {
        if (_trackingModelObject == null) {
            _trackingModelObject = createTrackingModelObject(_trackingModel);
        }
    }

    protected override void setupCamerasOnBound(AirVRClientConfig config) {
#if UNITY_2018_2_OR_NEWER
        var props = config.physicalCameraProps;

        leftEyeCamera.usePhysicalProperties = true;
        leftEyeCamera.focalLength = props.focalLength;
        leftEyeCamera.sensorSize = props.sensorSize;
        leftEyeCamera.lensShift = props.leftLensShift;
        leftEyeCamera.aspect = props.aspect;
        leftEyeCamera.gateFit = Camera.GateFitMode.None;

        rightEyeCamera.usePhysicalProperties = true;
        rightEyeCamera.focalLength = props.focalLength;
        rightEyeCamera.sensorSize = props.sensorSize;
        rightEyeCamera.lensShift = props.rightLensShift;
        rightEyeCamera.aspect = props.aspect;
        rightEyeCamera.gateFit = Camera.GateFitMode.None;
#else
        leftEyeCamera.projectionMatrix = config.GetLeftEyeCameraProjection(leftEyeCamera.nearClipPlane, leftEyeCamera.farClipPlane);
        rightEyeCamera.projectionMatrix = config.GetRightEyeCameraProjection(rightEyeCamera.nearClipPlane, rightEyeCamera.farClipPlane);
#endif
    }

    protected override void onStartRender() {
        updateTrackingModel();
        _trackingModelObject.StartTracking();
    }

    protected override void onStopRender() {
        updateTrackingModel();
        _trackingModelObject.StopTracking();
    }

    protected override void updateCameraProjection(AirVRClientConfig config, float[] projection) {
        // do nothing; a stereoscopic camera must keep its appropriate projection
    }

    protected override void updateCameraTransforms(AirVRClientConfig config, Vector3 centerEyePosition, Quaternion centerEyeOrientation) {
        updateTrackingModel();
        _trackingModelObject.UpdateEyePose(config, centerEyePosition, centerEyeOrientation);
    }

    protected override void updateControllerTransforms(AirVRClientConfig config) {
        var pose = inputStream.GetPose((byte)AirVRInputDeviceID.LeftHandTracker, (byte)AirVRHandTrackerControl.Pose);
        leftHandAnchor.localPosition = pose.position;
        leftHandAnchor.localRotation = pose.rotation;

        pose = inputStream.GetPose((byte)AirVRInputDeviceID.RightHandTracker, (byte)AirVRHandTrackerControl.Pose);
        rightHandAnchor.localPosition = pose.position;
        rightHandAnchor.localRotation = pose.rotation;
    }

    internal override bool raycastGraphic => _raycastGraphic;
    internal override Matrix4x4 clientSpaceToWorldMatrix => _trackingModelObject.HMDSpaceToWorldMatrix;
    internal override Transform headPose => centerEyeAnchor;
    internal override Camera[] cameras => _cameras;

    // implements IAirVRTrackingModelContext
    void IAirVRTrackingModelContext.RecenterCameraRigPose() {
        RecenterPose();
    }
}
