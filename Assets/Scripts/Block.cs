using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour
{
    // ── Configuración ──────────────────────────────────────
    [SerializeField] int points = 10;
    [SerializeField] int hitPoints = 1;

    [Header("Sprites de daño")]
    [SerializeField] Sprite[] damageSprites;

    [Header("Animación")]
    // Ahora referenciamos el Animator del hijo BreakEffect
    [SerializeField] Animator breakAnimator;
    [SerializeField] float destroyDelay = 0.25f;

    // ── Estado interno ─────────────────────────────────────
    bool isBreaking = false;
    SpriteRenderer sr;
    GameManager gameManager;
    int maxHitPoints;

    // ── Propiedad pública ──────────────────────────────────
    public int Points => points;

    // ── Inicialización ─────────────────────────────────────
    void Awake()
    {
        // SpriteRenderer del objeto RAÍZ (el bloque principal)
        sr = GetComponent<SpriteRenderer>();
        gameManager = FindFirstObjectByType<GameManager>();
        maxHitPoints = hitPoints;
    }

    // ── Recibir impacto ────────────────────────────────────
    public void TakeHit()
    {
        if (isBreaking) return;

        hitPoints--;

        if (hitPoints <= 0)
        {
            isBreaking = true;

            // Suma puntos cuando se destruye definitivamente
            if (gameManager != null)
                gameManager.AddScore(points);

            // Ocultamos el sprite principal del bloque
            // La animación del hijo se encarga de la rotura
            sr.enabled = false;

            if (breakAnimator != null)
            {
                // Activamos la animación en el objeto hijo
                breakAnimator.SetTrigger("Break");
                StartCoroutine(DestroyAfterAnimation());
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            // Actualiza sprite de daño
            UpdateDamageSprite();
        }
    }

    // ── Actualizar sprite de daño ──────────────────────────
    void UpdateDamageSprite()
    {
        if (damageSprites == null || damageSprites.Length == 0) return;

        int damageTaken = maxHitPoints - hitPoints;
        int index = Mathf.Clamp(damageTaken - 1, 0, damageSprites.Length - 1);

        if (damageSprites[index] != null)
            sr.sprite = damageSprites[index];
    }

    // ── Corrutina de destrucción retrasada ─────────────────
    IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}