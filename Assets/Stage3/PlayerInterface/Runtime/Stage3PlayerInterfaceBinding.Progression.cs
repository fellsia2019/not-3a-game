using Not3A.Stage2.Integration;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3PlayerInterfaceBinding
    {
        private Stage2HeroProgressionDriver heroProgression;
        private Stage3HeroProgressionEffects heroProgressionEffects;
        private Stage3HeroLevelUpPresentation heroLevelUp;
        private int observedHeroLevel;
        private Stage3HeroProgressionView progressionView;
        private bool blinkArmed, blinkConsumedPointer;
        public Stage2HeroProgressionDriver HeroProgression => heroProgression;
        public bool IsBlinkArmed => blinkArmed;

        private void BindHeroProgression()
        {
            var catalog = Resources.Load<Stage2HeroProgressionCatalog>("Stage3HeroProgression");
            if (catalog == null || !catalog.TryValidate(out _))
            { Debug.LogError("S3_HERO_PROGRESSION: missing or invalid Stage3HeroProgression data asset."); return; }
            heroProgression = GetComponent<Stage2HeroProgressionDriver>();
            if (heroProgression == null) heroProgression = gameObject.AddComponent<Stage2HeroProgressionDriver>();
            heroProgression.Configure(game, scene.PlayerMover, catalog);
            legacy?.BindHeroProgressionTools(heroProgression);
            heroProgression.ControlBlocked = () => !bound || !isActiveAndEnabled || !presentation.isActiveAndEnabled ||
                Blocked() || IsDraggingBuilding || HasBuildingDraft ||
                legacy != null && legacy.DebugOpen;
            heroProgressionEffects = GetComponent<Stage3HeroProgressionEffects>();
            if (heroProgressionEffects == null) heroProgressionEffects = gameObject.AddComponent<Stage3HeroProgressionEffects>();
            heroProgressionEffects.Initialize(scene.CameraFollow.BoundCamera, scene.PlayerMover.transform, presentation);
            heroLevelUp = GetComponent<Stage3HeroLevelUpPresentation>();
            if (heroLevelUp == null) heroLevelUp = gameObject.AddComponent<Stage3HeroLevelUpPresentation>();
            heroLevelUp.Initialize(presentation);
            observedHeroLevel = heroProgression.Rules.Level;
            progressionView = new Stage3HeroProgressionView {
                BlinkRanges = new float[catalog.BlinkMaxRank],
                BlinkManaCosts = new int[catalog.BlinkMaxRank],BlinkCooldowns=new float[catalog.BlinkMaxRank],
                Passives = new[] {
                    new Stage3HeroPassiveView { Title="Точный удар", Description="Урон по одной цели" },
                    new Stage3HeroPassiveView { Title="Быстрые руки", Description="Скорость атаки" },
                    new Stage3HeroPassiveView { Title="Лёгкий шаг", Description="Скорость движения" }
                }
            };
            for (var i = 0; i < catalog.BlinkMaxRank; i++)
            {
                progressionView.BlinkRanges[i]=catalog.BlinkDisplayRange(i+1);
                progressionView.BlinkManaCosts[i]=catalog.BlinkManaCost(i+1);
                progressionView.BlinkCooldowns[i]=catalog.BlinkRankCooldown(i+1);
            }
            presentation.SetHeroProgression(progressionView);
            presentation.BlinkArmRequested += ArmBlink;
            presentation.BlinkCancelRequested += CancelBlink;
            presentation.BlinkUpgradeRequested += UpgradeBlink;
            presentation.HeroPassiveUpgradeRequested += UpgradeHeroPassive;
            presentation.WindowChanged += CancelBlink;
            heroProgression.Blinked += OnBlinked;
            heroProgression.Rules.ExperienceGained += heroProgressionEffects.ShowExperience;
            heroProgression.Rules.LevelRaised += OnHeroLevelRaised;
            game.RunDriver.Run.Reset += ClearProgressionTransient;
            game.RunDriver.Run.Rewound += ClearProgressionTransient;
            game.RunDriver.Run.Changed += OnProgressionRunChanged;
            RefreshHeroProgression();
        }
        public void ArmBlink()
        {
            if (heroProgression == null || !heroProgression.CanCast || !heroProgression.Rules.CanBlink) return;
            ClearBuildingSelection(); game.Gathering.CancelGatherUntilRelease(); blinkArmed = true;
            RefreshHeroProgression();
        }
        public void CancelBlink()
        { blinkArmed = false; heroProgressionEffects?.SetBlinkAim(false, Vector2.zero, 0, false); }
        private void UpgradeBlink()
        {
            if (CanSpendHeroPoints) heroProgression.Rules.TryUpgradeBlink();
            RefreshHeroProgression();
        }
        private void UpgradeHeroPassive(int index)
        {
            if (index is >= 0 and < 3 && CanSpendHeroPoints)
                heroProgression.Rules.TryUpgradePassive((Stage2HeroPassive)index);
            RefreshHeroProgression();
        }
        private bool CanSpendHeroPoints => heroProgression != null && !game.RunDriver.Run.IsPaused && !game.RunDriver.Run.IsTerminal &&
            !(legacy != null && legacy.DebugOpen);
        private void OnBlinked(Vector2 from, Vector2 to)
        {
            CancelHeroSkill();
            heroProgressionEffects.PlayBlink(from, to);
            CancelBlink(); ClearBuildingSelection();
        }
        private void ClearProgressionTransient()
        {
            CancelBlink(); heroProgressionEffects?.ClearEffects(); heroLevelUp?.ClearEffects();
            if (heroProgression != null) observedHeroLevel = heroProgression.Rules.Level;
            RefreshHeroProgression();
        }
        private void OnHeroLevelRaised(int level)
        {
            var earned = (level - observedHeroLevel) * heroProgression.Catalog.PointsPerLevel;
            observedHeroLevel = level;
            if (earned > 0 && !game.RunDriver.Run.IsTerminal) heroLevelUp?.PlayLevelUp(level, earned);
        }
        private void OnProgressionRunChanged()
        {
            if (!game.RunDriver.Run.IsTerminal) return;
            CancelBlink(); heroProgressionEffects?.ClearEffects(); heroLevelUp?.ClearEffects();
        }
        private void UpdateHeroProgression()
        {
            blinkConsumedPointer = false;
            if (heroProgression == null) return;
            var keyboard = Keyboard.current; var mouse = Mouse.current;
            if (!heroProgression.CanCast || keyboard != null && keyboard.escapeKey.wasPressedThisFrame ||
                mouse != null && mouse.rightButton.wasPressedThisFrame) CancelBlink();
            var qCast = keyboard != null && keyboard.qKey.wasPressedThisFrame && heroProgression.CanCast;
            var clickCast = blinkArmed && mouse != null && mouse.leftButton.wasPressedThisFrame;
            if (mouse != null && (qCast || blinkArmed))
            {
                var screen = mouse.position.ReadValue();
                var visible = !presentation.IsPointerOverUi(screen);
                var camera = scene.CameraFollow.BoundCamera;
                var ray = camera.ScreenPointToRay(screen);
                if (visible && new Plane(Vector3.forward, Vector3.zero).Raycast(ray, out var distance))
                {
                    var cursor = (Vector2)ray.GetPoint(distance);
                    var rules = heroProgression.Rules;
                    var valid = rules.TryGetBlinkLanding(scene.PlayerMover.transform.position, cursor, heroProgression.CanLand, out var landing);
                    heroProgressionEffects.SetBlinkAim(blinkArmed, landing, rules.BlinkRange, valid);
                    if (qCast || clickCast)
                    {
                        blinkConsumedPointer = clickCast;
                        bufferedBuildingPress = false;
                        heroProgression.TryBlink(cursor);
                    }
                }
                else heroProgressionEffects.SetBlinkAim(false, Vector2.zero, 0, false);
            }
            RefreshHeroProgression();
        }
        private void RefreshHeroProgression()
        {
            if (heroProgression == null || progressionView == null) return;
            var rules = heroProgression.Rules; var catalog = heroProgression.Catalog; var view = progressionView;
            view.Level = rules.Level; view.MaxLevel = catalog.MaxLevel;
            view.RankPointCost = catalog.RankPointCost;
            view.XpWithin = rules.XpWithin; view.RequiredXp = rules.RequiredXp; view.AvailablePoints = rules.AvailablePoints;
            view.BlinkRank = rules.BlinkRank; view.BlinkCooldownRemaining = rules.BlinkCooldownRemaining;
            view.Health=rules.Health;view.MaximumHealth=rules.MaximumHealth;view.Mana=rules.Mana;view.MaximumMana=rules.MaximumMana;
            view.BlinkManaCost=rules.EffectiveBlinkManaCost;
            view.BlinkCooldownDuration = rules.EffectiveBlinkCooldownDuration; view.BlinkArmed = blinkArmed;
            view.BlinkCooldownTotal=rules.BlinkCooldownTotal;
            view.CanArmBlink = heroProgression.CanCast && rules.CanBlink;
            view.BlinkUnavailableReason = rules.BlinkCooldownRemaining > 0 ? "Перезарядка" : rules.Mana<rules.EffectiveBlinkManaCost?"Недостаточно MP":"Сейчас недоступно";
            view.CanUpgradeBlink = CanSpendHeroPoints && rules.CanUpgradeBlink;
            view.BlinkUpgradeReason = rules.BlinkRank >= catalog.BlinkMaxRank ? "Максимальный ранг" :
                !CanSpendHeroPoints ? "Сейчас недоступно" : "Нужно очко героя";
            view.WeaponDamage = game.HeroCombat.Rules.EffectiveDamage;
            view.WeaponRange = game.HeroCombat.Rules.EffectiveRange;
            view.WeaponInterval = game.HeroCombat.Rules.EffectiveFireInterval;
            for (var i = 0; i < 3; i++)
            {
                var passive = view.Passives[i]; var rank = rules.PassiveRank((Stage2HeroPassive)i);
                // Strings change only on rank changes; cooldown refresh does not allocate labels each frame.
                if (passive.MaxRank == 0 || passive.Rank != rank)
                {
                    var next = Mathf.Min(rank + 1, catalog.PassiveMaxRank);
                    var bonus = i == 0 ? catalog.DamagePerRank : (i == 1 ? catalog.AttackSpeedPerRank : catalog.MovementSpeedPerRank) * 100;
                    var currentText = $"+{Mathf.RoundToInt(rank * bonus)}" + (i == 0 ? "" : "%");
                    var nextText = $"+{Mathf.RoundToInt(next * bonus)}" + (i == 0 ? "" : "%");
                    passive.EffectText = (i == 0 ? "Урон " : "") + currentText + (next > rank ? " → " + nextText : "");
                }
                passive.Rank = rank; passive.MaxRank = catalog.PassiveMaxRank;
                passive.CanUpgrade = CanSpendHeroPoints && rules.CanUpgradePassive((Stage2HeroPassive)i);
                passive.UnavailableReason = rank >= catalog.PassiveMaxRank ? "Максимальный ранг" :
                    !CanSpendHeroPoints ? "Сейчас недоступно" : "Нужно очко героя";
            }
        }
        private void UnbindHeroProgression()
        {
            legacy?.BindHeroProgressionTools(null);
            if (presentation != null)
            {
                presentation.BlinkArmRequested -= ArmBlink; presentation.BlinkCancelRequested -= CancelBlink;
                presentation.BlinkUpgradeRequested -= UpgradeBlink; presentation.HeroPassiveUpgradeRequested -= UpgradeHeroPassive;
                presentation.WindowChanged -= CancelBlink; presentation.SetHeroProgression(null);
            }
            if (heroProgression != null)
            {
                heroProgression.ControlBlocked = null; heroProgression.Blinked -= OnBlinked;
                heroProgression.Rules.LevelRaised -= OnHeroLevelRaised;
                if (heroProgressionEffects != null) heroProgression.Rules.ExperienceGained -= heroProgressionEffects.ShowExperience;
            }
            if (game?.RunDriver?.Run != null)
            {
                game.RunDriver.Run.Reset -= ClearProgressionTransient; game.RunDriver.Run.Rewound -= ClearProgressionTransient;
                game.RunDriver.Run.Changed -= OnProgressionRunChanged;
            }
            heroProgressionEffects?.ClearEffects();
            heroLevelUp?.ClearEffects();
        }
    }
}
