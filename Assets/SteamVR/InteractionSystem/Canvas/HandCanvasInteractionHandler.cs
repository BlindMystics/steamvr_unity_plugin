using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Valve.VR.InteractionSystem {
    public class HandCanvasInteractionHandler : MonoBehaviour {
        private SteamVR_Action_Boolean interactUI = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("InteractUI");

        public Camera handInteractionEventCamera;
        public LineRenderer lineRenderer;

        private int raycastLayerMask;

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

            raycastLayerMask = 1 << LayerMask.NameToLayer("UI");
            PointingInputModule.instance.AddCanvasInteractionHandler(this);

            pointerEventData = new PointerEventData(PointingInputModule.instance.EventSystem);
        }

        public void UpdateInteractionHandler() {
            previousGameObject = currentGameObject;

            bool buttonState = VrInputRemapper.GetState(interactUI, Hand.handType);
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
                lastInteractTime = Time.time;
            }

            Ray cameraRay = handInteractionEventCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

            RaycastHit hit;
            bool raycastHit = Physics.Raycast(cameraRay, out hit, 10f, raycastLayerMask);
            if (raycastHit) {
                Canvas canvas = hit.collider.gameObject.GetComponent<Canvas>();
                if (canvas == null) {
                    return;
                }

                currentCanvas = canvas;
            } else {
                handInteractionEventCamera.enabled = false;
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

        private void RaycastCanvas() {
            GraphicRaycaster raycaster = currentCanvas.GetComponent<GraphicRaycaster>();

            //This is relative to the event camera POV.
            //Ensure that the camera FOV is near 0.
            pointerEventData.position = new Vector2(0.5f, 0.5f);

            List<RaycastResult> raycastResults = new List<RaycastResult>();

            raycaster.Raycast(pointerEventData, raycastResults);
            
            if (raycastResults.Count > 0) {
                currentGameObject = raycastResults[0].gameObject;
            } else {
                currentGameObject = null;
            }
        }

        public void ShowPointer(bool value) {
            lineRenderer.enabled = value;
        }

        public void ClaimCanvas() {
            handInteractionEventCamera.enabled = true;
            currentCanvas.worldCamera = handInteractionEventCamera;
        }

        public void UnclaimCanvas() {
            if (currentCanvas == null) {
                return;
            }

            handInteractionEventCamera.enabled = false;
            if (currentCanvas.worldCamera == handInteractionEventCamera) {
                currentCanvas.worldCamera = null;
            }
        }
    }
}

