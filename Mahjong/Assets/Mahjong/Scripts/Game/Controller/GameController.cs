using UnityEngine;

public class GameController : MonoBehaviour
{
    // 入力ハンドラ
    [SerializeField] private TouchInputHandler _input;
    public TouchInputHandler TouchInputHandler => _input;

    // ステージビュー管理クラス
    [SerializeField] private StageViewManager _stageViewManager;
    public StageViewManager StageViewManager => _stageViewManager;

    // バトルビュー管理クラス
    [SerializeField] private BattleViewManager _battleViewManager;
    public BattleViewManager BattleViewManager => _battleViewManager;

    // パズルビュー管理クラス
    [SerializeField] private PuzzleViewManager _puzzleViewManager;
    public PuzzleViewManager PuzzleViewManager => _puzzleViewManager;

    // ステージデータ
    [SerializeField] private StageData _stageData;

    // ステージコントローラー
    private StageController _stageController;

    void Awake()
    {
        // 麻雀牌のスケールと隙間の計算
        GameUILayoutUtility.CalcUILayout();
    }

    void Start()
    {
        // ステージコントローラーの作成
        _stageController = new StageController();
        _stageController.Init(this, _stageData);
    }

    void Update()
    {
        _stageController.TickProcess(Time.deltaTime);
    }
}
