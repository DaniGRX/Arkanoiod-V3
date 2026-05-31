using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BallController : MonoBehaviour
{
    // ── Configuración ──────────────────────────────────────
    [SerializeField] float initialSpeed = 7f;
    [SerializeField] float maxBounceAngle = 75f;

    // ── Referencias ────────────────────────────────────────
    [SerializeField] Transform paddleTransform;
    [SerializeField] Vector2 attachOffset = new Vector2(0f, 0.6f);
    [SerializeField] GameManager gameManager;
    [SerializeField] GameObject launchText;

    // ── Audio ──────────────────────────────────────────────
    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip bounceClip;
    [SerializeField] AudioClip blockClip;

    // ── Efectos visuales ───────────────────────────────────
    [Header("Efectos visuales")]
    [SerializeField] GameObject bounceEffectPrefab;

    // ── Estado interno ─────────────────────────────────────
    Rigidbody2D rb;
    bool isAttached = true;

    // ── Inicialización ─────────────────────────────────────
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Si gameManager no está asignado en el Inspector
        // lo buscamos automáticamente en la escena
        // Esto es necesario para las bolas extra del MultiBall
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        // Si paddleTransform no está asignado lo buscamos por tag
        if (paddleTransform == null)
        {
            GameObject paddleObj = GameObject.FindWithTag("Paddle");
            if (paddleObj != null)
                paddleTransform = paddleObj.transform;
        }
    }

    // ── Cada frame ─────────────────────────────────────────
    void Update()
    {
        if (isAttached && paddleTransform != null)
        {
            transform.position = paddleTransform.position + (Vector3)attachOffset;

            if (gameManager != null &&
                gameManager.CurrentState == GameManager.GameState.Playing &&
                Input.GetKeyDown(KeyCode.Space))
            {
                Launch();
            }
        }
    }

    // ── Corrección de velocidad ────────────────────────────
    void FixedUpdate()
    {
        if (!isAttached && rb.simulated)
        {
            float currentSpeed = rb.linearVelocity.magnitude;

            if (Mathf.Abs(currentSpeed - initialSpeed) > 0.1f)
                rb.linearVelocity = rb.linearVelocity.normalized * initialSpeed;
        }
    }

    // ── Lanzamiento ────────────────────────────────────────
    public void Launch()
    {
        if (!isAttached) return;

        isAttached = false;
        rb.simulated = true;

        if (launchText != null)
            launchText.SetActive(false);

        Vector2 dir = new Vector2(Random.Range(-0.3f, 0.3f), 1f).normalized;
        rb.linearVelocity = dir * initialSpeed;
    }

    // ── Reinicio (llamado por GameManager) ─────────────────
    public void ResetBall(Vector2 position)
    {
        transform.position = position;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        isAttached = true;

        if (launchText != null)
            launchText.SetActive(true);
    }

    // ── Colisiones ─────────────────────────────────────────
    void OnCollisionEnter2D(Collision2D collision)
    {
        // ¿Es un bloque?
        Block block = collision.collider.GetComponent<Block>();
        if (block != null)
        {
            if (audioSource != null && blockClip != null)
                audioSource.PlayOneShot(blockClip);

            block.TakeHit();
            return;
        }

        // ¿Es pared o pala?
        if (audioSource != null && bounceClip != null)
            audioSource.PlayOneShot(bounceClip);

        // ¿Es la pala?
        if (collision.collider.CompareTag("Paddle"))
        {
            float paddleX = collision.transform.position.x;
            float hitX = transform.position.x;
            float normalizedHit = Mathf.Clamp((paddleX - hitX) / 1.0f, -1f, 1f);
            float angle = normalizedHit * maxBounceAngle;

            Vector2 newDir = (Quaternion.Euler(0f, 0f, angle) * Vector2.up).normalized;
            float speed = rb.linearVelocity.magnitude;
            if (speed < 0.01f) speed = initialSpeed;
            rb.linearVelocity = newDir * speed;

            // Efecto visual en el punto de impacto
            if (bounceEffectPrefab != null)
            {
                ContactPoint2D contact = collision.GetContact(0);
                Vector2 hitPoint = contact.point;
                Instantiate(bounceEffectPrefab, hitPoint, Quaternion.identity);
            }

            // Resetea el combo con comprobación de null
            if (gameManager != null)
                gameManager.ResetCombo();
        }
    }
}