using UnityEditor.Animations;
using UnityEngine;

public class EffectPlayer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Animator _animator;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    /// <summary>
    /// エフェクト終了時処理
    /// </summary>
    public void OnEffectEnd()
    {
        // 削除
        Destroy(this.gameObject);
    }

    /// <summary>
    /// エフェクトのセット
    /// </summary>
    /// <param name="beginImage">開始画像</param>
    /// <param name="controller">アニメーターコントローラー</param>
    /// <param name="speed">再生速度</param>
    public void SetEffect(Sprite beginImage, AnimatorController controller, float speed)
    {
        _spriteRenderer.sprite = beginImage;
        _animator.runtimeAnimatorController = controller;
        _animator.speed = speed;
    }
}
