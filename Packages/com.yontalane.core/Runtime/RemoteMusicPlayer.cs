using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Yontalane
{
    /// <summary>
    /// A struct representing a track for the RemoteMusicPlayer.
    /// </summary>
    [System.Serializable]
    public struct RemoteMusicPlayerTrack
    {
        [Tooltip("The URL of the audio file to play.")]
        public string url;

        [Tooltip("The volume of the audio file.")]
        [Range(0f, 1f)]
        public float volume;

        [Tooltip("Should the track loop when finished?")]
        public bool loop;

        [Tooltip("The AudioSource to play this track. If unnassigned, a new AudioSource will be created with default settings.")]
        public AudioSource audioSource;
    }

    /// <summary>
    /// A component that plays music from a remote source.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Remote Music Player")]
    public class RemoteMusicPlayer : MonoBehaviour
    {
        /// <summary>
        /// A list of runtime-created AudioSources used to play the music.
        /// </summary>
        private readonly List<AudioSource> m_tempAudioSources = new();

        /// <summary>
        /// A list of pre-existing AudioSources used to play the music.
        /// </summary>
        private readonly List<AudioSource> m_preExistingAudioSources = new();


        [Tooltip("The list of tracks to be played by the RemoteMusicPlayer.")]
        [SerializeField]
        private RemoteMusicPlayerTrack[] m_tracks;

        [Tooltip("If true, music will automatically start playing on Start().")]
        [SerializeField]
        private bool m_autoPlay = true;


        /// <summary>
        /// Gets the volume of the music.
        /// </summary>
        public virtual float MusicVolume => 1f;

        /// <summary>
        /// Gets whether the music is on.
        /// </summary>
        public virtual bool MusicOn => true;


        protected virtual void Reset()
        {
            m_tracks = new RemoteMusicPlayerTrack[0];
            m_autoPlay = true;
        }


        protected virtual void Start()
        {
            if (m_autoPlay)
            {
                Play();
            }
        }

        /// <summary>
        /// Stops all music playback.
        /// </summary>
        public void Stop()
        {
            // Stop and destroy all temporary AudioSources created at runtime.
            for (int i = m_tempAudioSources.Count - 1; i >= 0; i--)
            {
                m_tempAudioSources[i].Stop();
                Destroy(m_tempAudioSources[i].gameObject);
            }

            // Stop all pre-existing AudioSources used for music playback.
            for (int i = m_preExistingAudioSources.Count - 1; i >= 0; i--)
            {
                m_preExistingAudioSources[i].Stop();
            }

            // Clear the lists of AudioSources.
            m_tempAudioSources.Clear();
            m_preExistingAudioSources.Clear();
        }

        /// <summary>
        /// Plays the music.
        /// </summary>
        public void Play()
        {
            // Stop any currently playing music before starting new playback.
            Stop();

            // If music is not enabled, exit early and do not play anything.
            if (!MusicOn)
            {
                return;
            }

            // Start playing each track asynchronously using coroutines.
            foreach (RemoteMusicPlayerTrack track in m_tracks)
            {
                StartCoroutine(PlayMusic(track));
            }
        }

        /// <summary>
        /// Plays a single track.
        /// </summary>
        private IEnumerator PlayMusic(RemoteMusicPlayerTrack track)
        {
            // If the track URL is null or empty, exit the coroutine early.
            if (string.IsNullOrEmpty(track.url))
            {
                yield break;
            }

            // Download the audio clip from the URL.
            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(track.url, AudioType.MPEG);

            // Wait for the download to complete.
            yield return www.SendWebRequest();

            // If the download failed, log the error.
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogWarning(www.error);
            }
            else
            {
                // Create a new GameObject to play the music.
                GameObject child = new() { name = "Music Player" };
                child.transform.SetParent(transform);
                child.transform.localPosition = Vector3.zero;
                child.transform.localEulerAngles = Vector3.zero;
                child.transform.localScale = Vector3.one;

                AudioSource audioSource;

                // If the track already has an assigned AudioSource, use it and add it to the list of pre-existing audio sources.
                if (track.audioSource != null)
                {
                    audioSource = track.audioSource;
                    m_preExistingAudioSources.Add(audioSource);
                }
                // If the track does not have an assigned AudioSource, create a new one.
                else
                {
                    // Add an AudioSource component to the GameObject.
                    audioSource = child.AddComponent<AudioSource>();

                    // Add the AudioSource to the list of audio sources.
                    m_tempAudioSources.Add(audioSource);
                }

                // Get the audio clip from the download.
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);

                // Set the audio clip to the AudioSource.
                audioSource.clip = clip;

                // Set the loop property of the AudioSource.
                audioSource.loop = track.loop;

                // Set the volume of the AudioSource.
                audioSource.volume = track.volume * Mathf.Clamp01(MusicVolume);

                // Play the audio clip.
                audioSource.Play();
            }
        }
    }
}
