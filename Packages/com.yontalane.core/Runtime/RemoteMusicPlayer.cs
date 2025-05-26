using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Yontalane
{
    [System.Serializable]
    public struct RemoteMusicPlayerTrack
    {
        public string url;

        [Range(0f, 1f)]
        public float volume;

        public bool loop;
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Remote Music Player")]
    public class RemoteMusicPlayer : MonoBehaviour
    {
        private readonly List<AudioSource> m_audioSources = new();


        [SerializeField]
        private RemoteMusicPlayerTrack[] m_tracks;

        [SerializeField]
        private bool m_autoPlay = true;


        public virtual float MusicVolume => 1f;

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

        public void Stop()
        {
            for (int i = m_audioSources.Count - 1; i >= 0; i--)
            {
                m_audioSources[i].Stop();
                Destroy(m_audioSources[i].gameObject);
            }

            m_audioSources.Clear();
        }

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

        private IEnumerator PlayMusic(RemoteMusicPlayerTrack track)
        {
            if (string.IsNullOrEmpty(track.url))
            {
                yield break;
            }

            GameObject child = new() { name = "Music Player" };
            child.transform.SetParent(transform);
            child.transform.localPosition = Vector3.zero;
            child.transform.localEulerAngles = Vector3.zero;
            child.transform.localScale = Vector3.one;
            AudioSource audioSource = child.AddComponent<AudioSource>();

            m_audioSources.Add(audioSource);

            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(track.url, AudioType.MPEG);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogWarning(www.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.loop = track.loop;
                audioSource.volume = track.volume * Mathf.Clamp01(MusicVolume);
                audioSource.Play();
            }
        }
    }
}
