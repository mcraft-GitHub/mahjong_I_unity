using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/EnemyData")]
public class EnemyData : ScriptableObject
{
    // 画像
    public Sprite _enemyImage = null;

    // 攻撃間隔
    public float _attackDelay = 10.0f;

    // 攻撃ダメージ
    public int _attackDamage = 1200;

    // 体力
    public int _hitPoint = 10000;
}
