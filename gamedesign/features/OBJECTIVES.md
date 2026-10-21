# Feature: Objectives And NPC Quests

## D116 open encounter environments — locally implemented

The owner approves both camp_v2 appearances and now authorizes world scenery.
Each typed spot reserves a large clear forest territory before manual-resource
spawn. The central fighting meadow and at least three broad exits remain free;
orc hide/timber structures and ogre masonry ruins sit to the side/rear. Real
solid props use the shared swept obstacle map for hero and world enemies.
World regeneration relocates existing survivors to valid new homes while
preserving IDs/generation/health/confirmed kills and current contracts. Ordinary
reset refills baseline camps without duplicating scenery; throne retry keeps it.
This changes environment only: no extra populations, quests, rewards or loot.

Local qualification:82 distinct latest PlayMode PASS,30 native frames, camp
Astra visual PASS. Wide routes tested with real swept movement at.6radius on
several seeds, solidity and quest/reset/retry/regeneration verified. Ordinary Mac
player restarted without fixture. Source/provenance and remaining Windows/night
qualification:art/source/world_camps_2026-10-21_verification.json.

## D114 current guild interaction correction

Quest actions use proximity to the actual guild building ground footprint and
existing interaction reach, not only one displaced entry point. Far click may
preview orders; the action explicitly asks to approach the guild. Acceptance and
turn-in remain local, preserving the return-to-guild reward loop. Decoration is
never the service target. Open-camp environment concepts are D115, not new quests.


D111 latest owner correction: the small world notice board is decoration only,
placed substantially farther right of the guild door with a free approach,
smaller and muted to avoid attracting attention. Click the guild BUILDING
to open the contract-board window; local actions use GuildInteractionPosition.


## D-110/D111 adventurers' guild — implemented first slice2026-10-21

The owner canceled the temple elder as quest giver. The approved city retains
both green-roof guild/outdoor board and original temple with an open public
teleport forecourt. Final city_v4_open_temple, board_f_v1 and trial_e_v1 approved.
Main owns rules/AI/integration; Astra owns native city/board/enemy presentation.

### Progression and contract lifecycle

- Personal badge starts F; exactly F/E/D/C/B/A/S, separate from hero/equipment.
- Three successful TURN-INS of the current rank, including repeated orders,
  open ONE next-rank trial. 3/3 alone does not promote. Trial turn-in grants the
  new rank and its ordinary board; repeat toward S. Lower-rank work does not
  advance the current rank. No guild-building upgrade or badge inventory item.
- Selecting the first order only previews it. Rank seals filter available boards.
  Two visible papers plus scroll is presentation, not a two-quest content cap.
- ACCEPT refills/resets the order's typed spot. One active contract per spot;
  separate orc and ogre contracts may coexist. Opening/selecting/turning in never
  refreshes enemies. A cleared spot stays empty until another acceptance.
- Only confirmed kills from that acceptance's generation count; old shots or
  unrelated kills cannot complete it. CLEAR means ready to return, no contract
  reward yet. TURN-IN at the nearby guild board grants gold/XP exactly once.
- Initial slice uses two separate existing-terrain clearings, orcs and ogres,
  with serialized rank-specific population/stats/rewards and six next-rank trials.
  Camp ink art is decorative, not an approved modular world environment kit.
- Enemy XP is separate from contract XP. Hero death leaves quest/camp progress;
  throne retry keeps CURRENT guild/world state. Full reset clears to F and initial
  camps. No disk persistence, extra currency, global quest shortcut or new NPC.

Runtime balance belongs to Resources/Stage3Guild.asset, not this spec. Native
board uses fixed paper geometry, physical wood/brass rank filters, location,
reward and contextual accept/in-progress/claim action. Actions revalidate local
guild-entrance range and gameplay state; a remote preview grants nothing. More world
spots/kinds, loot and final tuning remain later content. Source/provenance:
art/source/guild_2026-10-21_*.json. Local qualification:89distinct PlayMode outcomes PASS; native F→E and rule F→S, repeat/turn-in/death/retry/click/lock checks. Evidence: art/source/guild_2026-10-21_verification.json. Windows and final balance remain unverified.

## D-109 temple quests — superseded by D110,2026-10-21

Historical concept only: owner canceled the temple/elder and this window.
The separate orc/ogre spot intent remains accepted under D110.

Owner requests a quest-selection/receiving window on clicking the city temple
mage. The mage, purple ogre, final orc size and courtyard placement are already
appearance-approved by D108; the quest window itself is a new appearance review.
Main owns ImageGen/provenance and future gameplay integration; Astra owns visual
direction/review and later approved visual implementation.

The world contains separate encounter spots such as camps and ruins, with
different enemy kinds by location. Current orcs and ogres belong in separate
spots. Goblins and other undead are future content examples, not current assets
or required populations. HERO_COMBAT.md owns the world-enemy combat boundary.

### Window proposal — revision requested

Owner returned v1 with «Нужна доработка». Specific changes are being clarified;
the current window is not approved. D108 character/placement approvals remain.

[Quest window v1](../../art/concepts/temple_quests_2026-10-21/window_v1.png)
uses the current fullscreen Lunar Porcelain style and familiar global navigation/
Esc. Compact mage portrait in the header; two columns, available quests left
and the selected quest right, with image, objective, place and one «Принять
задание» action. Two illustrative entries, «Лагерь орков» and «Стоянка огров»,
represent separate locations. Equal card geometry, safe text insets and cyan
selection are visual requirements; enemy quality/rank is not a quest label.

The scene illustration is concept artwork, including the generated camp
backdrop; it is not approved modular camp assets, native UI or final world layout.
No reward values/counters, goblin entries or extra controls are assigned by the
mockup. Quest text is illustrative. Acceptance is readable choice → selected
detail → accept action matching existing fullscreen UI, without mixed camps.
Exact quest targets, rewards, acceptance limits, completion/turn-in, repeats,
population/respawn and navigation markers remain a later ready gameplay brief.

Source/prompts/rights: art/source/temple_quests_2026-10-21_generation.json and
temple_quests_2026-10-21_v1_prompt.json. No Unity assets, input bindings or quest
rules change in this concept-only step. Existing legacy objectives below retain
their historical scope rather than becoming the NPC quest implementation.

## Current product goal and throne growth — D-075/D-094

Конечная цель нового продукта — победить трёх разных сильных боссов,
после чего закончить игру победой. Подробности/открытый выбор места боя:
BOSSES_AND_VICTORY.md. Ночные орды развивают героя/оборону, но последняя
обычная волна не заменяет победу над боссами. Трон остаётся критичным:
разрушение вызывает текущий повтор D-087 с сохранением развития. Второстепенные задания ниже
сохраняют исторический отложенный статус; активный D110 выше — отдельный
guild-board concept, а не реализация старой gather/build задачи.

Владелец задал **ровно 3 улучшения трона по +5 максимального HP**.
При исходном максимуме H состояния: H, H+5, H+10, H+15. Четвёртого
платного улучшения нет. D-094 реализует ап в существующей карточке выбранного
трона, над обычным HUD и над строительством B. Текущие пробные цены материалов
и золота принадлежат HordeRun.asset; отдельного исследования/ремонта/кнопки
перемещения и нового здания нет. При базовых5HP максимум5→10→15→20.

Перенос здоровья: прибавить5к максимуму и текущему HP,
сохранив прежний абсолютный недостающий запас; не лечить весь старый урон.
Например3/5→8/10. При нулевом
HP поражение уже произошло — ап не отменяет терминальный исход.
Усиление разрешено в активной подготовке/волне при наличии цены; пауза,
модальное окно, проигрыш и повторный запрос во время оплаты блокируют действие.
Текущие апы/оплата сохраняются при повторе D-087, трон восстанавливается до
нового максимума. Полный Reset возвращает0апов/исходные5HP. В историческом
конечном Stage2 снимок восстанавливает апы/HP/кошелёк вместе.
Все индикаторы/эффекты повреждения используют фактический новый максимум,
а не константу исходного каталога. После третьего апа показывается предел,
повторный запрос не списывает цену. Карточка показывает «УсиленияN/3», живые
HP и следующий результат/цену; после третьего апа — «Максимальное усиление».
Внешность купола/карточки и эффекты поражения D078 сохранены.

## Historical lightweight-objective proposal

Status: historical candidate; secondary objective deferred beyond the first
playable by D-015. The primary throne/wave outcome remains active.
Owner/workstream: Run state / UI integration  
Depends on: resource inventory, building events, wave state, D-008

## Player Outcome

Игрок всегда понимает долгосрочную цель забега и получает одну ближайшую
необязательную задачу, которая направляет добычу или строительство и даёт
небольшую полезную награду.

## Objective Layers

- Primary: защитить трон и пережить финальную волну.
- Phase: подготовиться к следующей волне / отбить активную волну.
- Secondary: не более одной активной gather/build задачи.

Primary и phase objectives показывают состояние забега. Только secondary
objective имеет отдельный progress и resource reward.

## MVP Rules

- Objectives заданы вручную для карты и идут в фиксированном порядке.
- Поддерживаются только `Gather(resource, amount)` и
  `Build(towerRole, amount)`.
- Progress считается только после активации задачи и только от подтверждённых
  gameplay events; расход ресурса не уменьшает gather progress.
- Secondary objective необязательна, не останавливает timer/wave и не является
  условием победы.
- Completion срабатывает один раз и выдаёт только wood/stone/metal.
- Нельзя выбрать задачу на недоступный ресурс, tower role или невозможное
  количество в текущей фазе.
- Для MVP нет fail penalty, quest log, reroll, procedural generation, отдельной
  валюты, сюжетного текста или нескольких активных задач.

## UI

Одна компактная строка рядом с wave state: иконка, короткий текст и
`current / target`. При завершении — короткий completion feedback и показ
resource reward.
Инструменты не занимают UI slots и не являются objectives.

## Graybox Slice

Первая проверка: `Gather 5 wood` с одной фиксированной наградой существующим
ресурсом. Она подтверждает event contract, progress, one-shot completion и HUD.
Build objective добавляется после стабильного tower placement event.

## Acceptance

- [ ] Primary и phase objective соответствуют текущему run state.
- [ ] Одновременно активна максимум одна secondary objective.
- [ ] Gather/build progress не начисляется от неверного ресурса/объекта.
- [ ] Completion и reward происходят строго один раз.
- [ ] Objective не задерживает и не отменяет волну.
- [ ] Reload/restart очищает runtime progress предсказуемо.
- [ ] Progress/reward rules имеют EditMode tests; HUD integration имеет smoke
  check или точный degraded manual check.

## Post-MVP Candidates

Time limits, throne-health challenges, tower-specific kills, random contracts,
chains, meta rewards and achievements рассматриваются только после playtest MVP.
