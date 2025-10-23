using HarmonyLib;
using MGSC;
using ModConfigMenu.Services;
using MorePerks.Helper;
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
using UnityEngine.UI;
using static HarmonyLib.Code;
using static MGSC.BinaryPresetsMap;
using static System.Net.Mime.MediaTypeNames;

namespace MorePerks
{
    public class ModPatches
    {
        public static class SaveVars
        {
            public static string MutateUsesLeft = "MutateUsesLeft";
        }


        // Patch to create slots for MercenaryClassScreen (screen used on missions to display character class)
        [HarmonyPatch(typeof(MercenaryClassScreen), nameof(MercenaryClassScreen.OnEnable))]
        public static class Patch_MercenaryClassScreen_OnEnable
        {
            public static void Prefix(MercenaryClassScreen __instance)
            {
                if (__instance._perSlots.Length != 12) { __instance._perSlots = ScreenHelper.IncreaseSlots(__instance._perSlots, __instance._classBgIcon.transform); }
            }

            public static void Postfix(MercenaryClassScreen __instance)
            {
                if (__instance._perSlots.Length == 12)  { ScreenHelper.RefreshSlots(__instance._perSlots); }
            }
        }


        public static CommonButton RerollPerksButton { get; set; }
        public static SelectClassScreen ScreenInstance { get; set; }
        public static PerkFactory StoredPerkFactory { get; set; }

        // Patch to create slots for SelectClassScreen (screen used on orbit to change character class)
        [HarmonyPatch(typeof(SelectClassScreen), nameof(SelectClassScreen.OnEnable))]
        public static class Patch_SelectClassScreen_OnEnable
        {
            public static void Prefix(SelectClassScreen __instance)
            {
                if (__instance._perkSlots.Length != 12) { __instance._perkSlots = ScreenHelper.IncreaseSlots(__instance._perkSlots, __instance._classBgIcon.transform); }
            }

            public static void Postfix(SelectClassScreen __instance)
            {
                if (ScreenInstance == null) { ScreenInstance = __instance; }
                if (RerollPerksButton == null) { RerollPerksButton = CreateClonedButton(__instance._selectClassButton); }

                //ScreenHelper.RefreshSlots(__instance._perkSlots);
                ButtonHelper.CleanClonedButton(RerollPerksButton);

                if (string.IsNullOrEmpty(__instance._selectedClassId) && !string.IsNullOrEmpty(__instance._merc.MercClassId))
                {
                    ButtonHelper.UpdateMutateButtonStatus(RerollPerksButton);
                }
                else
                {
                    RerollPerksButton.gameObject.SetActive(false);
                }
            }

            public static CommonButton CreateClonedButton(CommonButton buttonToClone)
            {
                CommonButton result = UnityEngine.Object.Instantiate(buttonToClone, buttonToClone.transform.parent.transform);
                result.transform.localPosition += new Vector3(0f, -16f, 0f);
                result.ChangeLabel(ModLocalization.MutateButtonCharge.Key);
                result.OnClick += MutatePerkClick;
                return result;
            }

            public static void MutatePerkClick(CommonButton arg1, int arg2)
            {
                if (Plugin.ConfigGeneral.ModData.GetConfigValue<bool>(Keys.FreeMutation))
                {
                    UI.Chain<ConfirmDialogWindow>().Invoke(delegate (ConfirmDialogWindow v)
                    {
                        v.Configure(new Action<ConfirmDialogWindow.Option>(ConfirmMutateFreePerkDialog), ModLocalization.MutateDialogUse.Key, true, null, ModLocalization.MutateConfirm.Key, ModLocalization.MutateReturn.Key);
                    }).Show(true);
                }
                else
                {
                    if (Plugin.Save.GetCurrentSlotValue<int>(SaveVars.MutateUsesLeft) > 0)
                    {
                        UI.Chain<ConfirmDialogWindow>().Invoke(delegate (ConfirmDialogWindow v)
                        {
                            v.Configure(new Action<ConfirmDialogWindow.Option>(ConfirmMutatePerkDialog), ModLocalization.MutateDialogUse.Key, true, null, ModLocalization.MutateConfirm.Key, ModLocalization.MutateReturn.Key);
                        }).Show(true);
                    }
                    else
                    {
                        UI.Chain<ConfirmDialogWindow>().Invoke(delegate (ConfirmDialogWindow v)
                        {
                            v.Configure(new Action<ConfirmDialogWindow.Option>(ChargeMutateDeviceDialog), ModLocalization.MutateDialogCharge.Key, true, null, ModLocalization.MutateConfirm.Key, ModLocalization.MutateReturn.Key);
                        }).Show(true);
                    }
                }
                return;
            }

            public static void ConfirmMutateFreePerkDialog(ConfirmDialogWindow.Option obj)
            {
                if (obj == ConfirmDialogWindow.Option.Yes)
                {
                    // Reroll perks and go back
                    PerkHelper.MutatePerks(ScreenInstance._merc, ScreenInstance._merc.CreatureData.Perks, ScreenInstance._perkFactory);
                    PerkHelper.RefreshMerc(ScreenInstance._merc);
                    ScreenHelper.RefreshSlots(ScreenInstance._perkSlots);
                    UI.Back(false);
                }
            }

            private static void ChargeMutateDeviceDialog(ConfirmDialogWindow.Option obj)
            {
                if (obj == ConfirmDialogWindow.Option.Yes)
                {
                    // Get current uses
                    int usages = Plugin.Save.GetCurrentSlotValue<int>(SaveVars.MutateUsesLeft);

                    // Loop cargo
                    List<ItemStorage> cargo = UI.Get<SpaceshipScreen>()._magnumCargo.ShipCargo;
                    foreach (ItemStorage storage in cargo)
                    {
                        if (storage.ContainsItem("classUSB"))
                        {
                            // Remove first Class Chip
                            storage.RemoveSpecificItem("classUSB", 1);
                            // Increase usages
                            usages = 10;
                            break;
                        }
                    }
                    // Save and go back
                    Plugin.Save.SetCurrentSlotValue<int>(SaveVars.MutateUsesLeft, usages);
                    UI.Back(false);
                }
            }

            private static void ConfirmMutatePerkDialog(ConfirmDialogWindow.Option obj)
            {
                if (obj == ConfirmDialogWindow.Option.Yes)
                {
                    // Reroll perks
                    PerkHelper.MutatePerks(ScreenInstance._merc, ScreenInstance._merc.CreatureData.Perks, ScreenInstance._perkFactory);
                    PerkHelper.RefreshMerc(ScreenInstance._merc);
                    ScreenHelper.RefreshSlots(ScreenInstance._perkSlots);

                    // Decrease uses
                    int usages = Plugin.Save.GetCurrentSlotValue<int>(SaveVars.MutateUsesLeft);
                    if (usages > 0) { usages--; }
                    Plugin.Save.SetCurrentSlotValue<int>(SaveVars.MutateUsesLeft, usages);

                    // Return to previous screen
                    UI.Back(false);
                }
            }
        }

        // Patch to handle displaying/hiding custom perk slots when changing class
        [HarmonyPatch(typeof(SelectClassScreen), nameof(SelectClassScreen.RefreshClassBlock))]
        public static class Patch_SelectClassScreen_RefreshClassBlock
        {
            public static void Postfix(SelectClassScreen __instance)
            {
                if (ScreenInstance == null) { ScreenInstance = __instance; }
                if (RerollPerksButton == null) { RerollPerksButton = Patch_SelectClassScreen_OnEnable.CreateClonedButton(__instance._selectClassButton); }

                ScreenHelper.RefreshSlots(__instance._perkSlots);
                ButtonHelper.CleanClonedButton(RerollPerksButton);
               
                if (__instance._selectedClassId == __instance._merc.MercClassId && !string.IsNullOrEmpty(__instance._selectedClassId))
                {
                    ButtonHelper.UpdateMutateButtonStatus(RerollPerksButton);
                }
                else
                {
                    RerollPerksButton.gameObject.SetActive(false);
                }
            }

        }

        // Patch to store PerkFactory instance for later use when adding custom perks
        [HarmonyPatch(typeof(MercenarySystem), nameof(MercenarySystem.ApplyClassForMercenary))]
        public static class Patch_MercenarySystem_ApplyClassForMercenary
        {
            public static void Prefix(PerkFactory perkFactory) { if (StoredPerkFactory == null) { StoredPerkFactory = perkFactory; } }
        }

        // Patch to add custom perks when setting mercenary class
        [HarmonyPatch(typeof(Mercenary), nameof(Mercenary.SetMercClass))]
        public static class Patch_Mercenary_SetMercClass
        {
            public static void Prefix(Mercenary __instance, List<Perk> perks)
            {
                if (perks == null) { return; }

                // We need to keep perks when changing classes. Doesn't work when clone dies as clone is deleted.
                foreach (Perk perk in __instance.CreatureData.Perks)
                {
                    if (perk.HasParameter("MorePerks_CustomPerk") && !perks.Any(p => p.PerkId == perk.PerkId))
                    {
                        perks.Add(perk);
                    }
                }

                // Exisitng perk HashSet for fast checks, gets updated automatically if passed to PerkHelper functions
                HashSet<string> ExistingPerks = new HashSet<string>(perks.Select(p => p.PerkId));

                if (Plugin.ConfigGeneral.ModData.GetConfigValue<bool>(Keys.StartPerks))
                {
                    // We keep adding perks until enough are generated
                    int perksLeftToAdd = Plugin.ConfigGeneral.ModData.GetConfigValue<int>(Keys.PerkAmount) - perks.Count(perk => perk.HasParameter("MorePerks_CustomPerk"));
                    while (perksLeftToAdd > 0)
                    {
                        perks.Add(PerkHelper.GetRandomCustomPerk(StoredPerkFactory, ExistingPerks, true));
                        perksLeftToAdd--;
                    }
                }

                // If character used to have extra slot talent but doens't have it anymore then its need to be removed manually
                if (!ExistingPerks.Contains("talent_weapon_slot")) { __instance.CreatureData.Inventory.AdditionalSlot.Resize(0, 0); }
            }
        }

        // Patch to handle level up of custom perks. Without this MorePerks_CustomPerk is not keep on levelup
        [HarmonyPatch(typeof(PerkSystem), nameof(PerkSystem.DoLevelUpPerks))]
        public static class Patch_PerkSystem_DoLevelUpPerks
        {
            private static Dictionary<string, PerkParameter> savedCustomParam = new Dictionary<string, PerkParameter>();

            public static void Prefix(List<Perk> perksToLevelUp)
            {
                savedCustomParam.Clear();
                foreach(Perk perk in perksToLevelUp)
                {
                    if (perk.HasParameter("MorePerks_CustomPerk")) { savedCustomParam[perk.NextPerkId] = perk.Get("MorePerks_CustomPerk"); }
                }
            }

            public static void Postfix(Mercenary mercenary)
            {
                foreach (KeyValuePair<string, PerkParameter> kvp in savedCustomParam)
                {
                    foreach (Perk perk in mercenary.CreatureData.Perks)
                    {
                        if(kvp.Key == perk.PerkId) { perk.Parameters.Add(kvp.Value); }
                    }
                }
            }
        }
    }
}
