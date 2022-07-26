using UnityEngine;
using TMPro;

public class UICharacterEditorCosmeticConfigurationWindowPopupContents : UIWindowContents
{
    private CharacterBehavior charBeh;
    private CharacterCosmeticInstance cosmeticInstance;

    private UICosmeticConfigurationColorSliders[] colorSliders;
    private UILabelBehavior cosmeticLabel;
    private UISliderBehavior scaleXSlider;
    private UISliderBehavior scaleYSlider;
    private UISliderBehavior scaleZSlider;
    private UISliderBehavior offsetXSlider;
    private UISliderBehavior offsetYSlider;
    private UISliderBehavior offsetZSlider;
    private UISliderBehavior rotationXSlider;
    private UISliderBehavior rotationYSlider;
    private UISliderBehavior rotationZSlider;

    private bool populating;

    private void SetColorRValue(float value)
    {
        if (UIElementBehavior.current is UISliderBehavior slider)
        {
            Vector3 cur = cosmeticInstance.colorLerps[slider.index];
            cur.x = value;
            cosmeticInstance.colorLerps[slider.index] = cur;
            charBeh.UpdateCosmetics();
        }
    }

    private void SetColorGValue(float value)
    {
        if (UIElementBehavior.current is UISliderBehavior slider)
        {
            Vector3 cur = cosmeticInstance.colorLerps[slider.index];
            cur.y = value;
            cosmeticInstance.colorLerps[slider.index] = cur;
            charBeh.UpdateCosmetics();
        }
    }

    private void SetColorBValue(float value)
    {
        if (UIElementBehavior.current is UISliderBehavior slider)
        {
            Vector3 cur = cosmeticInstance.colorLerps[slider.index];
            cur.z = value;
            cosmeticInstance.colorLerps[slider.index] = cur;
            charBeh.UpdateCosmetics();
        }
    }

    private void SetScaleValue(float value)
    {
        cosmeticInstance.scaleLerp.x = scaleXSlider.GetValue();
        cosmeticInstance.scaleLerp.y = scaleYSlider.GetValue();
        cosmeticInstance.scaleLerp.z = scaleZSlider.GetValue();
        charBeh.UpdateCosmetics();
    }

    private void SetOffsetValue(float value)
    {
        cosmeticInstance.offsetLerp.x = offsetXSlider.GetValue();
        cosmeticInstance.offsetLerp.y = offsetYSlider.GetValue();
        cosmeticInstance.offsetLerp.z = offsetZSlider.GetValue();
        charBeh.UpdateCosmetics();
    }

    private void SetRotationValue(float value)
    {
        cosmeticInstance.rotationLerp.x = rotationXSlider.GetValue();
        cosmeticInstance.rotationLerp.y = rotationYSlider.GetValue();
        cosmeticInstance.rotationLerp.z = rotationZSlider.GetValue();
        charBeh.UpdateCosmetics();
    }

    public override void Start()
    {
        base.Start();

        cosmeticLabel = (UILabelBehavior)UIManager.Instance.AddLabel(window, cosmeticInstance.cosmetic.name).SetFontColor(ManagerBehavior.Instance.titleTextColor).SetFontSize(24).SetAlignment(TextAlignmentOptions.Center).SizeHeightToContent();

        if (cosmeticInstance.cosmetic.instanceColors.Count > 0)
        {
            colorSliders = new UICosmeticConfigurationColorSliders[cosmeticInstance.cosmetic.instanceColors.Count];
            for (int i = 0; i < cosmeticInstance.cosmetic.instanceColors.Count; ++i)
            {
                if (cosmeticInstance.cosmetic.instanceColors[i].inheritHair >= 1f)
                {
                    continue;
                }
                if (cosmeticInstance.cosmetic.instanceColors[i].inheritSkin >= 1f)
                {
                    continue;
                }

                colorSliders[i] = new UICosmeticConfigurationColorSliders();

                string name = "Color " + (i + 1);
                if (cosmeticInstance.cosmetic.instanceColors[i].materials.Count > 0)
                {
                    name = cosmeticInstance.cosmetic.instanceColors[i].materials[0];
                }

                UIManager.Instance.AddLabel(window, name);
                colorSliders[i].colorRSlider = (UISliderBehavior)UIManager.Instance.AddSlider(window).SetValue(cosmeticInstance.colorLerps[i].x).AddValueChangedHandler(SetColorRValue).SetIndex(i);
                if (!cosmeticInstance.cosmetic.instanceColors[i].linearColor)
                {
                    colorSliders[i].colorGSlider = (UISliderBehavior)UIManager.Instance.AddSlider(window).SetValue(cosmeticInstance.colorLerps[i].y).AddValueChangedHandler(SetColorGValue).SetIndex(i);
                    colorSliders[i].colorBSlider = (UISliderBehavior)UIManager.Instance.AddSlider(window).SetValue(cosmeticInstance.colorLerps[i].z).AddValueChangedHandler(SetColorBValue).SetIndex(i);
                }
            }
        }

        if (cosmeticInstance.cosmetic.scaleRange != Vector3.zero)
        {
            UIManager.Instance.AddLabel(window, "Scale");
            scaleXSlider = UIManager.Instance.AddSlider(window).SetValue(cosmeticInstance.scaleLerp.x).AddValueChangedHandler(SetScaleValue);
            scaleYSlider = UIManager.Instance.AddSlider(window).SetValue(cosmeticInstance.scaleLerp.y).AddValueChangedHandler(SetScaleValue);
            scaleZSlider = UIManager.Instance.AddSlider(window).SetValue(cosmeticInstance.scaleLerp.z).AddValueChangedHandler(SetScaleValue);
        }

        if (cosmeticInstance.cosmetic.offsetRange != Vector3.zero)
        {
            UIManager.Instance.AddLabel(window, "Offset");
            offsetXSlider = UIManager.Instance.AddSlider(window).SetValue(cosmeticInstance.offsetLerp.x).AddValueChangedHandler(SetOffsetValue);
            offsetYSlider = UIManager.Instance.AddSlider(window).SetValue(cosmeticInstance.offsetLerp.y).AddValueChangedHandler(SetOffsetValue);
            offsetZSlider = UIManager.Instance.AddSlider(window).SetValue(cosmeticInstance.offsetLerp.z).AddValueChangedHandler(SetOffsetValue);
        }

        if (cosmeticInstance.cosmetic.rotationRange != Rotator.Zero)
        {
            UIManager.Instance.AddLabel(window, "Rotation");
            rotationXSlider = UIManager.Instance.AddSlider(window).SetValue(cosmeticInstance.rotationLerp.x).AddValueChangedHandler(SetRotationValue);
            rotationYSlider = UIManager.Instance.AddSlider(window).SetValue(cosmeticInstance.rotationLerp.y).AddValueChangedHandler(SetRotationValue);
            rotationZSlider = UIManager.Instance.AddSlider(window).SetValue(cosmeticInstance.rotationLerp.z).AddValueChangedHandler(SetRotationValue);
        }
    }

    public void SetCosmetic(CharacterBehavior charBeh, CharacterCosmeticInstance cosmeticInstance)
    {
        this.charBeh = charBeh;
        this.cosmeticInstance = cosmeticInstance;
    }
}
