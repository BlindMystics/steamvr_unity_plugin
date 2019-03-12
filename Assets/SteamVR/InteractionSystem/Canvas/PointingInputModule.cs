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

        private List<HandCanvasInteractionHandler> interactionHandlers;

        private HandCanvasInteractionHandler currentInteractionHandler = null;

        protected override void Awake() {
            base.Awake();
            _instance = this;
            interactionHandlers = new List<HandCanvasInteractionHandler>();
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
                    currentInteractionHandler = null;
                    return;
                } else {
                    currentInteractionHandler.ClaimCanvas();
                    currentInteractionHandler.ShowPointer(true);
                }

                PointerEventData currentPointerEventData = currentInteractionHandler.PointerEventData;
                base.HandlePointerExitAndEnter(currentPointerEventData, currentInteractionHandler.CurrentGameObject);

                if (currentInteractionHandler.InteractionButtonPressed) {

                    GameObject newPressed = ExecuteEvents.ExecuteHierarchy(currentInteractionHandler.CurrentGameObject,
                        currentPointerEventData, ExecuteEvents.pointerDownHandler);

                    if (newPressed == null) {
                        newPressed = ExecuteEvents.ExecuteHierarchy(currentInteractionHandler.CurrentGameObject,
                            currentPointerEventData, ExecuteEvents.pointerClickHandler);
                    }

                    currentPointerEventData.pointerPress = newPressed;
                    currentPointerEventData.pointerDrag = null;
                    Select(newPressed);
                }

                if (currentInteractionHandler.InteractionButtonHeld && !currentPointerEventData.dragging) {
                    ExecuteEvents.Execute(currentPointerEventData.pointerPress, currentPointerEventData, ExecuteEvents.beginDragHandler);
                    currentPointerEventData.pointerDrag = currentPointerEventData.pointerPress;
                    currentPointerEventData.dragging = true;
                    Debug.Log("Dragging started on object " + currentPointerEventData.pointerDrag?.name);
                }

                if (currentInteractionHandler.InteractionButtonReleased) {
                    currentPointerEventData.dragging = false;

                    if (currentPointerEventData.pointerDrag != null) {
                        ExecuteEvents.Execute(currentPointerEventData.pointerDrag, currentPointerEventData, ExecuteEvents.endDragHandler);
                        if (currentInteractionHandler.CurrentGameObject != null) {
                            ExecuteEvents.ExecuteHierarchy(currentInteractionHandler.CurrentGameObject, currentPointerEventData, ExecuteEvents.dropHandler);
                        }
                        currentPointerEventData.pointerDrag = null;
                        Debug.Log("Stopped dragging.");
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
            interactionHandlers.Add(interactionHandler);
        }
    }
}
