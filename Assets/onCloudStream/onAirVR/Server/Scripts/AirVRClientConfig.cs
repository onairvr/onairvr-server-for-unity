/*****************;******************************************

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
    public struct PhysicalCameraProps {
        public Vector2 sensorSize;
        public float focalLength;
        public Vector2 lensShift;

        public Vector2 leftLensShift {
            get {
                return lensShift;
            }
        }

        public Vector2 rightLensShift {
            get {
                var result = lensShift;
                result.x = -result.x;

                return result;
            }
        }

        public float aspect {
            get {
                return sensorSize.x / sensorSize.y;
            }
        }
    }

    internal static Matrix4x4 CalcCameraProjectionMatrix(float[] projection, float near, float far) {
        float left = projection[0] * near;
        float top = projection[1] * near;
        float right = projection[2] * near;
        float bottom = projection[3] * near;

        Matrix4x4 result = Matrix4x4.zero;
        result[0, 0] = 2 * near / (right - left);
        result[1, 1] = 2 * near / (top - bottom);
        result[0, 2] = (right + left) / (right - left);
        result[1, 2] = (top + bottom) / (top - bottom);
        result[2, 2] = (near + far) / (near - far);
        result[2, 3] = 2 * near * far / (near - far);
        result[3, 2] = -1.0f;

        return result;
    }

    internal static PhysicalCameraProps CalcPhysicalCameraProps(float[] projection) {
        var result = new PhysicalCameraProps();
        result.sensorSize = new Vector2(projection[2] - projection[0],
                                        projection[1] - projection[3]);
        result.focalLength = 1;
        result.lensShift = new Vector2((projection[2] + projection[0]) / 2 / (projection[2] - projection[0]),
                                       (projection[1] + projection[3]) / 2 / (projection[1] - projection[3]));
        return result;
    }

    public static AirVRClientConfig Get(int playerID) {
        string json = "";
        if (OCSServerPlugin.GetConfig(playerID, ref json)) {
            return JsonUtility.FromJson<AirVRClientConfig>(json);
        }
        return null;
    }

    public static void Set(int playerID, AirVRClientConfig config) {
        OCSServerPlugin.SetConfig(playerID, JsonUtility.ToJson(config));
    }

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

    internal Matrix4x4 GetCameraProjectionMatrix(float near, float far) {
        if (isCameraProjectionValid(CameraProjection) == false) { return Matrix4x4.zero; }

        return CalcCameraProjectionMatrix(CameraProjection, near, far);
    }

    internal Matrix4x4 GetLeftEyeCameraProjection(float near, float far) {
        return GetCameraProjectionMatrix(near, far);
    }

    internal Matrix4x4 GetRightEyeCameraProjection(float near, float far) {
        Matrix4x4 result = GetCameraProjectionMatrix(near, far);
        if (result != Matrix4x4.zero) {
            result[0, 2] *= -1.0f;
        }
        return result;
    }

    public AirVRClientType type => Stereoscopy ? AirVRClientType.Stereoscopic : AirVRClientType.Monoscopic;
    public int videoWidth => VideoWidth;
    public int videoHeight => VideoHeight;
    public float framerate => FrameRate;

    public float fov {
        get {
            float tAngle = Mathf.Atan(Mathf.Abs(CameraProjection[1]));
            float bAngle = Mathf.Atan(Mathf.Abs(CameraProjection[3]));
            return Mathf.Rad2Deg * (tAngle * Mathf.Sign(CameraProjection[1]) - bAngle * Mathf.Sign(CameraProjection[3]));
        }
    }

    public Vector3 eyeCenterPosition => EyeCenterPosition;
    public float ipd => InterpupillaryDistance;
    public string userID => UserID;
    public PhysicalCameraProps physicalCameraProps => CalcPhysicalCameraProps(CameraProjection);

    private bool isCameraProjectionValid(float[] projection) {
        return projection[2] - projection[0] > 0 && projection[1] - projection[3] > 0;
    }
}
