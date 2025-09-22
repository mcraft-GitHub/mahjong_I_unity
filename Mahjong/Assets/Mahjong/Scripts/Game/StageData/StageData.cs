using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Game/StageData")]
public class StageData : ScriptableObject
{
    // 名前
    public string _name;

    // 説明文
    public string _description;

    // 難易度
    public int _difficulty;

    // 出現敵
    public List<EnemyData> _appearEnemy;

    // 使用牌
    public List<MahjongLogic.TILE_KIND> _useTilesKind;

    // ステージ進行時画像
    public Sprite _advanceImage;

    // ステージ背景色
    public Color _backgroundColor;

    // TODO: 報酬(未定)
}
