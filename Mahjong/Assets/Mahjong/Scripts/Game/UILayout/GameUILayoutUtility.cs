using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using static CommonUtility;

public static class GameUILayoutUtility
{
    // ***** Public変数
    // 手牌・パズル牌の最終サイズ(基本サイズ x スケール)
    static public Vector2 _handTilesFinalSize = Vector2.zero;
    static public Vector2 _puzzleTilesFinalSize = Vector2.zero;

    // 手牌・パズル牌のスケール
    static public float _handTilesScale = 2.0f;
    static public float _puzzleTilesScale = 3.0f;

    // パズル牌の見た目スケール
    static public float _puzzleTilesViewScale = 1.0f;

    // 並べられたパズル牌の4隅座標
    static public Rect _puzzleBoardRect = Rect.zero;

    // パズル盤面の枠画像スケール
    static public float _puzzleFrameScale = 1.0f;

    // 補助表示牌のY座標
    static public float _auxiliaryTilesPosY = 0.0f;

    // 自風表示牌のX座標
    static public float _jikazeTilesPosX = 0.0f;

    // ドラ表示牌のX座標
    static public float _doraTilesPosX = 0.0f;

    // 雀頭表示牌のX座標
    static public float[] _headTilesPosX = { 0.0f, 0.0f };

    // 割合計算済みプレイヤーHPゲージの縦のサイズ
    static public float _calcPlayerHpGaugeHeight = 24.0f;

    // 割合計算済み敵HPゲージの縦のサイズ
    static public float _calcEnemyHpGaugeHeight = 42.0f;

    // 割合計算済み敵攻撃ゲージの縦のサイズ
    static public float _calcEnemyAttackGaugeHeight = 24.0f;

    // バトルゲージの幅のサイズ
    static public float _gaugeWidth = 0.0f;

    // プレイヤーHPゲージのY座標
    static public float _playerHpGaugePosY = 0.0f;

    // 敵HPゲージのY座標
    static public float _enemyHpGaugePosY = 0.0f;

    // 敵攻撃ゲージのY座標
    static public float _enemyAttackGaugePosY = 0.0f;

    // 敵画像のサイズ
    static public float _enemyImageSize = 1.0f;

    // 敵画像のY座標
    static public float _enemyImagePosY = 0.0f;

    // ***** Private変数
    // タイルの基本サイズ
    static private readonly Vector2 TILE_SIZE = new Vector2(47.0f, 63.0f);

    // ゲームのUIレイアウトデータ
    static private GameUILayoutData _data;

    // 既に計算を行っているか(一度だけでいいからね)
    static private bool _isAlreadyCalc = false;

    // 基準画面サイズと実画面サイズの割合
    static private float _screenRate = 1.0f;

    // パズルUI部分の合計の高さ
    static private float _uiHeight = 0.0f;

    // 並べられた手牌・パズル牌の横の余白(最低限の余白 + 余白 = 最終余白)
    static private float _handTilesMargin = 0.0f;
    static private float _puzzleTilesMargin = 0.0f;

    // 割合計算済み画面上の空白のサイズ
    static private float _calcTopSafeBlank = 64.0f;

    // 割合計算済み画面下の空白のサイズ
    static private float _calcButtomSafeBlank = 36.0f;

    // 割合計算済み画面左右の空白のサイズ
    static private float _calcSideSafeBlank = 24.0f;

    // 割合計算済みUI同士の縦の空白のサイズ
    static private float _calcHeightBlank = 24.0f;

    // 割合計算済みパズル盤面の枠の幅のサイズ
    static private float _calcPuzzleBlank = 32.0f;

    // 割合計算済み自風牌とドラ牌の隙間のサイズ
    static private float _calcJikazeDoraBlank = 32.0f;

    // パズル牌の基本位置(0,0)
    static public Vector2 _puzzleTileBasePos = Vector2.zero;


    // ***** Public関数
    /// <summary>
    /// ゲーム部のUIのレイアウト計算
    /// </summary>
    static public void CalcUILayout()
    {
        // 既に計算を行っていれば終了
        if (_isAlreadyCalc)
            return;

        // GameUILayoutDataの読み込み
        _data = Resources.Load<GameUILayoutData>("GameUILayoutData");
        if (_data == null)
        {
            Debug.LogError("GameUILayoutDataの読み込みに失敗！");
            return;
        }

        // 基準画面サイズと実画面サイズの割合の計算
        _screenRate = (float)Screen.height / _data.BaseScreenHeight;

        // UIサイズの計算
        CalcUISizeWithScreenRate();

        // 牌サイズの計算
        CalcTileScaleAndMargin();

        // バトルUIのレイアウトを計算する
        CalcBattleUILayout();

        // 並べられたパズル牌の4隅座標を計算
        _puzzleBoardRect = new Rect(
            _puzzleTilesFinalSize.x * GameData.PUZZLE_BOARD_SIZE_X * -HALF,
            _calcPuzzleBlank + _calcHeightBlank + _calcPlayerHpGaugeHeight + _calcHeightBlank + _calcHeightBlank + _calcButtomSafeBlank + _handTilesFinalSize.y * DOUBLE,
            _puzzleTilesFinalSize.x * GameData.PUZZLE_BOARD_SIZE_X, _puzzleTilesFinalSize.y * GameData.PUZZLE_BOARD_SIZE_Y);

        // パズル牌の基本位置(0,0)を計算
        _puzzleTileBasePos = new Vector2(_puzzleBoardRect.xMin + _puzzleTilesFinalSize.x * HALF, _puzzleBoardRect.yMax + _puzzleTilesFinalSize.y * -HALF);

        // パズル盤面の枠画像スケールを計算
        _puzzleFrameScale = GameData.PUZZLE_BOARD_SIZE_Y * _puzzleTilesScale + (_calcPuzzleBlank / _puzzleTilesFinalSize.y);

        // 計算したのでフラグをtrueに
        _isAlreadyCalc = true;
    }

    /// <summary>
    /// パズル牌の添え字から画面上の座標を計算する
    /// </summary>
    /// <param name="index">計算牌の盤面インデックス</param>
    /// <returns>画面上の座標</returns>
    static public Vector2 CalcPuzzleTilePosFromIndex(Vector2Int index)
    {
        return new Vector2(_puzzleTileBasePos.x + index.x * _puzzleTilesFinalSize.x, _puzzleTileBasePos.y - index.y * _puzzleTilesFinalSize.y);
    }

    /// <summary>
    /// 手牌の数(添え字)から画面上の座標を計算する
    /// </summary>
    /// <param name="index">計算牌の手牌インデックス</param>
    /// <returns>画面上の座標</returns>
    static public Vector2 CalcHandTilePosFromIndex(int index)
    {
        // 画面の左端
        float screanLeftEnd = Screen.width * -0.5f;
        // 左の空白の幅
        float leftMargin = _calcSideSafeBlank + _handTilesMargin;

        return new Vector2(screanLeftEnd + leftMargin + _handTilesFinalSize.x * (HALF + index), _calcButtomSafeBlank + _handTilesFinalSize.y * HALF);
    }

    // ***** Private関数
    /// <summary>
    /// 各種UIサイズを画面の割合に適応させる
    /// </summary>
    static private void CalcUISizeWithScreenRate()
    {
        _calcTopSafeBlank           = _data.TopSafeBlank            * _screenRate;
        _calcButtomSafeBlank        = _data.ButtomSafeBlank         * _screenRate;
        _calcSideSafeBlank          = _data.SideSafeBlank           * _screenRate;
        _calcHeightBlank            = _data.HeightBlank             * _screenRate;
        _calcPuzzleBlank            = _data.PuzzleBlank             * _screenRate;
        _calcPlayerHpGaugeHeight    = _data.PlayerHpGaugeHeight     * _screenRate;
        _calcEnemyHpGaugeHeight     = _data.EnemyHpGaugeHeight      * _screenRate;
        _calcEnemyAttackGaugeHeight = _data.EnemyAttackGaugeHeight  * _screenRate;
        _calcJikazeDoraBlank        = _data.JikazeDoraBlank         * _screenRate;
    }

    /// <summary>
    /// 画面サイズから、牌のスケールと余白を計算する
    /// </summary>
    static private void CalcTileScaleAndMargin()
    {
        _handTilesMargin = 0.0f;
        _puzzleTilesMargin = 0.0f;

        //*** 画面の横サイズから牌のサイズを求める
        // 横並びの牌の合計幅(余白を引いた画面幅)
        float totalHandTileWidth = Screen.width - (_calcSideSafeBlank + _handTilesMargin) * 2.0f;
        float totalPuzzleTileWidth = Screen.width - (_calcSideSafeBlank + _puzzleTilesMargin + _calcPuzzleBlank) * 2.0f;

        // 求めた合計幅を(牌の数x牌の基本サイズ幅)で割り、暫定的な牌のスケールを求める
        _handTilesScale = totalHandTileWidth / (TILE_SIZE.x * GameData.HAND_TILES_NUM);
        _puzzleTilesScale = totalPuzzleTileWidth / (TILE_SIZE.x * GameData.PUZZLE_BOARD_SIZE_X);

        // 牌のサイズを計算
        _handTilesFinalSize = TILE_SIZE * _handTilesScale;
        _puzzleTilesFinalSize = TILE_SIZE * _puzzleTilesScale;

        //*** 求めた牌のサイズからパズルUI部分の高さを求める
        // 牌を除いたパズルUI部分の高さ
        float noTilesUIHeight = _calcPuzzleBlank + _calcPuzzleBlank + _calcHeightBlank + _calcPlayerHpGaugeHeight + _calcHeightBlank + _calcHeightBlank + _calcButtomSafeBlank;

        // パズルUI部分の高さ
        _uiHeight = _handTilesFinalSize.y * DOUBLE + _puzzleTilesFinalSize.y * GameData.PUZZLE_BOARD_SIZE_Y + noTilesUIHeight;

        // パズルUI部分の縦の割合が最大値を超えていなければ確定
        if (_uiHeight <= Screen.height * _data.MaxHeightUiRate)
        {
            // 表示時のパズル牌のサイズを計算
            _puzzleTilesViewScale = _puzzleTilesScale * (1.0f - _data.PuzzleTileMarginRate);
            return;
        }

        //*** サイズを調整する
        // 1つの牌につき、どのくらい削るかを計算する
        float cutHeight = (_uiHeight - Screen.height * _data.MaxHeightUiRate) / (GameData.PUZZLE_BOARD_SIZE_Y + DOUBLE) / TILE_SIZE.y;

        // 牌のサイズの決定
        _handTilesScale -= cutHeight;
        _puzzleTilesScale -= cutHeight;

        // 牌のサイズを計算
        _handTilesFinalSize = TILE_SIZE * _handTilesScale;
        _puzzleTilesFinalSize = TILE_SIZE * _puzzleTilesScale;

        // 表示時のパズル牌のサイズを計算
        _puzzleTilesViewScale = _puzzleTilesScale * (1.0f - _data.PuzzleTileMarginRate);

        // 横の余白の決定
        _handTilesMargin = (Screen.width - _handTilesFinalSize.x * GameData.HAND_TILES_NUM) * HALF - _calcSideSafeBlank;
        _puzzleTilesMargin = (Screen.width - _puzzleTilesFinalSize.x * GameData.PUZZLE_BOARD_SIZE_X) * HALF - _calcSideSafeBlank - _calcPuzzleBlank;

        // パズルUI部分の高さ
        _uiHeight = _handTilesFinalSize.y * DOUBLE + _puzzleTilesFinalSize.y * GameData.PUZZLE_BOARD_SIZE_Y + noTilesUIHeight;
    }

    /// <summary>
    /// バトルUIのレイアウトを計算する
    /// </summary>
    static private void CalcBattleUILayout()
    {
        // 画面の左端
        float screanLeftEnd = Screen.width * -0.5f;

        // 横の空白の幅
        float sideMargin = _calcSideSafeBlank + _handTilesMargin;

        // 手牌のハーフサイズ
        Vector2 handTilesFinalHalfSize = _handTilesFinalSize * HALF;

        //*** 補助表示牌の座標を計算
        // Y座標
        _auxiliaryTilesPosY = _calcButtomSafeBlank + _calcHeightBlank + _handTilesFinalSize.y + handTilesFinalHalfSize.y;
        // X座標
        _jikazeTilesPosX = screanLeftEnd + sideMargin + handTilesFinalHalfSize.x;
        _doraTilesPosX = screanLeftEnd + sideMargin + handTilesFinalHalfSize.x + _handTilesFinalSize.x + _calcJikazeDoraBlank;
        _headTilesPosX[0] = -screanLeftEnd + -sideMargin + -handTilesFinalHalfSize.x;
        _headTilesPosX[1] = -screanLeftEnd + -sideMargin + -handTilesFinalHalfSize.x + -_handTilesFinalSize.x;

        //*** バトルゲージの幅とY座標を計算
        // 幅
        _gaugeWidth = _handTilesFinalSize.x * GameData.HAND_TILES_NUM;
        // Y座標
        _playerHpGaugePosY = _calcButtomSafeBlank + _handTilesFinalSize.y * DOUBLE + _calcHeightBlank * DOUBLE + _calcPlayerHpGaugeHeight * HALF;
        _enemyHpGaugePosY = -_calcTopSafeBlank - _calcEnemyHpGaugeHeight * HALF;
        _enemyAttackGaugePosY = -_calcTopSafeBlank - _calcEnemyHpGaugeHeight - _calcEnemyAttackGaugeHeight * HALF;

        //*** 敵画像のサイズとY座標を計算
        // サイズ
        _enemyImageSize = Screen.height - _uiHeight - _calcTopSafeBlank;
        // Y座標
        _enemyImagePosY = -_calcTopSafeBlank - _enemyImageSize * HALF;
    }
}
