using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    public static GameController instance = null;

    // 管道预制件
    public GameObject pipesPrefabs;
    public Bird player;
    public Transform birdSpawn;
    public ScoreBoard scoreBoard;

    public Text countText;
    public GameObject welcomeNode;
    public GameObject readyStartNode;
    public GameObject gameOverNode;

    // 管道产生的频率，每几秒产生一个
    public float createPipesRate = 3f;

    // 管道中心位置的y最小值
    public float minPipPosY = -1f;
    // 管道中心位置的y最大值
    public float maxPipPosY = 4f;
    // 初始化管道的位置，x最好为负数不可见位置
    public Vector2 startPipPos = new Vector2(-12f, 0f);

    // 统计已经成功过了几个管道
    private int count = 0;
    
    // 定义一个游戏状态：未准备好，准备开始，正在游戏，游戏结束
    public enum GameState {
        NotReady,
        Ready,
        Playing,
        Over
    }

    // 小鸟是否已经死了
    public GameState state { get; private set; }

    // 上一次创建出管道的时间
    private float lastCreatePipTime = float.NegativeInfinity;

    // 缓存管道的链表，用来复用管道
    private List<GameObject> pipes = new List<GameObject>();
    // 管道缓存的个数
    private const int PIPESTOTAL = 8;
    // 当前管道下标，用来更新管道
    private int currPipesIndex = 0;

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(gameObject);
        }

        welcomeNode.SetActive(true);
        readyStartNode.SetActive(false);
        gameOverNode.SetActive(false);
    }

    private void Start() {
        state = GameState.NotReady;

        player.SetFree();
        InitPipesPool(); // 开始时，创建出管道缓存
    }

    private void Update() {

        switch (state) {
            case GameState.NotReady:
                
            case GameState.Ready:
                if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))) {
                    GameStart();
                }
                break;
            case GameState.Playing:
                // 当可以创建出管道时，拿出缓存中一个管道来更新位置
                if (lastCreatePipTime + createPipesRate < Time.time) {

                    lastCreatePipTime = Time.time;

                    UpdatePipesPosition();
                    currPipesIndex = (currPipesIndex + 1) % PIPESTOTAL;
                }
                
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) {
                    player.Fly();
                }
        
                break;
            case GameState.Over:
                // 等待输入重新开始游戏
                if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))) {
                    GameRestart();
                }
                break;
        }

    }

    /// <summary>
    /// 准备开始游戏
    /// </summary>
    public void GameReadyStart() {
        if (state != GameState.NotReady && state != GameState.Over) return;

        welcomeNode.SetActive(false);
        readyStartNode.SetActive(true);

        state = GameState.Ready;
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    public void GameStart() {
        if (state != GameState.Ready) return;

        readyStartNode.SetActive(false);

        player.SetControl();

        state = GameState.Playing;
    }

    /// <summary>
    /// 游戏结束
    /// </summary>
    public void GameOver() {
        if (state != GameState.Playing) return;

        readyStartNode.SetActive(false);
        gameOverNode.SetActive(true);

        SoundManager.instance.PlayDie();
        ReportScore();

        state = GameState.Over;
    }

    /// <summary>
    /// 游戏重新开始
    /// </summary>
    public void GameRestart() {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        readyStartNode.SetActive(true);
        gameOverNode.SetActive(false);

        count = 0;
        CountScore(0);

        foreach (GameObject pipe in pipes) {
            Destroy(pipe);
        }
        pipes.Clear();
        InitPipesPool();

        player.SetFree();
        player.transform.position = birdSpawn.position;
        player.transform.rotation = birdSpawn.rotation;

        state = GameState.Ready;
    }

    /// <summary>
    /// 初始化管道缓存池
    /// </summary>
    private void InitPipesPool() {
        for (int i = 0; i < PIPESTOTAL; ++i) {
            GameObject obj = Instantiate(pipesPrefabs, startPipPos, Quaternion.identity);
            pipes.Add(obj);
        }
    }

    /// <summary>
    /// 更新当前管道的位置
    /// </summary>
    private void UpdatePipesPosition() {
        float randomPosY = Random.Range(minPipPosY, maxPipPosY);
        Vector2 position = new Vector2(10f, randomPosY);
        pipes[currPipesIndex].transform.position = position;
    }

    /// <summary>
    /// 笨鸟成功通过一个管道
    /// </summary>
    public void PassOnePip() {
        CountScore(++count);
    }

    /// <summary>
    /// 记一次分数，并更新提示文字
    /// </summary>
    /// <param name="score">分数</param>
    private void CountScore(int score) {
        countText.text = "Count: " + score.ToString();
    }

    /// <summary>
    /// 生成成绩并评级
    /// </summary>
    private void ReportScore() {

        // 查找最好记录，并比较当前值决定是否更新
        int bestScore = PlayerPrefs.GetInt("BestScore");
        if (count > bestScore) {
            PlayerPrefs.SetInt("BestScore", count);
        }

        // 将数据评级并显示到成绩板
        scoreBoard.SetScore(count);
        scoreBoard.SetBestScore(bestScore);
        scoreBoard.CreateRating();
    }
}
