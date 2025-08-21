using UnityEngine;

public class ViewManager : MonoBehaviour
{
    // �����v�v���n�u
    [SerializeField] private GameObject _tilePrefab;

    // �l���v�̐e�I�u�W�F�N�gTransform
    [SerializeField] private Transform _acquiredTilesParent;
    // �p�Y���v�̐e�I�u�W�F�N�gTransform
    [SerializeField] private Transform _puzzleTilesParent;

    // �Q�[���R���g���[���[
    private GameController _gameController;
    // �p�Y���}�l�[�W���[
    private PuzzleManager _puzzleManager;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    // �p�Y���}�l�[�W���[�̃Z�b�g
    public void SetClass(GameController gameController, PuzzleManager puzzleManager)
    {
        _gameController = gameController;
        _puzzleManager = puzzleManager;
    }

    // �p�Y���Ֆʂ̔v�̐���
    public void CreatePuzzleBoard()
    { 
        MahjongLogic.TILE_KIND[,] boardTiles = _puzzleManager._boardTiles;

        // �p�Y���Ֆʂ̊J�n�ʒu�̌v�Z
        Vector2 puzzleTileSize = GameData.TILE_SIZE * GameData.puzzleTilesScale;
        Vector2 pos = new Vector2(
            puzzleTileSize.x * GameData.PUZZLE_BOARD_SIZE_X * -0.5f + puzzleTileSize.x * 0.5f,
            GameData.uiHeight - GameData.HEIGHT_BLANK - GameData.PUZZLE_BLANK - puzzleTileSize.y * 0.5f
        );
        // ���[�v�Ő���
        for (int y = 0; y < boardTiles.GetLength(0); y++)
        {
            for (int x = 0; x < boardTiles.GetLength(1); x++)
            {
                GameObject obj = Instantiate(_tilePrefab, _puzzleTilesParent);
                MahjongTileView tile = obj.GetComponent<MahjongTileView>();
                tile.SetPos(new Vector2(pos.x + x * puzzleTileSize.x, pos.y - y * puzzleTileSize.y));
                tile.SetScale(GameData.puzzleTilesScale * (1.0f - GameData.PUZZLE_TILE_MARGIN_RATE));
                // �v�ނ̃Z�b�g
                tile.SetKind(boardTiles[y, x]);
            }
        }
    }

    // �^�b�`����Ă���p�Y���v�C���f�b�N�X
    public Vector2Int? CalcTouchPuzzleTileIndex(Vector2 touchPos)
    {
        // �^�񒆉���(0,0)��
        touchPos.x -= Screen.width * 0.5f;

        // �p�Y���Ֆʂ̎l���ʒu�̌v�Z
        Vector2 puzzleTileSize = GameData.TILE_SIZE * GameData.puzzleTilesScale;
        Vector2 leftUp = new Vector2(puzzleTileSize.x * GameData.PUZZLE_BOARD_SIZE_X * -0.5f,�@GameData.uiHeight - GameData.HEIGHT_BLANK - GameData.PUZZLE_BLANK);
        Vector2 rightButtom = new Vector2(-leftUp.x, leftUp.y - puzzleTileSize.y * GameData.PUZZLE_BOARD_SIZE_Y);

        if (leftUp.x > touchPos.x || touchPos.x > rightButtom.x || rightButtom.y > touchPos.y || touchPos.y > leftUp.y)
            return null;

        // X���W��0�ȏォ0�ȉ����Ń��[�v�̓Y������ς���(�኱���������Ȃ邯�ǉ��̔v�̐�����ɂȂ����玀��)
        int startIdx = 0;
        int endIdx = GameData.PUZZLE_BOARD_SIZE_X;
        if (touchPos.x <= 0)
            endIdx = GameData.PUZZLE_BOARD_SIZE_X / 2;
        else
            startIdx = GameData.PUZZLE_BOARD_SIZE_X / 2;

        // ���[�v�łǂ����^�b�`����Ă���𒲂ׂ�
        for (int y = 0; y < GameData.PUZZLE_BOARD_SIZE_Y; y++)
        {
            Vector2 tileLeftUp = new Vector2(leftUp.x + startIdx * puzzleTileSize.x, leftUp.y - y * puzzleTileSize.y);
            for (int x = startIdx; x < endIdx; x++)
            {
                Vector2 tileRightButtom = new Vector2(tileLeftUp.x + puzzleTileSize.x, tileLeftUp.y - puzzleTileSize.y);

                // �^�b�`����Ă��邩
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
