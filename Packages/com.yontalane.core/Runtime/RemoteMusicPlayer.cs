using System.Collections;
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
    public sealed class RemoteMusicPlayer : MonoBehaviour
    {
        [SerializeField]
        private RemoteMusicPlayerTrack[] m_tracks;

        private void Reset()
        {
            m_tracks = new RemoteMusicPlayerTrack[0];
        }

        private void Start()
        {
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
                audioSource.volume = track.volume;
                audioSource.Play();
            }
        }
    }
}
