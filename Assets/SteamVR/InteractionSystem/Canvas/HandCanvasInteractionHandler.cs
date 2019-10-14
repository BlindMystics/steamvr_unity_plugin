using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Valve.VR.InteractionSystem {
    public class HandCanvasInteractionHandler : MonoBehaviour {
        private SteamVR_Action_Boolean interactUI = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("InteractUI");

        public Camera eventCamera;
        public LineRenderer lineRenderer;

        private LayerMask _raycastLayerMask;

        private PointerEventData pointerEventData;
        public PointerEventData PointerEventData {
            get => pointerEventData;
        }

        private Hand hand;
        public Hand Hand { get => hand; }

        private Canvas currentCanvas = null;
        public Canvas CurrentCanvas { get => currentCanvas; }

        private GameObject previousGameObject = null;
        public GameObject PreviousGameObject { get => previousGameObject; }

        private GameObject currentGameObject = null;
        public GameObject CurrentGameObject { get => currentGameObject; }

        private float lastInteractTime;
        public float LastInteractTime {
            get => lastInteractTime;
        }

        private bool interactionButtonPressed = false;
        public bool InteractionButtonPressed {
            get => interactionButtonPressed;
        }

        private bool interactionButtonHeld = false;
        public bool InteractionButtonHeld {
            get => interactionButtonHeld;
        }

        private bool interactionButtonReleased = false;
        public bool InteractionButtonReleased {
            get => interactionButtonReleased;
        }

        private void Start() {
            hand = GetComponent<Hand>();

            _raycastLayerMask = 1 << LayerMask.NameToLayer("UI");

            foreach (string layer in PointingInputModule.Instance.additionalUILayers) {
                _raycastLayerMask |= 1 << LayerMask.NameToLayer(layer);
            }

            pointerEventData = new PointerEventData(PointingInputModule.Instance.EventSystem);
        }

        private void OnEnable() {
            PointingInputModule.Instance.AddCanvasInteractionHandler(this);
        }

        private void OnDisable() {
            PointingInputModule.Instance.RemoveCanvasInteractionHandler(this);
        }

        private LayerMask RaycastLayerMask {
            get {
                if (PointingInputModule.Instance.UseRaycastLayerMaskOverride) {
                    return PointingInputModule.Instance.RaycastLayerMaskOverride;
                }
                return _raycastLayerMask;
            }
        }

        public void UpdateInteractionHandler() {
            previousGameObject = currentGameObject;

            UpdateInteractionButtonState();

            bool raycastHit;
            RaycastHit hit = new RaycastHit();

            if (Hand.AttachedObjects.Count > 0) {
                raycastHit = false;
            } else {
                Ray cameraRay = eventCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
                raycastHit = Physics.Raycast(cameraRay, out hit, 10f, RaycastLayerMask);
            }
            
            if (raycastHit) {
                GameObject hitObject = hit.collider.gameObject;
                Canvas canvas = hitObject.GetComponent<Canvas>();
                if (canvas == null) {
                    CanvasHolder holder = hitObject.GetComponent<CanvasHolder>();
                    if (holder != null) {
                        canvas = holder.canvas;
                    } else {
                        return;
                    }
                }

                currentCanvas = canvas;
            } else {
                eventCamera.enabled = false;
                UnclaimCanvas();
                currentCanvas = null;
            }

            if (currentCanvas != null) {
                lineRenderer.SetPositions(new Vector3[] {
                    new Vector3(),
                    lineRenderer.transform.InverseTransformPoint(hit.point)
                });

                RaycastCanvas();
            }
        }

        private void UpdateInteractionButtonState() {
            bool buttonState = !PointingInputModule.Instance.InputLock && 
                VrInputRemapper.GetState(interactUI, Hand.handType);

            if (buttonState) {
                if (interactionButtonPressed) {
                    interactionButtonPressed = false;
                    interactionButtonHeld = true;
                } else if (!interactionButtonHeld) {
                    interactionButtonPressed = true;
                }
            } else {
                if (interactionButtonPressed || interactionButtonHeld) {
                    interactionButtonReleased = true;
                } else {
                    interactionButtonReleased = false;
                }

                interactionButtonPressed = false;
                interactionButtonHeld = false;
            }

            if (interactionButtonPressed) {
                lastInteractTime = Time.unscaledTime;
            }
        }

        private void RaycastCanvas() {
            GraphicRaycaster raycaster = currentCanvas.GetComponent<GraphicRaycaster>();

            //This is relative to the event camera POV. We want to use the center here.
            pointerEventData.position = new Vector2(eventCamera.pixelWidth * 0.5f, eventCamera.pixelHeight * 0.5f);
            pointerEventData.delta = new Vector2(0f, 0f);

            List<RaycastResult> raycastResults = new List<RaycastResult>();

            raycaster.Raycast(pointerEventData, raycastResults);
            
            if (raycastResults.Count > 0) {
                pointerEventData.pointerCurrentRaycast = raycastResults[0];
                currentGameObject = pointerEventData.pointerCurrentRaycast.gameObject;
            } else {
                pointerEventData.pointerCurrentRaycast = new RaycastResult();
                currentGameObject = null;
            }
        }

        public void ShowPointer(bool value) {
            lineRenderer.enabled = value;
        }

        public void ClaimCanvas() {
            eventCamera.enabled = true;
            if (currentCanvas.renderMode == RenderMode.WorldSpace) {
                currentCanvas.worldCamera = eventCamera;
            }
        }

        public void UnclaimCanvas() {
            if (currentCanvas == null) {
                return;
            }

            eventCamera.enabled = false;
            if (currentCanvas.worldCamera == eventCamera) {
                currentCanvas.worldCamera = null;
            }
        }
    }
}

