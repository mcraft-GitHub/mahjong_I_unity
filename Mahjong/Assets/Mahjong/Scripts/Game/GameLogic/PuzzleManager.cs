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

    // 雀頭牌種
    public MahjongLogic.TILE_KIND headTilesKind { get; private set; } = MahjongLogic.TILE_KIND.NONE;

    // *** READY

    // *** MATCH
    // マッチ牌インデックス
    public List<Vector2Int[]> matchTilesIndex { get; } = new List<Vector2Int[]>();
    // マッチ牌種
    public List<MahjongLogic.TILE_KIND[]> matchTilesKind { get; } = new List<MahjongLogic.TILE_KIND[]>();

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
        matchTilesIndex.Clear();
        matchTilesKind.Clear();

        // 雀頭牌の決定
        headTilesKind = _useTiles[UnityEngine.Random.Range(0, _useTiles.Count)];
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
                SwitchTile(_nowMoveIndex, index);
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
    public void FinishMatch()
    {
        // いろいろクリア
        _beginMoveIndex = null;
        _moveIndexHistory.Clear();
        matchTilesIndex.Clear();
        matchTilesKind.Clear();

        // 落ちコンの判定(コンボじゃないけど)
        // ほんとは落ちた列の周りの牌だけでいいけど、全部確認
        for (int y = 0; y < GameData.PUZZLE_BOARD_SIZE_Y; y++)
        {
            for (int x = 0; x < GameData.PUZZLE_BOARD_SIZE_X; x++)
            {
                MatchCheck(x, y);
            }
        }

        if (matchTilesIndex.Count > 0)
        {
            // マッチしている
            MatchProcess();
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
                if (MatchCheck(x, y, true))
                    SetBoardUnmatchRandomKind(x, y);
            }
        }

        // 再度確認
        for (int y = 0; y < GameData.PUZZLE_BOARD_SIZE_Y; y++)
        {
            for (int x = 0; x < GameData.PUZZLE_BOARD_SIZE_X; x++)
            {
                if (MatchCheck(x, y, true))
                    Debug.Log("バグや！初手マッチ！：" + new Vector2Int(x, y));
            }
        }
    }

    /// <summary>
    /// 入れ替え処理(入れ替え＆マッチ判定)
    /// </summary>
    /// <param name="tile1">入れ替え牌の盤面インデックス1</param>
    /// <param name="tile2">入れ替え牌の盤面インデックス2</param>
    private void SwitchTile(Vector2Int tile1, Vector2Int tile2)
    {
        // 入れ替え処理
        (boardTiles[tile1.y, tile1.x], boardTiles[tile2.y, tile2.x]) = (boardTiles[tile2.y, tile2.x], boardTiles[tile1.y, tile1.x]);

        // マッチしているか
        bool isMatch = false;

        // 入れ替えた牌のマッチ判定
        isMatch = MatchCheck(tile1.x, tile1.y) || isMatch;
        isMatch = MatchCheck(tile2.x, tile2.y) || isMatch;

        // 入れ替えた牌の周りの牌のマッチ判定
        isMatch = MatchCheckFourSides(tile1.x, tile1.y) || isMatch;
        isMatch = MatchCheckFourSides(tile2.x, tile2.y) || isMatch;

        if (isMatch)
            MatchProcess();
    }

    /// <summary>
    /// 指定した盤面インデックスの周りの牌がマッチしているか
    /// </summary>
    /// <param name="indexX">盤面Xインデックス</param>
    /// <param name="indexY">盤面Yインデックス</param>
    /// <returns>マッチしているか</returns>
    private bool MatchCheckFourSides(int indexX, int indexY)
    {
        bool isMatch = false;
        // 上
        if (indexY > 0)
            isMatch = MatchCheck(indexX, indexY - 1);
        // 下    
        if (indexY < GameData.PUZZLE_BOARD_SIZE_Y - 1)
            isMatch = MatchCheck(indexX, indexY + 1) || isMatch;
        // 左
        if (indexX > 0)
            isMatch = MatchCheck(indexX - 1, indexY) || isMatch;
        // 右
        if (indexX < GameData.PUZZLE_BOARD_SIZE_X - 1)
            isMatch = MatchCheck(indexX + 1, indexY) || isMatch;
        return isMatch;
    }

    /// <summary>
    /// マッチしているかの判定
    /// </summary>
    /// <param name="indexX">チェック牌の盤面Xインデックス</param>
    /// <param name="indexY">チェック牌の盤面Yインデックス</param>
    /// <param name="prev">事前チェックか(マッチ時の処理をしないか)</param>
    /// <returns>マッチしたか</returns>
    private bool MatchCheck(int indexX, int indexY, bool prev = false)
    {
        // 上下左右の牌種
        MahjongLogic.TILE_KIND[] adjacentTile = {
            MahjongLogic.TILE_KIND.NONE, MahjongLogic.TILE_KIND.NONE, MahjongLogic.TILE_KIND.NONE, MahjongLogic.TILE_KIND.NONE };

        // 左右
        if (indexX > 0)
            adjacentTile[2] = boardTiles[indexY, indexX - 1];
        if (indexX < GameData.PUZZLE_BOARD_SIZE_X - 1)
            adjacentTile[3] = boardTiles[indexY, indexX + 1];
        if (MahjongLogic.CheckMentu(boardTiles[indexY, indexX], adjacentTile[2], adjacentTile[3]) > 0)
        {
            if (!prev)
            {
                // 追加
                matchTilesIndex.Add(new Vector2Int[3] { new Vector2Int(indexX, indexY), new Vector2Int(indexX - 1, indexY), new Vector2Int(indexX + 1, indexY) });
                matchTilesKind.Add(new MahjongLogic.TILE_KIND[] { boardTiles[indexY, indexX], boardTiles[indexY, indexX - 1], boardTiles[indexY, indexX + 1] });

                // マッチした牌をなくす
                boardTiles[indexY, indexX] = MahjongLogic.TILE_KIND.NONE;
                boardTiles[indexY, indexX - 1] = MahjongLogic.TILE_KIND.NONE;
                boardTiles[indexY, indexX + 1] = MahjongLogic.TILE_KIND.NONE;
            }
                
            return true;
        }

        // 上下
        if (indexY > 0)
            adjacentTile[0] = boardTiles[indexY - 1, indexX];
        if (indexY < GameData.PUZZLE_BOARD_SIZE_Y - 1)
            adjacentTile[1] = boardTiles[indexY + 1, indexX];
        if (MahjongLogic.CheckMentu(boardTiles[indexY, indexX], adjacentTile[0], adjacentTile[1]) > 0)
        {
            if (!prev)
            {
                // 追加
                matchTilesIndex.Add(new Vector2Int[3] { new Vector2Int(indexX, indexY - 1), new Vector2Int(indexX, indexY), new Vector2Int(indexX, indexY + 1) });
                matchTilesKind.Add(new MahjongLogic.TILE_KIND[] { boardTiles[indexY, indexX], boardTiles[indexY - 1, indexX], boardTiles[indexY + 1, indexX] });

                // マッチした牌をなくす
                boardTiles[indexY, indexX] = MahjongLogic.TILE_KIND.NONE;
                boardTiles[indexY - 1, indexX] = MahjongLogic.TILE_KIND.NONE;
                boardTiles[indexY + 1, indexX] = MahjongLogic.TILE_KIND.NONE;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// マッチしていた場合の処理
    /// </summary>
    private void MatchProcess()
    {
        Debug.Log("ステート変更：" + state + " > " + GameState.MATCH);
        state = GameState.MATCH;

        // 牌を落とす
        int kindNum = _useTiles.Count;
        for (int i = 0; i < matchTilesIndex.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Vector2Int idx = matchTilesIndex[i][j];

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
    /// <param name="indexX">設定牌の盤面Xインデックス</param>
    /// <param name="indexY">設定牌の盤面Yインデックス</param>
    private void SetBoardUnmatchRandomKind(int indexX, int indexY)
    {
        bool isMatch = true;
        int kindNum = _useTiles.Count;
        // ランダムで生成し続けて「マッチしてなかったらラッキー」っていうコードだからあんまりよくないココ
        while (isMatch)
        {
            Debug.Log("無限ループ疑惑");

            isMatch = false;

            // もう一度ランダム取得
            boardTiles[indexY, indexX] = _useTiles[UnityEngine.Random.Range(0, kindNum)];

            if (MatchCheck(indexX, indexY, true)) 
                isMatch = true;
            // 上
            else if (indexY > 0 && MatchCheck(indexX, indexY - 1, true)) 
                isMatch = true; 
            // 下
            else if (indexY < GameData.PUZZLE_BOARD_SIZE_Y - 1 && MatchCheck(indexX, indexY + 1, true)) 
                isMatch = true;
            // 左
            else if (indexX > 0 && MatchCheck(indexX - 1, indexY, true)) 
                isMatch = true;
            // 右
            else if (indexX < GameData.PUZZLE_BOARD_SIZE_X - 1 && MatchCheck(indexX + 1, indexY, true)) 
                isMatch = true; 
        }
    }
}
