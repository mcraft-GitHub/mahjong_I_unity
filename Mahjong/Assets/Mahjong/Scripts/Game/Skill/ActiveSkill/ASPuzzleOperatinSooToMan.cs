using UnityEngine;

[CreateAssetMenu(fileName = "ASPuzzleOperatinSooToMan", menuName = "Game/ActiveSkill/ASPuzzleOperatinSooToMan")]
public class ASPuzzleOperatinSooToMan : ActiveSkillBase
{
    // 効果
    public override EFFECTIVE_TYPE _effectiveType => EFFECTIVE_TYPE.PUZZLE_OPERATION;

    /// <summary>
    /// パズル盤面操作スキル
    /// </summary>
    /// <param name="board">パズル盤面配列</param>
    public override void PuzzleOperationSkill(MahjongLogic.TILE_KIND[,] board)
    {
        // 仮スキル(索子を萬子に置き換える)
        for (int y = 0; y < GameData.PUZZLE_BOARD_SIZE_Y; y++)
        {
            for (int x = 0; x < GameData.PUZZLE_BOARD_SIZE_X; x++)
            {
                // 索子の場合
                if ((int)MahjongLogic.TILE_KIND.SOO_1 <= (int)board[y, x] && (int)board[y, x] <= (int)MahjongLogic.TILE_KIND.SOO_9)
                {
                    // 萬子に変換
                    board[y, x] = (MahjongLogic.TILE_KIND)((int)board[y, x] - (int)MahjongLogic.TILE_KIND.SOO_1);
                }
            }
        }
    }
}
