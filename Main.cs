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
                    name = "MrPurple's Mod Settings",
                    canReset = true,
                    global = true,
                    order = -1000
                };
                SettingsCategory.SetId("MrPurple6411_Settings");
                SettingsCategory.PostInit();
            }


            var infoSettingDefinition = new InformativeSettingDefinition(default);
            infoSettingDefinition.SetId("Oct.Settings.MySettings.Initial_Nobles_Section");
            infoSettingDefinition.name = "Initial Nobles Settings:";
            infoSettingDefinition.category = SettingsCategory;
            infoSettingDefinition.order = SettingsCategory.settings.Count;

            SettingsCategory.AddSetting(infoSettingDefinition);
            SettingsCategory.AddSetting(new DisableTutorialPopUp(new OctDatGlobalInitializer()));
            SettingsCategory.AddSetting(new SkipIntroDialog(new OctDatGlobalInitializer()));
            SettingsCategory.AddSetting(new RemoveKingdomSizePreferences(new OctDatGlobalInitializer()));
            SettingsCategory.AddSetting(new InitialNoblesSlider(new OctDatGlobalInitializer()));
        }
    }
}
