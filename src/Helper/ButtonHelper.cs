using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MorePerks.ModPatches;

namespace MorePerks.Helper
{
    public static class ButtonHelper
    {
        public static bool UpdateMutateButtonStatus(CommonButton buttonToClean)
        {
            bool canInteract = false;
            // If FreeMutation is enabled then we just allow button use
            if (Plugin.ConfigGeneral.ModData.GetConfigValue<bool>(Keys.FreeMutation))
            {
                canInteract = true;
                RerollPerksButton.ChangeLabel(ModLocalization.MutateButtonFree.Key);
            }

            // If we have some uses left then we also allow mutation
            else if (Plugin.Save.GetCurrentSlotValue<int>(SaveVars.MutateUsesLeft) > 0)
            {
                canInteract = true;
                RerollPerksButton.ChangeLabel(ModLocalization.MutateButtonUses[Plugin.Save.GetCurrentSlotValue<int>(SaveVars.MutateUsesLeft) - 1].Key);
            }

            // If we don't have then we check if there is chip in cargo
            else
            {
                List<ItemStorage> cargo = UI.Get<SpaceshipScreen>()._magnumCargo.ShipCargo;
                canInteract = cargo.Any(storage => storage != null && storage.ContainsItem("classUSB"));
                RerollPerksButton.ChangeLabel(ModLocalization.MutateButtonCharge.Key);
            }
            RerollPerksButton.gameObject.SetActive(true);
            RerollPerksButton.SetInteractable(canInteract);
            return canInteract;
        }

        public static void CleanClonedButton(CommonButton buttonToClean)
        {
            Transform hotkeyTransform = buttonToClean.gameObject.transform.Find("GameKeyPanel");
            if (hotkeyTransform != null)
            {
                List<GameObject> hotKeyIcons = new List<GameObject>();
                foreach (Transform child in hotkeyTransform)
                {
                    if (child.name.Contains("HotkeyIcon")) { hotKeyIcons.Add(child.gameObject); }
                }

                if(hotKeyIcons.Count > 1)
                {
                    for (int i = 1; i < hotKeyIcons.Count; i++)
                    {
                        // Destroy sometimes doesn't immediately destroy GameObject so we hide it
                        hotKeyIcons[i].transform.localScale = new Vector3(0f, 0f, 0f);
                        GameObject.Destroy(hotKeyIcons[i]);
                    }
                }
                GameKeyPanel panel = hotkeyTransform.GetComponent<GameKeyPanel>();
                panel._hotkeyIcons.Clear();
                panel._hotkeyIcons.Add(hotKeyIcons[0].GetComponent<HotkeyIcon>());
            }
        }

    }
}
