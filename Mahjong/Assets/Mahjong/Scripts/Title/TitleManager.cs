using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections;

public class TitleManager : MonoBehaviour
{
    private const float FADE_TIME = 1.0f;

    [SerializeField] private Image _fadeImage;

    void Start()
    {
        // フェードイン
        _fadeImage.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        _fadeImage.DOColor(new Color(0.0f, 0.0f, 0.0f, 0.0f), FADE_TIME);
    }

    void Update()
    {
        
    }

    public void ToGameScene()
    {
        // ゲームシーンへ遷移
        StartCoroutine(LoadScene(1));   
    }

    IEnumerator LoadScene(int buildIndex)
    {
        // フェードアウト
        yield return _fadeImage.DOColor(new Color(0.0f, 0.0f, 0.0f, 1.0f), FADE_TIME).WaitForCompletion();
        SceneManager.LoadScene(buildIndex);
    }
}
