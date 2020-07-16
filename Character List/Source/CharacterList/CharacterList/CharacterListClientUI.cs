
using Mohawk.SystemCore;
using System;
using System.Collections.Generic;
using TenCrowns.ClientCore;
using TenCrowns.GameCore;
using UnityEngine;

namespace CharacterList
{
    internal class CharacterListClientUI : ClientUI
    {
        public CharacterListClientUI(ClientManager pClientManager) : base(pClientManager)
        {
            return;
        }

        protected override void updateCharacters()
        {
            Player pActivePlayer = mManager.activePlayer();

            if (pActivePlayer.hasLeader())
            {
                UIAttributeTag leaderTag = mPlayerFamily.GetSubTag("-CurrentLeader");
                UIAttributeTag characterTabTag = ui.GetUIAttributeTag("TabPanel-Character");
                Character playerLeader = pActivePlayer.leader();
                updateCharacterData(leaderTag, playerLeader);
                leaderTag.IsActive = true;

                using (var cognomenSB = CollectionCache.GetStringBuilderScoped())
                {
                    HelpText.buildCognomenLink(cognomenSB.Value, pActivePlayer.leader().getCognomen(), pActivePlayer.leader());
                    mPlayerFamily.SetKey("CurrentLeader-Cognomen", cognomenSB.Value);
                }

                int iLivingSpouses = 0;
                for (int iLoopSpouse = 0; iLoopSpouse < playerLeader.getSpouses().Count; iLoopSpouse++)
                {
                    Character pLoopCharacter = Game.character(playerLeader.getSpouses()[iLoopSpouse]);

                    if (pLoopCharacter.isAlive())
                    {
                        UIAttributeTag leaderSpouseTag = characterTabTag.GetSubTag("-Spouse", iLivingSpouses);
                        int iOpinion = pActivePlayer.calculateCharacterOpinionRate(pLoopCharacter);
                        updateCharacterData(leaderSpouseTag, pLoopCharacter, iOpinion, pActivePlayer.hasCharacterOpinionValues(pLoopCharacter), Infos.Helpers.getOpinionCharacterFromRate(iOpinion));

                        UIAttributeTag spouseSlotTag = leaderTag.GetSubTag("-Spouse", iLivingSpouses);
                        updateCharacterSlotData(spouseSlotTag, pLoopCharacter, RoleType.SPOUSE, (pLoopCharacter != null));
                        iLivingSpouses++;
                    }
                }
                ui.SetUIAttribute("SpouseList-Count", Math.Max(iLivingSpouses, 1).ToStringCached());
                characterTabTag.SetBool("Spouse-IsActive", iLivingSpouses > 0);

                if (iLivingSpouses == 0)
                {
                    UIAttributeTag spouseSlotTag = leaderTag.GetSubTag("-Spouse", 0);
                    updateCharacterSlotData(spouseSlotTag, null, RoleType.SPOUSE, false);
                }

                //Heir
                {
                    Character pNextLeader = pActivePlayer.findHeir();
                    if (pNextLeader != null)
                    {
                        UIAttributeTag nextLeaderTag = characterTabTag.GetSubTag("-Heir");
                        int iOpinion = pActivePlayer.calculateCharacterOpinionRate(pNextLeader);
                        updateCharacterData(nextLeaderTag, pNextLeader, iOpinion, pActivePlayer.hasCharacterOpinionValues(pNextLeader), Infos.Helpers.getOpinionCharacterFromRate(iOpinion));
                        nextLeaderTag.IsActive = true;
                    }
                    UIAttributeTag heirSlotTag = leaderTag.GetSubTag("-Heir");
                    updateCharacterSlotData(heirSlotTag, pNextLeader, RoleType.HEIR, (pNextLeader != null));
                }

                //Council
                {
                    for (CouncilType eLoopCouncil = 0; eLoopCouncil < Infos.councilsNum(); eLoopCouncil++)
                    {
                        UIAttributeTag councilListTag = leaderTag.GetSubTag("-CouncilList", (int)eLoopCouncil);
                        Character pCharacter = pActivePlayer.councilCharacter(eLoopCouncil);

                        updateCharacterSlotData(councilListTag, pCharacter, RoleType.COUNCIL, pActivePlayer.isCouncilUnlock(eLoopCouncil), eLoopCouncil);
                    }
                    mPlayerFamily.SetInt("CurrentLeader-CouncilList-Count", (int)Infos.councilsNum());
                }

                //Other Characters
                using (var characterList = CollectionCache.GetListScoped<int>())
                {
                    bool showCharacters = false;
                    List<int> successionList = characterList.Value;
                    pActivePlayer.findSuccessionList(successionList);

                    //Heirs
                    {
                        int numHeirs = 0;
                        foreach (int iLoopHeir in successionList)
                        {
                            Character pLoopHeir = Game.character(iLoopHeir);

                            if (pLoopHeir.isAlive())
                            {
                                UIAttributeTag currentLeaderHeirListTag = characterTabTag.GetSubTag("-HeirList", numHeirs);
                                int iOpinion = pActivePlayer.calculateCharacterOpinionRate(pLoopHeir);
                                updateCharacterData(currentLeaderHeirListTag, pLoopHeir, iOpinion, pActivePlayer.hasCharacterOpinionValues(pLoopHeir), Infos.Helpers.getOpinionCharacterFromRate(iOpinion));
                                numHeirs++;
                            }
                        }
                        characterTabTag.SetInt("HeirList-Count", numHeirs);
                        characterTabTag.SetBool("HeirList-IsActive", numHeirs > 0);
                        showCharacters |= numHeirs > 0;
                    }

                    //Court
                    {
                        int numCourtiers = 0;
                        using (var charListScoped = CollectionCache.GetListScoped<int>())
                        {
                            pActivePlayer.getActiveCharacters(charListScoped.Value);

                            foreach (int iLoopCourtier in charListScoped.Value)
                            {
                                Character pLoopCourtier = Game.character(iLoopCourtier);

                                if (pLoopCourtier.isCourtier() && !pLoopCourtier.isLeaderOrSpouseOrHeir())
                                {
                                    UIAttributeTag currentLeaderCourtierListTag = characterTabTag.GetSubTag("-CourtierList", numCourtiers);
                                    int iOpinion = pActivePlayer.calculateCharacterOpinionRate(pLoopCourtier);
                                    updateCharacterData(currentLeaderCourtierListTag, pLoopCourtier, iOpinion, pActivePlayer.hasCharacterOpinionValues(pLoopCourtier), Infos.Helpers.getOpinionCharacterFromRate(iOpinion));
                                    numCourtiers++;
                                }
                            }
                        }
                        characterTabTag.SetInt("CourtierList-Count", numCourtiers);
                        characterTabTag.SetBool("CourtierList-IsActive", numCourtiers > 0);
                        showCharacters |= numCourtiers > 0;
                    }

                    //Others
                    {
                        int numOthers = 0;
                        using (var charListScoped = CollectionCache.GetListScoped<int>())
                        {
                            pActivePlayer.getActiveCharacters(charListScoped.Value);

                            foreach (int iLoopOthers in charListScoped.Value)
                            {
                                Character pLoopOthers = Game.character(iLoopOthers);

                                if (!pLoopOthers.isCourtier() && !pLoopOthers.isLeaderOrSpouseOrHeir() && !pLoopOthers.isSuccessor())
                                {
                                    UIAttributeTag currentLeaderOtherListTag = characterTabTag.GetSubTag("-OtherList", numOthers);
                                    int iOpinion = pActivePlayer.calculateCharacterOpinionRate(pLoopOthers);
                                    updateCharacterData(currentLeaderOtherListTag, pLoopOthers, iOpinion, pActivePlayer.hasCharacterOpinionValues(pLoopOthers), Infos.Helpers.getOpinionCharacterFromRate(iOpinion));
                                    numOthers++;
                                }
                            }
                        }
                        characterTabTag.SetInt("OtherList-Count", numOthers);
                        characterTabTag.SetBool("OtherList-IsActive", numOthers > 0);
                        showCharacters |= numOthers > 0;
                    }

                    mCharacters.IsActive = showCharacters;
                }
                mPlayerFamily.SetBool("CurrentNation-ShowNation", false);
            }
        }
    }
}
