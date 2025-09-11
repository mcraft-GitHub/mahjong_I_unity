using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StageViewManager : MonoBehaviour
{
    // フェードの時間
    private const float FADE_TIME = 1.0f;

    // 仮の演出時間
    private const float TMP_VISUAL_PRESENTATION_TIME = 1.0f;

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
        _fadeImage.color = Color.black;
        _fadeImage.DOColor(Color.clear, FADE_TIME);
        return FADE_TIME;
    }


    /// <summary>
    /// フェードアウトを開始
    /// </summary>
    /// <returns>フェードにかかる時間</returns>
    public float BeginFadeOut()
    {
        _fadeImage.color = Color.clear;
        _fadeImage.DOColor(Color.black, FADE_TIME);
        return FADE_TIME;
    }

    /// <summary>
    /// オープニング演出
    /// </summary>
    /// <returns>演出時間</returns>
    public float OpeningVisualPresentation()
    {
        // TODO:演出
        return TMP_VISUAL_PRESENTATION_TIME;
    }

    /// <summary>
    /// ステージ進行演出
    /// </summary>
    /// <returns>演出時間</returns>
    public float AdvanceVisualPresentation()
    {
        // TODO:演出
        return TMP_VISUAL_PRESENTATION_TIME;
    }

    /// <summary>
    /// 敵と遭遇演出
    /// </summary>
    /// <returns>演出時間</returns>
    public float EncountVisualPresentation()
    {
        // TODO:演出
        return TMP_VISUAL_PRESENTATION_TIME;
    }

    /// <summary>
    /// 戦闘勝利演出
    /// </summary>
    /// <returns>演出時間</returns>
    public float BattleWinVisualPresentation()
    {
        // TODO:演出
        return TMP_VISUAL_PRESENTATION_TIME;
    }

    /// <summary>
    /// 戦闘敗北演出
    /// </summary>
    /// <returns>演出時間</returns>
    public float BattleLoseVisualPresentation()
    {
        // TODO:演出
        return TMP_VISUAL_PRESENTATION_TIME;
    }

    /// <summary>
    /// ステージクリア演出
    /// </summary>
    /// <returns>演出時間</returns>
    public float StageClearVisualPresentation()
    {
        // TODO:演出
        return TMP_VISUAL_PRESENTATION_TIME;
    }
}
