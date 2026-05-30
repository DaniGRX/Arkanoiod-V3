using UnityEngine;

public class DeathZone : MonoBehaviour
{
    // Referencia al GameManager para poder acceder
    // al sistema de vidas y control de la partida
    GameManager gm;

    void Awake()
    {
        // Busco automáticamente el GameManager
        // existente en la escena al iniciar el objeto
        gm = FindFirstObjectByType<GameManager>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Compruebo si el objeto que ha entrado
        // en la zona de muerte es una pelota
        BallController ball = other.GetComponent<BallController>();

        if (ball != null)
        {
            // Siempre avisamos al GameManager
            // Él decide si restar vida o no según las bolas restantes
            gm.LoseLife();

            // Si hay más de una bola simplemente destruimos esta
            // sin resetear ni restar vida (lo gestiona GameManager)
            BallController[] balls = FindObjectsByType<BallController>(FindObjectsSortMode.None);
            if (balls.Length > 1)
                Destroy(other.gameObject);
        }
    }
}