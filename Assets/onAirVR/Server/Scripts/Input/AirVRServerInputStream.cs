/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System.Runtime.InteropServices;
using UnityEngine;

public class AirVRServerInputStream : AirVRInputStream {
    [DllImport(AirVRServerPlugin.Name)]
    private static extern void ocs_BeginPendInput(int playerID, ref long timestamp);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void ocs_PendInputState(int playerID, byte device, byte control, byte state);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void ocs_PendInputRaycastHit(int playerID, byte device, byte control, AirVRVector3D origin, AirVRVector3D hitPosition, AirVRVector3D hitNormal);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void ocs_PendInputVibration(int playerID, byte device, byte control, float frequency, float amplitude);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void ocs_SendPendingInputs(int playerID, long timestamp);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern long ocs_GetInputRecvTimestamp(int playerID);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool ocs_GetInputState(int playerID, byte device, byte control, ref byte state);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool ocs_GetInputByteAxis(int playerID, byte device, byte control, ref byte axis);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool ocs_GetInputAxis(int playerID, byte device, byte control, ref float axis);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool ocs_GetInputAxis2D(int playerID, byte device, byte control, ref AirVRVector2D axis2D);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool ocs_GetInputPose(int playerID, byte device, byte control, ref AirVRVector3D position, ref AirVRVector4D rotation);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool ocs_GetInputTouch2D(int playerID, byte device, byte control, ref AirVRVector2D position, ref byte state);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool ocs_IsInputActive(int playerID, byte device, byte control);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool ocs_IsInputDirectionActive(int playerID, byte device, byte control, byte direction);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool ocs_GetInputActivated(int playerID, byte device, byte control);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool ocs_GetInputDirectionActivated(int playerID, byte device, byte control, byte direction);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool ocs_GetInputDeactivated(int playerID, byte device, byte control);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool ocs_GetInputDirectionDeactivated(int playerID, byte device, byte control, byte direction);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void ocs_UpdateInputFrame(int playerID);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void ocs_ClearInput(int playerID);

    public AirVRServerInputStream(AirVRCameraRig owner) {
        this.owner = owner;
    }

    public AirVRCameraRig owner { get; set; }

    public long inputRecvTimestamp {
        get {
            return ocs_GetInputRecvTimestamp(owner.playerID);
        }
    }

    // implements AirVRInputStream
    protected override float maxSendingRatePerSec { get { return 90.0f; } }

    protected override void BeginPendInputImpl(ref long timestamp) {
        ocs_BeginPendInput(owner.playerID, ref timestamp);
    }

    protected override void PendStateImpl(byte device, byte control, byte state) {
        if (owner.isBoundToClient == false) { return; }

        ocs_PendInputState(owner.playerID, device, control, state);
    }

    protected override void PendByteAxisImpl(byte device, byte control, byte axis) { }
    protected override void PendAxisImpl(byte device, byte control, float axis) { }
    protected override void PendAxis2DImpl(byte device, byte control, Vector2 axis2D) { }
    protected override void PendPoseImpl(byte device, byte control, Vector3 position, Quaternion rotation) { }

    protected override void PendRaycastHitImpl(byte device, byte control, Vector3 origin, Vector3 hitPosition, Vector3 hitNormal) {
        if (owner.isBoundToClient == false) { return; }

        ocs_PendInputRaycastHit(owner.playerID, device, control, new AirVRVector3D(origin), new AirVRVector3D(hitPosition), new AirVRVector3D(hitNormal));
    }

    protected override void PendVibrationImpl(byte device, byte control, float frequency, float amplitude) {
        if (owner.isBoundToClient == false) { return; }

        ocs_PendInputVibration(owner.playerID, device, control, frequency, amplitude);
    }

    protected override void PendTouch2DImpl(byte device, byte control, Vector2 position, byte state, bool active) {}

    protected override void SendPendingInputEventsImpl(long timestamp) {
        ocs_SendPendingInputs(owner.playerID, timestamp);
    }

    protected override bool GetStateImpl(byte device, byte control, ref byte state) {
        if (owner.isBoundToClient == false) { return false; }

        return ocs_GetInputState(owner.playerID, device, control, ref state);
    }

    protected override bool GetByteAxisImpl(byte device, byte control, ref byte axis) {
        if (owner.isBoundToClient == false) { return false; }

        return ocs_GetInputByteAxis(owner.playerID, device, control, ref axis);   
    }

    protected override bool GetAxisImpl(byte device, byte control, ref float axis) {
        if (owner.isBoundToClient == false) { return false; }

        return ocs_GetInputAxis(owner.playerID, device, control, ref axis);
    }

    protected override bool GetAxis2DImpl(byte device, byte control, ref Vector2 axis2D) {
        if (owner.isBoundToClient == false) { return false; }

        AirVRVector2D axis = new AirVRVector2D();

        if (ocs_GetInputAxis2D(owner.playerID, device, control, ref axis) == false) { return false; }

        axis2D = axis.toVector2();
        return true;
    }

    protected override bool GetPoseImpl(byte device, byte control, ref Vector3 position, ref Quaternion rotation) {
        if (owner.isBoundToClient == false) { return false; }

        AirVRVector3D pos = new AirVRVector3D();
        AirVRVector4D rot = new AirVRVector4D();

        if (ocs_GetInputPose(owner.playerID, device, control, ref pos, ref rot) == false) { return false; }

        position = pos.toVector3();
        rotation = rot.toQuaternion();
        return true;
    }

    protected override bool GetRaycastHitImpl(byte device, byte control, ref Vector3 origin, ref Vector3 hitPosition, ref Vector3 hitNormal) { return false; }
    protected override bool GetVibrationImpl(byte device, byte control, ref float frequency, ref float amplitude) { return false; }

    protected override bool GetTouch2DImpl(byte device, byte control, ref Vector2 position, ref byte state) {
        if (owner.isBoundToClient == false) { return false; }

        AirVRVector2D pos = new AirVRVector2D();

        if (ocs_GetInputTouch2D(owner.playerID, device, control, ref pos, ref state) == false) { return false; }

        position = pos.toVector2();
        return true;
    }

    protected override bool IsActiveImpl(byte device, byte control) {
        if (owner.isBoundToClient == false) { return false; }

        return ocs_IsInputActive(owner.playerID, device, control);
    }

    protected override bool IsActiveImpl(byte device, byte control, AirVRInputDirection direction) {
        if (owner.isBoundToClient == false) { return false; }

        return ocs_IsInputDirectionActive(owner.playerID, device, control, (byte)direction);
    }

    protected override bool GetActivatedImpl(byte device, byte control) {
        if (owner.isBoundToClient == false) { return false; }

        return ocs_GetInputActivated(owner.playerID, device, control);
    }

    protected override bool GetActivatedImpl(byte device, byte control, AirVRInputDirection direction) {
        if (owner.isBoundToClient == false) { return false; }

        return ocs_GetInputDirectionActivated(owner.playerID, device, control, (byte)direction);
    }

    protected override bool GetDeactivatedImpl(byte device, byte control) {
        if (owner.isBoundToClient == false) { return false; }

        return ocs_GetInputDeactivated(owner.playerID, device, control);
    }

    protected override bool GetDeactivatedImpl(byte device, byte control, AirVRInputDirection direction) {
        if (owner.isBoundToClient == false) { return false; }

        return ocs_GetInputDirectionDeactivated(owner.playerID, device, control, (byte)direction);
    }

    protected override void UpdateInputFrameImpl() {
        if (owner.isBoundToClient == false) { return; }

        ocs_UpdateInputFrame(owner.playerID);
    }

    protected override void ClearInputImpl() {
        if (owner.isBoundToClient == false) { return; }

        ocs_ClearInput(owner.playerID);
    }
}
