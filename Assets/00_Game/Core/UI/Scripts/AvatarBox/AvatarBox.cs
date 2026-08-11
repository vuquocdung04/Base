using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarBox : BaseBox, IPopupScale
{
    [Header("Buttons")]
    public Button btnSave;

    [Header("Main")]
    public Image mainAvatar;

    [Header("Avatar Items")]
    public AvatarItem avatarItemPrefab;
    public Transform avatarItemParent;

    private readonly List<AvatarItem> _items = new();

    public event Action<int> OnPreviewChanged;

    public int PreviewId { get; private set; }

    protected override void Init()
    {
    }

    protected override void InitState()
    {
    }

    public void Populate(IReadOnlyList<Sprite> sprites)
    {
        foreach (AvatarItem item in _items) Destroy(item.gameObject);
        _items.Clear();

        for (int i = 0; i < sprites.Count; i++)
        {
            AvatarItem item = Instantiate(avatarItemPrefab, avatarItemParent);
            item.Setup(i, sprites[i], OnAvatarItemClicked);
            _items.Add(item);
        }
    }

    public void SetPreview(int id, Sprite sprite)
    {
        PreviewId = id;
        mainAvatar.sprite = sprite;

        foreach (AvatarItem item in _items) item.SetChoosing(item.IdAvatar == id);
    }

    private void OnAvatarItemClicked(AvatarItem item) => OnPreviewChanged?.Invoke(item.IdAvatar);
}
