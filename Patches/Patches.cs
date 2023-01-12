namespace InitialNobles.Patches
{
    using HarmonyLib;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection.Emit;
    using UnityEngine;


    [HarmonyPatch]
    public static class Patches
    {
        public static int codepoint = -1;
        public static int InitialNobles = 4;
        public static bool RemoveKingomPrefs = false;
        public static bool DisableTutorials = false;
        public static bool SkipIntroDialog = false;


        /// <summary>
        /// This Transpiler runs only once at the start of the game and first tries to find where in this method an exact set of 3 instructions are called back to back.
        /// These 3 instructions equate to the ** int num6 = 4; **  on line 31 of <see cref="KingdomManager.GeneratePlayerParty"/> 
        /// If it finds these it will replace the 4 with a call to my <see cref="GetMaxInitialNobles"/> instead.
        /// this will make it equate to ** int num6 = KingdomManager_Patches.GetMaxInitialNobles(); **.
        /// 
        /// If the 3 instructions are not found then it will log an error that people can report to me as the code for this method changed and this needs to be fixed.
        /// </summary>
        [HarmonyPatch(typeof(KingdomManager), nameof(KingdomManager.GeneratePlayerParty))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 1; i < instructions.Count()-1; i++)
            {
                var currentInstruction = codeInstructions[i];
                var secondInstruction = codeInstructions[i + 1];
                var thirdInstruction = codeInstructions[i + 2];
                
                if (currentInstruction.opcode != OpCodes.Ldc_I4_4
                    || secondInstruction.opcode != OpCodes.Stloc_0
                    || thirdInstruction.opcode != OpCodes.Newobj) continue;

                codepoint = i;
                
                codeInstructions[i].opcode = OpCodes.Call;
                codeInstructions[i].operand = typeof(Patches).GetMethod("GetMaxInitialNobles");
                break;
            }

            if (codepoint == -1)
                Debug.LogError("InitialNobles: Patching Failed! \nKingdomManager GeneratePlayerParty Transpiler injection point NOT found!!  Game has most likely updated and broken this mod!");

            return codeInstructions.AsEnumerable();
        }

        public static int GetMaxInitialNobles()
        {
            return Math.Max(InitialNobles + 2, 4);
        }


        /// <summary>
        /// This patch fixes the FoundKingdomScriptableConversationStep to actually add all the extra Nobles to the Kingdom
        /// Otherwise you will still only get the first 2 and the rest will just walk away.
        /// 
        /// Only works if the Transpiler was able to patch the GenerateParty method.
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(typeof(FoundKingdomScriptableConversationStep), nameof(FoundKingdomScriptableConversationStep.Enter))]
        [HarmonyPrefix]
        public static void Prefix(FoundKingdomScriptableConversationStep __instance)
        {
            if(codepoint == -1 && InitialNobles > 2)
                InitialNobles = 2;

            __instance.vassals.Clear();
            for (int i = 1; i <= InitialNobles; i++)
            {
                __instance.vassals.Add("vassal" + i);
            }
        }


        /// <summary>
        /// IF SkipIntro is NOT set then this patch will proccess the starting kingdom members.
        /// </summary>
        [HarmonyPatch(typeof(FoundKingdomScriptableConversationStep), nameof(FoundKingdomScriptableConversationStep.Enter))]
        [HarmonyPostfix]
        public static void Postfix(FoundKingdomScriptableConversationStep __instance)
        {
            if (!KingdomManager.SkipIntro)
            {
                ProcessInitialKingdomeMembers();
            }
        }

        /// <summary>
        /// IF SkipIntro IS set then this patch will proccess the starting kingdom members
        /// </summary>
        [HarmonyPatch(typeof(ManagerBehavior), nameof(ManagerBehavior.StartPlaying))]
        [HarmonyPostfix]
        public static void Postfix(ManagerBehavior __instance)
        {
            if (KingdomManager.SkipIntro)
            {
                ProcessInitialKingdomeMembers();
            }
        }

        /// <summary>
        /// This method will remove extra kingdom members after making them drop any supplies they may be holding.
        /// it will also check iff any members that are staying are naked it will generate clothing for them. 
        /// Then it will give extra supplies based on how many extra kingdom members there are above the default value.
        /// </summary>
        private static void ProcessInitialKingdomeMembers()
        {
            KingdomManager kingdomManager = Manager<KingdomManager>.Instance;
            var count = InitialNobles;
            new List<Pawn>(kingdomManager.playerKingdom.members).ForEach(member =>
            {
                if (member != kingdomManager.playerKingdom.ruler)
                {
                    if (count > 0)
                    {
                        count--;

                        var equipment = member.character.inventory.GetPrivateField<List<Item>>("equipment");
                        if (equipment.Where(e => e != null).Count() <= 2)
                        {
                            member.character.inventory.GenerateEquipment();
                        }
                    }
                    else
                    {
                        member.character.inventory.DropSupplies();
                        member.character.Destroy();
                    }
                }
            });

            if(InitialNobles > 2)
            {
                GiveItem(Manager<ItemManager>.Instance.GetItemType("Travel Meals"), 18, kingdomManager.playerKingdom.members);
                GiveItem(Manager<ItemManager>.Instance.GetItemType("Cotton"), 48, kingdomManager.playerKingdom.members);
                GiveItem(Manager<ItemManager>.Instance.GetItemType("Straw"), 64, kingdomManager.playerKingdom.members);
                GiveItem(Manager<ItemManager>.Instance.GetItemType("Corn Seeds"), 8, kingdomManager.playerKingdom.members);
            }
        }

        internal static void GiveItem(IItemType itemType, int count, List<Pawn> members)
        {
            int nobles = InitialNobles > 4?InitialNobles - 2: 4;
            int countPerMember = (((count * nobles) / 2)-count)/nobles;
            bool flag = itemType is StackedItemType;
            if (flag)
            {
                for (int i = 0; i < members.Count; i++)
                {
                    StackedItem item = itemType.NewItem() as StackedItem;
                    item.AddInstances(countPerMember, 1f);
                    item.SetInInventory(members[i].character.inventory);
                }
            }
        }

        /// <summary>
        /// If <see cref="RemoveKingomPrefs"/> is true, this will set the kingdom prefs value to half maxint and then remove any esteem buffs or debuffs from the player. 
        /// </summary>
        [HarmonyPatch(typeof(Pawn), nameof(Pawn.SetKingdomSizePreference))]
        [HarmonyPrefix]
        public static bool Prefix(Pawn __instance)
        {
            if (!RemoveKingomPrefs) return true;

            __instance.GetType().GetProperty("kingdomSizePreference").SetValue(__instance, int.MaxValue / 2);
            if (__instance.kingdomSizePreferenceInfo != null)
            {
                __instance.RemoveInformation(__instance.kingdomSizePreferenceInfo);
                __instance.GetType().GetProperty("kingdomSizePreferenceInfo").SetValue(__instance, null);
            }
            RemoveKingdomSizeFeeling(__instance);
            return false;
        }


        [HarmonyPatch(typeof(Pawn), nameof(Pawn.AddInformation))]
        [HarmonyPrefix]
        public static bool AddInformation_Prefix(Pawn __instance, Information info)
        {
            if (!RemoveKingomPrefs || info is not KingdomSizePreferenceInformation) 
                return true;
            
            return false;
        }


        [HarmonyPatch(typeof(KingdomSizePreferenceInformation), nameof(KingdomSizePreferenceInformation.GetResponse))]
        [HarmonyPrefix]
        public static bool AddInformation_Prefix(KingdomSizePreferenceInformation __instance, ref Information __result)
        {
            if (!RemoveKingomPrefs)
                return true;

            __result = null;
            return false;
        }

        private static void RemoveKingdomSizeFeeling(Pawn pawn)
        {
            foreach (Pawn pawn2 in Manager<KingdomManager>.Instance.pawns)
            {
                Opinion opinion = pawn.character.pawn.GetOpinion(pawn2);
                
                if (opinion == null)
                { return; }

                var modifiers = opinion.GetPrivateField<List<FeelingModifier>>("modifiers");
                if (modifiers != null)
                {
                    foreach (var feeling in new List<FeelingModifier>(modifiers))
                    {
                        if (feeling.reason.info is KingdomSizePreferenceInformation)
                        {
                            feeling.Destroy();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// If <see cref="RemoveKingomPrefs"/> is true, this will set the kingdom prefs value to half maxint and then remove any esteem buffs or debuffs from the player. 
        /// </summary>
        [HarmonyPatch(typeof(Pawn), nameof(Pawn.PostLoad))]
        [HarmonyPrefix]
        public static void Prefix_PostLoad(Pawn __instance)
        {
            if (!RemoveKingomPrefs)
            { return; }

            __instance.GetType().GetProperty("kingdomSizePreference").SetValue(__instance, int.MaxValue / 2);
            if (__instance.kingdomSizePreferenceInfo != null)
            {
                __instance.RemoveInformation(__instance.kingdomSizePreferenceInfo);

                __instance.GetType().GetProperty("kingdomSizePreferenceInfo").SetValue(__instance, null);
            }
            RemoveKingdomSizeFeeling(__instance);
            return;
        }

        /// <summary>
        /// Forces the Guided Tutorial off if the InitialNobles is set to 0.
        /// </summary>
        [HarmonyPatch(typeof(GuidedExperienceToggleSettingDefinition), nameof(GuidedExperienceToggleSettingDefinition.SetValue))]
        [HarmonyPostfix]
        public static void Postfix(GuidedExperienceToggleSettingDefinition __instance, object value)
        {
            if (__instance.id == "Oct.Settings.Game.GuidedExperience" && (InitialNobles == 0 || DisableTutorials) && (bool)value)
            {
                __instance.SetValue(false);
            }
        }


        [HarmonyPatch(typeof(Pawn), nameof(Pawn.kingdomSizePreferenceInfo), MethodType.Getter)]
        [HarmonyPostfix]
        public static void Postfix(ref KingdomSizePreferenceInformation __result)
        {
            if (!RemoveKingomPrefs) return;

            __result = null;
        }

        [HarmonyPatch(typeof(ConversationManager), nameof(ConversationManager.WasDiscussedInConversation))]
        [HarmonyPrefix]
        public static bool Prefix(ref Information info, ref bool __result)
        {
            if (!RemoveKingomPrefs || info is not KingdomSizePreferenceInformation) return true;

            __result = true;
            return false;
        }

        [HarmonyPatch(typeof(ScriptableConversationInstance), nameof(ScriptableConversationInstance.ExecuteStep))]
        [HarmonyPrefix]
        public static void Prefix(ScriptableConversationInstance __instance, ref ScriptableConversationStep step)
        {

            if (!ManagerBehavior.Instance.isIntro || !SkipIntroDialog || step == null) return;

            while (step is LineScriptableConversationStep || step is GrammarScriptableConversationStep)
            {
                step = step.nextStep;
            }

            return;
        }
        
        [HarmonyPatch(typeof(TimeManager), nameof(TimeManager.PopSoftPause))]
        [HarmonyPrefix]
        public static void Prefix(TimeManager __instance)
        {
            if(__instance.softPauseStack < 0)
            {
                // As per killabi, this shouldn't be < 0 but it may be being set that way due to this mods skipping of the intro.
                __instance.SetPrivateField<int>("softPauseStack", 0);
            }

        }

        [HarmonyPatch(typeof(IntroDioramaBehavior), nameof(IntroDioramaBehavior.RerollCharacter))]
        [HarmonyPrefix]
        public static bool Prefix(IntroDioramaBehavior __instance)
        {
            if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftShift)) return true;

            RerollCharacter(__instance);
            return false;
        }

        private static bool RollingCharacter = false; 

        private static async void RerollCharacter(IntroDioramaBehavior dioramaBehavior)
        {
            if (!RollingCharacter)
            {
                RollingCharacter = true;

                string key = UIElementBehavior.current?.obj as string;
                Pawn speaker = key != null ? dioramaBehavior.context.GetValue(key) as Pawn : null;
                DioramaCharacterInstance dioramaCharacterInstance = key != null && speaker != null ? DioramaBooth.Instance.dioramaBeh.GetSpeaker(speaker) : null;

                if (key != null && speaker != null && dioramaCharacterInstance != null)
                {
                    CharacterManager manager = Manager<CharacterManager>.Instance;
                    if (manager.GetPrivateField<UIWindowBehavior>("typeEditor") == null)
                    {
                        if (Input.GetKey(KeyCode.LeftShift))
                        {
                            ManagerBehavior.Instance.scenes.gameObject.SetActive(false);
                            ManagerBehavior.Instance.center.gameObject.SetActive(true);
                            var typeEditor = Manager<UIManager>.Instance.AddWindow(ManagerBehavior.Instance.center);
                            manager.SetPrivateField<UIWindowBehavior>("typeEditor", typeEditor);
                            typeEditor.InstanceContents<UINoblesEditorWindowContents>().Initialize(ref speaker);

                            while (manager.GetPrivateField<UIWindowBehavior>("typeEditor") != null)
                            {
                                await System.Threading.Tasks.Task.Yield();
                            }
                            ManagerBehavior.Instance.scenes.gameObject.SetActive(true);
                            ManagerBehavior.Instance.center.gameObject.SetActive(false);
                            string appearance = speaker.character.CopyAppearance();

                            speaker.Transfer(PawnTransferReason.Deserted, null, PawnTitleType.None, null, null, true);
                            if (speaker.party.place != null)
                            {
                                speaker.party.place.RemoveParty(speaker.party, true);
                            }
                            speaker.character.Damage(null, 10000f);

                            var km = Manager<KingdomManager>.Instance;
                            Pawn newSpeaker = km.GeneratePawn(speaker, 1, race: speaker.character.type); 
                            newSpeaker.SetPosition(km.playerParty.place.pos);
                            newSpeaker.Transfer(PawnTransferReason.Joined, km.playerRival, PawnTitleType.Noble, null, null, false);
                            km.Chat(km.playerRival.ruler, newSpeaker, true, 10, Pawn.LightTopics.OpinionsAndRelationships);
                            newSpeaker.Transfer(PawnTransferReason.Deserted, null, PawnTitleType.None, km.playerParty, null, false);
                            km.playerParty.place.AddToPlace(newSpeaker, true);
                            newSpeaker.character.PasteAppearance(appearance);
                            newSpeaker.UpdateProfession();
                            newSpeaker.RollAttractions();
                            dioramaBehavior.context.SetValue(key, newSpeaker);
                            Manager<UIManager>.Instance.PlayAudio(Manager<AudioManager>.Instance.global.GetHandle("Reroll"), 1f, OctoberMath.DistributedRandom(0.95f, 1.05f, 2f), 0f);
                            Manager<EffectsManager>.Instance.InstanceEffect("IntroReroll", new EffectPositioning(dioramaCharacterInstance.behavior.transform.position, new Rotator(0f, -90f, 0f)));
                            dioramaCharacterInstance.SetSpeaker(newSpeaker);
                        }
                        else
                        {
                            ManagerBehavior.Instance.scenes.gameObject.SetActive(false);
                            ManagerBehavior.Instance.center.gameObject.SetActive(true);

                            var typeEditor = Manager<UIManager>.Instance.AddWindow(ManagerBehavior.Instance.center);
                            manager.SetPrivateField<UIWindowBehavior>("typeEditor", typeEditor);
                            typeEditor.InstanceContents<UICharacterPersonalityEditorWindowContents>().Initialize(ref speaker);

                            while (manager.GetPrivateField<UIWindowBehavior>("typeEditor") != null)
                            {
                                await System.Threading.Tasks.Task.Yield();
                            }
                            ManagerBehavior.Instance.scenes.gameObject.SetActive(true);
                            ManagerBehavior.Instance.center.gameObject.SetActive(false);
                            speaker.UpdateProfession();
                            dioramaCharacterInstance.SetSpeaker(speaker);
                        }
                        dioramaBehavior.UpdateUI();
                    }
                }
                RollingCharacter = false; 
            }
        }
    }
}
