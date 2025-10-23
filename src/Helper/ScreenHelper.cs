using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MorePerks.Helper
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
}
