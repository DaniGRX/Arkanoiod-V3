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

    [Header("Power-Ups")]
    [SerializeField] GameObject[] powerUpPrefabs; // arrastra los 3 prefabs
    [SerializeField] float powerUpDropChance = 0.3f; // 30% de probabilidad

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

            if (gameManager != null)
                gameManager.AddScore(points);

            TrySpawnPowerUp(); // ← AÑADE ESTA LÍNEA

            sr.enabled = false;

            if (breakAnimator != null)
            {
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
    void TrySpawnPowerUp()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;

        // Comprobamos probabilidad
        if (Random.value > powerUpDropChance) return;

        // Elegimos un power-up aleatorio
        int index = Random.Range(0, powerUpPrefabs.Length);
        if (powerUpPrefabs[index] != null)
            Instantiate(powerUpPrefabs[index], transform.position, Quaternion.identity);
    }
}