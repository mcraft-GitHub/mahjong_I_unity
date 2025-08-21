using UnityEngine;

public class GameData
{
    // ***** Public�ϐ�
    // �p�Y���̏c�̔v�̐�
    static public readonly int PUZZLE_BOARD_SIZE_X = 8;
    // �p�Y���̉��̔v�̐�
    static public readonly int PUZZLE_BOARD_SIZE_Y = 6;

    // ��ʂ̏c�T�C�Y�ɂ�����A�p�Y�� + �擾�v�\���̍ő劄��
    static public readonly float MAX_HEIGHT_UI_RATE = 0.65f;

    // ��ʉ��̋󔒂̃T�C�Y
    static public readonly float BUTTOM_SAFE_BLANK = 36.0f;

    // �c�̋󔒂̃T�C�Y
    static public readonly float HEIGHT_BLANK = 24.0f;

    // �p�Y���Ֆʂ̘g�̕��̃T�C�Y
    static public readonly float PUZZLE_BLANK = 32.0f;

    // HP�Q�[�W�̏c�̃T�C�Y
    static public readonly float HP_GAUGE_BLANK = 36.0f;

    // �^�C���̉��̍Œ���̋�
    static public readonly float MINIMUM_BLANK = 24.0f;

    // �p�Y���^�C���̎���̋󔒊���
    static public readonly float PUZZLE_TILE_MARGIN_RATE = 0.05f;

    // �^�C���̊�{�T�C�Y
    static public readonly Vector2 TILE_SIZE = new Vector2(47.0f, 63.0f);

    // �l���E�p�Y���^�C���̃X�P�[��(��{�T�C�Y x �X�P�[�� = �ŏI�T�C�Y)
    static public float acquiredTilesScale = 2.0f;
    static public float puzzleTilesScale = 3.0f;

    // ���ׂ�ꂽ�l���E�p�Y���^�C���̉��̗]��(�Œ���̗]�� + �]�� = �ŏI�]��)
    static public float acquiredTilesMargin = 0.0f;
    static public float puzzleTilesMargin = 0.0f;

    // UI�����̍���
    static public float uiHeight = 0.0f;

    // ***** Private�ϐ�
    // UI�����̍��v�Œ�T�C�Y(UI�����̍��� - �����v)(�p�Y���g + �p�Y���g + �c�� + �c�� + �c�� + ��ʉ���)
    static private readonly float TOTAL_FIXED_UI_HEIGHT = PUZZLE_BLANK * 2.0f + HEIGHT_BLANK * 3.0f + BUTTOM_SAFE_BLANK;

    // ***** Public�֐�
    // ��ʃT�C�Y����A�^�C���̃X�P�[���Ɨ]�����v�Z����
    static public void CalcTileScaleAndMargin()
    {
        int width = Screen.width;
        int height = Screen.height;

        acquiredTilesMargin = 0.0f;
        puzzleTilesMargin = 0.0f;

        // ��ʂ̉��T�C�Y����v�̑傫�������߂�
        acquiredTilesScale = (width - (MINIMUM_BLANK + acquiredTilesMargin) * 2.0f) / (12.0f * TILE_SIZE.x);
        puzzleTilesScale = (width - (MINIMUM_BLANK + puzzleTilesMargin + PUZZLE_BLANK) * 2.0f) / (PUZZLE_BOARD_SIZE_X * TILE_SIZE.x);

        // ���߂��v�̑傫������UI�����̍��������߂�
        uiHeight = (acquiredTilesScale * TILE_SIZE.y * 2.0f) + (puzzleTilesScale * TILE_SIZE.y * PUZZLE_BOARD_SIZE_Y) + TOTAL_FIXED_UI_HEIGHT;

        // UI�����̏c�̊������ő�l�𒴂��Ă��Ȃ���Ίm��
        if (uiHeight <= height * MAX_HEIGHT_UI_RATE)
            return;

        // 1�̔v�ɂ��A�ǂ̂��炢���̂����v�Z����
        float cutHeight = (uiHeight - height * MAX_HEIGHT_UI_RATE) / (PUZZLE_BOARD_SIZE_Y + 2.0f) / TILE_SIZE.y;

        // �v�̑傫���̌���
        acquiredTilesScale -= cutHeight;
        puzzleTilesScale -= cutHeight;

        // ���̗]���̌���
        acquiredTilesMargin = (width - TILE_SIZE.x * acquiredTilesScale * 12.0f) * 0.5f - MINIMUM_BLANK;
        puzzleTilesMargin = (width - TILE_SIZE.x * puzzleTilesScale * 8.0f) * 0.5f - MINIMUM_BLANK - PUZZLE_BLANK;
        
        // �S�̂̍��� * MAX_HEIGHT_UI_RATO �̂͂������ǈꉞ�v�Z
        uiHeight = (acquiredTilesScale * TILE_SIZE.y * 2.0f) + (puzzleTilesScale * TILE_SIZE.y * PUZZLE_BOARD_SIZE_Y) + TOTAL_FIXED_UI_HEIGHT;
    }
}
