using UnityEngine;
using UnityEngine.UI;

public class PuzzleViewManager : MonoBehaviour
{
    // パズル牌の移動時間
    public static readonly float PUZZLE_TILE_MOVE_TIME = 0.15f;

    // パズル牌の落下時間(1マス)
    public static readonly float PUZZLE_TILE_FALL_TIME = 0.2f;

    // 麻雀牌プレハブ
    [SerializeField] private GameObject _tilePrefab;

    // パズル牌の親オブジェクトTransform
    [SerializeField] private Transform _puzzleTilesParent;

    // パズル枠兼背景
    [SerializeField] private RectTransform _puzzleFrameRect;

    // パズル枠兼背景の画像
    [SerializeField] private Image _puzzleFrameImage;

    // パズルマネージャー
    private PuzzleManager _puzzleManager;

    // パズル牌オブジェクト
    private MahjongTileView[,] _boardTileObjects = new MahjongTileView[GameData.PUZZLE_BOARD_SIZE_Y, GameData.PUZZLE_BOARD_SIZE_X];

    void Start()
    {
        // パズル枠兼背景の配置・拡縮
        _puzzleFrameRect.anchoredPosition = new Vector2(0.0f, GameUILayoutUtility._puzzleBoardRect.center.y);
        _puzzleFrameRect.localScale = Vector3.one * GameUILayoutUtility._puzzleFrameScale;
    }

    /// <summary>
    /// 必要クラスのセット
    /// </summary>
    /// <param name="puzzleManager">パズルマネージャー</param>
    public void SetClass(PuzzleManager puzzleManager)
    {
        _puzzleManager = puzzleManager;
    }

    /// <summary>
    /// パズル枠の色のセット
    /// </summary>
    /// <param name="color">パズル枠の色</param>
    public void SetPuzzleFrameColor(Color color)
    {
        _puzzleFrameImage.color = color;
    }

    /// <summary>
    /// パズル盤面の牌の生成
    /// </summary>
    public void CreatePuzzleBoard()
    {
        // パズル牌の初期化
        for (int i = _puzzleTilesParent.childCount - 1; i >= 0; i--)
        {
            Destroy(_puzzleTilesParent.GetChild(i).gameObject);
        }

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
        _boardTileObjects[index.y, index.x] = null;
    }

    /// <summary>
    /// パズル牌の落下
    /// </summary>
    /// <return>落下時間</return>
    public float FallPuzzleTile()
    {
        MahjongLogic.TILE_KIND[,] boardTiles = _puzzleManager._boardTiles;

        // 最大何個落ちるか
        int maxMatchTileCount = 0;

        // 列ごとに考える
        for (int x = 0; x < GameData.PUZZLE_BOARD_SIZE_X; x++)
        {
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

            // マッチ牌がなければ終了
            if (matchTileCount == 0)
                continue;

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

            // 一番多い落ちる牌の数
            if (maxMatchTileCount < matchTileCount)
                maxMatchTileCount = matchTileCount;
        }

        // 最大落下時間
        return PUZZLE_TILE_FALL_TIME * maxMatchTileCount;
    }
}
