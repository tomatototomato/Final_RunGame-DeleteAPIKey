using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Auth;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class RankingManager : MonoBehaviour
{
    [Header("UI参照")]
    public Text currentScoreText;   // 今回のスコア表示用
    public Text statusText;         // 保存・更新の状態表示用
    public Text rankingListText;    // ランキング10位までの表示用

    [Header("ボタン参照")]
    public Button saveAndRefreshButton; // 保存と更新を行うボタン
    public Button retryButton;          // ゲームシーンへ戻るボタン
    public Button logoutButton;         // ログインシーンへ戻るボタン

    private FirebaseFirestore db;
    private FirebaseAuth auth;

    void Start()
    {
        // Firebaseの初期化
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;

        // 各ボタンに機能を登録
        if (saveAndRefreshButton != null)
            saveAndRefreshButton.onClick.AddListener(OnSaveAndRefreshClick);

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClick);

        if (logoutButton != null)
            logoutButton.onClick.AddListener(OnLogoutClick);

        // ゲーム画面から引き継いだスコアを画面に表示
        UpdateScoreDisplay();

        // 起動時に最新のランキングを読み込む
        _ = RefreshRankingDisplay();
    }

    void UpdateScoreDisplay()
    {
        if (currentScoreText != null)
        {
            currentScoreText.text = "今回のスコア: " + GameDirector.Score.ToString();
        }
    }

    // 保存と更新ボタンが押された時の処理
    async void OnSaveAndRefreshClick()
    {
        if (auth.CurrentUser == null)
        {
            statusText.text = "エラー: ログインが必要です";
            return;
        }

        saveAndRefreshButton.interactable = false; // 連打防止

        bool success = await SaveScoreToFirestore();

        if (success)
        {
            await RefreshRankingDisplay();
        }

        saveAndRefreshButton.interactable = true;
    }

    // ゲーム画面へ戻る
    void OnRetryClick()
    {
        SceneManager.LoadScene("GameScene");
    }

    // ログアウトしてログイン画面へ戻る
    void OnLogoutClick()
    {
        if (auth != null)
        {
            auth.SignOut();
        }
        SceneManager.LoadScene("LoginScene");
    }

    // Firestoreへスコアを保存する処理
    private async Task<bool> SaveScoreToFirestore()
    {
        statusText.text = "スコアを保存中...";
        string userId = auth.CurrentUser.UserId;

        // ユーザー名が設定されていなければメールアドレスの頭を使用
        string userName = !string.IsNullOrEmpty(auth.CurrentUser.DisplayName)
                          ? auth.CurrentUser.DisplayName
                          : auth.CurrentUser.Email.Split('@')[0];

        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "userId", userId },
            { "userName", userName },
            { "score", GameDirector.Score },
            { "timestamp", FieldValue.ServerTimestamp }
        };

        try
        {
            // ユーザーIDをドキュメント名にして保存（上書き）
            await db.Collection("rankings").Document(userId).SetAsync(data);
            statusText.text = "保存に成功しました！";
            return true;
        }
        catch (Exception e)
        {
            statusText.text = "保存失敗: " + e.Message;
            Debug.LogError(e);
            return false;
        }
    }

    // ランキングを読み込んで表示する処理
    private async Task RefreshRankingDisplay()
    {
        statusText.text = "ランキングを読み込み中...";

        try
        {
            // スコアの高い順に10件取得
            Query query = db.Collection("rankings")
                            .OrderByDescending("score")
                            .Limit(10);

            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            string displayStr = "--- トップ10 ランキング ---\n\n";
            int rank = 1;

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                Dictionary<string, object> entry = document.ToDictionary();
                string name = entry.ContainsKey("userName") ? entry["userName"].ToString() : "不明";
                string score = entry.ContainsKey("score") ? entry["score"].ToString() : "0";

                displayStr += rank + "位: " + name + " - " + score + "点\n";
                rank++;
            }

            rankingListText.text = displayStr;
            statusText.text = "更新完了";
        }
        catch (Exception e)
        {
            statusText.text = "読み込み失敗";
            Debug.LogError(e);
        }
    }
}