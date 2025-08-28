using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Xml;
using System.Collections;

public class BattleViewManager : MonoBehaviour
{
    // 点数テキストの高さ
    private const float ROLE_POINT_TEXT_HEIGHT = 240.0f;
    // 役テキストと点数テキストの隙間の高さ
    private const float ROLE_POINT_TO_RESULT_BLANK = 20.0f;

    // 役テキスト・点数テキストの横の空白の割合
    private const float ROLE_RESULT_BLANK_RATE = 0.1f;

    // パズルリザルトの背景のアルファ値
    private const float ROLE_RESULT_BACKGROUND_ALPHA = 0.9f;

    // 半分
    private const float HALF = 0.5f;
    // 二倍
    private const float DOUBLE = 2.0f;

    // フェード時間
    private const float FADE_TIME = 0.5f;
    // フェード後役を表示し始めるまでの時間
    private const float BEGIN_DRAW_ROLE_DELAY = 0.1f;
    // 役表示間隔時間
    private const float DRAW_ROLE_DELAY = 0.5f;
    // 表示後消し始めるまでの時間
    private const float CLEAR_ROLE_RESULT_TIME = 2.0f;

    // 敵のHpゲージ・プレイヤーのHpゲージ・敵の攻撃ゲージ
    [SerializeField] private Slider _enemyHpGauge;
    [SerializeField] private Slider _playerHpGauge;
    [SerializeField] private Slider _enemyAttackGauge;

    // 敵画像
    [SerializeField] private Image _enemyImage;

    // パズル(役)リザルトの背景・役テキスト・点数テキスト
    [SerializeField] private Image _puzzleResultBackground;
    [SerializeField] private TMP_Text _roleResultText;
    [SerializeField] private TMP_Text _rolePointText;

    // ゲーム開始カウント
    [SerializeField] private TMP_Text _beginCntText;

    void Start()
    {
        //*** プレイヤーのHPゲージの配置・拡縮
        RectTransform playerHpRect = _playerHpGauge.GetComponent<RectTransform>();
        // 手牌の大きさ
        Vector2 handTileSize = GameData.TILE_SIZE * GameData._handTilesScale;
        // HPゲージの高さの半分
        float halfHpGaugeHeight = GameData.PLAYER_HP_GAUGE_HEIGHT * HALF;
        // 設定
        playerHpRect.anchoredPosition = new Vector2(0.0f, GameData.BUTTOM_SAFE_BLANK + handTileSize.y * DOUBLE + GameData.HEIGHT_BLANK * DOUBLE + halfHpGaugeHeight);
        playerHpRect.sizeDelta = new Vector2(Screen.width - GameData.MINIMUM_BLANK * DOUBLE, GameData.PLAYER_HP_GAUGE_HEIGHT);

        //*** 敵画像の配置・拡縮
        RectTransform enemyImageRect = _enemyImage.GetComponent<RectTransform>();
        // 画面の上の幅
        float screenUpHeight = GameData.TOP_SAFE_BLANK + GameData.ENEMY_HP_GAUGE_HEIGHT + GameData.ENEMY_ATTACK_GAUGE_HEIGHT;
        // 敵画像の縦の大きさ
        float enemyImageHeight = Screen.height - GameData._uiHeight - screenUpHeight;
        // 横幅より大きければ、横幅に合わせる
        if (enemyImageHeight > Screen.width)
            enemyImageHeight = Screen.width;
        // 設定
        enemyImageRect.sizeDelta = new Vector2(enemyImageHeight, enemyImageHeight);
        enemyImageRect.anchoredPosition = new Vector2(0.0f, -screenUpHeight - enemyImageHeight * HALF);

        //*** 敵UIの配置・拡縮
        RectTransform enemyHpRect = _enemyHpGauge.GetComponent<RectTransform>();
        RectTransform enemyAttackRect = _enemyAttackGauge.GetComponent<RectTransform>();
        enemyHpRect.sizeDelta = new Vector2(Screen.width - GameData.MINIMUM_BLANK * DOUBLE, GameData.ENEMY_HP_GAUGE_HEIGHT);
        enemyAttackRect.sizeDelta = new Vector2(Screen.width - GameData.MINIMUM_BLANK * DOUBLE, GameData.ENEMY_ATTACK_GAUGE_HEIGHT);
        enemyHpRect.anchoredPosition = new Vector2(0.0f, -GameData.TOP_SAFE_BLANK - GameData.ENEMY_HP_GAUGE_HEIGHT * HALF);
        enemyAttackRect.anchoredPosition = new Vector2(0.0f, -GameData.TOP_SAFE_BLANK - GameData.ENEMY_HP_GAUGE_HEIGHT - GameData.ENEMY_ATTACK_GAUGE_HEIGHT * HALF);

        //*** パズルリザルトテキストの配置・拡縮
        RectTransform roleResultRect = _roleResultText.GetComponent<RectTransform>();
        RectTransform rolePointRect = _rolePointText.GetComponent<RectTransform>();
        // 表示しない横幅
        float roleUnDrawWidth = Screen.width * ROLE_RESULT_BLANK_RATE;
        // 設定
        roleResultRect.offsetMax = new Vector2(-roleUnDrawWidth, -GameData.TOP_SAFE_BLANK);
        rolePointRect.offsetMin = new Vector2(roleUnDrawWidth, playerHpRect.anchoredPosition.y);
        rolePointRect.offsetMax = new Vector2(-roleUnDrawWidth, -(Screen.height - playerHpRect.anchoredPosition.y - ROLE_POINT_TEXT_HEIGHT));
        roleResultRect.offsetMin = new Vector2(roleUnDrawWidth, playerHpRect.anchoredPosition.y + ROLE_POINT_TEXT_HEIGHT + ROLE_POINT_TO_RESULT_BLANK);

    }

    /// <summary>
    /// ゲーム開始カウントダウンのセット
    /// </summary>
    /// <param name="cnt">カウント, -1なら表示消去</param>
    public void SetBeginGameCount(int cnt)
    {
        if (cnt == -1)
            _beginCntText.text = "";
        else if (cnt != int.Parse(_beginCntText.text))
        {
            _beginCntText.text = cnt.ToString();
        }
    }

    /// <summary>
    /// 役攻撃演出
    /// </summary>
    /// <param name="role">役情報</param>
    /// <param name="damage">ダメージ</param>
    /// <returns>演出時間</returns>
    public float BeginRoleResult(MahjongLogic.Role role, int damage)
    {
        StartCoroutine(ShowRoleResultCoroutine(role, damage));

        // フェード時間 + 役表示 + ダメージ表示時間
        return FADE_TIME + FADE_TIME + BEGIN_DRAW_ROLE_DELAY + (role.roleKinds.Count + role.dora > 0 ? 1 : 0) * DRAW_ROLE_DELAY + CLEAR_ROLE_RESULT_TIME;
    }

    /// <summary>
    /// 敵HPのセット
    /// </summary>
    /// <param name="value">敵HP(1f～0f)</param>
    public void SetEnemyHp(float value)
    {
        _enemyHpGauge.DOValue(value, _enemyHpGauge.value - value * 2.0f);
    }

    /// <summary>
    /// プレイヤーのセット
    /// </summary>
    /// <param name="value">プレイヤーHP(1f～0f)</param>
    public void SetPlayerHp(float value)
    {
        _playerHpGauge.DOValue(value, _playerHpGauge.value - value * 2.0f);
    }

    /// <summary>
    /// 攻撃カウントのセット
    /// </summary>
    /// <param name="value">攻撃カウント(1f～0f)</param>
    public void SetEnemyAttack(float value)
    {
        _enemyAttackGauge.value = value;
    }

    /// <summary>
    /// 敵の画像のセット
    /// </summary>
    /// <param name="image">敵の画像</param>
    public void SetEnemyImage(Sprite image)
    {
        _enemyImage.sprite = image;
    }

    /// <summary>
    /// 役攻撃演出コルーチン
    /// </summary>
    /// <param name="role">役情報</param>
    /// <param name="damage">ダメージ</param>
    private IEnumerator ShowRoleResultCoroutine(MahjongLogic.Role role, int damage)
    {
        // フェード
        _puzzleResultBackground.DOColor(new Color(0.0f, 0.0f, 0.0f, ROLE_RESULT_BACKGROUND_ALPHA), FADE_TIME);
        yield return new WaitForSeconds(FADE_TIME + BEGIN_DRAW_ROLE_DELAY);

        // 役を1つずつ表示
        for (int i = 0; i < role.roleKinds.Count; i++)
        {
            _roleResultText.text += MahjongLogic.ROLE_NAME[(int)role.roleKinds[i]] + '\n';
            yield return new WaitForSeconds(DRAW_ROLE_DELAY);
        }
        if (role.dora > 0)
        {
            _roleResultText.text += "ドラ" + role.dora + '\n';
            yield return new WaitForSeconds(DRAW_ROLE_DELAY);
        }

        // ダメージ表示
        _rolePointText.text = damage + "ダメージ";
        yield return new WaitForSeconds(CLEAR_ROLE_RESULT_TIME);

        // 表示消去
        _roleResultText.text = "";
        _rolePointText.text = "";

        // フェード
        yield return _puzzleResultBackground.DOColor(new Color(0.0f, 0.0f, 0.0f, 0.0f), FADE_TIME).WaitForCompletion();
    }
}
