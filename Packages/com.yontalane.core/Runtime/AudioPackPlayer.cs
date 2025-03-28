using System;
using System.Collections;
using UnityEngine;

namespace Yontalane
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/AudioPack Player")]
    public class AudioPackPlayer : MonoBehaviour
    {
        public void Initiate( AudioClip clip, float volume, Action<AudioPackPlayer> callback )
        {
            AudioSource audioSource = GetComponent<AudioSource>();

            gameObject.SetActive(true);
            audioSource.volume = volume;
            audioSource.clip = clip;
            audioSource.Play();

            StartCoroutine(DestroyAfterDelay(clip.length + 1f, callback));
        }

        private IEnumerator DestroyAfterDelay( float duration, Action<AudioPackPlayer> callback )
        {
            yield return new WaitForSeconds(duration);
            gameObject.SetActive(false);
            callback?.Invoke(this);
        }
    }
}
