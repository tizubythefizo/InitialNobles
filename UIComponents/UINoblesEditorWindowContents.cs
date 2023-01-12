using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using System.Linq;
using InitialNobles;
using System.Reflection;
using System;

public class UINoblesEditorWindowContents : UIWindowContents
{
    internal UIImageBehavior preview;

    internal static RenderTexture renderTexture;
    internal static GameObject previewArena;
    internal static Camera previewCamera;

    internal UIToggleBehavior zoomToggle;

    internal UILayoutBehavior controls;

    internal UILabelBehavior skinColorLabel;
    internal UISliderBehavior[] skinColorSliders = new UISliderBehavior[3];
    internal UILabelBehavior hairColorLabel;
    internal UISliderBehavior[] hairColorSliders = new UISliderBehavior[3];
    internal UILabelBehavior eyeColorLabel;
    internal UISliderBehavior[] eyeColorSliders = new UISliderBehavior[3];
    internal UISliderBehavior weightSlider;
    internal UISliderBehavior bustSlider;
    internal UISliderBehavior[] irisOffsetSliders = new UISliderBehavior[3];
    internal UISliderBehavior irisSizeSlider;
    internal UISliderBehavior irisSaturationSlider;
    internal UISliderBehavior irisHighlightSlider;

    internal UIButtonBehavior charTypeButton;
    internal UIIconButtonBehavior charTypeLockButton;

    internal UIButtonBehavior charTypeVariantButton;
    internal UIIconButtonBehavior charTypeVariantLockButton;

    internal const int DisplayedElements = 10;
    internal UIVerticalScrollList settingElementScroll;
    internal UILayoutBehavior[] settingElementContainers = new UILayoutBehavior[DisplayedElements];

    internal List<UIElementCharacterTypeEditorSlot> elements = new List<UIElementCharacterTypeEditorSlot>();
    internal void AddElement(UIElementBehavior element)
    {
        UIElementCharacterTypeEditorSlot elementSlot = new UIElementCharacterTypeEditorSlot(element);
        elementSlot.Hide();
        elements.Add(elementSlot);
    }

    internal const int DisplayedSlots = 5;
    internal UIVerticalScrollList cosmeticScroll;
    internal UILayoutBehavior[] cosmeticSlotContainers = new UILayoutBehavior[DisplayedSlots];
    internal CharacterCosmeticSlot[] cosmeticSlots = new CharacterCosmeticSlot[DisplayedSlots];
    internal UILabelBehavior[] cosmeticLabels = new UILabelBehavior[DisplayedSlots];
    internal UIButtonBehavior[] cosmeticSelectionButtons = new UIButtonBehavior[DisplayedSlots];
    internal UIIconButtonBehavior[] cosmeticConfigureButtons = new UIIconButtonBehavior[DisplayedSlots];
    internal UIIconButtonBehavior[] cosmeticLockButtons = new UIIconButtonBehavior[DisplayedSlots];

    internal const int MaxBoneScales = 10;
    internal UILabelBehavior[] boneScaleLabels = new UILabelBehavior[MaxBoneScales];
    internal UISliderBehavior[] boneScaleX = new UISliderBehavior[MaxBoneScales];
    internal UISliderBehavior[] boneScaleY = new UISliderBehavior[MaxBoneScales];
    internal UISliderBehavior[] boneScaleZ = new UISliderBehavior[MaxBoneScales];
    internal Dictionary<string, Vector3> boneScales = new Dictionary<string, Vector3>();

    internal Dictionary<string, List<CharacterCosmeticBehavior>> slotCosmetics = new Dictionary<string, List<CharacterCosmeticBehavior>>();

    internal CharacterType lastCharType;
    internal CharacterTypeVariant lastCharTypeVariant;
    internal CharacterType charType;
    internal CharacterTypeVariant charTypeVariant;
    internal CharacterBehavior charBeh;
    internal Character character;
    internal CharacterType startCharType;
    internal CharacterTypeVariant startCharTypeVariant;

    internal List<CharacterCosmeticInstance> cosmeticInstances = new List<CharacterCosmeticInstance>();
    internal List<bool> cosmeticLocked = new List<bool>();

    internal bool rolling;

    internal bool charTypeLocked;
    internal bool charTypeVariantLocked;

    internal void SkinSliderChanged(float value)
    {
        UpdateSkinColor();
    }

    internal void UpdateSkinColor(bool randomise = false)
    {
        if (rolling)
        {
            return;
        }
        Color skinColor;
        if (this.charType.skinColors != null)
        {
            if (randomise)
            {
                skinColor = CharacterManager.RollColorBlocks(this.charType.skinColors);
                skinColorSliders[0].SetValue(skinColor.r);
                skinColorSliders[1].SetValue(skinColor.g);
                skinColorSliders[2].SetValue(skinColor.b);
            }
            else
            {
                skinColor = new Color(
                    skinColorSliders[0].GetValue(),
                    skinColorSliders[1].GetValue(),
                    skinColorSliders[2].GetValue()
                    );
            }
        }
        else if (charType.skinLinearColor)
        {
            skinColor = Color.Lerp(charType.skinColorFrom, charType.skinColorTo, skinColorSliders[0].GetValue());
        }
        else
        {
            skinColor = new Color(
                Mathf.Lerp(charType.skinColorFrom.r, charType.skinColorTo.r, skinColorSliders[0].GetValue()), 
                Mathf.Lerp(charType.skinColorFrom.g, charType.skinColorTo.g, skinColorSliders[1].GetValue()), 
                Mathf.Lerp(charType.skinColorFrom.b, charType.skinColorTo.b, skinColorSliders[2].GetValue())
                );
        }
        charBeh.SetSkinColor(skinColor);
    }

    internal void EyeColorChanged(float value)
    {
        UpdateEyeColor();
    }

    internal void UpdateEyeColor()
    {
        if (rolling)
        {
            return;
        }

        Color eyeColor = new Color(
            eyeColorSliders[0].GetValue(),
            eyeColorSliders[1].GetValue(),
            eyeColorSliders[2].GetValue()
            );
        charBeh.SetEyeColor(eyeColor);
    }

    internal void IrisOffsetChanged(float value)
    {
        UpdateIrisOffset();
    }

    internal void UpdateIrisOffset()
    {
        if (rolling)
        {
            return;
        }

        Vector3 irisOffset = new Vector3(
            Mathf.Lerp(charType.irisOffsetCenter.x - charType.irisOffsetRange.x, charType.irisOffsetCenter.x + charType.irisOffsetRange.x, irisOffsetSliders[0].GetValue()),
            Mathf.Lerp(charType.irisOffsetCenter.y - charType.irisOffsetRange.y, charType.irisOffsetCenter.y + charType.irisOffsetRange.y, irisOffsetSliders[1].GetValue()),
            Mathf.Lerp(charType.irisOffsetCenter.z - charType.irisOffsetRange.z, charType.irisOffsetCenter.z + charType.irisOffsetRange.z, irisOffsetSliders[2].GetValue()));

        charBeh.SetIrisOffset(irisOffset);
    }

    internal void DisplaySlot(int disp, int entry)
    {
        CharacterCosmeticSlot slot = charTypeVariant.cosmeticSlots[entry];
        cosmeticSlots[disp] = slot;
        cosmeticLabels[disp].SetText(slot.name);
        cosmeticSelectionButtons[disp].SetText(cosmeticInstances[entry].cosmetic.name).SetActive(true);
        cosmeticConfigureButtons[disp].SetActive(true);
        cosmeticLockButtons[disp].SetIconColor(cosmeticLocked[entry] ? ManagerBehavior.Instance.titleTextColor : Color.white).SetActive(true);
    }

    internal void DisplayBlankSlot(int disp)
    {
        cosmeticSlots[disp] = null;
        cosmeticLabels[disp].SetText("");
        cosmeticSelectionButtons[disp].SetActive(false);
        cosmeticConfigureButtons[disp].SetActive(false);
        cosmeticLockButtons[disp].SetActive(false);
    }

    internal bool FilterSlot(int entry)
    {
        CharacterCosmeticSlot slot = charTypeVariant.cosmeticSlots[entry];
        if (slot.name == "Body")
        {
            return true;
        }
        return false;
    }

    internal void UpdateWeight(float value)
    {
        if (rolling)
        {
            return;
        }

        charBeh.SetWeight(value);
    }

    internal void UpdateBust(float value)
    {
        if (rolling)
        {
            return;
        }

        charBeh.SetBust(.5f + value * .5f);
    }

    internal void UpdateBoneScale(float value)
    {
        if (rolling)
        {
            return;
        }

        UpdateBoneScale(UIElementBehavior.current.index);
    }

    internal void UpdateBoneScale(int index)
    {
        if (index < charType.boneScales.Count)
        {
            CharacterTypeBoneScale scale = charType.boneScales[index];
            boneScaleLabels[index].SetText(scale.boneName);
            boneScales[scale.boneName] = new Vector3(
                Mathf.Lerp(scale.center.x - scale.range.x, scale.center.x + scale.range.x, boneScaleX[index].GetValue()),
                Mathf.Lerp(scale.center.y - scale.range.y, scale.center.y + scale.range.y, boneScaleY[index].GetValue()),
                Mathf.Lerp(scale.center.z - scale.range.z, scale.center.z + scale.range.z, boneScaleZ[index].GetValue()));
        }

        charBeh.SetBoneScales(boneScales);
    }

    internal void UpdateIrisSize(float value)
    {
        if (rolling)
        {
            return;
        }

        charBeh.SetIrisSize(Mathf.Clamp(Mathf.Lerp(charType.irisSizeCenter - charType.irisSizeRange, charType.irisSizeCenter + charType.irisSizeRange, value), 0, 1));
    }

    internal void UpdateIrisSaturation(float value)
    {
        if (rolling)
        {
            return;
        }

        charBeh.SetIrisSaturation(value);
    }

    internal void UpdateIrisHighlight(float value)
    {
        if (rolling)
        {
            return;
        }

        charBeh.SetIrisHighlight(value);
    }

    internal void ClickedType()
    {
        UIWindowBehavior typePopup = UIManager.Instance.AddPopupWindow();
        UISelectionPopupWindowContents contents = typePopup.InstanceContents<UISelectionPopupWindowContents>();
        var characterTypes = Manager<CharacterManager>.Instance.GetPrivateField<List<CharacterType>>("types");

        if (characterTypes != null)
        { throw new NobleFatesBreakingChangeException("field 'types' no longer exists on CharacterManager or the field was set to null"); }

        foreach (CharacterType type in characterTypes.Where(t => t.genus == CharacterType.Genus.Humanoid))
        {
            contents.AddOption(type.name, type);
        }
        contents.AddCallback(TypeChosen);
    }

    internal void TypeChosen(string name, object obj)
    {
        Refresh(true, obj as CharacterType);
    }

    internal void ClickedTypeVariant()
    {
        UIWindowBehavior jobPopup = UIManager.Instance.AddPopupWindow();
        UISelectionPopupWindowContents contents = jobPopup.InstanceContents<UISelectionPopupWindowContents>();
        foreach (CharacterTypeVariant variant in charType.variants)
        {
            contents.AddOption(variant.name, variant);
        }
        contents.AddCallback(TypeVariantChosen);
    }

    internal void TypeVariantChosen(string name, object obj)
    {
        Refresh(false, charType, obj as CharacterTypeVariant);
    }

    internal void ClickedCosmetic(int index)
    {
        UIWindowBehavior jobPopup = UIManager.Instance.AddPopupWindow();
        UISelectionPopupWindowContents contents = jobPopup.InstanceContents<UISelectionPopupWindowContents>();
        CharacterCosmeticSlot slot = cosmeticSlots[index];
        foreach (var slotCosmetic in slot.cosmetics)
        {
            contents.AddOption(slotCosmetic.name, slotCosmetic);
        }
        void Chosen(string key, object obj)
        {
            CosmeticChosen(slot.name, obj as CharacterCosmetic);
        }
        contents.AddCallback(Chosen);
    }

    internal void CosmeticChosen(string slot, CharacterCosmetic cosmetic)
    {
        int instIndex = -1;
        for (int i = 0; i < cosmeticInstances.Count; ++i)
        {
            if (cosmeticInstances[i].slot == slot)
            {
                instIndex = i;
                break;
            }
        }

        if (instIndex == -1)
        {
            return;
        }

        CharacterCosmeticInstance instance = cosmeticInstances[instIndex];
        instance.cosmetic = cosmetic;

        Refresh(false);
    }

    internal void ClickedCosmeticConfiguration(int index)
    {
        CharacterCosmeticSlot slot = cosmeticSlots[index];

        int instIndex = -1;
        for (int i = 0; i < cosmeticInstances.Count; ++i)
        {
            if (cosmeticInstances[i].slot == slot.name)
            {
                instIndex = i;
                break;
            }
        }

        CharacterCosmeticInstance instance = cosmeticInstances[instIndex];
        UIWindowBehavior jobPopup = UIManager.Instance.AddPopupWindow();
        UICharacterEditorCosmeticConfigurationWindowPopupContents contents = jobPopup.InstanceContents<UICharacterEditorCosmeticConfigurationWindowPopupContents>();
        contents.SetCosmetic(charBeh, instance);
    }

    internal void HairColorChanged(float value)
    {
        UpdateHairColor();
    }

    internal void UpdateHairColor()
    {
        if (rolling)
        {
            return;
        }

        Color hairColor;
            hairColor = new Color( 
                hairColorSliders[0].GetValue(),
                hairColorSliders[1].GetValue(),
                hairColorSliders[2].GetValue());
        charBeh.SetHairColor(hairColor);
    }

    internal void ToggleCharTypeLock()
    {
        charTypeLocked = !charTypeLocked;
        charTypeLockButton.SetIconColor(charTypeLocked ? ManagerBehavior.Instance.titleTextColor : Color.white);
    }

    internal void ToggleCharTypeVariantLock()
    {
        charTypeVariantLocked = !charTypeVariantLocked;
        charTypeVariantLockButton.SetIconColor(charTypeVariantLocked ? ManagerBehavior.Instance.titleTextColor : Color.white);
    }

    internal void ToggleSlotLock(int index)
    {
        CharacterCosmeticSlot slot = cosmeticSlots[index];

        int instIndex = -1;
        for (int i = 0; i < cosmeticInstances.Count; ++i)
        {
            if (cosmeticInstances[i].slot == slot.name)
            {
                instIndex = i;
                break;
            }
        }

        if (instIndex == -1)
        {
            return;
        }

        cosmeticLocked[instIndex] = !cosmeticLocked[instIndex];
        cosmeticLockButtons[index].SetIconColor(cosmeticLocked[instIndex] ? ManagerBehavior.Instance.titleTextColor : Color.white);
    }

    internal void DisplayElement(int disp, int element)
    {
        UIElementCharacterTypeEditorSlot ele = elements[element];
        if (settingElementContainers[disp].obj is UIElementCharacterTypeEditorSlot existing)
        {
            if (existing != ele)
            {
                if (existing.element.transform.parent == settingElementContainers[disp].transform)
                {
                    existing.Hide();
                }
            }
            else
            {
                return;
            }
        }

        ele.Show(settingElementContainers[disp]);
        settingElementContainers[disp].SetObject(ele);
    }

    internal void DisplayBlankElement(int disp)
    {
        if (settingElementContainers[disp].obj is UIElementCharacterTypeEditorSlot existing)
        {
            existing.Hide();
            settingElementContainers[disp].SetObject(null);
        }
    }

    internal bool FilterElement(int element)
    {
        UIElementCharacterTypeEditorSlot ele = elements[element];
        if (ele.element == skinColorLabel || ele.element == skinColorSliders[0])
        {
            return charType.skinColors != null; ;
        }
        if (ele.element == skinColorSliders[1] || ele.element == skinColorSliders[2])
        {
            return charType.skinColors != null || charType.skinLinearColor;
        }

        if (ele.element == eyeColorLabel || ele.element == eyeColorSliders[0] || ele.element == eyeColorSliders[1] || ele.element == eyeColorSliders[2])
        {
            if (charTypeVariant.cosmeticSlots.FindIndex(c => c.name == "Eyes") == -1)
                return true;
        }
        if (ele.element == hairColorLabel || ele.element == hairColorSliders[0] || ele.element == hairColorSliders[1] || ele.element == hairColorSliders[2])
        {
            if(charTypeVariant.cosmeticSlots.FindIndex(c=>c.name == "Hair") == -1)
                return true;
        }

        if ((string)ele.element.obj == "BoneScale")
        {
            return ele.element.index >= charType.boneScales.Count;
        }

        return false;
    }

    internal void ToggleZoom(UIToggleBehavior.Value zoom)
    {
        if (zoom == UIToggleBehavior.Value.On)
        {
            previewCamera.transform.localPosition = new Vector3(0, .675f, -1f);
        }
        else
        {
            previewCamera.transform.localPosition = new Vector3(0, .475f, -1.5f);
        }
    }

    internal void Close()
    {
        if(character != null)
        {
            character.GetType().GetProperty("variant").SetValue(character, charTypeVariant);

            if (character.type != startCharType || character.variant != startCharTypeVariant)
            {
                character.cosmetics.Clear();
                character.SetName(character.variant.names[UnityEngine.Random.Range(0, character.variant.names.Count)]);
                bool flag4 = character.variant.materialSlots != null;
                if (flag4)
                {
                    
                    character.GetType().GetProperty("materials").SetValue(character, new List<CharacterMaterial>());
                    foreach (CharacterTypeMaterialSlot mat in character.variant.materialSlots)
                    {
                        CharacterMaterial material = mat.Roll();
                        bool flag5 = material != null;
                        if (flag5)
                        {
                            character.materials.Add(material);
                        }
                    }
                }
                bool flag6 = character.variant.lexicon != null;
                if (flag6)
                {
                    character.GetType().GetProperty("lexicon").SetValue(character, new CharacterLexicon(character.variant.lexicon));
                }
                character.GetType().GetProperty("gender").SetValue(character, character.variant.gender);
                bool flag7 = character.gender != "Male";
                if (flag7)
                {
                    character.GetType().GetProperty("bust").SetValue(character, UnityEngine.Random.value);
                }
                character.GetType().GetProperty("weight").SetValue(character, UnityEngine.Random.value);
                bool flag8 = character.variant.voices.Count > 0;
                if (flag8)
                {
                    character.GetType().GetProperty("voice").SetValue(character, character.variant.voices[UnityEngine.Random.Range(0, character.variant.voices.Count)]);
                }
                character.GetType().GetProperty("pitch").SetValue(character, UnityEngine.Random.Range(character.variant.pitchFrom, character.variant.pitchTo));

                character.pawn.RerollAttractions();
            }
            character.PasteAppearance(CopyAppearance());
        }
        GameObject.Destroy(window.gameObject);
    }

    internal void Refresh(bool randomize = true, CharacterType type = null, CharacterTypeVariant variant = null)
    {
        if (charBeh != null)
        {
            GameObject.Destroy(charBeh.gameObject);
            charBeh = null;
        }
        slotCosmetics.Clear();

        if (type != null)
        {
            charType = type;
        }
        else if (randomize && !charTypeLocked)
        {
            charType = CharacterManager.Instance.GetRandomType(CharacterType.Genus.Humanoid);
        }
        charTypeButton.SetText(charType.name);

        if (variant != null)
        {
            charTypeVariant = variant;
        }
        else if (randomize && !charTypeVariantLocked)
        {
            charTypeVariant = charType.variants[UnityEngine.Random.Range(0, charType.variants.Count)];
        }
        charTypeVariantButton.SetText(charTypeVariant.name);

        if (charTypeVariant != lastCharTypeVariant || lastCharType != charType)
        {
            cosmeticLocked.Clear();
            for (int i = 0; i < charTypeVariant.cosmeticSlots.Count; ++i)
            {
                cosmeticLocked.Add(false);
            }
        }

        charBeh = OctBehavior.InstantiatePrefab(charType.prefab);
        charBeh.transform.SetParent(previewArena.transform, false);

        charBeh.SetCharacterType(charType);

        // reset bounds to match archetype to avoid edge cases where loading a lying down character introduces bad bounds
        SkinnedMeshRenderer baseRenderer = charType.prefab.GetComponentInChildren<SkinnedMeshRenderer>();
        SkinnedMeshRenderer ourRenderer = charBeh.GetComponentInChildren<SkinnedMeshRenderer>();
        if (baseRenderer != null)
        {
            ourRenderer.localBounds = baseRenderer.localBounds;
        }

        if (randomize)
        {
            rolling = true;

            if (charType.skinColors == null)
            {
                if (charType.skinLinearColor)
                {
                    skinColorSliders[0].SetValue(UnityEngine.Random.value);
                }
                else
                {
                    skinColorSliders[0].SetValue(UnityEngine.Random.value);
                    skinColorSliders[1].SetValue(UnityEngine.Random.value);
                    skinColorSliders[2].SetValue(UnityEngine.Random.value);
                }
            }

            if (charType.hairColors == null)
            {
                hairColorSliders[0].SetValue(UnityEngine.Random.value);
                hairColorSliders[1].SetValue(UnityEngine.Random.value);
                hairColorSliders[2].SetValue(UnityEngine.Random.value);
            }

            bustSlider.SetValue(UnityEngine.Random.value);
            weightSlider.SetValue(Mathf.Pow(UnityEngine.Random.value, 2));

            irisOffsetSliders[0].SetValue(OctoberMath.DistributedRandom(0f, 1f, charType.irisOffsetExponent));
            irisOffsetSliders[1].SetValue(OctoberMath.DistributedRandom(0f, 1f, charType.irisOffsetExponent));
            irisOffsetSliders[2].SetValue(OctoberMath.DistributedRandom(0f, 1f, charType.irisOffsetExponent));
            irisSizeSlider.SetValue(OctoberMath.DistributedRandom(0f, 1f, charType.irisSizeExponent));
            irisSaturationSlider.SetValue(OctoberMath.DistributedRandom(0f, 1f, 2f));
            irisHighlightSlider.SetValue(OctoberMath.DistributedRandom(0f, 1f, 2f));

            for (int i = 0; i < charType.boneScales.Count; ++i)
            {
                boneScaleLabels[i].SetText(charType.boneScales[i].boneName);
                boneScaleX[i].SetValue(OctoberMath.DistributedRandom(0f, 1f, charType.boneScales[i].exponent));
                boneScaleY[i].SetValue(OctoberMath.DistributedRandom(0f, 1f, charType.boneScales[i].exponent));
                boneScaleZ[i].SetValue(OctoberMath.DistributedRandom(0f, 1f, charType.boneScales[i].exponent));
            }

            rolling = false;
        }

        UpdateSkinColor(randomize);
        UpdateHairColor();
        UpdateEyeColor();
        UpdateBust(bustSlider.GetValue());
        UpdateWeight(weightSlider.GetValue());
        UpdateIrisOffset();
        UpdateIrisSize(irisSizeSlider.GetValue());
        UpdateIrisSaturation(irisSaturationSlider.GetValue());
        UpdateIrisHighlight(irisHighlightSlider.GetValue());

        if (charType.boneScales.Count > 0)
        {
            boneScales.Clear();
            for (int i = 0; i < charType.boneScales.Count; ++i)
            {
                UpdateBoneScale(i);
            }
            charBeh.SetBoneScales(boneScales);
        }

        // instance our cosmetics
        int index;
        for (index = 0; index < charTypeVariant.cosmeticSlots.Count; ++index)
        {
            CharacterCosmeticSlot slot = charTypeVariant.cosmeticSlots[index];

            if (index >= cosmeticInstances.Count)
            {
                cosmeticInstances.Add(null);
            }

            if (lastCharTypeVariant != charTypeVariant || (randomize && !cosmeticLocked[index]))
            {
                cosmeticInstances[index] = new CharacterCosmeticInstance(null, slot.name, slot.RollCosmetic());
            }

            CharacterCosmeticInstance instance = cosmeticInstances[index];
            if (instance.cosmetic.prefab == null)
            {
                continue;
            }

            List<CharacterCosmeticBehavior> behaviors = new List<CharacterCosmeticBehavior>();
            instance.cosmetic.SpawnBehavior(instance, charBeh, behaviors);
            slotCosmetics.Add(slot.name, behaviors);
        }
        while (index < cosmeticInstances.Count)
        {
            cosmeticInstances.RemoveAt(index);
        }

        Animation anim = charBeh.GetComponent<Animation>();
        AnimationState state = anim["Armature|Bind"];
        if (state != null)
        {
            state.speed = 1;
            anim.Play("Armature|Bind");
            state.time = 0;
        }

        lastCharType = charType;
        lastCharTypeVariant = charTypeVariant;
        settingElementScroll.Refresh(elements.Count);
        cosmeticScroll.Refresh(cosmeticInstances.Count);
    }

    internal void Initialize(ref Pawn pawn)
    {
        character = pawn?.character;

        if(character == null)
            Close();

        startCharType = character.type;
        startCharTypeVariant = character.variant;
        PasteAppearance(character.CopyAppearance());
    }

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        if (previewArena == null)
        {
            renderTexture = new RenderTexture(1024, 1024, 16);

            previewArena = GameObject.Instantiate(ManagerBehavior.Instance.prefabs[(int)ManagerBehavior.Prefab.Editor]);
            previewArena.transform.position = new Vector3(-1000, -1000, -1000);
            previewCamera = previewArena.GetComponentInChildren<Camera>();
            previewCamera.targetTexture = renderTexture;
        }
        else
        {
            previewArena.gameObject.SetActive(true);
        }

        window.transform.localScale *= 1.5f;
        window.SetChildAlignment(TextAnchor.MiddleCenter);

        UIManager.Instance.AddLabel(window, "Noble Editor").SetFontStyle(FontStyles.Bold).SetFontSize(24).SetAlignment(TextAlignmentOptions.Center).SetFontColor(ManagerBehavior.Instance.titleTextColor).SetWidth(525).SetHeight(40);

        UILayoutBehavior horiz = UIManager.Instance.AddHorizontalGroup(window);
        horiz.SetPadding(new RectOffset(8, 8, 8, 8));
        horiz.SetSpacing(8);

        controls = UIManager.Instance.AddVerticalGroup(horiz);

        UILayoutBehavior previewVert = UIManager.Instance.AddVerticalGroup(horiz);
        preview = (UIImageBehavior)UIManager.Instance.AddImage(previewVert, renderTexture).SetWidth(512).SetHeight(512);

        UILayoutBehavior zoomHoriz = UIManager.Instance.AddHorizontalGroup(previewVert).SetChildAlignment(TextAnchor.MiddleLeft);
        UIManager.Instance.AddLabel(zoomHoriz, "Zoom:").SetWidth(75);
        zoomToggle = UIManager.Instance.AddToggle(zoomHoriz, ToggleZoom);

        UIManager.Instance.AddButton(controls, "Randomize", delegate { Refresh(true); });

        UIManager.Instance.AddLabel(controls, "Type").SetAlignment(TextAlignmentOptions.Center);
        UILayoutBehavior charTypeHoriz = UIManager.Instance.AddHorizontalGroup(controls);
        charTypeHoriz.SetChildAlignment(TextAnchor.MiddleCenter);
        charTypeButton = (UIButtonBehavior)UIManager.Instance.AddButton(charTypeHoriz, "", ClickedType).SetWidth(136);
        charTypeLockButton = (UIIconButtonBehavior)UIManager.Instance.AddIconButton(charTypeHoriz, ToggleCharTypeLock).SetIcon(ManagerBehavior.Instance.lockUISprite.texture).SetWidth(24).SetHeight(24);

        UIManager.Instance.AddLabel(controls, "Variant").SetAlignment(TextAlignmentOptions.Center);
        UILayoutBehavior charTypeVariantHoriz = UIManager.Instance.AddHorizontalGroup(controls);
        charTypeVariantHoriz.SetChildAlignment(TextAnchor.MiddleCenter);
        charTypeVariantButton = (UIButtonBehavior)UIManager.Instance.AddButton(charTypeVariantHoriz, "", ClickedTypeVariant).SetWidth(136);
        charTypeVariantLockButton = (UIIconButtonBehavior)UIManager.Instance.AddIconButton(charTypeVariantHoriz, ToggleCharTypeVariantLock).SetIcon(ManagerBehavior.Instance.lockUISprite.texture).SetWidth(24).SetHeight(24);

        settingElementScroll = UIManager.Instance.AddVerticalScrollList(controls, DisplayedElements, DisplayElement, DisplayBlankElement, FilterElement);
        for (int i = 0; i < DisplayedElements; ++i)
        {
            settingElementContainers[i] = (UILayoutBehavior)UIManager.Instance.AddHorizontalGroup(settingElementScroll).SetWidth(160).SetHeight(24);
        }

        AddElement(UIManager.Instance.AddLabel(controls, "Body").SetAlignment(TextAlignmentOptions.Center));
        weightSlider = UIManager.Instance.AddSlider(controls);
        weightSlider.SetMinValue(0);
        weightSlider.SetMaxValue(1);
        weightSlider.SetValue(0);
        weightSlider.AddValueChangedHandler(UpdateWeight);
        AddElement(weightSlider);

        bustSlider = UIManager.Instance.AddSlider(controls);
        bustSlider.SetMinValue(0);
        bustSlider.SetMaxValue(1);
        bustSlider.SetValue(0);
        bustSlider.AddValueChangedHandler(UpdateBust);
        AddElement(bustSlider);

        for (int i = 0; i < MaxBoneScales; ++i)
        {
            boneScaleLabels[i] = (UILabelBehavior)UIManager.Instance.AddLabel(controls, "Bone").SetAlignment(TextAlignmentOptions.Center).SetIndex(i).SetObject("BoneScale");
            AddElement(boneScaleLabels[i]);

            boneScaleX[i] = (UISliderBehavior)UIManager.Instance.AddSlider(controls).SetIndex(i).SetObject("BoneScale");
            boneScaleX[i].SetMinValue(0);
            boneScaleX[i].SetMaxValue(1);
            boneScaleX[i].SetValue(0);
            boneScaleX[i].AddValueChangedHandler(UpdateBoneScale);
            AddElement(boneScaleX[i]);

            boneScaleY[i] = (UISliderBehavior)UIManager.Instance.AddSlider(controls).SetIndex(i).SetObject("BoneScale");
            boneScaleY[i].SetMinValue(0);
            boneScaleY[i].SetMaxValue(1);
            boneScaleY[i].SetValue(0);
            boneScaleY[i].AddValueChangedHandler(UpdateBoneScale);
            AddElement(boneScaleY[i]);

            boneScaleZ[i] = (UISliderBehavior)UIManager.Instance.AddSlider(controls).SetIndex(i).SetObject("BoneScale");
            boneScaleZ[i].SetMinValue(0);
            boneScaleZ[i].SetMaxValue(1);
            boneScaleZ[i].SetValue(0);
            boneScaleZ[i].AddValueChangedHandler(UpdateBoneScale);
            AddElement(boneScaleZ[i]);
        }

        skinColorLabel = (UILabelBehavior)UIManager.Instance.AddLabel(controls, "Skin Color").SetAlignment(TextAlignmentOptions.Center);
        AddElement(skinColorLabel);
        skinColorSliders[0] = UIManager.Instance.AddSlider(controls);
        skinColorSliders[0].SetMinValue(0);
        skinColorSliders[0].SetMaxValue(1);
        skinColorSliders[0].SetValue(0);
        skinColorSliders[0].AddValueChangedHandler(SkinSliderChanged);
        AddElement(skinColorSliders[0]);
        skinColorSliders[1] = UIManager.Instance.AddSlider(controls);
        skinColorSliders[1].SetMinValue(0);
        skinColorSliders[1].SetMaxValue(1);
        skinColorSliders[1].SetValue(0);
        skinColorSliders[1].AddValueChangedHandler(SkinSliderChanged);
        AddElement(skinColorSliders[1]);
        skinColorSliders[2] = UIManager.Instance.AddSlider(controls);
        skinColorSliders[2].SetMinValue(0);
        skinColorSliders[2].SetMaxValue(1);
        skinColorSliders[2].SetValue(0);
        skinColorSliders[2].AddValueChangedHandler(SkinSliderChanged);
        AddElement(skinColorSliders[2]);

        hairColorLabel = (UILabelBehavior)UIManager.Instance.AddLabel(controls, "Hair Color").SetAlignment(TextAlignmentOptions.Center);
        AddElement(hairColorLabel);
        hairColorSliders[0] = UIManager.Instance.AddSlider(controls);
        hairColorSliders[0].SetMinValue(0);
        hairColorSliders[0].SetMaxValue(1);
        hairColorSliders[0].SetValue(0);
        hairColorSliders[0].AddValueChangedHandler(HairColorChanged);
        AddElement(hairColorSliders[0]);
        hairColorSliders[1] = UIManager.Instance.AddSlider(controls);
        hairColorSliders[1].SetMinValue(0);
        hairColorSliders[1].SetMaxValue(1);
        hairColorSliders[1].SetValue(0);
        AddElement(hairColorSliders[1]);
        hairColorSliders[1].AddValueChangedHandler(HairColorChanged);
        hairColorSliders[2] = UIManager.Instance.AddSlider(controls);
        hairColorSliders[2].SetMinValue(0);
        hairColorSliders[2].SetMaxValue(1);
        hairColorSliders[2].SetValue(0);
        hairColorSliders[2].AddValueChangedHandler(HairColorChanged);
        AddElement(hairColorSliders[2]);

        eyeColorLabel = (UILabelBehavior)UIManager.Instance.AddLabel(controls, "Eye Color").SetAlignment(TextAlignmentOptions.Center);
        AddElement(eyeColorLabel);
        eyeColorSliders[0] = UIManager.Instance.AddSlider(controls);
        eyeColorSliders[0].SetMinValue(0);
        eyeColorSliders[0].SetMaxValue(1);
        eyeColorSliders[0].SetValue(0);
        eyeColorSliders[0].AddValueChangedHandler(EyeColorChanged);
        AddElement(eyeColorSliders[0]);
        eyeColorSliders[1] = UIManager.Instance.AddSlider(controls);
        eyeColorSliders[1].SetMinValue(0);
        eyeColorSliders[1].SetMaxValue(1);
        eyeColorSliders[1].SetValue(0);
        eyeColorSliders[1].AddValueChangedHandler(EyeColorChanged);
        AddElement(eyeColorSliders[1]);
        eyeColorSliders[2] = UIManager.Instance.AddSlider(controls);
        eyeColorSliders[2].SetMinValue(0);
        eyeColorSliders[2].SetMaxValue(1);
        eyeColorSliders[2].SetValue(0);
        eyeColorSliders[2].AddValueChangedHandler(EyeColorChanged);
        AddElement(eyeColorSliders[2]);

        AddElement(UIManager.Instance.AddLabel(controls, "Iris Offset").SetAlignment(TextAlignmentOptions.Center));
        irisOffsetSliders[0] = UIManager.Instance.AddSlider(controls);
        irisOffsetSliders[0].SetMinValue(0);
        irisOffsetSliders[0].SetMaxValue(1);
        irisOffsetSliders[0].SetValue(0);
        irisOffsetSliders[0].AddValueChangedHandler(IrisOffsetChanged);
        AddElement(irisOffsetSliders[0]);
        irisOffsetSliders[1] = UIManager.Instance.AddSlider(controls);
        irisOffsetSliders[1].SetMinValue(0);
        irisOffsetSliders[1].SetMaxValue(1);
        irisOffsetSliders[1].SetValue(0);
        irisOffsetSliders[1].AddValueChangedHandler(IrisOffsetChanged);
        AddElement(irisOffsetSliders[1]);
        irisOffsetSliders[2] = UIManager.Instance.AddSlider(controls);
        irisOffsetSliders[2].SetMinValue(0);
        irisOffsetSliders[2].SetMaxValue(1);
        irisOffsetSliders[2].SetValue(0);
        irisOffsetSliders[2].AddValueChangedHandler(IrisOffsetChanged);
        AddElement(irisOffsetSliders[2]);

        AddElement(UIManager.Instance.AddLabel(controls, "Iris Size").SetAlignment(TextAlignmentOptions.Center));
        irisSizeSlider = UIManager.Instance.AddSlider(controls);
        irisSizeSlider.SetMinValue(0);
        irisSizeSlider.SetMaxValue(1);
        irisSizeSlider.SetValue(0);
        irisSizeSlider.AddValueChangedHandler(UpdateIrisSize);
        AddElement(irisSizeSlider);

        AddElement(UIManager.Instance.AddLabel(controls, "Iris Saturation").SetAlignment(TextAlignmentOptions.Center));
        irisSaturationSlider = UIManager.Instance.AddSlider(controls);
        irisSaturationSlider.SetMinValue(0);
        irisSaturationSlider.SetMaxValue(1);
        irisSaturationSlider.SetValue(0);
        irisSaturationSlider.AddValueChangedHandler(UpdateIrisSaturation);
        AddElement(irisSaturationSlider);

        AddElement(UIManager.Instance.AddLabel(controls, "Iris Highlight").SetAlignment(TextAlignmentOptions.Center));
        irisHighlightSlider = UIManager.Instance.AddSlider(controls);
        irisHighlightSlider.SetMinValue(0);
        irisHighlightSlider.SetMaxValue(1);
        irisHighlightSlider.SetValue(0);
        irisHighlightSlider.AddValueChangedHandler(UpdateIrisHighlight);
        AddElement(irisHighlightSlider);

        cosmeticScroll = UIManager.Instance.AddVerticalScrollList(controls, DisplayedSlots, DisplaySlot, DisplayBlankSlot, FilterSlot); 
        for (int i = 0; i < DisplayedSlots; ++i)
        {
            int index = i;
            cosmeticSlotContainers[i] = (UILayoutBehavior)UIManager.Instance.AddVerticalGroup(cosmeticScroll);
            cosmeticLabels[i] = (UILabelBehavior)UIManager.Instance.AddLabel(cosmeticSlotContainers[i], "").SetAlignment(TextAlignmentOptions.Center);
            UILayoutBehavior cosmeticSlotHoriz = UIManager.Instance.AddHorizontalGroup(cosmeticSlotContainers[i]).SetChildAlignment(TextAnchor.MiddleCenter);
            cosmeticSelectionButtons[i] = (UIButtonBehavior)UIManager.Instance.AddButton(cosmeticSlotHoriz, "", delegate { ClickedCosmetic(index); }).SetWidth(112);
            cosmeticConfigureButtons[i] = (UIIconButtonBehavior)UIManager.Instance.AddIconButton(cosmeticSlotHoriz, delegate { ClickedCosmeticConfiguration(index); }).SetIcon(ManagerBehavior.Instance.configureUISprite.texture).SetWidth(24).SetHeight(24);
            cosmeticLockButtons[i] = (UIIconButtonBehavior)UIManager.Instance.AddIconButton(cosmeticSlotHoriz, delegate { ToggleSlotLock(index); }).SetIcon(ManagerBehavior.Instance.lockUISprite.texture).SetWidth(24).SetHeight(24);
        }

        UIManager.Instance.AddButton(window, "Close", Close);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (charBeh != null)
        {
            GameObject.Destroy(charBeh.gameObject);
            charBeh = null;
        }

        previewArena.gameObject.SetActive(false);
    }

    internal string CopyAppearance()
    {
        Dictionary<string, string> appearance = new Dictionary<string, string>();

        appearance.Add("type", charType?.id ?? charBeh?.characterType?.id);
        appearance.Add("variant", charTypeVariant?.id);
        appearance.Add("weight", weightSlider?.GetValue().ToString("F3"));
        appearance.Add("bust", bustSlider?.GetValue().ToString("F3"));
        if(charBeh != null)
        {
            appearance.Add("skinColor", charBeh.skinColor.ToString("F3"));
            appearance.Add("hairColor", charBeh.hairColor.ToString("F3"));
            appearance.Add("eyeColor", charBeh.eyeColor.ToString("F3"));
            appearance.Add("irisOffset", charBeh.irisOffset.ToString("F3"));
            appearance.Add("irisSize", charBeh.irisSize.ToString("F3"));
            appearance.Add("irisSaturation", charBeh.irisSaturation.ToString("F3"));
            appearance.Add("irisHighlight", charBeh.irisHighlight.ToString("F3"));
        }
        if (boneScales != null)
        {
            foreach (KeyValuePair<string, Vector3> keyValuePair in boneScales)
            {
                appearance.Add("boneScale." + keyValuePair.Key, keyValuePair.Value.ToString("F3"));
            }
        }
        foreach (var cosmetic in cosmeticInstances)
        {
            cosmetic?.CopyAppearance(appearance);
        }

        string final = null;
        foreach (var key in appearance)
        {
            final += key.Key + "=" + key.Value + "\n";
        }
        return final;
    }

    internal void PasteAppearance(string value)
    {
        bool valid = false;
        Dictionary<string, string> appearance = new Dictionary<string, string>();
        if (value != null && value.Length > 0)
        {
            valid = true;
            StringReader reader = new StringReader(value);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] keyval = line.Split('=');
                if (keyval.Length == 2)
                {
                    appearance[keyval[0]] = keyval[1];
                }
            }
        }
        if (valid)
        {
            if (appearance.TryGetValue("type", out string typeName))
            {
                if (OctDatManager.Instance.GetOctDatObject(typeName) is OctDatObject typeObj)
                {
                    if (typeObj.Instance() is CharacterType type)
                    {
                        charType = type;
                        appearance.Remove("type");
                    }
                }
            }
            if (appearance.TryGetValue("variant", out string variantName))
            {
                if (OctDatManager.Instance.GetOctDatObject(variantName) is OctDatObject variantObj)
                {
                    if (variantObj.Instance() is CharacterTypeVariant variant)
                    {
                        charTypeVariant = variant;
                        appearance.Remove("variant");
                    }
                }
            }
            if (appearance.TryGetValue("weight", out string weight))
            {
                weightSlider.SetValue(float.Parse(weight, System.Globalization.NumberFormatInfo.InvariantInfo));
                appearance.Remove("weight");
            }
            if (appearance.TryGetValue("bust", out string bust))
            {
                bustSlider.SetValue(float.Parse(bust, System.Globalization.NumberFormatInfo.InvariantInfo));
                appearance.Remove("bust");
            }
            if (appearance.TryGetValue("skinColor", out string skinColorString))
            {
                Color color = OctoberUtils.ColorFromString(skinColorString);

                if(charType.skinColors != null)
                {
                    skinColorSliders[0].SetValue(color.r);
                    skinColorSliders[1].SetValue(color.g);
                    skinColorSliders[2].SetValue(color.b);
                }
                else if (charType.skinLinearColor)
                {
                    skinColorSliders[0].SetValue((color.r - charType.skinColorFrom.r) / (charType.skinColorTo.r - charType.skinColorFrom.r));
                }
                else
                {
                    skinColorSliders[0].SetValue((color.r - charType.skinColorFrom.r) / (charType.skinColorTo.r - charType.skinColorFrom.r));
                    skinColorSliders[1].SetValue((color.g - charType.skinColorFrom.g) / (charType.skinColorTo.g - charType.skinColorFrom.g));
                    skinColorSliders[2].SetValue((color.b - charType.skinColorFrom.b) / (charType.skinColorTo.b - charType.skinColorFrom.b));
                }
                appearance.Remove("skinColor");
            }
            if (appearance.TryGetValue("hairColor", out string hairColorString))
            {
                Color hairColor = OctoberUtils.ColorFromString(hairColorString);

                hairColorSliders[0].SetValue(hairColor.r);
                hairColorSliders[1].SetValue(hairColor.g);
                hairColorSliders[2].SetValue(hairColor.b);
                appearance.Remove("hairColor");
            }
            if (appearance.TryGetValue("eyeColor", out string eyeColorString))
            {
                Color eyeColor = OctoberUtils.ColorFromString(eyeColorString);

                eyeColorSliders[0].SetValue(eyeColor.r);
                eyeColorSliders[1].SetValue(eyeColor.g);
                eyeColorSliders[2].SetValue(eyeColor.b);
                appearance.Remove("eyeColor");
            }
            if (appearance.TryGetValue("irisOffset", out string irisOffsetString))
            {
                var irisOffset = OctoberUtils.VectorFromString(irisOffsetString);
                irisOffsetSliders[0].SetValue(irisOffset.x);
                irisOffsetSliders[1].SetValue(irisOffset.y);
                irisOffsetSliders[2].SetValue(irisOffset.z);
                appearance.Remove("irisOffset");
            }
            if (appearance.TryGetValue("irisSize", out string irisSizeString))
            {
                irisSizeSlider.SetValue(float.Parse(irisSizeString, System.Globalization.NumberFormatInfo.InvariantInfo));
                appearance.Remove("irisSize");
            }
            if (appearance.TryGetValue("irisSaturation", out string irisSaturationString))
            {
                irisSaturationSlider.SetValue(float.Parse(irisSaturationString, System.Globalization.NumberFormatInfo.InvariantInfo));
                appearance.Remove("irisSaturation");
            }
            if (appearance.TryGetValue("irisHighlight", out string irisHighlightString))
            {
                irisHighlightSlider.SetValue(float.Parse(irisHighlightString, System.Globalization.NumberFormatInfo.InvariantInfo));
                appearance.Remove("irisHighlight");
            }
            for (int i = 0; i < charType.boneScales.Count; i++)
            {
                CharacterTypeBoneScale characterTypeBoneScale = charType.boneScales[i];
                boneScales = new Dictionary<string, Vector3>();
                string scaleString;
                if (appearance.TryGetValue("boneScale." + characterTypeBoneScale.boneName, out scaleString))
                {
                    var scale = OctoberUtils.VectorFromString(scaleString);
                    boneScales[characterTypeBoneScale.boneName] = scale;
                    boneScaleX[i].SetValue(scale.x);
                    boneScaleY[i].SetValue(scale.y);
                    boneScaleZ[i].SetValue(scale.z);
                }
            }


            for (int i = 0; i < charTypeVariant.cosmeticSlots.Count; ++i)
            {
                CharacterCosmeticSlot slot = charTypeVariant.cosmeticSlots[i];
                if (appearance.TryGetValue(slot.name, out string id))
                {
                    if (OctDatManager.Instance.GetOctDatObject(id) is OctDatObject obj)
                    {
                        if(i == cosmeticInstances.Count)
                        {
                            cosmeticInstances.Add(null);
                        }
                        // remove existing
                        cosmeticInstances[i] = new CharacterCosmeticInstance(null, slot.name, obj.Instance() as CharacterCosmetic);
                        cosmeticInstances[i].PasteAppearance(appearance);
                        foreach (string key in appearance.Keys.Where(k => k.StartsWith(slot.name)).ToList())
                        {
                            appearance.Remove(key);
                        }
                    }
                }
            }
            /*
            if(appearance.Count > 0)
            {
                System.Console.WriteLine($"Leftover values:");
                foreach (var pair in appearance)
                {
                    System.Console.WriteLine($"key: {pair.Key}, value: {pair.Value}");
                }
            }
            */

            while(charTypeVariant.cosmeticSlots.Count < cosmeticInstances.Count)
            {
                cosmeticInstances.RemoveAt(cosmeticInstances.Count - 1);
            }
            Refresh(false, charType, charTypeVariant);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (InputManager.Instance.IsTriggered("Oct.Settings.Global.Controls.Capture.CopyAppearance"))
        {
            GUIUtility.systemCopyBuffer = CopyAppearance();
        }
        else if (InputManager.Instance.IsTriggered("Oct.Settings.Global.Controls.Capture.PasteAppearance"))
        {
            PasteAppearance(GUIUtility.systemCopyBuffer);
        }
    }
}
