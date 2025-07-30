using UnityEngine;

public class SoundTrigger : MonoBehaviour
{
    [SerializeField] private bool use3DSound = true;
    [SerializeField] private Transform soundOrigin;
    [SerializeField] private float defaultVolumeMultiplier = 1f;

    void Awake()
    {
        if (soundOrigin == null)
            soundOrigin = transform;
    }

    public void PlayStep()
    {
        PlaySound(SoundType.Step);
    }

    public void PlayJump()
    {
        PlaySound(SoundType.Jump);
    }

    public void PlayLand()
    {
        PlaySound(SoundType.Land);
    }

    public void PlayPunch()
    {
        PlaySound(SoundType.Punch);
    }

    public void PlaySlide()
    {
        PlaySound(SoundType.Slide);
    }

    public void PlayStepWithVolume(float volume)
    {
        PlaySound(SoundType.Step, volume);
    }

    public void PlayJumpWithVolume(float volume)
    {
        PlaySound(SoundType.Jump, volume);
    }

    public void PlayLandWithVolume(float volume)
    {
        PlaySound(SoundType.Land, volume);
    }

    public void PlayPunchWithVolume(float volume)
    {
        PlaySound(SoundType.Punch, volume);
    }

    public void PlaySlideWithVolume(float volume)
    {
        PlaySound(SoundType.Slide, volume);
    }

    public void PlaySound(SoundType soundType)
    {
        PlaySound(soundType, defaultVolumeMultiplier);
    }

    public void PlaySound(SoundType soundType, float volumeMultiplier)
    {
        if (use3DSound)
        {
            SoundManager.PlaySound3D(soundType, soundOrigin.position, volumeMultiplier);
        }
        else
        {
            SoundManager.PlaySound(soundType, volumeMultiplier);
        }
    }

    public void PlayCustomSound(string soundTypeName)
    {
        if (System.Enum.TryParse<SoundType>(soundTypeName, true, out SoundType soundType))
        {
            PlaySound(soundType);
        }
        else
        {
            Debug.LogWarning($"SoundType '{soundTypeName}' not found on {gameObject.name}");
        }
    }

    public void PlayCustomSoundWithVolume(string soundTypeAndVolume)
    {
        string[] parts = soundTypeAndVolume.Split(',');
        if (parts.Length >= 1)
        {
            string soundTypeName = parts[0].Trim();
            float volume = parts.Length > 1 && float.TryParse(parts[1].Trim(), out float vol) ? vol : defaultVolumeMultiplier;

            if (System.Enum.TryParse<SoundType>(soundTypeName, true, out SoundType soundType))
            {
                PlaySound(soundType, volume);
            }
            else
            {
                Debug.LogWarning($"SoundType '{soundTypeName}' not found on {gameObject.name}");
            }
        }
    }
}