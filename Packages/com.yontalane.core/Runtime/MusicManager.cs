using System.Collections;
using UnityEngine;
using Yontalane;

namespace Yontalane
{
    /// <summary>
    /// Manages persistent background music playback across scenes.
    /// Handles seamless cross-fading, fade-ins/outs, and preserves music state between scene loads.
    /// Use this class to play, stop, and cross-fade background music in your game.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Music Manager")]
    public sealed class MusicManager : Singleton<MusicManager>
    {
        #region Constants

        /// <summary>
        /// Name given to the AudioSource GameObject when stopped.
        /// </summary>
        private const string STOPPED_NAME = "Stopped";

        #endregion

        #region Private Fields
        
        /// <summary>
        /// The volume of all music played through the MusicManager is scaled by this value.
        /// </summary>
        private static float s_globalVolume = 1f;

        /// <summary>
        /// Only play music through the MusicManager if this field is <c>true</c>;
        /// </summary>
        private static bool s_isOn = true;

        /// <summary>
        /// Array holding the audio sources used for playing and cross-fading music.
        /// </summary>
        private readonly AudioSource[] m_audioSources = new AudioSource[2];

        /// <summary>
        /// Index of the currently active audio source.
        /// </summary>
        private int m_currentActive = 0;

        #endregion

        #region Private Properties

        /// <summary>
        /// The currently active AudioSource for playback.
        /// </summary>
        private AudioSource CurrentAudioSource => m_audioSources[m_currentActive];

        /// <summary>
        /// The inactive AudioSource, potentially used for cross-fading and fade-outs.
        /// </summary>
        private AudioSource OtherAudioSource => m_audioSources[1 - m_currentActive];

        #endregion

        #region Public Properties

        /// <summary>
        /// The volume of all music played through the MusicManager is scaled by this value.
        /// </summary>
        public static float GlobalVolume
        {
            get => s_globalVolume;
            set => s_globalVolume = Mathf.Clamp01(value);
        }

        /// <summary>
        /// Only play music through the MusicManager if this field is <c>true</c>;
        /// </summary>
        public static bool IsOn
        {
            get => s_isOn;
            set => s_isOn = value;
        }

        /// <summary>
        /// The MusicSource that initiated the current playback, if any.
        /// </summary>
        public static MusicSource CurrentSource { get; private set; }

        /// <summary>
        /// The currently playing AudioClip, or <c>null</c> if nothing is playing.
        /// </summary>
        public static AudioClip CurrentClip { get; private set; }

        /// <summary>
        /// The name of the currently playing AudioClip, or an empty string.
        /// </summary>
        public static string CurrentClipName => CurrentClip != null ? CurrentClip.name : string.Empty;

        /// <summary>
        /// Returns <c>true</c> if music is currently playing.
        /// </summary>
        public static bool IsPlaying => CurrentClip != null;

        /// <summary>
        /// Returns <c>true</c> if a fade-out is currently in progress (via inactive AudioSource).
        /// </summary>
        public static bool IsFadingOut => Instance != null && Instance.OtherAudioSource != null && Instance.OtherAudioSource.isPlaying;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// Unity Awake callback.
        /// Initializes audio sources and ensures singleton instance.
        /// </summary>
        protected override void Awake()
        {
            // If an instance already exists, destroy this duplicate.
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            // Initialize singleton base.
            base.Awake();

            // Build both AudioSource components if not set yet.
            if (m_audioSources[0] == null)
            {
                m_audioSources[0] = BuildAudioSource();
            }
            if (m_audioSources[1] == null)
            {
                m_audioSources[1] = BuildAudioSource();
            }

            // Ensure this GameObject isn't destroyed on scene load.
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        #region AudioSource Management

        /// <summary>
        /// Utility method to build and configure a new AudioSource as a child of manager.
        /// </summary>
        /// <returns>A configured, stopped AudioSource.</returns>
        private static AudioSource BuildAudioSource()
        {
            // Create new GameObject for the audio source.
            GameObject gameObject = new(STOPPED_NAME);
            gameObject.transform.SetParent(Instance.transform);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localEulerAngles = Vector3.zero;
            gameObject.transform.localScale = Vector3.one;

            // Add and configure the AudioSource component.
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;

            return audioSource;
        }

        /// <summary>
        /// Gets an initialized persistent music manager instance.
        /// Creates one if necessary.
        /// </summary>
        /// <returns>The MusicManager singleton instance.</returns>
        private static MusicManager GetInstance()
        {
            // Return existing instance if possible.
            if (Instance != null)
            {
                return Instance;
            }

            // Otherwise, create a new GameObject with the manager attached.
            GameObject gameObject = new(nameof(MusicManager));
            gameObject.transform.position = Vector3.zero;
            gameObject.transform.localEulerAngles = Vector3.zero;
            gameObject.transform.localScale = Vector3.one;

            MusicManager musicManager = gameObject.AddComponent<MusicManager>();

            // Ensure both AudioSources are created.
            if (musicManager.m_audioSources[0] == null)
            {
                musicManager.m_audioSources[0] = BuildAudioSource();
            }
            if (musicManager.m_audioSources[1] == null)
            {
                musicManager.m_audioSources[1] = BuildAudioSource();
            }

            return musicManager;
        }

        #endregion

        #region Playback API

        /// <summary>
        /// Plays a new AudioClip as music. Can cross-fade and assign source context.
        /// </summary>
        /// <param name="clip">The music clip to play.</param>
        /// <param name="source">The originating MusicSource, or null.</param>
        /// <param name="volumeScale">Volume multiplier for playback.</param>
        /// <param name="fadeIn">Should the clip fade in?</param>
        internal static void Play(AudioClip clip, MusicSource source, float volumeScale = 1f, bool fadeIn = false)
        {
            // Ignore the Play() function if the MusicManager is not on.
            if (!IsOn)
            {
                return;
            }

            // Skip if already playing this clip.
            if (IsPlaying && CurrentClip == clip)
            {
                return;
            }

            // Stop any previous music before starting new playback.
            Stop();

            // If no clip specified, nothing to play.
            if (clip == null)
            {
                return;
            }

            // Get or create instance.
            MusicManager instance = GetInstance();

            // Set up current audio source for playback.
            instance.CurrentAudioSource.name = clip.name;
            instance.CurrentAudioSource.clip = clip;
            CurrentClip = clip;
            CurrentSource = source;

            // Start fade-in coroutine if requested, else play instantly.
            if (fadeIn)
            {
                instance.StartCoroutine(instance.FadeIn(instance.CurrentAudioSource, volumeScale * GlobalVolume));
            }
            else
            {
                instance.CurrentAudioSource.volume = volumeScale * GlobalVolume;
                instance.CurrentAudioSource.Play();
            }
        }

        /// <summary>
        /// Plays a new AudioClip as music, with optional fade-in.
        /// </summary>
        /// <param name="audioPack">The <see cref="AudioPack"/> containing the music clip to play.</param>
        /// <param name="fadeIn">Should the clip fade in?</param>
        public static void Play(AudioPack audioPack, bool fadeIn = false)
        {
            // Exit early if the audio pack is null or does not contain clips.
            if (audioPack == null || audioPack.clips == null || audioPack.clips.Length == 0)
            {
                return;
            }
            
            // Get a random clip from the audio pack.
            AudioClip clip = audioPack.clips[Mathf.FloorToInt(audioPack.clips.Length * Random.value)];
            
            // Delegate to internal Play with null MusicSource.
            Play(clip, null, audioPack.volume, fadeIn);
        }

        /// <summary>
        /// Plays a new AudioClip as music, with optional fade-in.
        /// </summary>
        /// <param name="clip">The music clip to play.</param>
        /// <param name="volumeScale">Volume multiplier for playback.</param>
        /// <param name="fadeIn">Should the clip fade in?</param>
        public static void Play(AudioClip clip, float volumeScale = 1f, bool fadeIn = false)
        {
            // Delegate to internal Play with null MusicSource.
            Play(clip, null, volumeScale, fadeIn);
        }

        /// <summary>
        /// Plays a new AudioClip as music, with optional fade-in.
        /// </summary>
        /// <param name="clip">The music clip to play.</param>
        /// <param name="fadeIn">Should the clip fade in?</param>
        public static void Play(AudioClip clip, bool fadeIn = false)
        {
            // Delegate to internal Play with null MusicSource.
            Play(clip, null, 1f, fadeIn);
        }

        /// <summary>
        /// Stops any currently playing music, with optional fade-out.
        /// </summary>
        /// <param name="fadeOut">Should the music fade out instead of stopping instantly?</param>
        public static void Stop(bool fadeOut = true)
        {
            // If nothing is playing, do nothing.
            if (!IsPlaying)
            {
                return;
            }

            // Clear static playback state.
            CurrentClip = null;
            CurrentSource = null;

            // Access current manager instance.
            MusicManager instance = Instance;

            // If there is no instance, nothing else to do.
            if (instance == null)
            {
                return;
            }

            // Fade out or stop current audio source accordingly.
            if (fadeOut)
            {
                instance.StartCoroutine(instance.FadeOut(instance.CurrentAudioSource));
            }
            else
            {
                instance.CurrentAudioSource.Stop();
                instance.CurrentAudioSource.name = STOPPED_NAME;
                instance.CurrentAudioSource.clip = null;
            }

            // Prepare for next playback by switching the active audio source.
            instance.SwitchActiveAudioSource();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Flips the active AudioSource index for next operation.
        /// Used to alternate between sources for cross-fading.
        /// </summary>
        private void SwitchActiveAudioSource()
        {
            m_currentActive = 1 - m_currentActive;
        }

        #endregion

        #region Fading Coroutines

        /// <summary>
        /// Smoothly fades in an AudioSource to the specified volume.
        /// </summary>
        /// <param name="audioSource">The AudioSource to fade in.</param>
        /// <param name="volumeScale">Target volume after fade.</param>
        private IEnumerator FadeIn(AudioSource audioSource, float volumeScale)
        {
            // Abort if source is null.
            if (audioSource == null)
            {
                yield break;
            }

            // Start at zero volume.
            audioSource.volume = 0f;

            // Fade up to the desired volume.
            yield return Fade(audioSource, volumeScale);
        }

        /// <summary>
        /// Smoothly fades out an AudioSource, then stops and cleans up.
        /// </summary>
        /// <param name="audioSource">The AudioSource to fade out.</param>
        private IEnumerator FadeOut(AudioSource audioSource)
        {
            // Abort if source is null.
            if (audioSource == null)
            {
                yield break;
            }

            // Fade down to zero volume.
            yield return Fade(audioSource, 0f);

            // Stop and clear after fade completes.
            audioSource.Stop();
            audioSource.name = STOPPED_NAME;
            audioSource.clip = null;
        }

        /// <summary>
        /// Smoothly fades an AudioSource volume to a target level over time.
        /// </summary>
        /// <param name="audioSource">The AudioSource to fade.</param>
        /// <param name="targetVolume">The target volume to reach.</param>
        private IEnumerator Fade(AudioSource audioSource, float targetVolume)
        {
            // Abort if source is null.
            if (audioSource == null)
            {
                yield break;
            }

            // Set up fade parameters.
            float startVolume = audioSource.volume;
            const float DURATION = 1f;
            float startTime = Time.time;
            float t = 0f;

            // Ensure playback is active to allow volume adjustment.
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }

            // Lerp the volume to smooth the fade.
            while (t < 1f)
            {
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
                t = (Time.time - startTime) / DURATION;
                yield return new WaitForEndOfFrame();
            }

            // Guarantee final volume is set.
            audioSource.volume = targetVolume;
        }

        #endregion
    }
}