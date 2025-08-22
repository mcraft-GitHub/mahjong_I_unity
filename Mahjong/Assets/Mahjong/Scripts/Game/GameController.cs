using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

// �}�b�` �� �ꎞ��~ �� ��v�ɒǉ� �� ��v�����Ȃ�U�� �� �܂��}�b�`������Ȃ�[��v�ɒǉ�]�� �� ��~���� �� ������ �� �}�b�`���� �� �ŏ���

public class GameController : MonoBehaviour
{
    [SerializeField] private TouchInputHandler _input;
    [SerializeField] private ViewManager _viewManager;

    // �p�Y���}�l�[�W���[
    private PuzzleManager _puzzleManager;

    // �O�t���[���̃X�e�[�g
    private PuzzleManager.GameState _prevState = PuzzleManager.GameState.PAUSE;

    // ***** READY
    // �ړ��J�n�ʒu
    private Vector2Int? _currentMoveIndex = null;

    // ***** MATCH
    // �A�j���[�V��������
    private bool _isAnimation = false;

    void Start()
    {
        // �����v�̃X�P�[���ƌ��Ԃ̌v�Z
        GameData.CalcTileScaleAndMargin();

        // �p�Y���}�l�[�W���[�̐���
        _puzzleManager = new PuzzleManager();

        // �p�Y���}�l�[�W���[�̃Z�b�g
        _viewManager.SetClass(this, _puzzleManager);

        // �Q�[���̏�����
        InitGame();
    }

    void Update()
    {
        PuzzleManager.GameState prevState = _puzzleManager.state;
        switch (_puzzleManager.state)
        {
            case PuzzleManager.GameState.READY:
                UpdateReady();
                break;
            case PuzzleManager.GameState.MATCH:
                UpdateMatch();
                break;
            case PuzzleManager.GameState.PREV_MOVE:
                break;
            case PuzzleManager.GameState.PAUSE:
                break;
        }
        _prevState = prevState;
    }

    // �X�e�[�g��READY�̍ۂ̏���
    private void UpdateReady()
    {
        // �؂�ւ�����猻�݂̈ʒu��������
        if (_prevState != PuzzleManager.GameState.READY)
            _currentMoveIndex = null;

        if (!_currentMoveIndex.HasValue)
        {
            // �ړ��J�n�^�f
            if (_input.GetTouchState() == TouchInputHandler.TouchState.TouchStarted)
            {
                _currentMoveIndex = _viewManager.CalcTouchPuzzleTileIndex(_input.GetCurrentDragPosition());
                if (_currentMoveIndex.HasValue)
                    _puzzleManager.MoveNow(_currentMoveIndex.Value);
            }
        }
        else
        {
            // �ړ��I��
            if (_input.GetTouchState() == TouchInputHandler.TouchState.TouchEnded || _input.GetTouchState() == TouchInputHandler.TouchState.None)
            {
                _puzzleManager.MoveEnd(_currentMoveIndex.Value);
                _currentMoveIndex = null;
                return;
            }

            // �I���p�Y���v�̌v�Z
            Vector2Int? newIndex = _viewManager.CalcTouchPuzzleTileIndex(_input.GetCurrentDragPosition());
            // �ړ��I��
            if (!newIndex.HasValue)
            {
                _puzzleManager.MoveEnd(_currentMoveIndex.Value);
                _currentMoveIndex = null;
                return;
            }

            // �ړ�
            if (newIndex.Value.x != _currentMoveIndex.Value.x || newIndex.Value.y != _currentMoveIndex.Value.y)
            {
                _viewManager.SwitchingPuzzleTile(_currentMoveIndex.Value, newIndex.Value);
                _currentMoveIndex = newIndex;
                _puzzleManager.MoveNow(_currentMoveIndex.Value);
            }
        }
    }

    // �X�e�[�g��MATCH�̍ۂ̏���
    private void UpdateMatch()
    {
        // �}�b�`����
        if (!_isAnimation && _puzzleManager.matchingTilesIndex.Count > 0)
        {
            StartCoroutine(ScalePosCoroutine());
        }
    }

    // �Q�[���̏�����
    private void InitGame()
    {
        // �g�p����v
        List<MahjongLogic.TILE_KIND> useTileKinds = new List<MahjongLogic.TILE_KIND>();
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_1);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_2);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_3);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_4);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_5);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_6);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_7);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_8);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_9);
        useTileKinds.Add(MahjongLogic.TILE_KIND.TON);
        useTileKinds.Add(MahjongLogic.TILE_KIND.NAN);
        useTileKinds.Add(MahjongLogic.TILE_KIND.SYA);
        useTileKinds.Add(MahjongLogic.TILE_KIND.PEE);
        useTileKinds.Add(MahjongLogic.TILE_KIND.HAKU);
        useTileKinds.Add(MahjongLogic.TILE_KIND.HATU);
        useTileKinds.Add(MahjongLogic.TILE_KIND.TYUN);
        // �p�Y���̏�����
        _puzzleManager.InitPuzzle(useTileKinds);

        // �v�̔z�u
        _viewManager.CreatePuzzleBoard();
    }

    // �}�b�`���̏���
    IEnumerator ScalePosCoroutine()
    {
        // �A�j���[�V�����J�n
        _isAnimation = true;

        // �Œ��������Ԃ��v�Z���邽�߂ɊeX��̏�����v�̐���ێ�
        int[] fallY = Enumerable.Range(0, GameData.PUZZLE_BOARD_SIZE_X).Select(_ => 0).ToArray();

        // �}�b�`�v�̍폜
        for (int i = 0; i < _puzzleManager.matchingTilesIndex.Count; i++)
        {
            for (int j = 0; j < _puzzleManager.matchingTilesIndex[i].Length; j++)
            {
                fallY[_puzzleManager.matchingTilesIndex[i][j].x]++;
                _viewManager.DestroyPuzzleTile(_puzzleManager.matchingTilesIndex[i][j]);
            }
            // ������Ǝ~�߂�(�����Ŏ�v�ɉ����鉉�o&��v����������U���Ƃ���)
            yield return new WaitForSeconds(0.5f);
        }

        // ���Ƃ�
        _viewManager.FallPuzzleTile();

        // ��������
        yield return new WaitForSeconds(fallY.Max() * ViewManager.PUZZLE_TILE_FALL_TIME);

        // �A�j���[�V�����I��
        _isAnimation = false;

        // �}�b�`�I��
        _puzzleManager.FinishMatching();
    }
}
