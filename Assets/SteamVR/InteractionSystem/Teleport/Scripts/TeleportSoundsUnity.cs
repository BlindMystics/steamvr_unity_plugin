using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem {
    public class TeleportSoundsUnity : TeleportSounds {

        [Header( "Audio Sources" )]
        public AudioSource pointerAudioSource;
        public AudioSource loopingAudioSource;
        public AudioSource headAudioSource;
        public AudioSource reticleAudioSource;

        [Header( "Sounds" )]
        public AudioClip teleportSound;
        public AudioClip pointerStartSound;
        public AudioClip pointerLoopSound;
        public AudioClip pointerStopSound;
        public AudioClip goodHighlightSound;
        public AudioClip badHighlightSound;

        // --- Private Variables.

        private float loopingAudioMaxVolume;

        private Player player;

        private void Start() {

            player = InteractionSystem.Player.instance;

            loopingAudioMaxVolume = loopingAudioSource.volume;
        }

        private void PlayAudioClip(AudioSource source, AudioClip clip) {
            source.clip = clip;
            source.Play();
        }

        #region TeleportSounds Implementation

        public override void PlayPointerLoopSound() {
            loopingAudioSource.clip = pointerLoopSound;
            loopingAudioSource.loop = true;
            loopingAudioSource.Play();
            loopingAudioSource.volume = 0.0f;
        }

        public override void PlayPointerStartSound() {
            PlayAudioClip(pointerAudioSource, pointerStartSound);
        }

        public override void PlayPointerStopSound() {
            PlayAudioClip(pointerAudioSource, pointerStopSound);
        }

        public override void SetPointerStartTransform(Transform pointerStartTransform) {
            pointerAudioSource.transform.SetParent(pointerStartTransform);
            pointerAudioSource.transform.localPosition = Vector3.zero;

            loopingAudioSource.transform.SetParent(pointerStartTransform);
            loopingAudioSource.transform.localPosition = Vector3.zero;
        }

        public override void StopPointerLoopSound() {
            loopingAudioSource.Stop();
        }

        public override void SetPointerLoopVolumePercent(float percentage) {
            loopingAudioSource.volume = percentage * loopingAudioMaxVolume;
        }

        public override void PlayTeleportGoSound() {
            headAudioSource.transform.SetParent(player.hmdTransform);
            headAudioSource.transform.localPosition = Vector3.zero;
            PlayAudioClip(headAudioSource, teleportSound);
        }

        public override void SetReticleAudioSourcePosition(Vector3 position) {
            reticleAudioSource.transform.position = position;
        }

        public override void PlayReticleHighlightGoodSound() {
            PlayAudioClip(reticleAudioSource, goodHighlightSound);
        }

        public override void PlayReticleHighlightBadSound() {
            PlayAudioClip(reticleAudioSource, badHighlightSound);
        }

        #endregion

    }
}