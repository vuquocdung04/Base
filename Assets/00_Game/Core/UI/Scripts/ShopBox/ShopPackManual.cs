using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPackManual : ShopPackBase
{
    [Serializable, HideReferenceObjectPicker]
    public class Slot
    {
        public Image icon;
        public TextMeshProUGUI txtQuantity;
    }

    [Header("Reward Slots")]
    [Tooltip("Kéo tay theo đúng thứ tự thưởng trong catalog. Ô thừa sẽ tự trống.")]
    public List<Slot> slots = new();

    protected override void FillRewards()
    {
        List<GameReward> rewards = Config.CollectRewards();
        RewardManager manager = RewardManager.Instance;

        for (int i = 0; i < slots.Count; i++)
        {
            Slot slot = slots[i];
            if (slot == null) continue;

            GameReward reward = i < rewards.Count ? rewards[i] : null;

            if (slot.icon != null)
            {
                Sprite sprite = reward != null && manager != null ? manager.GetIcon(reward.id) : null;

                slot.icon.sprite = sprite;
                slot.icon.preserveAspect = true;
                slot.icon.enabled = sprite != null;
            }

            if (slot.txtQuantity != null)
            {
                slot.txtQuantity.text = reward != null && manager != null
                    ? manager.Format(reward.id, reward.quantity)
                    : string.Empty;
            }
        }

        if (rewards.Count > slots.Count)
            Debug.LogWarning($"[Shop] '{packId}' có {rewards.Count} thưởng nhưng chỉ có {slots.Count} ô.", this);
    }
}
