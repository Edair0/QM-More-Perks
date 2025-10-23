using ModConfigMenu.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorePerks
{
    public class LocalizationEntry
    {
        public string Key { get; set; }
        public string Data { get; set; }
        public LocalizationEntry(string key, string data)
        {
            Key = key;
            Data = data;
        }
    }

    public static class ModLocalization
    {
        public static LocalizationEntry MutateButtonFree = new LocalizationEntry("moreperks.mutate.button.free", "Mutate Perks");
        public static LocalizationEntry MutateButtonCharge = new LocalizationEntry("moreperks.mutate.button.charge", "<size=5.5>Charge Mutation Chamber</size>");
        public static List<LocalizationEntry> MutateButtonUses = new List<LocalizationEntry>();

        public static LocalizationEntry MutateDialogUse = new LocalizationEntry("moreperks.mutate.dialog.use", "Use Mutation Chamber to modify perks?");
        public static LocalizationEntry MutateDialogCharge = new LocalizationEntry("moreperks.mutate.dialog.charge", "Use 1 Class Chip to charge Mutation Chamber?");
        public static LocalizationEntry MutateConfirm = new LocalizationEntry("moreperks.mutate.dialog.confirm", "Confirm");
        public static LocalizationEntry MutateReturn = new LocalizationEntry("moreperks.mutate.dialog.return", "Return");


        public static void Initialize()
        {
            LocalizationHelper.AddKeyToAllDictionaries(MutateButtonFree.Key, MutateButtonFree.Data);
            LocalizationHelper.AddKeyToAllDictionaries(MutateButtonCharge.Key, MutateButtonCharge.Data);
            for (int i = 1; i <= 10; i++)
            {
                LocalizationEntry localizationEntry = new LocalizationEntry($"moreperks.mutate.button.{i}", $"<size=8.2>Mutate Perks {i}/10</size>");
                MutateButtonUses.Add(localizationEntry);
                LocalizationHelper.AddKeyToAllDictionaries(localizationEntry.Key, localizationEntry.Data);
            }

            LocalizationHelper.AddKeyToAllDictionaries(MutateDialogUse.Key, MutateDialogUse.Data);
            LocalizationHelper.AddKeyToAllDictionaries(MutateDialogCharge.Key, MutateDialogCharge.Data);
            LocalizationHelper.AddKeyToAllDictionaries(MutateConfirm.Key, MutateConfirm.Data);
            LocalizationHelper.AddKeyToAllDictionaries(MutateReturn.Key, MutateReturn.Data);
        }
    }
}
