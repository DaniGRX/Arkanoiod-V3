using UnityEngine;

public class DeathZone : MonoBehaviour
{
    GameManager gm;
    bool isProcessing = false; // ← evita procesar dos colisiones a la vez

    void Awake()
    {
        gm = FindFirstObjectByType<GameManager>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        BallController ball = other.GetComponent<BallController>();

        if (ball == null) return;

        // Contamos las bolas ANTES de destruir ninguna
        BallController[] balls = FindObjectsByType<BallController>(FindObjectsSortMode.None);

        if (balls.Length > 1)
        {
            // Hay más de una bola: destruimos esta sin avisar al GameManager
            Destroy(other.gameObject);
        }
        else
        {
            // Es la última bola pero evitamos procesarla dos veces
            if (isProcessing) return;
            isProcessing = true;

            gm.LoseLife();

            // Reseteamos el flag tras un frame
            Invoke(nameof(ResetProcessing), 0.1f);
        }
    }

    void ResetProcessing()
    {
        isProcessing = false;
    }
}