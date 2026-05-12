using DG.Tweening;
using UnityEngine;

public class HighlightSystem : MonoBehaviour
{
  public static HighlightSystem Instance { get; private set; }
  [SerializeField] int highlightLayerIndex = 6;
  GameObject _current;
  int[] _originalLayers;

  public void Init()
  {
    Instance = this;
  }

  void OnDestroy()
  {
    Clear();
  }

  public void Highlight(GameObject target)
  {
    Clear();
    _current = target;
    var renderers = target.GetComponentsInChildren<Renderer>(true);
    _originalLayers = new int[renderers.Length];
    for (int i = 0; i < renderers.Length; i++)
    {
      _originalLayers[i] = renderers[i].gameObject.layer;
      renderers[i].gameObject.layer = highlightLayerIndex;
    }
    HighlightRendererFeature.IsActive = true;
  }

  public void Clear()
  {
    if (_current != null)
    {
      var renderers = _current.GetComponentsInChildren<Renderer>(true);
      for (int i = 0; i < renderers.Length && i < _originalLayers.Length; i++)
        renderers[i].gameObject.layer = _originalLayers[i];
      _current = null;
      _originalLayers = null;
    }
    HighlightRendererFeature.IsActive = false;
  }


}
