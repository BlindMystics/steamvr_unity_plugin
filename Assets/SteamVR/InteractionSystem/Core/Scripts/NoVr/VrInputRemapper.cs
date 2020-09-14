using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem {

    public interface IBooleanRemapping {
        bool GetState(SteamVR_Input_Sources inputType);
        bool GetStateDown(SteamVR_Input_Sources inputType);
        bool GetStateUp(SteamVR_Input_Sources inputType);
    }

    public class VrInputRemapper : MonoBehaviour {

        private struct ActionOverrideInformation {
            public bool currentDown;
            public int currentFrameId;
        }

        private static Dictionary<SteamVR_Action_Boolean, ActionOverrideInformation> overiddenActions = 
            new Dictionary<SteamVR_Action_Boolean, ActionOverrideInformation>();

        public static void UpdateActionOverride(SteamVR_Action_Boolean action, bool isDown) {
            ActionOverrideInformation actionOverrideInformation = new ActionOverrideInformation();

            actionOverrideInformation.currentFrameId = Time.frameCount + 1; //Do it next frame.
            actionOverrideInformation.currentDown = isDown;

            overiddenActions[action] = actionOverrideInformation;
            //overiddenActions.Add(action, actionOverrideInformation); ???
        }

        public static void ClearActionOverride(SteamVR_Action_Boolean action) {
            if (overiddenActions.ContainsKey(action)) {
                overiddenActions.Remove(action);
            }
        }

        /// <summary>
        /// Returns true the frame that the boolean is set to true.
        /// </summary>
        /// <param name="vR_Action_Boolean"></param>
        /// <param name="inputType"></param>
        /// <returns></returns>
        public static bool GetStateDown(SteamVR_Action_Boolean vR_Action_Boolean, SteamVR_Input_Sources inputType) {

            if (overiddenActions.ContainsKey(vR_Action_Boolean)) {
                ActionOverrideInformation actionOverrideInformation = overiddenActions[vR_Action_Boolean];
                return ((actionOverrideInformation.currentFrameId == Time.frameCount) &&
                    (actionOverrideInformation.currentDown));
            }

            IBooleanRemapping booleanRemapping = NoVrCamera.GetRemapping(vR_Action_Boolean);

            if (booleanRemapping != null) {
                return booleanRemapping.GetStateDown(inputType);
            } else if (!NoVrCamera.NoVrEnabled) {
                return vR_Action_Boolean.GetStateDown(inputType);
            } else {
                return false;
            }

        }

        /// <summary>
        /// Returns true the frame that the boolean is set to false
        /// </summary>
        /// <param name="vR_Action_Boolean"></param>
        /// <param name="inputType"></param>
        /// <returns></returns>
        public static bool GetStateUp(SteamVR_Action_Boolean vR_Action_Boolean, SteamVR_Input_Sources inputType) {

            if (overiddenActions.ContainsKey(vR_Action_Boolean)) {
                ActionOverrideInformation actionOverrideInformation = overiddenActions[vR_Action_Boolean];
                return ((actionOverrideInformation.currentFrameId == Time.frameCount) &&
                    (!actionOverrideInformation.currentDown));
            }

            IBooleanRemapping booleanRemapping = NoVrCamera.GetRemapping(vR_Action_Boolean);

            if (booleanRemapping != null) {
                return booleanRemapping.GetStateUp(inputType);
            } else if (!NoVrCamera.NoVrEnabled) {
                return vR_Action_Boolean.GetStateUp(inputType);
            } else {
                return false;
            }

        }

        /// <summary>
        /// Returns the state of the boolean (IE true == pressed & false == not pressed).
        /// </summary>
        /// <param name="vR_Action_Boolean"></param>
        /// <param name="inputType"></param>
        /// <returns></returns>
        public static bool GetState(SteamVR_Action_Boolean vR_Action_Boolean, SteamVR_Input_Sources inputType) {

            if (overiddenActions.ContainsKey(vR_Action_Boolean)) {
                ActionOverrideInformation actionOverrideInformation = overiddenActions[vR_Action_Boolean];
                return actionOverrideInformation.currentDown;
            }

            IBooleanRemapping booleanRemapping = NoVrCamera.GetRemapping(vR_Action_Boolean);

            if (booleanRemapping != null) {
                return booleanRemapping.GetState(inputType);
            } else if (!NoVrCamera.NoVrEnabled) {
                return vR_Action_Boolean.GetState(inputType);
            } else {
                return false;
            }

        }

    }
}