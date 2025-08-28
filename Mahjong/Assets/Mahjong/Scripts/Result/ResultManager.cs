using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class ResultManager : MonoBehaviour
{
    private const float FADE_TIME = 1.0f;

    private const string WIN_TEXT = "勝利";
    private const string LOSE_TEXT = "敗北";

    [SerializeField] private Image _fadeImage;
    [SerializeField] private TMP_Text _resultText;

    void Start()
    {
        // 勝敗
        if (GameController._isWin)
            _resultText.text = WIN_TEXT;
        else
            _resultText.text = LOSE_TEXT;

        // フェードイン
        _fadeImage.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        _fadeImage.DOColor(new Color(0.0f, 0.0f, 0.0f, 0.0f), FADE_TIME);
    }

    public void ToGameScene()
    {
        // ゲームシーンへ遷移
        StartCoroutine(LoadScene("GameScene"));   
    }

    public void ToTitleScene()
    {
        // タイトルシーンへ遷移
        StartCoroutine(LoadScene("TitleScene"));
    }

    IEnumerator LoadScene(string sceneName)
    {
        // フェードアウト
        yield return _fadeImage.DOColor(new Color(0.0f, 0.0f, 0.0f, 1.0f), FADE_TIME).WaitForCompletion();
        SceneManager.LoadScene(sceneName);
    }
}
