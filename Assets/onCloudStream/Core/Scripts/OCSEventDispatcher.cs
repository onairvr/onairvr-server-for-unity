/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using System;
using System.Runtime.InteropServices;

public abstract class OCSEventDispatcher {
    public delegate void MessageReceiveHandler(OCSMessage message);
    public event MessageReceiveHandler MessageReceived;

    protected abstract OCSMessage ParseMessageImpl(IntPtr source, string message);
    protected abstract bool CheckMessageQueueImpl(out IntPtr source, out IntPtr data, out int length);
    protected abstract void RemoveFirstMessageFromQueueImpl();

    protected virtual void OnMessageReceived(OCSMessage message) {
        if (MessageReceived != null) {
            MessageReceived(message);
        }
    }

    public void DispatchEvent() {
        if (Application.platform != RuntimePlatform.Android || Application.isEditor == false) {
            IntPtr source = default(IntPtr);
            IntPtr data = default(IntPtr);
            int length = 0;

            while (CheckMessageQueueImpl(out source, out data, out length)) {
                var array = new byte[length];
                Marshal.Copy(data, array, 0, length);
                RemoveFirstMessageFromQueueImpl();

                OnMessageReceived(ParseMessageImpl(source, System.Text.Encoding.UTF8.GetString(array, 0, length)));
            }
        }
    }
}
