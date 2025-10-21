using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MGSC;
using UnityEngine;

namespace MorePerks
{
    public static class Plugin
    {

        public static string ModAssemblyName => Assembly.GetExecutingAssembly().GetName().Name;
        private static string ModPersistenceFolder => Path.Combine($"{Application.persistentDataPath}/../Quasimorph_ModConfigs", "Edair0_MorePerks");
        private static string ConfigPath => Path.Combine(ModPersistenceFolder, "config.txt");

        public static Logger Logger { get; private set; } = new Logger();

        public static ModConfigGeneral ConfigGeneral { get; set; }


        [Hook(ModHookType.AfterConfigsLoaded)]
        public static void AfterConfig(IModContext context)
        {

            ConfigGeneral = new ModConfigGeneral("QM More Perks", ConfigPath);
            Harmony harmony = new Harmony("Edair0_" + ModAssemblyName);

            ModLocalization.Initialize();
            new Harmony("Edair0_" + ModAssemblyName).PatchAll();
        }

    }
}
