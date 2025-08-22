using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static UnityEngine.Rendering.VolumeComponent;

public class PuzzleManager
{
    public enum GameState
    {
        READY = 0, // ��������(�v���C���[�����)
        MATCH, // �v�ړ���(�v���C���[����s��)
        PREV_MOVE, // �ړ��v�߂���(�v���C���[����s��)�����ꂢ��Ȃ������B�g�p����
        PAUSE, // �ꎞ��~��(�v���C���[����s��)
        MAX,
    }

    // ***** Public�ϐ�
    // �Q�[���X�e�[�g
    public GameState state { get; private set; } = GameState.READY;

    // �{�[�h�^�C���z��
    public MahjongLogic.TILE_KIND[,] boardTiles { get; } = new MahjongLogic.TILE_KIND[GameData.PUZZLE_BOARD_SIZE_Y, GameData.PUZZLE_BOARD_SIZE_X];

    // *** READY

    // *** MATCH
    // �}�b�`�v�C���f�b�N�X
    public List<Vector2Int[]> matchingTilesIndex { get; } = new List<Vector2Int[]>();

    // ***** Private�ϐ�
    // �g�p�v�탊�X�g
    private List<MahjongLogic.TILE_KIND> _useTiles;

    // *** READY
    // �ړ��J�n�ʒu
    private Vector2Int? _beginMoveIndex = null;
    // ���݈ړ��ʒu
    private Vector2Int _nowMoveIndex;
    // �ړ��ʒu����
    private List<Vector2> _moveIndexHistory = new List<Vector2>();

    // *** MATCH


    // ***** Public�֐�
    // �p�Y���̏�����
    public void InitPuzzle(List<MahjongLogic.TILE_KIND> useTiles)
    {
        _useTiles = useTiles;
        InitBoardTiles();

        _beginMoveIndex = null;
        _moveIndexHistory.Clear();
        matchingTilesIndex.Clear();
    }

    // �w�ړ���
    public void MoveNow(Vector2Int index)
    {
        if (_beginMoveIndex.HasValue)
        {
            // �ړ����Ă���
            if (_nowMoveIndex.x != index.x || _nowMoveIndex.y != index.y)
            {
                _moveIndexHistory.Add(index);
                // �}�b�`����
                SwitchingTile(_nowMoveIndex, index);
            }
            _nowMoveIndex = index;
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
        // �ړ����Ă��Ȃ�
        if (_moveIndexHistory.Count <= 1)
        {
            _beginMoveIndex = null;
            return;
        }

        // �ړ����Ă���
        //Debug.Log("�X�e�[�g�ύX�F" + state + " > " + GameState.PREV_MOVE);
        //state = GameState.PREV_MOVE;

        _beginMoveIndex = null;
    }

    // �}�b�`���O�����̏I��
    public void FinishMatching()
    {
        // ���낢��N���A
        _beginMoveIndex = null;
        _moveIndexHistory.Clear();
        matchingTilesIndex.Clear();

        // �����R���̔���(�R���{����Ȃ�����)
        // �ق�Ƃ͗�������̎���̔v�����ł������ǁA�S���m�F
        for (int y = 0; y < GameData.PUZZLE_BOARD_SIZE_Y; y++)
        {
            for (int x = 0; x < GameData.PUZZLE_BOARD_SIZE_X; x++)
            {
                MatchingCheck(new Vector2Int(x, y));
            }
        }

        if (matchingTilesIndex.Count > 0)
        {
            // �}�b�`���Ă���
            MatchingProcess();
        }
        else
        {
            // �}�b�`���Ă��Ȃ�
            Debug.Log("�X�e�[�g�ύX�F" + state + " > " + GameState.READY);
            state = GameState.READY;
        }
    }

    // ***** Private�֐�
    // �{�[�h�^�C���̏�����
    private void InitBoardTiles()
    {
        int kindNum = _useTiles.Count;

        // �����_���ɐ���
        for (int y = 0; y < GameData.PUZZLE_BOARD_SIZE_Y; y++)
        {
            for (int x = 0; x < GameData.PUZZLE_BOARD_SIZE_X; x++)
            {
                boardTiles[y, x] = _useTiles[UnityEngine.Random.Range(0, kindNum)];
            }
        }

        // ����Ń}�b�`���Ă���ꏊ���C��
        for (int y = 0; y < GameData.PUZZLE_BOARD_SIZE_Y; y++)
        {
            for (int x = 0; x < GameData.PUZZLE_BOARD_SIZE_X; x++)
            {
                if (MatchingCheck(new Vector2Int(x, y), true))
                    SetBoardUnmatchRandomKind(new Vector2Int(x, y));
            }
        }

        // �ēx�m�F
        for (int y = 0; y < GameData.PUZZLE_BOARD_SIZE_Y; y++)
        {
            for (int x = 0; x < GameData.PUZZLE_BOARD_SIZE_X; x++)
            {
                if (MatchingCheck(new Vector2Int(x, y), true))
                    Debug.Log("�o�O��I����}�b�`�I�F" + new Vector2Int(x, y));
            }
        }
    }

    // ����ւ�����(����ւ����}�b�`����)
    private void SwitchingTile(Vector2Int tile1, Vector2Int tile2)
    {
        // ����ւ�����
        (boardTiles[tile1.y, tile1.x], boardTiles[tile2.y, tile2.x]) = (boardTiles[tile2.y, tile2.x], boardTiles[tile1.y, tile1.x]);

        // �}�b�`���Ă��邩
        bool isMatch = false;

        // ����ւ����v�̃}�b�`����
        isMatch = MatchingCheck(tile1) || isMatch;
        isMatch = MatchingCheck(tile2) || isMatch;

        // ����ւ����v�̎���̔v�̃}�b�`����
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

    // �}�b�`���Ă��邩�̔���(prev:�O�i�K�̃}�b�`�`�F�b�N)
    private bool MatchingCheck(Vector2Int index, bool prev = false)
    {
        // �㉺���E�̔v��
        MahjongLogic.TILE_KIND[] adjacentTile = {
            MahjongLogic.TILE_KIND.NONE, MahjongLogic.TILE_KIND.NONE, MahjongLogic.TILE_KIND.NONE, MahjongLogic.TILE_KIND.NONE };

        // ���E
        if (index.x > 0)
            adjacentTile[2] = boardTiles[index.y, index.x - 1];
        if (index.x < GameData.PUZZLE_BOARD_SIZE_X - 1)
            adjacentTile[3] = boardTiles[index.y, index.x + 1];
        if (MahjongLogic.CheckMentu(boardTiles[index.y, index.x], adjacentTile[2], adjacentTile[3]))
        {
            if (!prev)
            {
                // �}�b�`�����v���Ȃ���
                boardTiles[index.y, index.x] = MahjongLogic.TILE_KIND.NONE;
                boardTiles[index.y, index.x - 1] = MahjongLogic.TILE_KIND.NONE;
                boardTiles[index.y, index.x + 1] = MahjongLogic.TILE_KIND.NONE;
                // �ǉ�
                matchingTilesIndex.Add(new Vector2Int[3] { index, new Vector2Int(index.x - 1, index.y), new Vector2Int(index.x + 1, index.y) });
            }
                
            return true;
        }

        // �㉺
        if (index.y > 0)
            adjacentTile[0] = boardTiles[index.y - 1, index.x];
        if (index.y < GameData.PUZZLE_BOARD_SIZE_Y - 1)
            adjacentTile[1] = boardTiles[index.y + 1, index.x];
        if (MahjongLogic.CheckMentu(boardTiles[index.y, index.x], adjacentTile[0], adjacentTile[1]))
        {
            if (!prev)
            {
                // �}�b�`�����v���Ȃ���
                boardTiles[index.y, index.x] = MahjongLogic.TILE_KIND.NONE;
                boardTiles[index.y - 1, index.x] = MahjongLogic.TILE_KIND.NONE;
                boardTiles[index.y + 1, index.x] = MahjongLogic.TILE_KIND.NONE;
                // �ǉ�
                matchingTilesIndex.Add(new Vector2Int[3] { new Vector2Int(index.x, index.y - 1), index, new Vector2Int(index.x, index.y + 1) });
            }

            return true;
        }

        return false;
    }

    // �}�b�`���Ă����ꍇ�̏���
    private void MatchingProcess()
    {
        Debug.Log("�X�e�[�g�ύX�F" + state + " > " + GameState.MATCH);
        state = GameState.MATCH;

        // �v�𗎂Ƃ�
        int kindNum = _useTiles.Count;
        for (int i = 0; i < matchingTilesIndex.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Vector2Int idx = matchingTilesIndex[i][j];

                // ���ɂɂ��炷
                for (int y = idx.y; y > 0; y--)
                    boardTiles[y, idx.x] = boardTiles[y - 1, idx.x];

                // ��ԏ�ɂ̓����_���̔v�������
                boardTiles[0, idx.x] = _useTiles[UnityEngine.Random.Range(0, kindNum)];
            }
        }
    }

    // ����ƃ}�b�`���Ȃ������_���Ȕv��̐ݒ�
    private void SetBoardUnmatchRandomKind(Vector2Int index)
    {
        bool isMatch = true;
        int kindNum = _useTiles.Count;
        // �����_���Ő����������āu�}�b�`���ĂȂ������烉�b�L�[�v���Ă����R�[�h�����炠��܂�悭�Ȃ��R�R
        while (isMatch)
        {
            Debug.Log("�������[�v�^�f");

            isMatch = false;

            // ������x�����_���擾
            boardTiles[index.y, index.x] = _useTiles[UnityEngine.Random.Range(0, kindNum)];

            if (MatchingCheck(new Vector2Int(index.x, index.y), true)) { isMatch = true; continue; }
            // �㉺���E�̊m�F
            if (index.y > 0 && MatchingCheck(new Vector2Int(index.x, index.y - 1), true)) { isMatch = true; continue; }
            if (index.y < GameData.PUZZLE_BOARD_SIZE_Y - 1 && MatchingCheck(new Vector2Int(index.x, index.y + 1), true)) { isMatch = true; continue; }
            if (index.x > 0 && MatchingCheck(new Vector2Int(index.x - 1, index.y), true)) { isMatch = true; continue; }
            if (index.x < GameData.PUZZLE_BOARD_SIZE_X - 1 && MatchingCheck(new Vector2Int(index.x + 1, index.y), true)) { isMatch = true; continue; }
        }
    }
}
