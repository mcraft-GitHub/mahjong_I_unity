using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class ViewManager : MonoBehaviour
{
    // �p�Y���v�̈ړ�����
    public static readonly float PUZZLE_TILE_MOVE_TIME = 0.15f;

    // �p�Y���v�̗�������(1�}�X)
    public static readonly float PUZZLE_TILE_FALL_TIME = 0.2f;

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

    // �p�Y���v�̊�{�ʒu(0,0)
    private Vector2? _basePuzzleTilePos = null;

    // �p�Y���v�I�u�W�F�N�g
    private MahjongTileView[,] _boardTileObjects = new MahjongTileView[GameData.PUZZLE_BOARD_SIZE_Y, GameData.PUZZLE_BOARD_SIZE_X];

    // X�񂲂Ƃ̍폜����p�Y���v��Y�C���f�b�N�X(_destroyPuzzleTilesY[ X�� ][ 0�` ] == �폜����v��Y)
    List<int>[] _destroyPuzzleTilesY = Enumerable.Range(0, GameData.PUZZLE_BOARD_SIZE_X).Select(_ => new List<int>()).ToArray();

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
        MahjongLogic.TILE_KIND[,] boardTiles = _puzzleManager.boardTiles;

        // ���[�v�Ő���
        for (int y = 0; y < boardTiles.GetLength(0); y++)
        {
            for (int x = 0; x < boardTiles.GetLength(1); x++)
            {
                GameObject obj = Instantiate(_tilePrefab, _puzzleTilesParent);
                MahjongTileView tile = obj.GetComponent<MahjongTileView>();
                _boardTileObjects[y, x] = tile;
                tile.SetPos(CalcPositionFromIndex(new Vector2Int(x, y)));
                tile.SetScale(GameData.puzzleTilesScale * (1.0f - GameData.PUZZLE_TILE_MARGIN_RATE));
                // �v�ނ̃Z�b�g
                tile.SetKind(boardTiles[y, x]);
            }
        }
    }

    // �^�b�`����Ă���p�Y���v�C���f�b�N�X(���̏����͐�����������Ȃ��C������)
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
                    return new Vector2Int(x, y);

                tileLeftUp.x = tileRightButtom.x;
            }
        }

        return null;
    }

    // �p�Y���v�̓���ւ�
    public void SwitchingPuzzleTile(Vector2Int tile1, Vector2Int tile2)
    {
        // ���W�̓���ւ�
        _boardTileObjects[tile1.y, tile1.x].SetPos(CalcPositionFromIndex(tile2), PUZZLE_TILE_MOVE_TIME);
        _boardTileObjects[tile2.y, tile2.x].SetPos(CalcPositionFromIndex(tile1), PUZZLE_TILE_MOVE_TIME);

        // �z��̓���ւ�
        (_boardTileObjects[tile1.y, tile1.x], _boardTileObjects[tile2.y, tile2.x]) = (_boardTileObjects[tile2.y, tile2.x], _boardTileObjects[tile1.y, tile1.x]);
    }

    // �p�Y���v�̍폜(�}�b�`�v)
    public void DestroyPuzzleTile(Vector2Int matchIndex)
    {
        _boardTileObjects[matchIndex.y, matchIndex.x].SetKind(MahjongLogic.TILE_KIND.NONE);
    }

    // �p�Y���v�̗���
    public void FallPuzzleTile()
    {
        MahjongLogic.TILE_KIND[,] boardTiles = _puzzleManager.boardTiles;
        List<Vector2Int[]> matchIndex = _puzzleManager.matchingTilesIndex;

        // ������X�񂪂ǂ���
        bool[] isFallX = Enumerable.Range(0, GameData.PUZZLE_BOARD_SIZE_X).Select(_ => false).ToArray();
        for (int i = 0; i < matchIndex.Count; i++) 
        {
            for (int j = 0; j < 3; j++)
                isFallX[matchIndex[i][j].x] = true;
        }

        // �񂲂Ƃɍl����
        for (int x = 0; x < GameData.PUZZLE_BOARD_SIZE_X; x++)
        {
            // �������Ȃ���Ύ���
            if (!isFallX[x])
                continue;

            // ������X�V���Ă���
            int matchTileCount = 0; // �����ςݔv�J�E���g(����)
            for (int y = GameData.PUZZLE_BOARD_SIZE_Y - 1; y >= 0; y--)
            {
                if (_boardTileObjects[y, x] == null || _boardTileObjects[y, x].GetKind() == MahjongLogic.TILE_KIND.NONE)
                    matchTileCount++;
                else
                {
                    // �܂�����Ă��Ȃ���΃X���[
                    if (matchTileCount == 0)
                        continue;

                    // ���ꂽ���������ɍs��
                    _boardTileObjects[y + matchTileCount, x] = _boardTileObjects[y, x];
                    // �ړ�����W�̐ݒ�
                    _boardTileObjects[y, x].SetPos(CalcPositionFromIndex(new Vector2Int(x, y + matchTileCount)), PUZZLE_TILE_FALL_TIME * matchTileCount);
                }
            }

            // �V�����v�̐����E���W�w��E�ړ�����W�̐ݒ�
            for (int i = 1; i <= matchTileCount; i++)
            {
                // ����
                GameObject obj = Instantiate(_tilePrefab, _puzzleTilesParent);
                MahjongTileView tile = obj.GetComponent<MahjongTileView>();
                _boardTileObjects[(matchTileCount - i), x] = tile;
                tile.SetPos(CalcPositionFromIndex(new Vector2Int(x, -i)));
                tile.SetScale(GameData.puzzleTilesScale * (1.0f - GameData.PUZZLE_TILE_MARGIN_RATE));
                // �v�ނ̃Z�b�g
                tile.SetKind(boardTiles[(matchTileCount - i), x]);
                // �ړ�����W
                tile.SetPos(CalcPositionFromIndex(new Vector2Int(x, (matchTileCount - i))), PUZZLE_TILE_FALL_TIME * matchTileCount);
            }
        }
    }

    // �p�Y���v�̍폜
    public void ClearPazzleBoard()
    {
        for (int y = 0; y < _boardTileObjects.GetLength(0); y++)
        {
            for (int x = 0; x < _boardTileObjects.GetLength(1); x++)
            {
                _boardTileObjects[y, x].SetKind(MahjongLogic.TILE_KIND.NONE);
            }
        }
    }

    // �p�Y���v�̓Y����������W���v�Z����
    private Vector2 CalcPositionFromIndex(Vector2Int index)
    {
        // �p�Y���v�̑傫��
        Vector2 puzzleTileSize = GameData.TILE_SIZE * GameData.puzzleTilesScale;

        // ��{�ʒu(0,0)
        if (!_basePuzzleTilePos.HasValue)
            _basePuzzleTilePos = new Vector2(
                puzzleTileSize.x * GameData.PUZZLE_BOARD_SIZE_X * -0.5f + puzzleTileSize.x * 0.5f,
                GameData.uiHeight - GameData.HEIGHT_BLANK - GameData.PUZZLE_BLANK - puzzleTileSize.y * 0.5f
            );

        return new Vector2(_basePuzzleTilePos.Value.x + index.x * puzzleTileSize.x, _basePuzzleTilePos.Value.y - index.y * puzzleTileSize.y);
    }
}
