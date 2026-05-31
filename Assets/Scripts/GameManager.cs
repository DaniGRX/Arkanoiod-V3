using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    // ── Estado global ──────────────────────────────────────
    public enum GameState { Menu, Playing, Paused, Victory, GameOver }
    public GameState CurrentState { get; private set; } = GameState.Menu;

    // ── Referencias a otros sistemas ──────────────────────
    [Header("Sistemas")]
    [SerializeField] BlockGridSpawner blockSpawner;
    [SerializeField] BallController ball;
    [SerializeField] Transform ballSpawn;

    // ── UI ─────────────────────────────────────────────────
    [Header("Paneles UI")]
    [SerializeField] GameObject panelMenu;
    [SerializeField] GameObject panelHUD;
    [SerializeField] GameObject panelVictory;
    [SerializeField] GameObject panelGameOver;
    [SerializeField] GameObject panelPause;

    [Header("Textos HUD")]
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text livesText;

    // ── Variables de partida ───────────────────────────────
    [Header("Configuración")]
    [SerializeField] int maxLives = 3;
    int score;
    int currentLives;
    int blocksRemaining;

    // ── Combo ──────────────────────────────────────────────
    [Header("Combo")]                                        // ← NUEVO
    [SerializeField] public int comboMultiplier = 1;        // ← NUEVO
    [SerializeField] int maxCombo = 10;                     // ← NUEVO
    [SerializeField] TMP_Text comboText;                    // ← NUEVO
    public int ComboMultiplier => comboMultiplier;          // ← NUEVO

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioClip victoryClip;
    [SerializeField] AudioClip gameOverClip;
    [SerializeField] AudioClip loseLifeClip;
    [SerializeField] AudioClip musicMenu;
    [SerializeField] AudioClip musicVictory;
    [SerializeField] AudioClip musicPlaying;
    [SerializeField] AudioClip musicPause;

    // ── Awake ──────────────────────────────────────────────
    void Awake()
    {
        if (ball == null)
        {
            GameObject mainBallObj = GameObject.FindWithTag("MainBall");
            if (mainBallObj != null)
                ball = mainBallObj.GetComponent<BallController>();
        }
    }

    // ── Inicio ─────────────────────────────────────────────
    void Start()
    {
        SetState(GameState.Menu);
    }

    // ── Tecla Escape para pausar/reanudar ──────────────────
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CurrentState == GameState.Playing)
                SetState(GameState.Paused);
            else if (CurrentState == GameState.Paused)
                ResumeGame();
        }
    }

    // ── Cambio de estado central ───────────────────────────
    void SetState(GameState newState)
    {
        CurrentState = newState;

        panelMenu.SetActive(newState == GameState.Menu);
        panelHUD.SetActive(newState == GameState.Playing);
        panelPause.SetActive(newState == GameState.Paused);
        panelVictory.SetActive(newState == GameState.Victory);
        panelGameOver.SetActive(newState == GameState.GameOver);

        if (audioSource != null) audioSource.Stop();
        if (musicSource != null) musicSource.Stop();

        switch (newState)
        {
            case GameState.Menu:
                Time.timeScale = 1f;
                if (audioSource != null && musicMenu != null)
                {
                    audioSource.clip = musicMenu;
                    audioSource.loop = true;
                    audioSource.Play();
                }
                break;

            case GameState.Playing:
                Time.timeScale = 1f;
                if (musicSource != null && musicPlaying != null)
                {
                    musicSource.clip = musicPlaying;
                    musicSource.loop = true;
                    musicSource.Play();
                }
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                if (musicSource != null && musicPause != null)
                {
                    musicSource.clip = musicPause;
                    musicSource.loop = true;
                    musicSource.Play();
                }
                break;

            case GameState.Victory:
                Time.timeScale = 1f;
                if (musicSource != null && musicVictory != null)
                {
                    musicSource.clip = musicVictory;
                    musicSource.loop = true;
                    musicSource.Play();
                }
                break;

            case GameState.GameOver:
                Time.timeScale = 1f;
                if (audioSource != null && gameOverClip != null)
                    audioSource.PlayOneShot(gameOverClip);
                break;
        }

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
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        panelPause.SetActive(false);
        panelHUD.SetActive(true);

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
        score = 0;
        currentLives = maxLives;
        comboMultiplier = 1;    // ← NUEVO
        UpdateScoreUI();
        UpdateLivesUI();
        UpdateComboUI();        // ← NUEVO

        blockSpawner.Spawn();
        blocksRemaining = blockSpawner.GetTotalBlocks();

        SetState(GameState.Playing);
    }

    // ── Volver al menú ─────────────────────────────────────
    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SetState(GameState.Menu);
    }

    // ── Puntuación ─────────────────────────────────────────
    public void AddScore(int amount)
    {
        // Multiplicamos los puntos por el combo actual
        int finalPoints = amount * comboMultiplier;         // ← NUEVO
        score += finalPoints;                               // ← NUEVO

        // Limitamos el combo al máximo configurado
        comboMultiplier = Mathf.Clamp(                     // ← NUEVO
            comboMultiplier, 1, maxCombo);                 // ← NUEVO

        UpdateScoreUI();
        blocksRemaining--;
        CheckVictory();
    }

    void CheckVictory()
    {
        if (blocksRemaining <= 0)
            SetState(GameState.Victory);
    }

    // ── Combo ──────────────────────────────────────────────
    public void ResetCombo()                               // ← NUEVO
    {                                                      // ← NUEVO
        comboMultiplier = 1;                               // ← NUEVO
        UpdateComboUI();                                   // ← NUEVO
    }                                                      // ← NUEVO

    public void UpdateComboUI()                            // ← NUEVO
    {                                                      // ← NUEVO
        if (comboText != null)                             // ← NUEVO
            comboText.text = "Combo X" + comboMultiplier; // ← NUEVO
    }                                                      // ← NUEVO

    // ── Vidas ──────────────────────────────────────────────
    public void LoseLife()
    {
        if (CurrentState != GameState.Playing) return;

        if (CountActiveBalls() > 1) return;

        currentLives--;
        UpdateLivesUI();

        if (audioSource != null && loseLifeClip != null)
            audioSource.PlayOneShot(loseLifeClip);

        if (currentLives <= 0)
            SetState(GameState.GameOver);
        else
            ResetBall();
    }

    // ── Resetear bola ──────────────────────────────────────
    void ResetBall()
    {
        BallController[] balls = FindObjectsByType<BallController>(FindObjectsSortMode.None);
        foreach (BallController b in balls)
        {
            if (b != ball)
                Destroy(b.gameObject);
        }

        if (ball == null)
        {
            GameObject mainBallObj = GameObject.FindWithTag("MainBall");
            if (mainBallObj != null)
                ball = mainBallObj.GetComponent<BallController>();
        }

        if (ball == null)
        {
            Debug.LogWarning("No se encontró la bola principal.");
            return;
        }

        ball.ResetBall(ballSpawn.position);
    }

    // ── Contar bolas en juego ──────────────────────────────
    int CountActiveBalls()
    {
        return FindObjectsByType<BallController>(FindObjectsSortMode.None).Length;
    }

    // ── Actualizar UI ──────────────────────────────────────
    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    void UpdateLivesUI()
    {
        if (livesText != null)
            livesText.text = "Lives: " + currentLives;
    }

    // ── Salir del juego ────────────────────────────────────
    public void ExitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}