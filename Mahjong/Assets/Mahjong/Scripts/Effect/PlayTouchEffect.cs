using UnityEngine;

public class PlayTouchEffect : MonoBehaviour
{
    [SerializeField] private TouchInputHandler _input;
    [SerializeField] private EffectManager _effectManager;

    void Update()
    {
        if (_input.GetTouchState() == TouchInputHandler.TouchState.TouchStarted)
        {
            _effectManager.PlayEffect(EffectManager.EFFECT_KIND.TOUCH, _input.GetTouchPosition());
        }
    }
}
