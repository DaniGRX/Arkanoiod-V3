using UnityEngine;      // Librería principal de Unity

public class PowerUp : MonoBehaviour
{
    // ── Tipos de Power-Up disponibles ────────────────────

    // Enum que define todos los power-ups posibles del juego
    public enum PowerUpType { PaddleGrow, PaddleShrink, MultiBall }

    // Tipo de power-up que representa este objeto
    [SerializeField] PowerUpType type;

    // Velocidad de caída del power-up
    [SerializeField] float fallSpeed = 3f;

    // Referencia al Rigidbody2D para controlar el movimiento
    Rigidbody2D rb;

    // Propiedad pública para permitir consultar el tipo
    // de power-up desde otros scripts
    public PowerUpType Type => type;

    void Awake()
    {
        // Obtengo automáticamente el Rigidbody2D
        // asociado a este objeto
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Aplico una velocidad constante hacia abajo
        // para simular la caída del power-up
        rb.linearVelocity = Vector2.down * fallSpeed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Compruebo si el objeto que ha colisionado
        // es la pala del jugador
        if (other.CompareTag("Paddle"))
        {
            // Busco el PowerUpManager existente en la escena
            PowerUpManager pum = FindFirstObjectByType<PowerUpManager>();

            // Si existe, activo el efecto correspondiente
            // al tipo de power-up recogido
            if (pum != null)
                pum.ActivatePowerUp(type);

            // Destruyo el power-up después de aplicarlo
            Destroy(gameObject);
        }

        // Si el power-up llega hasta la DeathZone
        // desaparece sin aplicar ningún efecto
        if (other.CompareTag("DeathZone"))
            Destroy(gameObject);
    }
}