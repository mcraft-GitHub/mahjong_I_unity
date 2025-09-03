using UnityEngine;

[CreateAssetMenu(fileName = "PSPlayerBufHp", menuName = "Game/PassiveSkill/PSPlayerBufHp")]
public class PSPlayerBuffHp : PassiveSkillBase
{
    // 効果
    public override EFFECTIVE_TYPE _effectiveType => EFFECTIVE_TYPE.PLAYER_BUFF;

    // Hp増加率
    [SerializeField] private float _hpBuffRate = 1.0f;

    /// <summary>
    /// パズル盤面操作スキル
    /// </summary>
    /// <param name="board">パズル盤面配列</param>
    public override void PlayerBuffSkill(CharacterData characterData)
    {
        characterData._hitPoint = (int)(characterData._hitPoint * _hpBuffRate);
    }
}
