/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System;

public class AirVRServerEventDispatcher : OCSEventDispatcher {
    protected override OCSMessage ParseMessageImpl(IntPtr source, string message) {
        return AirVRServerMessage.Parse(source, message);
    }

    protected override bool CheckMessageQueueImpl(out IntPtr source, out IntPtr data, out int length) {
        return OCSServerPlugin.CheckMessageQueue(out source, out data, out length);
    }

    protected override void RemoveFirstMessageFromQueueImpl() {
        OCSServerPlugin.RemoveFirstMessage();
    }
}
