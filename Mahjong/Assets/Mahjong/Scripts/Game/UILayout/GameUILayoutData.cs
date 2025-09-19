using UnityEngine;

[CreateAssetMenu(fileName = "GameUILayoutData", menuName = "Game/GameUILayoutData")]
public class GameUILayoutData : ScriptableObject
{
    // 基準の画面の高さ(サイズを決めるときに使用していた画面の高さ)
    [SerializeField] private int _baseScreenHeight = 2532; // iPhone14
    public int BaseScreenHeight => _baseScreenHeight;

    // 基準の画面の幅(サイズを決めるときに使用していた画面の幅)
    [SerializeField] private int _baseScreenWidth = 1170; // iPhone14
    public int BaseScreenWidth => _baseScreenWidth;

    // 画面の縦サイズにおける、パズル + 取得牌表示の最大割合
    [SerializeField] private float _maxHeightUiRate = 0.65f;
    public float MaxHeightUiRate => _maxHeightUiRate;

    // 画面上の空白のサイズ
    [SerializeField] private float _topSafeBlank = 64.0f;
    public float TopSafeBlank => _topSafeBlank;

    // 画面下の空白のサイズ
    [SerializeField] private float _buttomSafeBlank = 36.0f;
    public float ButtomSafeBlank => _buttomSafeBlank;

    // 画面左右の空白のサイズ
    [SerializeField] private float _sideSafeBlank = 24.0f;
    public float SideSafeBlank => _sideSafeBlank;

    // UI同士の縦の空白のサイズ
    [SerializeField] private float _heightBlank = 24.0f;
    public float HeightBlank => _heightBlank;

    // パズル盤面の枠の幅のサイズ
    [SerializeField] private float _puzzleBlank = 32.0f;
    public float PuzzleBlank => _puzzleBlank;

    // プレイヤーキャラの画像の縦のサイズ
    [SerializeField] private float _playerCharaImageSize = 160.0f;
    public float PlayerCharaImageSize => _playerCharaImageSize;

    // プレイヤーHPゲージの縦のサイズ
    [SerializeField] private float _playerHpGaugeHeight = 24.0f;
    public float PlayerHpGaugeHeight => _playerHpGaugeHeight;

    // 敵HPゲージの縦のサイズ
    [SerializeField] private float _enemyHpGaugeHeight = 42.0f;
    public float EnemyHpGaugeHeight => _enemyHpGaugeHeight;

    // 敵攻撃ゲージの縦のサイズ
    [SerializeField] private float _enemyAttackGaugeHeight = 24.0f;
    public float EnemyAttackGaugeHeight => _enemyAttackGaugeHeight;

    // 自風牌とドラ牌の隙間のサイズ
    [SerializeField] private float _jikazeDoraBlank = 32.0f;
    public float JikazeDoraBlank => _jikazeDoraBlank;

    // パズルタイルの周りの空白割合
    [SerializeField] private float _puzzleTileMarginRate = 0.05f;
    public float PuzzleTileMarginRate => _puzzleTileMarginRate;
}
