using UnityEngine;
using UnityEngine.InputSystem;

public class TouchInputHandler : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    private InputAction touchPositionAction;
    private InputAction touchPressAction;

    private Vector2 startTouchPosition;
    private Vector2 currentTouchPosition;
    private float touchStartTime;
    private bool isDragging = false;

    private bool touchStartedThisFrame = false;

    public enum TouchState
    {
        None,         // �^�b�`����Ă��Ȃ���ԁi�ʏ�j
        TouchStarted, // �^�b�`���n�܂����u�ԁi�����ꂽ�j
        TouchHeld,    // �^�b�`���p�����Ă����ԁi�������ςȂ��j
        TouchEnded    // �^�b�`�������ꂽ�u�ԁi�����[�X���ꂽ�j
    }

    private TouchState currentTouchState = TouchState.None;

    void Awake()
    {
        var touchMap = inputActions.FindActionMap("Touch");
        touchPositionAction = touchMap.FindAction("PrimaryTouch");
        touchPressAction = touchMap.FindAction("TouchPress");
    }

    void Update()
    {
        if (isDragging)
        {
            currentTouchPosition = touchPositionAction.ReadValue<Vector2>();
        }
    }

    void OnEnable()
    {
        touchPressAction.started += OnTouchStarted;
        touchPressAction.canceled += OnTouchEnded;
        touchPressAction.performed += OnTouchHeld;

        touchPositionAction.Enable();
        touchPressAction.Enable();
    }

    void OnDisable()
    {
        touchPressAction.started -= OnTouchStarted;
        touchPressAction.canceled -= OnTouchEnded;
        touchPressAction.performed -= OnTouchHeld;

        touchPositionAction.Disable();
        touchPressAction.Disable();
    }

    private void OnTouchStarted(InputAction.CallbackContext context)
    {
        currentTouchPosition = touchPositionAction.ReadValue<Vector2>();
        startTouchPosition = currentTouchPosition;
        touchStartTime = Time.time;
        currentTouchState = TouchState.TouchStarted;
        touchStartedThisFrame = true;
        isDragging = true;
    }

    private void OnTouchHeld(InputAction.CallbackContext context)
    {
        currentTouchPosition = touchPositionAction.ReadValue<Vector2>();

        // TouchStarted ��Ԃ͂��̃t���[���͈ێ����A�㏑�����Ȃ�
        if (!touchStartedThisFrame)
        {
            currentTouchState = TouchState.TouchHeld;
        }
    }

    private void OnTouchEnded(InputAction.CallbackContext context)
    {
        currentTouchPosition = touchPositionAction.ReadValue<Vector2>();
        currentTouchState = TouchState.TouchEnded;
        isDragging = false;
    }

    // ���݂̃^�b�`��Ԃ�Ԃ��iNone, TouchStarted, TouchHeld, TouchEnded�j
    public TouchState GetTouchState() => currentTouchState;

    // ���݂̃^�b�`���W�i�܂��̓}�E�X�ʒu�j��Ԃ�
    public Vector2 GetTouchPosition() => currentTouchPosition;

    // �h���b�O�����ǂ�����Ԃ��i�^�b�`�J�n����A�܂�������Ă��Ȃ����j
    public bool IsDragging() => isDragging;

    // �h���b�O���n�܂����ʒu�i�ŏ��Ƀ^�b�`�������W�j��Ԃ�
    public Vector2 GetDragStartPosition() => startTouchPosition;

    // ���݂̃h���b�O�ʒu�i�w��������ʒu�j��Ԃ�
    public Vector2 GetCurrentDragPosition() => currentTouchPosition;

    // �h���b�O�J�n����̌o�ߎ��ԁi�b�j��Ԃ�
    // �h���b�O���̂݃J�E���g����A�w�𗣂���0�Ƀ��Z�b�g�����
    public float GetDragDuration() => isDragging ? Time.time - touchStartTime : 0f;

    void LateUpdate()
    {
        // TouchEnded �� None �̑J��
        if (currentTouchState == TouchState.TouchEnded)
        {
            currentTouchState = TouchState.None;
        }

        // TouchStarted ��1�t���[�������ێ����A���̃t���[���� Held �ɂ���
        if (currentTouchState == TouchState.TouchStarted && touchStartedThisFrame)
        {
            currentTouchState = TouchState.TouchHeld;
            touchStartedThisFrame = false;
        }
    }
}
