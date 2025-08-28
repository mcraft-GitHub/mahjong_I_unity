using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using NUnit.Framework;
using UnityEngine;
using static UnityEngine.InputManagerEntry;

public class MahjongLogic
{
    // 手牌の牌の数
    public static readonly int HAND_TILES_NUM = 14;

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

    // 役種
    // 使用しない役→リーチ,七対子,対々和,混老頭,三槓子,国士無双,天和,地和,四槓子,一発,ダブルリーチ,海底,河底,嶺上開花,槍槓,場風
    public enum ROLE_KIND
    {
        // 1ハン
        TUMO = 0,
        PINFU,
        TANYAO,
        KAZE,
        HAKU,
        HATU,
        TYUN,
        IPEKO,
        // 2ハン
        SANSYOKUDOUJUN,
        ITTU,
        TYANTA,
        SANANKO,
        SYOSANGEN,
        SANSYOKUDOUKOU,
        // 3ハン
        HONITU,
        RYANPEKO,
        JUNTYAN,
        // 6ハン
        TINITU,
        // 役満
        SUANKO,
        DAISANGEN,
        SYOSUSI,
        DAISUSI,
        TYURENPOTO,
        RYUISO,
        TUISO,
        TINROTO,
    }

    // 役名
    public static readonly string[] ROLE_NAME = { 
        // 1ハン
        "門前自摸",
        "平和",
        "断幺九",
        "自風牌",
        "白",
        "發",
        "中",
        "一盃口",
        // 2ハン
        "三色同順",
        "一気通貫",
        "混全帯幺九",
        "三暗刻",
        "小三元",
        "三色同刻",
        // 3ハン
        "混一色",
        "二盃口",
        "純全帯幺九",
        // 6ハン
        "清一色",
        // 役満
        "四暗刻",
        "大三元",
        "小四喜",
        "大四喜",
        "九蓮宝燈",
        "緑一色",
        "字一色",
        "清老頭",
    };

    // 役結果
    public struct Role
    {
        public List<ROLE_KIND> roleKinds;
        public int han;
        public int dora;
        public int fu;
    }

    // ハン数
    private int[] ROLE_HAN = { 
        1, 1, 1, 1, 1, 1, 1,
        2, 2, 2, 2, 2, 2, 2, 2,
        3, 3, 3, 6, 12, 12, 12, 12, 12, 12, 12
    };

    /// <summary>
    /// 面子かどうかの判定
    /// </summary>
    /// <param name="k1">牌種1</param>
    /// <param name="k2">牌種2</param>
    /// <param name="k3">牌種3</param>
    /// <returns>
    /// 0:面子不成立
    /// 1:順子
    /// 2:刻子
    /// </returns>
    public static int CheckMentu(TILE_KIND k1, TILE_KIND k2, TILE_KIND k3)
    {
        // 種類判定
        int[] mpst = { CalcMPST(k1), CalcMPST(k2), CalcMPST(k3) };

        if (mpst[0] != mpst[1] || mpst[0] != mpst[2] || mpst[0] == 0)
            return 0;

        // 刻子
        if (k1 == k2 && k1 == k3)
            return 2;

        // 字牌の場合は順子がないので終了
        if (mpst[0] == 4)
            return 0;

        // 小さい順に並び変え
        int[] nks = { (int)k1, (int)k2, (int)k3 };
        Array.Sort(nks);

        // 1づつ増えていれば順子
        if (nks[0] == nks[1] - 1 && nks[0] == nks[2] - 2)
            return 1;

        return 0;
    }

    /// <summary>
    /// 役判定
    /// </summary>
    /// <param name="hand">手牌の牌種リスト</param>
    /// <param name="dora">ドラの牌種</param>
    /// <param name="jikaze">自風の牌種</param>
    /// <returns>役判定結果</returns>
    public static Role CalcHandTilesRole(List<TILE_KIND> hand, TILE_KIND dora, TILE_KIND jikaze)
    {
        // 戻り値初期化
        Role role = new Role();
        role.roleKinds = new List<ROLE_KIND>();
        role.han = 0;
        role.dora = 0;
        role.fu = 0;

        //*** 手牌に不備があれば終了
        if (hand.Count != HAND_TILES_NUM)
            return role;
        for (int i = 0; i < HAND_TILES_NUM; i++)
        {
            if (hand[i] == TILE_KIND.NONE)
                return role;
        }

        // 計算用手牌
        List<TILE_KIND> calcHand = new List<TILE_KIND>(hand);
        // 頭候補
        List<TILE_KIND> head = new List<TILE_KIND>();
        // 種類(萬筒索字)ごと
        List<TILE_KIND>[] handMPST = Enumerable.Range(0, 4).Select(_ => new List<TILE_KIND>()).ToArray();

        //*** ソート(必須)
        hand.Sort();

        //*** 種類(萬筒索字)ごとにする
        for (int i = 0; i < HAND_TILES_NUM; i++) 
        {
            handMPST[CalcMPST(hand[i]) - 1].Add(hand[i]);
        }

        //*** 頭候補を決める
        for (int i = 1; i < HAND_TILES_NUM; i++)
        {
            if (hand[i] == hand[i - 1] && (head.Count == 0 || head[head.Count - 1] != hand[i]))
                head.Add(hand[i]);
        }

        //*** 面子候補を決める・役判定
        for (int i = 0; i < head.Count; i++)
        {
            // 面子の探索
            List<TILE_KIND[][]> mentu = CalcMentu(head[i], hand);

            // 同じ面子の組み合わせの削除
            mentu = RemoveDuplicateMentu(mentu);

            for (int j = 0; j < mentu.Count; j++)
            {
                // 役の判定
                Role tmpRole = CalcRole(head[i], mentu[j], dora, jikaze);

                // 比較
                if (role.han < tmpRole.han)
                {
                    role = tmpRole;
                }
                else if(role.han == tmpRole.han && role.fu < tmpRole.fu)
                {
                    role = tmpRole;
                }

                // デバッグ
                string headText = head[i] + ", " + head[i];

                string mentuText = "";
                for (int dm = 0; dm < 4; dm++)
                {
                    mentuText += mentu[j][dm][0] + ",";
                    mentuText += mentu[j][dm][1] + ",";
                    mentuText += mentu[j][dm][2] + ",";
                    mentuText += " / ";
                }
                Debug.Log(headText + " / " + mentuText);

                string roleText = " >>> " + tmpRole.han + "翻 " + tmpRole.fu + "符 / ";
                for (int d = 0; d < tmpRole.roleKinds.Count; d++)
                {
                    roleText += tmpRole.roleKinds[d] + ",";
                }
                if (tmpRole.dora > 0)
                    roleText += "DORA" + tmpRole.dora + ",";
                Debug.Log(roleText);
            }
        }

        return role;
    }

    /// <summary>
    /// 萬子、筒子、索子、字牌の判定
    /// </summary>
    /// <param name="k">牌種</param>
    /// <returns>萬子:1、筒子:2、索子:3、字牌:4</returns>
    static private int CalcMPST(TILE_KIND k)
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
    /// 面子候補の計算
    /// </summary>
    /// <param name="headKind">雀頭の牌種</param>
    /// <param name="hand">手牌</param>
    /// <returns>面子候補リスト</returns>
    private static List<TILE_KIND[][]> CalcMentu(TILE_KIND headKind, List<TILE_KIND> hand)
    {
        // 面子候補 { new TILE_KIND[3], new TILE_KIND[3], new TILE_KIND[3], new TILE_KIND[3] }
        List<TILE_KIND[][]> mentu = new List<TILE_KIND[][]>();

        // 頭をのぞいた手牌(削除に失敗したら、頭が2枚ないということなのでそのまま終了)
        List<TILE_KIND> calcHand = new List<TILE_KIND>(hand);
        if(!calcHand.Remove(headKind) || !calcHand.Remove(headKind))
        {
            Debug.Log("バグや！頭が二枚ない！");
            return mentu;
        }

        // 再帰処理で求める
        SearchMentu(mentu, new List<TILE_KIND>(calcHand), new List<TILE_KIND[]>());

        return mentu;
    }

    /// <summary>
    /// 面子の検索
    /// </summary>
    /// <param name="mentu">検索面子格納リスト</param>
    /// <param name="hand">手牌</param>
    /// <param name="checkKind">基準牌</param>
    /// <param name="calcMentu">検索済み面子</param>
    private static void SearchMentu(List<TILE_KIND[][]> mentu, List<TILE_KIND> hand, List<TILE_KIND[]> calcMentu)
    {
        // 総当たりで面子が完成する候補を考える
        for (int i = 0; i < hand.Count; i++)
        {
            // 前と同じ値ならスキップ
            if (i > 0 && hand[i] == hand[i - 1]) continue;

            for (int j = i + 1; j < hand.Count; j++)
            {
                if (j > i + 1 && hand[j] == hand[j - 1]) continue;

                for (int k = j + 1; k < hand.Count; k++)
                {
                    if (k > j + 1 && hand[k] == hand[k - 1]) continue;

                    // 面子チェック
                    if (CheckMentu(hand[i], hand[j], hand[k]) != 0)
                    {
                        // 手牌のコピーを作成
                        List<TILE_KIND> calcHand = new List<TILE_KIND>(hand);
                        // 面子に使用した牌を除外
                        calcHand.Remove(hand[i]);
                        calcHand.Remove(hand[j]);
                        calcHand.Remove(hand[k]);
                        // 面子の追加
                        List<TILE_KIND[]> nextMentu = new List<TILE_KIND[]>(calcMentu);
                        nextMentu.Add(new TILE_KIND[] { hand[i], hand[j], hand[k] });

                        // 全て調べ終わったら終了
                        if (calcHand.Count == 0)
                        {
                            // 追加
                            mentu.Add(nextMentu.ToArray());
                            continue;
                        }

                        // 次を調べる
                        SearchMentu(mentu, new List<TILE_KIND>(calcHand), nextMentu);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 同じ面子の組み合わせの削除
    /// </summary>
    /// <param name="mentu">削除前面子リスト</param>
    /// <returns>削除後面子リスト</returns>
    private static List<TILE_KIND[][]> RemoveDuplicateMentu(List<TILE_KIND[][]> mentu)
    {
        List<TILE_KIND[][]> uniqueMentu = new List<TILE_KIND[][]>();

        foreach (TILE_KIND[][] m in mentu)
        {
            bool isDuplicate = uniqueMentu.Any(u =>
                // u の面子と m の面子で 4つ以上一致しているか
                u.Count(um => m.Any(mm => um[0] == mm[0] && um[1] == mm[1] && um[2] == mm[2])) >= 4
            );

            if (!isDuplicate)
                uniqueMentu.Add(m);
        }

        return uniqueMentu;
    }

    /// <summary>
    /// 役の判定
    /// </summary>
    /// <param name="head">雀頭の牌種</param>
    /// <param name="mentu">面子の牌種[4面子][3牌]</param>
    /// <param name="dora">ドラの牌種</param>
    /// <param name="jikaze">自風の牌種</param>
    /// <returns>判定結果</returns>
    private static Role CalcRole(TILE_KIND head, TILE_KIND[][] mentu, TILE_KIND dora, TILE_KIND jikaze)
    {
        Role role = new Role();
        role.roleKinds = new List<ROLE_KIND>();
        role.han = 0;
        role.dora = 0;
        role.fu = 0;

        // 頭の萬,筒,索,字
        int headMPST = CalcMPST(head) - 1;
        // 萬,筒,索,字で分ける
        List<TILE_KIND[]>[] mpst = { new List<TILE_KIND[]>(), new List<TILE_KIND[]>(), new List<TILE_KIND[]>(), new List<TILE_KIND[]>() };
        for (int i = 0; i < 4; i++)
        {
            mpst[CalcMPST(mentu[i][0]) - 1].Add(mentu[i]);
        }
        // 順子,刻子で分ける
        List<TILE_KIND[]>[] syunko = { new List<TILE_KIND[]>(), new List<TILE_KIND[]>() };
        for (int i = 0; i < 4; i++)
        {
            if (mentu[i][0] != mentu[i][1])
                syunko[0].Add(mentu[i]);
            else
                syunko[1].Add(mentu[i]);
        }
        // 順子,刻子 > 萬,筒,索,字で分ける
        List<TILE_KIND[]>[][] syunkoMpst 
            = { new List<TILE_KIND[]>[] { new List<TILE_KIND[]>(), new List<TILE_KIND[]>(), new List<TILE_KIND[]>(), new List<TILE_KIND[]>() },
            new List<TILE_KIND[]>[] { new List<TILE_KIND[]>(), new List<TILE_KIND[]>(), new List<TILE_KIND[]>(), new List<TILE_KIND[]>() } };
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < syunko[i].Count; j++)
            {
                syunkoMpst[i][CalcMPST(syunko[i][j][0]) - 1].Add(syunko[i][j]);
            }
        }

        // *** 役満 ***
        //*** 四暗刻
        if (syunko[1].Count == 4)
        {
            role.roleKinds.Add(ROLE_KIND.SUANKO);
            role.han += 13;
        }

        //*** 大三元
        bool[] hht = { false, false, false };
        // 字牌面子が2組以上なければチェックしない(大三元的には3組だけど、後々小三元の判定にを使うから2組)
        if (mpst[3].Count >= 2)
        {
            // 白發中
            for (int i = 0; i < syunkoMpst[1][3].Count; i++)
            {
                TILE_KIND kind = syunkoMpst[1][3][i][0];
                // 三元牌チェック
                if (kind == TILE_KIND.HAKU || kind == TILE_KIND.HATU || kind == TILE_KIND.TYUN)
                {
                    hht[(int)syunkoMpst[1][3][i][0] - (int)TILE_KIND.HAKU] = true;
                }
            }
            if (hht[0] && hht[1] && hht[2])
            {
                role.roleKinds.Add(ROLE_KIND.DAISANGEN);
                role.han += 13;
            }
        }

        //*** 小四喜・大四喜
        // 字牌面子が3組以上なければチェックしない
        if (mpst[3].Count >= 3)
        {
            // 東南西北
            bool[] tnsp = { false, false, false, false };
            for (int i = 0; i < syunkoMpst[1][3].Count; i++)
            {
                TILE_KIND kind = syunkoMpst[1][3][i][0];
                // 東南西北チェック
                if (kind == TILE_KIND.TON || kind == TILE_KIND.NAN || kind == TILE_KIND.SYA || kind == TILE_KIND.PEE)
                {
                    tnsp[(int)kind - (int)TILE_KIND.TON] = true;
                }
            }
            // 面子の判定だけで行けたら大四喜
            if (tnsp[0] && tnsp[1] && tnsp[2] && tnsp[3])
            {
                role.roleKinds.Add(ROLE_KIND.DAISUSI);
                role.han += 13;
            }
            else
            {
                // 頭の東南西北チェック
                if (head == TILE_KIND.TON || head == TILE_KIND.NAN || head == TILE_KIND.SYA || head == TILE_KIND.PEE)
                {
                    tnsp[(int)head - (int)TILE_KIND.TON] = true;
                    // 小四喜
                    if (tnsp[0] && tnsp[1] && tnsp[2] && tnsp[3])
                    {
                        role.roleKinds.Add(ROLE_KIND.SYOSUSI);
                        role.han += 13;
                    }
                }
            }
        }

        //*** 九蓮宝燈
        // 頭と同じ色の牌が4面子か
        if (mpst[headMPST].Count == 4 && headMPST != 3)
        {
            // 数を数える
            int[] tyurenCnt = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            // 頭分を足す
            tyurenCnt[CalcNumber(head) - 1] += 2;
            for (int i = 0; i < mpst[headMPST].Count; i++)
            {
                for (int j = 0; j < mpst[headMPST][i].Length; j++)
                {
                    tyurenCnt[CalcNumber(mpst[headMPST][i][j]) - 1]++;
                }
            }
            // 判定
            if (tyurenCnt[0] >= 3 && tyurenCnt[1] >= 1 && tyurenCnt[2] >= 1 && tyurenCnt[3] >= 1 && tyurenCnt[4] >= 1 && 
                tyurenCnt[5] >= 1 && tyurenCnt[6] >= 1 && tyurenCnt[7] >= 1 && tyurenCnt[8] >= 3)
            {
                role.roleKinds.Add(ROLE_KIND.TYURENPOTO);
                role.han += 13;
            }
        }


        //*** 緑一色
        // 全部索子or字牌
        if (mpst[2].Count + mpst[3].Count == 4 && headMPST >= 2)
        {
            bool isGreen = true;
            // 順子
            for (int i = 0; i < syunkoMpst[0][2].Count; i++)
            {
                // 2索が最初の順子以外は終了
                if (syunkoMpst[0][2][i][0] != TILE_KIND.SOO_2)
                {
                    isGreen = false;
                    break;
                }
            }
            // 刻子
            for (int i = 0; i < syunkoMpst[1][2].Count && isGreen; i++)
            {
                TILE_KIND kind = syunkoMpst[1][2][i][0];
                // 2,3,4,6,8索以外は終了
                if (kind != TILE_KIND.SOO_2 && kind != TILE_KIND.SOO_3 && kind != TILE_KIND.SOO_4 && kind != TILE_KIND.SOO_6 && kind != TILE_KIND.SOO_8)
                {
                    isGreen = false;
                    break;
                }
            }
            // 雀頭
            if (isGreen && 
                (head == TILE_KIND.HATU || head == TILE_KIND.SOO_2 || head == TILE_KIND.SOO_3 || head == TILE_KIND.SOO_4 || head == TILE_KIND.SOO_6 || head == TILE_KIND.SOO_8))
            {
                // 字牌
                for (int i = 0; i < mpst[3].Count; i++)
                {
                    if (mpst[3][i][0] != TILE_KIND.HATU)
                    {
                        isGreen = false;
                        break;
                    }
                }

                // 緑一色
                if (isGreen)
                {
                    role.roleKinds.Add(ROLE_KIND.RYUISO);
                    role.han += 13;
                }
            }
        }

        //*** 字一色
        if (headMPST == 3 && mpst[3].Count == 4)
        {
            role.roleKinds.Add(ROLE_KIND.TUISO);
            role.han += 13;
        }

        //*** 清老頭
        // 大前提四暗刻
        if (syunko[1].Count == 4)
        {
            bool isTinroto = true;
            for (int i = 0; i < syunko[1].Count; i++)
            {
                if (CalcNumber(syunko[1][i][0]) != 1 && CalcNumber(syunko[1][i][0]) != 9)
                {
                    isTinroto = false;
                    break;
                }
            }
            if (isTinroto && (CalcNumber(head) == 1 || CalcNumber(head) == 9))
            {
                role.roleKinds.Add(ROLE_KIND.TINROTO);
                role.han += 13;
            }
        }

        // 役満を上がっていればここで終了
        if (role.han > 0)
            return role;


        // *** 役満以外 ***
        //*** ドラ
        if (head == dora)
        {
            role.han += 2;
            role.dora += 2;
        }
        for (int i = 0; i < mentu.Length; i++)
        {
            for (int j = 0; j < mentu[i].Length; j++)
            {
                if (mentu[i][j] == dora)
                {
                    role.han++;
                    role.dora++;
                }
            }
        }

        //*** ツモ
        // 生きてるだけで偉いのと同じで、上がるだけでツモはつきます
        role.roleKinds.Add(ROLE_KIND.TUMO);
        role.han += 1;

        //*** 平和
        if (syunko[0].Count >= 4)
        {
            if (head != jikaze && head != TILE_KIND.HAKU && head != TILE_KIND.HATU && head != TILE_KIND.TYUN)
            {
                role.roleKinds.Add(ROLE_KIND.PINFU);
                role.han += 1;
            }
        }

        //*** 断么九
        if (mpst[3].Count == 0)
        {
            bool isTanyao = true;

            // 萬,筒,索のみループ
            for (int i = 0; i < 3 && isTanyao; i++)
            {
                // 順子
                for (int j = 0; j < syunkoMpst[0][i].Count; j++)
                {
                    int number = CalcNumber(syunkoMpst[0][i][j][0]);

                    // 最初の牌が1か7なら終了
                    if (number == 1 || number == 7)
                    {
                        isTanyao = false;
                        break;
                    }
                }
                // 刻子
                for (int j = 0; j < syunkoMpst[1][i].Count && isTanyao; j++)
                {
                    int number = CalcNumber(syunkoMpst[1][i][j][0]);

                    // 最初の牌が1か9なら終了
                    if (number == 1 || number == 9)
                    {
                        isTanyao = false;
                        break;
                    }
                }
            }

            // タンヤオか
            if (isTanyao)
            {
                role.roleKinds.Add(ROLE_KIND.TANYAO);
                role.han += 1;
            }
        }

        //*** 風・白・發・中
        bool[] jihaiOnce = { false, false, false, false };
        for (int i = 0; i < syunkoMpst[1][3].Count; i++)
        {
            if (!jihaiOnce[0] && syunkoMpst[1][3][i][0] == jikaze)
            {
                role.roleKinds.Add(ROLE_KIND.KAZE);
                role.han += 1;
                jihaiOnce[0] = true;
                continue;
            }
            else if (!jihaiOnce[1] && syunkoMpst[1][3][i][0] == TILE_KIND.HAKU)
            {
                role.roleKinds.Add(ROLE_KIND.HAKU);
                role.han += 1;
                jihaiOnce[1] = true;
                continue;
            }
            else if (!jihaiOnce[2] && syunkoMpst[1][3][i][0] == TILE_KIND.HATU)
            {
                role.roleKinds.Add(ROLE_KIND.HATU);
                role.han += 1;
                jihaiOnce[2] = true;
                continue;
            }
            else if (!jihaiOnce[3] && syunkoMpst[1][3][i][0] == TILE_KIND.TYUN)
            {
                role.roleKinds.Add(ROLE_KIND.TYUN);
                role.han += 1;
                jihaiOnce[3] = true;
                continue;
            }
        }

        //*** 一盃口・二盃口
        // 同じ種類の順子が複数無ければ判定しない
        if (syunkoMpst[0][0].Count >= 2 || syunkoMpst[0][1].Count >= 2 || syunkoMpst[0][2].Count >= 2)
        {
            // 盃口数
            int pekoCnt = 0;
            // 一盃口判定済み順子
            bool[] useSyuntu = Enumerable.Range(0, syunko[0].Count).Select(_ => false).ToArray();
            for (int i = 0; i < syunko[0].Count; i++)
            {
                if (useSyuntu[i]) continue;

                for (int j = i + 1; j < syunko[0].Count; j++)
                {
                    if (useSyuntu[j]) continue;

                    // ソートされているはずだから最初だけ同じなら全部同じなはず
                    if (syunko[0][i][0] == syunko[0][j][0])
                    {
                        useSyuntu[i] = true;
                        useSyuntu[j] = true;
                        pekoCnt++;
                        break;
                    }
                }
            }
            if (pekoCnt != 0)
            {
                if (pekoCnt < 2)
                {
                    role.roleKinds.Add(ROLE_KIND.IPEKO);
                    role.han += 1;
                }
                else
                {
                    role.roleKinds.Add(ROLE_KIND.RYANPEKO);
                    role.han += 3;
                }
            }
        }

        //*** 三色同順
        if (syunkoMpst[0][0].Count != 0 && syunkoMpst[0][1].Count != 0 && syunkoMpst[0][2].Count != 0)
        {
            // 2こ順子があるMPS
            int twoMps = -1;
            for (int i = 0; i < 3; i++)
            {
                if (syunkoMpst[0][i].Count == 2)
                {
                    twoMps = i;
                    break;
                }
            }

            // 判定
            if (CalcNumber(syunkoMpst[0][0][0][0], 0) == CalcNumber(syunkoMpst[0][1][0][0], 1) && CalcNumber(syunkoMpst[0][0][0][0], 0) == CalcNumber(syunkoMpst[0][2][0][0], 2))
            {
                role.roleKinds.Add(ROLE_KIND.SANSYOKUDOUJUN);
                role.han += 2;
            }
            else if (twoMps != -1)
            {
                // 2こ目の順子があればそれも判定
                if (CalcNumber(syunkoMpst[0][0][twoMps == 0 ? 1 : 0][0], 0) == CalcNumber(syunkoMpst[0][1][twoMps == 1 ? 1 : 0][0], 1) && 
                    CalcNumber(syunkoMpst[0][0][twoMps == 0 ? 1 : 0][0], 0) == CalcNumber(syunkoMpst[0][2][twoMps == 2 ? 1 : 0][0], 2))
                {
                    role.roleKinds.Add(ROLE_KIND.SANSYOKUDOUJUN);
                    role.han += 2;
                }
            }
        }

        //*** 一気通貫
        for (int i = 0; i < 3; i++)
        {
            if (syunkoMpst[0][i].Count >= 3)
            {
                bool[] use147 = { false, false, false };
                for (int j = 0; j < syunkoMpst[0][i].Count; j++)
                {
                    if (CalcNumber(syunkoMpst[0][i][j][0]) == 1)
                        use147[0] = true;
                    if (CalcNumber(syunkoMpst[0][i][j][0]) == 4)
                        use147[1] = true;
                    if (CalcNumber(syunkoMpst[0][i][j][0]) == 7)
                        use147[2] = true;
                }

                // 順子の先頭が1,4,7が揃っていれば成立
                if (use147[0] && use147[1] && use147[2])
                {
                    role.roleKinds.Add(ROLE_KIND.ITTU);
                    role.han += 2;
                }

                // 順子が3つあるモノが2つ以上あることはない
                break;
            }
        }

        //*** チャンタ・純チャン
        // 頭チェック
        if (CalcNumber(head) == 1 || CalcNumber(head) == 9 || CalcNumber(head) == 0)
        {
            bool isTyan = true;
            // 順子のチェック
            for (int i = 0; i < syunko[0].Count; i++)
            {
                // 開始牌が1か7なら端
                if (CalcNumber(syunko[0][i][0]) != 1 && CalcNumber(syunko[0][i][0]) != 7)
                {
                    isTyan = false;
                    break;
                }
            }
            // 既に中張牌のみの順子があれば行わない
            if (isTyan)
            {
                for (int i = 0; i < syunko[1].Count; i++)
                {
                    // 開始牌が1か9なら端,0なら字
                    if (CalcNumber(syunko[1][i][0]) != 1 && CalcNumber(syunko[1][i][0]) != 9 && CalcNumber(syunko[1][i][0]) != 0)
                    {
                        isTyan = false;
                        break;
                    }
                }
                if (isTyan)
                {
                    // 字牌があればチャンタ
                    if (mpst[3].Count > 0)
                    {
                        role.roleKinds.Add(ROLE_KIND.TYANTA);
                        role.han += 2;
                    }
                    else
                    {
                        role.roleKinds.Add(ROLE_KIND.JUNTYAN);
                        role.han += 3;
                    }
                }
            }
        }

        //*** 三暗刻
        if (syunko[1].Count == 3)
        {
            role.roleKinds.Add(ROLE_KIND.SANANKO);
            role.han += 2;
        }

        //*** 小三元
        // 頭が三元牌以外なら違う
        if (head == TILE_KIND.HAKU || head == TILE_KIND.HATU || head == TILE_KIND.TYUN)
        {
            hht[(int)head - (int)TILE_KIND.HAKU] = true;

            // 四暗刻の時に三元牌はチェック済み
            if (hht[0] && hht[1] && hht[2])
            {
                role.roleKinds.Add(ROLE_KIND.SYOSANGEN);
                role.han += 2;
            }
        }


        //*** 三色同刻
        if (syunkoMpst[1][0].Count != 0 && syunkoMpst[1][1].Count != 0 && syunkoMpst[1][2].Count != 0)
        {
            // 2こ刻子があるMPS
            int twoMps = -1;
            for (int i = 0; i < 3; i++)
            {
                if (syunkoMpst[1][i].Count == 2)
                {
                    twoMps = i;
                    break;
                }
            }

            // 判定
            if (CalcNumber(syunkoMpst[1][0][0][0], 0) == CalcNumber(syunkoMpst[1][1][0][0], 1) && CalcNumber(syunkoMpst[1][0][0][0], 0) == CalcNumber(syunkoMpst[1][2][0][0], 2))
            {
                role.roleKinds.Add(ROLE_KIND.SANSYOKUDOUKOU);
                role.han += 2;
            }
            else if (twoMps != -1)
            {
                // 2こ目の刻子があればそれも判定
                if (CalcNumber(syunkoMpst[1][0][twoMps == 0 ? 1 : 0][0], 0) == CalcNumber(syunkoMpst[1][1][twoMps == 1 ? 1 : 0][0], 1) &&
                    CalcNumber(syunkoMpst[1][0][twoMps == 0 ? 1 : 0][0], 0) == CalcNumber(syunkoMpst[1][2][twoMps == 2 ? 1 : 0][0], 2))
                {
                    role.roleKinds.Add(ROLE_KIND.SANSYOKUDOUKOU);
                    role.han += 2;
                }
            }
        }

        //*** ホンイツ・清一色
        // 頭込みの使用されている牌種数(萬,筒,索)
        int useMPS = 0;
        useMPS += mpst[0].Count > 0 || headMPST == 0 ? 1 : 0;
        useMPS += mpst[1].Count > 0 || headMPST == 1 ? 1 : 0;
        useMPS += mpst[2].Count > 0 || headMPST == 2 ? 1 : 0;
        // 一種類のみ使用
        if (useMPS == 1)
        {
            // 字牌がなければホンイツ
            if (mpst[3].Count > 0)
            {
                role.roleKinds.Add(ROLE_KIND.HONITU);
                role.han += 3;
            }
            else
            {
                role.roleKinds.Add(ROLE_KIND.TINITU);
                role.han += 6;
            }
        }

        // *** 符計算 ***
        // 基本符 + あがり符
        role.fu += 20 + 2;
        // 刻子
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < syunkoMpst[1][i].Count; j++)
            {
                // 字牌なら判定不要
                if (i != 3)
                {
                    int number = CalcNumber(syunkoMpst[1][i][j][0], i);
                    if (number == 1 || number == 9)
                        role.fu += 8;
                    else
                        role.fu += 4;
                }
                else
                {
                    role.fu += 8;
                }
            }
        }
        // 雀頭
        if (head == jikaze || head == TILE_KIND.HAKU || head == TILE_KIND.HATU || head == TILE_KIND.TYUN)
            role.fu += 2;
        // 平和
        for (int i = 0; i < role.roleKinds.Count; i++)
        {
            if (role.roleKinds[i] == ROLE_KIND.PINFU)
                role.fu = 20;
        }

        return role;
    }

    /// <summary>
    /// 萬,筒,索の数字の計算
    /// </summary>
    /// <param name="k">牌種</param>
    /// <param name="mps">萬,筒,索が判明していれば(0:萬,1:筒,2:索)</param>
    /// <returns>牌の数字</returns>
    private static int CalcNumber(TILE_KIND k, int mps = -1)
    {
        if (mps == -1)
            mps = CalcMPST(k) - 1;

        switch (mps)
        {
            case 0:
                return (int)k - (int)TILE_KIND.MAN_1 + 1;
            case 1:
                return (int)k - (int)TILE_KIND.PIN_1 + 1;
            case 2:
                return (int)k - (int)TILE_KIND.SOO_1 + 1;
        }
        return 0;
    }
}
