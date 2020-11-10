using UnityEngine;
using System.Collections;
using System;
using Valve.VR;
using Valve.VR.InteractionSystem;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Valve.VR {
    [Serializable]
    public class SteamVR_Action_Boolean_List {

        [SerializeField]
        public string actionName;

        private string initialisedActionName = "";
        private List<SteamVR_Action_Boolean> actions;

        public SteamVR_Action_Boolean_List(string actionName) {
            this.actionName = actionName;
            UpdateActionsIfApplicable();
        }

        private void UpdateActionsIfApplicable() {
            if (!string.IsNullOrEmpty(initialisedActionName) && initialisedActionName.Equals(actionName)) {
                //Already up to date.
                return;
            }
            initialisedActionName = actionName;
            actions = SteamVR_Input.GetActions<SteamVR_Action_Boolean>(actionName);
        }

        public SteamVR_Action_Boolean SingleAction {
            get {
                if (actions.Count == 0) {
                    return null;
                }
                return actions[0];
            }
        }

        public bool GetState(SteamVR_Input_Sources inputSource) {

            UpdateActionsIfApplicable();

            foreach (SteamVR_Action_Boolean action in actions) {
                if (VrInputRemapper.GetState(action, inputSource)){
                    return true;
                }
            }

            return false;
        }

        public bool GetStateUp(SteamVR_Input_Sources inputSource) {

            UpdateActionsIfApplicable();
            
            foreach (SteamVR_Action_Boolean action in actions) {
                if (VrInputRemapper.GetState(action, inputSource)) {
                    //Something else is still down.
                    return false;
                }
                
            }
            foreach (SteamVR_Action_Boolean action in actions) {
                if (VrInputRemapper.GetStateUp(action, inputSource)) {
                    return true;
                }
            }

            return false;

        }

        public bool GetStateDown(SteamVR_Input_Sources inputSource) {

            UpdateActionsIfApplicable();

            foreach (SteamVR_Action_Boolean action in actions) {
                if (VrInputRemapper.GetState(action, inputSource) && !VrInputRemapper.GetStateDown(action, inputSource)) {
                    //Something else is already down.
                    return false;
                }

            }
            foreach (SteamVR_Action_Boolean action in actions) {
                if (VrInputRemapper.GetStateDown(action, inputSource)) {
                    return true;
                }
            }

            return false;

        }

    }
}
