using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyStatusAudio : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip alertVisualClip;    // Vista (!)
    public AudioClip alertAudioClip;     // Udito (?)
    public AudioClip searchingClip;
    public AudioClip sleepingClip;
    public AudioClip distractedClip;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound (world space)
    }

    public void PlayDetectionSound(bool isAudio)
    {
        if (isAudio && alertAudioClip != null)
            audioSource.PlayOneShot(alertAudioClip);
        else if (!isAudio && alertVisualClip != null)
            audioSource.PlayOneShot(alertVisualClip);
    }

    public void PlayStateSound(STATE state)
    {
        AudioClip clipToPlay = null;

        switch (state)
        {
            case STATE.SEARCHING:
                clipToPlay = searchingClip;
                break;

            case STATE.SLEEPING:
                clipToPlay = sleepingClip;
                break;

            case STATE.DISTRACTED:
                clipToPlay = distractedClip;
                break;
        }

        if (clipToPlay != null)
            audioSource.PlayOneShot(clipToPlay);
    }
}
