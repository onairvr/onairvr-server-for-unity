/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using System;

public enum AirVRClientType {
    Monoscopic,
    Stereoscopic
}

[Serializable]
public class AirVRClientConfig {
    public AirVRClientConfig() {
        CameraProjection = new float[4];
    }

    [SerializeField] protected string UserID;
    [SerializeField] protected bool Stereoscopy;
    [SerializeField] protected int VideoWidth;
    [SerializeField] protected int VideoHeight;
    [SerializeField] protected float[] CameraProjection;
    [SerializeField] protected float FrameRate;
    [SerializeField] protected float InterpupillaryDistance;
    [SerializeField] protected Vector3 EyeCenterPosition;

    internal Matrix4x4 GetCameraProjection(float near, float far) {
        if (isCameraProjectionValid(CameraProjection) == false) { return Matrix4x4.zero; }

        return makeProjection(CameraProjection[0] * near,
                              CameraProjection[1] * near,
                              CameraProjection[2] * near,
                              CameraProjection[3] * near,
                              near,
                              far);
    }

    internal Matrix4x4 GetLeftEyeCameraProjection(float near, float far) {
        return GetCameraProjection(near, far);
    }

    internal Matrix4x4 GetRightEyeCameraProjection(float near, float far) {
        Matrix4x4 result = GetCameraProjection(near, far);
        if (result != Matrix4x4.zero) {
            result[0, 2] *= -1.0f;
        }
        return result;
    }

    public AirVRClientType type {
        get {
            return Stereoscopy ? AirVRClientType.Stereoscopic : AirVRClientType.Monoscopic;
        }
    }

    public int videoWidth {
        get {
            return VideoWidth;
        }
    }

    public int videoHeight {
        get {
            return VideoHeight;
        }
    }

    public float framerate {
        get {
            return Mathf.Min(FrameRate, AirVRServer.serverParams.MaxFrameRate);
        }
    }

    public float fov {
        get {
            float tAngle = Mathf.Atan(Mathf.Abs(CameraProjection[1]));
            float bAngle = Mathf.Atan(Mathf.Abs(CameraProjection[3]));
            return Mathf.Rad2Deg * (tAngle * Mathf.Sign(CameraProjection[1]) - bAngle * Mathf.Sign(CameraProjection[3]));
        }
    }

    public Vector3 eyeCenterPosition {
        get {
            return EyeCenterPosition;
        }
    }

    public float ipd {
        get {
            return InterpupillaryDistance;
        }
    }

    public string userID {
        get {
            return UserID;
        }
    }

    public Vector2 cameraSensorSize {
        get {
            return new Vector2(CameraProjection[2] - CameraProjection[0],
                               CameraProjection[1] - CameraProjection[3]);
        }
    }

    public float cameraFocalLength {
        get {
            return 1;
        }
    }

    public Vector2 cameraLensShift {
        get {
            return new Vector2((CameraProjection[2] + CameraProjection[0]) / 2 / (CameraProjection[2] - CameraProjection[0]),
                               (CameraProjection[1] + CameraProjection[3]) / 2 / (CameraProjection[1] - CameraProjection[3]));
        }
    }

    public Vector2 cameraLeftLensShift {
        get {
            return cameraLensShift;
        }
    }

    public Vector2 cameraRightLensShift {
        get {
            var result = cameraLensShift;
            result.x = -result.x;

            return result;
        }
    }

    public float cameraAspect {
        get {
            return cameraSensorSize.x / cameraSensorSize.y;
        }
    }

    private Matrix4x4 makeProjection(float l, float t, float r, float b, float n, float f) {
        Matrix4x4 result = Matrix4x4.zero;
        result[0, 0] = 2 * n / (r - l);
        result[1, 1] = 2 * n / (t - b);
        result[0, 2] = (r + l) / (r - l);
        result[1, 2] = (t + b) / (t - b);
        result[2, 2] = (n + f) / (n - f);
        result[2, 3] = 2 * n * f / (n - f);
        result[3, 2] = -1.0f;

        return result;
    }

    private bool isCameraProjectionValid(float[] projection) {
        return projection[2] - projection[0] > 0 && projection[1] - projection[3] > 0;
    }
}
