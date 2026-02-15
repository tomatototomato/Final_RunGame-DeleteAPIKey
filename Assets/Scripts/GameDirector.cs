using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameDirector : MonoBehaviour
{
    public static int Score = 0;
    public GameObject GoldCoin;
    public GameObject SilverCoin;
    public GameObject WoodCoin;
    public GameObject Car;

    float gameTimer = 60.0f; //ゲーム時間

    //出現確率の設定;
    int goldCoinRatio = 1;    //金貨
    int silverCoinRatio = 3;  //銀貨
    int woodCoinRatio = 5;    //木貨
    //int carRatio = 4;         //車

    //生成時間
    float generateWaitTime = 1.0f;
    float generateTimer = 1.0f;

    [SerializeField]private Text timerText;
    [SerializeField]private Text scoreText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        generateTimer = generateWaitTime;

    }

    // Update is called once per frame
    void Update()
    {
        //タイマーのカウントダウン
        gameTimer -= Time.deltaTime;

        if (timerText != null) {
            timerText.GetComponent<Text>().text = "Time : " + gameTimer.ToString("F1");
        }

        //アイテムの生成タイマー
        generateTimer -= Time.deltaTime;

        if (0 > generateTimer)
        {
            //アイテムのランダム生成
            bool carFlag = false;
            int rand = Random.Range(0, 13);
            GameObject instanceItem;

            if (rand < goldCoinRatio)
            {
                instanceItem = Instantiate(GoldCoin);
            }
            else if (rand < goldCoinRatio + silverCoinRatio)
            {
                instanceItem = Instantiate(SilverCoin);
            }
            else if (rand < goldCoinRatio + silverCoinRatio + woodCoinRatio)
            {
                instanceItem = Instantiate(WoodCoin);
            }
            else
            {
                instanceItem = Instantiate(Car);
                carFlag = true;
            }

            //生成位置のランダム設定
            float x = Random.Range(-5, 5);
            float y;

            if (carFlag)
            {
                y = -0.5f;
            }
            else
            {
                y = (Random.Range(0, 2) == 0) ? 0.5f : 1.5f;
            }

            //ゲームオブジェクトの生成
            instanceItem.transform.position = new Vector3(x, y, 20);


            //生成タイマーのリセット
            float next = generateWaitTime;

            if (gameTimer < 10) 
            {
                next = generateWaitTime * 0.1f;
            }
            else if (gameTimer < 30)
            {
                next = generateWaitTime * 0.3f;
            }
            else if (gameTimer < 50)
            {
                next = generateWaitTime * 0.7f;
            }

            generateTimer = next;

            if(gameTimer < 0)
            {
                SceneManager.LoadScene("RankingDisplay");
            }

            //carFlagの初期化
            carFlag = false;

        }
    }

    public void AddScore(int v = 100)
    {
        Score += v;
        scoreText.GetComponent<Text>().text = "Score : " + Score;
    }
}
