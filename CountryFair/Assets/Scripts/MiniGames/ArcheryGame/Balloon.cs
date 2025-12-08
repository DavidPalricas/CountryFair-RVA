using UnityEngine;

public class BalloonScript : MonoBehaviour
{
    [Header("Explosion Effect (optional)")]
    public GameObject popEffect;

    private bool popped = false;

    private Renderer balloonRenderer;
    private Color balloonColor;


    private void Awake()
    {
        // Apanha o renderer do balão
        balloonRenderer = GetComponent<Renderer>();

        // Guarda a cor principal do material
        if (balloonRenderer != null && balloonRenderer.material.HasProperty("_Color"))
        {
            balloonColor = balloonRenderer.material.color;
        }
        else
        {
            balloonColor = Color.white; // fallback
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (popped) return;

        if (collision.gameObject.CompareTag("Arrow"))
        {
            Pop();
        }
    }

    private void Pop()
    {
        popped = true;

        if (popEffect != null)
        {
            // Instancia o efeito
            GameObject fx = Instantiate(popEffect, transform.position, Quaternion.identity);

            // Apanha o ParticleSystem do efeito
            ParticleSystem ps = fx.GetComponent<ParticleSystem>();

            if (ps != null)
            {
                var main = ps.main;
                main.startColor = balloonColor;    // define a cor no particle system
            }
        }

        ArcheryGameManager.Instance.SetScore(1);

        Destroy(gameObject);
    }
}
