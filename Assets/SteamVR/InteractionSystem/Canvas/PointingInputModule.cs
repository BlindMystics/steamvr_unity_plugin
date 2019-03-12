using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Valve.VR.InteractionSystem {
    public class PointingInputModule : BaseInputModule {
        private static PointingInputModule _instance;
        public static PointingInputModule instance {
            get {
                if (_instance == null)
                    _instance = GameObject.FindObjectOfType<PointingInputModule>();

                return _instance;
            }
        }

        public EventSystem EventSystem {
            get => base.eventSystem;
        }

        public Camera EventCamera {
            get {
                return currentInteractionHandler?.eventCamera;
            }
        }

        private List<HandCanvasInteractionHandler> interactionHandlers;

        private HandCanvasInteractionHandler currentInteractionHandler = null;

        protected override void Awake() {
            base.Awake();
            _instance = this;
        }

        public override void Process() {
            foreach(HandCanvasInteractionHandler interactionHandler in interactionHandlers) {
                interactionHandler.UpdateInteractionHandler();
                interactionHandler.ShowPointer(false);

                if (currentInteractionHandler == null ||
                    interactionHandler.LastInteractTime > currentInteractionHandler.LastInteractTime) {
                    currentInteractionHandler = interactionHandler;
                }
            }

            if (currentInteractionHandler != null) {
                if (currentInteractionHandler.CurrentCanvas == null) {
                    return;
                } else {
                    currentInteractionHandler.ClaimCanvas();
                    currentInteractionHandler.ShowPointer(true);
                }

                PointerEventData currentPointerEventData = currentInteractionHandler.PointerEventData;
                base.HandlePointerExitAndEnter(currentPointerEventData, currentInteractionHandler.CurrentGameObject);

                if (currentInteractionHandler.InteractionButtonPressed) {
                    currentPointerEventData.pressPosition = currentPointerEventData.position;
                    currentPointerEventData.pointerPressRaycast = currentPointerEventData.pointerCurrentRaycast;

                    GameObject newPressed = ExecuteEvents.ExecuteHierarchy(currentInteractionHandler.CurrentGameObject,
                        currentPointerEventData, ExecuteEvents.pointerDownHandler);

                    if (newPressed == null) {
                        newPressed = ExecuteEvents.ExecuteHierarchy(currentInteractionHandler.CurrentGameObject,
                            currentPointerEventData, ExecuteEvents.pointerClickHandler);
                    }

                    currentPointerEventData.pointerPress = newPressed;
                    currentPointerEventData.rawPointerPress = currentInteractionHandler.CurrentGameObject;
                    currentPointerEventData.pointerDrag = null;
                    Select(newPressed);
                }

                if (currentInteractionHandler.InteractionButtonHeld && !currentPointerEventData.dragging) {
                    ExecuteEvents.Execute(currentPointerEventData.pointerPress, currentPointerEventData, ExecuteEvents.beginDragHandler);
                    currentPointerEventData.pointerDrag = currentPointerEventData.pointerPress;
                    currentPointerEventData.dragging = true;
                }

                if (currentInteractionHandler.InteractionButtonReleased) {
                    currentPointerEventData.dragging = false;

                    if (currentPointerEventData.pointerDrag != null) {
                        ExecuteEvents.Execute(currentPointerEventData.pointerDrag, currentPointerEventData, ExecuteEvents.endDragHandler);
                        if (currentInteractionHandler.CurrentGameObject != null) {
                            ExecuteEvents.ExecuteHierarchy(currentInteractionHandler.CurrentGameObject, currentPointerEventData, ExecuteEvents.dropHandler);
                        }
                        currentPointerEventData.pointerDrag = null;
                    }

                    if (currentPointerEventData.pointerPress != null) {
                        ExecuteEvents.Execute(currentPointerEventData.pointerPress, currentPointerEventData, ExecuteEvents.pointerClickHandler);

                        ExecuteEvents.Execute(currentPointerEventData.pointerPress, currentPointerEventData, ExecuteEvents.pointerUpHandler);

                        currentPointerEventData.rawPointerPress = null;
                        currentPointerEventData.pointerPress = null;
                    }
                }

                if (currentPointerEventData.dragging) {
                    ExecuteEvents.Execute(currentPointerEventData.pointerDrag, currentPointerEventData, ExecuteEvents.dragHandler);
                }
            }
        }

        public void ClearSelection() {
            if (base.eventSystem.currentSelectedGameObject) {
                base.eventSystem.SetSelectedGameObject(null);
            }
        }

        public void Select(GameObject gameObject) {
            ClearSelection();

            if (ExecuteEvents.GetEventHandler<ISelectHandler>(gameObject)) {
                base.eventSystem.SetSelectedGameObject(gameObject);
            }
        }

        public void AddCanvasInteractionHandler(HandCanvasInteractionHandler interactionHandler) {
            if (interactionHandlers == null) {
                interactionHandlers = new List<HandCanvasInteractionHandler>();
            }

            interactionHandlers.Add(interactionHandler);

            if (currentInteractionHandler == null) {
                interactionHandler = currentInteractionHandler;
            }
        }

        public void RemoveCanvasInteractionHandler(HandCanvasInteractionHandler interactionHandler) {
            interactionHandlers?.Remove(interactionHandler);
        }
    }
}
