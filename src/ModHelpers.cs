using MGSC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MorePerks.ModPatches;

namespace MorePerks
{
    public static class ScreenHelper
    {
        public static List<Vector3> PerkSlotPosition = new List<Vector3>()
        {
            new Vector3(-46f, 0f, 0f),
            new Vector3(-66f, -13f, 0f),
            new Vector3(-26f, -13f, 0f),
            new Vector3(-86f, -13f, 0f),
            new Vector3(-6f, -13f, 0f)
        };
        public static Vector3 HiddenPosition = new Vector3(-2000f, 0f, 0f);

        // Instantiates new slots and adds them to array
        public static PerkSlot[] IncreaseSlots(PerkSlot[] currentSlots, Transform parent)
        {
            List<PerkSlot> newSlots = currentSlots.ToList();
            for (int i = 0; i < 5; i++)
            {
                PerkSlot newPerkSlot = UnityEngine.Object.Instantiate(currentSlots[0], parent);
                newPerkSlot.transform.localScale = new Vector3(0.75f, 0.75f, 1f);          
                newSlots.Add(newPerkSlot);
            }
            return newSlots.ToArray();
        }

        // Used to position the slots and hides unnecessary ones
        public static void RefreshSlots(PerkSlot[] perkSlots)
        {
            int count = 0;
            foreach (PerkSlot slot in perkSlots)
            {
                // original slots cannot have both perk and merc null, this means that it's additional slot and class is yet to be selected on UI
                if (slot._perk == null && slot._mercenary == null) { HideSlot(slot); continue; } 

                if (slot._perk != null && slot._perk.HasParameter("MorePerks_CustomPerk"))
                {
                    slot.transform.localPosition = PerkSlotPosition[Mathf.Min(count, PerkSlotPosition.Count)];
                    count++;
                }
            }
        }

        public static void HideCustomSlots(PerkSlot[] perkSlots)
        {
            foreach (PerkSlot slot in perkSlots)
            {
                if (slot._perkRecord == null) { HideSlot(slot); }
                else if (slot._perk != null && slot._perk.HasParameter("MorePerks_CustomPerk"))
                {
                    HideSlot(slot);
                }
            }
        }

        public static void HideSlot(PerkSlot slot) { slot.transform.localPosition = HiddenPosition; }
    }

    public static class PerkHelper
    {
        public static Perk GetRandomCustomPerk(PerkFactory perkFactory, HashSet<string> ExistingPerks, bool EnsureForcedPerks)
        {
            return CreateCustomPerk(perkFactory, GetRandomPerkID(ExistingPerks, EnsureForcedPerks));
        }

        public static Perk CreateCustomPerk(PerkFactory perkFactory, string perkID)
        {
            Perk perk = perkFactory.CreatePerk(Data.Perks.GetRecord(perkID, true));
            if (perk != null && perk.PerkType == PerkType.Talent) { perk.PerkType = PerkType.Passive; } // If perk is a talent then PerkType needs to be passive otherwise it is skipped in some important places, and I don't want to patch all that stuff
            PerkParameter param = new PerkParameter { Name = "MorePerks_CustomPerk" };
            perk.Parameters.Add(param);
            return perk;
        }

        public static string GetRandomPerkID(HashSet<string> ExistingPerks, bool EnsureForcedPerks)
        {
            // We check if we want perks in option screen forced
            if (EnsureForcedPerks)
            {
                // List of perks to force excluding ones with moreperks_random ID
                List<string> PerksToForce = new List<string>()
                {
                    Plugin.ConfigGeneral.AllowedPerks[Plugin.ConfigGeneral.ModData.GetDropdownValue<int>(Keys.Perk1)],
                    Plugin.ConfigGeneral.AllowedPerks[Plugin.ConfigGeneral.ModData.GetDropdownValue<int>(Keys.Perk2)],
                    Plugin.ConfigGeneral.AllowedPerks[Plugin.ConfigGeneral.ModData.GetDropdownValue<int>(Keys.Perk3)],
                    Plugin.ConfigGeneral.AllowedPerks[Plugin.ConfigGeneral.ModData.GetDropdownValue<int>(Keys.Perk4)],
                    Plugin.ConfigGeneral.AllowedPerks[Plugin.ConfigGeneral.ModData.GetDropdownValue<int>(Keys.Perk5)]
                }.Where(perkID => perkID != "moreperks_random").ToList();

                // Quick check to see if perks from options of any rank are already added, if not we just return random perk ID.
                foreach (string perkID in PerksToForce)
                {
                    if (!ContainsAnyRank(ExistingPerks, perkID))
                    {
                        ExistingPerks.Add(perkID);
                        return perkID;
                    }
                }
            }
            // If we are here then EnsureForcedPerks didn't add any perk

            // Grab random perk and hope that there won't be a duplicate 100 times in a row (it won't)
            for (int i = 0; i < 100; i++)
            {
                string randomPerkID = Plugin.ConfigGeneral.AllowedPerks[UnityEngine.Random.Range(1, Plugin.ConfigGeneral.AllowedPerks.Count)]; // first perk is not a real perk so we start from 1
                if (!ContainsAnyRank(ExistingPerks, randomPerkID))
                {
                    ExistingPerks.Add(randomPerkID);
                    return randomPerkID;
                }
                if (i == 99) { Plugin.Logger.LogWarning("Could not find a unique perk to add after 100 attempts. That's weird"); }
            }
            return "ERROR";
        }

        public static void RefreshMerc(Mercenary mercenary)
        {
            PerkSystem.RefreshPerkPassives(mercenary.CreatureData);
            mercenary.CreatureData.Health.Restore(10000, false);
            foreach (ItemStorage itemStorage in mercenary.CreatureData.Inventory.Slots)
            {
                ItemInteractionSystem.FixItemDurability(itemStorage);
            }
            StarvationEffect starvation = mercenary.CreatureData.Starvation;
            starvation.Reinitialize(starvation.MaxLevel);
        }

        public static void MutatePerks(Mercenary merc, List<Perk> currentPerks, PerkFactory perkFactory)
        {
            // Some ugly LINQ stuff to gather neccessary data for perk replacement, we need to know where are located our custom perks to replace them
            List<int> customPerkIndices = currentPerks.Select((perk, index) => new { perk, index }).Where(x => x.perk.HasParameter("MorePerks_CustomPerk")).Select(x => x.index).ToList();
            HashSet<string> ExisitngPerks = new HashSet<string>(currentPerks.Where((perk, index) => !customPerkIndices.Contains(index)).Select(perk => perk.PerkId));

            int maxCustomPerks = Plugin.ConfigGeneral.ModData.GetConfigValue<int>(Keys.PerkAmount);
            int excessCount = customPerkIndices.Count - maxCustomPerks;

            if (excessCount > 0)
            {
                // We remove custom perks starting from the end
                foreach (int indexToRemove in customPerkIndices.OrderByDescending(i => i).Take(excessCount))
                {
                    currentPerks.RemoveAt(indexToRemove);
                }
                // update to the customPerkIndices as we removed some perks
                customPerkIndices = currentPerks
                    .Select((perk, index) => new { perk, index })
                    .Where(x => x.perk.HasParameter("MorePerks_CustomPerk"))
                    .Select(x => x.index)
                    .ToList();
            }

            // We loop to reroll our already existing perks
            foreach (int i in customPerkIndices)
            {
                currentPerks[i] = PerkHelper.GetRandomCustomPerk(perkFactory, ExisitngPerks, true);
            }

            // How much perks we need to add to meet mod options
            int perksToAdd = maxCustomPerks - customPerkIndices.Count;

            // we add new perks if necessary
            for (int i = 0; i < perksToAdd; i++)
            {
                currentPerks.Add(PerkHelper.GetRandomCustomPerk(perkFactory, ExisitngPerks, true));
            }

            // If character used to have extra slot talent but doens't have it anymore then its need to be removed manually
            if (!ExisitngPerks.Contains("talent_weapon_slot")) { merc.CreatureData.Inventory.AdditionalSlot.Resize(0, 0); }
        }

        public static HashSet<string> GetAllPerkRanks(string perkID)
        {
            // If it's talent then just return it
            if (perkID.StartsWith("talent_")) { return new HashSet<string> { perkID }; }

            // If it's normal perk then we just create all variants to a list
            string basePerk = perkID.Replace("_basic", "");
            return new HashSet<string>
            {
                basePerk + "_basic",
                basePerk + "_advanced",
                basePerk + "_master",
                basePerk + "_legend"
            };
        }

        public static bool ContainsAnyRank(HashSet<string> existingPerks, string perkID)
        {
            return GetAllPerkRanks(perkID).Any(rankID => existingPerks.Contains(rankID));
        }
    }
}
