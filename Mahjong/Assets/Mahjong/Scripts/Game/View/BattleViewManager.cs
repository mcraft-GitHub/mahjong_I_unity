using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleViewManager : MonoBehaviour
{
    [SerializeField] private Slider _enemyHpGauge;
    [SerializeField] private Slider _playerHpGauge;
    [SerializeField] private Slider _enemyAttackGauge;

    [SerializeField] private Image _enemyImage;

    void Start()
    {
        //*** プレイヤーのHPゲージの配置・拡縮
        RectTransform playerHpRect = _playerHpGauge.GetComponent<RectTransform>();
        // 手牌の大きさ
        Vector2 handTileSize = GameData.TILE_SIZE * GameData.handTilesScale;
        // HPゲージの高さの半分
        float halfHpGaugeHeight = GameData.PLAYER_HP_GAUGE_HEIGHT * 0.5f;
        // 設定
        playerHpRect.anchoredPosition = new Vector2(0.0f, GameData.BUTTOM_SAFE_BLANK + handTileSize.y * 2.0f + GameData.HEIGHT_BLANK * 2.0f + halfHpGaugeHeight);
        playerHpRect.sizeDelta = new Vector2(Screen.width - GameData.MINIMUM_BLANK * 2.0f, GameData.PLAYER_HP_GAUGE_HEIGHT);

        //*** 敵画像の配置・拡縮
        RectTransform enemyImageRect = _enemyImage.GetComponent<RectTransform>();
        // 画面の上の幅
        float screenUpHeight = GameData.UP_SAFE_BLANK + GameData.ENEMY_HP_GAUGE_HEIGHT + GameData.ENEMY_ATTACK_GAUGE_HEIGHT;
        // 敵画像の縦の大きさ
        float enemyImageHeight = Screen.height - GameData.uiHeight - screenUpHeight;
        // 横幅より大きければ、横幅に合わせる
        if (enemyImageHeight > Screen.width)
            enemyImageHeight = Screen.width;
        // 設定
        enemyImageRect.sizeDelta = new Vector2(enemyImageHeight, enemyImageHeight);
        enemyImageRect.anchoredPosition = new Vector2(0.0f, -screenUpHeight - enemyImageHeight * 0.5f);

        //*** 敵UIの配置・拡縮
        RectTransform enemyHpRect = _enemyHpGauge.GetComponent<RectTransform>();
        RectTransform enemyAttackRect = _enemyAttackGauge.GetComponent<RectTransform>();
        enemyHpRect.sizeDelta = new Vector2(Screen.width - GameData.MINIMUM_BLANK * 2.0f, GameData.ENEMY_HP_GAUGE_HEIGHT);
        enemyAttackRect.sizeDelta = new Vector2(Screen.width - GameData.MINIMUM_BLANK * 2.0f, GameData.ENEMY_ATTACK_GAUGE_HEIGHT);
        enemyHpRect.anchoredPosition = new Vector2(0.0f, -GameData.UP_SAFE_BLANK - GameData.ENEMY_HP_GAUGE_HEIGHT * 0.5f);
        enemyAttackRect.anchoredPosition = new Vector2(0.0f, -GameData.UP_SAFE_BLANK - GameData.ENEMY_HP_GAUGE_HEIGHT - GameData.ENEMY_ATTACK_GAUGE_HEIGHT * 0.5f);
    }

    void Update()
    {
        
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
}
