/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the MIT license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OCSCameraFade : MonoBehaviour {
    private static List<OCSCameraFade> _cameraFades = new List<OCSCameraFade>();

    public static async Task FadeAllCameras(bool fadeIn, float duration) {
        await Task.WhenAll(_cameraFades.Select(fade => fade.fadeTask(fadeIn, duration)));

        for (bool anyCameraIsFading = true; anyCameraIsFading;) {
            anyCameraIsFading = false;
            foreach (var cameraFade in _cameraFades) {
                anyCameraIsFading = anyCameraIsFading || cameraFade.isFading;
                if (anyCameraIsFading) {
                    break;
                }
            }
            if (anyCameraIsFading) {
                await Task.Yield();
            }
        }
    }

    public static void FadeAllCamerasImmediately(bool fadeIn) {
        foreach (var cameraFade in _cameraFades) {
            cameraFade.FadeImmediately(fadeIn);
        }
    }

    private Material _fadeMaterial;
    private Color _startFadeColor;
    private Color _endFadeColor;
    private float _startTimeToFade;

    [SerializeField]
    internal Color fadeOutColor = Color.black;

    private void Awake() {
        _fadeMaterial = new Material(Shader.Find("onCloudStream/Unlit transparent color"));
        _fadeMaterial.color = Color.clear;

        _cameraFades.Add(this);
    }

    private void OnDestroy() {
        _cameraFades.Remove(this);
    }

    private void OnPostRender() {
        if (_fadeMaterial.color != Color.clear) {
            _fadeMaterial.SetPass(0);
            GL.PushMatrix();
            GL.LoadOrtho();
            GL.Color(_fadeMaterial.color);
            GL.Begin(GL.QUADS);
            GL.Vertex3(0.0f, 0.0f, -1.0f);
            GL.Vertex3(0.0f, 1.0f, -1.0f);
            GL.Vertex3(1.0f, 1.0f, -1.0f);
            GL.Vertex3(1.0f, 0.0f, -1.0f);
            GL.End();
            GL.PopMatrix();
        }
    }

    public bool isFading { get; private set; }

    public async void Fade(bool fadeIn, float duration) {
        await fadeTask(fadeIn, duration);
    }

    public void FadeImmediately(bool fadeIn) {
        _startFadeColor = fadeIn ? fadeOutColor : Color.clear;
        _endFadeColor = fadeIn ? Color.clear : fadeOutColor;

        _fadeMaterial.color = _endFadeColor;
    }

    private async Task fadeTask(bool fadeIn, float duration) {
        _startFadeColor = isFading ? _fadeMaterial.color : (fadeIn ? fadeOutColor : Color.clear);
        _endFadeColor = fadeIn ? Color.clear : fadeOutColor;

        _startTimeToFade = Time.realtimeSinceStartup;

        if (isFading == false) {
            isFading = true;
            _fadeMaterial.color = _startFadeColor;
            while (_fadeMaterial.color != _endFadeColor) {
                _fadeMaterial.color = Color.Lerp(_startFadeColor, _endFadeColor, (Time.realtimeSinceStartup - _startTimeToFade) / duration);
                await Task.Yield();
            }
            isFading = false;
        }
    }
}
