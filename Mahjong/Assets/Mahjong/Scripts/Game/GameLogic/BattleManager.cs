using System;
using UnityEngine;

public class BattleManager
{
    // ダメージ倍数(将来的にステージによって変わる可能性も考えて、変更可)
    private float _damageMultiple = 0.1f;

    // プレイヤーの最大体力
    private int _playerMaxHp;

    // プレイヤーの体力
    private int _playerHp;

    // 敵データ
    private EnemyData _enemyData;

    // 敵の体力
    private int _enemyHp;

    // 攻撃間隔時間カウント
    private float _attackDelayCnt = 0.0f;

    /// <summary>
    /// バトルの初期化
    /// </summary>
    /// <param name="enemyData">敵データ</param>
    /// <param name="playerHp">プレイヤー体力</param>
    public void InitBattle(EnemyData enemyData, int playerHp)
    {
        // 各変数の代入
        _enemyData = enemyData;
        _enemyHp = enemyData._hitPoint;
        _playerMaxHp = playerHp;
        _playerHp = playerHp;
    }

    /// <summary>
    /// ダメージ計算
    /// </summary>
    /// <param name="role">役</param>
    /// <returns>ダメージ</returns>
    public int CalcDamage(MahjongLogic.Role role)
    {
        // 符の切り上げ
        role.fu = (int)(Math.Ceiling(role.fu / (double)10) * 10);

        // 満貫以上は翻数で確定
        if (role.han >= 13) return (int)(32000 * (role.han / 13) * _damageMultiple);
        if (role.han >= 11) return (int)(24000 * _damageMultiple);
        if (role.han >= 8) return (int)(16000 * _damageMultiple);
        if (role.han >= 6) return (int)(12000 * _damageMultiple);
        if (role.han >= 5) return (int)(8000 * _damageMultiple);
        if (role.han >= 4 && role.fu >= 40) return (int)(8000 * _damageMultiple);
        if (role.han >= 3 && role.fu >= 70) return (int)(8000 * _damageMultiple);

        // 点数計算
        double damage = role.fu * 4 * Math.Pow(2, role.han + 2);

        // 切り上げ
        damage = (int)(Math.Ceiling(damage / 100) * 100);

        return (int)(damage * _damageMultiple);
    }

    /// <summary>
    /// 敵攻撃チェック
    /// </summary>
    /// <param name="deltaTime">前処理からの経過時間</param>
    /// <returns>攻撃しなければ(-1f),攻撃したらプレイヤーの体力割合(1f～0f)を返す</returns>
    public float EnemyAttackCheck(float deltaTime)
    {
        _attackDelayCnt += deltaTime;
        if (_attackDelayCnt >= _enemyData._attackDelay)
        {
            // 敵の攻撃
            _playerHp -= _enemyData._attackDamage;
            if (_playerHp < 0)
                _playerHp = 0;

            Debug.Log("敵の攻撃 > " + _enemyData._attackDamage + "ダメージ / 残り体力" + (int)((float)_playerHp / _playerMaxHp * 100.0f) + "%");

            _attackDelayCnt = 0;
            return (float)_playerHp / _playerMaxHp;
        }
        return -1.0f;
    }

    /// <summary>
    /// プレイヤーの攻撃
    /// </summary>
    /// <param name="attackDamage">ダメージ量</param>
    /// <returns>敵の体力割合(1f～0f)</returns>
    public float PlayerAttackCheck(int attackDamage)
    {
        // プレイヤーの攻撃
        _enemyHp -= attackDamage;
        if (_enemyHp < 0)
            _enemyHp = 0;

        Debug.Log("プレイヤーの攻撃 > " + attackDamage + "ダメージ / 残り体力" + (int)((float)_enemyHp / _enemyData._hitPoint * 100.0f) + "%");

        return (float)_enemyHp / _enemyData._hitPoint;
    }

    /// <summary>
    /// 敵の攻撃カウント割合
    /// </summary>
    /// <returns>(1f～0f)</returns>
    public float GetEnemyAttackDelayRate()
    {
        return _attackDelayCnt / _enemyData._attackDelay;
    }

    /// <summary>
    /// ゲームオーバーか(プレイヤーか敵のHPが0)
    /// </summary>
    /// <returns>ゲーム中:0, 勝利:1, 敗北:2</returns>
    public int IsGameOver()
    {
        if (_enemyHp == 0)
            return 1;

        if (_playerHp == 0)
            return 2;

        return 0;
    }
}
