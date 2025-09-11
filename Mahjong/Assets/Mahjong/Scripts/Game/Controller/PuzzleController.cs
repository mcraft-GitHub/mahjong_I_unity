using System.Collections;
using UnityEditor.Overlays;
using UnityEngine;

public class PuzzleController : MonoBehaviour
{
    // パズルビュー管理クラス
    [SerializeField] private PuzzleViewManager _puzzleViewManager;

    // 入力ハンドラ
    [SerializeField] private TouchInputHandler _input;

    // ステージデータ
    private StageData _stageData;

    // パズルマネージャー
    private PuzzleManager _puzzleManager;

    // ***** READY
    // 移動開始位置
    private Vector2Int? _currentMoveIndex = null;

    // ***** MATCH
    // 牌が落下中か
    private bool _isFalling = false;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <param name="stageData">ステージデータ</param>
    public void Init(StageData stageData)
    {
        // 変数の初期化
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
                InitPuzzle(gameData, prevState);
                break;
            // パズル
            case GameController.GAME_STATE.PUZZLE:
                UpdatePuzzleTileMove(gameData, prevState);
                break;
            // マッチング結果処理
            case GameController.GAME_STATE.MATCHING_RESULT:
                UpdateMatchMentuAndFallTiles(gameData, prevState);
                break;
            // その他
            default:
                break;
        }
    }

    /// <summary>
    /// パズルの初期化
    /// </summary>
    /// <param name="gameData">ゲームデータ</param>
    /// <param name="prevState">1フレーム前のステート</param>
    private void InitPuzzle(GameController.GameData gameData, GameController.GAME_STATE prevState)
    {
        if (prevState != GameController.GAME_STATE.COUNTDOWN)
        {
            // パズルマネージャーの生成・初期化
            _puzzleManager = new PuzzleManager();
            _puzzleManager.InitPuzzle(_stageData._useTilesKind);

            // パズルマネージャーのセット
            _puzzleViewManager.SetClass(_puzzleManager);
            // 盤面のセット
            _puzzleViewManager.CreatePuzzleBoard();

            // マッチ面子のクリア
            _puzzleManager._matchMentu.Clear();
        }
    }

    /// <summary>
    /// パズル牌の移動の更新
    /// </summary>
    /// <param name="gameData">ゲームデータ</param>
    /// <param name="prevState">1フレーム前のステート</param>
    private void UpdatePuzzleTileMove(GameController.GameData gameData, GameController.GAME_STATE prevState)
    {
        // 切り替わったら現在の位置を初期化
        if (prevState != GameController.GAME_STATE.PUZZLE)
        {
            _currentMoveIndex = null;
        }

        if (_currentMoveIndex.HasValue)
        {
            // 移動終了
            if (_input.GetTouchState() == TouchInputHandler.TouchState.TouchEnded || _input.GetTouchState() == TouchInputHandler.TouchState.None)
            {
                // 移動終了
                _puzzleManager.MoveEnd(_currentMoveIndex.Value);
                _currentMoveIndex = null;
                return;
            }

            // 選択パズル牌の計算
            Vector2Int? newIndex = _puzzleViewManager.CalcTouchPuzzleTileIndex(_input.GetCurrentDragPosition());
            if (!newIndex.HasValue)
            {
                // 移動終了
                _puzzleManager.MoveEnd(_currentMoveIndex.Value);
                _currentMoveIndex = null;
                return;
            }

            // 移動
            if (newIndex.Value.x != _currentMoveIndex.Value.x || newIndex.Value.y != _currentMoveIndex.Value.y)
            {
                // 牌の入れ替え
                _puzzleViewManager.SwitchingPuzzleTile(_currentMoveIndex.Value, newIndex.Value);
                _currentMoveIndex = newIndex;
                _puzzleManager.MoveNow(_currentMoveIndex.Value, gameData);
            }
        }
        else
        {
            // 移動開始判定
            if (_input.GetTouchState() == TouchInputHandler.TouchState.TouchStarted)
            {
                _currentMoveIndex = _puzzleViewManager.CalcTouchPuzzleTileIndex(_input.GetCurrentDragPosition());
                if (_currentMoveIndex.HasValue)
                {
                    // 移動開始
                    _puzzleManager.MoveNow(_currentMoveIndex.Value, gameData);
                }
            }
        }
    }

    /// <summary>
    /// マッチした面子と落下の更新
    /// </summary>
    /// <param name="gameData">ゲームデータ</param>
    /// <param name="prevState">1フレーム前のステート</param>
    private void UpdateMatchMentuAndFallTiles(GameController.GameData gameData, GameController.GAME_STATE prevState)
    {
        // 手牌の追加が処理されたらnullになる
        if (gameData.addHandMentu == null)
        {
            if (_puzzleManager._matchMentu.Count > 0)
            {
                // 次の手牌追加面子にセット
                gameData.addHandMentu = _puzzleManager._matchMentu[0];

                // マッチした面子の牌分ループ
                for (int i = 0; i < GameData.MENTU_TILES_NUM; i++)
                {
                    // マッチした牌を削除
                    _puzzleViewManager.DestroyPuzzleTile(_puzzleManager._matchMentu[0].tilesIndex[i]);
                }

                // 処理済みの面子の削除
                _puzzleManager._matchMentu.RemoveAt(0);
            }
            else
            {
                // 牌を落とす
                if (!_isFalling)
                {
                    // 牌の落下
                    float fallTime = _puzzleViewManager.FallPuzzleTile();

                    // 落下時間分待ってから落下の終了
                    StartCoroutine(FinishFallCoroutine(gameData, fallTime));

                    _isFalling = true;
                }
            }
        }
    }

    /// <summary>
    /// 落下終了コルーチン
    /// </summary>
    /// <param name="gameData">ゲームデータ</param>
    /// <param name="fallTime"></param>
    private IEnumerator FinishFallCoroutine(GameController.GameData gameData, float fallTime)
    {
        yield return new WaitForSeconds(fallTime);

        // マッチの終了
        _puzzleManager.FinishMatch(gameData);

        // 落下の終了
        _isFalling = false;
    }
}
