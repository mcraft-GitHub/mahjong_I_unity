using UnityEngine;
using UnityEngine.UI;

public class BattleViewManager : MonoBehaviour
{
    [SerializeField] private Slider _enemyHpGauge;
    [SerializeField] private Slider _playerHpGauge;
    [SerializeField] private Slider _enemyAttackGauge;

    void Start()
    {
        // プレイヤーのHPゲージの配置・拡縮
        RectTransform playerHpRect = _playerHpGauge.GetComponent<RectTransform>();
        // 手牌の大きさ
        Vector2 handTileSize = GameData.TILE_SIZE * GameData.handTilesScale;
        // HPゲージの高さの半分
        float halfHpGaugeHeight = GameData.HP_GAUGE_HEIGHT * 0.5f;
        // 設定
        playerHpRect.anchoredPosition = new Vector2(0.0f, GameData.BUTTOM_SAFE_BLANK + handTileSize.y * 2.0f + GameData.HEIGHT_BLANK * 2.0f + halfHpGaugeHeight);
        playerHpRect.sizeDelta = new Vector2(Screen.width - GameData.MINIMUM_BLANK * 2.0f, GameData.HP_GAUGE_HEIGHT);
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
        _enemyHpGauge.value = value;
    }

    /// <summary>
    /// プレイヤーのセット
    /// </summary>
    /// <param name="value">プレイヤーHP(1f～0f)</param>
    public void SetPlayerHp(float value)
    {
        _playerHpGauge.value = value;
    }

    /// <summary>
    /// 攻撃カウントのセット
    /// </summary>
    /// <param name="value">攻撃カウント(1f～0f)</param>
    public void SetEnemyAttack(float value)
    {
        _enemyAttackGauge.value = value;
    }
}
