namespace InitialNobles
{
    using HarmonyLib;
    using InitialNobles.Settings;
    using UnityEngine;

    public class Main : BaseManager
    {
        internal static readonly SettingsCategory SettingsCategory;

        static Main()
        {
            ManagerBehavior.SkipMainMenu = true;
            Debug.Log($"[InitialNobles] Loading");
            new Harmony("MrPurple6411.InitialNobles").PatchAll();
            Debug.Log($"[InitialNobles] Patched Successfully");

            SettingsCategory = Manager<SettingsManager>.Instance.globalSettingsCategories.Find(x=> x.id == "MrPurple6411_Settings");
            if (SettingsCategory == null)
            {
                SettingsCategory = new SettingsCategory(new OctDatGlobalInitializer())
                {
                    id = "MrPurple6411_Settings",
                    name = "MrPurple's Mod Settings",
                    canReset = true,
                    global = true,
                    order = -1000
                };
                SettingsCategory.PostInit();
            }

            SettingsCategory.AddSetting(new InformativeSettingDefinition(default)
            {
                id = "Oct.Settings.MySettings.Initial_Nobles_Section",
                name = "Initial Nobles Settings:",
                category = SettingsCategory,
                order = SettingsCategory.settings.Count,
            });
            SettingsCategory.AddSetting(new DisableTutorialPopUp(new OctDatGlobalInitializer()));
            SettingsCategory.AddSetting(new SkipIntroDialog(new OctDatGlobalInitializer()));
            SettingsCategory.AddSetting(new RemoveKingdomSizePreferences(new OctDatGlobalInitializer()));
            SettingsCategory.AddSetting(new InitialNoblesSlider(new OctDatGlobalInitializer()));
        }
    }
}
