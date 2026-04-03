using UnityEngine;

namespace Yontalane
{
    #region PlayType Enum

    /// <summary>
    /// Specifies how and when the MusicSource should play its audio clip.
    /// Used to control automatic playback and fade-in logic on start.
    /// </summary>
    [System.Serializable]
    internal enum MusicSourcePlayType
    {
        /// <summary>No music will be played automatically.</summary>
        DontPlay = 0,
        /// <summary>Play music on start, regardless of existing music.</summary>
        PlayOnStart = 10,
        /// <summary>Play music on start only if no music is currently playing.</summary>
        PlayOnStartIfNoMusicIsPlaying = 20,
        /// <summary>Fade in music on start, regardless of existing music.</summary>
        FadeInOnStart = 100,
        /// <summary>Fade in music on start only if no music is currently playing.</summary>
        FadeInOnStartIfNoMusicIsPlaying = 110,
        /// <summary>Fade in music on start only if music is playing or fading out. Otherwise, play immediately.</summary>
        FadeInOnStartIfMusicIsPlayingOtherwiseStartImmediately = 150,
    }

    #endregion

    /// <summary>
    /// Component that acts as a persistent music source in the scene.
    /// Configurable to determine if and how to play audio via a MusicManager, 
    /// including fade-in, conditional logic, and stopping on destroy.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Music Source")]
    public class MusicSource : MonoBehaviour
    {
        #region Serialized Fields

        [Tooltip("The AudioClip assigned to this persistent music source.")]
        [SerializeField]
        private AudioClip m_clip;

        [Tooltip("The playback volume for this music source (0.0 to 1.0).")]
        [SerializeField]
        [Range(0f, 1f)]
        private float m_volume = 1f;

        [Tooltip("Specifies when and how the music should play when this GameObject starts.")]
        [SerializeField]
        private MusicSourcePlayType m_playSetting = MusicSourcePlayType.PlayOnStart;

        [Space]

        [Tooltip("If true, the music will be stopped when this source is destroyed and it is currently the active source.")]
        [SerializeField]
        private bool m_stopOnDestroy = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the AudioClip associated with this music source.
        /// </summary>
        public AudioClip Clip => m_clip;

        #endregion

        #region Unity Callbacks

        /// <summary>
        /// Unity Start callback. Determines behavior based on <see cref="m_playSetting"/> and calls the appropriate
        /// music manager function to play or fade in music as needed.
        /// </summary>
        private void Start()
        {
            // Evaluate the play setting to decide which playback logic to execute on start
            switch (m_playSetting)
            {
                // Play or fade in immediately, or if no music is currently playing
                case MusicSourcePlayType.PlayOnStart:
                case MusicSourcePlayType.PlayOnStartIfNoMusicIsPlaying when !MusicManager.IsPlaying:
                case MusicSourcePlayType.FadeInOnStart:
                case MusicSourcePlayType.FadeInOnStartIfNoMusicIsPlaying when !MusicManager.IsPlaying:
                    Play();
                    break;

                // Fade in only if music is already playing or fading out
                case MusicSourcePlayType.FadeInOnStartIfMusicIsPlayingOtherwiseStartImmediately when MusicManager.IsPlaying || MusicManager.IsFadingOut:
                    MusicManager.Play(m_clip, this, m_volume, true);
                    break;

                // Play without fade in if music is not currently playing (fallback case)
                case MusicSourcePlayType.FadeInOnStartIfMusicIsPlayingOtherwiseStartImmediately:
                    MusicManager.Play(m_clip, this, m_volume, false);
                    break;
            }
        }

        /// <summary>
        /// Unity OnDestroy callback. Optionally stops music if this is the active music source 
        /// and <see cref="m_stopOnDestroy"/> is true.
        /// </summary>
        private void OnDestroy()
        {
            // Only proceed if stopping on destroy is enabled
            if (!m_stopOnDestroy)
            {
                return;
            }

            // Only stop if this component is the current music source
            if (MusicManager.CurrentSource != this)
            {
                return;
            }

            // Request the music manager to stop playback
            MusicManager.Stop();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Plays the assigned AudioClip via the music manager, using fade-in if indicated by the play setting.
        /// </summary>
        public void Play()
        {
            // Determine if fade-in should be used based on the selected play type
            bool fadeIn = m_playSetting == MusicSourcePlayType.FadeInOnStart 
                || m_playSetting == MusicSourcePlayType.FadeInOnStartIfNoMusicIsPlaying;
            MusicManager.Play(m_clip, this, m_volume, fadeIn);
        }

        #endregion
    }
}
