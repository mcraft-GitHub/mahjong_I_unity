using UnityEngine;

public class GameData
{
    // ***** Public変数
    // パズルの縦の牌の数
    static public readonly int PUZZLE_BOARD_SIZE_X = 8;
    // パズルの横の牌の数
    static public readonly int PUZZLE_BOARD_SIZE_Y = 6;

    // 画面の縦サイズにおける、パズル + 取得牌表示の最大割合
    static public readonly float MAX_HEIGHT_UI_RATE = 0.65f;

    // 画面下の空白のサイズ
    static public readonly float BUTTOM_SAFE_BLANK = 36.0f;

    // 縦の空白のサイズ
    static public readonly float HEIGHT_BLANK = 24.0f;

    // パズル盤面の枠の幅のサイズ
    static public readonly float PUZZLE_BLANK = 32.0f;

    // HPゲージの縦のサイズ
    static public readonly float HP_GAUGE_BLANK = 36.0f;

    // タイルの横の最低限の空白
    static public readonly float MINIMUM_BLANK = 24.0f;

    // パズルタイルの周りの空白割合
    static public readonly float PUZZLE_TILE_MARGIN_RATE = 0.05f;

    // タイルの基本サイズ
    static public readonly Vector2 TILE_SIZE = new Vector2(47.0f, 63.0f);

    // 獲得・パズルタイルのスケール(基本サイズ x スケール = 最終サイズ)
    static public float acquiredTilesScale = 2.0f;
    static public float puzzleTilesScale = 3.0f;

    // 並べられた獲得・パズルタイルの横の余白(最低限の余白 + 余白 = 最終余白)
    static public float acquiredTilesMargin = 0.0f;
    static public float puzzleTilesMargin = 0.0f;

    // UI部分の高さ
    static public float uiHeight = 0.0f;

    // ***** Private変数
    // UI部分の合計固定サイズ(UI部分の高さ - 麻雀牌)(パズル枠 + パズル枠 + 縦空白 + 縦空白 + 縦空白 + 画面下空白)
    static private readonly float TOTAL_FIXED_UI_HEIGHT = PUZZLE_BLANK * 2.0f + HEIGHT_BLANK * 3.0f + BUTTOM_SAFE_BLANK;

    // ***** Public関数
    // 画面サイズから、タイルのスケールと余白を計算する
    static public void CalcTileScaleAndMargin()
    {
        int width = Screen.width;
        int height = Screen.height;

        acquiredTilesMargin = 0.0f;
        puzzleTilesMargin = 0.0f;

        // 画面の横サイズから牌の大きさを求める
        acquiredTilesScale = (width - (MINIMUM_BLANK + acquiredTilesMargin) * 2.0f) / (12.0f * TILE_SIZE.x);
        puzzleTilesScale = (width - (MINIMUM_BLANK + puzzleTilesMargin + PUZZLE_BLANK) * 2.0f) / (PUZZLE_BOARD_SIZE_X * TILE_SIZE.x);

        // 求めた牌の大きさからUI部分の高さを求める
        uiHeight = (acquiredTilesScale * TILE_SIZE.y * 2.0f) + (puzzleTilesScale * TILE_SIZE.y * PUZZLE_BOARD_SIZE_Y) + TOTAL_FIXED_UI_HEIGHT;

        // UI部分の縦の割合が最大値を超えていなければ確定
        if (uiHeight <= height * MAX_HEIGHT_UI_RATE)
            return;

        // 1つの牌につき、どのくらい削るのかを計算する
        float cutHeight = (uiHeight - height * MAX_HEIGHT_UI_RATE) / (PUZZLE_BOARD_SIZE_Y + 2.0f) / TILE_SIZE.y;

        // 牌の大きさの決定
        acquiredTilesScale -= cutHeight;
        puzzleTilesScale -= cutHeight;

        // 横の余白の決定
        acquiredTilesMargin = (width - TILE_SIZE.x * acquiredTilesScale * 12.0f) * 0.5f - MINIMUM_BLANK;
        puzzleTilesMargin = (width - TILE_SIZE.x * puzzleTilesScale * 8.0f) * 0.5f - MINIMUM_BLANK - PUZZLE_BLANK;
        
        // 全体の高さ * MAX_HEIGHT_UI_RATO のはずだけど一応計算
        uiHeight = (acquiredTilesScale * TILE_SIZE.y * 2.0f) + (puzzleTilesScale * TILE_SIZE.y * PUZZLE_BOARD_SIZE_Y) + TOTAL_FIXED_UI_HEIGHT;
    }
}
