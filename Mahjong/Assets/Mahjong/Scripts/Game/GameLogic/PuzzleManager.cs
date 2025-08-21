using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PuzzleManager
{
    public enum GameState
    {
        READY = 0, // ��������(�v���C���[�����)
        MATCH, // �v�ړ���(�v���C���[����s��)
        PAUSE, // �ꎞ��~��(�v���C���[����s��)
        MAX,
    }
    private enum MatchState
    {
        NONE = 0, // �ړ����ł͂Ȃ�
        SWITCH_TRY, // �v����ւ���
        SWITCH_PREV, // �v�߂���
        MATCH, // �}�b�`(�v����)
        FALL, //������
        MAX,
    }

    // ***** Public�ϐ�
    // �Q�[���X�e�[�g
    public GameState state { get; } = GameState.READY;

    // �{�[�h�^�C���z��
    public MahjongLogic.TILE_KIND[,] _boardTiles { get; } = new MahjongLogic.TILE_KIND[GameData.PUZZLE_BOARD_SIZE_Y, GameData.PUZZLE_BOARD_SIZE_X];


    // ***** Private�ϐ�
    // ���[�u�X�e�[�g(�Q�[���X�e�[�g�̒���MATCH�̃X�e�[�g)
    private MatchState _matchState = MatchState.NONE;

    // �g�p�v�탊�X�g
    private List<MahjongLogic.TILE_KIND> _useTiles;

    // *** READY
    // �ړ��J�n�ʒu
    private Vector2? _beginMoveIndex = null;
    // ���݈ړ��ʒu
    private Vector2 _nowMoveIndex;
    // �ړ��ʒu����
    private List<Vector2> _moveIndexHistory = new List<Vector2>();

    // ***** Public�֐�
    // �p�Y���̏�����
    public void InitPuzzle(List<MahjongLogic.TILE_KIND> useTiles)
    {
        _useTiles = useTiles;
        InitBoardTiles();
    }
    
    // �w�ړ���
    public void MoveNow(Vector2Int index)
    {
        Debug.Log("�ړ�:" + index);
        if (_beginMoveIndex.HasValue)
        {
            // �ړ����Ă���
            if (_nowMoveIndex.x != index.x || _nowMoveIndex.y != index.y)
            {
                _moveIndexHistory.Add(index);
                // �}�b�`����
            }
        }
        else
        {
            // �J�n
            _beginMoveIndex = index;
            _nowMoveIndex = index;
            _moveIndexHistory.Clear();
            _moveIndexHistory.Add(index);
        }
    }

    // �w�ړ��I��
    public void MoveEnd(Vector2Int index)
    {
        Debug.Log("�I��:" + index);
    }

    // ***** Private�֐�
    // �{�[�h�^�C���̏�����
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

    // �}�b�`���Ă��邩�̔���
    private bool MatchingCheck(Vector2Int index)
    {
        MahjongLogic.TILE_KIND[] adjacentTile = new MahjongLogic.TILE_KIND[4]; // �㉺���E

        // ��
        if (index.y > 0)
            adjacentTile[0] = _boardTiles[index.x, index.y - 1];
        // ��
        if (index.y < GameData.PUZZLE_BOARD_SIZE_Y - 1)
            adjacentTile[1] = _boardTiles[index.x, index.y + 1];
        // ��
        if (index.x > 0)
            adjacentTile[0] = _boardTiles[index.x - 1, index.y];
        // �E
        if (index.x < GameData.PUZZLE_BOARD_SIZE_X - 1)
            adjacentTile[1] = _boardTiles[index.x + 1, index.y];



        return false;
    }
}
