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
        public const string PerkAmount = "Perk_Amount";
        public const string StartPerks = "Start_Perks";
        public const string FreeMutation = "Free_Mutation";
        public const string Perk1 = "Perk_1";
        public const string Perk2 = "Perk_2";
        public const string Perk3 = "Perk_3";
        public const string Perk4 = "Perk_4";
        public const string Perk5 = "Perk_5";

        public const string About = "about";
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

            LocalizationHelper.AddKeyToAllDictionaries("perk.moreperks_random.name", "Random Perk");
            AllowedPerks = new List<string>()
            {
                "moreperks_random"
            };
            AllowedPerks.AddRange(Data.Perks._records.Keys.Where(key => key.StartsWith("talent_") || key.EndsWith("_basic")).ToList());

            this.PerkList = new List<object>();

            foreach (string perkID in AllowedPerks)
            {
                ParseHelper.GetGradeByPerkId(perkID, out _, out string text, out _);
                this.PerkList.Add($"{PerkList.Count + 1}. {Localization.Get($"perk.{text}.name")}");
            }

            // ======================================== Character Settings ========================================
            this.ModData.AddConfigHeader("STRING:General Settings", Keys.HeaderGeneral);

            this.ModData.AddConfigValue(Keys.HeaderGeneral, Keys.PerkAmount, defaultValue: 1, min: 0, max: 5,
                labelKey: "STRING:Additional Perks",
                tooltipKey: "STRING:Allows adding between 0 and 5 new perk slots.\n\n" +
                "0 - Disables additional perks\n" +
                "1 - Enables 1 Perk\n" +
                "2 - Enables 2 Perks\n" +
                "3 - Enables 3 Perks\n" +
                "4 - Enables 4 Perks\n" +
                "5 - Enables 5 Perks\n\n" +
                "Perks will apply on the next class change.\n" +
                "Removing this mod while having a character who has more than the default amount of perks may cause bugs in the stat display window." +
                "To safely remove this mod set this to 0 and then change class of every merc that had additional perks.");

            this.ModData.AddConfigValue(Keys.HeaderGeneral, Keys.StartPerks, defaultValue: true,
                labelKey: "STRING:Clones Start With Perks",
                tooltipKey: "STRING:This option controls whether, when setting the class for the first time, additional perks are applied.\n\n" +
                "On - Gain additional perks when setting the class for the first time\n" +
                "Off - Mutating is required to gain additional perks");

            this.ModData.AddConfigValue(Keys.HeaderGeneral, Keys.FreeMutation, defaultValue: false,
                labelKey: "STRING:Free Mutation",
                tooltipKey: "STRING:This option removes the 1 class chip cost of mutating and makes it free.\n\n" +
                "On - Mutating perks costs nothing\n" +
                "Off - Mutating perks costs 1 class chip (10 uses)");

            this.ModData.AddConfigValue(Keys.HeaderGeneral, Keys.Perk1, 
                defaultValue: "1. Random Perk",
                valueList: this.PerkList,
                labelKey: "STRING:Perk 1",
                tooltipKey: "STRING:Allows specifying the perk that will be added to the clone when selecting a class.");

            this.ModData.AddConfigValue(Keys.HeaderGeneral, Keys.Perk2,
                defaultValue: "1. Random Perk",
                valueList: this.PerkList,
                labelKey: "STRING:Perk 2",
                tooltipKey: "STRING:Allows specifying the perk that will be added to the clone when selecting a class.");

            this.ModData.AddConfigValue(Keys.HeaderGeneral, Keys.Perk3, 
                defaultValue: "1. Random Perk",
                valueList: this.PerkList,
                labelKey: "STRING:Perk 3",
                tooltipKey: "STRING:Allows specifying the perk that will be added to the clone when selecting a class.");

            this.ModData.AddConfigValue(Keys.HeaderGeneral, Keys.Perk4,
                defaultValue: "1. Random Perk",
                valueList: this.PerkList,
                labelKey: "STRING:Perk 4",
                tooltipKey: "STRING:Allows specifying the perk that will be added to the clone when selecting a class.");

            this.ModData.AddConfigValue(Keys.HeaderGeneral, Keys.Perk5,
                defaultValue: "1. Random Perk",
                valueList: this.PerkList,
                labelKey: "STRING:Perk 5",
                tooltipKey: "STRING:Allows specifying the perk that will be added to the clone when selecting a class.");

            this.ModData.AddConfigValue(Keys.HeaderGeneral, Keys.About,
                stringKey: $"STRING:\nThis mod adds up to 5 additional perk slots.\n\n" +
                "Additional perks are generated when a class is assigned to a clone for the first time.\n\n" +
                "Additional perks persist across class changes.\n\n" +
                "Additional perks can be rerolled only by 'mutating' the clone on the class select screen (or dying).\n\n" +
                "Mutation Chamber costs 1 Class Chip to charge, increasing charges by 10.\n\n" +
                "Class Chip is chosen randomly from the ship's cargo.\n\n" +
                "Perks set to anything other than random are guaranteed to generate when choosing a class and mutating perks.\n\n");

            this.ModData.RegisterModConfigData(ModName);
		}
    }

}
