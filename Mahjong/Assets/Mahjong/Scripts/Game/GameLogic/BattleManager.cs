using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager
{
    // ステート
    public enum BATTLE_STATE
    {
        COUNTDOWN = 0, // カウントダウン
        BATTLE, // メインバトル(パズル,敵の攻撃)
        ADD_HAND_TILES, // 手牌の追加(プレイヤーの攻撃)
        FINISH_WIN, // バトル終了(勝利)
        FINISH_LOSE, // バトル終了(敗北)
        MAX,
    }

    // ***** Public変数
    // ゲームステート
    public BATTLE_STATE _state { get; private set; } = BATTLE_STATE.COUNTDOWN;

    // 手牌
    public List<MahjongLogic.TILE_KIND> _handTilesKindList { get; private set; } = new List<MahjongLogic.TILE_KIND>();

    // ゲーム開始カウントダウン
    public float _beginCnt { get; private set; } = GameData.BEGIN_GAME_COUNTDOWN_TIME;

    // 雀頭牌
    public MahjongLogic.TILE_KIND _headTilesKind = MahjongLogic.TILE_KIND.NONE;
    // ドラ牌
    public MahjongLogic.TILE_KIND _doraTilesKind = MahjongLogic.TILE_KIND.NONE;
    // 自風カウント(0～3)
    public int _jikazeCnt = 0;

    // ***** Private変数
    // 役満,三倍満.倍満,跳満,満貫の翻数
    private const int YAKUMAN_HAN = 13;
    private const int SANBAIMAN_HAN = 11;
    private const int BAIMAN_HAN = 8;
    private const int HANEMAN_HAN = 6;
    private const int MANGAN_HAN = 4;

    // 役満,三倍満.倍満,跳満,満貫の点数
    private const int YAKUMAN_POINT = 32000;
    private const int SANBAIMAN_POINT = 24000;
    private const int BAIMAN_POINT = 16000;
    private const int HANEMAN_POINT = 12000;
    private const int MANGAN_POINT = 8000;

    // 4翻,3翻の時に満貫になる符数
    private const int FOUR_HAN_MANGAN_FU = 40;
    private const int THREE_HAN_MANGAN_FU = 70;

    // 風牌の種類
    private const int KAZEHAI_KIND_NUM = 4;

    // ダメージ倍数(将来的にステージによって変わる可能性も考えて、変更可)
    private float _damageMultiple = 0.1f;

    // ステージデータ
    private StageData _stageData;

    // プレイヤーの攻撃
    private (MahjongLogic.Role role, int damage)? _playerAttackData = null;

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

    // ***** Public関数
    /// <summary>
    /// バトルの初期化
    /// </summary>
    /// <param name="enemyData">敵データ</param>
    /// <param name="playerHp">プレイヤー体力</param>
    public void InitBattle(StageData stageData, int currentEnemyIdx, int playerHp)
    {
        // 各変数の代入
        _stageData = stageData;
        _enemyData = stageData._appearEnemy[currentEnemyIdx];
        _enemyHp = _enemyData._hitPoint;
        _playerMaxHp = playerHp;
        _playerHp = playerHp;
        _attackDelayCnt = 0.0f;

        // 各変数の初期化
        _state = BATTLE_STATE.COUNTDOWN;
        _beginCnt = GameData.BEGIN_GAME_COUNTDOWN_TIME;
        _playerAttackData = null;

        // ドラの決定
        _doraTilesKind = MahjongLogic.GetRandomTileKind(_stageData._useTilesKind);
        // 雀頭の決定
        _headTilesKind = MahjongLogic.GetRandomTileKind(_stageData._useTilesKind);
        // 自風のカウント
        _jikazeCnt = 0;
    }

    /// <summary>
    /// バトル開始カウントダウンの毎フレーム実行処理
    /// </summary>
    /// <param name="deltaTime">前フレームからの経過時間</param>
    public void TickCountDown(float deltaTime)
    {
        // ゲーム開始カウントダウン
        _beginCnt -= deltaTime;
        if (_beginCnt <= 0)
        {
            // 開始したら値を-1にする
            _beginCnt = -1;

            // 開始
            Debug.Log("バトルステート変更：" + _state + " > " + BATTLE_STATE.BATTLE);
            _state = BATTLE_STATE.BATTLE;
        }
    }

    /// <summary>
    /// 敵の攻撃の毎フレーム実行処理
    /// </summary>
    /// <param name="deltaTime">前フレームからの経過時間</param>
    /// <returns>敵が攻撃したか</returns>
    public bool TickEnemyAttack(float deltaTime)
    {
        // 敵攻撃カウント
        _attackDelayCnt += deltaTime;
        if (_attackDelayCnt >= _enemyData._attackDelay)
        {
            // 敵の攻撃
            _playerHp -= _enemyData._attackDamage;
            if (_playerHp < 0)
            {
                _playerHp = 0;
                Debug.Log("バトルステート変更：" + _state + " > " + BATTLE_STATE.FINISH_LOSE);
                _state = BATTLE_STATE.FINISH_LOSE;
            }

            Debug.Log("敵の攻撃 > " + _enemyData._attackDamage + "ダメージ / 残り体力" + (int)((float)_playerHp / _playerMaxHp * 100.0f) + "%");

            _attackDelayCnt = 0;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 手牌の追加
    /// </summary>
    /// <param name="kinds">追加牌種配列</param>
    /// <returns>未攻撃：null, 攻撃：役とダメージ</returns>
    public (MahjongLogic.Role role, int damage)? AddHandTiles(MahjongLogic.TILE_KIND[] kinds)
    {
        Debug.Log("バトルステート変更：" + _state + " > " + BATTLE_STATE.ADD_HAND_TILES);
        _state = BATTLE_STATE.ADD_HAND_TILES;

        for (int i = 0; i < kinds.Length; i++)
        {
            // 手牌に追加
            _handTilesKindList.Add(kinds[i]);
        }

        // 手牌がそろった
        if (_handTilesKindList.Count >= GameData.HAND_TILES_NUM)
        {
            // 手牌に雀頭を追加
            _handTilesKindList.Add(_headTilesKind);
            _handTilesKindList.Add(_headTilesKind);

            // 役の判定
            MahjongLogic.Role role =　MahjongLogic.CalcHandTilesRole(_handTilesKindList, _doraTilesKind, (MahjongLogic.TILE_KIND)((int)MahjongLogic.TILE_KIND.TON + _jikazeCnt));

            // ダメージの計算
            int damage = CalcDamage(role);

            // プレイヤー攻撃データのセット
            _playerAttackData = (role, damage);
        }

        return _playerAttackData;
    }

    /// <summary>
    /// 手牌の追加(プレイヤーの攻撃)終了
    /// </summary>
    public void FinishAddHandTiles()
    {
        // 攻撃をしていなければすぐに終了
        if (!_playerAttackData.HasValue)
        {
            Debug.Log("バトルステート変更：" + _state + " > " + BATTLE_STATE.BATTLE);
            _state = BATTLE_STATE.BATTLE;
            return;
        }

        // プレイヤーの攻撃計算
        _enemyHp -= _playerAttackData.Value.damage;

        Debug.Log("プレイヤーの攻撃 > " + _playerAttackData.Value.damage + "ダメージ / 残り体力" + (int)((float)_enemyHp / _enemyData._hitPoint * 100.0f) + "%");

        // 敵が倒れたか判定
        if (_enemyHp <= 0)
        {
            _enemyHp = 0;
            
            Debug.Log("バトルステート変更：" + _state + " > " + BATTLE_STATE.FINISH_WIN);
            _state = BATTLE_STATE.FINISH_WIN;
        }
        else
        {
            // プレイヤーの攻撃データの初期化
            _playerAttackData = null;

            // ドラの決定
            _doraTilesKind = MahjongLogic.GetRandomTileKind(_stageData._useTilesKind);
            // 雀頭の決定
            _headTilesKind = MahjongLogic.GetRandomTileKind(_stageData._useTilesKind);
            // 自風のカウント
            _jikazeCnt = (_jikazeCnt + 1) % KAZEHAI_KIND_NUM;

            // 手牌のクリア
            _handTilesKindList.Clear();

            Debug.Log("バトルステート変更：" + _state + " > " + BATTLE_STATE.BATTLE);
            _state = BATTLE_STATE.BATTLE;
        }
    }

    /// <summary>
    /// プレイヤーHP割合
    /// </summary>
    /// <returns>(1f～0f)</returns>
    public float GetPlayerHpRate()
    {
        return (float)_playerHp / _playerMaxHp;
    }

    /// <summary>
    /// 敵のHP割合
    /// </summary>
    /// <returns>(1f～0f)</returns>
    public float GetEnemyHpRate()
    {
        return (float)_enemyHp / _enemyData._hitPoint;
    }

    /// <summary>
    /// 敵の攻撃カウント割合
    /// </summary>
    /// <returns>(1f～0f)</returns>
    public float GetEnemyAttackRate()
    {
        return _attackDelayCnt / _enemyData._attackDelay;
    }

    // ***** Private関数
    /// <summary>
    /// ダメージ計算
    /// </summary>
    /// <param name="role">役</param>
    /// <returns>ダメージ</returns>
    private int CalcDamage(MahjongLogic.Role role)
    {
        // 符の切り上げ
        role.fu = (int)(Math.Ceiling(role.fu / (double)10) * 10);

        // 満貫以上は翻数で確定
        if (role.han >= YAKUMAN_HAN) return (int)(YAKUMAN_POINT * (role.han / YAKUMAN_HAN) * _damageMultiple);
        if (role.han >= SANBAIMAN_HAN) return (int)(SANBAIMAN_POINT * _damageMultiple);
        if (role.han >= BAIMAN_HAN) return (int)(BAIMAN_POINT * _damageMultiple);
        if (role.han >= HANEMAN_HAN) return (int)(HANEMAN_POINT * _damageMultiple);
        if (role.han >= (MANGAN_HAN + 1)) return (int)(MANGAN_POINT * _damageMultiple);
        if (role.han >= MANGAN_HAN && role.fu >= FOUR_HAN_MANGAN_FU) return (int)(MANGAN_POINT * _damageMultiple);
        if (role.han >= (MANGAN_HAN - 1) && role.fu >= THREE_HAN_MANGAN_FU) return (int)(MANGAN_POINT * _damageMultiple);

        // 点数計算
        double damage = role.fu * 4 * Math.Pow(2, role.han + 2);

        // 切り上げ
        damage = (int)(Math.Ceiling(damage / 100) * 100);

        return (int)(damage * _damageMultiple);
    }
}
