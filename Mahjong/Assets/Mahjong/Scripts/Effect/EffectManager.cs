using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class EffectManager : MonoBehaviour
{
    // レンダーテクスチャの深度バッファを無効
    private const int RENDER_TEXTURE_DEPTH = 24;

    // スクリーンサイズを半分にする
    private const float SCREEN_SIZE_HALF = 0.5f;

    public enum EFFECT_KIND
    {
        TOUCH = 0,
        FIRE,
        WATER,
        WOOD,
        VOID,
        OCEAN,
        FOREST,
        FLAME,
        MAX,
    }

    [System.Serializable]
    public class EffectData
    {
        public EFFECT_KIND _kind;
        public Sprite _beginImage;
        public AnimatorController _controller;
        public float _baseScale;
        public float _baseSpeed;
    }

    [SerializeField] private Camera _effectCamera;
    [SerializeField] private RawImage _effectImage;
    [SerializeField] private GameObject _effectPrefab;

    [Space(20)]

    [SerializeField] private List<EffectData> _effectDataList = new List<EffectData>();

    // エフェクト表示座標の原点
    private Vector2 _effectOriginPoint = Vector2.zero;

    // エフェクトデータマップ
    private Dictionary<EFFECT_KIND, EffectData> _effectDataMap = new Dictionary<EFFECT_KIND, EffectData>();

    void Awake()
    {
        // エフェクトを描画するレンダーテクスチャの作成
        RenderTexture effectRenderTexture = new RenderTexture(Screen.width, Screen.height, RENDER_TEXTURE_DEPTH, RenderTextureFormat.ARGB32);
        effectRenderTexture.Create();

        // カメラにレンダーテクスチャをセット
        _effectCamera.targetTexture = effectRenderTexture;

        // エフェクト表示座標の原点の計算
        _effectOriginPoint.x = _effectCamera.transform.position.x - Screen.width * SCREEN_SIZE_HALF;
        _effectOriginPoint.y = _effectCamera.transform.position.y - Screen.height * SCREEN_SIZE_HALF;

        // カメラの解像度を画面に合わせる
        _effectCamera.orthographicSize = Screen.height * SCREEN_SIZE_HALF;

        // エフェクト描画画像にレンダーテクスチャをセット
        _effectImage.color = Color.white;
        _effectImage.texture = effectRenderTexture;

        // エフェクトデータリストをマップに置き換える
        foreach (EffectData data in _effectDataList)
        {
            if (!_effectDataMap.ContainsKey(data._kind))
                _effectDataMap.Add(data._kind, data);
        }
    }

    /// <summary>
    /// エフェクトの再生
    /// </summary>
    /// <param name="kind">エフェクトの種類</param>
    /// <param name="pos">エフェクトの座標</param>
    /// <param name="scale">エフェクトのスケール</param>
    /// <param name="speed">エフェクトの再生速度</param>
    public void PlayEffect(EFFECT_KIND kind, Vector2 pos, float scale = 1.0f, float speed = 1.0f)
    {
        EffectData data = _effectDataMap[kind];

        // エフェクトの生成
        GameObject effectObj = Instantiate(_effectPrefab, transform);
        // 座標のセット(座標の原点が違うので合わせる)
        effectObj.transform.position = new Vector3(pos.x + _effectOriginPoint.x, pos.y + _effectOriginPoint.y, 0.0f);
        // スケールのセット
        effectObj.transform.localScale = Vector3.one * (data._baseScale * scale);
        // アニメーションのセット
        effectObj.GetComponent<EffectPlayer>().SetEffect(data._beginImage, data._controller, data._baseSpeed * speed);
    }
}
