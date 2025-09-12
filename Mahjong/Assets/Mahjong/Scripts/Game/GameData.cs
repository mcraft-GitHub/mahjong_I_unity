using UnityEngine;

public class GameData
{
    // ***** Public変数
    // パズルの縦の牌の数
    public static readonly int PUZZLE_BOARD_SIZE_X = 8;
    // パズルの横の牌の数
    public static readonly int PUZZLE_BOARD_SIZE_Y = 6;

    // 手牌の数(雀頭抜き)
    public static readonly int HAND_TILES_NUM = 12;
    // 雀頭の牌の数
    public static readonly int HEAD_TILES_NUM = 2;

    // 面子の牌の数
    public static readonly int MENTU_TILES_NUM = 3;

    // 最大の面子の数
    public static readonly int MAX_MENTU_NUM = 4;

    // ゲーム開始カウントダウン
    public static readonly float BEGIN_GAME_COUNTDOWN_TIME = 4.0f;

    // 属性
    public enum ELEMENTAL
    {
        FIRE = 0,   // 萬子(火)
        WATER,      // 筒子(水)
        WOOD,       // 索子(木)
        VOID,       // 風牌(無)
        OCEAN,      // 白(淼)
        FOREST,     // 發(森)
        FLAME,      // 中(焱)
    }

    // 属性相性ダメージ(通常)
    public static readonly float ELEMENTAL_AFFINITY_DAMAGE_DEFAULT = 1.0f;
    // 属性相性ダメージ(抜群)
    public static readonly float ELEMENTAL_AFFINITY_DAMAGE_GOOD = 1.6f;
    // 属性相性ダメージ(超抜群)
    public static readonly float ELEMENTAL_AFFINITY_DAMAGE_GREAT = 2.5f;
    // 属性相性ダメージ(今一つ)
    public static readonly float ELEMENTAL_AFFINITY_DAMAGE_POOR = 0.625f;
    // 属性相性ダメージ(超今一つ)
    public static readonly float ELEMENTAL_AFFINITY_DAMAGE_AWFUL = 0.4f;
}
