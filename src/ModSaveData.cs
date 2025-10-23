using HarmonyLib;
using MGSC;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace MorePerks
{

    // Patch to create slots for MercenaryClassScreen (screen used on missions to display character class)
    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SaveGame))]
    public static class Patch_SaveManager_SaveGame
    {
        public static void Postfix()
        {
            Plugin.Save.SaveToDisk();
        }
    }

    // Patch to create slots for MercenaryClassScreen (screen used on missions to display character class)
    [HarmonyPatch(typeof(GameModeStateMachine), nameof(GameModeStateMachine.ProcessStartGame))]
    public static class Patch_GameModeStateMachine_ProcessStartGame
    {
        
        public static void Postfix(int slot)
        {
            Plugin.Save.LoadFromDisk();
            Plugin.Save.CurrentSlot = slot;
        }
    }

    public class ModSave
    {
        public int CurrentSlot { get; set; }

        private string SavePath;
        private Dictionary<int, SlotData> SaveData;

        public ModSave(string savePath)
        {
            this.SavePath = savePath;
            this.SaveData = LoadFromDisk();
            this.CurrentSlot = 0;
        }

        public T GetCurrentSlotValue<T>(string key)
        {
            return GetSlot(CurrentSlot).GetValue<T>(key);
        }

        public void SetCurrentSlotValue<T>(string key, T value)
        {
            GetSlot(CurrentSlot).SetValue<T>(key, value);
        }

        public SlotData GetSlot(int slot)
        {
            if (!SaveData.TryGetValue(slot, out SlotData slotData))
            {
                slotData = new SlotData();
                SaveData[slot] = slotData;
            }
            return slotData;
        }

        public void ClearSlot(int slot)
        {
            SaveData[slot] = new SlotData();
        }

        public void SaveToDisk()
        {
            JSONObject rootNode = new JSONObject();
            foreach (KeyValuePair<int, SlotData> kvp in SaveData)
            {
                rootNode[kvp.Key.ToString()] = kvp.Value.ToJson();
            }

            File.WriteAllText(SavePath, rootNode.ToString());
        }

        public Dictionary<int, SlotData> LoadFromDisk()
        {
            if (!File.Exists(SavePath))
            {
                SaveData = new Dictionary<int, SlotData>();
                return SaveData;
            }

            string json = File.ReadAllText(SavePath);
            JSONObject rootNode = JSON.Parse(json) as JSONObject;

            Dictionary<int, SlotData> loadedData = new Dictionary<int, SlotData>();
            foreach (string key in rootNode.Keys)
            {
                if (int.TryParse(key, out int slotId))
                {
                    loadedData[slotId] = new SlotData(rootNode[key]);
                }
                else
                {
                    Plugin.Logger.LogWarning($"Incorrect key encountered while loading json data: {key}");
                }
            }

            SaveData = loadedData;
            return SaveData;
        }
    }

    public class SlotData
    {
        private Dictionary<string, SlotEntry> slotData;

        public SlotData(JSONNode node = null)
        {
            slotData = new Dictionary<string, SlotEntry>();
            if (node != null)
            {
                foreach (string key in node.Keys)
                {
                    slotData[key] = new SlotEntry(node[key]);
                }
            }
        }

        public void SetValue<T>(string key, T value)
        {
            ValType type;
            if (value is bool) { type = ValType.Bool; }
            else if (value is int) { type = ValType.Int; }
            else if (value is float) { type = ValType.Float; }
            else if (value is string) { type = ValType.String; }
            else { type = ValType.Unsupported; }
            
            if (type != ValType.Unsupported) { slotData[key] = new SlotEntry(value, type); }
            else
            {
                Plugin.Logger.LogError($"[SlotEntry] Unsupported type for SlotData is being used for key:{key}");
                slotData[key] = new SlotEntry(0, ValType.Int);
            }
            
        }

        public T GetValue<T>(string key, T fallback = default)
        {
            if (slotData.TryGetValue(key, out var entry))
            {
                return entry.GetValue<T>(fallback);
            }
            Plugin.Logger.LogWarning($"Unable to get value for key:{key} | Data not found.");
            return fallback;
        }

        public bool Contains(string key)
        {
            return slotData.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return slotData.Remove(key);
        }

        public JSONNode ToJson()
        {
            var node = new JSONObject();
            foreach (var kvp in slotData)
            {
                node[kvp.Key] = kvp.Value.ToJson();
            }
            return node;
        }
    }

    public class SlotEntry
    {
        public object Value;
        public ValType Type;

        public SlotEntry(object value, ValType type)
        {
            Value = value;
            Type = type;
        }

        public SlotEntry(JSONNode node)
        {
            try
            {
                Type = (ValType)Enum.Parse(typeof(ValType), node["Type"]);
                switch (Type)
                {
                    case ValType.Bool: Value = node["Value"].AsBool; break;
                    case ValType.Int: Value = int.Parse(node["Value"]); break;
                    case ValType.Float: Value = float.Parse(node["Value"]); break;
                    case ValType.String: Value = node["Value"]; break;
                }
            }
            catch (Exception ex)
            {
                Value = 0;
                Type = ValType.Int;
                Plugin.Logger.LogError($"[SlotEntry] Deserialization error: {ex}");
            }

        }
        public void SetValue(object value, ValType type)
        {
            Value = value;
            Type = type;
        }

        public T GetValue<T>(T fallback = default)
        {
            if (Value is T variable) { return variable; }
            return fallback;
        }

        public JSONNode ToJson()
        {
            JSONObject node = new JSONObject();
            node["Type"] = Type.ToString();
            switch (Type)
            {
                case ValType.Bool: node["Value"] = (bool)Value; break;
                case ValType.Int: node["Value"] = (int)Value; break;
                case ValType.Float: node["Value"] = (float)Value; break;
                case ValType.String: node["Value"] = (string)Value; break;
            }
            return node;
        }
    }

    public enum ValType
    {
        Bool,
        Int,
        Float,
        String,
        Unsupported
    }

}
