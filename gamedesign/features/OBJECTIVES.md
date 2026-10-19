# Feature: Lightweight Objectives

## Current product goal and throne growth — D-075, not implemented

Конечная цель нового продукта — победить трёх разных сильных боссов,
после чего закончить игру победой. Подробности/открытый выбор места боя:
BOSSES_AND_VICTORY.md. Ночные орды развивают героя/оборону, но последняя
обычная волна не заменяет победу над боссами. Трон остаётся критичным:
разрушение вызывает текущий повтор D-062. Второстепенные задания ниже
сохраняют исторический отложенный статус, новый quest log не вводится.

Владелец задал **ровно 3 улучшения трона по +5 максимального HP**.
При исходном максимуме H состояния: H, H+5, H+10, H+15. Четвёртого
платного улучшения нет. Цена может включать ресурсы и золото; величины
и условия доступа не выбраны. Ап доступен в карточке выбранного трона.

Рекомендуемый перенос здоровья: прибавить 5 к максимуму и текущему HP,
сохранив прежний абсолютный недостающий запас; не лечить весь старый урон.
Это предложение переноса, не отдельная новая механика ремонта. При нулевом
HP поражение уже произошло — ап не отменяет терминальный исход.
Количество апов, текущий/максимальный HP и оплата входят в снимок.
Все индикаторы/эффекты повреждения используют фактический новый максимум,
а не константу исходного каталога. После третьего апа показывается предел,
повторный запрос не списывает цену. Runtime пока имеет только исходный HP.

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
