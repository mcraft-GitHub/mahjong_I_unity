using System;
using System.Net.NetworkInformation;
using UnityEngine;

public class MahjongLogic
{
    // 牌種
    public enum TILE_KIND
    {
        NONE = -1,
        MAN_1 = 0,
        MAN_2,
        MAN_3,
        MAN_4,
        MAN_5,
        MAN_6,
        MAN_7,
        MAN_8,
        MAN_9,
        PIN_1,
        PIN_2,
        PIN_3,
        PIN_4,
        PIN_5,
        PIN_6,
        PIN_7,
        PIN_8,
        PIN_9,
        SOO_1,
        SOO_2,
        SOO_3,
        SOO_4,
        SOO_5,
        SOO_6,
        SOO_7,
        SOO_8,
        SOO_9,
        TON,
        NAN,
        SYA,
        PEE,
        HAKU,
        HATU,
        TYUN,
        MAX,
    }

    /// <summary>
    /// 萬子、筒子、索子、字牌の判定
    /// </summary>
    /// <param name="k">牌種</param>
    /// <returns>萬子:1、筒子:2、索子:3、字牌:4</returns>
    static public int CalcMPST(TILE_KIND k)
    {
        if (k == TILE_KIND.NONE)
            return 0;

        int nk = (int)k;
        if (nk <= (int)TILE_KIND.PIN_9)
        {
            if (nk <= (int)TILE_KIND.MAN_9)
                return 1;
            else
                return 2;
        }
        else
        {
            if (nk <= (int)TILE_KIND.SOO_9)
                return 3;
            else
                return 4;
        }
    }

    /// <summary>
    /// 面子かどうかの判定
    /// </summary>
    /// <param name="k1">牌種1</param>
    /// <param name="k2">牌種2</param>
    /// <param name="k3">牌種3</param>
    /// <returns>面子か</returns>
    public static bool CheckMentu(TILE_KIND k1, TILE_KIND k2, TILE_KIND k3)
    {
        // 種類判定
        int[] mpst = { CalcMPST(k1), CalcMPST(k2), CalcMPST(k3) };

        if (mpst[0] != mpst[1] || mpst[0] != mpst[2] || mpst[0] == 0)
            return false;

        // 刻子
        if (k1 == k2 && k1 == k3)
            return true;

        // 字牌の場合は順子がないので終了
        if (mpst[0] == 4)
            return false;

        // 小さい順に並び変え
        int[] nks = { (int)k1, (int)k2, (int)k3 };
        Array.Sort(nks);

        // 1づつ増えていれば順子
        if (nks[0] == nks[1] - 1 && nks[0] == nks[2] - 2)
            return true;

        return false;
    }
}
