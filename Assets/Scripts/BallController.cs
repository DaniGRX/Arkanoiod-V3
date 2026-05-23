using UnityEngine; // Librería principal de Unity

// Obligo a que este objeto tenga un Rigidbody2D
// Así evito errores físicos si me olvido de añadirlo
[RequireComponent(typeof(Rigidbody2D))]
public class BallController : MonoBehaviour
{
    // ── Configuración ──────────────────────────────────────

    // Velocidad inicial de la bola al lanzarla
    [SerializeField] float initialSpeed = 7f;

    // Ángulo máximo que puede alcanzar el rebote respecto a la pala
    [SerializeField] float maxBounceAngle = 75f;

    // ── Referencias ────────────────────────────────────────

    // Referencia a la pala para que la bola pueda seguirla antes del lanzamiento
    [SerializeField] Transform paddleTransform;

    // Pequeño desplazamiento para colocar la bola encima de la pala
    [SerializeField] Vector2 attachOffset = new Vector2(0f, 0.6f);

    // Referencia al GameManager para controlar estados y puntuación
    [SerializeField] GameManager gameManager;

    // Texto de ayuda que indica cómo lanzar la bola
    [SerializeField] GameObject launchText;

    // ── Audio ──────────────────────────────────────────────

    [Header("Audio")]

    // Fuente de audio para reproducir sonidos
    [SerializeField] AudioSource audioSource;

    // Sonido de rebote
    [SerializeField] AudioClip bounceClip;

    // Sonido al romper un bloque
    [SerializeField] AudioClip blockClip;

    // ── Efectos visuales ───────────────────────────────────

    [Header("Efectos visuales")]

    // Prefab del efecto visual al rebotar
    [SerializeField] GameObject bounceEffectPrefab;

    // ── Estado interno ─────────────────────────────────────

    // Referencia al Rigidbody2D de la bola
    Rigidbody2D rb;

    // Variable que indica si la bola sigue pegada a la pala
    bool isAttached = true;

    // ── Inicialización ─────────────────────────────────────

    void Awake()
    {
        // Obtengo automáticamente el Rigidbody2D del objeto
        rb = GetComponent<Rigidbody2D>();
    }

    // ── Cada frame ─────────────────────────────────────────

    void Update()
    {
        // Si la bola sigue enganchada a la pala
        if (isAttached && paddleTransform != null)
        {
            // La bola sigue constantemente la posición de la pala
            transform.position = paddleTransform.position + (Vector3)attachOffset;

            // Solo permito lanzar la bola si el juego está realmente jugando
            if (gameManager.CurrentState == GameManager.GameState.Playing &&
                Input.GetKeyDown(KeyCode.Space))
            {
                // Lanzo la bola al pulsar espacio
                Launch();
            }
        }
    }

    // ── Lanzamiento ────────────────────────────────────────

    public void Launch()
    {
        // Evito lanzar la bola varias veces seguidas
        if (!isAttached) return;

        // La bola deja de estar pegada a la pala
        isAttached = false;

        // Reactivo las físicas de la bola
        rb.simulated = true;

        // Oculto el texto de ayuda al lanzar
        if (launchText != null)
            launchText.SetActive(false);

        // Creo una dirección aleatoria ligeramente inclinada
        // Así la bola no sale siempre totalmente recta
        Vector2 dir = new Vector2(Random.Range(-0.3f, 0.3f), 1f).normalized;

        // Aplico velocidad a la bola
        rb.linearVelocity = dir * initialSpeed;
    }

    // ── Reinicio (llamado por GameManager) ─────────────────

    public void ResetBall(Vector2 position)
    {
        // Coloco la bola en la posición inicial
        transform.position = position;

        // Detengo completamente su movimiento
        rb.linearVelocity = Vector2.zero;

        // Desactivo temporalmente las físicas
        rb.simulated = false;

        // La bola vuelve a quedarse pegada a la pala
        isAttached = true;

        // Muestro otra vez el texto de ayuda
        if (launchText != null)
            launchText.SetActive(true);
    }

    // ── Colisiones ─────────────────────────────────────────

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Compruebo si el objeto golpeado tiene script Block
        Block block = collision.collider.GetComponent<Block>();

        // Si realmente es un bloque
        if (block != null)
        {
            // Reproduzco sonido de impacto contra bloque
            if (audioSource != null && blockClip != null)
                audioSource.PlayOneShot(blockClip);

            // GameManager.AddScore(block.Points);
            // Ahora el propio bloque avisa al GameManager cuando se destruye

            // Llamo al bloque para que gestione el impacto
            block.TakeHit();

            return;
        }

        // Si no es bloque entonces será pared o pala
        // Reproduzco sonido de rebote
        if (audioSource != null && bounceClip != null)
            audioSource.PlayOneShot(bounceClip);

        // Compruebo si la colisión fue específicamente con la pala
        if (collision.collider.CompareTag("Paddle"))
        {
            // Posición X de la pala
            float paddleX = collision.transform.position.x;

            // Posición X de la bola al impactar
            float hitX = transform.position.x;

            // Calculo qué tan lejos del centro golpeó la bola
            // Clamp evita valores demasiado extremos
            float normalizedHit = Mathf.Clamp((paddleX - hitX) / 1.0f, -1f, 1f);

            // Transformo ese valor en un ángulo de rebote
            float angle = normalizedHit * maxBounceAngle;

            // Creo una nueva dirección basada en el ángulo calculado
            Vector2 newDir = (Quaternion.Euler(0f, 0f, angle) * Vector2.up).normalized;

            // Guardo la velocidad actual de la bola
            float speed = rb.linearVelocity.magnitude;

            // Seguridad por si la velocidad fuese demasiado baja
            if (speed < 0.01f)
                speed = initialSpeed;

            // Aplico la nueva dirección manteniendo la velocidad
            rb.linearVelocity = newDir * speed;

            // Efecto visual justo en el punto exacto del impacto
            if (bounceEffectPrefab != null)
            {
                // Obtengo el punto de contacto real
                ContactPoint2D contact = collision.GetContact(0);

                // Guardo la posición exacta del golpe
                Vector2 hitPoint = contact.point;

                // Instancio el efecto visual en ese punto
                Instantiate(bounceEffectPrefab, hitPoint, Quaternion.identity);
            }
        }
    }
}