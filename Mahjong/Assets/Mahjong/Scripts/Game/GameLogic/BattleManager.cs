using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager
{
    // プレイヤーの攻撃データ
    public class PlayerAttackData
    {
        public MahjongLogic.Role _role;
        public int _damage;

        public PlayerAttackData(MahjongLogic.Role role, int damage)
        {
            this._role = role;
            this._damage = damage;
        }
    }

    // ***** Public変数
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
    // 風牌の種類
    private const int KAZEHAI_KIND_NUM = 4;

    // ダメージ倍数(将来的にステージによって変わる可能性も考えて、変更可)
    private float _damageMultiple = 0.1f;

    // ステージデータ
    private StageData _stageData;

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
        _beginCnt = GameData.BEGIN_GAME_COUNTDOWN_TIME;

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
    /// <param name="gameData">ゲームデータ</param>
    public void TickCountDown(float deltaTime, GameController.GameData gameData)
    {
        // ゲーム開始カウントダウン
        _beginCnt -= deltaTime;
        if (_beginCnt <= 0)
        {
            // 開始したら値を-1にする
            _beginCnt = -1;

            // 開始
            gameData._currentState = GameController.GAME_STATE.PUZZLE;
        }
    }

    /// <summary>
    /// 敵の攻撃の毎フレーム実行処理
    /// </summary>
    /// <param name="deltaTime">前フレームからの経過時間</param>
    /// <param name="gameData">ゲームデータ</param>
    /// <returns>敵が攻撃したか</returns>
    public bool TickEnemyAttack(float deltaTime, GameController.GameData gameData)
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
                // 敗北
                gameData._currentState = GameController.GAME_STATE.BATTLE_LOSE;
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
    /// <param name="addHandMentu">追加された面子</param>
    /// <returns>プレイヤーの攻撃データ</returns>
    public PlayerAttackData AddHandTiles(MahjongLogic.GameMentu addHandMentu)
    {
        for (int i = 0; i < addHandMentu.tilesKind.Length; i++)
        {
            // 手牌に追加
            _handTilesKindList.Add(addHandMentu.tilesKind[i]);
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
            int damage = (int)(MahjongLogic.CalcScore(role) * _damageMultiple);

            // プレイヤー攻撃データを返却
            return new PlayerAttackData(role, damage);
        }

        return null;
    }

    /// <summary>
    /// プレイヤーの攻撃
    /// </summary>
    /// <param name="damage">ダメージ</param>
    /// <param name="gameData">ゲームデータ</param>
    public void PlayerAttack(int damage, GameController.GameData gameData)
    {
        // プレイヤーの攻撃計算
        _enemyHp -= damage;

        Debug.Log("プレイヤーの攻撃 > " + damage + "ダメージ / 残り体力" + (int)((float)_enemyHp / _enemyData._hitPoint * 100.0f) + "%");

        // 敵が倒れたか判定
        if (_enemyHp <= 0)
        {
            _enemyHp = 0;

            // 勝利
            gameData._currentState = GameController.GAME_STATE.BATTLE_WIN;
        }
        else
        {
            // ドラの決定
            _doraTilesKind = MahjongLogic.GetRandomTileKind(_stageData._useTilesKind);
            // 雀頭の決定
            _headTilesKind = MahjongLogic.GetRandomTileKind(_stageData._useTilesKind);
            // 自風のカウント
            _jikazeCnt = (_jikazeCnt + 1) % KAZEHAI_KIND_NUM;

            // 手牌のクリア
            _handTilesKindList.Clear();
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
}
