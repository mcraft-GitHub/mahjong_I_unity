using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UI;

public class StageViewManager : MonoBehaviour
{
    // フェードの時間
    private const float FADE_TIME = 1.0f;

    // 仮の演出時間
    private const float TMP_VISUAL_PRESENTATION_TIME = 1.0f;

    // 演出同士の間隔
    private const float VISUAL_PRESENTATION_DELAY_TIME = 0.5f;

    //***OpeningVisualPresentation
    // 文字フェード時間
    private const float OPENING_TEXT_FADE_TIME = 0.7f;

    //*** BattleWinVisualPresentation
    // 演出開始待機時間
    private const float BATTLE_WIN_BEGIN_WAIT_TIME = 1.5f;
    // 敵降下時間
    private const float BATTLE_WIN_ENEMY_EXIT_TIME = 0.7f;

    // フェード
    [SerializeField] private Image _fadeImage;

    // ステージタイトル
    [SerializeField] private TMP_Text _openingText;
    // ステージタイトル色
    [SerializeField] private Color _openingTextColor;

    // 敵の画像描画範囲(マスク)
    [SerializeField] private RectTransform _enemyImageMaskRect;
    // 敵の画像
    [SerializeField] private Image _enemyImage;

    //  敵の画像の変形
    RectTransform _enemyImageRect;

    void Start()
    {
        // 敵の画像描画範囲(マスク)の拡縮・座標設定
        _enemyImageMaskRect.sizeDelta = new Vector2(GameUILayoutUtility._enemyGaugeWidth, GameUILayoutUtility._enemyImageSize);
        _enemyImageMaskRect.anchoredPosition = new Vector2(0.0f, GameUILayoutUtility._enemyImagePosY);
        // 敵の画像の変形の取得
        _enemyImageRect = _enemyImage.GetComponent<RectTransform>();

        // テキストサイズの調整
        _openingText.fontSize = _openingText.fontSize * GameUILayoutUtility._screenWidthRate;
    }

    /// <summary>
    /// フェードインを開始
    /// </summary>
    /// <param name="waitTime">初手待機時間</param>
    /// <returns>フェードにかかる時間</returns>
    public float BeginFadeIn(float waitTime = 0.0f)
    {
        // 演出コルーチン
        IEnumerator VisualPresentation()
        {
            yield return new WaitForSeconds(waitTime);
            _fadeImage.color = Color.black;
            _fadeImage.DOColor(Color.clear, FADE_TIME);
        }
        // コルーチンの開始
        StartCoroutine(VisualPresentation());
        return FADE_TIME;
    }


    /// <summary>
    /// フェードアウトを開始
    /// </summary>
    /// <param name="waitTime">初手待機時間</param>
    /// <returns>フェードにかかる時間</returns>
    public float BeginFadeOut(float waitTime = 0.0f)
    {
        // 演出コルーチン
        IEnumerator VisualPresentation()
        {
            yield return new WaitForSeconds(waitTime);
            _fadeImage.color = Color.clear;
            _fadeImage.DOColor(Color.black, FADE_TIME);
        }
        // コルーチンの開始
        StartCoroutine(VisualPresentation());
        return FADE_TIME;
    }

    /// <summary>
    /// 敵の画像のセット・初期化
    /// </summary>
    /// <param name="image">敵の画像</param>
    public void SetEnemyImage(Sprite image)
    {
        _enemyImage.sprite = image;
        _enemyImageRect.sizeDelta = new Vector2(GameUILayoutUtility._enemyImageSize, GameUILayoutUtility._enemyImageSize);
        _enemyImageRect.anchoredPosition = new Vector2(0.0f, 0.0f);
    }

    /// <summary>
    /// オープニング演出
    /// </summary>
    /// <returns>演出時間</returns>
    public float OpeningVisualPresentation(string stageName)
    {
        // 初手黒
        _fadeImage.color = Color.black;

        // 文字の初期化
        _openingText.text = stageName;
        _openingText.color = Color.clear;

        // 演出コルーチン
        IEnumerator VisualPresentation()
        {
            // フェードイン
            _openingText.DOColor(_openingTextColor, OPENING_TEXT_FADE_TIME);
            yield return new WaitForSeconds(OPENING_TEXT_FADE_TIME + VISUAL_PRESENTATION_DELAY_TIME);
            // フェードアウト
            _openingText.DOColor(Color.clear, OPENING_TEXT_FADE_TIME);
        }
        // コルーチンの開始
        StartCoroutine(VisualPresentation());

        return OPENING_TEXT_FADE_TIME + OPENING_TEXT_FADE_TIME + VISUAL_PRESENTATION_DELAY_TIME + VISUAL_PRESENTATION_DELAY_TIME;
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
        // 演出コルーチン
        IEnumerator VisualPresentation()
        {
            yield return new WaitForSeconds(BATTLE_WIN_BEGIN_WAIT_TIME);
            _enemyImageRect.DOAnchorPosY(-GameUILayoutUtility._enemyImageSize, BATTLE_WIN_ENEMY_EXIT_TIME);
        }
        // コルーチンの開始
        StartCoroutine(VisualPresentation());
        return BATTLE_WIN_BEGIN_WAIT_TIME + BATTLE_WIN_ENEMY_EXIT_TIME;
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
