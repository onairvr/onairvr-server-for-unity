using System.Runtime.InteropServices;
using UnityEngine;

public static class OCSPlugin {
    public const string Name = "ocs";
    public const string AudioPluginName = "AudioPlugin_ocs";
}

[StructLayout(LayoutKind.Sequential)]
public struct OCSVector2D {
    public float x;
    public float y;

    public OCSVector2D(Vector2 value) {
        x = value.x;
        y = value.y;
    }

    public Vector3 toVector2() {
        return new Vector2(x, y);
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct OCSVector3D {
    public float x;
    public float y;
    public float z;

    public OCSVector3D(Vector3 value) {
        x = value.x;
        y = value.y;
        z = value.z;
    }

    public Vector3 toVector3() {
        return new Vector3(x, y, z);
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct OCSVector4D {
    public float x;
    public float y;
    public float z;
    public float w;

    public OCSVector4D(Quaternion value) {
        x = value.x;
        y = value.y;
        z = value.z;
        w = value.w;
    }

    public Quaternion toQuaternion() {
        return new Quaternion(x, y, z, w);
    }
}
