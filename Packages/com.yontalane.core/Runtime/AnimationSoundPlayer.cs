using UnityEngine;

namespace Yontalane
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Yontalane/Animation Sound Player")]
    public class AnimationSoundPlayer : MonoBehaviour
    {
        public void Play(AnimationEvent animationEvent)
        {
            if (animationEvent.objectReferenceParameter == null)
            {
                return;
            }

            if ( animationEvent.objectReferenceParameter is AudioClip clip )
            {
                float volume = animationEvent.floatParameter > 0f ? animationEvent.floatParameter : 1f;
                PlayClip(clip, volume);
            }
            else if ( animationEvent.objectReferenceParameter is AudioPack pack )
            {
                PlayPack(pack);
            }
        }

        private void PlayClip( AudioClip clip, float volume )
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, volume);
        }

        private void PlayPack( AudioPack pack )
        {
            AudioPack.Play(pack);
        }
    }
}
