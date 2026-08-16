using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AvatarBox : BaseBox, IPopupScale
{
    [Header("Buttons")]
    public Button btnSave;

    [Header("Main")]
    public Image mainAvatar;
    public TMP_InputField inputName;

    [Header("Avatar Items")]
    public AvatarItem avatarItemPrefab;
    public Transform avatarItemParent;
    public ScrollRect scrollAvatar;

    private readonly List<AvatarItem> _items = new();

    public int PreviewId { get; private set; }

    private static AvatarDataBase Data => DataRepo.Instance != null ? DataRepo.Instance.avatarData : null;

    protected override void Init()
    {
        btnSave.OnClicked(Save);
        Populate();
    }

    protected override void InitState()
    {
        SetPreview(UseProfile.AvatarId);
        inputName.text = UseProfile.PlayerName;
        ResetScrollAsync().Forget();
    }

    public void Save()
    {
        string playerName = inputName.text.Trim();
        UseProfile.PlayerName = string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName;
        UseProfile.AvatarId = PreviewId;

        LobbyBox lobby = PopupManager.Peek<LobbyBox>();
        if (lobby != null) lobby.RefreshProfile();

        Close();
    }

    private void Populate()
    {
        foreach (AvatarItem item in _items) Destroy(item.gameObject);
        _items.Clear();

        if (Data == null) return;

        for (int i = 0; i < Data.Count; i++)
        {
            AvatarItem item = Instantiate(avatarItemPrefab, avatarItemParent);
            item.Setup(i, Data.GetSpriteById(i), OnAvatarItemClicked);
            _items.Add(item);
        }
    }

    private async Awaitable ResetScrollAsync()
    {
        if (scrollAvatar == null) return;

        StopScroll();
        await Awaitable.NextFrameAsync(destroyCancellationToken);
        StopScroll();
    }

    private void StopScroll()
    {
        scrollAvatar.StopMovement();
        Canvas.ForceUpdateCanvases();
        scrollAvatar.verticalNormalizedPosition = 1f;
    }

    private void SetPreview(int id)
    {
        PreviewId = id;
        mainAvatar.sprite = Data != null ? Data.GetSpriteById(id) : null;

        foreach (AvatarItem item in _items) item.SetChoosing(item.IdAvatar == id);
    }

    private void OnAvatarItemClicked(AvatarItem item) => SetPreview(item.IdAvatar);
}
