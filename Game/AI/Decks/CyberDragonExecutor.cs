using System;
using System.Collections.Generic;
using System.Linq;
using YGOSharp.OCGWrapper.Enums;
using WindBot;
using WindBot.Game;
using WindBot.Game.AI;

namespace WindBot.Game.AI.Decks
{
    [Deck("CyberDragon", "AI_CyberDragon")]
    public class CyberDragonExecutor : DefaultExecutor
    {
        bool _machineDupUsed;
        bool _galaxySoldierUsed;
        bool _overloadFusionUsed;
        bool _powerBondUsed;
        bool _limiterRemovalUsed;
        bool _overflowUsed;
        bool _coreSearched;
        bool _emergencyUsed;
        bool _siegerSummoned;
        bool _novaSummoned;
        int _infinityAbsorbTarget = 0;
        int _fusionTargetId = 0;
        int _contactFusionTargetId = 0;
        int _turn;

        public class CardId
        {
            public const int CyberDragon = 70095154;
            public const int CyberDragonCore = 23893227;
            public const int CyberDragonHerz = 56364287;
            public const int CyberDragonNachster = 1142880;
            public const int CyberDragonVier = 29975188;
            public const int CyberDragonDrei = 59281922;
            public const int GalaxySoldier = 46659709;
            public const int MachineDuplication = 63995093;
            public const int CyberEmergency = 60600126;
            public const int CyberRepairPlant = 86686671;
            public const int OverloadFusion = 3659803;
            public const int PowerBond = 37630732;
            public const int LimiterRemoval = 23171610;
            public const int MonsterReborn = 83764719;
            public const int CyberneticOverflow = 82428674;
            public const int ChimeratechFortressDragon = 79229522;
            public const int ChimeratechMegafleetDragon = 87116928;
            public const int ChimeratechRampageDragon = 84058253;
            public const int CyberDragonSieger = 46724542;
            public const int CyberDragonNova = 58069384;
            public const int CyberDragonInfinity = 10443957;
            public const int CyberTwinDragon = 74157028;
            public const int CyberEndDragon = 1546123;
            public const int ChimeratechOverdragon = 64599569;

            public static readonly int[] CyberDragonNamedMonsters = {
                CyberDragon, CyberDragonCore, CyberDragonHerz, CyberDragonNachster,
                CyberDragonVier, CyberDragonDrei, CyberDragonSieger
            };
        }

        public CyberDragonExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // ----- Hand traps & disruption -----
            AddExecutor(ExecutorType.Activate, _CardId.AshBlossom, DefaultAshBlossomAndJoyousSpring);
            AddExecutor(ExecutorType.Activate, _CardId.MaxxC, DefaultMaxxC);
            AddExecutor(ExecutorType.Activate, _CardId.CalledByTheGrave, DefaultCalledByTheGrave);
            AddExecutor(ExecutorType.Activate, _CardId.InfiniteImpermanence, DefaultInfiniteImpermanence);

            // ----- Search & extenders -----
            AddExecutor(ExecutorType.Activate, CardId.CyberEmergency, CyberEmergencyActivate);
            AddExecutor(ExecutorType.Activate, CardId.CyberRepairPlant, CyberRepairPlantActivate);
            AddExecutor(ExecutorType.Activate, CardId.MachineDuplication, MachineDuplicationActivate);
            AddExecutor(ExecutorType.Activate, CardId.GalaxySoldier, GalaxySoldierEffect);
            AddExecutor(ExecutorType.Activate, CardId.CyberDragonHerz, HerzDiscardSearch);
            AddExecutor(ExecutorType.Activate, CardId.CyberDragonNachster, NachsterSpecialSummon);
            AddExecutor(ExecutorType.Activate, CardId.CyberDragonVier, VierSpecialSummon);

            // ----- Fusion / OTK spells -----
            AddExecutor(ExecutorType.Activate, CardId.OverloadFusion, OverloadFusionActivate);
            AddExecutor(ExecutorType.Activate, CardId.PowerBond, PowerBondActivate);
            AddExecutor(ExecutorType.Activate, CardId.MonsterReborn, MonsterRebornActivate);
            AddExecutor(ExecutorType.Activate, CardId.LimiterRemoval, LimiterRemovalActivate);

            // ----- Removal -----
            AddExecutor(ExecutorType.Activate, _CardId.Raigeki, DefaultRaigeki);
            AddExecutor(ExecutorType.Activate, _CardId.CosmicCyclone, DefaultCosmicCyclone);
            AddExecutor(ExecutorType.Activate, CardId.CyberneticOverflow, CyberneticOverflowActivate);

            // ----- Normal summons -----
            AddExecutor(ExecutorType.Summon, CardId.CyberDragonCore, CoreSummon);
            AddExecutor(ExecutorType.Summon, CardId.CyberDragonDrei, DreiSummon);
            AddExecutor(ExecutorType.Summon, CardId.CyberDragonHerz, HerzSummon);
            AddExecutor(ExecutorType.Summon, CardId.CyberDragonVier, VierSummon);
            AddExecutor(ExecutorType.Summon, CardId.CyberDragonNachster, NachsterSummon);
            AddExecutor(ExecutorType.Summon, CardId.CyberDragon, CyberDragonSummon);

            // ----- Level up effects -----
            AddExecutor(ExecutorType.Activate, CardId.CyberDragonHerz, HerzLevelUp);

            // ----- Extra deck summons -----
            AddExecutor(ExecutorType.SpSummon, _CardId.JizukirutheStarDestroyingKaiju, DefaultKaijuSpsummon);
            AddExecutor(ExecutorType.SpSummon, CardId.ChimeratechFortressDragon, FortressSummon);
            AddExecutor(ExecutorType.SpSummon, CardId.ChimeratechMegafleetDragon, MegafleetSummon);
            AddExecutor(ExecutorType.SpSummon, CardId.CyberDragonSieger, SiegerSummon);
            AddExecutor(ExecutorType.SpSummon, CardId.CyberDragonNova, NovaSummon);
            AddExecutor(ExecutorType.SpSummon, CardId.CyberDragonInfinity, InfinitySummon);

            // ----- Monster effect activations -----
            AddExecutor(ExecutorType.Activate, CardId.CyberDragonNova, NovaReviveEffect);
            AddExecutor(ExecutorType.Activate, CardId.CyberDragonInfinity, InfinityAbsorbEffect);

            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
        }

        public override void OnNewTurn()
        {
            base.OnNewTurn();
            _turn++;
            _machineDupUsed = false;
            _galaxySoldierUsed = false;
            _overloadFusionUsed = false;
            _powerBondUsed = false;
            _limiterRemovalUsed = false;
            _overflowUsed = false;
            _coreSearched = false;
            _emergencyUsed = false;
            _siegerSummoned = false;
            _novaSummoned = false;
            _infinityAbsorbTarget = 0;
            _fusionTargetId = 0;
            _contactFusionTargetId = 0;
        }

        // ================================ HELPERS ================================

        private bool IsMainPhase()
        {
            return Duel.Player == 0 && (Duel.Phase == DuelPhase.Main1 || Duel.Phase == DuelPhase.Main2);
        }

        private bool IsCyberDragonNamed(ClientCard card)
        {
            if (card == null) return false;
            return card.IsCode(CardId.CyberDragonNamedMonsters);
        }

        private bool HasCDOnField()
        {
            return Bot.GetMonsters().Any(c => c.IsFaceup() && IsCyberDragonNamed(c));
        }

        private int CountCDNamedInLocations(IEnumerable<ClientCard> list)
        {
            int count = 0;
            foreach (ClientCard c in list)
            {
                if (c == null) continue;
                // Support monsters only get the "Cyber Dragon" name on the field or in the GY.
                if (c.Location == CardLocation.Hand)
                {
                    if (c.IsCode(CardId.CyberDragon)) count++;
                }
                else
                {
                    if (IsCyberDragonNamed(c)) count++;
                }
            }
            return count;
        }

        private bool CanMakeNova()
        {
            return Bot.GetMonsters().Count(c => c.IsFaceup() && c.Level == 5) >= 2;
        }

        private bool InfinityAvailable()
        {
            return Bot.HasInExtra(CardId.CyberDragonInfinity);
        }

        // Rough lethal estimate: can we deal >= enemy LP (considering enemy monsters we can attack over)?
        private bool CanDealLethal(int extraAttack, int enemyBlockingPower)
        {
            int myPower = 0;
            foreach (ClientCard monster in Bot.GetMonsters())
            {
                if (monster.IsFaceup() && monster.CanDirectAttack && !monster.IsDisabled())
                    myPower += monster.Attack;
            }
            int net = myPower + extraAttack - enemyBlockingPower;
            return net >= Enemy.LifePoints;
        }

        // ================================ HAND TRAPS ================================

        // ================================ SEARCH & EXTENDERS ================================

        private bool CyberEmergencyActivate()
        {
            if (Duel.Player != 0 || !(Duel.Phase == DuelPhase.Main1 || Duel.Phase == DuelPhase.Main2))
                return false;
            // Only useful if it advances our plan (search extenders).
            if (_emergencyUsed && Bot.HasInHand(CardId.CyberDragonHerz))
                return false;
            if (Bot.HasInHand(CardId.CyberDragonCore) && !_coreSearched && Bot.GetMonsterCount() == 0)
                return true;   // get another body / continue
            if (Bot.HasInHand(CardId.MachineDuplication) && !_machineDupUsed)
                return true;   // set up Herz for Machine Duplication
            if (Bot.GetMonsterCount() == 0 && Enemy.GetMonsterCount() > 0)
                return true;   // need an out
            if (Bot.HasInHand(CardId.PowerBond) || Bot.HasInHand(CardId.OverloadFusion))
                return true;   // fuel for fusion
            return !_emergencyUsed && Bot.GetMonsterCount() >= 1;
        }

        private bool CyberRepairPlantActivate()
        {
            if (!IsMainPhase()) return false;
            // Needs "Cyber Dragon" (the card) in GY.
            if (!Bot.HasInGraveyard(CardId.CyberDragon)) return false;
            if (_emergencyUsed && _machineDupUsed) return false;
            // revive/search useful only when we can extend or need a body
            return Bot.GetMonsterCount() < 2 || Bot.HasInHand(CardId.MachineDuplication);
        }

        private bool MachineDuplicationActivate()
        {
            if (!IsMainPhase()) return false;
            if (_machineDupUsed) return false;
            // Need a face-up Machine with <= 500 ATK (Core 400, Herz 100, Nachster 200).
            return Bot.GetMonsters().Any(c => c.IsFaceup() && c.HasRace(CardRace.Machine) && c.Attack <= 500 && !c.IsDisabled());
        }

        private bool GalaxySoldierEffect()
        {
            if (!IsMainPhase()) return false;
            if (_galaxySoldierUsed) return false;
            // Discard a LIGHT monster to special summon Galaxy Soldier (Level 5) and search another copy.
            // Don't spend a Cyber Dragon we still need for fusion.
            if (!Bot.Hand.Any(c => c.IsMonster() && c.HasAttribute(CardAttribute.Light) && !c.IsCode(CardId.GalaxySoldier)))
                return false;
            if (Bot.Hand.Count <= 1) return false;
            // Only go for it if it leads to Nova/Infinity or we need a body.
            if (Enemy.GetMonsterCount() == 0 && Bot.GetMonsterCount() == 0 && Bot.Hand.Count < 3)
                return false;
            if (Bot.HasInHand(CardId.PowerBond) && CountCDNamedInLocations(Bot.Hand) + CountCDNamedInLocations(Bot.GetMonsters()) >= 3)
                return false; // keep the Cyber Dragons for Power Bond
            return true;
        }

        private bool HerzDiscardSearch()
        {
            // Discard Herz from hand: add 1 "Cyber Dragon" monster from Deck to hand.
            if (Card.Location != CardLocation.Hand) return false;
            if (!IsMainPhase()) return false;
            if (Bot.HasInHand(CardId.MachineDuplication) && !_machineDupUsed)
                return true;   // get a Machine Duplication target (Herz itself is the target, so pick Core/CD)
            if (Bot.GetMonsterCount() == 0 && Bot.HasInHand(CardId.CyberDragonCore))
                return false;  // rather normal summon Core
            return !_coreSearched || !Bot.HasInHand(CardId.CyberDragon);
        }

        private bool NachsterSpecialSummon()
        {
            // Discard 1 other monster; Special Summon this card from hand.
            if (Card.Location != CardLocation.Hand) return false;
            if (!IsMainPhase()) return false;
            if (Bot.Hand.Count <= 1) return false;
            ClientCard discardable = Bot.Hand.FirstOrDefault(c => c.IsMonster() && !c.IsCode(CardId.CyberDragonNachster));
            if (discardable == null) return false;
            // Useful when we need a body or can revive something strong from GY.
            if (Bot.Graveyard.Any(c => c.HasRace(CardRace.Machine) && (c.Attack == 2100 || c.Defense == 2100)))
                return true;
            if (Bot.GetMonsterCount() == 0 && Enemy.GetMonsterCount() > 0)
                return true;
            if (Bot.HasInHand(CardId.MachineDuplication) && !_machineDupUsed)
                return true;
            return false;
        }

        private bool VierSpecialSummon()
        {
            // If you Normal/Special Summon "Cyber Dragon", Special Summon this from hand.
            if (Card.Location != CardLocation.Hand) return false;
            if (Bot.HasInHand(CardId.PowerBond) && CountCDNamedInLocations(Bot.Hand) + CountCDNamedInLocations(Bot.GetMonsters()) >= 3)
                return false;
            return Bot.GetMonsters().Any(c => c.IsFaceup() && c.IsCode(CardId.CyberDragon));
        }

        // ================================ FUSION / OTK SPELLS ================================

        private bool OverloadFusionActivate()
        {
            if (!IsMainPhase()) return false;
            if (_overloadFusionUsed) return false;
            // Need materials: at least 1 "Cyber Dragon" + 1+ Machine monsters in field/GY.
            int cdCount = CountCDNamedInLocations(Bot.Graveyard) + CountCDNamedInLocations(Bot.GetMonsters());
            int machineCount = Bot.Graveyard.Count(c => c.HasRace(CardRace.Machine)) + Bot.GetMonsters().Count(c => c.HasRace(CardRace.Machine));
            if (cdCount < 2 && !(cdCount >= 1 && machineCount >= 2)) return false;
            if (Enemy.GetMonsterCount() == 0 && Enemy.GetSpells().Count(c => c.IsFaceup()) == 0)
                return false;
            // Use it to clear the board / finish.
            return true;
        }

        private bool PowerBondActivate()
        {
            if (!IsMainPhase()) return false;
            if (_powerBondUsed) return false;
            // Need 2+ "Cyber Dragon" monsters in hand/field for Cyber Twin/End.
            int cdHandField = CountCDNamedInLocations(Bot.Hand) + CountCDNamedInLocations(Bot.GetMonsters());
            if (cdHandField < 2) return false;
            int enemyBlocking = 0;
            foreach (ClientCard monster in Enemy.GetMonsters())
            {
                if (monster.IsFaceup())
                    enemyBlocking += Math.Min(monster.GetDefensePower(), monster.Attack);
            }
            // Cyber End (8000) or Twin (5600 x2).
            int fusionPower = (cdHandField >= 3) ? 8000 : 5600;
            if (CanDealLethal(fusionPower, enemyBlocking))
                return true;
            // Fall back: summon a wall/attacker if enemy has a clear board and we're ahead.
            if (Enemy.GetMonsterCount() == 0 && enemyBlocking == 0)
                return Bot.LifePoints > 4000;
            return false;
        }

        private bool MonsterRebornActivate()
        {
            if (!IsMainPhase()) return false;
            ClientCard best = Bot.Graveyard.FirstOrDefault(c => c.IsCanRevive() && c.IsMonster() &&
                (c.IsCode(CardId.CyberDragon, CardId.CyberDragonNova, CardId.GalaxySoldier)));
            if (best == null) return false;
            if (Enemy.GetMonsterCount() > 0 && Bot.GetMonsters().All(c => !c.IsFaceup() || c.Attack < 1500))
                return true;
            if (CountCDNamedInLocations(Bot.Hand) + CountCDNamedInLocations(Bot.GetMonsters()) >= 2)
                return false; // keep GY Cyber Dragon for fusion fodder
            return Bot.GetMonsterCount() < 2;
        }

        private bool LimiterRemovalActivate()
        {
            // Quick-play; use at battle start to finish.
            if (Duel.Phase != DuelPhase.BattleStart && Duel.Phase != DuelPhase.Battle)
                return false;
            if (_limiterRemovalUsed) return false;
            List<ClientCard> myMachines = Bot.GetMonsters().Where(c => c.IsFaceup() && c.HasRace(CardRace.Machine) && !c.IsDisabled()).ToList();
            if (myMachines.Count == 0) return false;
            int myPower = myMachines.Sum(c => c.Attack) * 2;
            int enemyBlocking = 0;
            foreach (ClientCard monster in Enemy.GetMonsters())
            {
                if (monster.IsFaceup())
                    enemyBlocking += Math.Min(monster.GetDefensePower(), monster.Attack);
            }
            if (myPower > enemyBlocking && myPower - enemyBlocking >= Enemy.LifePoints)
                return true;
            // Rampage / strong monsters can also force lethal.
            if (myMachines.Any(c => c.Attack * 2 >= 3000) && myPower > enemyBlocking && myPower - enemyBlocking >= Enemy.LifePoints - 1000)
                return true;
            return false;
        }

        // ================================ REMOVAL ================================

        private bool CyberneticOverflowActivate()
        {
            // Banish "Cyber Dragon" monsters with different Levels from hand/field/GY, destroy equal cards.
            if (!HasCDOnField()) return false;
            if (_overflowUsed) return false;
            int cdInHand = CountCDNamedInLocations(Bot.Hand);
            int cdOnField = CountCDNamedInLocations(Bot.GetMonsters());
            int cdInGY = CountCDNamedInLocations(Bot.Graveyard);
            int maxDestroy = Math.Min(2, cdInHand + cdOnField + cdInGY);
            int enemyThreats = Enemy.GetMonsters().Count(c => c.IsFaceup() && c.IsMonsterDangerous());
            enemyThreats += Enemy.GetSpells().Count(c => c.IsFaceup() && (c.IsFloodgate() || c.HasType(CardType.Continuous)));
            if (maxDestroy == 0) return false;
            if (Duel.Phase >= DuelPhase.BattleStart && Duel.Phase <= DuelPhase.Battle)
                return false;
            if (enemyThreats >= 2) return true;
            if (Enemy.GetMonsterCount() >= 2 && maxDestroy >= 2) return true;
            return false;
        }

        // ================================ NORMAL SUMMONS ================================

        private bool CoreSummon()
        {
            if (!IsMainPhase()) return false;
            // Priority normal summon to get the search.
            if (Bot.HasInHand(CardId.MachineDuplication) && !_machineDupUsed)
                return true;
            if (Enemy.GetMonsterCount() > 0 && Bot.GetMonsterCount() == 0)
                return true;
            return true;
        }

        private bool DreiSummon()
        {
            if (!IsMainPhase()) return false;
            // Drei becomes "Cyber Dragon" on field -> fusion material / Nova.
            if (Bot.HasInHand(CardId.PowerBond) || Bot.HasInHand(CardId.OverloadFusion))
                return true;
            if (CanMakeNova() && InfinityAvailable())
                return true;
            return Bot.GetMonsterCount() == 0;
        }

        private bool HerzSummon()
        {
            if (!IsMainPhase()) return false;
            return Bot.HasInHand(CardId.MachineDuplication) && !_machineDupUsed;
        }

        private bool VierSummon()
        {
            if (!IsMainPhase()) return false;
            return Bot.GetMonsterCount() == 0 && Enemy.GetMonsterCount() > 0;
        }

        private bool NachsterSummon()
        {
            if (!IsMainPhase()) return false;
            return Bot.GetMonsterCount() == 0;
        }

        private bool CyberDragonSummon()
        {
            if (!IsMainPhase()) return false;
            // Keep it for Machine Duplication? (not a valid target, 2100 ATK). Use as a body/attacker.
            if (Bot.HasInHand(CardId.MachineDuplication) && !_machineDupUsed && !Bot.HasInHand(new[] { CardId.CyberDragonCore, CardId.CyberDragonHerz }))
                return false; // save the normal summon for a machine-duplication target
            return Bot.GetMonsterCount() == 0 || CountCDNamedInLocations(Bot.GetMonsters()) >= 2;
        }

        // ================================ LEVEL UP EFFECTS ================================

        private bool HerzLevelUp()
        {
            // If Special Summoned: make this card Level 5.
            if (Card.Location != CardLocation.MonsterZone) return false;
            if (!Card.IsSpecialSummoned) return false;
            return CanMakeNova() || (Duel.Player == 0 && Duel.Phase == DuelPhase.Main1 || Duel.Phase == DuelPhase.Main2);
        }

        // ================================ EXTRA DECK SUMMONS ================================

        private bool FortressSummon()
        {
            // "Cyber Dragon" + 1+ Machine monsters from either field.
            if (!IsMainPhase()) return false;
            ClientCard enemyMachine = Enemy.GetMonsters().FirstOrDefault(c => c.IsFaceup() && c.HasRace(CardRace.Machine));
            if (enemyMachine == null) return false;
            if (!Bot.GetMonsters().Any(c => c.IsFaceup() && IsCyberDragonNamed(c))) return false;
            _contactFusionTargetId = CardId.ChimeratechFortressDragon;
            return true;
        }

        private bool MegafleetSummon()
        {
            // 1 "Cyber Dragon" monster + 1+ monsters in the Extra Monster Zone.
            if (!IsMainPhase()) return false;
            if (Enemy.GetMonstersInExtraZone().Count == 0) return false;
            ClientCard cd = Bot.GetMonsters().FirstOrDefault(c => c.IsFaceup() && IsCyberDragonNamed(c));
            if (cd == null && !Bot.Hand.Any(c => IsCyberDragonNamed(c) && c.IsMonster())) return false;
            _contactFusionTargetId = CardId.ChimeratechMegafleetDragon;
            return true;
        }

        private bool SiegerSummon()
        {
            // 2 Machine monsters, including "Cyber Dragon".
            if (!IsMainPhase()) return false;
            if (_siegerSummoned) return false;
            List<ClientCard> machines = Bot.GetMonsters().Where(c => c.IsFaceup() && c.HasRace(CardRace.Machine)).ToList();
            if (machines.Count < 2) return false;
            if (!machines.Any(c => IsCyberDragonNamed(c))) return false;
            _siegerSummoned = true;
            return true;
        }

        private bool NovaSummon()
        {
            // 2 Level 5 Machine monsters.
            if (!IsMainPhase()) return false;
            if (_novaSummoned) return false;
            if (!CanMakeNova()) return false;
            if (!InfinityAvailable()) return false;
            _novaSummoned = true;
            return true;
        }

        private bool InfinitySummon()
        {
            // Xyz Summon using "Cyber Dragon Nova".
            if (!IsMainPhase()) return false;
            return Bot.HasInMonstersZone(CardId.CyberDragonNova, true);
        }

        // ================================ MONSTER EFFECTS ================================

        private bool NovaReviveEffect()
        {
            // Detach 1 material: Special Summon 1 "Cyber Dragon" from GY.
            if (Card.Location != CardLocation.MonsterZone) return false;
            if (!Bot.Graveyard.Any(c => c.IsCode(CardId.CyberDragon) && c.IsCanRevive())) return false;
            return Bot.GetMonsterCount() < 5;
        }

        private bool InfinityAbsorbEffect()
        {
            // Attach an Attack Position opponent monster as material.
            if (Card.Location != CardLocation.MonsterZone) return false;
            ClientCard target = Enemy.GetMonsters().FirstOrDefault(c => c.IsFaceup() && c.IsAttack() && !c.IsShouldNotBeTarget());
            if (target == null) return false;
            if (Duel.Player == 1 && Duel.Phase == DuelPhase.Battle)
                return false;
            _infinityAbsorbTarget = target.Id;
            return true;
        }

        // ================================ CARD SELECTION ================================

        public override IList<ClientCard> OnSelectCard(IList<ClientCard> cards, int min, int max, long hint, bool cancelable)
        {
            ChainInfo currentSolvingChain = Duel.GetCurrentSolvingChainInfo();

            if (hint == HintMsg.AddToHand && currentSolvingChain != null)
            {
                List<ClientCard> result = null;
                if (currentSolvingChain.IsCode(CardId.CyberEmergency))
                {
                    result = PickEmergencySearch(cards);
                }
                else if (currentSolvingChain.IsCode(CardId.CyberDragonCore))
                {
                    result = PickCoreSearch(cards);
                }
                else if (currentSolvingChain.IsCode(CardId.CyberRepairPlant))
                {
                    result = PickRepairPlantSearch(cards);
                }
                else if (currentSolvingChain.IsCode(CardId.GalaxySoldier))
                {
                    result = cards.Where(c => c.IsCode(CardId.GalaxySoldier)).ToList();
                }
                else if (currentSolvingChain.IsCode(CardId.CyberDragonHerz))
                {
                    result = PickHerzSearch(cards);
                }
                if (result != null && result.Count >= min)
                    return Util.CheckSelectCount(result, cards, min, max);
            }

            if (hint == HintMsg.Target && currentSolvingChain != null)
            {
                if (currentSolvingChain.IsCode(CardId.MachineDuplication))
                {
                    ClientCard target = cards.FirstOrDefault(c => c.Controller == 0 && c.IsFaceup() &&
                        c.HasRace(CardRace.Machine) && c.Attack <= 500 && !c.IsDisabled());
                    if (target != null)
                    {
                        // Prefer Herz / Nachster (they become Level 5) over Core.
                        ClientCard better = cards.FirstOrDefault(c => c.Controller == 0 && c.IsFaceup() &&
                            c.HasRace(CardRace.Machine) && c.Attack <= 500 && !c.IsDisabled() &&
                            !c.IsCode(CardId.CyberDragonCore));
                        if (better != null) target = better;
                        return Util.CheckSelectCount(new List<ClientCard> { target }, cards, min, max);
                    }
                }
                else if (currentSolvingChain.IsCode(CardId.MonsterReborn))
                {
                    ClientCard target = cards.Where(c => c.IsCanRevive()).OrderByDescending(c => c.Attack)
                        .FirstOrDefault(c => c.IsCode(CardId.CyberDragon, CardId.CyberDragonNova, CardId.GalaxySoldier));
                    if (target == null) target = cards.Where(c => c.IsCanRevive()).OrderByDescending(c => c.Attack).FirstOrDefault();
                    if (target != null)
                        return Util.CheckSelectCount(new List<ClientCard> { target }, cards, min, max);
                }
                else if (currentSolvingChain.IsCode(CardId.CyberDragonNova))
                {
                    ClientCard target = cards.FirstOrDefault(c => c.IsCode(CardId.CyberDragon));
                    if (target == null) target = cards.FirstOrDefault(c => IsCyberDragonNamed(c));
                    if (target != null)
                        return Util.CheckSelectCount(new List<ClientCard> { target }, cards, min, max);
                }
                else if (currentSolvingChain.IsCode(CardId.CyberDragonInfinity) && _infinityAbsorbTarget != 0)
                {
                    ClientCard target = cards.FirstOrDefault(c => c.Id == _infinityAbsorbTarget);
                    if (target == null) target = cards.FirstOrDefault(c => c.Controller == 1 && c.IsAttack() && !c.IsShouldNotBeTarget());
                    if (target != null)
                    {
                        _infinityAbsorbTarget = 0;
                        return Util.CheckSelectCount(new List<ClientCard> { target }, cards, min, max);
                    }
                }
            }

            if (hint == HintMsg.ToGrave && currentSolvingChain != null)
            {
                if (currentSolvingChain.IsCode(CardId.GalaxySoldier))
                {
                    // discard a LIGHT monster, prefer the least useful.
                    ClientCard discard = cards.OrderBy(c => c.IsCode(CardId.CyberDragon) ? 1 : 0).FirstOrDefault(c => c.IsMonster());
                    if (discard != null)
                        return Util.CheckSelectCount(new List<ClientCard> { discard }, cards, min, max);
                }
            }

            if (hint == HintMsg.FUSION)
            {
                List<ClientCard> ordered = OrderFusionTargets(cards);
                if (ordered.Count >= min)
                    return Util.CheckSelectCount(ordered, cards, min, max);
            }

            if (hint == HintMsg.XyzMaterial && currentSolvingChain == null)
            {
                // Nova: prefer Cyber Dragon + Galaxy Soldier, then any Level 5 machines.
                List<ClientCard> selected = cards.Where(c => c.Controller == 0 && c.Level == 5).ToList();
                if (selected.Count >= min)
                    return Util.CheckSelectCount(selected, cards, min, max);
            }

            return base.OnSelectCard(cards, min, max, hint, cancelable);
        }

        private List<ClientCard> PickEmergencySearch(IList<ClientCard> cards)
        {
            // Cyber Emergency -> add 1 "Cyber Dragon" monster.
            if (Bot.HasInHand(CardId.MachineDuplication) && !_machineDupUsed)
            {
                ClientCard herz = cards.FirstOrDefault(c => c.IsCode(CardId.CyberDragonHerz));
                if (herz != null) return new List<ClientCard> { herz };
            }
            if (!_coreSearched && !Bot.HasInHand(CardId.CyberDragonCore))
            {
                ClientCard core = cards.FirstOrDefault(c => c.IsCode(CardId.CyberDragonCore));
                if (core != null) return new List<ClientCard> { core };
            }
            if (Bot.HasInHand(CardId.PowerBond) || Bot.HasInHand(CardId.OverloadFusion))
            {
                ClientCard cd = cards.FirstOrDefault(c => c.IsCode(CardId.CyberDragon));
                if (cd != null) return new List<ClientCard> { cd };
            }
            ClientCard herz2 = cards.FirstOrDefault(c => c.IsCode(CardId.CyberDragonHerz));
            if (herz2 != null) return new List<ClientCard> { herz2 };
            ClientCard drei = cards.FirstOrDefault(c => c.IsCode(CardId.CyberDragonDrei));
            if (drei != null) return new List<ClientCard> { drei };
            ClientCard any = cards.FirstOrDefault();
            return any != null ? new List<ClientCard> { any } : null;
        }

        private List<ClientCard> PickCoreSearch(IList<ClientCard> cards)
        {
            // Cyber Dragon Core -> add 1 "Cyber" Spell/Trap.
            if (!Bot.HasInHand(CardId.CyberEmergency) && !_emergencyUsed)
            {
                ClientCard em = cards.FirstOrDefault(c => c.IsCode(CardId.CyberEmergency));
                if (em != null) return new List<ClientCard> { em };
            }
            if (Bot.HasInGraveyard(CardId.CyberDragon) && !Bot.HasInHand(CardId.CyberRepairPlant))
            {
                ClientCard rp = cards.FirstOrDefault(c => c.IsCode(CardId.CyberRepairPlant));
                if (rp != null) return new List<ClientCard> { rp };
            }
            ClientCard overflow = cards.FirstOrDefault(c => c.IsCode(CardId.CyberneticOverflow));
            if (overflow != null) return new List<ClientCard> { overflow };
            ClientCard any = cards.FirstOrDefault();
            return any != null ? new List<ClientCard> { any } : null;
        }

        private List<ClientCard> PickRepairPlantSearch(IList<ClientCard> cards)
        {
            // Add 1 "Cyber Dragon" monster.
            if (Bot.HasInHand(CardId.MachineDuplication) && !_machineDupUsed)
            {
                ClientCard herz = cards.FirstOrDefault(c => c.IsCode(CardId.CyberDragonHerz));
                if (herz != null) return new List<ClientCard> { herz };
            }
            if (!_coreSearched && !Bot.HasInHand(CardId.CyberDragonCore))
            {
                ClientCard core = cards.FirstOrDefault(c => c.IsCode(CardId.CyberDragonCore));
                if (core != null) return new List<ClientCard> { core };
            }
            ClientCard cd = cards.FirstOrDefault(c => c.IsCode(CardId.CyberDragon));
            if (cd != null) return new List<ClientCard> { cd };
            ClientCard any = cards.FirstOrDefault();
            return any != null ? new List<ClientCard> { any } : null;
        }

        private List<ClientCard> PickHerzSearch(IList<ClientCard> cards)
        {
            // Herz discard -> add 1 "Cyber Dragon" monster.
            if (Bot.HasInHand(CardId.MachineDuplication) && !_machineDupUsed)
            {
                ClientCard herz = cards.FirstOrDefault(c => c.IsCode(CardId.CyberDragonHerz));
                if (herz != null) return new List<ClientCard> { herz };
            }
            if (Bot.HasInHand(CardId.PowerBond) || Bot.HasInHand(CardId.OverloadFusion))
            {
                ClientCard cd = cards.FirstOrDefault(c => c.IsCode(CardId.CyberDragon));
                if (cd != null) return new List<ClientCard> { cd };
            }
            ClientCard drei = cards.FirstOrDefault(c => c.IsCode(CardId.CyberDragonDrei));
            if (drei != null) return new List<ClientCard> { drei };
            ClientCard any = cards.FirstOrDefault();
            return any != null ? new List<ClientCard> { any } : null;
        }

        private List<ClientCard> OrderFusionTargets(IList<ClientCard> cards)
        {
            // Prefer Cyber End > Cyber Twin > Rampage > Overdragon, based on available materials.
            ChainInfo chain = Duel.GetCurrentSolvingChainInfo();
            bool powerBond = chain != null && chain.IsCode(CardId.PowerBond);
            int cdHandField = CountCDNamedInLocations(Bot.Hand) + CountCDNamedInLocations(Bot.GetMonsters());
            int cdGY = CountCDNamedInLocations(Bot.Graveyard);

            List<int> priority = new List<int>();
            if (powerBond)
            {
                if (cdHandField >= 3) priority.Add(CardId.CyberEndDragon);
                if (cdHandField >= 2) priority.Add(CardId.CyberTwinDragon);
                priority.Add(CardId.ChimeratechRampageDragon);
                priority.Add(CardId.ChimeratechOverdragon);
            }
            else
            {
                if (cdGY + cdHandField >= 2) priority.Add(CardId.ChimeratechRampageDragon);
                priority.Add(CardId.ChimeratechOverdragon);
            }

            List<ClientCard> ordered = new List<ClientCard>();
            foreach (int id in priority)
            {
                ClientCard match = cards.FirstOrDefault(c => c.IsCode(id));
                if (match != null && !ordered.Contains(match))
                {
                    ordered.Add(match);
                    _fusionTargetId = id;
                }
            }
            foreach (ClientCard card in cards)
            {
                if (!ordered.Contains(card))
                    ordered.Add(card);
            }
            return ordered;
        }

        public override IList<ClientCard> OnSelectFusionMaterial(IList<ClientCard> cards, int min, int max)
        {
            ChainInfo chain = Duel.GetCurrentSolvingChainInfo();
            List<ClientCard> selected = new List<ClientCard>();

            // Contact fusions (Fortress / Megafleet) - no chain.
            if (chain == null && _contactFusionTargetId != 0)
            {
                if (_contactFusionTargetId == CardId.ChimeratechFortressDragon)
                {
                    ClientCard enemyMachine = cards.FirstOrDefault(c => c.Controller == 1 && c.HasRace(CardRace.Machine));
                    if (enemyMachine != null) selected.Add(enemyMachine);
                    ClientCard cd = cards.FirstOrDefault(c => c.Controller == 0 && IsCyberDragonNamed(c));
                    if (cd != null && !selected.Contains(cd)) selected.Add(cd);
                }
                else if (_contactFusionTargetId == CardId.ChimeratechMegafleetDragon)
                {
                    ClientCard enemyEmz = cards.FirstOrDefault(c => c.Controller == 1);
                    if (enemyEmz != null) selected.Add(enemyEmz);
                    ClientCard cd = cards.FirstOrDefault(c => c.Controller == 0 && IsCyberDragonNamed(c));
                    if (cd != null && !selected.Contains(cd)) selected.Add(cd);
                }
                _contactFusionTargetId = 0;
                return Util.CheckSelectCount(selected, cards, min, max);
            }

            // Overload Fusion: banish from GY first (keep field), prefer "Cyber Dragon" materials.
            if (chain != null && chain.IsCode(CardId.OverloadFusion))
            {
                List<ClientCard> cdGY = cards.Where(c => c.Location == CardLocation.Grave && IsCyberDragonNamed(c)).ToList();
                List<ClientCard> cdField = cards.Where(c => c.Location != CardLocation.Grave && IsCyberDragonNamed(c)).ToList();
                List<ClientCard> machines = cards.Where(c => c.HasRace(CardRace.Machine) && !IsCyberDragonNamed(c)).ToList();
                foreach (ClientCard c in cdGY) { if (selected.Count >= max) break; selected.Add(c); }
                foreach (ClientCard c in cdField) { if (selected.Count >= max) break; selected.Add(c); }
                foreach (ClientCard c in machines) { if (selected.Count >= max) break; selected.Add(c); }
                return Util.CheckSelectCount(selected, cards, min, max);
            }

            // Power Bond: use hand/field, prefer non-Cyber-Dragon machines, keep attackers.
            List<ClientCard> fieldCds = cards.Where(c => c.Location == CardLocation.MonsterZone && IsCyberDragonNamed(c)).ToList();
            List<ClientCard> handCds = cards.Where(c => c.Location == CardLocation.Hand && IsCyberDragonNamed(c)).ToList();
            List<ClientCard> otherMachines = cards.Where(c => c.HasRace(CardRace.Machine) && !IsCyberDragonNamed(c)).ToList();
            foreach (ClientCard c in otherMachines) { if (selected.Count >= max) break; selected.Add(c); }
            foreach (ClientCard c in handCds) { if (selected.Count >= max) break; selected.Add(c); }
            foreach (ClientCard c in fieldCds) { if (selected.Count >= max) break; selected.Add(c); }
            return Util.CheckSelectCount(selected, cards, min, max);
        }

        // ================================ OPTION SELECTION ================================

        public override int OnSelectOption(IList<long> options)
        {
            ChainInfo chain = Duel.GetCurrentSolvingChainInfo();
            if (chain != null && (chain.IsCode(CardId.CyberDragonHerz) || chain.IsCode(CardId.CyberDragonDrei)))
            {
                // "make Level 5" effect options. Choose the option that levels up.
                if (options.Count > 0)
                    return options.Count > 1 ? 1 : 0;
            }
            return base.OnSelectOption(options);
        }
    }
}
