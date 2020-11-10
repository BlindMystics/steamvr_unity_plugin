using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Valve.VR.InteractionSystem {
    public class SceneCanvasHandler : MonoBehaviour {

        public static SceneCanvasHandler Instance {
            get;
            private set;
        }

        private void Awake() {
            if (Instance != null) {
                throw new UnityException("Multiple " + GetType().Name + " found!");
            }
            Instance = this;
        }

        public bool showCanvasDebugInformation = true;

        public static bool SetupSingleCanvas(Canvas canvas, out string errorInfo) {

            if (canvas.renderMode != RenderMode.WorldSpace) {
                errorInfo = "canvas.renderMode != RenderMode.WorldSpace";
                return false;
            }

            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster != null) {
                BoxCollider collider = canvas.gameObject.AddComponent<BoxCollider>();
                RectTransform rectTransform = canvas.GetComponent<RectTransform>();
                collider.size = rectTransform.sizeDelta;

                canvas.worldCamera = null;
            } else {
                errorInfo = "No raycaster found on canvas";
                return false;
            }

            errorInfo = "";
            return true;

        }

    }
}
