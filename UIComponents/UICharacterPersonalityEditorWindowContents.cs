using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class UICharacterPersonalityEditorWindowContents : UIWindowContents
{

    private UILayoutBehavior H1;
    private UILayoutBehavior H1V1;
    private UILayoutBehavior H1V2;
    private UILayoutBehavior H1V3;


    private Pawn pawn;

    private UIClickableLabelBehavior nameLabel;
    private UIInputBoxBehavior nameInputBox;
    private UILabelBehavior ageText;
    private UILabelBehavior levelText;

    private UIButtonBehavior alignmentButton;
    private UIIconButtonBehavior alignmentLockButton;
    private bool alignmentLocked = false;
    private UILayoutBehavior alignmentHG;

    private UIButtonBehavior kingdomSizeButton;
    private UIIconButtonBehavior kingdomSizeLockButton;
    private bool kingdomSizeLocked = false;
    private UILayoutBehavior kingdomSizeHG;


    private UIButtonBehavior[] attributePotentialButtons;
    private UIIconButtonBehavior[] attributePotentialLockButtons;
    private List<bool> attributePotentialLocks = new List<bool>();
    private UILabelBehavior[] attributeLabels;

    private UISliderBehavior[] attributeSliders;
    private UIIconButtonBehavior[] attributeLockButtons;
    private List<bool> attributeLocks = new List<bool>();


    private UIIconButtonBehavior opinionsLockButton;
    private bool opinionsLocked = false;
    private UILayoutBehavior opinionsHG;

    private List<bool> opinionLocks = new List<bool>();
    private Opinion[] opinions;
    private UILayoutBehavior[] opinionLayouts;
    private UIButtonBehavior[] opinionButtons;
    private UIIconButtonBehavior[] opinionLockButtons;
    private UIInputBoxBehavior opinionFilterInput;
    private UIVerticalScrollList opinionVSL;
    private List<UIElementCharacterTypeEditorSlot> elements = new List<UIElementCharacterTypeEditorSlot>();
    internal UILayoutBehavior[] settingElementContainers;


    private List<UILabelBehavior> colorSyncedLabels = new List<UILabelBehavior>();
    private Color color => (pawn.alignment <= -0.33f) ? ManagerBehavior.Instance.badUIColor : ((pawn.alignment >= 0.33f) ? ManagerBehavior.Instance.goodUIColor : Color.white);


    internal void AddElement(UIElementBehavior element)
    {
        UIElementCharacterTypeEditorSlot elementSlot = new UIElementCharacterTypeEditorSlot(element);
        elementSlot.Hide();
        elements.Add(elementSlot);
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
        settingElementContainers[disp].SizeHeightToContent();
        settingElementContainers[disp].SizeWidthToContent();
        H1V1.SizeHeightToContent();
        H1V1.SizeWidthToContent();
        H1V2.SizeHeightToContent();
        H1V2.SizeWidthToContent();
        opinionVSL.SetHeight(H1V2.GetHeight());
        opinionVSL.SizeWidthToContent();
        H1V3.SetHeight(H1V2.GetHeight());
        H1V3.SetWidth(H1V2.GetWidth());

        H1.SizeHeightToContent();
        H1.SizeWidthToContent();
    }

    internal void DisplayBlankElement(int disp)
    {
        if (settingElementContainers[disp].obj is UIElementCharacterTypeEditorSlot existing)
        {
            existing.Hide();
            settingElementContainers[disp].SetObject(null);
        }
        H1V1.SizeHeightToContent();
        H1V1.SizeWidthToContent();
        H1V2.SizeHeightToContent();
        H1V2.SizeWidthToContent();
        opinionVSL.SetHeight(H1V2.GetHeight());
        opinionVSL.SizeWidthToContent();
        H1V3.SetHeight(H1V2.GetHeight());
        H1V3.SetWidth(H1V2.GetWidth());

        H1.SizeHeightToContent();
        H1.SizeWidthToContent();
        window.SizeHeightToContent();
        window.SizeWidthToContent();
    }

    public void Initialize(ref Pawn pawn)
    {
        this.pawn = pawn;

        if(this.pawn == null)
            Close();


        window.SetChildAlignment(TextAnchor.UpperCenter);
        var label =((UILabelBehavior)UIManager.Instance.AddLabel(window, "Noble Personality Editor").SetMargins(new Vector4(5,5,5,5)).SetFontStyle(FontStyles.Bold).SetFontSize(24).SetAlignment(TextAlignmentOptions.Center).SetFontColor(color));
        label.SizeHeightToContent();
        label.SizeWidthToContent();
        colorSyncedLabels.Add(label);

        //Randomize
        UIManager.Instance.AddButton(window, "Randomize", () => Randomize());
        H1 = UIManager.Instance.AddHorizontalGroup(window);
        H1.SetWidth(Screen.width * .9f).SetHeight(Screen.height * .9f);
        H1.SetPadding(new RectOffset(8, 8, 8, 8));
        H1.SetChildAlignment(TextAnchor.UpperCenter);


        H1V1 = UIManager.Instance.AddVerticalGroup(H1).SetChildAlignment(TextAnchor.UpperLeft);
        H1V1.SetPadding(new RectOffset(5, 5, 5, 5));


        nameLabel = UIManager.Instance.AddClickableLabel(H1V1, this.pawn.GetName(), this.NameChange);
        nameLabel.SetMargins(new Vector4(5, 5, 5, 5)).SetFontStyle(FontStyles.Bold).SetFontSize(20).SetAlignment(TextAlignmentOptions.Center);
        nameLabel.SizeHeightToContent();
        nameLabel.SizeWidthToContent();

        nameInputBox = UIManager.Instance.AddInputBox(H1V1, this.pawn.GetName());
        nameInputBox.SetMargins(new Vector4(5, 5, 5, 5)).SetFontStyle(FontStyles.Bold).SetFontSize(20).SetAlignment(TextAlignmentOptions.Center);
        nameInputBox.SetActive(false);

        var ageLabel = UIManager.Instance.AddLabel(H1V1, $"Age: {this.pawn.character.age}");
        ageLabel.SetMargins(new Vector4(5, 5, 5, 5)).SetFontStyle(FontStyles.Bold).SetFontSize(20).SetAlignment(TextAlignmentOptions.Center);
        ageLabel.SizeHeightToContent();
        ageLabel.SizeWidthToContent();
        colorSyncedLabels.Add(ageLabel);

        var levelLabel = UIManager.Instance.AddLabel(H1V1, $"Level: {this.pawn.character.level}");
        levelLabel.SetMargins(new Vector4(5, 5, 5, 5)).SetFontStyle(FontStyles.Bold).SetFontSize(20).SetAlignment(TextAlignmentOptions.Center);
        levelLabel.SizeHeightToContent();
        levelLabel.SizeWidthToContent();
        colorSyncedLabels.Add(levelLabel);


        //Alignment
        alignmentHG = UIManager.Instance.AddHorizontalGroup(H1V1);
        alignmentHG.SetChildAlignment(TextAnchor.MiddleCenter);
        alignmentButton = (UIButtonBehavior)UIManager.Instance.AddButton(alignmentHG, Pawn.AlignmentFromValue(this.pawn.alignment).ToString(), this.ClickedAlignment)
            .SetFontColor(color).SetWidth(136);

        alignmentLockButton = (UIIconButtonBehavior)UIManager.Instance.AddIconButton(alignmentHG, ToggleAlignmentLock).SetIcon(ManagerBehavior.Instance.lockUISprite.texture)
            .SetWidth(24).SetHeight(24);

        alignmentHG.SizeHeightToContent();
        alignmentHG.SizeWidthToContent();


        H1V2 = UIManager.Instance.AddVerticalGroup(H1).SetChildAlignment(TextAnchor.UpperCenter);
        H1V2.SetPadding(new RectOffset(5, 5, 5, 5));

        int attLength = this.pawn.character.attributes.Length;

        attributeLabels = new UILabelBehavior[attLength];
        attributeSliders = new UISliderBehavior[attLength];
        attributeLockButtons = new UIIconButtonBehavior[attLength];
        attributePotentialButtons = new UIButtonBehavior[attLength];
        attributePotentialLockButtons = new UIIconButtonBehavior[attLength];


        int i = 0;
        foreach(var attribute in this.pawn.character.attributes)
        {
            int index = i;
            UILayoutBehavior attributeLayout = (UILayoutBehavior)UIManager.Instance.AddHorizontalGroup(H1V2)
                .SetPadding(new RectOffset(5, 5, 5, 5)).SetGlobalKey($"{attribute.name}HG");

            attributeLayout.SetChildAlignment(TextAnchor.MiddleCenter);

            attributeLabels[index] = (UILabelBehavior)UIManager.Instance.AddLabel(attributeLayout, $"{attribute.name}: ({this.pawn.character.stats.GetStatValue(attribute.name)})").SetMargins(new Vector4(2,2,2,2))
                .SetFontStyle(FontStyles.Bold).SetFontSize(20).SetAlignment(TextAlignmentOptions.Center);
            attributeLabels[index].SizeHeightToContent();
            attributeLabels[index].SizeWidthToContent();
            colorSyncedLabels.Add(attributeLabels[index]);

            attributeLockButtons[index] = (UIIconButtonBehavior)UIManager.Instance.AddIconButton(attributeLayout, () => ToggleAttrtibuteLock(index))
                .SetIcon(ManagerBehavior.Instance.lockUISprite.texture).SetMargins(new Vector4(2, 2, 2, 2)).SetWidth(24).SetHeight(24);

            attributeSliders[index] = (UISliderBehavior)UIManager.Instance.AddSlider(attributeLayout)
                .SetMinValue(0).SetMaxValue(20).AddDoneChangingHandler((f)=>{
                    AttributeChanged(attribute.name, index, f);
                }).SetHeight(24).SetWidth(136);
            attributePotentialButtons[index] = (UIButtonBehavior)UIManager.Instance.AddButton(attributeLayout, Character.AttributePotentialToString(attribute.potential), () => ClickedAttrtibutePotential(attribute.name, index))
                .SetMargins(new Vector4(2, 2, 2, 2)).SizeHeightToContent().SetWidth(136);

            attributePotentialLockButtons[index] = (UIIconButtonBehavior)UIManager.Instance.AddIconButton(attributeLayout, () =>
                ToggleAttrtibutePotentialLock(index)).SetIcon(ManagerBehavior.Instance.lockUISprite.texture).SetMargins(new Vector4(2, 2, 2, 2))
                .SetWidth(24).SetHeight(24);

            attributeLayout.SizeHeightToContent();
            attributeLayout.SizeWidthToContent();
            attributeLocks.Add(false);
            attributePotentialLocks.Add(false);
            i++;
        }


        H1V3 = UIManager.Instance.AddVerticalGroup(H1).SetChildAlignment(TextAnchor.UpperCenter);
        H1V3.SetPadding(new RectOffset(5, 5, 5, 5));
        H1V3.SetChildAlignment(TextAnchor.UpperCenter);

        i = 0;
        var filteredOpinions = pawn.subjectOpinions.Values.Where(o=>o.subject is not Pawn && o.subject is not Kingdom && o.subject is not WarInformation).ToList();
        int count = filteredOpinions.Count;

        opinionFilterInput = UIManager.Instance.AddInputBox(H1V3, "");
        opinionFilterInput.AddValueChangedHandler(FilterInputChanged).SetMargins(new Vector4(5,5,5,5)).SetWidth(160).SetHeight(24);
        opinionFilterInput.textMesh.richText = false;

        opinionVSL = UIManager.Instance.AddVerticalScrollList(H1V3, attLength, DisplayElement, DisplayBlankElement, FilterElements);
        opinionVSL.SetChildAlignment(TextAnchor.UpperCenter);
        opinionVSL.SetRightAlign(true);
        settingElementContainers = new UILayoutBehavior[count];
        opinions = new Opinion[count];
        opinionLayouts = new UILayoutBehavior[count];
        opinionButtons = new UIButtonBehavior[count];
        opinionLockButtons = new UIIconButtonBehavior[count];

        foreach (var opinion in filteredOpinions)
        {
            int index = i;
            opinions[index] = opinion;
            settingElementContainers[index] = (UILayoutBehavior)UIManager.Instance.AddHorizontalGroup(opinionVSL).SetPadding(new RectOffset(5, 5, 5, 5)).SetWidth(160).SetHeight(24);
            var layout = (UILayoutBehavior)UIManager.Instance.AddHorizontalGroup(H1V3)
                .SetPadding(new RectOffset(5, 5, 5, 5)).SetGlobalKey($"{opinion.subject.GetName()}HG");

            layout.SetChildAlignment(TextAnchor.MiddleCenter);

            opinionButtons[index] = (UIButtonBehavior)UIManager.Instance.AddButton(layout, $"{Opinion.ToString(opinion.GetStrength())} {opinion.subject.GetName()}", () => ClickedOpinionStrength(index)).SetFontSize(20).SetMargins(new Vector4(2, 2, 2, 2)).SizeHeightToContent().SizeWidthToContent();
            opinionLockButtons[index] = (UIIconButtonBehavior)UIManager.Instance.AddIconButton(layout, () =>
                ToggleOpinionLock(index)).SetIcon(ManagerBehavior.Instance.lockUISprite.texture)
                .SetWidth(24).SetHeight(24);

            layout.SizeHeightToContent();
            layout.SizeWidthToContent();
            opinionLocks.Add(false);
            opinionLayouts[index] = layout;
            AddElement(layout);
            i++;
        }

        opinionVSL.Refresh(count);

        UIManager.Instance.AddButton(window, "Close", Close); 
        SyncronizeColors();

        H1V1.SizeHeightToContent();
        H1V1.SizeWidthToContent();
        H1V2.SizeHeightToContent();
        H1V2.SizeWidthToContent();
        opinionVSL.SetHeight(Screen.height*.5f);
        opinionVSL.SizeWidthToContent();
        H1V3.SizeHeightToContent();
        H1V3.SizeWidthToContent();
        H1.SizeHeightToContent();
        H1.SizeWidthToContent();

        window.SizeHeightToContent();
        window.SizeWidthToContent();
    }



    private async void FilterInputChanged(string value)
    {
        await Task.Yield();
        opinionVSL.Refresh(elements.Count);
    }

    private bool FilterElements(int index)
    {
        string filtertext = Regex.Replace(opinionFilterInput.GetText(), "[^ -~]+", string.Empty);
        if (!string.IsNullOrWhiteSpace(filtertext) && !string.IsNullOrEmpty(filtertext))
        {
            return !opinions[index].ToString().Contains(filtertext);                
        }
        return false;
    }

    private void ToggleOpinionLock(int index)
    {
        opinionLocks[index] = !opinionLocks[index];
        opinionLockButtons[index].SetIconColor(opinionLocks[index] ? ManagerBehavior.Instance.titleTextColor : Color.white);
    }

    private void ClickedOpinionStrength(int index)
    {
        UIWindowBehavior typePopup = UIManager.Instance.AddPopupWindow();
        UISelectionPopupWindowContents contents = typePopup.InstanceContents<UISelectionPopupWindowContents>();

        foreach (Opinion.Strength strength in Enum.GetValues(typeof(Opinion.Strength)))
        {
            contents.AddOption(Opinion.ToString(strength), strength);
        }
        contents.AddCallback((string name, object obj) => OpinionStrengthChosen(index, obj));
    }

    private void OpinionStrengthChosen(int index, object obj)
    {
        Opinion.Strength strength = (Opinion.Strength)obj;
        var opinion = opinions[index];
        opinion.raw = Feeling.ValueToRaw(Opinion.ToValue(strength));
        opinion.UpdateInformation();
        opinionButtons[index].SetText($"{Opinion.ToString(strength)} {opinion.subject.GetName()}");
    }

    private void NameChange()
    {
        nameLabel.SetActive(false);
        nameInputBox.SetActive(true);
    }

    private void AttributeChanged(string name, int index, float f)
    {
        int value = Mathf.CeilToInt(f);
        pawn.character.stats.SetStat(name, value);
        attributeLabels[index].SetText($"{name}: ({value})");
        pawn.UpdateProfession();
        SyncronizeColors();
    }

    private void ToggleAttrtibuteLock(int i)
    {
        attributeLocks[i] = !attributeLocks[i];
        attributeLockButtons[i].SetIconColor(attributeLocks[i] ? ManagerBehavior.Instance.titleTextColor : Color.white);
    }

    private void ToggleAttrtibutePotentialLock(int i)
    {
        attributePotentialLocks[i] = !attributePotentialLocks[i];
        attributePotentialLockButtons[i].SetIconColor(attributePotentialLocks[i] ? ManagerBehavior.Instance.titleTextColor : Color.white);
    }

    private void ClickedAttrtibutePotential(string attribute, int i)
    {
        UIWindowBehavior typePopup = UIManager.Instance.AddPopupWindow();
        UISelectionPopupWindowContents contents = typePopup.InstanceContents<UISelectionPopupWindowContents>();

        foreach (CharacterAttributePotential characterAttributePotential in Enum.GetValues(typeof(CharacterAttributePotential)))
        {
            if(characterAttributePotential < CharacterAttributePotential.Count)
            contents.AddOption(characterAttributePotential.ToString(), characterAttributePotential);
        }
        contents.AddCallback((string name, object obj) =>AttrtibutePotentialChosen(attribute, i, obj)); 
    }

    private void AttrtibutePotentialChosen(string attribute, int index, object obj)
    {
        var potential = (CharacterAttributePotential)obj;
        pawn.character.SetAttributePotentialByName(attribute, potential);
        attributePotentialButtons[index].SetText(Character.AttributePotentialToString(potential));
        SyncronizeColors();
    }

    private void ClickedAlignment()
    {

        UIWindowBehavior typePopup = UIManager.Instance.AddPopupWindow();
        UISelectionPopupWindowContents contents = typePopup.InstanceContents<UISelectionPopupWindowContents>();


        foreach (PawnAlignment pawnAlignment in Enum.GetValues(typeof(PawnAlignment)))
        {
            contents.AddOption(pawnAlignment.ToString(), pawnAlignment);
        }
        contents.AddCallback(AlignmentChosen);
    }

    internal void AlignmentChosen(string name, object obj)
    {
        pawn.SetAlignment(Pawn.AlignmentToValue((PawnAlignment)obj));
        alignmentButton.SetText(Pawn.AlignmentFromValue(pawn.alignment).ToString()).SetFontColor(color);
        SyncronizeColors();
    }

    private void SyncronizeColors()
    {
        colorSyncedLabels.ForEach(ele =>ele.SetFontColor(color));
    }

    private void ToggleAlignmentLock()
    {
        alignmentLocked = !alignmentLocked;
        alignmentLockButton.SetIconColor(alignmentLocked ? ManagerBehavior.Instance.titleTextColor : Color.white);
    }

    private void Randomize()
    {
        Character character = pawn.character;
        CharacterType type = character.type;

        if (!alignmentLocked)
        {
            AlignmentChosen("", Pawn.AlignmentFromValue(UnityEngine.Random.value * 2f - 1f));
        }

        if (!kingdomSizeLocked)
        {
            pawn.SetKingdomSizePreference(Mathf.RoundToInt(Mathf.Clamp(OctoberMath.DistributedRandom(0f, 4f, 2f), 0f, 3f)));
        }

        for (int i = 0; i < type.attributes.Count; i++)
        {
            var attribute = character.attributes[i];
            if (!attributePotentialLocks[i])
            {
                float value = UnityEngine.Random.value;
                float f = 2.5f + Mathf.Sign(value - 0.5f) * Mathf.Pow(Mathf.Abs(value - 0.5f) * 2f, 3f) * 2.5f;
                AttrtibutePotentialChosen(attribute.name, i, (CharacterAttributePotential)Mathf.RoundToInt(f));
            }
            if (!attributeLocks[i])
            {
                int value = UnityEngine.Random.Range(0, character.level + 2);
                character.stats.SetStat(attribute.name, value);
                attributeSliders[i].SetValue(value);
                attributeLabels[i].SetText($"{attribute.name}: ({value})");
            }
        }

        if (!opinionsLocked)
        {
            var subjects = opinions.Select(o=>o.subject).ToList();
            for (int i = 0; i < subjects.Count; i++)
            {
                if (!opinionLocks[i])
                {
                    ISubject subject = subjects[i];

                    float min = -1f;
                    float max = 1f;
                    float center = 0f;
                    float range = 1f;
                    float exp = 2f;
                    float minSimilarity = -1f;
                    foreach (CharacterTypeOpinionRange characterTypeOpinionRange in type.opinions)
                    {
                        if (characterTypeOpinionRange.Affects(subject))
                        {
                            min = characterTypeOpinionRange.min;
                            max = characterTypeOpinionRange.max;
                            center = characterTypeOpinionRange.center;
                            range = characterTypeOpinionRange.range;
                            exp = characterTypeOpinionRange.exp;
                            minSimilarity = characterTypeOpinionRange.minSimilarity;
                            break;
                        }
                    }
                    float num5 = Mathf.Clamp(OctoberMath.DistributedRandom(center - range, center + range, exp), min, max);
                    var rival = Manager<KingdomManager>.Instance.playerRival?.ruler;
                    if (rival != null)
                    {
                        num5 = Mathf.Clamp(LerpSimilarity(num5, rival.GetOpinionValue(subject, true), min, max, minSimilarity), min, max);
                    }
                    float raw = Feeling.ValueToRaw(num5);
                    opinions[i].raw = raw;
                    opinions[i].UpdateInformation();
                    opinionButtons[i].SetText($"{Opinion.ToString(opinions[i].GetStrength())} {opinions[i].subject.GetName()}");
                }
            }
            opinionVSL.Refresh(elements.Count);
            pawn.RerollValuations();
        }
        pawn.UpdateProfession();
        SyncronizeColors();
    }

    private static float LerpSimilarity(float from, float to, float min, float max, float minSimilarity)
    {
        float num = (max + min) * 0.5f;
        float a = from - num;
        float num2 = to - num;
        float num3 = Mathf.Clamp(OctoberMath.DistributedRandom(-0.5f, 0f, 2f) + OctoberMath.DistributedRandom(-0.5f, 0.5f, 2f), minSimilarity, 1f);
        if (num3 < 0f)
        {
            num2 = -num2;
            num3 = -num3;
        }
        return num + Mathf.Lerp(a, num2, num3);
    }

    // Start is called before the first frame update
    public override void Awake()
    {
        base.Awake();
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public override void Update()
    {
        base.Update();

        if (nameInputBox.isActiveAndEnabled && Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrWhiteSpace(nameInputBox.GetText()))
        {
            string value = nameInputBox.GetText();
            pawn.character.SetName(value);
            nameLabel.SetText(value);
            nameLabel.SetActive(true);
            nameLabel.SizeWidthToContent();
            nameInputBox.SetActive(false);
        }
    }

    private void Close()
    {
        GameObject.Destroy(window.gameObject);
    }
}
