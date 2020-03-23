/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;

public abstract class AirVRInputDeviceFeedback : AirVRInputSender {
    // raycast hit
    private Vector3 _rayOrigin;
    private Vector3 _hitPosition;
    private Vector3 _hitNormal;

    protected bool supportsRaycastHit { get; private set; }
    protected virtual byte raycastHitResultKey { get { return 0; } }

    public byte[] cookieTexture { get; private set; }
    public float cookieDepthScaleMultiplier { get; private set; }

    public void ConfigureRaycastHit(Texture2D cookieTexture, float cookieDepthScaleMultiplier) {
#if UNITY_2017_1_OR_NEWER
        this.cookieTexture = ImageConversion.EncodeToPNG(cookieTexture);
#else
        this.cookieTexture = cookieTexture.EncodeToPNG();
#endif
        this.cookieDepthScaleMultiplier = cookieDepthScaleMultiplier;
    }

    public void EnableRaycastHit(bool enable) {
        if (cookieTexture == null || cookieDepthScaleMultiplier <= 0.0f) { return; }
        if (supportsRaycastHit == enable) { return; }

        supportsRaycastHit = enable;

        if (enable == false) {
            _rayOrigin = Vector3.zero;
            _hitPosition = Vector3.zero;
            _hitNormal = Vector3.zero;
        }
    }

    public void PendRaycastHitResult(Vector3 rayOrigin, Vector3 hitPosition, Vector3 hitNormal) {
        if (supportsRaycastHit == false) { return; }

        _rayOrigin = rayOrigin;
        _hitPosition = hitPosition;
        _hitNormal = hitNormal;
    }

    private void pendRaycastHitPerFrame(AirVRInputStream inputStream) {
        inputStream.PendRaycastHit(this, raycastHitResultKey, _rayOrigin, _hitPosition, _hitNormal);
    }

    // vibration
    private const float MinVibrateRequestInterval = 0.12f;

    private AirVRHapticVibration _lastVibration = AirVRHapticVibration.None;
    private float _remainingToResetVibration = -1.0f;

    protected abstract bool supportsVibrate { get; }
    protected virtual byte vibrateKey { get { return 0; } }

    public void PendVibrate(AirVRHapticVibration vibration) {
        if (supportsVibrate == false) { return; }

        _lastVibration = vibration;
        if (_lastVibration != AirVRHapticVibration.None) {
            _remainingToResetVibration = MinVibrateRequestInterval;
        }
    }

    private void pendVibrationPerFrame(AirVRInputStream inputStream) {
        if (_remainingToResetVibration > 0.0f) {
            _remainingToResetVibration -= Time.deltaTime;

            if (_remainingToResetVibration <= 0.0f) {
                _lastVibration = AirVRHapticVibration.None;
            }
        }

        inputStream.PendState(this, vibrateKey, (byte)_lastVibration);
    }

    // implements AirVRInputSender
    public override void PendInputsPerFrame(AirVRInputStream inputStream) {
        if (supportsVibrate) {
            pendVibrationPerFrame(inputStream);
        }
        if (supportsRaycastHit) {
            pendRaycastHitPerFrame(inputStream);
        }
    }
}

internal class AirVRHeadTrackerDeviceFeedback : AirVRInputDeviceFeedback {
    // implements AirVRInputDeviceFeedback
    public override string name {
        get { return AirVRInputDeviceName.HeadTracker; }
    }

    protected override byte raycastHitResultKey {
        get { return (byte)AirVRHeadTrackerKey.RaycastHitResult; }
    }

    protected override bool supportsVibrate {
        get { return false; }
    }
}

internal abstract class AirVRTrackerDeviceFeedback : AirVRInputDeviceFeedback {
    public bool renderOnClient { get; set; }

    public AirVRTrackerDeviceFeedback(Texture2D cookieTexture, float cookieDepthScaleMultiplier) {
        ConfigureRaycastHit(cookieTexture, cookieDepthScaleMultiplier);
    }

    protected abstract byte renderOnClientKey { get; }

    // implements AirVRInputDeviceFeedback
    protected override bool supportsVibrate {
        get { return true; }
    }

    public override void PendInputsPerFrame(AirVRInputStream inputStream) {
        base.PendInputsPerFrame(inputStream);

        inputStream.PendState(this, renderOnClientKey, (byte)(renderOnClient ? 1 : 0));
    }
}

internal class AirVRLeftHandTrackerDeviceFeedback : AirVRTrackerDeviceFeedback {
    public AirVRLeftHandTrackerDeviceFeedback(Texture2D cookieTexture, float cookieDepthScaleMultiplier) 
        : base(cookieTexture, cookieDepthScaleMultiplier) {}

    public override string name {
        get { return AirVRInputDeviceName.LeftHandTracker; }
    }

    protected override byte raycastHitResultKey {
        get { return (byte)AirVRLeftHandTrackerKey.RaycastHitResult; }
    }

    protected override byte vibrateKey {
        get { return (byte)AirVRLeftHandTrackerKey.Vibrate; }
    }

    protected override byte renderOnClientKey {
        get { return (byte)AirVRLeftHandTrackerKey.RenderOnClient; }
    }
}

internal class AirVRRightHandTrackerDeviceFeedback : AirVRTrackerDeviceFeedback {
    public AirVRRightHandTrackerDeviceFeedback(Texture2D cookieTexture, float cookieDepthScaleMultiplier)
        : base(cookieTexture, cookieDepthScaleMultiplier) { }

    public override string name {
        get { return AirVRInputDeviceName.RightHandTracker; }
    }

    protected override byte raycastHitResultKey {
        get { return (byte)AirVRRightHandTrackerKey.RaycastHitResult; }
    }

    protected override byte vibrateKey {
        get { return (byte)AirVRRightHandTrackerKey.Vibrate; }
    }

    protected override byte renderOnClientKey {
        get { return (byte)AirVRRightHandTrackerKey.RenderOnClient; }
    }
}
