using UnityEngine;

public abstract class IntroStep : MonoBehaviour
{
    public virtual void Prepare() { }

    public abstract Awaitable Play();
}
