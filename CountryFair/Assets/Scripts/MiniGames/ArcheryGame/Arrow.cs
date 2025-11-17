using UnityEngine;

public class Arrow : MonoBehaviour
{
    Rigidbody rb;
    bool launched = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Launch(Vector3 direction, float force)
    {
        launched = true;
        rb.useGravity = true;
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider col)
    {
        if (!launched) return; // evita bugs enquanto ainda está no arco

        // --- SE BATER NO BALÃO ---
        if (col.gameObject.CompareTag("Balloon"))
        {
            Destroy(gameObject); 
            return;
        }

        // --- SE BATER NO CHÃO ---
        if (col.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
            return;
        }

        // --- SE BATER NOUTRO OBJETO (ALVO, MADEIRA, ETC.) ---
        rb.isKinematic = true;
        rb.useGravity = false;
        transform.parent = col.transform; // seta fica presa
    }
}
