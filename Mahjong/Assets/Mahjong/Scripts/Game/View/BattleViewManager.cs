using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class BattleViewManager : MonoBehaviour
{
    // 画面フェードの時間
    private const float SCREEN_FADE_TIME = 1.0f;

    // 手牌の移動時間
    public static readonly float HAND_TILE_MOVE_TIME = 0.3f;

    // パズルリザルトの背景のアルファ値
    private const float ROLE_RESULT_BACKGROUND_ALPHA = 0.9f;

    // フェード時間
    private const float FADE_TIME = 0.5f;
    // フェード後役を表示し始めるまでの時間
    private const float BEGIN_DRAW_ROLE_DELAY = 0.1f;
    // 役表示間隔時間
    private const float DRAW_ROLE_DELAY = 0.5f;
    // 表示後消し始めるまでの時間
    private const float CLEAR_ROLE_RESULT_TIME = 2.0f;

    // ゲージの速度
    private const float GAUGE_MOVE_SPEED = 2.0f;

    // フェード
    [SerializeField] private Image _fadeImage;

    // 麻雀牌プレハブ
    [SerializeField] private GameObject _tilePrefab;

    // 手牌の親オブジェクトTransform
    [SerializeField] private Transform _handTilesParent;
    // 空の手牌の親オブジェクトTransform
    [SerializeField] private Transform _emptyHandTilesParent;

    // ドラ
    [SerializeField] private MahjongTileView _doraTile;
    // 自風
    [SerializeField] private MahjongTileView _jikazeTile;
    // 雀頭
    [SerializeField] private MahjongTileView _headTile1;
    [SerializeField] private MahjongTileView _headTile2;

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

    // 手牌オブジェクト
    private List<MahjongTileView> _handTileObjects = new List<MahjongTileView>();

    void Start()
    {
        //*** 雀頭牌とドラ牌と自風牌の配置・拡縮
        _headTile1.SetPos(new Vector2(GameUILayoutUtility._headTilesPosX[0], GameUILayoutUtility._auxiliaryTilesPosY));
        _headTile1.SetScale(GameUILayoutUtility._handTilesScale);
        _headTile2.SetPos(new Vector2(GameUILayoutUtility._headTilesPosX[1], GameUILayoutUtility._auxiliaryTilesPosY));
        _headTile2.SetScale(GameUILayoutUtility._handTilesScale);
        _doraTile.SetPos(new Vector2(GameUILayoutUtility._doraTilesPosX, GameUILayoutUtility._auxiliaryTilesPosY));
        _doraTile.SetScale(GameUILayoutUtility._handTilesScale);
        _jikazeTile.SetPos(new Vector2(GameUILayoutUtility._jikazeTilesPosX, GameUILayoutUtility._auxiliaryTilesPosY));
        _jikazeTile.SetScale(GameUILayoutUtility._handTilesScale);

        //*** プレイヤーのHPゲージの配置・拡縮
        RectTransform playerHpRect = _playerHpGauge.GetComponent<RectTransform>();
        playerHpRect.anchoredPosition = new Vector2(0.0f, GameUILayoutUtility._playerHpGaugePosY);
        playerHpRect.sizeDelta = new Vector2(GameUILayoutUtility._gaugeWidth, GameUILayoutUtility._calcPlayerHpGaugeHeight);

        //*** 敵画像の配置・拡縮
        RectTransform enemyImageRect = _enemyImage.GetComponent<RectTransform>();
        enemyImageRect.sizeDelta = new Vector2(GameUILayoutUtility._enemyImageSize, GameUILayoutUtility._enemyImageSize);
        enemyImageRect.anchoredPosition = new Vector2(0.0f, GameUILayoutUtility._enemyImagePosY);

        //*** 敵UIの配置・拡縮
        RectTransform enemyHpRect = _enemyHpGauge.GetComponent<RectTransform>();
        RectTransform enemyAttackRect = _enemyAttackGauge.GetComponent<RectTransform>();
        enemyHpRect.sizeDelta = new Vector2(GameUILayoutUtility._gaugeWidth, GameUILayoutUtility._calcEnemyHpGaugeHeight);
        enemyAttackRect.sizeDelta = new Vector2(GameUILayoutUtility._gaugeWidth, GameUILayoutUtility._calcEnemyAttackGaugeHeight);
        enemyHpRect.anchoredPosition = new Vector2(0.0f, GameUILayoutUtility._enemyHpGaugePosY);
        enemyAttackRect.anchoredPosition = new Vector2(0.0f, GameUILayoutUtility._enemyAttackGaugePosY);

        //*** 空の手牌の生成
        for (int i = 0; i < GameData.HAND_TILES_NUM; i++)
        {
            // 生成
            GameObject obj = Instantiate(_tilePrefab, _emptyHandTilesParent);
            MahjongTileView tile = obj.GetComponent<MahjongTileView>();
            tile.SetPos(GameUILayoutUtility.CalcHandTilePosFromIndex(i));
            tile.SetScale(GameUILayoutUtility._handTilesScale);
            // 牌類のセット
            tile.SetKind(MahjongLogic.TILE_KIND.NONE);
        }
    }

    /// <summary>
    /// ゲージの初期化
    /// </summary>
    public void InitUIGauge()
    {
        _playerHpGauge.value = 1.0f;
        _enemyHpGauge.value = 1.0f;
        _enemyAttackGauge.value = 1.0f;
    }

    /// <summary>
    /// フェードインを開始
    /// </summary>
    /// <returns>フェードにかかる時間</returns>
    public float BeginFadeIn()
    {
        _fadeImage.color = Color.black;
        _fadeImage.DOColor(Color.clear, SCREEN_FADE_TIME);
        return SCREEN_FADE_TIME;
    }


    /// <summary>
    /// フェードアウトを開始
    /// </summary>
    /// <returns>フェードにかかる時間</returns>
    public float BeginFadeOut()
    {
        _fadeImage.color = Color.clear;
        _fadeImage.DOColor(Color.black, SCREEN_FADE_TIME);
        return SCREEN_FADE_TIME;
    }

    /// <summary>
    /// ゲーム開始カウントダウンのセット
    /// </summary>
    /// <param name="cnt">カウント, -1なら表示消去</param>
    public void SetBeginGameCount(int cnt)
    {
        int value;
        if (cnt <= -1)
            _beginCntText.text = "";
        else if (!int.TryParse(_beginCntText.text, out value) || cnt != value)
        {
            _beginCntText.text = cnt.ToString();
        }
    }

    /// <summary>
    /// 敵HPのセット
    /// </summary>
    /// <param name="value">敵HP(1f～0f)</param>
    public void SetEnemyHp(float value)
    {
        // 敵HPの減少値(ダメージ)を計算
        float enemyDamage = _enemyHpGauge.value - value;

        // 敵HPゲージの更新
        _enemyHpGauge.DOValue(value, enemyDamage * GAUGE_MOVE_SPEED);
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
    /// 敵の攻撃ゲージ更新・攻撃
    /// </summary>
    /// <param name="enemyAttackRate">敵の攻撃ゲージ割合</param>
    /// <param name="isEnemtAttack">敵が攻撃したか</param>
    /// <param name="playerHpRate">プレイヤーのHP割合</param>
    public void EnemyAttackUpdate(float enemyAttackRate, bool isEnemtAttack, float playerHpRate)
    {
        // 敵の攻撃ゲージの更新
        _enemyAttackGauge.value = enemyAttackRate;

        // 敵が攻撃したら
        if (isEnemtAttack)
        {
            // TODO: 攻撃の演出

            // プレイヤーHPの減少値(ダメージ)を計算
            float playerDamage = _playerHpGauge.value - playerHpRate;

            // プレイヤーHPゲージの更新
            _playerHpGauge.DOValue(playerHpRate, playerDamage * GAUGE_MOVE_SPEED);
        }
    }

    /// <summary>
    /// 手牌の追加・役演出
    /// </summary>
    /// <param name="handTilesKindList">手牌の牌種リスト</param>
    /// <param name="tilesIndex">追加牌の盤面インデックス</param>
    /// <param name="playerAttackData">プレイヤーの攻撃データ</param>
    /// <returns>演出時間</returns>
    public float AddHandTiles(List<MahjongLogic.TILE_KIND> handTilesKindList, Vector2Int[] tilesIndex, BattleManager.PlayerAttackData playerAttackData)
    {
        // 中,左,右の順番で格納されているので自然な順番にする
        int[] index = { 1, 0, 2 };

        for (int i = 0; i < tilesIndex.Length; i++)
        {
            // 手牌の中でのインデックス
            int handIdx = handTilesKindList.Count - (GameData.MENTU_TILES_NUM - index[i]) - (playerAttackData != null ? GameData.HEAD_TILES_NUM : 0);

            // 手牌の生成
            GameObject obj = Instantiate(_tilePrefab, _handTilesParent);
            MahjongTileView tile = obj.GetComponent<MahjongTileView>();
            _handTileObjects.Add(tile);
            // 元の場所に生成
            tile.SetPos(GameUILayoutUtility.CalcPuzzleTilePosFromIndex(tilesIndex[index[i]].x, tilesIndex[index[i]].y));
            tile.SetScale(GameUILayoutUtility._puzzleTilesViewScale);
            // 牌類のセット
            tile.SetKind(handTilesKindList[handIdx]);

            // 手牌に移動・縮小
            tile.SetPos(GameUILayoutUtility.CalcHandTilePosFromIndex(handIdx), HAND_TILE_MOVE_TIME);
            tile.SetScale(GameUILayoutUtility._handTilesScale);
        }

        // 手牌(役)完成していたら
        if (playerAttackData != null)
        {
            // 手牌(役)完成演出
            StartCoroutine(ShowRoleResultCoroutine(playerAttackData._role, playerAttackData._damage));

            // フェード時間(2回分)
            float fadeTime = FADE_TIME + FADE_TIME;
            // 役表示間隔時間(役の数分)
            float roleTime = (playerAttackData._role.roleKinds.Count + (playerAttackData._role.dora > 0 ? 1 : 0)) * DRAW_ROLE_DELAY;

            //        手牌移動時間     フェード時間   最初の役表示間隔       役表示       役表示削除間隔
            return HAND_TILE_MOVE_TIME + fadeTime + BEGIN_DRAW_ROLE_DELAY + roleTime + CLEAR_ROLE_RESULT_TIME;
        }
        else
        {
            // 手牌移動時間
            return HAND_TILE_MOVE_TIME;
        }
    }

    /// <summary>
    /// 手牌の削除
    /// </summary>
    public void ClearHandTiles()
    {
        for (int i = 0; i < _handTileObjects.Count; i++)
        {
            Destroy(_handTileObjects[i].gameObject);
        }
        _handTileObjects.Clear();
    }

    /// <summary>
    /// ドラと雀頭と自風の牌種の設定
    /// </summary>
    /// <param name="dora">ドラの牌種</param>
    /// <param name="head">雀頭の牌種</param>
    /// <param name="jikazeCnt">自風のカウント</param>
    public void SetDoraHeadJikazeKind(MahjongLogic.TILE_KIND dora, MahjongLogic.TILE_KIND head, int jikazeCnt)
    {
        _doraTile.SetKind(dora);
        _headTile1.SetKind(head);
        _headTile2.SetKind(head);
        _jikazeTile.SetKind((MahjongLogic.TILE_KIND)((int)MahjongLogic.TILE_KIND.TON + jikazeCnt));
    }

    /// <summary>
    /// 役攻撃演出コルーチン
    /// </summary>
    /// <param name="role">役情報</param>
    /// <param name="damage">ダメージ</param>
    private IEnumerator ShowRoleResultCoroutine(MahjongLogic.Role role, int damage)
    {
        // 手牌移動時間
        yield return new WaitForSeconds(HAND_TILE_MOVE_TIME);

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
