# Feature: Lightweight Objectives

Status: Ready for minimal graybox slice  
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
