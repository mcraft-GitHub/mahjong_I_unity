using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static UnityEngine.Rendering.VolumeComponent;

public class PuzzleManager
{
    public enum GameState
    {
        READY = 0, // 準備完了(プレイヤー操作可)
        MATCH, // 牌移動中(プレイヤー操作不可)
        PREV_MOVE, // 移動牌戻し中(プレイヤー操作不可)←これいらないかも。使用次第
        PAUSE, // 一時停止中(プレイヤー操作不可)
        MAX,
    }

    // ***** Public変数
    // ゲームステート
    public GameState state { get; private set; } = GameState.READY;

    // ボードタイル配列
    public MahjongLogic.TILE_KIND[,] boardTiles { get; } = new MahjongLogic.TILE_KIND[GameData.PUZZLE_BOARD_SIZE_Y, GameData.PUZZLE_BOARD_SIZE_X];

    // *** READY

    // *** MATCH
    // マッチ牌インデックス
    public List<Vector2Int[]> matchingTilesIndex { get; } = new List<Vector2Int[]>();

    // ***** Private変数
    // 使用牌種リスト
    private List<MahjongLogic.TILE_KIND> _useTiles;

    // *** READY
    // 移動開始位置
    private Vector2Int? _beginMoveIndex = null;
    // 現在移動位置
    private Vector2Int _nowMoveIndex;
    // 移動位置履歴
    private List<Vector2> _moveIndexHistory = new List<Vector2>();

    // *** MATCH


    // ***** Public関数
    /// <summary>
    /// パズルの初期化
    /// </summary>
    /// <param name="useTiles">パズルに使用する牌の種類リスト</param>
    public void InitPuzzle(List<MahjongLogic.TILE_KIND> useTiles)
    {
        _useTiles = useTiles;
        InitBoardTiles();

        _beginMoveIndex = null;
        _moveIndexHistory.Clear();
        matchingTilesIndex.Clear();
    }

    /// <summary>
    /// 指移動中
    /// </summary>
    /// <param name="index">選択中盤面インデックス</param>
    public void MoveNow(Vector2Int index)
    {
        if (_beginMoveIndex.HasValue)
        {
            // 移動している
            if (_nowMoveIndex.x != index.x || _nowMoveIndex.y != index.y)
            {
                _moveIndexHistory.Add(index);
                // マッチ判定
                SwitchingTile(_nowMoveIndex, index);
            }
            _nowMoveIndex = index;
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

    /// <summary>
    /// 指移動終了
    /// </summary>
    /// <param name="index">選択中盤面インデックス</param>
    public void MoveEnd(Vector2Int index)
    {
        // 移動していない
        if (_moveIndexHistory.Count <= 1)
        {
            _beginMoveIndex = null;
            return;
        }

        // 移動している
        //Debug.Log("ステート変更：" + state + " > " + GameState.PREV_MOVE);
        //state = GameState.PREV_MOVE;

        _beginMoveIndex = null;
    }

    /// <summary>
    /// マッチング処理の終了
    /// </summary>
    public void FinishMatching()
    {
        // いろいろクリア
        _beginMoveIndex = null;
        _moveIndexHistory.Clear();
        matchingTilesIndex.Clear();

        // 落ちコンの判定(コンボじゃないけど)
        // ほんとは落ちた列の周りの牌だけでいいけど、全部確認
        for (int y = 0; y < GameData.PUZZLE_BOARD_SIZE_Y; y++)
        {
            for (int x = 0; x < GameData.PUZZLE_BOARD_SIZE_X; x++)
            {
                MatchingCheck(new Vector2Int(x, y));
            }
        }

        if (matchingTilesIndex.Count > 0)
        {
            // マッチしている
            MatchingProcess();
        }
        else
        {
            // マッチしていない
            Debug.Log("ステート変更：" + state + " > " + GameState.READY);
            state = GameState.READY;
        }
    }

    // ***** Private関数
    /// <summary>
    /// ボードタイルの初期化
    /// </summary>
    private void InitBoardTiles()
    {
        int kindNum = _useTiles.Count;

        // ランダムに生成
        for (int y = 0; y < GameData.PUZZLE_BOARD_SIZE_Y; y++)
        {
            for (int x = 0; x < GameData.PUZZLE_BOARD_SIZE_X; x++)
            {
                boardTiles[y, x] = _useTiles[UnityEngine.Random.Range(0, kindNum)];
            }
        }

        // 初手でマッチしている場所を修正
        for (int y = 0; y < GameData.PUZZLE_BOARD_SIZE_Y; y++)
        {
            for (int x = 0; x < GameData.PUZZLE_BOARD_SIZE_X; x++)
            {
                if (MatchingCheck(new Vector2Int(x, y), true))
                    SetBoardUnmatchRandomKind(new Vector2Int(x, y));
            }
        }

        // 再度確認
        for (int y = 0; y < GameData.PUZZLE_BOARD_SIZE_Y; y++)
        {
            for (int x = 0; x < GameData.PUZZLE_BOARD_SIZE_X; x++)
            {
                if (MatchingCheck(new Vector2Int(x, y), true))
                    Debug.Log("バグや！初手マッチ！：" + new Vector2Int(x, y));
            }
        }
    }

    /// <summary>
    /// 入れ替え処理(入れ替え＆マッチ判定)
    /// </summary>
    /// <param name="tile1">入れ替え牌の盤面インデックス1</param>
    /// <param name="tile2">入れ替え牌の盤面インデックス2</param>
    private void SwitchingTile(Vector2Int tile1, Vector2Int tile2)
    {
        // 入れ替え処理
        (boardTiles[tile1.y, tile1.x], boardTiles[tile2.y, tile2.x]) = (boardTiles[tile2.y, tile2.x], boardTiles[tile1.y, tile1.x]);

        // マッチしているか
        bool isMatch = false;

        // 入れ替えた牌のマッチ判定
        isMatch = MatchingCheck(tile1) || isMatch;
        isMatch = MatchingCheck(tile2) || isMatch;

        // 入れ替えた牌の周りの牌のマッチ判定
        if (tile1.y > 0) isMatch = MatchingCheck(new Vector2Int(tile1.x, tile1.y - 1)) || isMatch;
        if (tile1.y < GameData.PUZZLE_BOARD_SIZE_Y - 1) isMatch = MatchingCheck(new Vector2Int(tile1.x, tile1.y + 1)) || isMatch;
        if (tile1.x > 0) isMatch = MatchingCheck(new Vector2Int(tile1.x - 1, tile1.y)) || isMatch;
        if (tile1.x < GameData.PUZZLE_BOARD_SIZE_X - 1) isMatch = MatchingCheck(new Vector2Int(tile1.x + 1, tile1.y)) || isMatch;
        if (tile2.y > 0) isMatch = MatchingCheck(new Vector2Int(tile2.x, tile2.y - 1)) || isMatch;
        if (tile2.y < GameData.PUZZLE_BOARD_SIZE_Y - 1) isMatch = MatchingCheck(new Vector2Int(tile2.x, tile2.y + 1)) || isMatch;
        if (tile2.x > 0) isMatch = MatchingCheck(new Vector2Int(tile2.x - 1, tile2.y)) || isMatch;
        if (tile2.x < GameData.PUZZLE_BOARD_SIZE_X - 1) isMatch = MatchingCheck(new Vector2Int(tile2.x + 1, tile2.y)) || isMatch;

        if (isMatch)
            MatchingProcess();
    }

    /// <summary>
    /// マッチしているかの判定
    /// </summary>
    /// <param name="index">チェック牌の盤面インデックス</param>
    /// <param name="prev">事前チェックか(マッチ時の処理をしないか)</param>
    /// <returns>マッチしたか</returns>
    private bool MatchingCheck(Vector2Int index, bool prev = false)
    {
        // 上下左右の牌種
        MahjongLogic.TILE_KIND[] adjacentTile = {
            MahjongLogic.TILE_KIND.NONE, MahjongLogic.TILE_KIND.NONE, MahjongLogic.TILE_KIND.NONE, MahjongLogic.TILE_KIND.NONE };

        // 左右
        if (index.x > 0)
            adjacentTile[2] = boardTiles[index.y, index.x - 1];
        if (index.x < GameData.PUZZLE_BOARD_SIZE_X - 1)
            adjacentTile[3] = boardTiles[index.y, index.x + 1];
        if (MahjongLogic.CheckMentu(boardTiles[index.y, index.x], adjacentTile[2], adjacentTile[3]))
        {
            if (!prev)
            {
                // マッチした牌をなくす
                boardTiles[index.y, index.x] = MahjongLogic.TILE_KIND.NONE;
                boardTiles[index.y, index.x - 1] = MahjongLogic.TILE_KIND.NONE;
                boardTiles[index.y, index.x + 1] = MahjongLogic.TILE_KIND.NONE;
                // 追加
                matchingTilesIndex.Add(new Vector2Int[3] { index, new Vector2Int(index.x - 1, index.y), new Vector2Int(index.x + 1, index.y) });
            }
                
            return true;
        }

        // 上下
        if (index.y > 0)
            adjacentTile[0] = boardTiles[index.y - 1, index.x];
        if (index.y < GameData.PUZZLE_BOARD_SIZE_Y - 1)
            adjacentTile[1] = boardTiles[index.y + 1, index.x];
        if (MahjongLogic.CheckMentu(boardTiles[index.y, index.x], adjacentTile[0], adjacentTile[1]))
        {
            if (!prev)
            {
                // マッチした牌をなくす
                boardTiles[index.y, index.x] = MahjongLogic.TILE_KIND.NONE;
                boardTiles[index.y - 1, index.x] = MahjongLogic.TILE_KIND.NONE;
                boardTiles[index.y + 1, index.x] = MahjongLogic.TILE_KIND.NONE;
                // 追加
                matchingTilesIndex.Add(new Vector2Int[3] { new Vector2Int(index.x, index.y - 1), index, new Vector2Int(index.x, index.y + 1) });
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// マッチしていた場合の処理
    /// </summary>
    private void MatchingProcess()
    {
        Debug.Log("ステート変更：" + state + " > " + GameState.MATCH);
        state = GameState.MATCH;

        // 牌を落とす
        int kindNum = _useTiles.Count;
        for (int i = 0; i < matchingTilesIndex.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Vector2Int idx = matchingTilesIndex[i][j];

                // 下ににずらす
                for (int y = idx.y; y > 0; y--)
                    boardTiles[y, idx.x] = boardTiles[y - 1, idx.x];

                // 一番上にはランダムの牌種を入れる
                boardTiles[0, idx.x] = _useTiles[UnityEngine.Random.Range(0, kindNum)];
            }
        }
    }

    /// <summary>
    /// 周りとマッチしないランダムな牌種の設定
    /// </summary>
    /// <param name="index">設定牌の盤面インデックス</param>
    private void SetBoardUnmatchRandomKind(Vector2Int index)
    {
        bool isMatch = true;
        int kindNum = _useTiles.Count;
        // ランダムで生成し続けて「マッチしてなかったらラッキー」っていうコードだからあんまりよくないココ
        while (isMatch)
        {
            Debug.Log("無限ループ疑惑");

            isMatch = false;

            // もう一度ランダム取得
            boardTiles[index.y, index.x] = _useTiles[UnityEngine.Random.Range(0, kindNum)];

            if (MatchingCheck(new Vector2Int(index.x, index.y), true)) { isMatch = true; continue; }
            // 上下左右の確認
            if (index.y > 0 && MatchingCheck(new Vector2Int(index.x, index.y - 1), true)) { isMatch = true; continue; }
            if (index.y < GameData.PUZZLE_BOARD_SIZE_Y - 1 && MatchingCheck(new Vector2Int(index.x, index.y + 1), true)) { isMatch = true; continue; }
            if (index.x > 0 && MatchingCheck(new Vector2Int(index.x - 1, index.y), true)) { isMatch = true; continue; }
            if (index.x < GameData.PUZZLE_BOARD_SIZE_X - 1 && MatchingCheck(new Vector2Int(index.x + 1, index.y), true)) { isMatch = true; continue; }
        }
    }
}
