using System.Collections;
using UnityEngine;

public class BattleController
{
    // プレイヤーの攻撃データ
    public class PlayerAttackData
    {
        public MahjongLogic.Role role;
        public int damage;

        public PlayerAttackData(MahjongLogic.Role role, int damage)
        {
            this.role = role;
            this.damage = damage;
        }
    }

    // 仮のプレイヤーHP
    private const int TMP_PLAYER_HP = 2000;

    // ステージデータ
    private StageData _stageData;

    // 現在の敵インデックス
    private int _currentEnemyIdx = 0;

    // ゲームコントローラー
    private GameController _gameController;

    // パズルコントローラー
    private PuzzleController _puzzleController;

    // バトルマネージャー
    private BattleManager _battleManager;

    // バトルビューマネージャー
    private BattleViewManager _battleViewManager;

    // 前フレームのステート
    private BattleManager.BATTLE_STATE _prevState = BattleManager.BATTLE_STATE.COUNTDOWN;

    // 敵データ
    private EnemyData _enemyData;

    // 手牌の追加(プレイヤーの攻撃)演出の待機時間
    private float _waitMatchTime = 0.0f;

    // プレイヤーが攻撃しているか
    private bool _isPlayerAttack = false;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <param name="gameController">ゲームコントローラーコンポーネント</param>
    /// <param name="stageData">敵データ</param>
    /// <param name="currentEnemyIdx">現在の敵インデックス</param>
    public void Init(GameController gameController, StageData stageData, int currentEnemyIdx)
    {
        // 変数の初期化
        _gameController = gameController;
        _stageData = stageData;
        _currentEnemyIdx = currentEnemyIdx;
        _prevState = BattleManager.BATTLE_STATE.COUNTDOWN;

        // バトルマネージャーの生成・初期化(TODO: プレイヤーHPセットシステム)
        _battleManager = new BattleManager();
        _battleManager.InitBattle(stageData, currentEnemyIdx, TMP_PLAYER_HP);

        // 現在の敵データ
        _enemyData = stageData._appearEnemy[currentEnemyIdx];

        // バトルビューマネージャーの取得
        _battleViewManager = _gameController.BattleViewManager;
        // ゲージの初期化
        _battleViewManager.InitUIGauge();
        // 敵の画像のセット
        _battleViewManager.SetEnemyImage(_enemyData._enemyImage);
        // ドラ・雀頭・自風のセット
        _battleViewManager.SetDoraHeadJikazeKind(_battleManager._doraTilesKind, _battleManager._headTilesKind, _battleManager._jikazeCnt);

        // パズルコントローラーの生成・初期化
        _puzzleController = new PuzzleController();
        _puzzleController.Init(_gameController, stageData);
    }

    /// <summary>
    /// ステートの更新
    /// </summary>
    /// <param name="deltaTime">前フレームからの経過時間</param>
    public void StateUpdate(float deltaTime)
    {
        // ステート切り替えの影響を受けないため保持しておく
        BattleManager.BATTLE_STATE stateBuf = _battleManager._state;

        // 各ステートの処理
        switch (_battleManager._state)
        {
            // カウントダウン
            case BattleManager.BATTLE_STATE.COUNTDOWN:
                CountDownProcess(deltaTime);
                break;
            // バトル(パズル)
            case BattleManager.BATTLE_STATE.BATTLE:
                BattleProcess(deltaTime);
                break;
            // 手牌の追加(プレイヤーの攻撃)
            case BattleManager.BATTLE_STATE.ADD_HAND_TILES:
                AddHandTilesProcess();
                break;
            // 戦闘終了(勝利)
            case BattleManager.BATTLE_STATE.FINISH_WIN:
                break;
            // 戦闘終了(敗北)
            case BattleManager.BATTLE_STATE.FINISH_LOSE:
                break;
        }

        // 前フレームステート
        _prevState = stateBuf;
    }

    /// <summary>
    /// カウントダウン処理
    /// </summary>
    /// <param name="deltaTime">前フレームからの経過時間</param>
    private void CountDownProcess(float deltaTime)
    {
        // カウントダウン
        _battleManager.TickCountDown(deltaTime);

        // Viewの更新
        _battleViewManager.SetBeginGameCount((int)_battleManager._beginCnt);
    }

    /// <summary>
    /// バトル(パズル)処理
    /// </summary>
    /// <param name="deltaTime">前フレームからの経過時間</param>
    private void BattleProcess(float deltaTime)
    {
        // パズルコントローラーの更新
        _puzzleController.StateUpdate(deltaTime);

        // マッチしていたら
        if (_puzzleController._matchMentuData.HasValue)
        {
            // 手牌に追加・手牌がそろったら役計算
            PlayerAttackData roleResultData = _battleManager.AddHandTiles(_puzzleController._matchMentuData.Value.kinds);

            // プレイヤーが攻撃しているか
            _isPlayerAttack = roleResultData != null;

            // 手牌に加える演出
            _waitMatchTime = _battleViewManager.AddHandTiles(_battleManager._handTilesKindList, _puzzleController._matchMentuData.Value.index, roleResultData);
        }
        else
        {
            // 敵の攻撃
            bool isAttack = _battleManager.TickEnemyAttack(deltaTime);

            // 敵の攻撃演出・敵の攻撃ゲージ更新
            _battleViewManager.EnemyAttackUpdate(_battleManager.GetEnemyAttackRate(), isAttack, _battleManager.GetPlayerHpRate());
        }
    }

    /// <summary>
    /// 手牌の追加(プレイヤーの攻撃)処理
    /// </summary>
    private void AddHandTilesProcess()
    {
        if (_prevState != BattleManager.BATTLE_STATE.ADD_HAND_TILES)
        {
            _gameController.StartCoroutine(FinishAddHandTilesCoroutine(_waitMatchTime));
        }
    }

    /// <summary>
    /// バトルステートの取得
    /// </summary>
    /// <returns>バトルステート</returns>
    public BattleManager.BATTLE_STATE GetBattleState() => _battleManager._state;

    /// <summary>
    /// 手牌追加の終了コルーチン
    /// </summary>
    /// <param name="waitTime">待機時間</param>
    private IEnumerator FinishAddHandTilesCoroutine(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        // 手牌の追加(プレイヤーの攻撃)の終了
        _battleManager.FinishAddHandTiles();
        
        // プレイヤーが攻撃したか(次の局か)
        if (_isPlayerAttack)
        {
            // 敵HPゲージの更新
            _battleViewManager.SetEnemyHp(_battleManager.GetEnemyHpRate());

            // ドラ・雀頭・自風のセット
            _battleViewManager.SetDoraHeadJikazeKind(_battleManager._doraTilesKind, _battleManager._headTilesKind, _battleManager._jikazeCnt);

            // 手牌クリア
            _battleViewManager.ClearHandTiles();
        }
    }
}
