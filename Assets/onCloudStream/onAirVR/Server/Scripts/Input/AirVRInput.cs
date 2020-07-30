/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using UnityEngine.Assertions;

public static class AirVRInput {
    public enum Device : byte {
        HeadTracker = OCSInputDeviceID.HeadTracker,
        LeftHandTracker = OCSInputDeviceID.LeftHandTracker,
        RightHandTracker = OCSInputDeviceID.RightHandTracker,
        Controller = OCSInputDeviceID.Controller
    }

    public enum Axis2D {
        LThumbstick,
        RThumbstick
    }

    public enum Axis {
        LIndexTrigger,
        RIndexTrigger,
        LHandTrigger,
        RHandTrigger
    }

    public enum Button {
        LIndexTrigger,
        RIndexTrigger,
        LHandTrigger,
        RHandTrigger,
        A,
        B,
        X,
        Y,
        Start,
        Back,
        LThumbstick,
        RThumbstick,
        LThumbstickUp,
        LThumbstickDown,
        LThumbstickLeft,
        LThumbstickRight,
        RThumbstickUp,
        RThumbstickDown,
        RThumbstickLeft,
        RThumbstickRight
    }

    public static bool IsDeviceAvailable(AirVRCameraRig cameraRig, Device device) {
        switch (device) {
            case Device.HeadTracker:
                return true;
            case Device.LeftHandTracker:
            case Device.RightHandTracker:
                return cameraRig.inputStream.GetState((byte)device, (byte)OCSHandTrackerControl.Status) != 0;
            default:
                return false;
        }
    }

    public static Vector2 Get(AirVRCameraRig cameraRig, Axis2D axis) {
        switch (axis) {
            case Axis2D.LThumbstick:
                return cameraRig.inputStream.GetAxis2D((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DLThumbstick);
            case Axis2D.RThumbstick:
                return cameraRig.inputStream.GetAxis2D((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DRThumbstick);
            default:
                return Vector2.zero;
        }
    }

    public static float Get(AirVRCameraRig cameraRig, Axis axis) {
        switch (axis) {
            case Axis.LIndexTrigger:
                return cameraRig.inputStream.GetAxis((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.AxisLIndexTrigger);
            case Axis.RIndexTrigger:
                return cameraRig.inputStream.GetAxis((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.AxisRIndexTrigger);
            case Axis.LHandTrigger:
                return cameraRig.inputStream.GetAxis((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.AxisLHandTrigger);
            case Axis.RHandTrigger:
                return cameraRig.inputStream.GetAxis((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.AxisRHandTrigger);
            default:
                return 0;
        }
    }

    public static bool Get(AirVRCameraRig cameraRig, Button button) {
        switch (button) {
            case Button.LIndexTrigger:
                return cameraRig.inputStream.IsActive((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.AxisLIndexTrigger);
            case Button.RIndexTrigger:
                return cameraRig.inputStream.IsActive((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.AxisRIndexTrigger);
            case Button.LHandTrigger:
                return cameraRig.inputStream.IsActive((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.AxisLHandTrigger);
            case Button.RHandTrigger:
                return cameraRig.inputStream.IsActive((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.AxisRHandTrigger);
            case Button.A:
                return cameraRig.inputStream.IsActive((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.ButtonA);
            case Button.B:
                return cameraRig.inputStream.IsActive((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.ButtonB);
            case Button.X:
                return cameraRig.inputStream.IsActive((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.ButtonX);
            case Button.Y:
                return cameraRig.inputStream.IsActive((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.ButtonY);
            case Button.Start:
                return cameraRig.inputStream.IsActive((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.ButtonStart);
            case Button.Back:
                return cameraRig.inputStream.IsActive((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.ButtonBack);
            case Button.LThumbstick:
                return cameraRig.inputStream.IsActive((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.ButtonLThumbstick);
            case Button.RThumbstick:
                return cameraRig.inputStream.IsActive((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.ButtonRThumbstick);
            case Button.LThumbstickUp:
                return cameraRig.inputStream.IsActive((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DLThumbstick, OCSInputDirection.Up);
            case Button.LThumbstickDown:
                return cameraRig.inputStream.IsActive((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DLThumbstick, OCSInputDirection.Down);
            case Button.LThumbstickLeft:
                return cameraRig.inputStream.IsActive((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DLThumbstick, OCSInputDirection.Left);
            case Button.LThumbstickRight:
                return cameraRig.inputStream.IsActive((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DLThumbstick, OCSInputDirection.Right);
            case Button.RThumbstickUp:
                return cameraRig.inputStream.IsActive((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DRThumbstick, OCSInputDirection.Up);
            case Button.RThumbstickDown:
                return cameraRig.inputStream.IsActive((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DRThumbstick, OCSInputDirection.Down);
            case Button.RThumbstickLeft:
                return cameraRig.inputStream.IsActive((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DRThumbstick, OCSInputDirection.Left);
            case Button.RThumbstickRight:
                return cameraRig.inputStream.IsActive((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DRThumbstick, OCSInputDirection.Right);
            default:
                return false;
        }
    }

    public static bool GetDown(AirVRCameraRig cameraRig, Button button) {
        switch (button) {
            case Button.LIndexTrigger:
                return cameraRig.inputStream.GetActivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.AxisLIndexTrigger);
            case Button.RIndexTrigger:
                return cameraRig.inputStream.GetActivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.AxisRIndexTrigger);
            case Button.LHandTrigger:
                return cameraRig.inputStream.GetActivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.AxisLHandTrigger);
            case Button.RHandTrigger:
                return cameraRig.inputStream.GetActivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.AxisRHandTrigger);
            case Button.A:
                return cameraRig.inputStream.GetActivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.ButtonA);
            case Button.B:
                return cameraRig.inputStream.GetActivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.ButtonB);
            case Button.X:
                return cameraRig.inputStream.GetActivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.ButtonX);
            case Button.Y:
                return cameraRig.inputStream.GetActivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.ButtonY);
            case Button.Start:
                return cameraRig.inputStream.GetActivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.ButtonStart);
            case Button.Back:
                return cameraRig.inputStream.GetActivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.ButtonBack);
            case Button.LThumbstick:
                return cameraRig.inputStream.GetActivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.ButtonLThumbstick);
            case Button.RThumbstick:
                return cameraRig.inputStream.GetActivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.ButtonRThumbstick);
            case Button.LThumbstickUp:
                return cameraRig.inputStream.GetActivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DLThumbstick, OCSInputDirection.Up);
            case Button.LThumbstickDown:
                return cameraRig.inputStream.GetActivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DLThumbstick, OCSInputDirection.Down);
            case Button.LThumbstickLeft:
                return cameraRig.inputStream.GetActivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DLThumbstick, OCSInputDirection.Left);
            case Button.LThumbstickRight:
                return cameraRig.inputStream.GetActivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DLThumbstick, OCSInputDirection.Right);
            case Button.RThumbstickUp:
                return cameraRig.inputStream.GetActivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DRThumbstick, OCSInputDirection.Up);
            case Button.RThumbstickDown:
                return cameraRig.inputStream.GetActivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DRThumbstick, OCSInputDirection.Down);
            case Button.RThumbstickLeft:
                return cameraRig.inputStream.GetActivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DRThumbstick, OCSInputDirection.Left);
            case Button.RThumbstickRight:
                return cameraRig.inputStream.GetActivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DRThumbstick, OCSInputDirection.Right);
            default:
                return false;
        }
    }

    public static bool GetUp(AirVRCameraRig cameraRig, Button button) {
        switch (button) {
            case Button.LIndexTrigger:
                return cameraRig.inputStream.GetDeactivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.AxisLIndexTrigger);
            case Button.RIndexTrigger:
                return cameraRig.inputStream.GetDeactivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.AxisRIndexTrigger);
            case Button.LHandTrigger:
                return cameraRig.inputStream.GetDeactivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.AxisLHandTrigger);
            case Button.RHandTrigger:
                return cameraRig.inputStream.GetDeactivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.AxisRHandTrigger);
            case Button.A:
                return cameraRig.inputStream.GetDeactivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.ButtonA);
            case Button.B:
                return cameraRig.inputStream.GetDeactivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.ButtonB);
            case Button.X:
                return cameraRig.inputStream.GetDeactivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.ButtonX);
            case Button.Y:
                return cameraRig.inputStream.GetDeactivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.ButtonY);
            case Button.Start:
                return cameraRig.inputStream.GetDeactivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.ButtonStart);
            case Button.Back:
                return cameraRig.inputStream.GetDeactivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.ButtonBack);
            case Button.LThumbstick:
                return cameraRig.inputStream.GetDeactivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.ButtonLThumbstick);
            case Button.RThumbstick:
                return cameraRig.inputStream.GetDeactivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.ButtonRThumbstick);
            case Button.LThumbstickUp:
                return cameraRig.inputStream.GetDeactivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DLThumbstick, OCSInputDirection.Up);
            case Button.LThumbstickDown:
                return cameraRig.inputStream.GetDeactivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DLThumbstick, OCSInputDirection.Down);
            case Button.LThumbstickLeft:
                return cameraRig.inputStream.GetDeactivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DLThumbstick, OCSInputDirection.Left);
            case Button.LThumbstickRight:
                return cameraRig.inputStream.GetDeactivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DLThumbstick, OCSInputDirection.Right);
            case Button.RThumbstickUp:
                return cameraRig.inputStream.GetDeactivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DRThumbstick, OCSInputDirection.Up);
            case Button.RThumbstickDown:
                return cameraRig.inputStream.GetDeactivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DRThumbstick, OCSInputDirection.Down);
            case Button.RThumbstickLeft:
                return cameraRig.inputStream.GetDeactivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DRThumbstick, OCSInputDirection.Left);
            case Button.RThumbstickRight:
                return cameraRig.inputStream.GetDeactivated((byte)OCSInputDeviceID.Controller, (byte)OCSControllerControl.Axis2DRThumbstick, OCSInputDirection.Right);
            default:
                return false;
        }
    }

    public static void SetVibration(AirVRCameraRig cameraRig, Device device, float frequency, float amplitude) {
        switch (device) {
            case Device.LeftHandTracker:
                cameraRig.inputStream.PendVibration((byte)OCSInputDeviceID.LeftHandTracker, (byte)OCSHandTrackerFeedbackControl.Vibration, frequency, amplitude);
                break;
            case Device.RightHandTracker:
                cameraRig.inputStream.PendVibration((byte)OCSInputDeviceID.RightHandTracker, (byte)OCSHandTrackerFeedbackControl.Vibration, frequency, amplitude);
                break;
        }
    }

    public static void SetVibration(AirVRCameraRig cameraRig, Device device, AnimationCurve frequency, AnimationCurve amplitude) {
        switch (device) {
            case Device.LeftHandTracker:
                renderVibration(cameraRig, (byte)OCSInputDeviceID.LeftHandTracker, (byte)OCSHandTrackerFeedbackControl.Vibration, frequency, amplitude);
                break;
            case Device.RightHandTracker:
                renderVibration(cameraRig, (byte)OCSInputDeviceID.RightHandTracker, (byte)OCSHandTrackerFeedbackControl.Vibration, frequency, amplitude);
                break;
        }
    }

    public static int GetScreenTouchCount(AirVRCameraRig cameraRig) {
        var count = 0;
        for (byte control = (byte)OCSTouchScreenControl.TouchIndexStart; control <= (byte)OCSTouchScreenControl.TouchIndexEnd; control++) {
            if (cameraRig.inputStream.IsActive((byte)OCSInputDeviceID.TouchScreen, control) ||
                cameraRig.inputStream.GetDeactivated((byte)OCSInputDeviceID.TouchScreen, control)) {
                count++;
            }
        }
        return count;
    }

    public static Touch GetScreenTouch(AirVRCameraRig cameraRig, int index) {
        var i = 0;
        for (byte control = (byte)OCSTouchScreenControl.TouchIndexStart; control <= (byte)OCSTouchScreenControl.TouchIndexEnd; control++) {
            if (cameraRig.inputStream.IsActive((byte)OCSInputDeviceID.TouchScreen, control) == false &&
                cameraRig.inputStream.GetDeactivated((byte)OCSInputDeviceID.TouchScreen, control) == false) {
                continue;
            }

            if (i == index) {
                Touch touch = new Touch();
                touch.fingerID = control;

                byte state = 0;
                cameraRig.inputStream.GetTouch2D((byte)OCSInputDeviceID.TouchScreen, control, ref touch.position, ref state);

                if (cameraRig.inputStream.GetActivated((byte)OCSInputDeviceID.TouchScreen, control)) {
                    touch.phase = TouchPhase.Began;
                }
                else {
                    switch ((OCSTouchPhase)state) {
                        case OCSTouchPhase.Ended:
                            touch.phase = TouchPhase.Ended;
                            break;
                        case OCSTouchPhase.Canceled:
                            touch.phase = TouchPhase.Canceled;
                            break;
                        case OCSTouchPhase.Stationary:
                            touch.phase = TouchPhase.Stationary;
                            break;
                        case OCSTouchPhase.Moved:
                            touch.phase = TouchPhase.Moved;
                            break;
                        default:
                            Assert.IsTrue(false);
                            break;
                    }
                }

                return touch;
            }
            else {
                i++;
            }
        }
        return new Touch { fingerID = Touch.InvalidFingerID };
    }

    private static void renderVibration(AirVRCameraRig cameraRig, byte device, byte control, AnimationCurve frequency, AnimationCurve amplitude) {
        var fps = cameraRig.GetConfig().framerate;
        var duration = Mathf.Max(frequency.keys[frequency.keys.Length - 1].time, amplitude.keys[amplitude.keys.Length - 1].time);

        for (float t = 0.0f; t < duration; t += 1.0f / fps) {
            cameraRig.inputStream.PendVibration(device, control, frequency.Evaluate(t), amplitude.Evaluate(t));
        }

        // make sure to end with no vibration
        cameraRig.inputStream.PendVibration(device, control, 0, 0);
    }

    public struct Touch {
        public const int InvalidFingerID = -1;

        public int fingerID;
        public Vector2 position;
        public TouchPhase phase;

        public bool IsValid() {
            return fingerID != InvalidFingerID;
        }
    }
}
