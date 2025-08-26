using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MahjongTileView : MonoBehaviour
{
    [SerializeField] private TileImages _tileImages;
    [SerializeField] private Image _image;
    [SerializeField] private RectTransform _rectTransform;

    private MahjongLogic.TILE_KIND _kind = MahjongLogic.TILE_KIND.HAKU;
    
    void Start()
    {
        _image.sprite = _tileImages.tileImages[(int)_kind];
    }

    /// <summary>
    /// 種類の設定
    /// </summary>
    /// <param name="kind">牌種</param>
    public void SetKind(MahjongLogic.TILE_KIND kind)
    {
        _kind = kind;

        if (kind == MahjongLogic.TILE_KIND.NONE)
        {
            // NONEの場合はグレースケール白
            _image.sprite = _tileImages.tileImages[(int)MahjongLogic.TILE_KIND.HAKU];
            _image.color = Color.gray;
        }
        else
        {
            _image.sprite = _tileImages.tileImages[(int)_kind];
        }
    }

    public MahjongLogic.TILE_KIND GetKind() => _kind;

    /// <summary>
    /// 座標の設定
    /// </summary>
    /// <param name="pos">移動先座標</param>
    /// <param name="moveTime">移動時間</param>
    /// <param name="delayTime">移動開始時間</param>
    public void SetPos(Vector2 pos, float moveTime = 0.0f, float delayTime = 0.0f)
    {
        if (delayTime > 0.0f)
        {
            StartCoroutine(SetPostCoroutine(pos, moveTime, delayTime));
            return;
        }

        if (moveTime > 0.0f)
        {
            _rectTransform.DOAnchorPos(pos, moveTime).SetEase(Ease.InSine);
            return;
        }

        _rectTransform.anchoredPosition = pos;
    }
    IEnumerator SetPostCoroutine(Vector2 pos, float moveTime = 0.0f, float delayTime = 0.0f)
    {
        yield return new WaitForSeconds(delayTime);
        SetPos(pos, moveTime);
    }

    /// <summary>
    /// 座標の取得
    /// </summary>
    /// <returns>座標</returns>
    public Vector2 GetPos() { return _rectTransform.anchoredPosition; }

    /// <summary>
    /// 拡縮の設定
    /// </summary>
    /// <param name="scale">拡縮</param>
    /// <param name="scaleTime">拡縮時間</param>
    /// <param name="delayTime">拡縮開始時間</param>
    public void SetScale(float scale, float scaleTime = 0.0f, float delayTime = 0.0f)
    {
        if (delayTime > 0.0f)
        {
            StartCoroutine(ScalePosCoroutine(scale, scaleTime, delayTime));
            return;
        }

        if (scaleTime > 0.0f)
        {
            _rectTransform.DOScale(new Vector3(scale, scale, scale), scaleTime).SetEase(Ease.InOutSine);
            return;
        }

        _rectTransform.localScale = new Vector3(scale, scale, scale);
    }
    IEnumerator ScalePosCoroutine(float scale, float scaleTime = 0.0f, float delayTime = 0.0f)
    {
        yield return new WaitForSeconds(delayTime);
        SetScale(scale, scaleTime);
    }
}
