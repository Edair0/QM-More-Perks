using MGSC;
using ModConfigMenu;
using ModConfigMenu.Objects;
using ModConfigMenu.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MorePerks
{
    public static partial class Keys
    {
        public const string HeaderGeneral = "general";
        public const string RandomPerks = "Random_Perks";
        public const string PerkAmount = "Perk_Amount";
        public const string FirstPerk = "First_Perk";
        public const string SecondPerk = "Second_Perk";
        public const string ThirdPerk = "Third_Perk";
    }


    public class ModConfigGeneral
    {
        private string ModName;
		public ModConfigData ModData;
        public List<string> AllowedPerks;
        public List<object> PerkList;

        public ModConfigGeneral(string ModName, string ConfigPath)
        {
            this.ModName = ModName;
			this.ModData = new ModConfigData(ConfigPath);

            AllowedPerks = Data.Perks._records.Keys.Where(key => key.StartsWith("talent_") || key.EndsWith("_basic")).ToList();

            this.PerkList = new List<object>()
            {
                "1. Random Perk"
            };

            foreach (string perkID in AllowedPerks)
            {
                ParseHelper.GetGradeByPerkId(perkID, out _, out string text, out _);
                Localization.Get($"perk.{text}.name");
                this.PerkList.Add($"{PerkList.Count + 1}. {Localization.Get($"perk.{text}.name")}");
            }

            // ======================================== Character Settings ========================================
            this.ModData.AddConfigHeader("STRING:General Settings", Keys.HeaderGeneral);


            this.ModData.AddConfigValue(Keys.HeaderGeneral, Keys.PerkAmount, defaultValue: 0, min: 0, max: 3,
                labelKey: "STRING:Additional Perks",
                tooltipKey: "STRING:Allows adding between 0 and 3 new perk slots.\n\n" +
                "0 - Disables additional perks\n" +
                "1 - Enables first perk\n" +
                "2 - Enables first and second perks\n" +
                "3 - Enables first, second and third perks\n\n" +
                "Perks will apply on the next class change.\n" +
                "Removing this mod while having a character who has more than the default amount of perks may cause bugs in the stat display window.");

            this.ModData.AddConfigValue(Keys.HeaderGeneral, Keys.FirstPerk, 
                defaultValue: "1. Random Perk",
                valueList: this.PerkList,
                labelKey: "STRING:First Perk",
                tooltipKey: "STRING:Allows specifying the perk that will be added to the clone when selecting a class.");

            this.ModData.AddConfigValue(Keys.HeaderGeneral, Keys.SecondPerk,
                defaultValue: "1. Random Perk",
                valueList: this.PerkList,
                labelKey: "STRING:Second Perk",
                tooltipKey: "STRING:Allows specifying the perk that will be added to the clone when selecting a class.");

            this.ModData.AddConfigValue(Keys.HeaderGeneral, Keys.ThirdPerk, 
                defaultValue: "1. Random Perk",
                valueList: this.PerkList,
                labelKey: "STRING:Third Perk",
                tooltipKey: "STRING:Allows specifying the perk that will be added to the clone when selecting a class.");

            this.ModData.RegisterModConfigData(this.ModName, "QM More Perks");
		}
    }

}
