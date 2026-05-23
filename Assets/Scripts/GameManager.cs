using UnityEngine;          // Librería principal de Unity
using TMPro;                // Librería para usar textos TMP (TextMeshPro)

public class GameManager : MonoBehaviour
{
    // ── Estado global ──────────────────────────────────────

    // Creo los posibles estados del juego para poder controlar todo más fácilmente
    public enum GameState { Menu, Playing, Paused, Victory, GameOver }

    // Aquí guardo el estado actual del juego
    // El private set evita que otros scripts lo modifiquen directamente
    public GameState CurrentState { get; private set; } = GameState.Menu;

    // ── Referencias a otros sistemas ──────────────────────

    [Header("Sistemas")] // Esto solo sirve para organizar el inspector visualmente

    // Referencia al script que genera todos los bloques
    [SerializeField] BlockGridSpawner blockSpawner;

    // Referencia a la pelota principal
    [SerializeField] BallController ball;

    // Punto donde aparecerá la pelota al reiniciarse
    [SerializeField] Transform ballSpawn;

    // ── UI ─────────────────────────────────────────────────

    [Header("Paneles UI")]

    // Panel principal del menú
    [SerializeField] GameObject panelMenu;

    // Panel del HUD durante la partida
    [SerializeField] GameObject panelHUD;

    // Panel de victoria
    [SerializeField] GameObject panelVictory;

    // Panel de derrota
    [SerializeField] GameObject panelGameOver;

    // Panel de pausa
    [SerializeField] GameObject panelPause;

    [Header("Textos HUD")]

    // Texto donde se muestra la puntuación
    [SerializeField] TMP_Text scoreText;

    // Texto donde se muestran las vidas
    [SerializeField] TMP_Text livesText;

    // ── Variables de partida ───────────────────────────────

    [Header("Configuración")]

    // Número máximo de vidas al empezar la partida
    [SerializeField] int maxLives = 3;

    // Variable donde voy guardando la puntuación
    int score;

    // Variable para controlar las vidas actuales
    int currentLives;

    // Cantidad de bloques que quedan vivos
    int blocksRemaining;

    [Header("Audio")]

    // Audio principal para efectos sueltos
    [SerializeField] AudioSource audioSource;

    // Audio dedicado a la música
    [SerializeField] AudioSource musicSource;

    // Sonido de victoria
    [SerializeField] AudioClip victoryClip;

    // Sonido de derrota
    [SerializeField] AudioClip gameOverClip;

    // Sonido cuando pierdo una vida
    [SerializeField] AudioClip loseLifeClip;

    // Música del menú
    [SerializeField] AudioClip musicMenu;

    // Música de victoria
    [SerializeField] AudioClip musicVictory;

    // Música de partida
    [SerializeField] AudioClip musicPlaying;

    // Música de pausa
    [SerializeField] AudioClip musicPause;

    // ── Inicio ─────────────────────────────────────────────

    void Start()
    {
        // Al iniciar el juego entramos automáticamente al menú
        SetState(GameState.Menu);
    }

    // ── Tecla Escape para pausar/reanudar ──────────────────

    void Update()
    {
        // Detecto si el jugador pulsa Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Si estamos jugando, pausamos
            if (CurrentState == GameState.Playing)
                SetState(GameState.Paused);

            // Si ya estaba pausado, reanudamos
            else if (CurrentState == GameState.Paused)
                ResumeGame();
        }
    }

    // ── Cambio de estado central ───────────────────────────

    void SetState(GameState newState)
    {
        // Actualizo el estado actual
        CurrentState = newState;

        // Activo o desactivo paneles según el estado del juego
        panelMenu.SetActive(newState == GameState.Menu);
        panelHUD.SetActive(newState == GameState.Playing);
        panelPause.SetActive(newState == GameState.Paused);
        panelVictory.SetActive(newState == GameState.Victory);
        panelGameOver.SetActive(newState == GameState.GameOver);

        // Detengo cualquier música anterior para evitar solapamientos
        if (audioSource != null) audioSource.Stop();
        if (musicSource != null) musicSource.Stop();

        // Aquí gestiono qué ocurre en cada estado
        switch (newState)
        {
            case GameState.Menu:

                // Aseguro que el tiempo vuelve a la normalidad
                Time.timeScale = 1f;

                // Reproduzco música del menú
                if (audioSource != null && musicMenu != null)
                {
                    audioSource.clip = musicMenu;
                    audioSource.loop = true;
                    audioSource.Play();
                }
                break;

            case GameState.Playing:

                // El juego vuelve a moverse normalmente
                Time.timeScale = 1f;

                // Reproduzco música de partida
                if (musicSource != null && musicPlaying != null)
                {
                    musicSource.clip = musicPlaying;
                    musicSource.loop = true;
                    musicSource.Play();
                }
                break;

            case GameState.Paused:

                // Congelo completamente el juego
                Time.timeScale = 0f;

                // Música específica de pausa
                if (musicSource != null && musicPause != null)
                {
                    musicSource.clip = musicPause;
                    musicSource.loop = true;
                    musicSource.Play();
                }
                break;

            case GameState.Victory:

                // Vuelvo a activar el tiempo normal
                Time.timeScale = 1f;

                // Música de victoria
                if (musicSource != null && musicVictory != null)
                {
                    musicSource.clip = musicVictory;
                    musicSource.loop = true;
                    musicSource.Play();
                }
                break;

            case GameState.GameOver:

                // El tiempo vuelve a la normalidad
                Time.timeScale = 1f;

                // Reproduzco sonido de derrota
                if (audioSource != null && gameOverClip != null)
                    audioSource.PlayOneShot(gameOverClip);

                break;
        }

        // Reinicio la bola al salir de la partida
        // Pero NO lo hago en pausa para conservar la posición
        if (newState != GameState.Playing &&
            newState != GameState.Paused &&
            ball != null)
        {
            ball.ResetBall(ballSpawn.position);
        }
    }

    // ── Reanudar partida ───────────────────────────────────

    public void ResumeGame()
    {
        // Cambio manualmente el estado a Playing
        CurrentState = GameState.Playing;

        // Reactivo el tiempo del juego
        Time.timeScale = 1f;

        // Oculto panel pausa y vuelvo al HUD
        panelPause.SetActive(false);
        panelHUD.SetActive(true);

        // Recupero la música normal de partida
        if (musicSource != null && musicPlaying != null)
        {
            musicSource.clip = musicPlaying;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    // ── Iniciar partida ────────────────────────────────────

    public void StartGame()
    {
        // Reinicio puntuación
        score = 0;

        // Recupero vidas máximas
        currentLives = maxLives;

        // Actualizo interfaz
        UpdateScoreUI();
        UpdateLivesUI();

        // Genero todos los bloques
        blockSpawner.Spawn();

        // Guardo cuántos bloques existen
        blocksRemaining = blockSpawner.GetTotalBlocks();

        // Empiezo la partida
        SetState(GameState.Playing);
    }

    // ── Volver al menú ─────────────────────────────────────

    public void GoToMenu()
    {
        // Por seguridad reactivo el tiempo
        Time.timeScale = 1f;

        // Cambio al menú principal
        SetState(GameState.Menu);
    }

    // ── Puntuación ─────────────────────────────────────────

    public void AddScore(int amount)
    {
        // Sumo puntos
        score += amount;

        // Actualizo texto del HUD
        UpdateScoreUI();

        // Resto un bloque destruido
        blocksRemaining--;

        // Compruebo si ya he ganado
        CheckVictory();
    }

    void CheckVictory()
    {
        // Si no quedan bloques ganamos la partida
        if (blocksRemaining <= 0)
            SetState(GameState.Victory);
    }

    // ── Vidas ──────────────────────────────────────────────

    public void LoseLife()
    {
        // Evito perder vidas fuera de la partida
        if (CurrentState != GameState.Playing) return;

        // Resto una vida
        currentLives--;

        // Actualizo HUD
        UpdateLivesUI();

        // Sonido de perder vida
        if (audioSource != null && loseLifeClip != null)
            audioSource.PlayOneShot(loseLifeClip);

        // Si no quedan vidas termina la partida
        if (currentLives <= 0)
            SetState(GameState.GameOver);

        // Si aún quedan vidas reinicio la pelota
        else
            ResetBall();
    }

    void ResetBall()
    {
        // Coloco la bola otra vez en el punto inicial
        ball.ResetBall(ballSpawn.position);
    }

    // ── Actualizar UI ──────────────────────────────────────

    void UpdateScoreUI()
    {
        // Compruebo que el texto exista
        if (scoreText != null)

            // Muestro puntuación actual
            scoreText.text = "Score: " + score;
    }

    void UpdateLivesUI()
    {
        // Compruebo que el texto exista
        if (livesText != null)

            // Muestro vidas actuales
            livesText.text = "Lives: " + currentLives;
    }

    // ── Salir del juego ────────────────────────────────────

    public void ExitGame()
    {
        // Mensaje en consola para comprobar que funciona
        Debug.Log("Saliendo del juego...");

        // Cierra la aplicación
        Application.Quit();
    }
}