using System;
using UnityEngine;
using UnityEngine.UI;

public enum ESettingMode { OnOff, Slider }

[Serializable]
public class SettingControl
{
    public ESettingMode mode;

    [Header("OnOff")]
    public Button btn;
    public Image img;
    public Sprite sprOn, sprOff;

    [Header("Slider")]
    public Slider slider;

    private Func<float> get;
    private Action<float> set;

    public void Setup(Func<float> get, Action<float> set)
    {
        this.get = get;
        this.set = set;

        if (mode == ESettingMode.OnOff)
            btn.OnClicked(() => Apply(get() > 0 ? 0f : 1f));
        else
            slider.onValueChanged.AddListener(Apply);

        Refresh();
    }

    private void Apply(float value)
    {
        set(value);
        Refresh();
    }

    private void Refresh()
    {
        if (mode == ESettingMode.OnOff)
            img.SetSprite(get() > 0 ? sprOn : sprOff);
        else
            slider.SetValueWithoutNotify(get());
    }
}

public abstract class SettingBaseBox : BaseBox, IPopupScale
{
    [Header("Setting Controls")]
    [SerializeField] protected SettingControl sound;
    [SerializeField] protected SettingControl music;
    [SerializeField] protected SettingControl vib;

    public SettingControl Sound => sound;
    public SettingControl Music => music;
    public SettingControl Vib => vib;

    protected sealed override void Init() => OnInit();

    protected override void InitState() { }

    protected virtual void OnInit() { }
}
