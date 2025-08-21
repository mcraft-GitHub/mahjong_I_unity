using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

public class GameController : MonoBehaviour
{
    [SerializeField] private TouchInputHandler _input;
    [SerializeField] private ViewManager _viewManager;

    // �p�Y���}�l�[�W���[
    private PuzzleManager _puzzleManager;

    // ***** READY
    // �ړ��J�n�ʒu
    private Vector2Int? _currentMoveIndex = null;

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
        switch (_puzzleManager.state)
        {
            case PuzzleManager.GameState.READY:
                UpdateReady();
                break;
            case PuzzleManager.GameState.MATCH:
                break;
            case PuzzleManager.GameState.PAUSE:
                break;
        }
    }

    // �X�e�[�g��READY�̍ۂ̏���
    private void UpdateReady()
    {
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

            Vector2Int? newIndex = _viewManager.CalcTouchPuzzleTileIndex(_input.GetCurrentDragPosition());
            if (!newIndex.HasValue)
            {
                _puzzleManager.MoveEnd(_currentMoveIndex.Value);
                _currentMoveIndex = null;
                return;
            }

            // �ړ�
            if (newIndex.Value.x != _currentMoveIndex.Value.x || newIndex.Value.y != _currentMoveIndex.Value.y)
            {
                _currentMoveIndex = newIndex;
                _puzzleManager.MoveNow(_currentMoveIndex.Value);
            }
        }
    }

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
}
