using System.Collections;
using UnityEngine;

public class StageController : MonoBehaviour
{
    // ステージビュー管理クラス
    [SerializeField] private StageViewManager _stageViewManager;

    // ステージデータ
    private StageData _stageData;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <param name="stageData">ステージデータ</param>
    public void Init(StageData stageData)
    {
        // 変数の初期化
        _stageData = stageData;

        // advance画像のセット
        _stageViewManager.SetAdvanceImage(stageData._advanceImage);
    }

    /// <summary>
    /// ステートの更新
    /// </summary>
    /// <param name="gameData">ゲームデータ</param>
    /// <param name="prevState">1フレーム前のステート</param>
    public void StateUpdate(GameController.GameData gameData, GameController.GAME_STATE prevState)
    {
        // 各ステートの処理
        switch (gameData._currentState)
        {
            // オープニング
            case GameController.GAME_STATE.OPENING:
                ShowOpeningAndChangeState(gameData, prevState);
                break;
            // 進行
            case GameController.GAME_STATE.ADVANCE:
                ShowAdvanceAndChangeState(gameData, prevState);
                break;
            // バトル勝利
            case GameController.GAME_STATE.BATTLE_WIN:
                ShowBattleWinAndChangeState(gameData, prevState);
                break;
            // バトル敗北
            case GameController.GAME_STATE.BATTLE_LOSE:
                ShowBattleLoseAndChangeState(gameData, prevState);
                break;
            // ステージクリア
            case GameController.GAME_STATE.STAGE_CLEAR:
                ShowStageClearAndChangeState(gameData, prevState);
                break;
            // その他
            default:
                break;
        }
    }

    /// <summary>
    /// オープニング時のステートの更新
    /// </summary>
    /// <param name="gameData">ゲームデータ</param>
    /// <param name="prevState">1フレーム前のステート</param>
    private void ShowOpeningAndChangeState(GameController.GameData gameData, GameController.GAME_STATE prevState)
    {
        if (prevState != GameController.GAME_STATE.OPENING)
        {
            // オープニング演出
            float waitTime = _stageViewManager.OpeningVisualPresentation(_stageData._name);

            // ステート切り替え
            StartCoroutine(ChangeStateCoroutine(gameData, GameController.GAME_STATE.ADVANCE, waitTime));
        }
    }

    /// <summary>
    /// 進行時のステートの更新
    /// </summary>
    /// <param name="gameData">ゲームデータ</param>
    /// <param name="prevState">1フレーム前のステート</param>
    private void ShowAdvanceAndChangeState(GameController.GameData gameData, GameController.GAME_STATE prevState)
    {
        if (prevState != GameController.GAME_STATE.ADVANCE)
        {
            // ステージ進行演出
            float waitTime = _stageViewManager.AdvanceVisualPresentation(_stageData, gameData);

            // 敵の画像のセット
            _stageViewManager.SetEnemyImage(_stageData._appearEnemy[gameData._currentEnemtIdx]._enemyImage);

            // ステート切り替え
            StartCoroutine(ChangeStateCoroutine(gameData, GameController.GAME_STATE.COUNTDOWN, waitTime));
        }
    }

    /// <summary>
    /// バトル勝利時のステートの更新
    /// </summary>
    /// <param name="gameData">ゲームデータ</param>
    /// <param name="prevState">1フレーム前のステート</param>
    private void ShowBattleWinAndChangeState(GameController.GameData gameData, GameController.GAME_STATE prevState)
    {
        if (prevState != GameController.GAME_STATE.BATTLE_WIN)
        {
            float waitTime = 0.0f;

            // 勝利演出
            waitTime += _stageViewManager.BattleWinVisualPresentation();

            // 敵のインデックスを進める
            gameData._currentEnemtIdx++;

            // 全ての敵を倒したか
            if (_stageData._appearEnemy.Count <= gameData._currentEnemtIdx)
            {
                // ステージクリア
                StartCoroutine(ChangeStateCoroutine(gameData, GameController.GAME_STATE.STAGE_CLEAR, waitTime));
            }
            else
            {
                // フェードアウト
                waitTime += _stageViewManager.BeginFadeOut(waitTime);

                // ステージ進行
                StartCoroutine(ChangeStateCoroutine(gameData, GameController.GAME_STATE.ADVANCE, waitTime));
            }
        }
    }

    /// <summary>
    /// バトル敗北時のステートの更新
    /// </summary>
    /// <param name="gameData">ゲームデータ</param>
    /// <param name="prevState">1フレーム前のステート</param>
    private void ShowBattleLoseAndChangeState(GameController.GameData gameData, GameController.GAME_STATE prevState)
    {
        if (prevState != GameController.GAME_STATE.BATTLE_LOSE)
        {
            float waitTime = 0.0f;

            // 敗北演出
            waitTime += _stageViewManager.BattleLoseVisualPresentation();

            // フェードアウト
            waitTime += _stageViewManager.BeginFadeOut();

            // ステート切り替え
            StartCoroutine(ChangeStateCoroutine(gameData, GameController.GAME_STATE.LOAD_SCENE, waitTime));
        }
    }

    /// <summary>
    /// ステージクリア時のステートの更新
    /// </summary>
    /// <param name="gameData">ゲームデータ</param>
    /// <param name="prevState">1フレーム前のステート</param>
    private void ShowStageClearAndChangeState(GameController.GameData gameData, GameController.GAME_STATE prevState)
    {

        if (prevState != GameController.GAME_STATE.STAGE_CLEAR)
        {
            // クリア演出
            float waitTime = _stageViewManager.StageClearVisualPresentation();

            // ステート切り替え
            StartCoroutine(ChangeStateCoroutine(gameData, GameController.GAME_STATE.LOAD_SCENE, waitTime));
        }
    }

    /// <summary>
    /// ステート切り替えコルーチン
    /// </summary>
    /// <param name="gameData">ゲームデータ</param>
    /// <param name="state">切替先ステート</param>
    /// <param name="waitTime">待機時間</param>
    private IEnumerator ChangeStateCoroutine(GameController.GameData gameData, GameController.GAME_STATE state, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        gameData._currentState = state;
    }
}
