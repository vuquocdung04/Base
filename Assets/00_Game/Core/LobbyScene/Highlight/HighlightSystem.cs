using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class HighlightSystem : MonoBehaviour
{
  public static HighlightSystem Instance { get; private set; }
  [SerializeField] int highlightLayerIndex = 6;

  private Dictionary<Renderer, int> _originalLayers = new Dictionary<Renderer, int>();
  private List<GameObject> _currentTargets = new List<GameObject>();

  public void Init()
  {
    Instance = this;
  }

  public void Highlight(GameObject target)
  {
    if (target == null) return;
    Highlight(new List<GameObject> { target });
  }

  public void Highlight(List<GameObject> targets)
  {
    Clear();

    if (targets == null || targets.Count == 0) return;

    _currentTargets.AddRange(targets);

    foreach (var target in targets)
    {
      if (target == null) continue;

      var renderers = target.GetComponentsInChildren<Renderer>(true);
      foreach (var renderer in renderers)
      {
        if (!_originalLayers.ContainsKey(renderer))
        {
          _originalLayers[renderer] = renderer.gameObject.layer;
        }

        renderer.gameObject.layer = highlightLayerIndex;
      }
    }

    HighlightRendererFeature.IsActive = true;
  }

  public void Clear()
  {
    foreach (var kvp in _originalLayers)
    {
      if (kvp.Key != null)
      {
        kvp.Key.gameObject.layer = kvp.Value;
      }
    }

    _originalLayers.Clear();
    _currentTargets.Clear();
    HighlightRendererFeature.IsActive = false;
  }
}