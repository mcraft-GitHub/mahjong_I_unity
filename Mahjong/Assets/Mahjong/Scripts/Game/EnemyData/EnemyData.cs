using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/EnemyData")]
public class EnemyData : ScriptableObject
{
    // 画像
    public Sprite _enemyImage = null;

    // 攻撃間隔
    public float _attackDelay = 10.0f;

    // 属性
    public GameData.ELEMENTAL _elemental;

    // 与えるダメージ
    public int _attackDamage;

    // 攻撃力
    public int _attackPower;

    // 属性攻撃力
    public int _elementalAttackPower;

    // 体力
    public int _hitPoint = 10000;

    // 防御力
    public int _defence;
}
