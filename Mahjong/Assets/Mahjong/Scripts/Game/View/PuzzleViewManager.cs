using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuzzleViewManager : MonoBehaviour
{
    // パズル牌の移動時間
    public static readonly float PUZZLE_TILE_MOVE_TIME = 0.15f;

    // パズル牌の落下時間(1マス)
    public static readonly float PUZZLE_TILE_FALL_TIME = 0.2f;

    // 手牌の移動時間
    public static readonly float HAND_TILE_MOVE_TIME = 0.3f;

    // 麻雀牌プレハブ
    [SerializeField] private GameObject _tilePrefab;

    // 獲得牌の親オブジェクトTransform
    [SerializeField] private Transform _handTilesParent;
    // パズル牌の親オブジェクトTransform
    [SerializeField] private Transform _puzzleTilesParent;
    // 空の手牌の親オブジェクトTransform
    [SerializeField] private Transform _emptyHandTilesParent;

    // ドラ
    [SerializeField] private MahjongTileView _doraTile;
    // 自風
    [SerializeField] private MahjongTileView _jikazeTile;
    // 雀頭
    [SerializeField] private MahjongTileView _headTile1;
    [SerializeField] private MahjongTileView _headTile2;

    // パズル枠兼背景
    [SerializeField] private RectTransform _puzzleFrameRect;

    // ゲームコントローラー
    private GameController _gameController;
    // パズルマネージャー
    private PuzzleManager _puzzleManager;

    // パズル牌オブジェクト
    private MahjongTileView[,] _boardTileObjects = new MahjongTileView[GameData.PUZZLE_BOARD_SIZE_Y, GameData.PUZZLE_BOARD_SIZE_X];

    // 手牌オブジェクト
    private List<MahjongTileView> _handTileObjects = new List<MahjongTileView>();

    void Start()
    {
        // 雀頭牌とドラ牌と自風牌の配置・拡縮
        _headTile1.SetPos(new Vector2(GameUILayoutUtility._headTilesPosX[0], GameUILayoutUtility._auxiliaryTilesPosY));
        _headTile1.SetScale(GameUILayoutUtility._handTilesScale);
        _headTile2.SetPos(new Vector2(GameUILayoutUtility._headTilesPosX[1], GameUILayoutUtility._auxiliaryTilesPosY));
        _headTile2.SetScale(GameUILayoutUtility._handTilesScale);
        _doraTile.SetPos(new Vector2(GameUILayoutUtility._doraTilesPosX, GameUILayoutUtility._auxiliaryTilesPosY));
        _doraTile.SetScale(GameUILayoutUtility._handTilesScale);
        _jikazeTile.SetPos(new Vector2(GameUILayoutUtility._jikazeTilesPosX, GameUILayoutUtility._auxiliaryTilesPosY));
        _jikazeTile.SetScale(GameUILayoutUtility._handTilesScale);

        // パズル枠兼背景の配置・拡縮
        _puzzleFrameRect.anchoredPosition = new Vector2(0.0f, GameUILayoutUtility._puzzleBoardRect.center.y);
        _puzzleFrameRect.localScale = Vector3.one * GameUILayoutUtility._puzzleFrameScale;

        //*** 空の手牌の生成
        for (int i = 0; i < 12; i++)
        {
            // 生成
            GameObject obj = Instantiate(_tilePrefab, _emptyHandTilesParent);
            MahjongTileView tile = obj.GetComponent<MahjongTileView>();
            tile.SetPos(GameUILayoutUtility.CalcHandTilePosFromIndex(i));
            tile.SetScale(GameUILayoutUtility._handTilesScale);
            // 牌類のセット
            tile.SetKind(MahjongLogic.TILE_KIND.NONE);
        }
    }

    void Update()
    {
        
    }

    /// <summary>
    /// 必要クラスのセット
    /// </summary>
    /// <param name="gameController">ゲームコントローラー</param>
    /// <param name="puzzleManager">パズルマネージャー</param>
    public void SetClass(GameController gameController, PuzzleManager puzzleManager)
    {
        _gameController = gameController;
        _puzzleManager = puzzleManager;
    }

    /// <summary>
    /// パズル盤面の牌の生成
    /// </summary>
    public void CreatePuzzleBoard()
    { 
        MahjongLogic.TILE_KIND[,] boardTiles = _puzzleManager._boardTiles;

        // ループで生成
        for (int y = 0; y < boardTiles.GetLength(0); y++)
        {
            for (int x = 0; x < boardTiles.GetLength(1); x++)
            {
                GameObject obj = Instantiate(_tilePrefab, _puzzleTilesParent);
                MahjongTileView tile = obj.GetComponent<MahjongTileView>();
                _boardTileObjects[y, x] = tile;
                tile.SetPos(GameUILayoutUtility.CalcPuzzleTilePosFromIndex(x, y));
                tile.SetScale(GameUILayoutUtility._puzzleTilesViewScale);
                // 牌類のセット
                tile.SetKind(boardTiles[y, x]);
            }
        }
    }

    /// <summary>
    /// タッチされているパズル牌インデックス(この処理は正直このクラスじゃない気がする)
    /// </summary>
    /// <param name="touchPos">タッチ座標</param>
    /// <returns>タッチされた盤面インデックス(盤面外の場合はnull)</returns>
    public Vector2Int? CalcTouchPuzzleTileIndex(Vector2 touchPos)
    {
        // 真ん中下を(0,0)に
        touchPos.x -= Screen.width * 0.5f;

        // パズル盤面の四隅位置の計算
        Vector2 leftUp = new Vector2(GameUILayoutUtility._puzzleBoardRect.xMin, GameUILayoutUtility._puzzleBoardRect.yMax);
        Vector2 rightButtom = new Vector2(GameUILayoutUtility._puzzleBoardRect.xMax, GameUILayoutUtility._puzzleBoardRect.yMin);

        if (leftUp.x > touchPos.x || touchPos.x > rightButtom.x || rightButtom.y > touchPos.y || touchPos.y > leftUp.y)
            return null;

        // X座標が0以上か0以下かでループの添え字を変える(若干処理早くなるけど横の牌の数が奇数になったら死ぬ)
        int startIdx = 0;
        int endIdx = GameData.PUZZLE_BOARD_SIZE_X;
        if (touchPos.x <= 0)
            endIdx = GameData.PUZZLE_BOARD_SIZE_X / 2;
        else
            startIdx = GameData.PUZZLE_BOARD_SIZE_X / 2;

        // ループでどこがタッチされているを調べる
        for (int y = 0; y < GameData.PUZZLE_BOARD_SIZE_Y; y++)
        {
            Vector2 tileLeftUp = new Vector2(leftUp.x + startIdx * GameUILayoutUtility._puzzleTilesFinalSize.x, leftUp.y - y * GameUILayoutUtility._puzzleTilesFinalSize.y);
            for (int x = startIdx; x < endIdx; x++)
            {
                Vector2 tileRightButtom = new Vector2(tileLeftUp.x + GameUILayoutUtility._puzzleTilesFinalSize.x, tileLeftUp.y - GameUILayoutUtility._puzzleTilesFinalSize.y);

                // タッチされているか
                if (tileLeftUp.x <= touchPos.x && touchPos.x <= tileRightButtom.x && tileRightButtom.y <= touchPos.y && touchPos.y <= tileLeftUp.y)
                    return new Vector2Int(x, y);

                tileLeftUp.x = tileRightButtom.x;
            }
        }

        return null;
    }

    /// <summary>
    /// パズル牌の入れ替え
    /// </summary>
    /// <param name="tile1">入れ替えるパズル牌1</param>
    /// <param name="tile2">入れ替えるパズル牌2</param>
    public void SwitchingPuzzleTile(Vector2Int tile1, Vector2Int tile2)
    {
        // 座標の入れ替え
        _boardTileObjects[tile1.y, tile1.x].SetPos(GameUILayoutUtility.CalcPuzzleTilePosFromIndex(tile2.x, tile2.y), PUZZLE_TILE_MOVE_TIME);
        _boardTileObjects[tile2.y, tile2.x].SetPos(GameUILayoutUtility.CalcPuzzleTilePosFromIndex(tile1.x, tile1.y), PUZZLE_TILE_MOVE_TIME);

        // 配列の入れ替え
        (_boardTileObjects[tile1.y, tile1.x], _boardTileObjects[tile2.y, tile2.x]) = (_boardTileObjects[tile2.y, tile2.x], _boardTileObjects[tile1.y, tile1.x]);
    }

    /// <summary>
    /// パズル牌の削除
    /// </summary>
    /// <param name="index">削除牌インデックス</param>
    public void DestroyPuzzleTile(Vector2Int index)
    {
        Destroy(_boardTileObjects[index.y, index.x].gameObject);
    }

    /// <summary>
    /// パズル牌の落下
    /// </summary>
    public void FallPuzzleTile()
    {
        MahjongLogic.TILE_KIND[,] boardTiles = _puzzleManager._boardTiles;
        List<Vector2Int[]> matchIndex = _puzzleManager._matchTilesIndex;

        // 落ちるX列がどこか
        bool[] isFallX = Enumerable.Range(0, GameData.PUZZLE_BOARD_SIZE_X).Select(_ => false).ToArray();
        for (int i = 0; i < matchIndex.Count; i++) 
        {
            for (int j = 0; j < 3; j++)
                isFallX[matchIndex[i][j].x] = true;
        }

        // 列ごとに考える
        for (int x = 0; x < GameData.PUZZLE_BOARD_SIZE_X; x++)
        {
            // 落下しなければ次へ
            if (!isFallX[x])
                continue;

            // 下から更新していく
            int matchTileCount = 0; // 処理済み牌カウント(ずれ)
            for (int y = GameData.PUZZLE_BOARD_SIZE_Y - 1; y >= 0; y--)
            {
                if (_boardTileObjects[y, x] == null)
                    matchTileCount++;
                else
                {
                    // まだずれていなければスルー
                    if (matchTileCount == 0)
                        continue;

                    // ずれた分だけ下に行く
                    _boardTileObjects[y + matchTileCount, x] = _boardTileObjects[y, x];
                    // 移動先座標の設定
                    _boardTileObjects[y, x].SetPos(GameUILayoutUtility.CalcPuzzleTilePosFromIndex(x, y + matchTileCount), PUZZLE_TILE_FALL_TIME * matchTileCount);
                }
            }

            // 新しい牌の生成・座標指定・移動先座標の設定
            for (int i = 1; i <= matchTileCount; i++)
            {
                // 生成
                GameObject obj = Instantiate(_tilePrefab, _puzzleTilesParent);
                MahjongTileView tile = obj.GetComponent<MahjongTileView>();
                _boardTileObjects[(matchTileCount - i), x] = tile;
                tile.SetPos(GameUILayoutUtility.CalcPuzzleTilePosFromIndex(x, -i));
                tile.SetScale(GameUILayoutUtility._puzzleTilesViewScale);
                // 牌類のセット
                tile.SetKind(boardTiles[(matchTileCount - i), x]);
                // 移動先座標
                tile.SetPos(GameUILayoutUtility.CalcPuzzleTilePosFromIndex(x, (matchTileCount - i)), PUZZLE_TILE_FALL_TIME * matchTileCount);
            }
        }
    }

    /// <summary>
    /// 手牌の追加
    /// </summary>
    /// <param name="handTilesKindList">手牌の牌種リスト</param>
    /// <param name="tilesIndex">追加牌の盤面インデックス</param>
    public void AddHandTiles(List<MahjongLogic.TILE_KIND> handTilesKindList, Vector2Int[] tilesIndex)
    {
        // 中,左,右の順番で格納されているので自然な順番にする
        int[] index = { 1, 0, 2 };

        for (int i = 0; i < tilesIndex.Length; i++)
        {
            // 手牌の生成
            GameObject obj = Instantiate(_tilePrefab, _handTilesParent);
            MahjongTileView tile = obj.GetComponent<MahjongTileView>();
            _handTileObjects.Add(tile);
            // 元の場所に生成
            tile.SetPos(GameUILayoutUtility.CalcPuzzleTilePosFromIndex(tilesIndex[index[i]].x, tilesIndex[index[i]].y));
            tile.SetScale(GameUILayoutUtility._puzzleTilesViewScale);
            // 牌類のセット
            tile.SetKind(handTilesKindList[handTilesKindList.Count - (3 - index[i])]);

            // 手牌に移動・縮小
            tile.SetPos(GameUILayoutUtility.CalcHandTilePosFromIndex(handTilesKindList.Count - (3 - index[i])), HAND_TILE_MOVE_TIME);
            tile.SetScale(GameUILayoutUtility._handTilesScale);
        }
    }

    /// <summary>
    /// 手牌の削除(一旦)
    /// </summary>
    public void ClearHandTiles()
    {
        for (int i = 0; i < _handTileObjects.Count; i++)
        {
            Destroy(_handTileObjects[i].gameObject);
        }
        _handTileObjects.Clear();
    }

    /// <summary>
    /// ドラと雀頭と自風の牌種の設定
    /// </summary>
    /// <param name="dora">ドラの牌種</param>
    /// <param name="head">雀頭の牌種</param>
    /// <param name="jikazeCnt">自風のカウント</param>
    public void SetDoraHeadJikazeKind(MahjongLogic.TILE_KIND dora, MahjongLogic.TILE_KIND head, int jikazeCnt)
    {
        _doraTile.SetKind(dora);
        _headTile1.SetKind(head);
        _headTile2.SetKind(head);
        _jikazeTile.SetKind((MahjongLogic.TILE_KIND)((int)MahjongLogic.TILE_KIND.TON + jikazeCnt));
    }
}
