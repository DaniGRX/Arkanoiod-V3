using UnityEngine;          // Librería principal de Unity
using System.Collections;   // Librería necesaria para utilizar Coroutines

public class PowerUpManager : MonoBehaviour
{
    [Header("Referencias")]

    // Referencia a la pala del jugador
    [SerializeField] PaddleController paddle;

    // Prefab que utilizaré para generar nuevas bolas
    [SerializeField] BallController ballPrefab;

    // Punto donde aparecerán las bolas creadas por el multibola
    [SerializeField] Transform ballSpawn;

    [Header("Configuración")]

    // Tiempo que duran los efectos de aumentar o reducir la pala
    [SerializeField] float paddleGrowDuration = 8f;

    // Multiplicador de tamaño que se aplica a la pala
    [SerializeField] float paddleGrowScale = 1.6f;

    // ── Estado interno ────────────────────────────────────

    // Referencia a la Coroutine activa para controlar los efectos temporales
    Coroutine paddleCoroutine;

    // Almacena el tamaño original de la pala para restaurarlo después
    float originalPaddleScaleX;

    void Awake()
    {
        // Guardo la escala inicial de la pala al iniciar la escena
        // para poder recuperarla cuando finalice un power-up
        originalPaddleScaleX = paddle.transform.localScale.x;
    }

    public void ActivatePowerUp(PowerUp.PowerUpType type)
    {
        // Compruebo qué tipo de power-up se ha recogido
        switch (type)
        {
            case PowerUp.PowerUpType.PaddleGrow:

                // Activo el efecto de aumentar el tamaño de la pala
                ApplyPaddleScale(paddleGrowScale, paddleGrowDuration);
                break;

            case PowerUp.PowerUpType.PaddleShrink:

                // Activo el efecto de reducir el tamaño de la pala
                ApplyPaddleScale(1f / paddleGrowScale, paddleGrowDuration);
                break;

            case PowerUp.PowerUpType.MultiBall:

                // Genero una bola adicional en la partida
                SpawnExtraBall();
                break;
        }
    }

    // ── Pala más grande / más pequeña ─────────────────────

    void ApplyPaddleScale(float scaleMultiplier, float duration)
    {
        // Si ya existe un efecto activo lo cancelamos
        // para evitar que varios cambios de tamaño se acumulen
        if (paddleCoroutine != null)
            StopCoroutine(paddleCoroutine);

        // Iniciamos una nueva Coroutine para gestionar el efecto temporal
        paddleCoroutine = StartCoroutine(
            PaddleScaleRoutine(scaleMultiplier, duration)
        );
    }

    IEnumerator PaddleScaleRoutine(float multiplier, float duration)
    {
        // Obtengo la escala actual de la pala
        Vector3 scale = paddle.transform.localScale;

        // Modifico únicamente el tamaño horizontal
        scale.x = originalPaddleScaleX * multiplier;

        // Aplico la nueva escala a la pala
        paddle.transform.localScale = scale;

        // Mantengo el efecto activo durante el tiempo indicado
        yield return new WaitForSeconds(duration);

        // Recupero el tamaño original de la pala
        scale.x = originalPaddleScaleX;

        // Vuelvo a aplicar la escala inicial
        paddle.transform.localScale = scale;

        // Libero la referencia porque ya no hay efecto activo
        paddleCoroutine = null;
    }

    // ── Multibola ─────────────────────────────────────────

    void SpawnExtraBall()
    {
        // Compruebo que existan las referencias necesarias
        if (ballPrefab == null || ballSpawn == null) return;

        // Creo una nueva instancia de la pelota
        // utilizando el prefab configurado
        BallController newBall = Instantiate(
            ballPrefab,
            ballSpawn.position,
            Quaternion.identity
        );

        // Lanzo automáticamente la nueva pelota
        // sin necesidad de pulsar espacio
        newBall.Launch();
    }
}