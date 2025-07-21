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
    }

    /// <summary>
    /// A component that plays music from a remote source.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Remote Music Player")]
    public class RemoteMusicPlayer : MonoBehaviour
    {
        /// <summary>
        /// A list of audio sources used to play the music.
        /// </summary>
        private readonly List<AudioSource> m_audioSources = new();


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
            for (int i = m_audioSources.Count - 1; i >= 0; i--)
            {
                m_audioSources[i].Stop();
                Destroy(m_audioSources[i].gameObject);
            }

            m_audioSources.Clear();
        }

        /// <summary>
        /// Plays the music.
        /// </summary>
        public void Play()
        {
            Stop();

            if (!MusicOn)
            {
                return;
            }

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
            if (string.IsNullOrEmpty(track.url))
            {
                yield break;
            }

            // Create a new GameObject to play the music.
            GameObject child = new() { name = "Music Player" };
            child.transform.SetParent(transform);
            child.transform.localPosition = Vector3.zero;
            child.transform.localEulerAngles = Vector3.zero;
            child.transform.localScale = Vector3.one;

            // Add an AudioSource component to the GameObject.
            AudioSource audioSource = child.AddComponent<AudioSource>();

            // Add the AudioSource to the list of audio sources.
            m_audioSources.Add(audioSource);

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
