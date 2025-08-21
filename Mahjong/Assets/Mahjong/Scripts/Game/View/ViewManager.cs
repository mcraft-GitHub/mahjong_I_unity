using UnityEngine;

public class ViewManager : MonoBehaviour
{
    // 麻雀牌プレハブ
    [SerializeField] private GameObject _tilePrefab;

    // 獲得牌の親オブジェクトTransform
    [SerializeField] private Transform _acquiredTilesParent;
    // パズル牌の親オブジェクトTransform
    [SerializeField] private Transform _puzzleTilesParent;

    // ゲームコントローラー
    private GameController _gameController;
    // パズルマネージャー
    private PuzzleManager _puzzleManager;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    // パズルマネージャーのセット
    public void SetClass(GameController gameController, PuzzleManager puzzleManager)
    {
        _gameController = gameController;
        _puzzleManager = puzzleManager;
    }

    // パズル盤面の牌の生成
    public void CreatePuzzleBoard()
    { 
        MahjongLogic.TILE_KIND[,] boardTiles = _puzzleManager._boardTiles;

        // パズル盤面の開始位置の計算
        Vector2 puzzleTileSize = GameData.TILE_SIZE * GameData.puzzleTilesScale;
        Vector2 pos = new Vector2(
            puzzleTileSize.x * GameData.PUZZLE_BOARD_SIZE_X * -0.5f + puzzleTileSize.x * 0.5f,
            GameData.uiHeight - GameData.HEIGHT_BLANK - GameData.PUZZLE_BLANK - puzzleTileSize.y * 0.5f
        );
        // ループで生成
        for (int y = 0; y < boardTiles.GetLength(0); y++)
        {
            for (int x = 0; x < boardTiles.GetLength(1); x++)
            {
                GameObject obj = Instantiate(_tilePrefab, _puzzleTilesParent);
                MahjongTileView tile = obj.GetComponent<MahjongTileView>();
                tile.SetPos(new Vector2(pos.x + x * puzzleTileSize.x, pos.y - y * puzzleTileSize.y));
                tile.SetScale(GameData.puzzleTilesScale * (1.0f - GameData.PUZZLE_TILE_MARGIN_RATE));
                // 牌類のセット
                tile.SetKind(boardTiles[y, x]);
            }
        }
    }

    // タッチされているパズル牌インデックス
    public Vector2Int? CalcTouchPuzzleTileIndex(Vector2 touchPos)
    {
        // 真ん中下を(0,0)に
        touchPos.x -= Screen.width * 0.5f;

        // パズル盤面の四隅位置の計算
        Vector2 puzzleTileSize = GameData.TILE_SIZE * GameData.puzzleTilesScale;
        Vector2 leftUp = new Vector2(puzzleTileSize.x * GameData.PUZZLE_BOARD_SIZE_X * -0.5f,　GameData.uiHeight - GameData.HEIGHT_BLANK - GameData.PUZZLE_BLANK);
        Vector2 rightButtom = new Vector2(-leftUp.x, leftUp.y - puzzleTileSize.y * GameData.PUZZLE_BOARD_SIZE_Y);

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
            Vector2 tileLeftUp = new Vector2(leftUp.x + startIdx * puzzleTileSize.x, leftUp.y - y * puzzleTileSize.y);
            for (int x = startIdx; x < endIdx; x++)
            {
                Vector2 tileRightButtom = new Vector2(tileLeftUp.x + puzzleTileSize.x, tileLeftUp.y - puzzleTileSize.y);

                // タッチされているか
                if (tileLeftUp.x <= touchPos.x && touchPos.x <= tileRightButtom.x && tileRightButtom.y <= touchPos.y && touchPos.y <= tileLeftUp.y)
                {
                    return new Vector2Int(x, y);
                }

                tileLeftUp.x = tileRightButtom.x;
            }
        }

        return null;
    }
}
