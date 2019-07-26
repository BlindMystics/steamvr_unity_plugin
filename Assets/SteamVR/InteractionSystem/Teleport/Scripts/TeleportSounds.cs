using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem {
    public abstract class TeleportSounds : MonoBehaviour {

        public abstract void SetPointerStartTransform(Transform pointerStartTransform);

        public abstract void PlayPointerStartSound();

        public abstract void PlayPointerStopSound();

        public abstract void PlayPointerLoopSound();

        public abstract void SetPointerLoopVolumePercent(float percentage);

        public abstract void StopPointerLoopSound();

        public abstract void PlayTeleportGoSound();

        public abstract void SetReticleAudioSourcePosition(Vector3 position);

        public abstract void PlayReticleHighlightGoodSound();

        public abstract void PlayReticleHighlightBadSound();

    }
}