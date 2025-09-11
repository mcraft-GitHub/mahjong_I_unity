using System.Collections;
using UnityEngine;

public class BattleController : MonoBehaviour
{
    // バトルビュー管理クラス
    [SerializeField] private BattleViewManager _battleViewManager;

    // 仮のプレイヤーHP
    private const int TMP_PLAYER_HP = 2000;

    // ステージデータ
    private StageData _stageData;

    // バトルマネージャー
    private BattleManager _battleManager;

    // 手牌の追加(プレイヤーの攻撃)演出の待機時間
    private float _waitMatchTime = 0.0f;

    // プレイヤーの攻撃
    BattleManager.PlayerAttackData _playerAttackData = null;

    // 手牌に加える演出中か
    private bool _isAddHand = false;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <param name="stageData">敵データ</param>
    public void Init(StageData stageData)
    {
        // ステージデータのセット
        _stageData = stageData;
    }

    /// <summary>
    /// ステートの更新
    /// </summary>
    /// <param name="gameData">ゲームデータ</param>
    /// <param name="prevState">1フレーム前のステート</param>
    public void StateUpdate(GameController.GameData gameData, GameController.GAME_STATE prevState)
    {
        // 各ステートの処理
        switch (gameData.currentState)
        {
            // カウントダウン
            case GameController.GAME_STATE.COUNTDOWN:
                InitBattleAndCountdown(gameData, prevState);
                break;
            // パズル
            case GameController.GAME_STATE.PUZZLE:
                UpdateEnemyAttack(gameData, prevState);
                break;
            // マッチング結果処理
            case GameController.GAME_STATE.MATCHING_RESULT:
                UpdateHandWithNewMentu(gameData, prevState);
                break;
            // その他
            default:
                break;
        }
    }

    /// <summary>
    /// バトルの初期化とカウントダウン処理
    /// </summary>
    /// <param name="gameData">ゲームデータ</param>
    /// <param name="prevState">1フレーム前のステート</param>
    private void InitBattleAndCountdown(GameController.GameData gameData, GameController.GAME_STATE prevState)
    {
        if (prevState != GameController.GAME_STATE.COUNTDOWN)
        {
            // バトルの初期化
            InitBattle(gameData, TMP_PLAYER_HP);
        }

        // フェードイン
        _battleViewManager.BeginFadeIn();

        // カウントダウン
        _battleManager.TickCountDown(Time.deltaTime, gameData);

        // Viewの更新
        _battleViewManager.SetBeginGameCount((int)_battleManager._beginCnt);
    }

    /// <summary>
    /// 敵の攻撃の更新
    /// </summary>
    /// <param name="gameData">ゲームデータ</param>
    /// <param name="prevState">1フレーム前のステート</param>
    private void UpdateEnemyAttack(GameController.GameData gameData, GameController.GAME_STATE prevState)
    {
        // 敵の攻撃
        bool isAttack = _battleManager.TickEnemyAttack(Time.deltaTime, gameData);

        // 敵の攻撃演出・敵の攻撃ゲージ更新
        _battleViewManager.EnemyAttackUpdate(_battleManager.GetEnemyAttackRate(), isAttack, _battleManager.GetPlayerHpRate());
    }

    /// <summary>
    /// 手牌に新しい面子を追加する
    /// </summary>
    /// <param name="gameData">ゲームデータ</param>
    /// <param name="prevState">1フレーム前のステート</param>
    private void UpdateHandWithNewMentu(GameController.GameData gameData, GameController.GAME_STATE prevState)
    {
        // 手牌に追加する面子がある & 手牌に追加中ではない
        if (gameData.addHandMentu != null && !_isAddHand)
        {
            // 手牌に追加・手牌がそろったら役計算
            _playerAttackData = _battleManager.AddHandTiles(gameData.addHandMentu);

            // 手牌に加える演出
            _waitMatchTime = _battleViewManager.AddHandTiles(_battleManager._handTilesKindList, gameData.addHandMentu.tilesIndex, _playerAttackData);

            // 手牌・攻撃追加演出が終了処理
            StartCoroutine(FinishAddHandTilesProcessCoroutine(_waitMatchTime, gameData));
        }
    }

    /// <summary>
    /// 手牌追加の終了コルーチン
    /// </summary>
    /// <param name="waitTime">待機時間</param>
    /// <param name="gameData">ゲームデータ</param>
    private IEnumerator FinishAddHandTilesProcessCoroutine(float waitTime, GameController.GameData gameData)
    {
        // 手牌に加える処理の開始
        _isAddHand = true;

        yield return new WaitForSeconds(waitTime);

        // プレイヤーが攻撃したか(次の局か)
        if (_playerAttackData != null)
        {
            // プレイヤーの攻撃
            _battleManager.PlayerAttack(_playerAttackData.damage, gameData);

            // 敵HPゲージの更新
            _battleViewManager.SetEnemyHp(_battleManager.GetEnemyHpRate());

            // ドラ・雀頭・自風のセット
            _battleViewManager.SetDoraHeadJikazeKind(_battleManager._doraTilesKind, _battleManager._headTilesKind, _battleManager._jikazeCnt);

            // 手牌クリア
            _battleViewManager.ClearHandTiles();
        }

        // 追加した面子を削除
        gameData.addHandMentu = null;

        // 手牌に加える処理の終了
        _isAddHand = false;
    }

    /// <summary>
    /// バトルの初期化
    /// </summary>
    /// <param name="gameData">ゲームデータ</param>
    /// <param name="playerHp">プレイヤーのHP</param>
    private void InitBattle(GameController.GameData gameData, int playerHp)
    {
        // バトルマネージャーの生成・初期化(TODO: プレイヤーHPセットシステム)
        _battleManager = new BattleManager();
        _battleManager.InitBattle(_stageData, gameData.currentEnemtIdx, playerHp);

        // ゲージの初期化
        _battleViewManager.InitUIGauge();
        // 敵の画像のセット
        _battleViewManager.SetEnemyImage(_stageData._appearEnemy[gameData.currentEnemtIdx]._enemyImage);
        // ドラ・雀頭・自風のセット
        _battleViewManager.SetDoraHeadJikazeKind(_battleManager._doraTilesKind, _battleManager._headTilesKind, _battleManager._jikazeCnt);
    }
}
