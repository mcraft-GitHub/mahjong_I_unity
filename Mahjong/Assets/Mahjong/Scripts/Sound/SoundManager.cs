using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // 合計切り替え時間から、1つのBGMのフェード時間を求める
    private const float CALC_FADE_TIME_FROM_TOTAL_TIME = 0.5f;

    // シングルトン用
    public static SoundManager _instance { get; private set; }

    // マスターボリューム
    private static float _masterBgmVolume = 0.5f;
    private static float _masterSeVolume = 0.5f;

    public enum BGM_NAME
    {
        TITLE = 0,
        HOME,
        GAME_BATTLE,
        GAME_ADVANCE,
        RESULT,
        OTHER,
        MAX,
    }
    public enum SE_NAME
    {
        BUTTON_PUSH = 0,
        BUTTON_BACK,
        PUZZLE_MOVE,
        PUZZLE_MATCH,
        BATTLE_ATTTACK_FIRE,
        BATTLE_ATTTACK_WATER,
        BATTLE_ATTTACK_WOOD,
        BATTLE_ATTTACK_VOID,
        BATTLE_DESTROY,
        BATTLE_ROLE_POINT,
        MAX,
    }

    [System.Serializable]
    public class BgmData
    {
        public BGM_NAME _name;
        public float _baseVolume;
        public AudioClip _audioClip;
    }

    [System.Serializable]
    public class SeData
    {
        public SE_NAME _name;
        public float _baseVolume;
        public AudioClip _audioClip;
    }

    [SerializeField] private AudioSource _bgmAudioSource;
    [SerializeField] private AudioSource _seAudioSource;

    // サウンドデータリスト
    [SerializeField] private List<BgmData> _bgmDataList = new List<BgmData>();
    [SerializeField] private List<SeData> _seDataList = new List<SeData>();

    // 辞書化したサウンドデータ
    private Dictionary<BGM_NAME, BgmData> _bgmDataMap = new Dictionary<BGM_NAME, BgmData>();
    private Dictionary<SE_NAME, SeData> _seDataMap = new Dictionary<SE_NAME, SeData>();

    // 再生中のBGM
    private BGM_NAME _playingBgm = BGM_NAME.MAX;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this.gameObject);

        // リストを辞書に置き換え
        foreach (BgmData bgmData in _bgmDataList)
        {
            if (!_bgmDataMap.ContainsKey(bgmData._name))
                _bgmDataMap.Add(bgmData._name, bgmData);
        }
        foreach (SeData seData in _seDataList)
        {
            if (!_seDataMap.ContainsKey(seData._name))
                _seDataMap.Add(seData._name, seData);
        }
    }

    /// <summary>
    /// BGMの再生
    /// </summary>
    /// <param name="name">名前</param>
    /// <param name="volume">音量</param>
    /// <param name="switchingTime">切り替え時間</param>
    public void PlayBGM(BGM_NAME name, float volume, float switchingTime = 0.0f)
    {
        BgmData data;
        if (_playingBgm == name || !_bgmDataMap.TryGetValue(name, out data))
        {
            // 同じBGMまたはBGMが見つからなければ終了
            return;
        }

        if (_bgmAudioSource.isPlaying && switchingTime > 0.0f)
        {
            // フェード時間
            float fadeTime = switchingTime * CALC_FADE_TIME_FROM_TOTAL_TIME;

            // フェード切り替え
            _bgmAudioSource.DOKill();
            _bgmAudioSource.DOFade(0.0f, fadeTime).OnComplete(
                () => {
                    // 情報の適用
                    _bgmAudioSource.clip = data._audioClip;
                    _bgmAudioSource.volume = 0.0f;
                    _bgmAudioSource.loop = true;
                    // 再生
                    _bgmAudioSource.Play();
                    _playingBgm = name;
                    // フェードイン
                    _bgmAudioSource.DOFade(data._baseVolume * volume * _masterBgmVolume, fadeTime);
                });
        }
        else
        {
            // 情報の適用
            _bgmAudioSource.clip = data._audioClip;
            _bgmAudioSource.volume = data._baseVolume * volume * _masterBgmVolume;
            _bgmAudioSource.loop = true;
            // 再生
            _bgmAudioSource.Play();
            _playingBgm = name;
        }
    }

    /// <summary>
    /// SEの再生
    /// </summary>
    /// <param name="name">名前</param>
    /// <param name="volume">音量</param>
    public void PlaySE(SE_NAME name, float volume = 1.0f)
    {
        if (_seDataMap.TryGetValue(name, out SeData data))
        {
            // 再生
            _seAudioSource.PlayOneShot(data._audioClip, data._baseVolume * volume);
        }
    }

    /// <summary>
    /// BGMのマスター音量のセット
    /// </summary>
    /// <param name="masterVolume">新しいBGMのマスター音量</param>
    public void SetMasterBgmVolume(float masterVolume)
    {
        // 再生中の音量の変更
        if (_bgmAudioSource.isPlaying)
        {
            if (_masterBgmVolume == 0.0f)
            {
                // 元が0の場合は新しい音量にする
                _bgmAudioSource.volume = masterVolume;
            }
            else
            {
                // 変わった割合の計算
                float volumeRate = masterVolume / _masterBgmVolume;

                // 適用
                _bgmAudioSource.volume = _bgmAudioSource.volume * volumeRate;
            }
        }
        _masterBgmVolume = masterVolume;
    }

    /// <summary>
    /// SEのマスター音量のセット
    /// </summary>
    /// <param name="masterVolume">新しいSEのマスター音量</param>
    public void SetMasterSeVolume(float masterVolume)
    {
        _masterSeVolume = masterVolume;
    }
}
