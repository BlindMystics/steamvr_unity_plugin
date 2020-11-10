using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace Valve.VR.InteractionSystem {
    [RequireComponent(typeof(Canvas))]
    public class ThirdUiCanvasInitialiser : MonoBehaviour {

        private Canvas correspondingCanvas;
        private bool setup = false;

        private void OnEnable() {

            if (setup) {
                return;
            }
            setup = true;

            correspondingCanvas = GetComponent<Canvas>();

            string errorInfo;
            bool setupSuccess = SceneCanvasHandler.SetupSingleCanvas(correspondingCanvas, out errorInfo);
            if (!setupSuccess) {
                Debug.LogError("Failed to setup canvas '" + gameObject.name + "' - " + errorInfo + ". Not setting up as a VR interface.");
            }

        }

    }
}