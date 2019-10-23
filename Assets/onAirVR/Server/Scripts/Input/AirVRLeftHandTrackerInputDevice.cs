﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirVRLeftHandTrackerInputDevice : AirVRInputDevice {
    // implements AirVRInputDevice
    protected override string deviceName => AirVRInputDeviceName.LeftHandTracker;

    protected override void MakeControlList() {
        AddControlTransform((byte)AirVRLeftHandTrackerKey.Transform);
    }    
}
