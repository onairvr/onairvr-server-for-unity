using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirVRScreenTouchInputDevice : AirVRInputDevice {
    protected override string deviceName {
        get {
            return AirVRInputDeviceName.ScreenTouch;
        }
    }

    protected override void MakeControlList() {
        // supports up to 5 touches
        AddControlTouch(0);
        AddControlTouch(1);
        AddControlTouch(2);
        AddControlTouch(3);
        AddControlTouch(4);
    }
}
