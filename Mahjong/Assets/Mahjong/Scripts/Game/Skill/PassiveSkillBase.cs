using UnityEngine;

public class PassiveSkillBase : ScriptableObject
{
    // 効果の種類
    public enum EFFECTIVE_TYPE
    {
        NONE = 0,
        PLAYER_BUFF,        // プレイヤーパフ
        ENEMY_DEBUFF,       // 敵デバフ
        ROLE_BUFF,          // 役バフ
    }

    // スキル名
    public string _name;

    // 説明文
    public string _nescription;

    // 効果
    public virtual EFFECTIVE_TYPE _effectiveType => EFFECTIVE_TYPE.NONE;

    /// <summary>
    /// 敵デバフスキル
    /// </summary>
    /// <param name="enemyData">敵データ</param>
    public virtual void EnemyDebuffSkill(EnemyData enemyData) { }

    /// <summary>
    /// プレイヤーバフスキル
    /// </summary>
    /// <param name="characterData">プレイヤーデータ</param>
    public virtual void PlayerBuffSkill(CharacterData characterData) { }

    /// <summary>
    /// 役バフスキル
    /// </summary>
    /// /// <param name="未定">役の何か</param>
    public virtual void RoleBuffSkill() { }
}
