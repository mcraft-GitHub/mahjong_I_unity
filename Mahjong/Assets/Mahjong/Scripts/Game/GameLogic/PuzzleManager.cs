using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager
{
    // ***** Public変数
    // ボードタイル配列
    public MahjongLogic.TILE_KIND[,] _boardTiles { get; } = new MahjongLogic.TILE_KIND[GameData.PUZZLE_BOARD_SIZE_Y, GameData.PUZZLE_BOARD_SIZE_X];

    // マッチした面子
    public List<MahjongLogic.GameMentu> _matchMentu = new List<MahjongLogic.GameMentu>();

    // ***** Private変数
    // 使用牌種リスト
    private List<MahjongLogic.TILE_KIND> _useTiles;

    // *** READY
    // 移動開始位置
    private Vector2Int? _beginMoveIndex = null;
    // 現在移動位置
    private Vector2Int _nowMoveIndex;
    // 移動位置履歴
    private List<Vector2Int> _moveIndexHistory = new List<Vector2Int>();

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
    }

    /// <summary>
    /// 指移動中
    /// </summary>
    /// <param name="index">選択中盤面インデックス</param>
    /// <param name="gameData">ゲームデータ</param>
    public void MoveNow(Vector2Int index, GameController.GameData gameData)
    {
        if (_beginMoveIndex.HasValue)
        {
            // 移動している
            if (_nowMoveIndex.x != index.x || _nowMoveIndex.y != index.y)
            {
                _moveIndexHistory.Add(index);
                // マッチ判定
                SwitchTile(_nowMoveIndex, index, gameData);
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

        _beginMoveIndex = null;
    }

    /// <summary>
    /// マッチング処理の終了
    /// </summary>
    /// <param name="gameData">ゲームデータ</param>
    public void FinishMatch(GameController.GameData gameData)
    {
        // いろいろクリア
        _beginMoveIndex = null;
        _moveIndexHistory.Clear();

        // 落ちコンの判定(コンボじゃないけど)
        // ほんとは落ちた列の周りの牌だけでいいけど、全部確認
        for (int y = 0; y < GameData.PUZZLE_BOARD_SIZE_Y; y++)
        {
            for (int x = 0; x < GameData.PUZZLE_BOARD_SIZE_X; x++)
            {
                MatchCheck(x, y, gameData);
            }
        }

        if (_matchMentu.Count > 0)
        {
            // マッチしている
            MatchProcess(gameData);
        }
        else
        {
            // マッチしていない場合は、マッチ処理を終了する
            gameData._currentState = GameController.GAME_STATE.PUZZLE;
        }
    }

    // ***** Private関数
    /// <summary>
    /// ボードタイルの初期化
    /// </summary>
    private void InitBoardTiles()
    {
        // ランダムに生成
        for (int y = 0; y < GameData.PUZZLE_BOARD_SIZE_Y; y++)
        {
            for (int x = 0; x < GameData.PUZZLE_BOARD_SIZE_X; x++)
            {
                _boardTiles[y, x] = MahjongLogic.GetRandomTileKind(_useTiles);
            }
        }

        // 初手でマッチしている場所を修正
        for (int y = 0; y < GameData.PUZZLE_BOARD_SIZE_Y; y++)
        {
            for (int x = 0; x < GameData.PUZZLE_BOARD_SIZE_X; x++)
            {
                if (MatchCheck(x, y))
                    SetBoardUnmatchRandomKind(x, y);
            }
        }

        // 再度確認
        for (int y = 0; y < GameData.PUZZLE_BOARD_SIZE_Y; y++)
        {
            for (int x = 0; x < GameData.PUZZLE_BOARD_SIZE_X; x++)
            {
                if (MatchCheck(x, y))
                    Debug.LogError("バグや！初手マッチ！：" + (x, y));
            }
        }
    }

    /// <summary>
    /// 入れ替え処理(入れ替え＆マッチ判定)
    /// </summary>
    /// <param name="tile1">入れ替え牌の盤面インデックス1</param>
    /// <param name="tile2">入れ替え牌の盤面インデックス2</param>
    /// <param name="gameData">ゲームデータ</param>
    private void SwitchTile(Vector2Int tile1, Vector2Int tile2, GameController.GameData gameData)
    {
        // 入れ替え処理
        (_boardTiles[tile1.y, tile1.x], _boardTiles[tile2.y, tile2.x]) = (_boardTiles[tile2.y, tile2.x], _boardTiles[tile1.y, tile1.x]);

        // マッチしているか
        bool isMatchTile1 = false;
        bool isMatchTile2 = false;
        bool isMatchTile1FourSides = false;
        bool isMatchTile2FourSides = false;

        // 入れ替えた牌のマッチ判定
        isMatchTile1 = MatchCheck(tile1.x, tile1.y, gameData);
        isMatchTile2 = MatchCheck(tile2.x, tile2.y, gameData);

        // 入れ替えた牌の周りの牌のマッチ判定
        isMatchTile1FourSides = MatchCheckFourSides(tile1.x, tile1.y, gameData);
        isMatchTile2FourSides = MatchCheckFourSides(tile2.x, tile2.y, gameData);

        if (isMatchTile1 || isMatchTile2 || isMatchTile1FourSides || isMatchTile2FourSides)
            MatchProcess(gameData);
    }

    /// <summary>
    /// 指定した盤面インデックスの周りの牌がマッチしているか
    /// </summary>
    /// <param name="indexX">盤面Xインデックス</param>
    /// <param name="indexY">盤面Yインデックス</param>
    /// <param name="gameData">ゲームデータ</param>
    /// <returns>マッチしているか</returns>
    private bool MatchCheckFourSides(int indexX, int indexY, GameController.GameData gameData)
    {
        bool isMatchUp = false;
        bool isMatchDown = false;
        bool isMatchLeft = false;
        bool isMatchRight = false;
        // 上
        if (indexY > 0)
            isMatchUp = MatchCheck(indexX, indexY - 1, gameData);
        // 下    
        if (indexY < GameData.PUZZLE_BOARD_SIZE_Y - 1)
            isMatchDown = MatchCheck(indexX, indexY + 1, gameData);
        // 左
        if (indexX > 0)
            isMatchLeft = MatchCheck(indexX - 1, indexY, gameData);
        // 右
        if (indexX < GameData.PUZZLE_BOARD_SIZE_X - 1)
            isMatchRight = MatchCheck(indexX + 1, indexY, gameData);
        return isMatchUp || isMatchDown || isMatchLeft || isMatchRight;
    }

    /// <summary>
    /// マッチしているかの判定
    /// </summary>
    /// <param name="indexX">チェック牌の盤面Xインデックス</param>
    /// <param name="indexY">チェック牌の盤面Yインデックス</param>
    /// <param name="gameData">ゲームデータ</param>
    /// <returns>マッチしたか</returns>
    private bool MatchCheck(int indexX, int indexY, GameController.GameData gameData = null)
    {
        // 上下左右の牌種
        MahjongLogic.TILE_KIND[] adjacentTile = {
            MahjongLogic.TILE_KIND.NONE, MahjongLogic.TILE_KIND.NONE, MahjongLogic.TILE_KIND.NONE, MahjongLogic.TILE_KIND.NONE };

        // 左右
        if (indexX > 0)
            adjacentTile[2] = _boardTiles[indexY, indexX - 1];
        if (indexX < GameData.PUZZLE_BOARD_SIZE_X - 1)
            adjacentTile[3] = _boardTiles[indexY, indexX + 1];
        if (MahjongLogic.CheckMentu(_boardTiles[indexY, indexX], adjacentTile[2], adjacentTile[3]) > 0)
        {
            if (gameData != null)
            {
                // 追加
                MahjongLogic.GameMentu mentu = new MahjongLogic.GameMentu();
                mentu.tilesIndex[0] = new Vector2Int(indexX - 1, indexY);
                mentu.tilesIndex[1] = new Vector2Int(indexX, indexY);
                mentu.tilesIndex[2] = new Vector2Int(indexX + 1, indexY);
                mentu.tilesKind[0] = _boardTiles[indexY, indexX - 1];
                mentu.tilesKind[1] = _boardTiles[indexY, indexX];
                mentu.tilesKind[2] = _boardTiles[indexY, indexX + 1];
                _matchMentu.Add(mentu);

                // マッチした牌をなくす
                _boardTiles[indexY, indexX] = MahjongLogic.TILE_KIND.NONE;
                _boardTiles[indexY, indexX - 1] = MahjongLogic.TILE_KIND.NONE;
                _boardTiles[indexY, indexX + 1] = MahjongLogic.TILE_KIND.NONE;
            }
                
            return true;
        }

        // 上下
        if (indexY > 0)
            adjacentTile[0] = _boardTiles[indexY - 1, indexX];
        if (indexY < GameData.PUZZLE_BOARD_SIZE_Y - 1)
            adjacentTile[1] = _boardTiles[indexY + 1, indexX];
        if (MahjongLogic.CheckMentu(_boardTiles[indexY, indexX], adjacentTile[0], adjacentTile[1]) > 0)
        {
            if (gameData != null)
            {
                // 追加
                MahjongLogic.GameMentu mentu = new MahjongLogic.GameMentu();
                mentu.tilesIndex[0] = new Vector2Int(indexX, indexY - 1);
                mentu.tilesIndex[1] = new Vector2Int(indexX, indexY);
                mentu.tilesIndex[2] = new Vector2Int(indexX, indexY + 1);
                mentu.tilesKind[0] = _boardTiles[indexY - 1, indexX];
                mentu.tilesKind[1] = _boardTiles[indexY, indexX];
                mentu.tilesKind[2] = _boardTiles[indexY + 1, indexX];
                _matchMentu.Add(mentu);

                // マッチした牌をなくす
                _boardTiles[indexY, indexX] = MahjongLogic.TILE_KIND.NONE;
                _boardTiles[indexY - 1, indexX] = MahjongLogic.TILE_KIND.NONE;
                _boardTiles[indexY + 1, indexX] = MahjongLogic.TILE_KIND.NONE;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// マッチしていた場合の処理
    /// </summary>
    /// <param name="gameData">ゲームデータ</param>
    private void MatchProcess(GameController.GameData gameData)
    {
        // ステートの切り替え
        gameData._currentState = GameController.GAME_STATE.MATCHING_RESULT;

        // 牌を落とす
        for (int i = 0; i < _matchMentu.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Vector2Int idx = _matchMentu[i].tilesIndex[j];

                // 下ににずらす
                for (int y = idx.y; y > 0; y--)
                    _boardTiles[y, idx.x] = _boardTiles[y - 1, idx.x];

                // 一番上にはランダムの牌種を入れる
                _boardTiles[0, idx.x] = MahjongLogic.GetRandomTileKind(_useTiles);
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
        // ランダムで生成し続けて「マッチしてなかったらラッキー」っていうコードだからあんまりよくないココ
        while (isMatch)
        {
            Debug.Log("無限ループ疑惑");

            isMatch = false;

            // もう一度ランダム取得
            _boardTiles[indexY, indexX] = MahjongLogic.GetRandomTileKind(_useTiles);

            if (MatchCheck(indexX, indexY)) 
                isMatch = true;
            // 上
            else if (indexY > 0 && MatchCheck(indexX, indexY - 1)) 
                isMatch = true; 
            // 下
            else if (indexY < GameData.PUZZLE_BOARD_SIZE_Y - 1 && MatchCheck(indexX, indexY + 1)) 
                isMatch = true;
            // 左
            else if (indexX > 0 && MatchCheck(indexX - 1, indexY)) 
                isMatch = true;
            // 右
            else if (indexX < GameData.PUZZLE_BOARD_SIZE_X - 1 && MatchCheck(indexX + 1, indexY)) 
                isMatch = true; 
        }
    }
}
