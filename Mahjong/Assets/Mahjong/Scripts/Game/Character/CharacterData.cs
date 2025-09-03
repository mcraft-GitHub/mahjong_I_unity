using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Game/CharacterData")]
public class CharacterData : ScriptableObject
{
    // キャラクターID
    public int _characterId;

    // キャラの種類のID
    public int _kindId;

    // 画像
    public Sprite _image;

    // 名前
    public string _name;

    // 説明文
    public string _description;

    // レベル
    public int _level;

    // 経験値
    public int _experiencePoints;

    // 属性(複数属性の可能性もあるので、ビット演算かも)
    public int _elemental;

    // 攻撃力
    public int _attackPower;

    // 属性攻撃力
    public int _elementalAttackPower;

    // 体力
    public int _hitPoint;

    // 防御力
    public int _defence;

    // アクティブスキル
    public ActiveSkillBase _activeSkill;

    // パッシブスキル
    public List<PassiveSkillBase> _passiveSkills;
}   
