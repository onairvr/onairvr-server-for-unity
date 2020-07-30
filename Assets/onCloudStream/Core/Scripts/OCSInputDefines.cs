public enum OCSInputDeviceID : byte {
    HeadTracker = 0,
    LeftHandTracker = 1,
    RightHandTracker = 2,
    Controller = 3,
    TouchScreen = 4
}

public enum OCSInputDirection : byte {
    Up = 0,
    Down,
    Left,
    Right
}

public enum OCSDeviceStatus : byte {
    Unavailable = 0,
    Ready
}

public enum OCSHeadTrackerControl : byte {
    Pose = 0
}

public enum OCSHandTrackerControl : byte {
    Status = 0,
    Pose
}

public enum OCSHandTrackerFeedbackControl : byte {
    RenderOnClient = 0,
    RaycastHit,
    Vibration
}

public enum OCSControllerControl : byte {
    Axis2DLThumbstick = 0,
    Axis2DRThumbstick,
    AxisLIndexTrigger,
    AxisRIndexTrigger,
    AxisLHandTrigger,
    AxisRHandTrigger,
    ButtonA,
    ButtonB,
    ButtonX,
    ButtonY,
    ButtonStart,
    ButtonBack,
    ButtonLThumbstick,
    ButtonRThumbstick
}

public enum OCSTouchScreenControl : byte {
    TouchIndexStart = 0,
    TouchIndexEnd = 9
}

public enum OCSTouchPhase : byte {
    Ended = 0,
    Canceled,
    Stationary,
    Moved
}
