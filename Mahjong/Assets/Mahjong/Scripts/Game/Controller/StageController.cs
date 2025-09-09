using System.Collections;
using UnityEngine;

public class StageController
{
    // ステート
    public enum STAGE_STATE
    {
        OPENING = 0, // オープニング
        ADVANCE, // ステージ進行
        ENCOUNT, // 接敵
        BATTLE, // バトル
        BATTLE_WIN, // バトル勝利
        BATTLE_LOSE, // バトル敗北
        STAGE_CLEAR, // ステージクリア
        NONE,
    }

    // ステージデータ
    private StageData _stageData;

    // 現在の敵インデックス
    private int _currentEnemyIdx = 0;

    // ゲームコントローラー
    private GameController _gameController;

    // バトルコントローラー
    private BattleController _battleController;

    // ステージビューマネージャー
    private StageViewManager _stageViewManager;

    // 現在のステート
    private STAGE_STATE _currentState = STAGE_STATE.OPENING;

    // 前フレームのステート
    private STAGE_STATE _prevState = STAGE_STATE.NONE;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <param name="gameController">ゲームコントローラーコンポーネント</param>
    /// <param name="stageData">ステージデータ</param>
    public void Init(GameController gameController, StageData stageData)
    {
        // 変数の初期化
        _gameController = gameController;
        _stageData = stageData;
        _currentState = STAGE_STATE.OPENING;
        _prevState = STAGE_STATE.NONE;

        // ステージビューマネージャーの取得
        _stageViewManager = _gameController.StageViewManager;
    }

    /// <summary>
    /// 毎フレーム実行処理
    /// </summary>
    /// <param name="deltaTime">前フレームからの経過時間</param>
    public void TickProcess(float deltaTime)
    {
        // ステート切り替えの影響を受けないため保持しておく
        STAGE_STATE stateBuf = _currentState;

        // 各ステートの処理
        switch (_currentState)
        {
            // オープニング
            case STAGE_STATE.OPENING:
                OpeningProcess();
                break;
            // 進行
            case STAGE_STATE.ADVANCE:
                AdvanceProcess();
                break;
            // 遭遇
            case STAGE_STATE.ENCOUNT:
                EncountProcess();
                break;
            // バトル
            case STAGE_STATE.BATTLE:
                BattleProcess(deltaTime);
                break;
            // バトル勝利
            case STAGE_STATE.BATTLE_WIN:
                BattleWinProcess();
                break;
            // バトル敗北
            case STAGE_STATE.BATTLE_LOSE:
                BattleLoseProcess();
                break;
            // ステージクリア
            case STAGE_STATE.STAGE_CLEAR:
                StageClearProcess();
                break;
        }

        // 前フレームステート
        _prevState = stateBuf;
    }

    /// <summary>
    /// オープニング処理
    /// </summary>
    private void OpeningProcess()
    {
        if (_prevState != STAGE_STATE.OPENING)
        {
            float wateTime = 0.0f;

            // フェードイン
            wateTime += _stageViewManager.BeginFadeIn();

            // オープニング演出
            wateTime += _stageViewManager.OpeningVisualPresentation();

            // ステージ進行
            _gameController.StartCoroutine(ChangeStateCoroutine(STAGE_STATE.ADVANCE, wateTime));
        }
    }

    /// <summary>
    /// 進行処理
    /// </summary>
    private void AdvanceProcess()
    {
        if (_prevState != STAGE_STATE.ADVANCE)
        {
            float wateTime = 0.0f;

            // ステージ進行演出
            wateTime += _stageViewManager.AdvanceVisualPresentation();

            // フェードアウト
            wateTime += _stageViewManager.BeginFadeOut();

            // 敵遭遇
            _gameController.StartCoroutine(ChangeStateCoroutine(STAGE_STATE.ENCOUNT, wateTime));
        }
    }

    /// <summary>
    /// 遭遇処理
    /// </summary>
    private void EncountProcess()
    {
        if (_prevState != STAGE_STATE.ENCOUNT)
        {
            float wateTime = 0.0f;

            // バトルコントローラーの生成・初期化
            _battleController = new BattleController();
            _battleController.Init(_gameController, _stageData, _currentEnemyIdx);

            // フェードイン
            wateTime += _stageViewManager.BeginFadeIn();

            // 敵遭遇演出
            wateTime += _stageViewManager.EncountVisualPresentation();

            // バトル
            _gameController.StartCoroutine(ChangeStateCoroutine(STAGE_STATE.BATTLE, wateTime));
        }
    }

    /// <summary>
    /// バトル処理
    /// </summary>
    /// <param name="deltaTime">前フレームからの経過時間</param>
    private void BattleProcess(float deltaTime)
    {
        // バトルコントローラーの更新
        _battleController.TickProcess(deltaTime);

        // バトルが終了したか
        if (_battleController._isBattleWin.HasValue) 
        {
            // ステートの切り替え
            _currentState = _battleController._isBattleWin.Value ? STAGE_STATE.BATTLE_WIN : STAGE_STATE.BATTLE_LOSE;
        }
    }

    /// <summary>
    /// バトル勝利処理
    /// </summary>
    private void BattleWinProcess()
    {
        if (_prevState != STAGE_STATE.BATTLE_WIN)
        {
            float wateTime = 0.0f;

            // 勝利演出
            wateTime += _stageViewManager.BattleWinVisualPresentation();

            // 敵のカウントを進める
            _currentEnemyIdx++;

            // 全ての敵を倒したか
            if (_stageData._appearEnemy.Count <= _currentEnemyIdx)
            {
                // ステージクリア
                _gameController.StartCoroutine(ChangeStateCoroutine(STAGE_STATE.STAGE_CLEAR, wateTime));
            }
            else
            {
                // ステージ進行
                _gameController.StartCoroutine(ChangeStateCoroutine(STAGE_STATE.ADVANCE, wateTime));
            }
        }
    }

    /// <summary>
    /// バトル敗北
    /// </summary>
    private void BattleLoseProcess()
    {
        if (_prevState != STAGE_STATE.BATTLE_LOSE)
        {
            float wateTime = 0.0f;

            // 敗北演出
            wateTime += _stageViewManager.BattleLoseVisualPresentation();

            // フェードアウト
            wateTime += _stageViewManager.BeginFadeOut();

            // TODO: シーン切り替え
        }
    }

    /// <summary>
    /// ステージクリア
    /// </summary>
    /// <return>ゲーム(ステージ)が終了したか</return>
    private void StageClearProcess()
    {

        if (_prevState != STAGE_STATE.STAGE_CLEAR)
        {
            float wateTime = 0.0f;

            // クリア演出
            wateTime += _stageViewManager.StageClearVisualPresentation();

            // フェードアウト
            wateTime += _stageViewManager.BeginFadeOut();

            // TODO: シーン切り替え
        }
    }

    /// <summary>
    /// ステート切り替えコルーチン
    /// </summary>
    /// <param name="state">切替先ステート</param>
    /// <param name="waitTime">待機時間</param>
    private IEnumerator ChangeStateCoroutine(STAGE_STATE state, float waitTime)
    {
        Debug.Log("check:" + state + ", " + waitTime);

        yield return new WaitForSeconds(waitTime);

        _currentState = state;
    }
}
