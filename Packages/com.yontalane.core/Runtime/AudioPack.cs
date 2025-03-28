using System.Collections.Generic;
using UnityEngine;

namespace Yontalane
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Yontalane/AudioPack", fileName = "AudioPack")]
    public class AudioPack : ScriptableObject
    {
        private static readonly List<AudioPackPlayer> s_players = new();
        private static GameObject s_container = null;

        public AudioClip[] clips = new AudioClip[0];

        [Range(0f, 1f)]
        public float volume = 1f;

        public bool CanPlay()
        {
            return clips != null && clips.Length > 0;
        }

        public static bool TryPlay( AudioPack audio )
        {
            if ( audio == null )
            {
                return false;
            }

            if ( !audio.CanPlay() )
            {
                return false;
            }

            audio.Play();
            return true;
        }

        public static void Play( AudioPack audio )
        {
            _ = TryPlay(audio);
        }

        public static void Play( AudioClip clip, float volume = 1f )
        {
            if ( clip == null )
            {
                return;
            }

            //AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, volume);
            PlayClipFromPlayer(clip, volume);
        }

        public void Play( int index )
        {
            if ( !CanPlay() || index < 0 || index >= clips.Length )
            {
                return;
            }

            AudioClip clip = clips[index];

            //AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, volume);
            PlayClipFromPlayer(clip, volume);
        }

        public void Play()
        {
            Play(Mathf.FloorToInt(clips.Length * Random.value));
        }

        private static void PlayClipFromPlayer( AudioClip clip, float volume )
        {
            if ( clip == null )
            {
                return;
            }

            AudioPackPlayer player = GetOrConstructPlayer();

            player.transform.position = Camera.main.transform.position;

            player.Initiate(clip, volume, ( AudioPackPlayer returnedPlayer ) =>
            {
                s_players.Add(returnedPlayer);
            });
        }

        private static AudioPackPlayer GetOrConstructPlayer()
        {
            while ( s_players.Count > 0 )
            {
                if ( s_players[0] == null )
                {
                    s_players.RemoveAt(0);
                    continue;
                }

                AudioPackPlayer player = s_players[0];
                s_players.RemoveAt(0);
                return player;
            }

            {
                GameObject gameObject = new()
                {
                    name = "Audio Pack Player"
                };

                DontDestroyOnLoad(gameObject);

                gameObject.transform.SetParent(GetOrConstructContainer().transform);

                AudioPackPlayer player = gameObject.AddComponent<AudioPackPlayer>();

                AudioSource audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.loop = false;
                audioSource.playOnAwake = false;

                return player;
            }
        }

        private static GameObject GetOrConstructContainer()
        {
            if ( s_container == null )
            {
                s_container = new()
                {
                    name = "Audio Pack Players",
                    isStatic = true,
                };

                DontDestroyOnLoad(s_container);
            }

            return s_container;
        }
    }
}
