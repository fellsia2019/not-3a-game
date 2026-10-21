# Hero progression

## D-103 — starting vitals and ranked Blink (locally qualified)

Owner accepted100HP/100MP, recovery1HP/s and5MP/s per active simulation second.
BlinkI/II/III/IV keep four existing ranges, cost100/75/50/25MP and recharge
5/4/3/2seconds. Actual values live in Stage3HeroProgression. StartingI is free;
three further point upgrades remain. Blocked/invalid/paused casts spend nothing;
regen freezes on pause/loss, follows game speed and continues while modal world
time runs. Retry keeps current vitals; full Reset restores100/100. HP is a real
starting stat without a current damage source; it does not fake hero combat.
Existing v4 bars/art remain; Astra binds actual numeric values and fractions.

Debug WTF is explicitly accepted: current active ability Blink casts at0MP and
0s cooldown. It clears the current cooldown on enabling, never refunds mana,
keeps collision/pause/UI checks, and clears on full Reset/HUD disable. Ability
ranks/XP do not change. Normal rank data stays visible in the development page;
current HUD tooltip reflects the effective debug cost/cooldown.

27EditMode and27PlayMode checks pass, including real driver/HUD WTF on/off,
pause/invalid landing, mana/regen/retry/reset, explicit debug wave launch and
strict camera bounds. Native Q confirms ordinary100MP/5s; F3 opens debug.
Native automated command fixtures exercise consecutive zero-MP casts and first
night launch. Mouse-click qualification is degraded because the computer-use
tool cannot dispatch coordinate clicks to this Unity window; it is not claimed
as passed. Astra approves23HUD/rank/zoom frames. Windows remains unverified.
Evidence: art/source/hero_vitals_2026-10-20_verification.json.

## D-104 — approved markers; death/recovery locally qualified2026-10-21

Latest owner wording is «Выпавшие вещи» / «E · Забрать вещи». Direction cue is
compact tooltip + small arrow at any of8bearings, not a large standalone arrow
or medallion. Current world/map/tooltip proposals are in
art/concepts/death_recovery_2026-10-20/; latest3approved «Да, внедряй». Main rules/
binding/tests and Astra native art/presentation are implemented.75sharedPlayMode
checks PASS;34nativeframes at720/900/1080 AstraPASS, actual keyboardE/I confirms
pickup. Windows/future exploration enemies remain. Evidence:
art/source/death_recovery_2026-10-20_verification.json.

Only future exploration enemies hurt the hero; portal-path creeps keep targeting
throne/blockers. Free death respawn at base portal; lose half XP within current
level, never levels. Drop only bag instances (including spare gear/documents),
keep equipped gear/tools. Wallet/resource counters are separate. Recoverable
body location needs world/map marker and world direction indicator. Owner chose
this as next separate step with marker concepts before implementation.
Implementation details: lose floor(currentXP/2), retain level/points/ranks and
Blink cooldown. Immediate safe base travel-anchor respawn restores HP/MP, without
unlocking travel research. Every nonempty bag remains recoverable through later
deaths/throne retries; empty bag creates no false marker; full Reset clears.
E pickup is exclusive to recovery that frame, release required after death/retry.
Development F3 death command allows testing without adding exploration enemies.
No death API/source, corpse store, recovery or respawn was added in D103.

## D-092 gathering first slice — approved and locally qualified2026-10-20

Owner approved both concepts and first rules: «Утверждаю, внедряй через Astra».
Main implements gameplay and GPT-6 Astra the native Unity page. The approved
scope is below; exact XP/effect sizes remain trial serialized tuning.
Local qualification:38PlayMode +17legacy economy EditMode checks passed;
native mouse spending and nine720/900/1080p Mac frames passed Astra review.
No new attributable console errors; Windows remains unverified. Sources,
wiring and evidence: art/source/gatherer_professions_2026-10-20_verification.json.
«Добытчик» groups two independently advancing professions: Рудокоп (stone and
metal, equipped pickaxe) and Лесоруб (wood, equipped axe). Mastery cap
is100 each, separate from hero level and item level. Explorer, production/base
upgrades and forge scroll sources remain later slices.

Initial XP/points:1profession XP per material unit actually granted by a
confirmed manual extraction. A35-stock deposit grants35total XP regardless
of strike count. Stone/metal advance Рудокоп; wood advances Лесоруб. Empty,
cancelled or out-of-range strikes, forging/refunds and automatic production
grant none; gathering grants no combat hero XP. Each profession starts at1,
has its own points, and grants one at mastery5,10,…100 (20total). One point
buys one talent rank. Stage3Professions.asset is authoritative trial tuning:
RequiredXp(level) currently starts20 and grows5per mastery level. The shown
concept140/300 and40/100 are illustrative, not runtime thresholds.

First slice: three talents with three ranks per profession (6nodes/18ranks
across both). The broader12-node-per-profession budget below stays a future
proposal; unused points can be retained for future nodes, not silently spent.
No decorative locked branches or functioning explorer in this UI.

| Talent, independently ranked for each profession | Initial trial effect per rank |
| --- | --- |
| Сила удара | +1unit of extraction power, added to actual equipped tool power |
| Темп работы | +5% work speed, multiplying the actual tool work-speed factor |
| Досягаемость | +10% allowed interaction gap from the resource edge |

Power = tool power + strength rank. Work duration = node base duration /
(tool speed × (1 +0.05×tempo rank)). Reach multiplies only the permitted edge
gap, never resource visual/collision radius; the already comfortable base
range needs no talent. No matching equipped tool means no extraction even
with trained talents. No combat/automatic-production effects are implied.
Yield remains min(power, remaining node stock, wallet room); only actual grant
earns XP. Depletion/stages/collision use the same stock. Tool/rank/stat changes
cancel unfinished strikes until release instead of granting early completion.

D-087 retry preserves CURRENT profession XP/points/ranks and current world
stock; only full Reset starts professions over. Pause/modal/loss suspend work.
Integration consumes Economy.Gathered(kind,actualGrant), combines item
and talent facts once, and exposes read-only UI facts. No new global manager,
currency, package or scene migration is required by this concept.

Approved screen: existing fullscreen O → «Боевые навыки» / «Профессии».
Left: both professions with independent mastery/numeric orange XP/points.
Middle: three talent cards and rank indicators. Right: selected talent,
current→next actual harvest results, one action and read-only equipped tool
quality/LVL. Learning leaves item quality/level unchanged; forging stays local.
Keep existing global navigation/wallet/footer. Main owns rules/ImageGen;
GPT-6 Astra owns appearance. Selected miner_v3/logger_v2 concepts live in
art/concepts/gatherer_professions_2026-10-20/; prompts/source/rights/review:
art/source/gatherer_professions_2026-10-20_generation.json.

Implementation acceptance: real held-E awards separate XP only
for granted material; combined effects consume exact stock and share the same
interaction ranges across selection/E/marker; spending/cap/overflow/tool swap,
pause/Reset/retry do not lose or duplicate points; actual O values and native
720/900/1080p presentation pass tests and Astra visual review.

## D-085 / D-086 — approved first playable slice

2026-10-19: the owner approved the fullscreen development page, HUD, Blink/XP
and the first three passives. The replacement level-up polygon_v3 was then
explicitly approved after shape, blur and text revisions. The first playable slice and approved visual implementation are complete.
Qualification is recorded in DEVELOPMENT_PLAN.md and WORKSTREAMS.md.
The extra attack spell from the older proposal is deferred; the automatic bow
remains single-target and dense matched-tier hordes still require towers.

Current contracts:

- Confirmed deaths in the shared hero/tower enemy population award XP once,
  globally while the hero explores. Arrival, despawn, reset and restoration
  award nothing. There is no XP pickup or last-hit contest.
- Hero starts at level1 with BlinkI; cap100. Later levels award hero points.
  Consecutive Blink/passive ranks spend available points and stop at their cap.
  XP thresholds, kill rewards, point costs, cooldown and personal bonuses are
  initial playable tuning in the Unity catalog, not a fixed balance inferred
  from concept numbers. Broader12/48/36 content budgets below remain proposals.
- Blink uses the confirmed300/500/800/1000 display ranges, converted by the
  catalog to current world units. Q casts toward the world cursor; clicking
  the first HUD slot arms targeting, then world LMB casts. RMB/Esc cancels.
  Distant cursor clamps to reach. Landing requires the full hero footprint
  inside world bounds and clear of resources/buildings; invalid landings
  consume no cooldown. There is no attack, splash or mana cost.
- Three personal passives have three ranks each: «Точный удар» adds integer
  damage to one equipped-weapon target; «Быстрые руки» improves personal attack cadence;
  «Лёгкий шаг» improves walking speed independently of the F3 multiplier.
  Cards show current → next bonuses. They do not improve towers or tools.
- O is the existing fullscreen development page. The v4 dock keeps minimap,
  separate navigation and ten-cell capacity, with one Blink icon, portrait
  LVL, point badge and numeric orange XP. HP/MP remain inactive0/0. At cap the
  XP bar displays its maximum state rather than an inactive0/0 source.
- Blink has cyan valid landing/ghost/range, red blocked mark+X and brief local
  departure/arrival wisps. Marks render above terrain and obstacle art. Compact
  orange XP feedback stays at the live bar and yields to the level celebration.
- D-086 level-up is a brief spectral gold/ivory polygonal shield-seal: crisp
  varied angled central contour, soft diffuse energy waves to the sides. Text
  has only «НОВЫЙ УРОВЕНЬ», the level numeral and awarded points; no O/Развитие
  prompt. Multiple awards coalesce to the latest level and total earned points.
  The noninteractive overlay does not block movement/Blink or pause the world.
  Pause/modal UI freezes the same animation; reopening a page never replays it.
  Reset/retry/terminal state clears transient feedback.

Rules/data/lifetime: Stage2HeroProgression(.cs), Stage2HeroProgressionCatalog
and Stage2HeroProgressionDriver in the existing Integration assembly. Stage3
opts in through PlayerInterfaceBinding.Progression; no new global manager,
package, UI stack or scene migration. Data source is
Assets/Stage3/HeroProgression/Resources/Stage3HeroProgression.asset. Existing
Stage2 scenes do not receive the opt-in driver. Combat and mover apply personal
bonuses without mutating shared catalogs or tower stats.

D-087 supersedes the active Stage3 rewind: keep current XP/level/points/ranks/
Blink cooldown and personal stats. Retry clears transient feedback but does not
replay rewards or restore an old progression snapshot. New kills in repeated
attempts award normal XP again. Legacy finite fixtures retain old snapshots.

Audio: Resources/Stage3HeroLevelUpSound currently contains only the accepted
three-part female chord (bone666138, Freesound274121, CC0) and stereo echoes.
The owner explicitly postponed the hammer and kept the reward cue without it.
All four impact experiments were rejected and are absent from runtime. The
owner then selected the third supplied reference: Elden Ring Player Level Up
(https://www.youtube.com/watch?v=k1avFgP63Ng). Original candidate v5 was also
rejected as entirely unlike that reference; it was never imported. The prior
technical waveform checks did not include an auditory comparison and do not
establish similarity. Latest owner confirmation keeps the accepted voice/echo without an impact.
The impact task is deferred, not an open requirement of this playable slice.
Any future attempt requires actual auditory comparison; technical WAV checks
alone do not qualify similarity. No reference-game audio
is downloaded, sampled or shipped. Source bytes, license, reproducible numpy mix script and previews
are preserved in art/source/hero_levelup_audio_2026-10-19/.
PCM is3.2s, stereo44.1kHz16-bit, peak−1.51dBFS, without clipped samples.
SetAudioClip remains available for later production replacement.
Original prompts, references, rights, approval and PNG hashes:
art/source/hero_progression_2026-10-19_generation.json.

## Prior broader design and proposed budget

Status: design revision D-074 with D-075 campaign/equipment clarification,
2026-10-19; replaces the D-073 proposed spell
roster. Confirmed: level cap 100, cursor Blink, low-splash exploration hero and
towers responsible for dense hordes. Counts and point budgets below are
proposals, not implemented mechanics or approved art. Owner: current Stage 3
Game Design chat. Dependencies: HERO_COMBAT.md, PORTAL_CONTROL.md,
EQUIPMENT_AND_RUNES.md, STAGE2_ECONOMY.md, STAGE2_RUN_STATE.md and future hero
health/open-world enemies.

## Role and scope

Герой исследует мир, добывает ресурсы и зачищает небольшие группы в заброшенных
местах. Башни уничтожают массовые ночные орды. Магия героя преимущественно
одиночная: точный умеренный урон, короткий контроль одной цели, усиление личной
атаки и выживание. Небольшой огненный сплеш не превращается с рангами в средство
зачистки дороги.
По D-075 развитие готовит к трём сильным боссам, после победы над всеми
кампания заканчивается. Их место/режим боя ещё открыты: BOSSES_AND_VICTORY.md.

Предел — **100 уровней героя**, расширяемый позже. Это не 100 обязательных
ночей и не обещание сотен часов контента. Уровень героя, освоенной орды,
календарный день, мастерство профессии и уровень предмета — разные величины.
Уровень героя не повышает текущих врагов автоматически.

Проверять роль нужно при сопоставимом развитии героя, базы и ночной угрозы:
сочетание доступных навыков, скорости атаки, экипировки и рун не позволяет
герою одному стабильно зачищать соответствующую плотную орду. Подготовленная
оборона имеет путь к победе. Переросший старую угрозу герой может легко
побеждать старых врагов; искусственная неуязвимость к герою не нужна.
Первый лёгкий бой по D-064 остаётся доступен стартовому луку.

## Proposed complete budget

| Система | Планка |
| --- | --- |
| Уровень героя | 1–100; подтверждено владельцем |
| Библиотека активных умений | 12 умений, по 4 ранга |
| Одновременно доступные активные | Предложено 6 мест, одно занимает блинк; окончательный предел открыт |
| Общие пассивные таланты | 48 узлов: 3 ветки по 16, до 3 рангов у узла |
| Профессии | Рудокоп, лесоруб, исследователь; своё мастерство 1–100 |
| Таланты профессий | По 12 узлов с 3 рангами: всего 36 узлов |
| Очки героя | Блинк I бесплатно; по 1 очку за уровни 2–100, всего 99 |
| Очки профессии | По 1 за каждые 5 уровней мастерства; 20 на профессию |

Кроме подтверждённого предела героя, это рекомендуемый бюджет содержания,
а не требование изготовить всё до первого билда. Характеристики и пороги XP
при реализации хранятся в Unity data; таблица задаёт объём и правила выбора.

Активные ранги и общие пассивки расходуют очки героя. Возможны 47 платных
рангов активных и 144 ранга пассивок: **191 улучшение при 99 очках**.
Максимально изучить всё нельзя. В каждой профессии 36 возможных рангов при
20 очках. Расширение библиотеки не выдаёт бесплатные ранги или места панели.

D-078: интерфейс проектируется с возможностью расширить панель до десяти
ячеек (в концепте 2×5). Это пожелание к вместимости HUD, а не утверждение
десяти одновременно доступных умений, новых клавиш или изменения очков.
Число экипированных умений согласовать отдельно; библиотека/бюджеты выше
остаются предложениями. Числовые шкалы HP/MP/XP требуют реальных игровых
источников; значения на v4-картинках только демонстрируют оформление. В D-078
интерфейс уже показывает отключённое 0/0 + «Не открыто», без игрового баланса.

## Active abilities

Автоматическая базовая атака и кирка/топор мест на панели не занимают.
Герой универсален, стартовый лук не фиксирует класс. Названия рабочие;
клавиши, иконки и оформление требуют отдельного визуального согласования.

| Умение | Функция и предел |
| --- | --- |
| Блинк | Скачок к курсору; четыре ступени дальности |
| Чародейский снаряд | Точный умеренный удар по одной цели без взрыва |
| Огненный снаряд | Удар и горение основной цели; слабый побочный урон максимум двум соседним |
| Ледяная игла | Одна цель и краткое замедление; без ледяного поля |
| Метка охотника | Короткое усиление личных попаданий по одной цели |
| Сосредоточенность | Временное ускорение личной атаки с общим пределом скорости |
| Грозовой удар | Одиночный удар с прерыванием подходящего действия, без цепи |
| Попутный ветер | Короткое ускорение ходьбы для перемещения и отхода |
| Каменная кожа | Временное снижение входящего урона герою |
| Личный барьер | Краткое поглощение урона герою |
| Восстановление | Лечение героя; без лечения трона или башен |
| Поток маны | Личное восстановление маны; занимает место атакующего умения |

Первые шесть — начальный каталог, ещё шесть расширяют выбор позже. Защитные
умения вводятся вместе со здоровьем героя и врагами открытого мира. Старые
предложения «Ледяная печать», «Цепная молния» и рост площади вспышки заменены.
Полного контроля орды, цепей на десятки целей и взрывов от каждой смерти нет.

У каждого умения четыре ранга. Атакующие ранги усиливают основную цель или
эффективность; число побочных целей не растёт. Общий урон группе ограничен:
даже три цели не получают каждая полный основной удар. Скорость атаки,
перезарядки, предметы и руны проверяются вместе: одного запрета большого
сплеша недостаточно, если одиночные атаки становятся чрезмерно частыми.
Магия использует личную ману и перезарядки; исключение для базового блинка
предложено ниже. Не допускается каст нескольких взаимоисключающих действий
одним нажатием или обход темпа через смену набора на панели.

## Blink

Подтверждено: короткий скачок в точку курсора с первого уровня, четыре ранга
дальности **300/500/800/1000** по шкале спелла. Круги остаются для дальних
переходов. Рекомендация: первый ранг бесплатно, только перезарядка без маны.
Необходимое перемещение не конкурирует с первым боевым выбором.

Предлагаемый масштаб сетки — 100 единиц шкалы на клетку, то есть 3/5/8/10
клеток. Проверить по игровому кадру и времени ходьбы до фиксации в data.
Дальний курсор ограничивает скачок максимальной дальностью в его направлении.
Приземление проверяет полный радиус героя, границы и занятость земли.
Недопустимая точка не расходует перезарядку; пересечь небольшую преграду
при свободном приземлении допустимо. Предпросмотр показывает реальный предел.

За полноэкранным UI, в паузе и при поражении каст заблокирован. Скачок
прерывает незаконченный удар и размещение; продолжение добычи требует свежего
E. Один ввод не подтверждает спелл и постройку одновременно. Поздняя развилка
может дать второй заряд либо более частый одиночный скачок. Заряды входят
в общий бюджет силы и checkpoint; дальность блинка не усиливает прочую магию.

## Passive talents

| Ветка | 16 узлов; основные направления |
| --- | --- |
| Точная атака | Одиночный физический/магический урон, управляемая скорость атаки, горение основной цели, личная метка и эффективность против элиты |
| Выживание | Здоровье, личная защита, барьер и восстановление; после появления соответствующих механик |
| Подвижность и магия | Ходьба, блинк, мана, эффективность каста и одиночного контроля, альтернативные вершины перемещения |

Это бюджет трёх веток, а не 48 уже определённых процентных бонусов. Узлы
проектируются с работающими системами, короткими путями и несколькими
взаимоисключающими вершинами. Фиктивные характеристики и скрытый обход
ограничения площади не добавляются. Постоянные усиления башен относятся
к исследованиям и отдельным рунам обороны, не к личному урону или блинку.

## Professions and tools

Мастерство рудокопа/лесоруба растёт за фактически извлечённый материал,
исследователя — за первое обнаружение полезного места и завершённое открытие.
Бег по кругу, пустой удар и повторное открытие точки опыта не дают.

У каждой профессии три направления по четыре узла, до трёх рангов у каждого.
Для рудокопа/лесоруба — сила удара, темп работы и удобство похода за ресурсом.
Для исследователя — перемещение, обнаружение и польза первого исследования
места. Вес, ремонт и прочность инструментов не вводятся ради заполнения ветки.

Сила инструмента означает количество извлечённого запаса. При силе 3 удар
по остатку 35 сразу отдаёт 3 и оставляет 32; по остатку 2 отдаёт 2 и истощает
узел. Визуальная стадия и коллизия следуют остатку. Незавершённый удар
не платит. Улучшение не создаёт материал сверх запаса. Мастерство и качество
предмета-инструмента — разные источники эффективности, проверяемые в сумме.
По D-075 кузница теперь активирована как будущий путь создания/улучшения
самих кирок и топоров, отдельно от мастерства: EQUIPMENT_AND_RUNES.md.
Улучшения автоматических генераторов — отдельные технологии/вложения по
RESOURCE_PRODUCTION_BUILDINGS.md. Ремонт и производственные цепочки не выбраны.

## Level bands and experience

| Уровни героя | Ориентир наполнения |
| --- | --- |
| 1–20 | Блинк, базовая атака, выбор из первых шести активных, понятные пассивки, начало профессий |
| 21–40 | Часть альтернативных активных, сочетания талантов, небольшие руины и предметы |
| 41–60 | Вся библиотека из 12 активных; выбор шести, основа своего билда |
| 61–80 | Углубление специализации, трудные места, качество снаряжения/рун, рост базы |
| 81–100 | Завершение билда/обороны, серьёзная подготовка к боссам; без выдачи всех талантов разом |

Это ориентиры, не часы до уровня или фиксированный ранг предмета на пороге.
Точные XP требуют измерений реального цикла. Сто полос опыта не заменяют
содержательных мест, решений и развития базы.

Боевой опыт идёт за подтверждённые убийства: свои башни дают герою столько
же, сколько его атака, без перехвата последнего удара. Рекомендация:
оборона начисляет общий опыт и во время исследования мира, чтобы игрок
не стоял у портала постоянно. Добыча развивает прежде всего свою профессию;
полезные первые открытия и завершение мест могут давать опыт героя.
По D-075 основной боевой XP идёт через массовый TD; мир даёт меньше XP,
зато более вероятные ценные находки. Сравнивать нужно награду за время игры.

Дневной фарм повторяет освоенный уровень орды. Опыт сильно переросших врагов
рекомендуется постепенно снижать; кривая ещё не выбрана. Источник определяет
уровень награды. Старый лёгкий портал не выдаёт предметы 100 уровня только
из-за уровня героя.

## Checkpoints and first implementation

Уровни, очки, ранги и будущие профессии принадлежат текущему миру.
D-087 supersedes the active Stage3 D-062 snapshot: retry keeps CURRENT
XP/points/ranks/equipment/world progress and returns morning for the same
pending horde. Do not roll back gathering mastery or replay awarded points.
Legacy finite Stage2 retains its snapshot behavior. Постоянная руна означает
постоянство в текущем мире, а не roguelite-метапрогрессию. Дневные врата и
семантика текущего повтора реализованы: PORTAL_CONTROL.md.

По текущему D-085 первый срез использует каталог с пределом 100, блинк,
первые три предложенные пассивки и проверяемый XP. Дополнительный одиночный
атакующий спелл следует позже, отдельно от этого среза. Профессии
и следующий каталог добавляются по DEVELOPMENT_PLAN.md. Все 12/48/36 узлов
не нужны до первого игрового теста.

Приёмка: XP/очки без двойных наград; полезный выбор; допустимое приземление
блинка; откат всей прокачки; успех против небольшой группы и необходимость
обороны против сопоставимой ночи. Будущая внешность проходит ImageGen,
одобрение владельца и реализацию GPT-6 Astra. Здесь XP/UI/магия не внедряются.

## References

[Last Epoch Skill Specialization](https://support.lastepoch.com/hc/en-us/articles/46363203944859-Skill-Specialization)
— референс ограниченного набора и развития знакомых умений.
[New World Character Progression](https://www.newworld.com/en-gb/news/articles/character-progression)
— опубликованный разработчиком референс раздельного боевого и мирного развития.
Это источники принципов, а не наших количеств или текущего баланса этих игр.
WoW/Dota названы владельцем ориентиром управления блинком.


## D-089/D-090 weapon integration

The opt-in EquipmentDriver provides actual equipped bow/wand base damage,
range, cadence and flight. Existing personal passives compose with that profile;
they do not replace item bonuses or apply to towers. Unequipping the weapon
halts attacks and preserves its remaining cooldown (no equip-spam free shots).
The HUD reads effective current stats and actual weapon icon/name. Both weapons
remain single-target with no new area spell. Forge placement participates in
existing draft cancellation/Blink ownership. D-087 retry preserves equipped
items and personal progression independently; full Reset restores starters.
