using UnityEngine;

public class PlayTouchEffect : MonoBehaviour
{
    [SerializeField] private TouchInputHandler _input;

    void Update()
    {
        if (_input.GetTouchState() == TouchInputHandler.TouchState.TouchStarted)
        {
            EffectManager._instance.PlayEffect(EffectManager.EFFECT_KIND.TOUCH, _input.GetTouchPosition());
        }
    }
}
