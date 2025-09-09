using System.Collections;
using UnityEngine;

public class PuzzleController
{
    // ステージデータ
    private StageData _stageData;

    // ゲームコントローラー
    private GameController _gameController;

    // パズルマネージャー
    private PuzzleManager _puzzleManager;

    // パズルビューマネージャー
    private PuzzleViewManager _puzzleViewManager;

    // 前フレームのステート
    private PuzzleManager.PUZZLE_STATE _prevState = PuzzleManager.PUZZLE_STATE.PAUSE;

    // インプットハンドラー
    private TouchInputHandler _input;

    // ***** READY
    // 移動開始位置
    private Vector2Int? _currentMoveIndex = null;

    // ***** MATCH
    // 牌が落下中か
    private bool _isFalling = false;

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
        _prevState = PuzzleManager.PUZZLE_STATE.PAUSE;

        // パズルマネージャーの生成・初期化
        _puzzleManager = new PuzzleManager();
        _puzzleManager.InitPuzzle(stageData._useTilesKind);

        // パズルビューマネージャーの取得
        _puzzleViewManager = _gameController.PuzzleViewManager;
        // パズルマネージャーのセット
        _puzzleViewManager.SetClass(_puzzleManager);
        // 盤面のセット
        _puzzleViewManager.CreatePuzzleBoard();

        // インプットハンドラーの取得
        _input = _gameController.TouchInputHandler;
    }

    /// <summary>
    /// 毎フレーム実行処理
    /// </summary>
    /// <param name="deltaTime">前フレームからの経過時間</param>
    /// <returns>マッチした面子のデータ(盤面インデックス配列, 牌種配列)</returns>
    public (Vector2Int[] index, MahjongLogic.TILE_KIND[] kinds)? TickProcess(float deltaTime)
    {
        // 戻り値
        (Vector2Int[] index, MahjongLogic.TILE_KIND[] kinds)? mentuTilesData = null;

        // ステート切り替えの影響を受けないため保持しておく
        PuzzleManager.PUZZLE_STATE prevState = _puzzleManager._state;

        switch (_puzzleManager._state)
        {
            case PuzzleManager.PUZZLE_STATE.READY:
                ReadyProcess();
                break;
            case PuzzleManager.PUZZLE_STATE.MATCH:
                mentuTilesData = MatchProcess();
                break;
            case PuzzleManager.PUZZLE_STATE.PREV_MOVE: // TODO:非マッチ状態で指を話したら移動が戻る機能. 仕様未決定. いつか追加するかも知れない
                break;
            case PuzzleManager.PUZZLE_STATE.PAUSE:     // TODO:ポーズ機能. 仕様未決定. 設定ボタン追加時に追加するかも
                break;
        }
        _prevState = prevState;

        return mentuTilesData;
    }

    /// <summary>
    /// パズル準備完了(パズル操作中)処理
    /// </summary>
    private void ReadyProcess()
    {
        // 切り替わったら現在の位置を初期化
        if (_prevState != PuzzleManager.PUZZLE_STATE.READY)
            _currentMoveIndex = null;

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
                _puzzleManager.MoveNow(_currentMoveIndex.Value);
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
                    _puzzleManager.MoveNow(_currentMoveIndex.Value);
                }
            }
        }
    }

    /// <summary>
    /// パズルマッチ処理
    /// </summary>
    /// <returns>マッチした面子のデータ(盤面インデックス配列, 牌種配列)</returns>
    private (Vector2Int[] index, MahjongLogic.TILE_KIND[] kinds)? MatchProcess()
    {
        (Vector2Int[] index, MahjongLogic.TILE_KIND[] kinds)? mentuTilesData = null;

        if (_puzzleManager._matchTilesIndex.Count > 0)
        {
            // マッチした面子の牌分ループ
            for (int i = 0; i < GameData.MENTU_TILES_NUM; i++)
            {
                // マッチした牌を削除
                _puzzleViewManager.DestroyPuzzleTile(_puzzleManager._matchTilesIndex[0][i]);
            }

            // マッチ面子データの代入(タプル！)
            mentuTilesData = (
                (Vector2Int[])_puzzleManager._matchTilesIndex[0].Clone(),
                (MahjongLogic.TILE_KIND[])_puzzleManager._matchTilesKind[0].Clone()
            ); 

            // 処理済みの面子の削除
            _puzzleManager._matchTilesIndex.RemoveAt(0);
        }
        else
        {
            // 牌を落とす
            if (!_isFalling)
            {
                // 牌の落下
                float fallTime = _puzzleViewManager.FallPuzzleTile();

                // 落下時間分待ってから落下の終了
                _gameController.StartCoroutine(FinishFallCoroutine(fallTime));

                _isFalling = true;
            }
        }

        return mentuTilesData;
    }

    /// <summary>
    /// 落下終了コルーチン
    /// </summary>
    /// <param name="fallTime"></param>
    private IEnumerator FinishFallCoroutine(float fallTime)
    {
        yield return new WaitForSeconds(fallTime);

        // マッチの終了
        _puzzleManager.FinishMatch();

        // 落下の終了
        _isFalling = false;
    }
}
