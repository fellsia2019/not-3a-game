# Workstreams And Chat Handoffs

Этот файл — единая координация отдельных чатов. Один активный владелец на
систему/сцену/контракт; параллельные агенты не редактируют одни и те же файлы.

## Recommended Order

1. **Design coordinator:** vision, decisions, stage gates, feature briefs.
2. **Foundation:** isometric camera/movement, grid, sorting, input, scene shell.
3. **Economy:** resource nodes, gathering, inventory/cost contracts.
4. **Defense:** placement, towers, targeting, damage and throne health.
5. **Waves/AI:** route, spawn schedule, enemy movement, win/lose orchestration.
6. **UI/UX:** HUD, building flow, warnings, menus and accessibility.
7. **Art/content:** tile/sprite pipeline, animation, VFX/audio and content data.
8. **Platform/QA:** saves/settings, tests, Windows builds, Steam and release QA.

На Stage 1 Foundation владеет интеграционной сценой. Economy, Defense и
Waves/AI начинают параллельно только после согласования узких data contracts;
иначе работа последовательно интегрируется владельцем сцены. UI и art не должны
блокировать graybox.

## Stage 2 Ownership Rules

Canonical backlog и dependency graph находятся в
`gamedesign/features/STAGE2_VERTICAL_SLICE.md`.

- **Integration — единственный владелец
  `Assets/Scenes/Stage2VerticalSlice.unity`.** Только этот owner создаёт,
  редактирует, сохраняет и меняет build wiring Stage 2 scene. Map, Economy,
  Defense, Waves/AI, UI/UX, Art/Audio и Platform/QA передают ему prefabs, data,
  tests и wiring notes; они не открывают сцену для сохранения.
- Stage 1 scene/assets остаются regression baseline и не мигрируются скрыто.
- Параллельная работа разрешена только для task IDs с готовыми входными
  контрактами и непересекающимися surfaces. Активный owner до начала добавляет в
  этот файл task ID, owned files/contracts, dependencies и acceptance.
- Task-specific feature spec создаётся при фактической активации задачи, а не
  заранее. Integration возвращает дефект владельцу системы вместо
  opportunistic изменения чужого runtime/prefab surface.
- S2-INT-02, S2-QA-01 и S2-QA-02 закрываются последовательно. Только принятый
  внешний playtest переводит Stage 2 в complete.

## Generic Prompt For A New Chat

```text
Прочитай AGENTS.md, gamedesign/GAME_VISION.md,
gamedesign/DEVELOPMENT_PLAN.md, production/DECISIONS.md и этот файл.
Ты владеешь workstream <NAME> и задачей <TASK>. Сначала изучи проект и
соответствующий feature spec; не придумывай player-facing правила. Зафиксируй
goal, scope, contracts, acceptance и unresolved decisions. После готовности
реализуй минимальный coherent slice, проверь его и обнови canonical plan/spec и
Handoff Register. Не изменяй поверхности других активных workstreams без
явного handoff.
```

## Handoff Entry Format

```md
### YYYY-MM-DD — <source> -> <target/status>
- Outcome: <что теперь работает>.
- Contract/files: <что изменено и на что можно опираться>.
- Verification: <tests/build/manual; degraded reason if any>.
- Remaining: <явно вне завершённой задачи>.
```

## Handoff Register

### 2026-09-20 — Project setup -> All workstreams

- Outcome: создана базовая документация, scope MVP и правила работы чатов.
- Contract/files: `AGENTS.md`, `gamedesign/`, `production/`, `.cursor/rules/`.
- Verification: структура и ссылки проверяются до закрытия Stage 0.
- Remaining: настроить version control и реализовать Stage 1 graybox spec.

### 2026-09-20 — Design approval -> Foundation

- Outcome: GAME_VISION утверждён; герой не сражается, обучение и progression
  инструментов перенесены после MVP.
- Contract/files: `gamedesign/GAME_VISION.md`, `production/DECISIONS.md`,
  `gamedesign/features/GRAYBOX_CORE_LOOP.md`,
  `gamedesign/features/OBJECTIVES.md`.
- Verification: решения отражены в canonical vision/plan; Git baseline
  `0016bd5` опубликован в `origin/main`.
- Remaining: реализация интеграционного Unity graybox одним Foundation owner.

### 2026-09-20 — Foundation/integration -> Stage 1 complete

- Outcome: реализован полный graybox loop: WASD/fixed isometric camera, конечные
  tree/stone nodes с автоматическими топором/киркой, одна размещаемая башня,
  фиксированный маршрут и одна волна, throne damage, win/lose/restart и one-shot
  objective «добыть 5 дерева» с наградой существующим ресурсом.
- Contract/files: `Assets/Scenes/Stage1Graybox.unity`, `Assets/Stage1/Runtime/`,
  `Assets/Stage1/Prefabs/`, `Assets/Stage1/Tests/`,
  `ProjectSettings/EditorBuildSettings.asset`; integration scene ownership
  остаётся у Foundation до начала следующего согласованного workstream. Для
  discoverability добавлены `Tools > Stage 1 > Open Graybox Scene` и безопасное
  автопереключение только с неизменённой `SampleScene` при первом импорте.
- Verification: EditMode 7/7, PlayMode 3/3 (включая cell-space map bounds,
  Input System, gathering,
  invalid/valid placement, tower win, unopposed throne loss и restart), Windows
  x64 Development Build создан в `Builds/Windows/Not3AGraybox.exe`; headless
  player smoke запущен без attributable exceptions/errors.
- Readability follow-up: поле расширено с 15x11 до 21x17 клеток, внешний ряд
  получил контрастный boundary treatment, а hero/resources ограничены общей
  изометрической областью вместо несовпадающего world-space прямоугольника.
- Scale follow-up: hero, enemy, tree, stone, tower и throne переведены на
  простые grounded rectangular blocks; PlayMode фиксирует относительную высоту,
  физическую блокировку героя и наличие blocking footprints у ресурсов, трона и
  построенной башни. Встроенный UI sprite заменён на проектный solid sprite 1x1
  world unit; PlayMode проверяет точное совпадение нижней грани с ground origin.
- Visual cleanup: runtime-заливки `Resource Zone`/`Build Zone` удалены после
  обнаружения неверного масштаба встроенного UI sprite; PlayMode запрещает их
  повторное появление, enemy entrance использует изометрический marker.
- Remaining: финальный art/audio/UI, tutorial, metal, дополнительные
  towers/enemies/waves, tool progression, save/Steam и production balance вне
  Stage 1; P-002–P-006 не становятся durable решениями из-за prototype values.

### 2026-09-21 — Product/design pivot -> Stage 1 reopened

- Outcome: принято D-009 — башни формируют динамический grid route; длинный
  зигзаг является основной стратегией, а при полном перекрытии враги разрушают
  blocking towers и продолжают путь. Fixed-route build остаётся исторической
  проверенной базой, но больше не закрывает Stage 1.
- Contract/files: `gamedesign/GAME_VISION.md`,
  `gamedesign/features/DYNAMIC_ENEMY_PATHING.md`,
  `gamedesign/features/GRAYBOX_CORE_LOOP.md`, `gamedesign/DEVELOPMENT_PLAN.md`,
  `production/DECISIONS.md`. Foundation сохраняет integration/scene ownership;
  navigation grid/path result принадлежит Foundation/Waves, tower occupancy и
  health — Defense.
- Verification: canonical документы согласованы с maze-building direction;
  implementation verification ещё не выполнялась и прежние 7/7 EditMode,
  3/3 PlayMode не покрывают новый gate.
- Remaining: заменить `FixedRoute` и route-clearance rejection на grid search,
  occupancy/replanning, tower health и blocker siege; затем заново пройти
  EditMode, PlayMode, Windows build и ручной acceptance. Stage 2 не активен.

### 2026-09-21 — Foundation/integration -> Stage 1 human acceptance

- Outcome: D-009/D-011 реализованы на плоском 21x17 representative slice:
  широкий серый corridor и зелёная территория coplanar; башни занимают клетки
  обеих зон, меняют кратчайший путь и могут полностью закрыть corridor. При
  блокировке враг подходит к доступной стороне башни, разрушает её, освобождает
  occupancy и продолжает к трону. Старый `FixedRoute` больше не связан со
  сценой.
- Contract/files: `Assets/Stage1/Runtime/DynamicNavigationGrid.cs`,
  `Stage1Rules.GridPathfinder`, обновлённые `BuildPlacement`, `EnemyController`,
  `TowerController`, `WaveSpawner`, builder, prefabs и
  `Assets/Scenes/Stage1Graybox.unity`. Prototype tower cost — 2 wood/1 stone;
  значения остаются tuning, не durable balance.
- Verification: scene rebuild/compile passed; EditMode 8/8; PlayMode 4/4,
  включая route-cell placement, deterministic longer zigzag, full wall,
  blocker destruction, route resume, tower win, throne loss и restart. Windows
  x64 Development Build собран в `Builds/Windows/Not3AGraybox.exe`, запущен и
  отвечает без attributable startup errors.
- Remaining: человеку проверить читаемость серого corridor, ощущение
  maze-building/siege и prototype balance в Windows build. До этого Stage 1
  gate остаётся pending; production map 100x100 и сравнение 200x200 относятся
  к Stage 2/P-008, финальный арт и остальные out-of-scope системы не начаты.

### 2026-09-21 — Human playtest -> Stage 1 complete

- Outcome: человек подтвердил, что динамическое движение и разрушение полной
  стены работают корректно; Stage 1 gate закрыт. Временный siege-test profile
  удалён: стартовые ресурсы снова 0/0, здоровье врага снова 8.
- Contract/files: gameplay contracts не изменены; canonical status обновлён в
  `gamedesign/DEVELOPMENT_PLAN.md`, `GRAYBOX_CORE_LOOP.md` и
  `DYNAMIC_ENEMY_PATHING.md`.
- Verification: human acceptance получен; после возврата значений повторно
  пройдены EditMode 8/8 и PlayMode 4/4, Windows x64 Development Build успешно
  пересобран, запущен и не показывает attributable startup errors.
- Remaining: Stage 2 не активирован. Перед ним требуется отдельное решение о
  старте работ; P-002–P-006 и P-008 остаются открытыми, production-scale map и
  контент Stage 2 ещё не реализуются.

### 2026-09-21 — Stage 2 planning coordination -> Stage 2 workstreams

- Outcome: Stage 2 разложен на atomic task backlog от map/economy decisions до
  qualified build и external comprehension gate; отмечены dependency graph,
  parallel windows и единоличное владение Stage 2 scene Integration owner'ом.
- Contract/files: `gamedesign/features/STAGE2_VERTICAL_SLICE.md`, Stage 2 status
  в `gamedesign/DEVELOPMENT_PLAN.md` и ownership rules в этом файле. План не
  создаёт task-specific feature specs или Unity assets заранее.
- Verification: Stage 1 baseline `37178e2` перед планированием совпадал с
  `origin/main`, working tree был чист; документы сверены с D-001–D-011,
  P-002–P-006/P-008, Stage 1 contracts и art direction; Unity code/scenes не
  изменялись.
- Remaining: Stage 2 implementation не начата. Первый owner должен явно claim
  ready task и surface; P-002–P-006/P-008 закрываются соответствующими decision
  tasks до зависящей durable реализации.
