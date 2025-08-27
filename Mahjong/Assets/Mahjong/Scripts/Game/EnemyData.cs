using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/EnemyData")]
public class EnemyData : ScriptableObject
{
    // 画像
    public Sprite enemyImage = null;

    // 攻撃間隔
    public float attackDelay = 10.0f;

    // 攻撃ダメージ
    public int attackDamage = 1200;

    // 体力
    public int hitPoint = 10000;
}
