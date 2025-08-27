using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

// マッチ → 一時停止 → 手牌に追加 → 手牌完成なら攻撃 → まだマッチがあるなら[手牌に追加]へ → 停止解除 → 落ちる → マッチ判定 → 最初へ

public class GameController : MonoBehaviour
{
    [SerializeField] private TouchInputHandler _input;
    [SerializeField] private PuzzleViewManager _puzzleViewManager;
    [SerializeField] private EnemyData _enemyData;

    // パズルマネージャー
    private PuzzleManager _puzzleManager;

    // バトルマネージャー
    private BattleManager _battleManager;

    // 前フレームのステート
    private PuzzleManager.GameState _prevState = PuzzleManager.GameState.PAUSE;

    // 手牌
    private List<MahjongLogic.TILE_KIND> _handTilesKindList = new List<MahjongLogic.TILE_KIND>();

    // 雀頭牌
    private MahjongLogic.TILE_KIND _headTilesKind = MahjongLogic.TILE_KIND.NONE;

    // ドラ牌
    private MahjongLogic.TILE_KIND _doraTilesKind = MahjongLogic.TILE_KIND.NONE;

    // ゲームステート(ゲーム中:0, 勝利:1, 敗北:2)
    int _gameState = 0;

    // ***** READY
    // 移動開始位置
    private Vector2Int? _currentMoveIndex = null;

    // ***** MATCH
    // アニメーション中か
    private bool _isAnimation = false;

    void Awake()
    {
        // 麻雀牌のスケールと隙間の計算
        GameData.CalcTileScaleAndMargin();
    }

    void Start()
    {
        // パズルマネージャーの生成
        _puzzleManager = new PuzzleManager();

        // パズルマネージャーのセット
        _puzzleViewManager.SetClass(this, _puzzleManager);

        // バトルマネージャーの生成
        _battleManager = new BattleManager();

        // ゲームの初期化
        InitGame();
    }

    void Update()
    {
        if (_gameState == 0)
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

            // プレイヤーの攻撃で既に敵が倒れている可能性があるのでもう一度チェック
            if (_gameState == 0)
            {
                // 敵の攻撃チェック
                float result = _battleManager.EnemyAttackCheck(Time.deltaTime);

                // ゲームオーバーチェック
                _gameState = _battleManager.IsGameOver();
            }
        }
    }

    /// <summary>
    /// ステートがREADYの際の処理
    /// </summary>
    private void UpdateReady()
    {
        // 切り替わったら現在の位置を初期化
        if (_prevState != PuzzleManager.GameState.READY)
            _currentMoveIndex = null;

        if (!_currentMoveIndex.HasValue)
        {
            // 移動開始疑惑
            if (_input.GetTouchState() == TouchInputHandler.TouchState.TouchStarted)
            {
                _currentMoveIndex = _puzzleViewManager.CalcTouchPuzzleTileIndex(_input.GetCurrentDragPosition());
                if (_currentMoveIndex.HasValue)
                    _puzzleManager.MoveNow(_currentMoveIndex.Value);
            }
        }
        else
        {
            // 移動終了
            if (_input.GetTouchState() == TouchInputHandler.TouchState.TouchEnded || _input.GetTouchState() == TouchInputHandler.TouchState.None)
            {
                _puzzleManager.MoveEnd(_currentMoveIndex.Value);
                _currentMoveIndex = null;
                return;
            }

            // 選択パズル牌の計算
            Vector2Int? newIndex = _puzzleViewManager.CalcTouchPuzzleTileIndex(_input.GetCurrentDragPosition());
            // 移動終了
            if (!newIndex.HasValue)
            {
                _puzzleManager.MoveEnd(_currentMoveIndex.Value);
                _currentMoveIndex = null;
                return;
            }

            // 移動
            if (newIndex.Value.x != _currentMoveIndex.Value.x || newIndex.Value.y != _currentMoveIndex.Value.y)
            {
                _puzzleViewManager.SwitchingPuzzleTile(_currentMoveIndex.Value, newIndex.Value);
                _currentMoveIndex = newIndex;
                _puzzleManager.MoveNow(_currentMoveIndex.Value);
            }
        }
    }

    /// <summary>
    /// ステートがMATCHの際の処理
    /// </summary>
    private void UpdateMatch()
    {
        // マッチ処理
        if (!_isAnimation && _puzzleManager.matchTilesIndex.Count > 0)
        {
            StartCoroutine(ScalePosCoroutine());
        }
    }

    /// <summary>
    /// ゲームの初期化
    /// </summary>
    private void InitGame()
    {
        List<MahjongLogic.TILE_KIND> useTileKinds = new List<MahjongLogic.TILE_KIND>();
        // TODO 使用する牌の種類指定処理
        // 将来的には何かしらの方法で使用牌を指定するが、現時点では仕様を決めていないので、切り替えにはコメントアウトを使用する。
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_1);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_2);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_3);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_4);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_5);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_6);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_7);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_8);
        useTileKinds.Add(MahjongLogic.TILE_KIND.MAN_9);
        useTileKinds.Add(MahjongLogic.TILE_KIND.PIN_1);
        useTileKinds.Add(MahjongLogic.TILE_KIND.PIN_2);
        useTileKinds.Add(MahjongLogic.TILE_KIND.PIN_3);
        useTileKinds.Add(MahjongLogic.TILE_KIND.PIN_4);
        useTileKinds.Add(MahjongLogic.TILE_KIND.PIN_5);
        useTileKinds.Add(MahjongLogic.TILE_KIND.PIN_6);
        useTileKinds.Add(MahjongLogic.TILE_KIND.PIN_7);
        useTileKinds.Add(MahjongLogic.TILE_KIND.PIN_8);
        useTileKinds.Add(MahjongLogic.TILE_KIND.PIN_9);
        useTileKinds.Add(MahjongLogic.TILE_KIND.SOO_1);
        useTileKinds.Add(MahjongLogic.TILE_KIND.SOO_2);
        useTileKinds.Add(MahjongLogic.TILE_KIND.SOO_3);
        useTileKinds.Add(MahjongLogic.TILE_KIND.SOO_4);
        useTileKinds.Add(MahjongLogic.TILE_KIND.SOO_5);
        useTileKinds.Add(MahjongLogic.TILE_KIND.SOO_6);
        useTileKinds.Add(MahjongLogic.TILE_KIND.SOO_7);
        useTileKinds.Add(MahjongLogic.TILE_KIND.SOO_8);
        useTileKinds.Add(MahjongLogic.TILE_KIND.SOO_9);
        //useTileKinds.Add(MahjongLogic.TILE_KIND.TON);
        //useTileKinds.Add(MahjongLogic.TILE_KIND.NAN);
        //useTileKinds.Add(MahjongLogic.TILE_KIND.SYA);
        //useTileKinds.Add(MahjongLogic.TILE_KIND.PEE);
        //useTileKinds.Add(MahjongLogic.TILE_KIND.HAKU);
        //useTileKinds.Add(MahjongLogic.TILE_KIND.HATU);
        //useTileKinds.Add(MahjongLogic.TILE_KIND.TYUN);
        // パズルの初期化
        _puzzleManager.InitPuzzle(useTileKinds);

        // 牌の配置
        _puzzleViewManager.CreatePuzzleBoard();

        // ドラの決定
        _doraTilesKind = _puzzleManager.GetRandomTileKind();
        // 雀頭の決定
        _headTilesKind = _puzzleManager.GetRandomTileKind();
        // ドラと雀頭の設定
        _puzzleViewManager.SetDoraHeadKind(_doraTilesKind, _headTilesKind);

        // バトルの初期化(プレイヤーの体力は暫定＆テキトー)
        _battleManager.InitBattle(_enemyData, 2000);
    }

    /// <summary>
    /// マッチ時の処理
    /// </summary>
    /// <returns>IEnumerator</returns>
    IEnumerator ScalePosCoroutine()
    {
        // アニメーション開始
        _isAnimation = true;

        // 最長落下時間を計算するために各X列の消える牌の数を保持
        int[] fallY = Enumerable.Range(0, GameData.PUZZLE_BOARD_SIZE_X).Select(_ => 0).ToArray();

        // マッチ牌の削除
        for (int i = 0; i < _puzzleManager.matchTilesIndex.Count; i++)
        {
            for (int j = 0; j < _puzzleManager.matchTilesIndex[i].Length; j++)
            {
                fallY[_puzzleManager.matchTilesIndex[i][j].x]++;
                _puzzleViewManager.DestroyPuzzleTile(_puzzleManager.matchTilesIndex[i][j]);

                // 手牌に追加(必ず3個ずつ追加されると信じて個数チェックはしません！)
                _handTilesKindList.Add(_puzzleManager.matchTilesKind[i][j]);
            }

            // 手牌に加える演出
            _puzzleViewManager.AddHandTiles(_handTilesKindList, _puzzleManager.matchTilesIndex[i]);

            // 止める(手牌に加える演出時間)
            yield return new WaitForSeconds(PuzzleViewManager.HAND_TILE_MOVE_TIME + 0.1f);

            // 手牌がそろったか判定(手牌が12枚以外の事なんてないからマジックナンバーでもよい！！)
            if (_handTilesKindList.Count >= 12)
            {
                // 役の判定
                _handTilesKindList.Add(_headTilesKind);
                _handTilesKindList.Add(_headTilesKind);
                MahjongLogic.Role role = MahjongLogic.CalcHandTilesRole(_handTilesKindList, _doraTilesKind, MahjongLogic.TILE_KIND.TON);

                // ダメージの計算
                int damage = _battleManager.CalcDamage(role);

                // 止める(手牌に加える演出時間 + 攻撃演出時間)
                yield return new WaitForSeconds(0.5f);

                // プレイヤーの攻撃
                float enemyHpRato = _battleManager.PlayerAttackCheck(damage);

                // ゲームオーバーチェック
                _gameState = _battleManager.IsGameOver();

                // 手牌クリア
                _puzzleViewManager.ClearHandTiles();
                _handTilesKindList.Clear();

                // ドラの決定
                _doraTilesKind = _puzzleManager.GetRandomTileKind();
                // 雀頭の決定
                _headTilesKind = _puzzleManager.GetRandomTileKind();
                // ドラと雀頭の設定
                _puzzleViewManager.SetDoraHeadKind(_doraTilesKind, _headTilesKind);
            }
        }

        // 落とす
        _puzzleViewManager.FallPuzzleTile();

        // 落下時間
        yield return new WaitForSeconds(fallY.Max() * PuzzleViewManager.PUZZLE_TILE_FALL_TIME);

        // アニメーション終了
        _isAnimation = false;

        // マッチ終了
        _puzzleManager.FinishMatch();
    }
}
