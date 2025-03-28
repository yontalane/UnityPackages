using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Yontalane
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Remote Music Player")]
    public sealed class RemoteMusicPlayer : MonoBehaviour
    {
        [SerializeField]
        private string[] m_musicUrls;

        private void Reset()
        {
            m_musicUrls = new string[0];
        }

        private void Start()
        {
            foreach (string url in m_musicUrls)
            {
                StartCoroutine(PlayMusic(url));
            }
        }

        private IEnumerator PlayMusic(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                yield break;
            }

            GameObject child = new() { name = "Music Player" };
            child.transform.SetParent(transform);
            child.transform.localPosition = Vector3.zero;
            child.transform.localEulerAngles = Vector3.zero;
            child.transform.localScale = Vector3.one;
            AudioSource audioSource = child.AddComponent<AudioSource>();

            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogWarning(www.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
    }
}
