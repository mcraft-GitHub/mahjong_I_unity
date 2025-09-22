using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageViewManager : MonoBehaviour
{
    // フェードの時間
    private const float FADE_TIME = 1.0f;

    // 仮の演出時間
    private const float TMP_VISUAL_PRESENTATION_TIME = 1.0f;

    // 演出同士の間隔
    private const float VISUAL_PRESENTATION_DELAY_TIME = 1.0f;

    // 中心座標から幅を使用し端の座標を求める
    private const float CALC_SIDE_WITH_CENTER = 0.5f;

    //*** OpeningVisualPresentation
    // 文字フェード時間
    private const float OPENING_TEXT_FADE_TIME = 0.7f;

    //*** Advance・EncountVisualPresentation
    // ADVANCEの移動時間
    private const float ADVANCE_MOVE_TIME = 7.0f;
    // 進行度のフェード時間
    private const float ADVANCE_PROGRESS_FADE_TIME = 0.5f;
    // 進行度の非表示時間
    private const float ADVANCE_PROGRESS_NONE_DISPLAY_TIME = 0.0f;
    // 進行度の完全表示時間
    private const float ADVANCE_PROGRESS_DISPLAY_TIME = 1.0f;

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

    // Advance画像
    [SerializeField] private RectTransform _advanceImageRect;
    // 進行度表示
    [SerializeField] private TMP_Text _advanceProgressText;
    [SerializeField] private Image _advanceProgressUnderLineImage;

    // 敵の画像描画範囲(マスク)
    [SerializeField] private RectTransform _enemyImageMaskRect;
    // 敵の画像
    [SerializeField] private Image _enemyImage;

    // 敵の画像の変形
    private RectTransform _enemyImageRect;

    // Advance画像の移動先Z座標
    private float _advanceImageToPosX = 0.0f;

    void Start()
    {
        // 敵の画像描画範囲(マスク)の拡縮・座標設定
        _enemyImageMaskRect.sizeDelta = new Vector2(GameUILayoutUtility._enemyGaugeWidth, GameUILayoutUtility._enemyImageSize);
        _enemyImageMaskRect.anchoredPosition = new Vector2(0.0f, GameUILayoutUtility._enemyImagePosY);
        // 敵の画像の変形の取得
        _enemyImageRect = _enemyImage.GetComponent<RectTransform>();

        // テキストサイズの調整
        _openingText.fontSize = _openingText.fontSize * GameUILayoutUtility._screenWidthRate;

        // 進行度表示サイズの調整
        _advanceProgressText.fontSize = _advanceProgressText.fontSize * GameUILayoutUtility._screenHeightRate;
        RectTransform advanceProgressUnderLineRect = _advanceProgressUnderLineImage.GetComponent<RectTransform>();
        advanceProgressUnderLineRect.sizeDelta = advanceProgressUnderLineRect.sizeDelta * GameUILayoutUtility._screenHeightRate;
        advanceProgressUnderLineRect.anchoredPosition = advanceProgressUnderLineRect.anchoredPosition * GameUILayoutUtility._screenHeightRate;
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
            _fadeImage.DOColor(Color.clear, FADE_TIME).SetEase(Ease.InOutQuad);
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
            _fadeImage.DOColor(Color.black, FADE_TIME).SetEase(Ease.InOutQuad);
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
    /// Advance画像のセット・初期化
    /// </summary>
    /// <param name="image">Advance画像</param>
    public void SetAdvanceImage(Sprite image)
    {
        _advanceImageRect.GetComponent<Image>().sprite = image;
        _advanceImageRect.sizeDelta = new Vector2(Screen.height / image.textureRect.height * image.textureRect.width, Screen.height);
        _advanceImageToPosX = Screen.width * -CALC_SIDE_WITH_CENTER - _advanceImageRect.sizeDelta.x + Screen.width;
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
            _openingText.DOColor(_openingTextColor, OPENING_TEXT_FADE_TIME).SetEase(Ease.InOutQuad);
            yield return new WaitForSeconds(OPENING_TEXT_FADE_TIME + VISUAL_PRESENTATION_DELAY_TIME);
            // フェードアウト
            _openingText.DOColor(Color.clear, OPENING_TEXT_FADE_TIME).SetEase(Ease.InOutQuad);
        }
        // コルーチンの開始
        StartCoroutine(VisualPresentation());

        return OPENING_TEXT_FADE_TIME + OPENING_TEXT_FADE_TIME + VISUAL_PRESENTATION_DELAY_TIME + VISUAL_PRESENTATION_DELAY_TIME;
    }

    /// <summary>
    /// ステージ進行演出
    /// </summary>
    /// <param name="stageData">ステージデータ</param>
    /// <param name="gameData">ゲームデータ</param>
    /// <returns>演出時間</returns>
    public float AdvanceVisualPresentation(StageData stageData, GameController.GameData gameData)
    {
        // Advance画像のアクティブ化
        _advanceImageRect.gameObject.SetActive(true);
        // Advance画像の座標のセット
        _advanceImageRect.anchoredPosition = new Vector2(Screen.width * -CALC_SIDE_WITH_CENTER, Screen.height * -CALC_SIDE_WITH_CENTER);

        // 進行度の更新
        _advanceProgressText.text = (gameData._currentEnemtIdx + 1) + "/" + stageData._appearEnemy.Count;

        // 演出コルーチン
        IEnumerator VisualPresentation()
        {
            // フェードイン
            _fadeImage.color = Color.black;
            _fadeImage.DOColor(Color.clear, FADE_TIME).SetEase(Ease.InOutQuad);

            // advance画像移動開始
            _advanceImageRect.DOAnchorPosX(_advanceImageToPosX, ADVANCE_MOVE_TIME).SetEase(Ease.Linear);

            yield return new WaitForSeconds(ADVANCE_PROGRESS_NONE_DISPLAY_TIME + FADE_TIME);

            // 進行度のフェード
            Color fadeColor = _advanceProgressText.color;
            fadeColor.a = 1.0f;
            _advanceProgressText.DOColor(fadeColor, ADVANCE_PROGRESS_FADE_TIME);
            _advanceProgressUnderLineImage.DOColor(fadeColor, ADVANCE_PROGRESS_FADE_TIME).SetEase(Ease.InOutQuad);

            yield return new WaitForSeconds(ADVANCE_PROGRESS_DISPLAY_TIME + ADVANCE_PROGRESS_FADE_TIME);

            // 進行度のフェード
            fadeColor.a = 0.0f;
            _advanceProgressText.DOColor(fadeColor, ADVANCE_PROGRESS_FADE_TIME);
            _advanceProgressUnderLineImage.DOColor(fadeColor, ADVANCE_PROGRESS_FADE_TIME).SetEase(Ease.InOutQuad);

            yield return new WaitForSeconds(ADVANCE_PROGRESS_NONE_DISPLAY_TIME + ADVANCE_PROGRESS_FADE_TIME);

            // フェードアウト
            _fadeImage.color = Color.clear;
            _fadeImage.DOColor(Color.black, FADE_TIME).SetEase(Ease.InOutQuad);

            yield return new WaitForSeconds(FADE_TIME);

            // Advance画像の非アクティブ化
            _advanceImageRect.gameObject.SetActive(false);
        }
        // 演出の開始
        StartCoroutine(VisualPresentation());

        //     フェード            進行度非表示時間                   進行度フェード               進行度表示時間                進行度フェード                  進行度非表示時間              フェード
        return FADE_TIME + ADVANCE_PROGRESS_NONE_DISPLAY_TIME + ADVANCE_PROGRESS_FADE_TIME + ADVANCE_PROGRESS_DISPLAY_TIME + ADVANCE_PROGRESS_FADE_TIME + ADVANCE_PROGRESS_NONE_DISPLAY_TIME + FADE_TIME;
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
