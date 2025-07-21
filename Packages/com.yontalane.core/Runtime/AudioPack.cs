using System.Collections.Generic;
using UnityEngine;

namespace Yontalane
{
    /// <summary>
    /// A scriptable object that contains a list of audio clips and a volume.
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(menuName = "Yontalane/AudioPack", fileName = "AudioPack")]
    public class AudioPack : ScriptableObject
    {
        private static readonly List<AudioPackPlayer> s_players = new();
        private static GameObject s_container = null;

        [Tooltip("The list of audio clips to play.")]
        public AudioClip[] clips = new AudioClip[0];

        [Tooltip("The volume of the audio clips.")]
        [Range(0f, 1f)]
        public float volume = 1f;

        /// <summary>
        /// Checks if the audio pack can play.
        /// </summary>
        public bool CanPlay()
        {
            return clips != null && clips.Length > 0;
        }

        /// <summary>
        /// Tries to play the audio pack.
        /// </summary>
        public static bool TryPlay( AudioPack audio )
        {
            // Return false if the audio pack is null.
            if ( audio == null )
            {
                return false;
            }

            // Return false if the audio pack cannot play.
            if ( !audio.CanPlay() )
            {
                return false;
            }

            // Play the audio pack.
            audio.Play();
            return true;
        }

        /// <summary>
        /// Plays the audio pack.
        /// </summary>
        public static void Play( AudioPack audio )
        {
            _ = TryPlay(audio);
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        public static void Play( AudioClip clip, float volume = 1f )
        {
            if ( clip == null )
            {
                return;
            }

            //AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, volume);
            PlayClipFromPlayer(clip, volume);
        }

        /// <summary>
        /// Plays the audio clip at the specified index.
        /// </summary>
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

        /// <summary>
        /// Plays a random audio clip from the pack.
        /// </summary>
        public void Play()
        {
            Play(Mathf.FloorToInt(clips.Length * Random.value));
        }

        /// <summary>
        /// Plays the audio clip from the player.
        /// </summary>
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

        /// <summary>
        /// Gets or constructs the audio pack player.
        /// </summary>
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

        /// <summary>
        /// Gets or constructs the audio pack container.
        /// </summary>
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
