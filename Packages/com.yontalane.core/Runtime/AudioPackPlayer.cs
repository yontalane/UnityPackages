using System;
using System.Collections;
using UnityEngine;

namespace Yontalane
{
    /// <summary>
    /// A component that plays an audio clip and destroys itself after the clip has finished playing.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/AudioPack Player")]
    public class AudioPackPlayer : MonoBehaviour
    {
        /// <summary>
        /// Initializes the AudioPackPlayer with the specified audio clip, volume, and callback.
        /// </summary>
        public void Initiate( AudioClip clip, float volume, Action<AudioPackPlayer> callback )
        {
            // Get the AudioSource component.
            AudioSource audioSource = GetComponent<AudioSource>();

            // Set the game object to active.
            gameObject.SetActive(true);
            audioSource.volume = volume;
            audioSource.clip = clip;
            audioSource.Play();

            // Destroy the AudioPackPlayer after the clip has finished playing.
            StartCoroutine(DestroyAfterDelay(clip.length + 1f, callback));
        }

        /// <summary>
        /// Destroys the AudioPackPlayer after the specified duration.
        /// </summary>
        private IEnumerator DestroyAfterDelay( float duration, Action<AudioPackPlayer> callback )
        {
            // Wait for the specified duration.
            yield return new WaitForSeconds(duration);

            // Set the game object to inactive.
            gameObject.SetActive(false);

            // Invoke the callback.
            callback?.Invoke(this);
        }
    }
}
