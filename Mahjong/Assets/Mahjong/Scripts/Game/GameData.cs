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

    // BGMの標準切り替え時間
    public static readonly float BGM_SWITCHING_DEFAULT_TIME = 0.5f;

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
        MAX,
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

    // 平和の効果の倍率
    public static readonly float PINFU_MAGNIFICATION = 0.05f;
    // タンヤオの効果の倍率
    public static readonly float TANYAO_MAGNIFICATION = 1.25f;
    // 自風の効果の倍率
    public static readonly float KAZE_MAGNIFICATION = 5.0f;
    // 白の効果の倍率
    public static readonly float HAKU_MAGNIFICATION = 1.25f;
    // 發の効果の倍率
    public static readonly float HATU_MAGNIFICATION = 1.25f;
    // 中の効果の倍率
    public static readonly float TYUN_MAGNIFICATION = 1.25f;
    // 一盃口の効果の倍率
    public static readonly float IPEKO_MAGNIFICATION = 0.15f;
    // 三色同順の効果の倍率
    public static readonly float SANSYOKUDOUJUN_MAGNIFICATION = 0.7f;
    // 一通の効果の倍率
    public static readonly float ITTU_MAGNIFICATION = 1.5f;
    // チャンタの効果の倍率
    public static readonly float TYANTA_MAGNIFICATION = 0.8f;
    // 小三元の効果の倍率
    public static readonly float SYOSANGEN_MAGNIFICATION = 1.1f;
    // 三色同刻の効果の倍率
    public static readonly float SANSYOKUDOUKOU_MAGNIFICATION = 0.7f;
    // ホンイツの効果の倍率
    public static readonly float HONITU_MAGNIFICATION = 1.1f;
    // 二盃口の効果の倍率
    public static readonly float RYANPEKO_MAGNIFICATION = 0.25f;
    // 純チャンの効果の倍率
    public static readonly float JUNTYAN_MAGNIFICATION = 0.5f;
    // 清一色の効果の倍率
    public static readonly float TINITU_MAGNIFICATION = 1.25f;
    // 大三元の効果の倍率
    public static readonly float DAISANGEN_MAGNIFICATION = 1.5f;
    // 小四喜の効果の倍率
    public static readonly float SYOSUSI_MAGNIFICATION = 100.0f;
    // 大四喜の効果の倍率
    public static readonly float DAISUSI_MAGNIFICATION = 200.0f;
    // 九蓮宝燈の効果の倍率
    public static readonly float TYURENPOTO_MAGNIFICATION = 2.0f;
    // 清老頭の効果の倍率
    public static readonly float TINROTO_MAGNIFICATION = 3.0f;
}
