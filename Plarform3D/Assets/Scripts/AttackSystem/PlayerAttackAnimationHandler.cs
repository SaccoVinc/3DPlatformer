using UnityEngine;

public class PlayerAttackAnimationHandler : MonoBehaviour
{
    [SerializeField] float attackRadius = 1.5f;
    [SerializeField] float attackDistance = 1.0f;
    [SerializeField] LayerMask hittableLayer;

    bool isAttacking = false;

    public void StartAttack()
    {
        isAttacking = true;
        PerformAttack();
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    void Update()
    {
        if (isAttacking)
        {
            PerformAttack();
        }
    }

    void PerformAttack()
    {
        Vector3 attackPoint = transform.position + transform.forward * attackDistance;
        Collider[] hits = Physics.OverlapSphere(attackPoint, attackRadius, ~0);

        foreach (var hit in hits)
        {
            Hittable hittable = hit.GetComponent<Hittable>();
            if (hittable != null)
            {
                Debug.Log("Found");
                hittable.GetHit();
            }
        }
    }

    
    void OnDrawGizmos()
    {
        if (isAttacking)
        {
            Vector3 attackPoint = transform.position + transform.forward * attackDistance;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint, attackRadius);
        }
    }
}
