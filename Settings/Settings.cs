namespace InitialNobles.Settings
{
    using InitialNobles.Patches;
    using System;
    using UnityEngine;

    public class InitialNoblesSlider : SliderSettingDefinition
    {
        public InitialNoblesSlider(OctDatGlobalInitializer initializer) : base(initializer)
        {
            this.id = "Oct.Settings.MySettings.StartingNobles";
            this.name = "Starting Nobles";
            this.order = Main.SettingsCategory.settings.Count;
            this.category = Main.SettingsCategory;
            this.min = 0f;
            this.max = 16f;
            this.step = 1;
            this.unit = " Nobles";
            this.defaultValue = 2f;
        }

        public override void Apply()
        {
            Patches.InitialNobles = (int)Math.Round((float)this.GetValue());
                        
            if (Patches.InitialNobles == 0)
            {
                Manager<SettingsManager>.Instance.SetGameBoolValue("Oct.Settings.Game.GuidedExperience", false);
            }

            base.Apply();
        }

        public override Color LabelColor()
        {
            return Color.magenta;
        }
    }
    
    public class RemoveKingdomSizePreferences : ToggleSettingDefinition
    {
        public RemoveKingdomSizePreferences(OctDatGlobalInitializer initializer) : base(initializer)
        {
            this.id = "Oct.Settings.MySettings.RemoveKingdomSizePreferences";
            this.name = "Kingdom Size Preferences";
            this.order = Main.SettingsCategory.settings.Count;
            this.category = Main.SettingsCategory;
            this.defaultValue = true;
        }

        public override void Apply()
        {
            Patches.RemoveKingomPrefs = !(bool)this.GetValue();
            if (Patches.RemoveKingomPrefs)
            {
                Manager<KingdomManager>.Instance.kingdoms.ForEach(k => k.members.ForEach(m => {
                    m.SetKingdomSizePreference(3);
                }));
            }

            base.Apply();
        }

        public override Color LabelColor()
        {
            return Color.magenta;
        }
    }

    public class DisableTutorialPopUp : ToggleSettingDefinition
    {
        public DisableTutorialPopUp(OctDatGlobalInitializer initializer) : base(initializer)
        {
            this.id = "Oct.Settings.MySettings.DisableTutorialPopUp";
            this.name = "Allow Tutorials";
            this.order = Main.SettingsCategory.settings.Count;
            this.category = Main.SettingsCategory;
            this.defaultValue = true;
        }

        public override void Apply()
        {
            Patches.DisableTutorials = !(bool)this.GetValue();
            base.Apply();
        }

        public override Color LabelColor()
        {
            return Color.magenta;
        }
    }

    public class SkipIntroDialog : ToggleSettingDefinition
    {
        public SkipIntroDialog(OctDatGlobalInitializer initializer) : base(initializer)
        {
            this.id = "Oct.Settings.MySettings.SkipIntroDialog";
            this.name = "Fast Intro";
            this.order = Main.SettingsCategory.settings.Count;
            this.category = Main.SettingsCategory;
            this.defaultValue = false;
        }

        public override void Apply()
        {
            Patches.SkipIntroDialog = (bool)this.GetValue();
            base.Apply();
        }

        public override Color LabelColor()
        {
            return Color.magenta;
        }
    }
}
