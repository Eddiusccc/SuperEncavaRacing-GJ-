using DG.Tweening;
using UnityEngine;

public class Scale_animation : MonoBehaviour
{
    public float scaleTarget;
    public float duration;
    Tween t;

    private void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        t = rectTransform.DOScale(scaleTarget, duration).SetLoops(-1, LoopType.Yoyo);
    }

    public void StopScale()
    {
        t.Complete();
        t.Kill();
    }

    public void PlayScale()
    {
        t.Play();
    }
}
