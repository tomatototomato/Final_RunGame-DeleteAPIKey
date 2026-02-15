using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class FirebaseAuthTest : MonoBehaviour
{
    [Header("UI References (Legacy)")]
    public InputField emailInputField;
    public InputField passwordInputField;
    public Text statusText;
    public Button registerButton;
    public Button loginButton;
    public Button verificationButton; // 認証メール再送用
    public Button startGameButton;    // ログイン成功後に出現するボタン

    private FirebaseAuth auth;

    void Start()
    {
        // Firebaseの初期化と依存関係チェック
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                Debug.Log("Firebase Initialized.");
            }
            else
            {
                Debug.LogError($"Firebaseの初期化に失敗しました: {dependencyStatus}");
            }
        });

        // 各ボタンにメソッドを登録
        if (registerButton != null)
            registerButton.onClick.AddListener(OnRegisterClick);

        if (loginButton != null)
            loginButton.onClick.AddListener(OnLoginClick);

        if (verificationButton != null)
            verificationButton.onClick.AddListener(OnSendVerificationClick);

        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(MoveToGameScene);
            // 最初はゲーム開始ボタンを隠しておく
            startGameButton.gameObject.SetActive(false);
        }
    }

    // --- 新規登録処理 ---
    async void OnRegisterClick()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = "メールとパスワードを入力してください";
            return;
        }

        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            statusText.text = "登録成功！認証メールを送信します...";
            await SendEmailVerificationInternal();
        }
        catch (Exception e)
        {
            statusText.text = "登録失敗: " + (e.InnerException?.Message ?? e.Message);
        }
    }

    // --- ログイン処理 ---
    async void OnLoginClick()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = "メールとパスワードを入力してください";
            return;
        }

        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);

            // メール認証が済んでいるかチェック
            if (result.User.IsEmailVerified)
            {
                statusText.text = "<color=green>ログイン成功！\n開始ボタンを押してゲームを始めてください。</color>";

                // UIの切り替え：入力欄やログインボタンを隠し、開始ボタンを表示する
                loginButton.gameObject.SetActive(false);
                registerButton.gameObject.SetActive(false);
                emailInputField.gameObject.SetActive(false);
                passwordInputField.gameObject.SetActive(false);
                verificationButton.gameObject.SetActive(false);

                startGameButton.gameObject.SetActive(true);
            }
            else
            {
                statusText.text = "<color=red>メール認証が完了していません。\nメールを確認してください。</color>";
                auth.SignOut();
            }
        }
        catch (Exception e)
        {
            statusText.text = "ログイン失敗: " + (e.InnerException?.Message ?? e.Message);
        }
    }

    // --- 実際にシーンを移動させる処理 ---
    void MoveToGameScene()
    {
        // 遷移先のシーン名（GameScene）がBuild Settingsに登録されている必要があります
        SceneManager.LoadScene("GameScene");
    }

    // --- 認証メール送信（ボタン用） ---
    void OnSendVerificationClick()
    {
        _ = SendEmailVerificationInternal();
    }

    // --- 認証メール送信（内部処理） ---
    async Task SendEmailVerificationInternal()
    {
        if (auth.CurrentUser != null)
        {
            try
            {
                await auth.CurrentUser.SendEmailVerificationAsync();
                statusText.text = "認証メールを送信しました。";
            }
            catch (Exception e)
            {
                statusText.text = "メール送信失敗: " + e.Message;
            }
        }
        else
        {
            statusText.text = "対象のユーザーが見つかりません。";
        }
    }
}