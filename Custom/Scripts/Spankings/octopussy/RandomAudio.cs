
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace octopussy
{
    /*
    RandomAudio
    by Doc0ctopuss

    PlayRandom a random audio in a list of clips
    or loop the sequence

    next and pitch are prepared after the audio call
    */
    public class RandomAudio
    {
        int previous = -1;
        int next = 0;

        float _pitchlow = -0.07f;
        float _pitchhigh = +0.07f;
        float pitchlow;
        float pitchhigh;

        public bool playNow = false;
        public static float playNowWaitTimer = 0;

        public float pitchshift
        {
            set
            {
                pitchlow = 1.0f + value - _pitchlow;
                pitchhigh = 1.0f + value + _pitchhigh;
            }
        }

        List<NamedAudioClip> audioClips;

        public RandomAudio(List<NamedAudioClip> clips, float _shift = 0f)
        {
            audioClips = clips;
            pitchshift = _shift;
        }

        public void playRandomDelayedIfClear(AudioSource audioSource, float volume = 1.0f, float minDelay = 0, float maxDelay = 1)
        {
            playDelayedIfClear(audioSource, volume, UnityEngine.Random.Range(minDelay, maxDelay));
        }

        public void playDelayedIfClear(AudioSource audioSource, float volume = 1.0f, float delay = 0)
        {
            NamedAudioClip nac = audioClips[next];

            if (audioSource != null && nac.clipToPlay != null)
            {
                if ((playNow && Time.timeSinceLevelLoad >= playNowWaitTimer) || !audioSource.isPlaying)
                {
                    playNowWaitTimer = Time.timeSinceLevelLoad + nac.clipToPlay.length;
                    audioSource.clip = nac.clipToPlay;
                    audioSource.volume = volume;
                    audioSource.pitch = UnityEngine.Random.Range(pitchlow, pitchhigh);
                    audioSource.PlayDelayed(delay);
                    previous = next;
                    next = (int)UnityEngine.Random.Range(0f, (float)audioClips.Count - 1);
                    // take next if its the same again
                    if (next == previous) next = (next + 1) % audioClips.Count;
                }
            }
        }


        public void playRandom(AudioSource audioSource, float volume = 1.0f, float delay = 0)
        {
            NamedAudioClip nac = audioClips[next];
            if (audioSource != null && nac.clipToPlay != null)
            {
                audioSource.clip = audioClips[next].clipToPlay;
                audioSource.volume = volume;
                audioSource.pitch = UnityEngine.Random.Range(pitchlow, pitchhigh);
                audioSource.PlayDelayed(delay);

                previous = next;
                next = (int)UnityEngine.Random.Range(0f, (float)audioClips.Count - 1);
                // take next if its the same again
                if (next == previous) next = (next + 1) % audioClips.Count;
            }
        }

        public void playNext(AudioSource audioSource, float volume = 1.0f)
        {
            NamedAudioClip nac = audioClips[next];
            audioSource.clip = nac.clipToPlay;
            audioSource.volume = volume;
            audioSource.pitch = UnityEngine.Random.Range(pitchlow, pitchhigh);
            next = (next + 1) % audioClips.Count;
            audioSource.Play();
        }
    }
}
