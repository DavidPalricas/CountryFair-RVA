using UnityEngine;

public class BalloonScript : MonoBehaviour
{
    [Header("Explosion Effect (optional)")]
    public GameObject popEffect;

    private bool popped = false;   // evita múltiplos pops

    private void OnTriggerEnter(Collider collision)
    {
        if (popped) return; // já explodiu → ignora

        if (collision.gameObject.CompareTag("Arrow"))
        {
            Pop();
        }
    }

    private void Pop()
    {
        popped = true; // marca como já explodido

        if (popEffect != null)
            Instantiate(popEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
