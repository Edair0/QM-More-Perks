using HarmonyLib;
using MGSC;
using ModConfigMenu.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static HarmonyLib.Code;
using static MGSC.BinaryPresetsMap;
using static System.Net.Mime.MediaTypeNames;

namespace MorePerks
{
    public class ModPatches
    {
        [HarmonyPatch(typeof(MercenaryClassScreen), nameof(MercenaryClassScreen.OnEnable))]
        public static class Patch_MercenaryClassScreen_OnEnable
        {
            public static void Prefix(MercenaryClassScreen __instance)
            {
                if (__instance._perSlots.Length != 10) { __instance._perSlots = ScreenHelper.IncreaseSlots(__instance._perSlots, __instance._classBgIcon.transform); }
                ScreenHelper.PositionSlots(__instance._perSlots);
            }
            public static void Postfix(MercenaryClassScreen __instance)
            {
                ScreenHelper.RefreshSlots(__instance._perSlots);
            }
        }

        [HarmonyPatch(typeof(SelectClassScreen), nameof(SelectClassScreen.OnEnable))]
        public static class Patch_SelectClassScreen_OnEnable
        {
            public static void Prefix(SelectClassScreen __instance)
            {
                if (__instance._perkSlots.Length != 10) { __instance._perkSlots = ScreenHelper.IncreaseSlots(__instance._perkSlots, __instance._classBgIcon.transform); }
                ScreenHelper.PositionSlots(__instance._perkSlots);
            }
        }

        [HarmonyPatch(typeof(SelectClassScreen), nameof(SelectClassScreen.PanelOnSelectClass))]
        public static class Patch_SelectClassScreen_PanelOnSelectClass
        {
            public static void Postfix(SelectClassScreen __instance, MercenaryClassPanel arg1, string arg2)
            {
                if (!__instance._selectedClassId.Equals(__instance._merc.MercClassId)) { ScreenHelper.HideCustomSlots(__instance._perkSlots); }
                else 
                {
                    ScreenHelper.RefreshSlots(__instance._perkSlots);
                }
            }
        }

        [HarmonyPatch(typeof(SelectClassScreen), nameof(SelectClassScreen.RefreshClassBlock))]
        public static class Patch_SelectClassScreen_RefreshClassBlock
        {
            public static void Prefix(SelectClassScreen __instance, List<Perk> perks, MercenaryClassRecord record)
            {
                if (perks != null)
                {
                    for (int i = 7; i < perks.Count; i++)
                    {
                        if (perks[i].PerkType == PerkType.Talent)
                        {
                            perks[i].PerkType = PerkType.Passive;
                        }
                    }
                }
            }

            public static void Postfix(SelectClassScreen __instance)
            {
                if (__instance._perkSlots != null)
                {
                    for (int i = 7; i < 10; i++)
                    {
                        if (__instance._perkSlots[i]._perk != null && __instance._perkSlots[i]._perk.PerkId.Contains("talent_"))
                        {
                            __instance._perkSlots[i]._perk.PerkType = PerkType.Talent;
                        }
                    }
                    ScreenHelper.RefreshSlots(__instance._perkSlots);
                }
            }
        }

        public static class ScreenHelper
        {
            public static int CurrentAdditionalPerks = 0;

            public static PerkSlot[] IncreaseSlots(PerkSlot[] currentSlots, Transform parent)
            {
                List<PerkSlot> newSlots = currentSlots.Take(7).ToList();
                for (int i = 0; i < 3; i++)
                {
                    PerkSlot newPerkSlot = UnityEngine.Object.Instantiate(currentSlots[0], parent);
                    newPerkSlot.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
                    newSlots.Add(newPerkSlot);
                }
                return newSlots.ToArray();
            }

            public static void PositionSlots(PerkSlot[] perkSlots)
            {
                int NewAdditionalPerks = Plugin.ConfigGeneral.ModData.GetConfigValue<int>(Keys.PerkAmount);

                if (CurrentAdditionalPerks != NewAdditionalPerks) 
                {
                    if (NewAdditionalPerks == 0)
                    {
                        perkSlots[7].transform.localScale = new Vector3(0.75f, 0.75f, 0f);
                        perkSlots[8].transform.localScale = new Vector3(0.75f, 0.75f, 0f);
                        perkSlots[9].transform.localScale = new Vector3(0.75f, 0.75f, 0f);
                    }
                    if (NewAdditionalPerks == 1)
                    {
                        perkSlots[7].transform.localPosition = new Vector3(-46f, 0f, 0f);
                    }
                    if (NewAdditionalPerks == 2)
                    {
                        perkSlots[7].transform.localPosition = new Vector3(-35f, 2f, 0f);
                        perkSlots[8].transform.localPosition = new Vector3(-57f, 2f, 0f);
                    }
                    if (NewAdditionalPerks == 3)
                    {
                        perkSlots[7].transform.localPosition = new Vector3(-46f, 0f, 0f);
                        perkSlots[8].transform.localPosition = new Vector3(-66f, -13f, 0f);
                        perkSlots[9].transform.localPosition = new Vector3(-26f, -13f, 0f);
                    }
                }
            }

            public static void RefreshSlots(PerkSlot[] perkSlots)
            {
                for (int i = 7; i < perkSlots.Length; i++)
                {
                    if (perkSlots[i]._perk == null)
                    {
                        perkSlots[i].transform.localScale = new Vector3(0.75f, 0.75f, 0f);
                        perkSlots[i]._cooldown.gameObject.SetActive(false);
                    }
                    else
                    {
                        perkSlots[i].transform.localScale = new Vector3(0.75f, 0.75f, 1f);
                        perkSlots[i]._cooldown.gameObject.SetActive(true);
                    }
                }
            }

            public static void HideCustomSlots(PerkSlot[] perkSlots)
            {
                for (int i = 7; i < perkSlots.Length; i++)
                {
                    perkSlots[i].transform.localScale = new Vector3(0.75f, 0.75f, 0f);
                }
            }
        }


        [HarmonyPatch(typeof(MercenarySystem), nameof(MercenarySystem.ApplyClassForMercenary))]
        public static class Patch_MercenarySystem_ApplyClassForMercenary
        {
            public static PerkFactory StoredPerkFactory { get; set; }

            public static void Prefix(PerkFactory perkFactory)
            {
                if (StoredPerkFactory == null)
                {
                    StoredPerkFactory = perkFactory;
                }
            }
        }

        [HarmonyPatch(typeof(Mercenary), nameof(Mercenary.SetMercClass))]
        public static class Patch_Mercenary_SetMercClass
        {
            public static void Prefix(Mercenary __instance, List<Perk> perks)
            {
                HashSet<string> PerksToAdd = new HashSet<string>();
                HashSet<string> ExistingPerks = new HashSet<string>(perks.Select(p => p.PerkId));

                int AdditionalPerks = Plugin.ConfigGeneral.ModData.GetConfigValue<int>(Keys.PerkAmount);
                int[] PerkIDs = { 
                    Plugin.ConfigGeneral.ModData.GetDropdownValue<int>(Keys.FirstPerk), 
                    Plugin.ConfigGeneral.ModData.GetDropdownValue<int>(Keys.SecondPerk), 
                    Plugin.ConfigGeneral.ModData.GetDropdownValue<int>(Keys.ThirdPerk) 
                };

                for(int i = 0; i < AdditionalPerks; i++)
                {
                    if (PerkIDs[i] == 0)
                    {
                        AddRandomPerkID(PerksToAdd, ExistingPerks);
                    }
                    else
                    {
                        AddPerkID(Plugin.ConfigGeneral.AllowedPerks[PerkIDs[i] - 1], PerksToAdd, ExistingPerks);            
                    }
                }
                Plugin.Logger.Log($"!!!Adding perks to mercenary!!!");
                foreach (string newPerk in PerksToAdd)
                {
                    Plugin.Logger.Log($"Adding perk {newPerk} to mercenary.");;
                    Perk perk = Patch_MercenarySystem_ApplyClassForMercenary.StoredPerkFactory.CreatePerk(Data.Perks.GetRecord(newPerk, true));
                    perks.Add(perk);
                }
            }
            public static void AddPerkID(string PerkID, HashSet<string> PerksToAdd, HashSet<string> ExistingPerks)
            {
                if (!ExistingPerks.Contains(PerkID) && !PerksToAdd.Contains(PerkID))
                {
                    PerksToAdd.Add(PerkID);
                }
                else
                {
                    AddRandomPerkID(PerksToAdd, ExistingPerks);
                }
            }

            public static void AddRandomPerkID(HashSet<string> PerksToAdd, HashSet<string> ExistingPerks)
            {
                for (int i = 0; i < 100; i++)
                {
                    string randomPerkID = Plugin.ConfigGeneral.AllowedPerks[UnityEngine.Random.Range(0, Plugin.ConfigGeneral.AllowedPerks.Count)];
                    if (!ExistingPerks.Contains(randomPerkID) && !PerksToAdd.Contains(randomPerkID))
                    {

                        PerksToAdd.Add(randomPerkID);
                        break;
                    }
                    if(i == 99)
                    {
                        Plugin.Logger.LogWarning("Could not find a unique perk to add after 100 attempts.");
                    }
                }       
            }
        }
    }
}
