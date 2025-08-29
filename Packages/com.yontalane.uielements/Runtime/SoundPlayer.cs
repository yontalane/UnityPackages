using System.Collections;
using UnityEngine;

namespace Yontalane.UIElements
{
    /// <summary>
    /// Provides static methods for playing audio clips in the UI system.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/UI Elements/Sound Player")]
    public static class SoundPlayer
    {
        /// <summary>
        /// Plays the specified AudioClip using a temporary persistent audio player.
        /// </summary>
        /// <param name="clip">The AudioClip to play. If null, nothing happens.</param>
        public static void Play(AudioClip clip)
        {
            // If the provided AudioClip is null, do not attempt to play anything.
            if (clip == null)
            {
                return;
            }

            // Create a new GameObject and add the PersistentAudioPlayer component to it
            PersistentAudioPlayer player = new GameObject().AddComponent<PersistentAudioPlayer>();

            // Set the player's transform to match the main camera's position and reset rotation/scale.
            player.transform.position = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
            player.transform.localEulerAngles = Vector3.zero;
            player.transform.localScale = Vector3.one;

            // Initialize the player with the provided AudioClip
            player.Initialize(clip);
        }
    }


    /// <summary>
    /// PersistentAudioPlayer is a helper MonoBehaviour that plays an AudioClip and destroys itself after playback.
    /// It is used to ensure audio persists across scene loads and is not interrupted.
    /// </summary>
    internal class PersistentAudioPlayer : MonoBehaviour
    {
        /// <summary>
        /// Initializes the PersistentAudioPlayer with the specified AudioClip, sets up the AudioSource,
        /// ensures the GameObject persists across scene loads, and schedules its destruction after playback.
        /// </summary>
        /// <param name="clip">The AudioClip to play.</param>
        public void Initialize(AudioClip clip)
        {
            // Set the GameObject's name to indicate which clip is being played
            name = $"{clip.name} Player";

            // Make this GameObject persist across scene loads
            DontDestroyOnLoad(gameObject);

            // Add an AudioSource component and configure it to play the provided clip
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.clip = clip;
            audioSource.loop = false;
            audioSource.Play();

            // Start a coroutine to destroy this GameObject after the clip finishes playing
            StartCoroutine(DestroyAfterPlaying(clip.length + 2f));
        }

        /// <summary>
        /// Waits for the specified duration, then destroys the GameObject.
        /// </summary>
        /// <param name="duration">The time in seconds to wait before destroying the GameObject.</param>
        private IEnumerator DestroyAfterPlaying(float duration)
        {
            yield return new WaitForSeconds(duration);
            Destroy(gameObject);
        }
    }
}
