using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Valve.VR.InteractionSystem {
    public class SceneCanvasHandler : MonoBehaviour {
        public bool showCanvasRaycasterWarnings = true;

        private Canvas[] canvases;
        private int uiLayer;

        private void OnEnable() {
            SceneManager.sceneLoaded += OnSceneLoaded;
            uiLayer = LayerMask.NameToLayer("UI");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            Debug.Log("A scene was loaded. Updating canvases for SteamVR.");

            canvases = FindObjectsOfType<Canvas>();

            Debug.Log("Found a total of " + canvases.Length + " canvases.");

            SetupCanvases();
        }

        private void SetupCanvases() {
            foreach(Canvas canvas in canvases) {
                if (canvas.renderMode != RenderMode.WorldSpace) {
                    continue;
                }

                GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
                if (raycaster != null) {
                    BoxCollider collider = canvas.gameObject.AddComponent<BoxCollider>();
                    RectTransform rectTransform = canvas.GetComponent<RectTransform>();
                    collider.size = rectTransform.sizeDelta;

                    canvas.worldCamera = null;
                } else {
#if UNITY_EDITOR
                    if (showCanvasRaycasterWarnings)
                        Debug.Log("No raycaster found on canvas " + canvas.gameObject.name + ". Not setting up as a VR interface.");
#endif
                }
            }
        }

        private void OnDestroy() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
