using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StageViewManager : MonoBehaviour
{
    // フェードの時間
    private const float FADE_TIME = 1.0f;

    // フェード
    [SerializeField] private Image _fadeImage;

    void Start()
    {
    }

    /// <summary>
    /// フェードインを開始
    /// </summary>
    /// <returns>フェードにかかる時間</returns>
    public float BeginFadeIn()
    {
        _fadeImage.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        _fadeImage.DOColor(new Color(0.0f, 0.0f, 0.0f, 0.0f), FADE_TIME);
        return FADE_TIME;
    }


    /// <summary>
    /// フェードアウトを開始
    /// </summary>
    /// <returns>フェードにかかる時間</returns>
    public float BeginFadeOut()
    {
        _fadeImage.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        _fadeImage.DOColor(new Color(0.0f, 0.0f, 0.0f, 1.0f), FADE_TIME);
        return FADE_TIME;
    }
}
