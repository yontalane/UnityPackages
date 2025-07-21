using UnityEngine;

namespace Yontalane
{
    /// <summary>
    /// A component that plays audio clips and audio packs based on animation events.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Animation Sound Player")]
    public class AnimationSoundPlayer : MonoBehaviour
    {
        /// <summary>
        /// Plays the audio clip or audio pack based on the animation event.
        /// </summary>
        public void Play(AnimationEvent animationEvent)
        {
            // Return if the animation event has no object reference parameter.
            if (animationEvent.objectReferenceParameter == null)
            {
                return;
            }

            // Play the audio clip if the object reference parameter is an AudioClip.
            if ( animationEvent.objectReferenceParameter is AudioClip clip )
            {
                // Get the volume from the animation event.
                float volume = animationEvent.floatParameter > 0f ? animationEvent.floatParameter : 1f;
                PlayClip(clip, volume);
            }
            // Play the audio pack if the object reference parameter is an AudioPack.
            else if ( animationEvent.objectReferenceParameter is AudioPack pack )
            {
                // Play the audio pack.
                PlayPack(pack);
            }
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        private void PlayClip( AudioClip clip, float volume )
        {
            // Play the audio clip at the camera's position.
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, volume);
        }

        /// <summary>
        /// Plays the audio pack.
        /// </summary>
        private void PlayPack( AudioPack pack )
        {
            // Play the audio pack.
            AudioPack.Play(pack);
        }
    }
}
