using System;
using System.Collections.Generic;
using UnityEngine;


namespace PopupMini
{
    [Serializable] public class ItemReward { public string id; public int count = 1; }
    [Serializable] public class RewardEnvelope { public ItemReward[] rewards; }


    public static class RewardPayload
    {
        public static bool TryParse(string payload, out List<ItemReward> list)
        {
            list = null; if (string.IsNullOrWhiteSpace(payload)) return false;
            try
            {
                string trimmed = payload.Trim();
                // 1) Array envelope
                if (trimmed.Contains("\"rewards\""))
                {
                    var env = JsonUtility.FromJson<RewardEnvelope>(trimmed);
                    if (env?.rewards != null && env.rewards.Length > 0)
                    { list = new List<ItemReward>(env.rewards); return true; }
                    return false;
                }
                // 2) Single object
                if (trimmed.StartsWith("{"))
                {
                    var one = JsonUtility.FromJson<ItemReward>(trimmed);
                    if (one != null && !string.IsNullOrEmpty(one.id))
                    { list = new List<ItemReward> { one }; return true; }
                    return false;
                }
                // 3) Plain string (quoted)
                if (trimmed.StartsWith("\"") && trimmed.EndsWith("\""))
                {
                    string id = trimmed.Trim('"');
                    list = new List<ItemReward> { new ItemReward { id = id, count = 1 } };
                    return true;
                }
            }
            catch (Exception e) { Debug.LogWarning($"[RewardPayload] Parse error: {e.Message} | payload={payload}"); }
            return false;
        }
    }
}