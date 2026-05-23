using UnityEngine; // Librería principal de Unity

public class DeathZone : MonoBehaviour
{
    // Variable donde guardaré la referencia al GameManager
    GameManager gm;

    void Awake()
    {
        // Busco automáticamente el GameManager de la escena
        // Así puedo acceder a sus funciones desde este script
        gm = FindFirstObjectByType<GameManager>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Compruebo si el objeto que ha entrado tiene BallController
        // Esto sirve para detectar específicamente la bola
        if (other.GetComponent<BallController>() != null)
        {
            // Le aviso al GameManager de que el jugador ha perdido una vida
            gm.LoseLife();
        }
    }
}