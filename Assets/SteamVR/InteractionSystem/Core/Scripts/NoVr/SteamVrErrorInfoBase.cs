using UnityEngine;

public abstract class SteamVrErrorInfoBase : MonoBehaviour {


    public static SteamVrErrorInfoBase Instance {
        get;
        private set;
    }

    protected virtual void Awake() {
        Instance = this;
    }

    public abstract void OnSteamVrInitialisationFailed();

}
