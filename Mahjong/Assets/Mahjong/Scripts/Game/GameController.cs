using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;
using DG.Tweening;

// マッチ → 一時停止 → 手牌に追加 → 手牌完成なら攻撃 → まだマッチがあるなら[手牌に追加]へ → 停止解除 → 落ちる → マッチ判定 → 最初へ

public class GameController : MonoBehaviour
{
    // 勝敗
    static public bool _isWin = false;

    [SerializeField] private TouchInputHandler _input;
    [SerializeField] private PuzzleViewManager _puzzleViewManager;
    [SerializeField] private BattleViewManager _battleViewManager;
    [SerializeField] private EnemyData _enemyData;

    // フェード
    [SerializeField] private Image _fadeImage;

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
    // 自風カウント(0～3)
    private int _jikazeCnt = 0;

    // ゲームステート(ゲーム開始前:-1, ゲーム中:0, 勝利:1, 敗北:2)
    int _gameState = -1;

    // ゲーム開始カウントダウン
    float _beginCnt = 5.0f;

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

        // フェードイン
        _fadeImage.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        _fadeImage.DOColor(new Color(0.0f, 0.0f, 0.0f, 0.0f), 1.0f);
    }

    void Update()
    {
        // switch文 in switch文...
        // ゲームステート(開始前orゲーム中or終了)
        switch (_gameState)
        {
            case -1:
                // ゲーム開始前
                _beginCnt -= Time.deltaTime;
                // 開始
                if (_beginCnt <= 0)
                {
                    _gameState = 0;
                    _battleViewManager.SetBeginGameCount(-1);
                }
                else
                {
                    _battleViewManager.SetBeginGameCount((int)_beginCnt);
                }
                break;
            case 0:
                // パズルステート(牌移動中orマッチ処理中or未実装)
                PuzzleManager.GameState prevState = _puzzleManager._state;
                switch (_puzzleManager._state)
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
                break;
            case 1:
            case 2:
                // ゲーム終了後

                // ほんとはこのシーン内で勝敗リザルト出したいけど時間がないので一旦そのまま遷移
                break;
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

        // プレイヤーの攻撃で既に敵が倒れている可能性があるのでもう一度チェック
        if (_gameState == 0)
        {
            // 敵の攻撃チェック
            float result = _battleManager.EnemyAttackCheck(Time.deltaTime);

            // 攻撃ゲージの更新
            _battleViewManager.SetEnemyAttack(_battleManager.GetEnemyAttackDelayRate());

            // プレイヤーの体力の更新
            if (result != -1.0f)
                _battleViewManager.SetPlayerHp(result);

            // ゲームオーバーチェック
            _gameState = _battleManager.IsGameOver();
            _gameState = _battleManager.IsGameOver();
        }
    }

    /// <summary>
    /// ステートがMATCHの際の処理
    /// </summary>
    private void UpdateMatch()
    {
        // マッチ処理
        if (!_isAnimation && _puzzleManager._matchTilesIndex.Count > 0)
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
        // 自風の初期化(東スタートだけどランダムでもいいかも)
        _jikazeCnt = 0;
        // ドラと雀頭の設定
        _puzzleViewManager.SetDoraHeadJikazeKind(_doraTilesKind, _headTilesKind, _jikazeCnt);

        // バトルの初期化(プレイヤーの体力は暫定＆テキトー)
        _battleManager.InitBattle(_enemyData, 2000);

        // 敵の画像のセット
        _battleViewManager.SetEnemyImage(_enemyData._enemyImage);
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
        for (int i = 0; i < _puzzleManager._matchTilesIndex.Count; i++)
        {
            for (int j = 0; j < _puzzleManager._matchTilesIndex[i].Length; j++)
            {
                fallY[_puzzleManager._matchTilesIndex[i][j].x]++;
                _puzzleViewManager.DestroyPuzzleTile(_puzzleManager._matchTilesIndex[i][j]);

                // 手牌に追加(必ず3個ずつ追加されると信じて個数チェックはしません！)
                _handTilesKindList.Add(_puzzleManager._matchTilesKind[i][j]);
            }

            // 手牌に加える演出
            _puzzleViewManager.AddHandTiles(_handTilesKindList, _puzzleManager._matchTilesIndex[i]);

            // 止める(手牌に加える演出時間)
            yield return new WaitForSeconds(PuzzleViewManager.HAND_TILE_MOVE_TIME + 0.1f);

            // 手牌がそろったか判定(手牌が12枚以外の事なんてないからマジックナンバーでもよい！！)
            if (_handTilesKindList.Count >= 12)
            {
                // 役の判定
                _handTilesKindList.Add(_headTilesKind);
                _handTilesKindList.Add(_headTilesKind);
                MahjongLogic.Role role = 
                    MahjongLogic.CalcHandTilesRole(_handTilesKindList, _doraTilesKind, (MahjongLogic.TILE_KIND)((int)MahjongLogic.TILE_KIND.TON + _jikazeCnt));

                // ダメージの計算
                int damage = _battleManager.CalcDamage(role);

                // 役演出開始
                float resultTime = _battleViewManager.BeginRoleResult(role, damage);

                // 止める
                yield return new WaitForSeconds(resultTime + 0.3f);

                // プレイヤーの攻撃
                float enemyHpRate = _battleManager.PlayerAttackCheck(damage);

                // 敵HPゲージの更新
                _battleViewManager.SetEnemyHp(enemyHpRate);

                // ゲームオーバーチェック
                _gameState = _battleManager.IsGameOver();

                // ほんとはこのシーン内で勝敗リザルト出したいけど時間がないので一旦そのまま遷移
                if (_gameState == 1 || _gameState == 2)
                {
                    _isWin = _gameState == 1;

                    // 一定時間待機させてからフェードアウト
                    yield return new WaitForSeconds(3.0f);

                    _fadeImage.DOColor(new Color(0.0f, 0.0f, 0.0f, 1.0f), 1.0f);

                    // フェードしてから遷移
                    yield return new WaitForSeconds(1.0f);

                    // シーン遷移
                    Debug.Log("シーン遷移");
                    yield break;
                }

                // 手牌クリア
                _puzzleViewManager.ClearHandTiles();
                _handTilesKindList.Clear();

                // ドラの決定
                _doraTilesKind = _puzzleManager.GetRandomTileKind();
                // 雀頭の決定
                _headTilesKind = _puzzleManager.GetRandomTileKind();
                // 自風のカウント
                _jikazeCnt = (_jikazeCnt + 1) % 4;
                // ドラと雀頭の設定
                _puzzleViewManager.SetDoraHeadJikazeKind(_doraTilesKind, _headTilesKind, _jikazeCnt);
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
