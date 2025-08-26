using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

// マッチ → 一時停止 → 手牌に追加 → 手牌完成なら攻撃 → まだマッチがあるなら[手牌に追加]へ → 停止解除 → 落ちる → マッチ判定 → 最初へ

public class GameController : MonoBehaviour
{
    [SerializeField] private TouchInputHandler _input;
    [SerializeField] private ViewManager _viewManager;

    // パズルマネージャー
    private PuzzleManager _puzzleManager;

    // 前フレームのステート
    private PuzzleManager.GameState _prevState = PuzzleManager.GameState.PAUSE;

    // 手牌
    private List<MahjongLogic.TILE_KIND> _handTilesKindList = new List<MahjongLogic.TILE_KIND>();

    // ***** READY
    // 移動開始位置
    private Vector2Int? _currentMoveIndex = null;

    // ***** MATCH
    // アニメーション中か
    private bool _isAnimation = false;

    void Start()
    {
        // 麻雀牌のスケールと隙間の計算
        GameData.CalcTileScaleAndMargin();

        // パズルマネージャーの生成
        _puzzleManager = new PuzzleManager();

        // パズルマネージャーのセット
        _viewManager.SetClass(this, _puzzleManager);

        // ゲームの初期化
        InitGame();
    }

    void Update()
    {
        PuzzleManager.GameState prevState = _puzzleManager.state;
        switch (_puzzleManager.state)
        {
            case PuzzleManager.GameState.READY:
                UpdateReady();
                break;
            case PuzzleManager.GameState.MATCH:
                UpdateMatch();
                break;
            case PuzzleManager.GameState.PREV_MOVE:
                break;
            case PuzzleManager.GameState.PAUSE:
                break;
        }
        _prevState = prevState;
    }

    /// <summary>
    /// ステートがREADYの際の処理
    /// </summary>
    private void UpdateReady()
    {
        // 切り替わったら現在の位置を初期化
        if (_prevState != PuzzleManager.GameState.READY)
            _currentMoveIndex = null;

        if (!_currentMoveIndex.HasValue)
        {
            // 移動開始疑惑
            if (_input.GetTouchState() == TouchInputHandler.TouchState.TouchStarted)
            {
                _currentMoveIndex = _viewManager.CalcTouchPuzzleTileIndex(_input.GetCurrentDragPosition());
                if (_currentMoveIndex.HasValue)
                    _puzzleManager.MoveNow(_currentMoveIndex.Value);
            }
        }
        else
        {
            // 移動終了
            if (_input.GetTouchState() == TouchInputHandler.TouchState.TouchEnded || _input.GetTouchState() == TouchInputHandler.TouchState.None)
            {
                _puzzleManager.MoveEnd(_currentMoveIndex.Value);
                _currentMoveIndex = null;
                return;
            }

            // 選択パズル牌の計算
            Vector2Int? newIndex = _viewManager.CalcTouchPuzzleTileIndex(_input.GetCurrentDragPosition());
            // 移動終了
            if (!newIndex.HasValue)
            {
                _puzzleManager.MoveEnd(_currentMoveIndex.Value);
                _currentMoveIndex = null;
                return;
            }

            // 移動
            if (newIndex.Value.x != _currentMoveIndex.Value.x || newIndex.Value.y != _currentMoveIndex.Value.y)
            {
                _viewManager.SwitchingPuzzleTile(_currentMoveIndex.Value, newIndex.Value);
                _currentMoveIndex = newIndex;
                _puzzleManager.MoveNow(_currentMoveIndex.Value);
            }
        }
    }

    /// <summary>
    /// ステートがMATCHの際の処理
    /// </summary>
    private void UpdateMatch()
    {
        // マッチ処理
        if (!_isAnimation && _puzzleManager.matchTilesIndex.Count > 0)
        {
            StartCoroutine(ScalePosCoroutine());
        }
    }

    /// <summary>
    /// ゲームの初期化
    /// </summary>
    private void InitGame()
    {
        // 使用する牌
        List<MahjongLogic.TILE_KIND> useTileKinds = new List<MahjongLogic.TILE_KIND>();
        // TODO 使用する牌の種類。使用,不使用はコメントで切り替える。
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_1);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_2);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_3);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_4);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_5);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_6);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_7);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_8);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_9);
        useTileKinds.Add(MahjongLogic.TILE_KIND.PIN_1);
        useTileKinds.Add(MahjongLogic.TILE_KIND.PIN_2);
        useTileKinds.Add(MahjongLogic.TILE_KIND.PIN_3);
        useTileKinds.Add(MahjongLogic.TILE_KIND.PIN_4);
        useTileKinds.Add(MahjongLogic.TILE_KIND.PIN_5);
        useTileKinds.Add(MahjongLogic.TILE_KIND.PIN_6);
        useTileKinds.Add(MahjongLogic.TILE_KIND.PIN_7);
        useTileKinds.Add(MahjongLogic.TILE_KIND.PIN_8);
        useTileKinds.Add(MahjongLogic.TILE_KIND.PIN_9);
        useTileKinds.Add(MahjongLogic.TILE_KIND.SOO_1);
        useTileKinds.Add(MahjongLogic.TILE_KIND.SOO_2);
        useTileKinds.Add(MahjongLogic.TILE_KIND.SOO_3);
        useTileKinds.Add(MahjongLogic.TILE_KIND.SOO_4);
        useTileKinds.Add(MahjongLogic.TILE_KIND.SOO_5);
        useTileKinds.Add(MahjongLogic.TILE_KIND.SOO_6);
        useTileKinds.Add(MahjongLogic.TILE_KIND.SOO_7);
        useTileKinds.Add(MahjongLogic.TILE_KIND.SOO_8);
        useTileKinds.Add(MahjongLogic.TILE_KIND.SOO_9);
        //useTileKinds.Add(MahjongLogic.TILE_KIND.TON);
        //useTileKinds.Add(MahjongLogic.TILE_KIND.NAN);
        //useTileKinds.Add(MahjongLogic.TILE_KIND.SYA);
        //useTileKinds.Add(MahjongLogic.TILE_KIND.PEE);
        //useTileKinds.Add(MahjongLogic.TILE_KIND.HAKU);
        //useTileKinds.Add(MahjongLogic.TILE_KIND.HATU);
        //useTileKinds.Add(MahjongLogic.TILE_KIND.TYUN);
        // パズルの初期化
        _puzzleManager.InitPuzzle(useTileKinds);

        // 牌の配置
        _viewManager.CreatePuzzleBoard();
    }

    /// <summary>
    /// マッチ時の処理
    /// </summary>
    /// <returns>IEnumerator</returns>
    IEnumerator ScalePosCoroutine()
    {
        // アニメーション開始
        _isAnimation = true;

        // 最長落下時間を計算するために各X列の消える牌の数を保持
        int[] fallY = Enumerable.Range(0, GameData.PUZZLE_BOARD_SIZE_X).Select(_ => 0).ToArray();

        // マッチ牌の削除
        for (int i = 0; i < _puzzleManager.matchTilesIndex.Count; i++)
        {
            for (int j = 0; j < _puzzleManager.matchTilesIndex[i].Length; j++)
            {
                fallY[_puzzleManager.matchTilesIndex[i][j].x]++;
                _viewManager.DestroyPuzzleTile(_puzzleManager.matchTilesIndex[i][j]);

                // 手牌に追加(必ず3個ずつ追加されると信じて個数チェックはしません！)
                _handTilesKindList.Add(_puzzleManager.matchTilesKind[i][j]);
            }

            // 手牌に加える演出
            _viewManager.AddHandTiles(_handTilesKindList, _puzzleManager.matchTilesIndex[i]);

            // 止める(手牌に加える演出時間)
            yield return new WaitForSeconds(ViewManager.HAND_TILE_MOVE_TIME + 0.1f);

            // 手牌がそろったか判定(手牌が12枚以外の事なんてないからマジックナンバーでもよい！！)
            if (_handTilesKindList.Count >= 12)
            {
                // 役の判定
                _handTilesKindList.Add(MahjongLogic.TILE_KIND.SOO_1);
                _handTilesKindList.Add(MahjongLogic.TILE_KIND.SOO_1);
                MahjongLogic.Role role = MahjongLogic.CalcHandTilesRole(_handTilesKindList, MahjongLogic.TILE_KIND.MAN_3, MahjongLogic.TILE_KIND.TON);

                // 消す(一旦ね)
                _viewManager.ClearHandTiles();

                // 止める(手牌に加える演出時間 + 攻撃演出時間)
                yield return new WaitForSeconds(0.5f);

                // 手牌クリア
                _handTilesKindList.Clear();
            }
        }

        // 落とす
        _viewManager.FallPuzzleTile();

        // 落下時間
        yield return new WaitForSeconds(fallY.Max() * ViewManager.PUZZLE_TILE_FALL_TIME);

        // アニメーション終了
        _isAnimation = false;

        // マッチ終了
        _puzzleManager.FinishMatch();
    }
}
