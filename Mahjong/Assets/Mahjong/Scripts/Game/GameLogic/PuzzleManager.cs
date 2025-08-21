using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PuzzleManager
{
    public enum GameState
    {
        READY = 0, // 準備完了(プレイヤー操作可)
        MATCH, // 牌移動中(プレイヤー操作不可)
        PAUSE, // 一時停止中(プレイヤー操作不可)
        MAX,
    }
    private enum MatchState
    {
        NONE = 0, // 移動中ではない
        SWITCH_TRY, // 牌入れ替え中
        SWITCH_PREV, // 牌戻し中
        MATCH, // マッチ(牌消去)
        FALL, //落下中
        MAX,
    }

    // ***** Public変数
    // ゲームステート
    public GameState state { get; } = GameState.READY;

    // ボードタイル配列
    public MahjongLogic.TILE_KIND[,] _boardTiles { get; } = new MahjongLogic.TILE_KIND[GameData.PUZZLE_BOARD_SIZE_Y, GameData.PUZZLE_BOARD_SIZE_X];


    // ***** Private変数
    // ムーブステート(ゲームステートの中のMATCHのステート)
    private MatchState _matchState = MatchState.NONE;

    // 使用牌種リスト
    private List<MahjongLogic.TILE_KIND> _useTiles;

    // *** READY
    // 移動開始位置
    private Vector2? _beginMoveIndex = null;
    // 現在移動位置
    private Vector2 _nowMoveIndex;
    // 移動位置履歴
    private List<Vector2> _moveIndexHistory = new List<Vector2>();

    // ***** Public関数
    // パズルの初期化
    public void InitPuzzle(List<MahjongLogic.TILE_KIND> useTiles)
    {
        _useTiles = useTiles;
        InitBoardTiles();
    }
    
    // 指移動中
    public void MoveNow(Vector2Int index)
    {
        Debug.Log("移動:" + index);
        if (_beginMoveIndex.HasValue)
        {
            // 移動している
            if (_nowMoveIndex.x != index.x || _nowMoveIndex.y != index.y)
            {
                _moveIndexHistory.Add(index);
                // マッチ判定
            }
        }
        else
        {
            // 開始
            _beginMoveIndex = index;
            _nowMoveIndex = index;
            _moveIndexHistory.Clear();
            _moveIndexHistory.Add(index);
        }
    }

    // 指移動終了
    public void MoveEnd(Vector2Int index)
    {
        Debug.Log("終了:" + index);
    }

    // ***** Private関数
    // ボードタイルの初期化
    private void InitBoardTiles()
    {
        int kindNum = _useTiles.Count;

        for(int y = 0; y < GameData.PUZZLE_BOARD_SIZE_Y; y++)
        {
            for (int x = 0; x < GameData.PUZZLE_BOARD_SIZE_X; x++)
            {
                _boardTiles[y, x] = _useTiles[Random.Range(0,  kindNum)];
            }
        }
    }

    // マッチしているかの判定
    private bool MatchingCheck(Vector2Int index)
    {
        MahjongLogic.TILE_KIND[] adjacentTile = new MahjongLogic.TILE_KIND[4]; // 上下左右

        // 上
        if (index.y > 0)
            adjacentTile[0] = _boardTiles[index.x, index.y - 1];
        // 下
        if (index.y < GameData.PUZZLE_BOARD_SIZE_Y - 1)
            adjacentTile[1] = _boardTiles[index.x, index.y + 1];
        // 左
        if (index.x > 0)
            adjacentTile[0] = _boardTiles[index.x - 1, index.y];
        // 右
        if (index.x < GameData.PUZZLE_BOARD_SIZE_X - 1)
            adjacentTile[1] = _boardTiles[index.x + 1, index.y];



        return false;
    }
}
