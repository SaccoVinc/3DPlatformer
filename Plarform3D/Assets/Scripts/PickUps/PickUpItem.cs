using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    [Header("Abilita rotazione su ogni asse")]
    public bool ruotaX = true;
    public bool ruotaY = true;
    public bool ruotaZ = true;

    [Header("Velocità di rotazione (gradi/sec)")]
    public float speedX = 30f;
    public float speedY = 45f;
    public float speedZ = 60f;

    public Space relative = Space.Self;

    void Update()
    {
        
        float rotX = ruotaX ? speedX * Time.deltaTime : 0f;
        float rotY = ruotaY ? speedY * Time.deltaTime : 0f;
        float rotZ = ruotaZ ? speedZ * Time.deltaTime : 0f;

        transform.Rotate(rotX, rotY, rotZ, relative);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            OnPickUp(other);
        }
    }

    public virtual void OnPickUp(Collider player)
    {
        Destroy(gameObject);
    }
}
