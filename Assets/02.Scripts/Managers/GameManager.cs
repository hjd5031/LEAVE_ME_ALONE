using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public enum GameSceneState
{
    GameStartScene,
    MainGame,
    GameEnd
}

public class GameManager : Singleton<GameManager>
{
    public int PlayerScore = 0;
    public int EnemyScore = 0;

    public bool PLayerUsingItem;
    public bool EnemyUsingItem;

    public bool isGameOver = false;
    public bool isGameStart = false;

    public TextMeshProUGUI timeText;
    public TextMeshProUGUI playerScore;
    public TextMeshProUGUI enemyScore;

    public EnemyTomatoCtrl[] enemyTomatoes;
    public PlayerTomatoCtrl[] playerTomatoes;
    private bool hasInitializedMainGame = false;

    public GameSceneState CurrentSceneState { get; private set; }

    private float gameTimer = 0f;
    private float maxGameTime = 300f;

    private GameObject[] eraseCanvas;
    public GameObject[] instructionCanvas1;
    // public GameObject[] instructionCanvas2;
    private bool readInstructions1 = false;
    private bool readInstructions2 = false;
    private bool almostFinished = false;
    private bool singlePlayerModeSelected = false;

    private bool bgm3Started = false; // ✅ WebGL 대응용 BGM3 재생 체크

    public override void Awake()
    {
#if UNITY_SERVER
        Debug.Log("[GameManager] Disabled in UNITY_SERVER build.");
        Destroy(gameObject);
        return;
#endif

#if MULTIPLAYER_BUILD
        Debug.Log("[GameManager] Disabled in MULTIPLAYER_BUILD.");
        Destroy(gameObject);
        return;
#endif

        string activeSceneName = SceneManager.GetActiveScene().name;
        if (IsMultiplayerScene(activeSceneName))
        {
            Debug.Log($"[GameManager] Disabled in multiplayer scene '{activeSceneName}'.");
            Destroy(gameObject);
            return;
        }

        base.Awake();
    }

    void Start()
    {
        if (IsMultiplayerScene(SceneManager.GetActiveScene().name))
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        // SceneManager.LoadScene("GameStartScene");
        SetSceneState(GameSceneState.GameStartScene);
        eraseCanvas = GameObject.FindGameObjectsWithTag("GameNameCanvas");
        instructionCanvas1 = GameObject.FindGameObjectsWithTag("Instructions");
        if (instructionCanvas1 != null && instructionCanvas1.Length > 0)
            instructionCanvas1[0].SetActive(false);
        if (instructionCanvas1 != null && instructionCanvas1.Length > 1)
            instructionCanvas1[1].SetActive(false);
    }

    void Update()
    {
        // ✅ GameStartScene → 입력 1번 → 설명 / 2번 → MainGameScene
        if (CurrentSceneState == GameSceneState.GameStartScene && singlePlayerModeSelected)
        {
            if (!readInstructions1 && Input.anyKeyDown)
            {
                foreach (var canvas in eraseCanvas)
                {
                    if (canvas != null)
                        canvas.SetActive(false);
                }

                if (instructionCanvas1 != null && instructionCanvas1.Length > 0)
                    instructionCanvas1[0].SetActive(true);

                readInstructions1 = true;
            }
            else if (readInstructions1 && !readInstructions2 && Input.anyKeyDown)
            {
                if (instructionCanvas1 != null && instructionCanvas1.Length > 0)
                    instructionCanvas1[0].SetActive(false);
                if (instructionCanvas1 != null && instructionCanvas1.Length > 1)
                    instructionCanvas1[1].SetActive(true);

                readInstructions2 = true;
            }
            else if (readInstructions1 && readInstructions2 &&Input.anyKeyDown)
            {
                readInstructions1 = false;
                readInstructions2 = false;
                SceneManager.LoadScene("MainGameScene");
            }
        }

        // ✅ MainGameScene에서 첫 입력 시 BGM3 재생
        if (CurrentSceneState == GameSceneState.MainGame && !bgm3Started && Input.anyKeyDown)
        {
            SoundManager.Instance.PlayBGM(SoundManager.Bgm.BGM3, true);
            bgm3Started = true;
            Debug.Log("[GameManager] 첫 입력 감지 → BGM3 시작");
        }

        // ✅ 게임 타이머 진행
        if (isGameStart && !isGameOver)
        {
            gameTimer += Time.deltaTime;

            if (gameTimer >= 180f && !almostFinished)
            {
                almostFinished = true;
                SoundManager.Instance.PlayBGM(SoundManager.Bgm.BGM3, false); // BGM3 정지
                SoundManager.Instance.PlayBGM(SoundManager.Bgm.BGM2, true);  // BGM2 재생
                Debug.Log("[GameManager] 3분 경과 → BGM3 종료, BGM2 시작");
            }

            if (gameTimer >= maxGameTime)
            {
                gameTimer = maxGameTime;
                // SoundManager.Instance.StopAllSfx();
                SoundManager.Instance.PlayBGM(SoundManager.Bgm.BGM2, false);

                if (PlayerScore < EnemyScore)
                    SceneManager.LoadScene("GameLoseScene");
                else if(PlayerScore > EnemyScore)
                    SceneManager.LoadScene("GameWinScene");
                else
                {
                    SceneManager.LoadScene("GameTieScene");
                }
            }

            UpdateScoreText();
            UpdateTimeText();
        }
    }

    void UpdateTimeText()
    {
        int timeLeft = Mathf.FloorToInt(maxGameTime - gameTimer);
        int minutes = timeLeft / 60;
        int seconds = timeLeft % 60;
        timeText.text = $"{minutes:D2}:{seconds:D2}";
    }

    void UpdateScoreText()
    {
        if (playerScore != null)
            playerScore.text = $"Player: {PlayerScore}";
        if (enemyScore != null)
            enemyScore.text = $"Enemy: {EnemyScore}";
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsMultiplayerScene(scene.name))
        {
            Debug.Log($"[GameManager] Destroying single-player GameManager in multiplayer scene '{scene.name}'.");
            Destroy(gameObject);
            return;
        }

        if (scene.name == "MainGameScene" && !hasInitializedMainGame)
        {
            SetSceneState(GameSceneState.MainGame);
            hasInitializedMainGame = true;
            Debug.Log("[GameManager] MainGameScene 로딩 완료됨. 초기화 수행.");

            CountTomatoes();

            GameObject timeObj = GameObject.FindGameObjectWithTag("timer");
            if (timeObj != null)
                timeText = timeObj.GetComponent<TextMeshProUGUI>();

            GameObject playerScoreObj = GameObject.FindGameObjectWithTag("PlayerScore");
            if (playerScoreObj != null)
                playerScore = playerScoreObj.GetComponent<TextMeshProUGUI>();

            GameObject enemyScoreObj = GameObject.FindGameObjectWithTag("EnemyScore");
            if (enemyScoreObj != null)
                enemyScore = enemyScoreObj.GetComponent<TextMeshProUGUI>();

            gameTimer = 0f;
            UpdateTimeText();
            UpdateScoreText();
        }
        else if (scene.name == "GameLoseScene" || scene.name == "GameWinScene" || scene.name == "GameTieScene")
        {
            SoundManager.Instance.StopAllSfx();
            SetSceneState(GameSceneState.GameEnd);

            GameObject playerScoreObj = GameObject.FindGameObjectWithTag("PlayerScore");
            GameObject enemyScoreObj = GameObject.FindGameObjectWithTag("EnemyScore");

            if (playerScoreObj != null)
            {
                var text = playerScoreObj.GetComponent<TextMeshProUGUI>();
                text.text = PlayerScore.ToString("D3"); // 000 형식
            }

            if (enemyScoreObj != null)
            {
                var text = enemyScoreObj.GetComponent<TextMeshProUGUI>();
                text.text = EnemyScore.ToString("D3"); // 000 형식
            }
        }
        else if (scene.name == "GameStartScene")
        {
            SetSceneState(GameSceneState.GameStartScene);
            // eraseCanvas = GameObject.FindGameObjectsWithTag("GameNameCanvas");
            // instructionCanvas1 = GameObject.FindGameObjectWithTag("Instructions");
            // instructionCanvas1.SetActive(false);
        }
    }

    private static bool IsMultiplayerScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return false;
        }

        return sceneName.IndexOf("Multiplayer", StringComparison.OrdinalIgnoreCase) >= 0 ||
               sceneName.IndexOf("Multiplay", StringComparison.OrdinalIgnoreCase) >= 0 ||
               sceneName.IndexOf("DedicatedServer", StringComparison.OrdinalIgnoreCase) >= 0 ||
               sceneName.IndexOf("Network", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    void CountTomatoes()
    {
        enemyTomatoes = FindObjectsByType<EnemyTomatoCtrl>(FindObjectsSortMode.None);
        playerTomatoes = FindObjectsByType<PlayerTomatoCtrl>(FindObjectsSortMode.None);
        Debug.Log("Tomato Initialized");
        Debug.Log(enemyTomatoes.Length);
        Debug.Log(playerTomatoes.Length);
    }

    public void SetSceneState(GameSceneState newState)
    {
        if (CurrentSceneState == newState) return;

        CurrentSceneState = newState;
        Debug.Log($"[GameManager] Scene changed to: {newState}");

        switch (newState)
        {
            case GameSceneState.GameStartScene:
                break;
            case GameSceneState.MainGame:
                isGameStart = true;
                isGameOver = false;
                break;
            case GameSceneState.GameEnd:
                isGameOver = true;
                break;
        }
    }

    public void BeginSinglePlayerMode()
    {
        singlePlayerModeSelected = true;
        Debug.Log("[GameManager] Single Player mode selected. Existing intro flow is now enabled.");
    }
}
