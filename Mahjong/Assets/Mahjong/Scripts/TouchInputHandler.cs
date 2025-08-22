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
        None,         // タッチされていない状態（通常）
        TouchStarted, // タッチが始まった瞬間（押された）
        TouchHeld,    // タッチが継続している状態（押しっぱなし）
        TouchEnded    // タッチが離された瞬間（リリースされた）
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

        // TouchStarted 状態はこのフレームは維持し、上書きしない
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

    // 現在のタッチ状態を返す（None, TouchStarted, TouchHeld, TouchEnded）
    public TouchState GetTouchState() => currentTouchState;

    // 現在のタッチ座標（またはマウス位置）を返す
    public Vector2 GetTouchPosition() => currentTouchPosition;

    // ドラッグ中かどうかを返す（タッチ開始され、まだ離されていないか）
    public bool IsDragging() => isDragging;

    // ドラッグが始まった位置（最初にタッチした座標）を返す
    public Vector2 GetDragStartPosition() => startTouchPosition;

    // 現在のドラッグ位置（指が今ある位置）を返す
    public Vector2 GetCurrentDragPosition() => currentTouchPosition;

    // ドラッグ開始からの経過時間（秒）を返す
    // ドラッグ中のみカウントされ、指を離すと0にリセットされる
    public float GetDragDuration() => isDragging ? Time.time - touchStartTime : 0f;

    void LateUpdate()
    {
        // TouchEnded → None の遷移
        if (currentTouchState == TouchState.TouchEnded)
        {
            currentTouchState = TouchState.None;
        }

        // TouchStarted は1フレームだけ維持し、次のフレームで Held にする
        if (currentTouchState == TouchState.TouchStarted && touchStartedThisFrame)
        {
            currentTouchState = TouchState.TouchHeld;
            touchStartedThisFrame = false;
        }
    }
}
