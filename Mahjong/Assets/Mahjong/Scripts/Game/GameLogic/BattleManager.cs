using System;
using System.Collections.Generic;
using UnityEngine;
using static BattleManager;

public class BattleManager
{
    // プレイヤーの攻撃データ
    public class PlayerAttackData
    {
        public MahjongLogic.Role _role;
        public int _score;

        public PlayerAttackData(MahjongLogic.Role role, int damage)
        {
            _role = role;
            _score = damage;
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

    // プレイヤーキャラデータ
    private CharacterData _playerCharaData;

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
    public void InitBattle(StageData stageData, int currentEnemyIdx, CharacterData playerCharaData)
    {
        // 各変数の代入
        _stageData = stageData;
        _enemyData = stageData._appearEnemy[currentEnemyIdx];
        _enemyHp = _enemyData._hitPoint;
        _playerCharaData = playerCharaData;
        _playerHp = _playerCharaData._hitPoint;
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
            int damage = CalcEnemyAttackDamage();
            _playerHp -= damage;
            if (_playerHp < 0)
            {
                _playerHp = 0;
                // 敗北
                gameData._currentState = GameController.GAME_STATE.BATTLE_LOSE;
            }

            Debug.Log("敵の攻撃 > " + damage + "ダメージ / 残り体力" + (int)((float)_playerHp / _playerCharaData._hitPoint * 100.0f) + "%");

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
            int score = MahjongLogic.CalcScore(role);

            // プレイヤー攻撃データを返却
            return new PlayerAttackData(role, score);
        }

        return null;
    }

    /// <summary>
    /// プレイヤーの攻撃
    /// </summary>
    /// <param name="playerAttackData">プレイヤー攻撃情報</param>
    /// <param name="gameData">ゲームデータ</param>
    public void PlayerAttack(PlayerAttackData playerAttackData, GameController.GameData gameData)
    {
        // プレイヤーの攻撃計算
        int damage = CalcPlayerAttackDamage(playerAttackData);
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
        return (float)_playerHp / _playerCharaData._hitPoint;
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

    /// <summary>
    /// 敵の攻撃のダメージの計算
    /// </summary>
    /// <returns>ダメージ</returns>
    private int CalcEnemyAttackDamage()
    {
        float baseDamage = _enemyData._attackDamage;

        // 通常攻撃と属性攻撃で半分に分ける
        baseDamage = baseDamage * 0.5f;

        // 通常攻撃ダメージ
        float normalDamage = baseDamage * (float)((float)_enemyData._attackPower / _playerCharaData._defence);

        // 属性攻撃ダメージ
        float elementalDamage = baseDamage * (float)(_enemyData._elementalAttackPower / (_playerCharaData._defence * 0.5f));

        return (int)(normalDamage + elementalDamage);
    }

    /// <summary>
    /// プレイヤーの攻撃のダメージの計算
    /// </summary>
    /// <param name="playerAttackData">プレイヤー攻撃情報</param>
    /// <returns>ダメージ</returns>
    private int CalcPlayerAttackDamage(PlayerAttackData playerAttackData)
    {
        float baseDamage = playerAttackData._score;

        // 通常攻撃と属性攻撃で半分に分ける
        baseDamage = baseDamage * 0.5f;

        // 通常攻撃ダメージ
        float normalDamage = baseDamage * (float)((float)_playerCharaData._attackPower / _enemyData._defence);

        // 面子の数で分ける
        baseDamage = baseDamage * 0.25f;

        // 属性攻撃ダメージ
        float elementalDamage = 0.0f;
        for (int i = 0; i < GameData.MAX_MENTU_NUM; i++)
        {
            // 属性の相性レート
            float affinityRato = CalcElementalAffinityDamageRate(playerAttackData._role.elementals[i], _enemyData._elemental);
            // プレイヤーキャラと同じ属性ならダメージアップ
            float charaElementalBuff = _playerCharaData._elemental == playerAttackData._role.elementals[i] ? GameData.ELEMENTAL_AFFINITY_DAMAGE_GOOD : GameData.ELEMENTAL_AFFINITY_DAMAGE_DEFAULT;

            // 属性ダメージ計算
            elementalDamage += baseDamage * (float)(_playerCharaData._elementalAttackPower / (_enemyData._defence * 0.5f)) * affinityRato * charaElementalBuff;
        }

        return (int)(normalDamage + elementalDamage);
    }

    /// <summary>
    /// 属性相性のダメージ倍率の計算
    /// </summary>
    /// <param name="attack">攻撃側属性</param>
    /// <param name="defense">防御側属性</param>
    /// <returns>ダメージ倍率</returns>
    private float CalcElementalAffinityDamageRate(GameData.ELEMENTAL attack, GameData.ELEMENTAL defense)
    {
        // 萬子(火)
        if (attack == GameData.ELEMENTAL.FIRE)
        {
            // 筒子(水)
            if (defense == GameData.ELEMENTAL.WATER)
                return GameData.ELEMENTAL_AFFINITY_DAMAGE_POOR;
            // 索子(木)
            if (defense == GameData.ELEMENTAL.WOOD)
                return GameData.ELEMENTAL_AFFINITY_DAMAGE_GOOD;
            // 白(淼)
            if (defense == GameData.ELEMENTAL.OCEAN)
                return GameData.ELEMENTAL_AFFINITY_DAMAGE_AWFUL;
            // その他
            return GameData.ELEMENTAL_AFFINITY_DAMAGE_DEFAULT;
        }
        // 筒子(水)
        else if (attack == GameData.ELEMENTAL.WATER)
        {
            // 索子(木)
            if (defense == GameData.ELEMENTAL.WOOD)
                return GameData.ELEMENTAL_AFFINITY_DAMAGE_POOR;
            // 萬子(火)
            if (defense == GameData.ELEMENTAL.FIRE)
                return GameData.ELEMENTAL_AFFINITY_DAMAGE_GOOD;
            // 發(森)
            if (defense == GameData.ELEMENTAL.FOREST)
                return GameData.ELEMENTAL_AFFINITY_DAMAGE_AWFUL;
            // その他
            return GameData.ELEMENTAL_AFFINITY_DAMAGE_DEFAULT;
        }
        // 索子(木)
        else if (attack == GameData.ELEMENTAL.WOOD)
        {
            // 萬子(火)
            if (defense == GameData.ELEMENTAL.FIRE)
                return GameData.ELEMENTAL_AFFINITY_DAMAGE_POOR;
            // 筒子(水)
            if (defense == GameData.ELEMENTAL.WATER)
                return GameData.ELEMENTAL_AFFINITY_DAMAGE_GOOD;
            // 中(焱)
            if (defense == GameData.ELEMENTAL.FLAME)
                return GameData.ELEMENTAL_AFFINITY_DAMAGE_AWFUL;
            // その他
            return GameData.ELEMENTAL_AFFINITY_DAMAGE_DEFAULT;
        }
        // 風牌(無)
        else if (attack == GameData.ELEMENTAL.VOID)
        {
            return GameData.ELEMENTAL_AFFINITY_DAMAGE_DEFAULT;
        }
        // 白(淼)
        else if (attack == GameData.ELEMENTAL.OCEAN)
        {
            // 萬子(火)
            if (defense == GameData.ELEMENTAL.FIRE)
                return GameData.ELEMENTAL_AFFINITY_DAMAGE_GREAT;
            // 中(焱)
            if (defense == GameData.ELEMENTAL.FLAME)
                return GameData.ELEMENTAL_AFFINITY_DAMAGE_GOOD;
            // 發(森)
            if (defense == GameData.ELEMENTAL.FLAME)
                return GameData.ELEMENTAL_AFFINITY_DAMAGE_POOR;
            // その他
            return GameData.ELEMENTAL_AFFINITY_DAMAGE_DEFAULT;
        }
        // 發(森)
        else if (attack == GameData.ELEMENTAL.FOREST)
        {
            // 筒子(水)
            if (defense == GameData.ELEMENTAL.WATER)
                return GameData.ELEMENTAL_AFFINITY_DAMAGE_GREAT;
            // 白(淼)
            if (defense == GameData.ELEMENTAL.OCEAN)
                return GameData.ELEMENTAL_AFFINITY_DAMAGE_GOOD;
            // 中(焱)
            if (defense == GameData.ELEMENTAL.FLAME)
                return GameData.ELEMENTAL_AFFINITY_DAMAGE_POOR;
            // その他
            return GameData.ELEMENTAL_AFFINITY_DAMAGE_DEFAULT;
        }
        // 中(焱)
        else if (attack == GameData.ELEMENTAL.FLAME)
        {
            // 索子(木)
            if (defense == GameData.ELEMENTAL.WOOD)
                return GameData.ELEMENTAL_AFFINITY_DAMAGE_GREAT;
            // 發(森)
            if (defense == GameData.ELEMENTAL.FLAME)
                return GameData.ELEMENTAL_AFFINITY_DAMAGE_GOOD;
            // 白(淼)
            if (defense == GameData.ELEMENTAL.OCEAN)
                return GameData.ELEMENTAL_AFFINITY_DAMAGE_POOR;
            // その他
            return GameData.ELEMENTAL_AFFINITY_DAMAGE_DEFAULT;
        }
        return GameData.ELEMENTAL_AFFINITY_DAMAGE_DEFAULT;
    }
}
