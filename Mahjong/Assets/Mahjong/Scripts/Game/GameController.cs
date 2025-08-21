using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

public class GameController : MonoBehaviour
{
    [SerializeField] private TouchInputHandler _input;
    [SerializeField] private ViewManager _viewManager;

    // パズルマネージャー
    private PuzzleManager _puzzleManager;

    // ***** READY
    // 移動開始位置
    private Vector2Int? _currentMoveIndex = null;

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
        switch (_puzzleManager.state)
        {
            case PuzzleManager.GameState.READY:
                UpdateReady();
                break;
            case PuzzleManager.GameState.MATCH:
                break;
            case PuzzleManager.GameState.PAUSE:
                break;
        }
    }

    // ステートがREADYの際の処理
    private void UpdateReady()
    {
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

            Vector2Int? newIndex = _viewManager.CalcTouchPuzzleTileIndex(_input.GetCurrentDragPosition());
            if (!newIndex.HasValue)
            {
                _puzzleManager.MoveEnd(_currentMoveIndex.Value);
                _currentMoveIndex = null;
                return;
            }

            // 移動
            if (newIndex.Value.x != _currentMoveIndex.Value.x || newIndex.Value.y != _currentMoveIndex.Value.y)
            {
                _currentMoveIndex = newIndex;
                _puzzleManager.MoveNow(_currentMoveIndex.Value);
            }
        }
    }

    private void InitGame()
    {
        // 使用する牌
        List<MahjongLogic.TILE_KIND> useTileKinds = new List<MahjongLogic.TILE_KIND>();
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_1);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_2);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_3);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_4);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_5);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_6);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_7);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_8);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_9);
        useTileKinds.Add(MahjongLogic.TILE_KIND.TON);
        useTileKinds.Add(MahjongLogic.TILE_KIND.NAN);
        useTileKinds.Add(MahjongLogic.TILE_KIND.SYA);
        useTileKinds.Add(MahjongLogic.TILE_KIND.PEE);
        useTileKinds.Add(MahjongLogic.TILE_KIND.HAKU);
        useTileKinds.Add(MahjongLogic.TILE_KIND.HATU);
        useTileKinds.Add(MahjongLogic.TILE_KIND.TYUN);
        // パズルの初期化
        _puzzleManager.InitPuzzle(useTileKinds);

        // 牌の配置
        _viewManager.CreatePuzzleBoard();
    }
}
