using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Valve.VR.InteractionSystem {
    public class PointingInputModule : BaseInputModule {
        public string[] additionalUILayers;

        private static PointingInputModule _instance;
        public static PointingInputModule Instance {
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

        private bool inputLock = false;
        public bool InputLock {
            get => inputLock;
        }

        private List<HandCanvasInteractionHandler> interactionHandlers;

        private HandCanvasInteractionHandler currentInteractionHandler = null;

        private InputField activeInputField = null;

        private StandaloneInputModule keyboardHandlingInputModule;

        protected override void Awake() {
            base.Awake();
            _instance = this;

            SteamVR_Events.System(EVREventType.VREvent_KeyboardCharInput).Listen(OnKeyboard);
            SteamVR_Events.System(EVREventType.VREvent_KeyboardClosed).Listen(OnKeyboardClosed);

           //WARNING: Ensure that this input module is specified above the StandaloneInputModule, or we will lose control.
            keyboardHandlingInputModule = GetComponent<StandaloneInputModule>();
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

                        bool shouldReleaseOriginal = false;

                        if (currentInteractionHandler.CurrentGameObject != null) {
                            //Make sure that we're releasing the same object that we pressed.
                            GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerDownHandler>(currentInteractionHandler.CurrentGameObject);
                            if (eventHandler == currentPointerEventData.pointerPress) {
                                ExecuteEvents.Execute(currentPointerEventData.pointerPress, currentPointerEventData, ExecuteEvents.pointerClickHandler);
                                ExecuteEvents.Execute(currentPointerEventData.pointerPress, currentPointerEventData, ExecuteEvents.pointerUpHandler);
                            } else {
                                shouldReleaseOriginal = true;
                            }
                        } else {
                            shouldReleaseOriginal = true;
                        }

                        if (shouldReleaseOriginal) {
                            ExecuteEvents.Execute(currentPointerEventData.pointerPress, currentPointerEventData, ExecuteEvents.pointerUpHandler);
                            ClearSelection();
                        }

                        currentPointerEventData.rawPointerPress = null;
                        currentPointerEventData.pointerPress = null;

                    }
                }

                if (currentPointerEventData.dragging) {
                    ExecuteEvents.Execute(currentPointerEventData.pointerDrag, currentPointerEventData, ExecuteEvents.dragHandler);
                }
            }

            handleKeyboard();
        }

        private void handleKeyboard() {
            if (activeInputField == null) {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape)) {
                EndInputField();
            }

            keyboardHandlingInputModule.Process();
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
                HandleInputField(gameObject);
            }
        }

        private void HandleInputField(GameObject gameObject) {
            InputField inputField = gameObject.GetComponent<InputField>();
            if (inputField != null) {
                ShowKeyboard(inputField);
            }
        }

        //Documentation: https://github.com/ValveSoftware/openvr/blob/41bfc14efef21b2959394d8b4c29b82c3bdd7d12/samples/unity_keyboard_sample/Assets/KeyboardSample.cs
        private void ShowKeyboard(InputField inputField) {
            activeInputField = inputField;

            if (NoVrCamera.NoVrEnabled) {
                inputLock = true;
            } else {
                int characterLimit = inputField.characterLimit;

                if (characterLimit == 0) {
                    characterLimit = 256;
                }

                string descriptionText = inputField.placeholder?.GetComponent<Text>()?.text;
                SteamVR.instance.overlay.ShowKeyboard((int)EGamepadTextInputMode.k_EGamepadTextInputModeNormal,
                    (int)EGamepadTextInputLineMode.k_EGamepadTextInputLineModeSingleLine,
                    descriptionText, (uint)characterLimit, inputField.text, false, 0);
            }
        }

        private void OnKeyboard(VREvent_t args) {
            if (activeInputField == null) {
                SteamVR.instance.overlay.HideKeyboard();
                return;
            }

            VREvent_Keyboard_t keyboard = args.data.keyboard;
            byte[] inputBytes = new byte[] { keyboard.cNewInput0, keyboard.cNewInput1, keyboard.cNewInput2, keyboard.cNewInput3, keyboard.cNewInput4, keyboard.cNewInput5, keyboard.cNewInput6, keyboard.cNewInput7 };
            int len = 0;
            for (; inputBytes[len] != 0 && len < 7; len++) ;
            string input = System.Text.Encoding.UTF8.GetString(inputBytes, 0, len);

            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(1024);
            uint size = SteamVR.instance.overlay.GetKeyboardText(stringBuilder, 1024);
            activeInputField.text = stringBuilder.ToString();
        }

        private void OnKeyboardClosed(VREvent_t args) {
            EndInputField();
        }

        private void EndInputField() {
            if (activeInputField == null) {
                return;
            }

            inputLock = false;
            activeInputField = null;
            ClearSelection();
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
