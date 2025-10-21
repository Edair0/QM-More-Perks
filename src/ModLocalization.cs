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
        public static LocalizationEntry MutateButton = new LocalizationEntry("moreperks.mutate.button", "Mutate Perks");
        public static LocalizationEntry MutateDialog = new LocalizationEntry("moreperks.mutate.dialog", "Use 1 Class Chip to activate mutation protocol for all additional perks?");
        public static LocalizationEntry MutateConfirm = new LocalizationEntry("moreperks.mutate.dialog.confirm", "Mutate");
        public static LocalizationEntry MutateReturn = new LocalizationEntry("moreperks.mutate.dialog.return", "Return");

        public static void Initialize()
        {
            LocalizationHelper.AddKeyToAllDictionaries(MutateButton.Key, MutateButton.Data);
            LocalizationHelper.AddKeyToAllDictionaries(MutateDialog.Key, MutateDialog.Data);
            LocalizationHelper.AddKeyToAllDictionaries(MutateConfirm.Key, MutateConfirm.Data);
            LocalizationHelper.AddKeyToAllDictionaries(MutateReturn.Key, MutateReturn.Data);
        }
    }
}
