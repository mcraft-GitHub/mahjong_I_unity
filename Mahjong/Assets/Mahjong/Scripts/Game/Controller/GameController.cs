using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // ゲームステート
    public enum GAME_STATE
    {
        OPENING = 0,        // オープニング
        ADVANCE,            // ステージ進行
        ENCOUNT,            // 接敵
        COUNTDOWN,          // バトルカウントダウン
        PUZZLE,             // パズル
        MATCHING_RESULT,    // マッチ結果の処理
        BATTLE_WIN,         // バトル勝利
        BATTLE_LOSE,        // バトル敗北
        STAGE_CLEAR,        // ステージクリア
        LOAD_SCENE,         // シーンのロード
        NONE,
    }

    // ゲームデータ
    // 各コントローラーのStateUpdate()で変更される
    public class GameData
    {
        // ゲームステート
        public GAME_STATE _currentState = GAME_STATE.NONE;

        // 現在の敵のインデックス
        public int _currentEnemtIdx = 0;

        // 手牌に追加する面子
        public MahjongLogic.GameMentu _addHandMentu = null;
    }

    // ステージデータ
    [SerializeField] private StageData _stageData;

    // ステージコントローラー
    [SerializeField] private StageController _stageController;

    // バトルコントローラー
    [SerializeField] private BattleController _battleController;

    // パズルコントローラー
    [SerializeField] private PuzzleController _puzzleController;

    // プレイヤーキャラデータ(仮でインスペクターから追加)
    [SerializeField] private CharacterData _playerCharaData;

    // ゲームデータ
    private GameData _gameData;

    // 前フレームのゲームステート
    private GAME_STATE _prevState = GAME_STATE.NONE;

    void Awake()
    {
        // 麻雀牌のスケールと隙間の計算
        GameUILayoutUtility.CalcUILayout();
    }

    void Start()
    {
        // ゲームデータの作成
        _gameData = new GameData();

        // ステージコントローラーの初期化
        _stageController.Init(_stageData);

        // バトルコントローラーの初期化
        _battleController.Init(_stageData, _playerCharaData);

        // パズルコントローラーの初期化
        _puzzleController.Init(_stageData);

        // ゲームステートを開始時の値にする
        _gameData._currentState = GAME_STATE.OPENING;
    }

    void Update()
    {
        // 前フレームステート用に保持
        GAME_STATE bufState = _gameData._currentState;

        // ステートが切り替わったらデバッグ用表示
        if (_gameData._currentState != _prevState)
        {
            Debug.Log("StateChange:" + _prevState + " > " + _gameData._currentState);
        }

        // パズルコントローラーの更新
        _puzzleController.StateUpdate(_gameData, _prevState);

        // バトルコントローラーの更新
        _battleController.StateUpdate(_gameData, _prevState);

        // ステージコントローラーの更新
        _stageController.StateUpdate(_gameData, _prevState);

        // 前フレームステートのセット
        _prevState = bufState;
    }
}
