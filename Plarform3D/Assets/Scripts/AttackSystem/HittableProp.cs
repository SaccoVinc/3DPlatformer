using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HittableProp : MonoBehaviour, Hittable
{

    [SerializeField] float duration = 0.2f;
    [SerializeField] float amount = 0.5f;
    [SerializeField] AudioSource sound;
    Tween pos, rot, scale;

    public void GetHit()
    {
        if (scale == null)
        {
            
            scale = transform.DOShakeScale(duration, amount);
            if (sound != null)
            {
                sound.Play();
            }
        }
        else if ( !scale.IsPlaying()) { 
            scale = transform.DOShakeScale(duration, amount);
            if (sound != null)
            {
                sound.Play();
            }
        }

        
    }
}
