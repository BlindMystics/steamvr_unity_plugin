using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem {
    public class NoVrCamera : FallbackCameraController {

        [System.Serializable]
        public class KeyboardRemapping : IBooleanRemapping {
            public SteamVR_Action_Boolean action;
            public KeyCode keyboardKeyCode;

            public bool GetState(SteamVR_Input_Sources inputType) {
                if (noVrSuspended) {
                    return false;
                }
                return NoVrCamera.Instance.ControllingHand == inputType && Input.GetKey(keyboardKeyCode);
            }

            public bool GetStateDown(SteamVR_Input_Sources inputType) {
                if (noVrSuspended) {
                    return false;
                }
                return NoVrCamera.Instance.ControllingHand == inputType && Input.GetKeyDown(keyboardKeyCode);
            }

            public bool GetStateUp(SteamVR_Input_Sources inputType) {
                if (noVrSuspended) {
                    return false;
                }
                if (NoVrCamera.Instance.handChangedFrame) {
                    if (NoVrCamera.Instance.prevFrameControllingHand == inputType) {
                        return true;
                    }
                }
                return NoVrCamera.Instance.ControllingHand == inputType && Input.GetKeyUp(keyboardKeyCode);
            }
        }

        //public SteamVR_Action_Boolean teleportAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Teleport");


        public Hand rightHand;
        public Hand leftHand;

        private SteamVR_Input_Sources controllingHand = SteamVR_Input_Sources.RightHand;

        private SteamVR_Input_Sources prevFrameControllingHand = SteamVR_Input_Sources.RightHand;
        private bool handChangedFrame = false;

        public List<KeyboardRemapping> keyboardRemappings;

        public KeyCode handSwapKeycode = KeyCode.Tab;

        public KeyCode tiltHandUp = KeyCode.R;
        public KeyCode tiltHandDown = KeyCode.F;

        public enum HandControlType {
            STEAM_VR,
            FOCUSED_HAND_IN_FRONT_OF_CAMERA
        }

        public HandControlType controlType = HandControlType.FOCUSED_HAND_IN_FRONT_OF_CAMERA;

        private static bool noVrSuspended = false;
        public bool suspendNoVrInEditor = false;

        public static bool NoVrEnabled {
            get {
                return ((Instance != null) && (Instance.gameObject.activeInHierarchy));
            }
        }

        public static NoVrCamera Instance {
            get;
            private set;
        }

        public SteamVR_Input_Sources ControllingHand {
            get {
                return controllingHand;
            }
            private set {
                if (value == controllingHand) {
                    //Nothing to change.
                    return;
                }
                prevFrameControllingHand = controllingHand;
                controllingHand = value;
                if (controllingHand == SteamVR_Input_Sources.RightHand) {
                    //rightHand.ForceHoverUnlock();
                    //leftHand.ForceHoverLock();
                } else {
                    //rightHand.ForceHoverLock();
                    //leftHand.ForceHoverUnlock();
                }
            }
        }

        private void Awake() {
            Instance = this;
            ControllingHand = controllingHand;
        }

        protected override void OnEnable() {
            base.OnEnable();
            noVrSuspended = true;
#if UNITY_EDITOR
            noVrSuspended = suspendNoVrInEditor;
#endif
        }

        private void Start() {
            if (noVrSuspended) {
                if (SteamVrErrorInfoBase.Instance == null) {
                    Debug.LogWarning("No SteamVrErrorInfoBase.Instance found.");
                } else {
                    SteamVrErrorInfoBase.Instance.OnSteamVrInitialisationFailed();
                }
            }
        }

        public static void UnSuspendNoVr() {
            noVrSuspended = false;
        }

        public static IBooleanRemapping GetRemapping(SteamVR_Action_Boolean vR_Action_Boolean) {
            if (!NoVrEnabled) {
                return null;
            }
            foreach (KeyboardRemapping keyboardRemapping in Instance.keyboardRemappings) {
                if (keyboardRemapping.action.Equals(vR_Action_Boolean)) {
                    return keyboardRemapping;
                }
            }
            return null;
        }

        protected override void Update() {

            if (noVrSuspended) {
                return;
            }

            base.Update();

            if (Input.GetKeyDown(handSwapKeycode)) {
                if (ControllingHand == SteamVR_Input_Sources.LeftHand) {
                    ControllingHand = SteamVR_Input_Sources.RightHand;
                } else if (ControllingHand == SteamVR_Input_Sources.RightHand) {
                    ControllingHand = SteamVR_Input_Sources.LeftHand;
                } else {
                    Debug.LogError("Unsupported input source: " + ControllingHand);
                }
                handChangedFrame = true;
            } else {
                handChangedFrame = false;
            }

        }
    }
}