# Decision Log

Accepted решения обязательны для всех чатов. Новое решение не переписывает
историю: добавить запись, которая явно supersedes старую.

## D-001 — Target Platform

Status: Accepted, 2026-09-20.  
Decision: MVP выпускается только для Windows desktop через Steam. Порты
оцениваются после стабильного MVP.  
Reason: минимальный platform/QA scope.

## D-002 — Core Game Shape

Status: Accepted, 2026-09-20.  
Decision: трон расположен в центральной зоне; волны входят с одной стороны;
ресурсная зона находится с противоположной. Базовые ресурсы — дерево, камень и
металл.  
Reason: это исходный player-facing замысел.

## D-003 — MVP Visual Direction

Status: Superseded by D-015 on 2026-09-25; historical working direction.
Decision: 2D isometric pixel/near-pixel, fixed orthographic camera, no rotation,
one modular biome.  
Reason: самый достижимый pipeline среди данных референсов; лучше читаемость и
меньше объём ассетов, чем у painterly 2D или low-poly 3D.

## D-004 — Automation And Roguelite Scope

Status: Accepted for MVP scope, 2026-09-20.  
Decision: Factorio-style automation/logistics и persistent roguelite meta не
входят в MVP. Временный выбор улучшения между волнами можно оценить после
graybox.  
Reason: сначала нужно доказать базовый gather/build/defend loop.

## D-005 — Game Vision Approval

Status: Accepted, 2026-09-20.  
Decision: текущие core loop, pillars, MVP budget и art direction утверждены как
база проекта.  
Reason: человек подтвердил основной концепт, game design и game vision.

## D-006 — Non-Combat Hero And Tools

Status: Accepted, 2026-09-20.  
Decision: герой не имеет боёвки и выступает строителем/шахтёром. В MVP топор и
кирка — базовые контекстные инструменты без tiers, durability, crafting или
equipment UI, ручного переключения и ремонта. Tool progression рассматривается
после MVP.  
Reason: сохраняет идентичность героя и минимизирует combat/content scope.

## D-007 — Tutorial Scope

Status: Accepted, 2026-09-20.  
Decision: отдельное обучение не входит в MVP и оценивается после него. UI обязан
оставаться читаемым, но tutorial flow не разрабатывается.  
Reason: утверждённое ограничение scope.

## D-008 — Lightweight Objectives

Status: Accepted, 2026-09-20.  
Decision: главная цель — защитить трон до финальной волны. Дополнительно игрок
видит не более одной необязательной задачи на добычу ресурса или строительство
башен. Задача не блокирует волну и награждает только существующими ресурсами;
quest log, новая валюта и procedural generation не входят в MVP.  
Reason: даёт краткосрочную направленность и награду без отдельной тяжёлой quest
system или лишнего UI.

## D-009 — Player-Shaped Enemy Routes

Status: Accepted, 2026-09-21.
Decision: MVP использует динамический клеточный pathfinding вместо заранее
заданного неизменного маршрута. Размещённые башни занимают клетки и позволяют
игроку строить более длинные зигзагообразные пути. Полное перекрытие прохода не
запрещает постройку: враги атакуют и разрушают мешающие башни, после чего
пересчитывают путь к трону. Герой не становится боевой целью. Это решение
supersedes fixed-route recommendation и соответствующую Stage 1 assumption.
Reason: формирование лабиринта башнями является центральным стратегическим
решением в духе классических maze tower-defense карт, а не побочным вариантом
размещения.

## D-010 — Map Topology And Elevation Direction

Status: Accepted direction, 2026-09-21.
Decision: одна масштабируемая карта имеет вход волны в верхней правой стороне,
трон у противоположного конца invasion corridor и ресурсы преимущественно в
противоположной части мира. Серая зона вторжения находится на нижнем уровне;
зелёная территория текущего лесного биома — на верхнем. Более редкие ресурсы
располагаются дальше от трона. Точные размеры карты, ширина corridor и способ
пересечения уступов героем остаются tunable/open. Дополнительные биомы остаются
post-MVP. Это решение supersedes пространственную часть D-002, сохраняя её
базовый набор ресурсов.
Reason: фиксирует пространственную композицию пользовательского
`concept_map_global.png`, не превращая предварительный размер 100x100/200x200 в
непроверенное production-обязательство.

## D-011 — Flat MVP Map

Status: Accepted, 2026-09-21.
Decision: зелёная и серая зоны MVP находятся в одной игровой плоскости и
отличаются типом/цветом поверхности, но не высотой. Герой перемещается между
ними без уступов, прыжка или режима полёта. Решение supersedes elevation и
flight части D-010; его пространственная композиция позднее заменена D-016.
Reason: вертикальный traversal не усиливает основной gather/maze/defend loop и
создаёт лишнюю механику движения, визуальную неоднозначность и pathing scope.
Visual clarification, 2026-09-25: the player requested grass slightly above
the gray zone as in approved landscape concept B. This supersedes the equal
rendered height only: a shallow visual turf cap and soil edge are allowed.
Movement, targeting, placement, navigation and click selection remain on the
same flat gameplay plane; no traversal step or new buildable cells are added.

## D-012 — MVP Asset Detail And Deferred Blender Work

Status: Partly superseded by D-015 on 2026-09-25; the deferral principle
remains, but the pixel/2D target and post-MVP-only timing do not.
Decision: для MVP используются простые читаемые 2D-ассеты в рамках D-003;
детальные 3D-модели для Unity, их Blender-доработка и новый 3D asset pipeline
не являются зависимостью Stage 2–6. После проверки MVP Blender-работу можно
отдельно запланировать и при необходимости вести сложные задачи с GPT-6 Astra.
Формат применения 3D-исходников и возможная смена визуального направления
требуют отдельного решения; D-003 сейчас не superseded.
Reason: сохранить фокус, время и токены для проверки игрового цикла и
читаемости, не производя дорогой контент до подтверждения потребности.

## D-013 — Renewable Resource Nodes (P-003)

Status: Accepted by the human, 2026-09-24; closes P-003 and S2-ECO-01.
Decision: дерево, камень и металл имеют конечный запас в каждом узле.
Последняя добытая единица запускает восстановление полного запаса на том же
месте через положительный конечный интервал игрового времени. Частичный запас
не пополняется; попытки добывать пустой узел не перезапускают таймер.
Восстановление идёт в подготовке и во время волны, замораживается на паузе и
после win/loss, не зависит от завершения волны, оплаты или побочной награды.
Оно не начисляет инвентарь и не создаёт gather/objective fact. Каждый нужный
ресурс имеет доступный возобновляемый источник; место узла остаётся защищённым
от застройки и после истощения, восстановление не запирает героя в collider.
Рестарт возвращает полный запас узлов, очищает таймеры и незавершённую добычу,
обнуляет все три ресурса без переноса отложенных начислений между забегами.
Reason: исключить постоянное исчерпание ресурсов и зависимость восстановления
обороны от необязательной награды, сохранив цену пути и ручной добычи.
Scope: полный контракт и проверка правил —
[STAGE2_ECONOMY.md](../gamedesign/features/STAGE2_ECONOMY.md). Запасы, интервалы,
распределение и цены задаются Unity data при реализации. S2-ECO-02 проверяет
runtime lifecycle; интеграция проверяет доступ, размещение и reset; Checkpoint C
измеряет, успевает ли игрок восстановить оборону до поражения. Эти проверки
  позднее выполнены в Stage 2. Решение заменяет finite/no-recovery assumption только для
Stage 2 и дальнейшего MVP; принятый Stage 1 остаётся regression baseline.

## D-014 — Active-Wave Building And Actor Protection (P-002)

Status: Accepted by the human, 2026-09-24; closes P-002 and S2-BLD-01.
Decision: строительство разрешено в Preparation и Wave без паузы симуляции.
Нельзя строить в текущей клетке живого врага; начатый переход A -> B защищает
обе клетки до прибытия в B. Остальные клетки будущего пути можно застраивать,
включая полное перекрытие по D-009. При изменении маршрута враг завершает уже
защищённый шаг и пересчитывает путь перед следующим шагом или атакой, без
телепортации/выталкивания. Защита клетки учитывает всех живых врагов и не
регистрируется как tower occupancy. Постройка не перекрывает ground footprint
героя; герой остаётся non-combat. Маски, landmarks и ресурсные места по D-013
сохраняют свои ограничения.
Подтверждение заново проверяет фазу, клетку, акторов и полную цену. Успех даёт
одну башню, одно полное списание, одну регистрацию occupancy и один built fact;
отказ/отмена ничего не тратят. Preview не резервирует ресурсы или маршрут.
Пауза, win/loss и restart сбрасывают незавершённое размещение без отложенного
подтверждения; отмена не возвращает уже оплаченную башню. Разрушение и замена
используют identity-safe release; старые callbacks/атаки не затрагивают замену.
Reason: сохранить перестройку лабиринта во время волны, исключив наложение
башен на движущихся акторов и списание за несостоявшееся строительство.
Scope: полный принятый контракт —
[STAGE2_BUILDING.md](../gamedesign/features/STAGE2_BUILDING.md). S2-DEF-01
реализует транзакцию и проверки; S2-AI-02 — непрерывное движение/осаду и
active-step state; Integration/QA проверяют совместное поведение на карте.
Runtime verification позднее выполнена в Stage 2. Stage 1 и API S2-MAP-02 остаются
неизменными; их prototype assumption о строительстве уточняется для Stage 2/MVP.

## D-015 — First Playable And Target Look

Status: player-directed, 2026-09-25.
Decision: ближайшая цель — одна небольшая понятная TD-игра с ручной добычей
дерева, камня и металла, малым количеством башен, одной компактной картой и
законченным забегом. До проверки этого цикла не требуются новые виды контента,
тонкая балансировка, финальный UI, звук и красивые модели. После проверки
цикла развивать представление и содержание по результату игры. Целевая
эстетика — мягкая мультяшная объёмная изометрия в духе названных игроком
fantasy TD-референсов; pixel/near-pixel больше не целевой стиль. Текущий 2D
graybox остаётся допустимым временным представлением. Runtime 3D или
подготовленные из 3D спрайты выбираются малым пилотом перед производством
финальных ассетов. Пилот одного 3D-объекта в Blender и проверка в Unity
начинаются в Stage 3 сразу после принятого полного забега Stage 2; после
выбора pipeline можно делать остальные модели. Мирные постройки, пассивная
добыча, исследования, опыт,
крафт и расширенный мир остаются поздними кандидатами. Будущий замысел зон:
боевые башни на серой территории врагов, мирные здания на зелёной; текущие
маски и правила строительства первого играбельного билда не меняются.
Reason: игрок прямо уточнил желаемую игру и указал, что детальное планирование
и раннее производство контента расходуют время и токены до появления понятной
играбельной версии.
Supersedes: D-003 и визуальную часть D-005/D-012 о целевом pixel/2D стиле,
а также срок D-012 «только после MVP»: детальная работа над моделями возможна
после первого играбельного забега и малого арт-пилота. Сохраняется принцип
не производить дорогой арт до подтверждения цикла. D-008 и старый
MVP content ceiling не являются gate первого играбельного билда. D-001,
D-006, D-009, D-011, D-013 и D-014 остаются игровыми/техническими контрактами
действующего прототипа.

## D-016 — Corner Defense And Gray-Only Towers

Status: player-directed, 2026-09-25.
Decision: в первом играбельном билде портал врагов и трон находятся рядом в
одном углу карты. От этого угла зелёный мир с местами исследования и ручной
добычи расходится в трёх направлениях. Боевые башни можно строить только на
серой зоне вторжения; на зелёной нельзя. Общая walkable/buildable mask не
равна разрешению для боевых башен: принадлежность к серой зоне проверяется
отдельно при preview и покупке. Будущие мирные здания на зелёной зоне остаются
отдельной поздней механикой, без реализации сейчас. Точные размеры, очертания
серой зоны и расстояния до ресурсов подбираются в первом игровом забеге.
Reason: игрок выбрал угловую композицию и явно задал ограничение строительства.
Supersedes: пространственную композицию D-002/D-010 и фразу D-015 о том, что
зонирование башен относится только к будущему. D-011 о единой плоскости и
D-009 о динамическом маршруте/осаде сохраняются.
Implementation status: правило башен внесено в Defense; отдельный компактный
`Stage2Corner32x32.asset`, его builder и Integration-сцена реализуют угловую
композицию. Автоматические проверки, Windows startup, один человеческий
законченный забег и подтверждение финального стартового вида закрыли Stage 2
gate. Размер и темп остаются пробными данными.

## D-017 — One New Agent Per Stage

Status: player-directed, 2026-09-25.
Decision: каждый новый Stage 2–7 начинается в новой задаче Codex с новым
агентом. Агент получает промпт соответствующего этапа из
`reports/development-roadmap.html`, проверяет gate предыдущего этапа и ведёт
последовательные работы внутри своего stage. На границе он оставляет краткий
handoff в каноническом плане и `production/WORKSTREAMS.md`. Отдельный агент
для каждой мелкой правки не требуется. Stage 0–1 уже закрыты; их промпты
используются только при конкретном регрессе.
Reason: пользователь хочет явный переход ответственности между этапами без
возврата к большому дроблению задач и расходу токенов.

## D-018 — First-Wave Time And Rapid Gold Price

Status: player-directed, 2026-09-25, before Stage 3.
Decision: все три волны ждут 180 секунд подготовки; первый запрос касался
первой волны, затем игрок распространил время на следующие. Ранний старт по
Space остаётся. Обычная башня Rapid (позже визуально вышка с
лучниками) стоит 5 золота, 2 древесины и 1 камень, без металла. Игрок начинает
каждый забег с 5 золота и получает 1 золото за каждого убитого врага. Выход
врага к трону и очистка врагов при рестарте не оплачиваются. Золото хранится
в инвентаре как валюта, не добывается на карте. Цена Longshot не меняется.
Reason: явная правка темпа и экономики от игрока до начала Stage 3.
Supersedes: пробные 45 секунд первой волны и прежнюю цену Rapid. Не меняет
D-013 о цикле трёх добываемых ресурсов и D-016 о серой зоне башен.

## D-019 — Stage 3 Tactical Camera And Shared Ground

Status: Accepted direction and implementation scope, 2026-09-25.
Human request: follow the supplied TD/Dota-style examples for camera and tower
placement; change the camera and its integration now if necessary, record this
in Stage 3 and both HTML reports, then launch the playable result.
Decision: the active Stage 3 pilot uses a shared flat ground with a fixed,
elevated orthographic camera, upright runtime 3D towers, visible foundations,
contact shadows and readable square placement cells. A tower-specific camera
illusion is superseded. Adapt ground projection, mouse picking, preview,
camera following and temporary actor visuals together. Keep prices, combat,
navigation occupancy and run rules independent of visual assets.
Implementation: opt-in planar map copy; XY remains the rules/ground plane,
height is -Z. Existing Stage 2 data and its legacy view remain a regression
baseline. The runtime 3D pilot is the current implementation direction;
final visual acceptance and production budget remain open. Do not start
unapproved models or mass art production. The provided screenshots are visual
references only, not assets with redistribution rights.
Supersedes: the Stage 3 sprite-first working recommendation and the temporary
individually tilted tower presentation, not the accepted Stage 2 gameplay gate.

## D-020 — Tactical Mouse Zoom

Status: Superseded by D-021, 2026-09-25.
Decision: the Stage 3 playable camera zooms with the mouse wheel while keeping
the accepted fixed angle and ground picking. Its current opening scale is the
reference: maximum approach is 2× closer, maximum retreat is 4× farther.
The view outside the small playable map has ground, with no new buildable cells.
Reason: the player wants to inspect towers closely and read the wider field.
Supersedes: the earlier D-019 working note that the tactical pilot had no wheel
zoom; the fixed camera angle remains accepted.

## D-021 — Stronger Tactical Zoom

Status: Superseded by D-022, 2026-09-25.
Decision: after reviewing D-020 in the game, the player found both zoom
directions too weak. The tactical camera now moves 1.8× in scale per standard
wheel notch and clamps to 4× closer or 8× farther than its opening view.
The fixed angle and ground picking remain; visual ground extends beyond the
playable map at the far limit without adding buildable cells.
Supersedes: D-020 zoom speed and range.

## D-022 — Device-Independent Wheel Step

Status: Superseded by D-023, 2026-09-25.
Decision: keep the D-021 range (4× closer, 8× farther), but count any nonzero
mouse wheel delta as at least one deliberate step. One step changes orthographic
size by 2.5×; larger deltas can represent multiple steps. The fixed angle,
ground intersection and playable cell boundary stay unchanged.
Reason: after D-021 the player still found wheel zoom extremely weak. Dividing
all raw deltas by 120 makes small device-reported values nearly invisible.
Both small and standard deltas require runtime test coverage.
Supersedes: D-021 wheel speed and raw-delta interpretation.

## D-023 — Player-Tunable Zoom Strength

Status: player-directed, 2026-09-25.
Decision: the Stage 3 camera keeps the D-022 wheel-delta normalization and
4× closer / 8× farther limits. Its per-step multiplier is now a persistent
Unity asset exposed as a slider in the Inspector while the game runs, from
1.05× to 3×. The starting value is 1.4×. The player can tune it live in the
right-hand Inspector shown in their screenshot; editing the asset survives
leaving Play Mode and carries into later builds. Camera angle and game rules
stay fixed.
Reason: the D-022 hardcoded step felt too abrupt to the player.
Supersedes: D-022's fixed 2.5× step, not its input normalization.

## D-024 — Hero-Centered Zoom Within The Map

Status: superseded by D-025, 2026-09-25.
Decision: the Stage 3 tactical camera keeps the hero at the screen center. Its
maximum orthographic size is limited by the nearest playable map edge at the
hero's current position and by the viewport aspect ratio, so no view ray shows
the area beyond the map. Near an edge the camera automatically moves closer.
Mouse-wheel input changes a target size; the visible scale eases toward it while
the hero follow itself remains exact. The persistent D-023 step slider remains.
The decorative ground outside the map is removed. No player movement or
gameplay boundary changes.
Reason: the player saw the area beyond the small map, an off-center hero, and
abrupt wheel jumps in the real game view.
Supersedes: D-023's fixed far zoom limit and the tactical viewport focus clamp;
keeps its live tuning asset and wheel-step interpretation.

## D-025 — Fixed Zoom During Movement And Playable-Map Border

Status: player-directed, 2026-09-25.
Decision: WASD movement does not change tactical camera scale. Mouse wheel
alone changes the smooth zoom target, with maximum zoom-out 2× the opening
orthographic size. The hero remains centered. A finite, visibly distinct
non-walkable terrain border surrounds the existing 32×32 playable cells so
the 2× viewport remains on rendered ground even when the hero reaches an edge.
The border has no gather, build or navigation cells; the playable map and
rules remain unchanged. The D-023 Inspector step control remains.
Reason: the D-024 position-dependent zoom visibly changed scale while moving
and prevented any useful zoom-out near the starting corner. The player chose
the non-walkable border to reconcile 2× zoom, center follow and no empty frame.
Supersedes: D-024's position-dependent size limit, not center follow or smooth
wheel interpolation.

## D-026 — Closer View Limit From Player Screenshots

Status: player-directed, 2026-09-25.
Decision: reduce the maximum tactical close-up to 2× the opening view, or
0.5× its orthographic size. The prior 4× close-up (0.25× size) made the tower
and hero much larger than the player's target screenshot. Keep D-025's 2×
zoom-out, smooth wheel interpolation, hero-centered follow, non-walkable
border and persistent Inspector step strength.
Reason: the player's two in-game screenshots show the desired closest framing
at roughly half the apparent object size of the current maximum close-up.
Supersedes: only D-025's near zoom bound.

## D-027 — Runtime 3D Chosen For Stage 3

Status: player-approved, 2026-09-25.
Decision: use original runtime 3D models in the Unity game, not sprites
rendered from 3D. The player accepted the revised Rapid watchtower and the
shared tactical camera after reviewing the playable game. Keep the fixed
orthographic view and gameplay rules independent of the visual assets. For
each additional model, first discuss its role and proposed appearance with
the player, generate image concepts for their visual approval, and only then
model it in Blender and integrate it into Unity. This decision does not approve
the appearance, scope, or production budget of future assets.
Reason: the player explicitly chose 3D after reviewing the pilot in the game.
Supersedes: D-019's open runtime-3D-versus-rendered-sprites choice, not its
camera, ground, placement or gameplay contracts.

## D-028 — Longshot Stone Artillery

Status: player-directed, 2026-09-25.
Decision: Longshot becomes a stone-throwing artillery tower. Its price is two
wood and five stone, with no metal. It retains the current range, firing
interval and two damage, but each shot damages every enemy within a radius of
two map cells around the selected enemy's position; every affected enemy takes
the full two damage. The player chose the catapult-with-bowl concept in
`art/concepts/longshot_stone_catapult_option_a.png`. Blender modeling and Unity
model integration require a separate explicit go-ahead from the player. Metal
remains on the map and gatherable for future towers; do not change it now.
Reason: the player specified a stone artillery identity, exact price, splash
radius and damage behavior, selected the concept, and explicitly held modeling.
Follow-up, 2026-09-25: the player explicitly authorized building the selected
bowl-catapult model and asked to launch the playable game after completion.
The modeling hold above is therefore lifted for this specific model.
Supersedes: Longshot's earlier wood/stone/metal price and single-target role in
the Stage 2 prototype. D-018's Rapid price is unchanged.

## D-029 — Visible Tower Firing And Impacts

Status: player-directed, 2026-09-25.
Decision: both existing towers need visible firing and impact animation in
the playable game. Rapid fires arrows; Longshot animates its bowl mechanism,
throws a stone and shows its area impact. This is required Stage 3 feedback,
not a static-model-only delivery. Rules must remain independent of visual
assets. Exact animation and flight timings are implementation candidates to
review in play, not newly accepted balance values.
Reason: after inspecting the catapult delivery, the player explicitly said
both towers were static and required firing and impact animations.
Supersedes: the prior delivery's exclusion of arm/projectile/impact animation.
Follow-up: the player rejected tower recoloring, Rapid's recoil/shaking and
the first catapult animation. Towers must keep their authored color, Rapid
stays stationary, and the catapult's cup motion and stone release must be
continuous and synchronized.
Further clarification from the player's annotated screenshot: the complete
inner mechanism must turn around its own centered vertical axis when aiming
at enemies; the masonry foundation and outer parapet remain fixed.
Follow-up: after inspecting the corrected playable build, the player accepted
the centered rotation on 2026-09-25. This accepts the catapult correction only;
the commercial MVP content scope and Stage 3 gate remain open.

## D-030 — Landscape Before Final Hero Model

Status: player-directed production order, 2026-09-25.
Decision: leave the existing basic hero marker in the game and defer the hero's
final model until the very end of Stage 3 visual content. Work next on the map
landscape. The player supplied a bright grassy reference with small white
flowers and rounded low bushes. This establishes an aesthetic reference for
green terrain, not approval of a specific model, gray defense surface, playable
height change, or runtime use of the supplied image. Present image concepts
before Blender or Unity integration. D-011's shared flat gameplay plane and
D-016's gray-only tower placement remain in force.
Reason: the player explicitly changed the production order and supplied a
visual reference for the landscape.
Follow-up: the player selected landscape concept B, with a grassy green zone
and compacted gray earth. Bushes, rocks and other raised details must be real
3D props rather than painted relief. The first terrain pass may omit them.
The selected screenshot is an appearance target, not a runtime texture.
The player rejected the first playable texture-only pass as blurry and far
from the approved concept. A later pass used sharper original grass art and
separate 3D foliage geometry; the player rejected those bushes and requested
visible height between grass and gray earth. Bushes are removed from the
runtime scene and source assets. The turf's slight visual lift does not alter
the flat gameplay contract above. Visual acceptance remains pending.
Latest correction: use concept B as the appearance target for the grass
texture and the transition around every side of the gray zone, without
bushes. The player rejected the sharp dark ledge. The current visual pass uses
a shallow irregular grass verge; gameplay remains flat. Human visual approval
is still pending.
The player then judged the first concept-B runtime pass better but still far
from its reference. The next pass uses new original grass and earth albedos,
shallow real grass-surface relief, and a wider irregular verge; no bushes return.
This is a visual candidate awaiting human approval, not a new game rule.

## D-031 — Soul Sphere Objective Concept

Status: player-selected visual direction, 2026-09-26.
Decision: the loss-critical objective's central object is one smooth sphere
containing the hero's imprisoned soul, protected by a smooth uncracked blue ice
dome. Thin energy strips travel around it in a circular wave. The player chose
the fourth presented concept, `art/concepts/throne_soul_sphere_ice_dome.png`,
and explicitly authorized Blender modeling. The prior ice throne and obelisk
concepts are historical and not approved runtime targets. This selects the
visual form; it does not change throne health or combat rules.
Reason: the player corrected the central form and selected the revised image.

2026-09-26 correction: the player rejected the initial runtime result. The
objective must cover approximately three by three cells, and its external
energy must flow horizontally around the ground normal, never vertically.
The subsequent reference clarification requires clearly separate slate base
and raised bowl, four stone anchors, a blue soul flame inside the central
sphere, a smooth transparent protective dome, and external flowing arcs.
Uniform luminous hemispheres that conceal those elements are rejected. Nine
cells centered on the existing throne are reserved in Stage 3 map building
data; the rule target remains its existing central cell. No new health,
movement, attack, resource or loss rules are authorized by this visual change.
2026-09-26 acceptance: the player visually approved runtime revision 3 shown
in `art/review/soul_sphere_game_close.png`. Its separate stone bowl, four
anchors, soul flame, inner sphere, protective dome and horizontal outer flow
are the accepted Stage 3 objective. This does not close the Stage 3 gate.

## D-032 — Minimal Commercial MVP Content

Status: accepted by the player, 2026-09-26.
Decision: Stage 3 completes the existing compact map and short finite run with
the current gameplay rules. Its model set is Rapid, Longshot, the accepted soul
objective, the entrance portal, one baseline enemy, the wood/stone/metal source
objects, and the hero last. Until then the hero stays a basic marker. The HUD
must communicate inventory and prices, tower selection and placement, wave,
objective health, available actions and run outcome. Short audio cues cover
gathering, building, each tower's attack and impact, objective danger, wave and
outcome. No additional gameplay mechanics are accepted by this decision.
The player later specified an undead visual identity for the existing single
baseline enemy; this does not authorize a new enemy role or altered rules.
Each remaining model still requires its own player-approved image concept
before Blender production or Unity integration. Stage 3 remains unclosed until
the accepted content works in a complete playable run; Stage 4 waits for it.

## D-033 — Intact Dark Red Enemy Portal

Status: player-approved image and modeling, 2026-09-26.
Decision: the enemy entrance uses the original concept
`art/concepts/entrance_portal_option_b4_dark_red_ring.png`: complete dark stone
ring with an intact base/steps and a red inward vortex. It must feel ominous
because the existing baseline enemy will be undead. The broken-ring B and
spectral green/black B3 concepts are rejected; no ruined silhouette or change
to the red interior is approved. The player approved B4 and explicitly asked
to proceed with the model in Blender and the playable Unity scene.
Spawn location, enemy role and navigation remain data/rule contracts. This
visual approval does not close the Stage 3 gate or accept the eventual runtime
model before its playable review.
Placement correction, 2026-09-26: the player requested a 90-degree turn from
the first runtime placement and a 3x3 size matching the throne. The entrance
now faces the +X objective route. Reserve the centered nine cells in authored
building data; keep the existing spawn cell and walkability contract.
The actual tactical screenshot showed the aperture edge-on at that bearing.
The player explicitly chose to keep the exit toward the objective, as shown,
rather than face it toward the camera. The later 45-degree correction in D-035
supersedes the exact edge-on bearing while retaining the exit toward the goal.
Animation clarification, 2026-09-26: the player reported the red interior as
static and explicitly requested circulation. The accepted red vortex must
show continuous, visible movement inside the fixed stone ring.

## D-034 — 100×100 Diagonal Invasion Corner

Status: player-directed, 2026-09-26.
Decision: before the next Stage 3 model, replace the horizontal gray strip in
the playable 3D scene with a diagonal invasion zone starting in the upper
right corner and running down-left. The player specified a 100×100 map,
approximately 30 cells of corridor length and 12 cells of width. The portal
is at the upper right; the objective is lower and left within that zone.
The remainder is green territory with the accepted three resource kinds.
`C:/Users/MushketCO/OneDrive/Рабочий стол/Без имени.png` is the player's
layout mockup for the in-game composition, not a runtime texture or model.
The same three-wave short run, tower building only on gray, 3×3 portal and
objective footprints, and asset-independent navigation rules remain in force.
This Stage 3 layout supersedes the 32×32 horizontal prototype under D-016.
The Stage 2 map asset stays as historical regression coverage. Stage 3 gate
remains open; Stage 4 waits for a separate agent.

## D-035 — Diagonal Portal Bearing

Status: player-directed, 2026-09-26.
Decision: on the D-034 diagonal map, turn the already approved B4 enemy gate
to a 45-degree diagonal bearing. Keep the complete dark stone ring, red inner
vortex, its exit toward the objective, and the square 3x3 foundation within
the reserved nine cells. This is a placement correction to the accepted
model, not approval for another model or for changed spawn/navigation rules.
Stage 3 gate remains open.

## D-036 — Restore Tactical Zoom Scale On The 100×100 Map

Status: player-selected, 2026-09-26.
Decision: restore the Stage 3 camera's former opening orthographic size 5 and
wheel limits 2.5 (closest) to 10 (farthest). The D-034 map builder had raised
the opening size to 17, which changed the effective limits to 8.5–34 and
made even the closest view feel far away. The player explicitly chose the
former scale after being told that the portal and objective will start outside
the frame while the hero remains by the resource area. Keep the fixed angle,
hero-centered follow, smooth wheel and persistent step-strength setting.
The 100×100 map and its landmark positions do not change. Stage 3 gate stays
open; opening guidance/readability remains to be assessed in the playable run.

## D-037 — Granite Stone Source Appearance

Status: player-selected, 2026-09-26.
Decision: use `art/concepts/stone_source_option_a_granite_boulder.png` as the
appearance direction for the existing gatherable stone node: one pale granite
boulder with small stones at its base. The layered stone B is unselected. The
player requested the resource concepts together before producing all three
models. The concept
is a review image, not a runtime texture or mesh source. Existing economy and
map rules remain unchanged; verify every model in the playable Stage 3 scene.
Stage 3 gate remains open.

## D-038 — Conifer Wood And Rust-Seamed Iron Ore Sources

Status: player-selected, 2026-09-26.
Decision: model the existing wood node after
`art/concepts/wood_source_option_b_conifer.png`, a compact conifer, and the
existing metal node after
`art/concepts/iron_ore_source_option_a_jagged_outcrop.png`, a dark jagged rock
with visible rusty iron seams. The broadleaf wood A and upright-slab ore B
are unselected. Produce these with approved stone A as one resource-source
set, then check every model in the playable scene. Iron ore remains the visual
form of the existing metal resource. The concepts are review images, not
runtime textures or mesh sources. Gameplay and map contracts stay unchanged;
Stage 3 gate remains open.

Owner correction, 2026-09-26: the first three runtime models were rejected
for an excessively low-poly appearance. Rebuild their forms and materials
to match the already approved A/B/A concepts. This is implementation rework,
not a change of resource choices or authorization for additional content.
The owner also rejected the second, smoothed/textured implementation after
reviewing its actual Game frames. This second rejection does not revoke the
selected concepts; both produced versions remain visually unaccepted.
The owner then explicitly reaffirmed that the selected concepts are correct
and that the failure is their in-game realization. Keep A/B/A as the target.
After reviewing the stone-only revision, the owner narrowed the remaining
stone correction to smoothing and explicitly directed continuing with the
other resources. Correct the stone shading and proceed with wood B and ore A;
do not restart concept selection. This does not close the Stage 3 gate.

## D-039 — Cartoon Skeleton Sprite Walk Trial

Status: player-authorized visual trial, 2026-10-15; runtime appearance awaits review.
Decision: use the skeleton from cartoon concept B in
`art/concepts/sprite_style_review_2026-10-15/b_cartoon_2d.png` to test direct
sprite production. Create a walking animation and bind it to the existing
baseline enemy in the playable Stage 3 scene so it emerges from the portal.
This is a visual replacement only: no new enemy role, health, speed, wave,
pathfinding, siege, targeting, damage or reward change is authorized.
The trial uses four directions with six frames each, distance-driven playback,
an unlit transparent billboard and a ground contact shadow. Pause/stationary
behavior and existing hit feedback remain observable. Original imagegen art
and its exact prompt/provenance are retained in the project.
Supersedes: D-027's runtime-3D-only production direction for this one enemy
trial. It does not choose the final pipeline for all assets or approve the
current 3D towers. Stage 3 remains open until the owner's live review and its
other outstanding work are complete.

## D-040 — Portal And Soul Objective As Cartoon Sprites

Status: runtime appearance approved by the player, 2026-10-15.
Decision: reuse the saved B4 dark-red portal and the selected soul-sphere ice
dome references, redrawing them as smooth cartoon sprites for the current
playable Stage 3 scene alongside the skeleton. Preserve the complete dark
stone ring/red vortex and the distinct slate base, raised bowl, four anchors,
inner sphere/flame, clear dome and horizontal cyan energy of the objective.
Use original project-generated RGBA images with the same fixed tactical view.
Red circulation and travelling cyan highlights/flame shimmer stay animated;
explicit pause freezes them. Map anchors, portal bearing contract, 3x3 building
exclusions, spawning, objective health/loss and all combat rules stay intact.
Original 3D prefabs and Blender sources remain available, but the current
scene's existing visual fields point to the new sprite prefabs. Sorting uses
actual ground anchors, and depth testing against opaque scene objects remains.
Supersedes: D-027's runtime-3D-only visual direction for these two landmarks;
does not revoke D-031/D-033 reference identities or approve a whole-project
sprite migration. Stage 3 remains open pending owner review and remaining work.

## D-041 — Cartoon Towers And Terrain, Existing Map Retained

Status: player-authorized implementation, 2026-10-15; new tower/terrain appearance awaits live review.
Decision: use the project's original cartoon B board as the visual target for
Rapid, Longshot, green turf and gray stone paving. The owner explicitly
withdrew the initial request to reshape the map: retain the 100x100 world,
diagonal invasion area, portal/objective/resource positions and fixed camera.
Replace the two tower visuals with original transparent sprites, a red-roof
wooden watchtower and a stone-enclosed spoon catapult. Sprite artwork supplies
its own three-quarter perspective instead of rotating a billboard with the grid.
Use direct imagegen terrain textures, clearer stone courses and a soft visual
grass verge. No new wall, prop collision, building, resource or rule is authorized.
The catapult uses four drawn aiming directions and a windup/release/return
sequence on the existing shot clock; Rapid stays still while firing arrows.
This adapts D-029's presentation to drawn frames rather than continuous 3D
yaw/arm articulation; damage, release delay, impact and pause remain observable.
Supersedes: D-027 runtime-3D-only direction for these two towers and the old
landscape pigment choice. Original 3D assets remain available. The owner also
approved the preceding D-040 landmark appearance; remaining Stage 3 work and
the final pipeline for all resources/hero are still open.

## D-042 — Rework rejected cartoon terrain and tower grounding (2026-10-15)

Status: accepted scope; revised artwork awaits owner review.
The owner rejected the first D-041 result as far from the original cartoon B
reference and explicitly authorized matching its bottom GAMEPLAY strip:
soil between short turf, grass in worn paving joints, little flowers/bushes and
broken roadside stone masonry. This supersedes D-041's no-wall/no-bush limit
for decorative artwork only. The 100x100 map, diagonal invasion zone, anchors,
resources, fixed camera, build/navigation/collision rules and combat values stay.
The owner also requested reworking both buildings, particularly the spoon
catapult throw and contact with ground. Keep Rapid's red roof and the spoon
catapult identity; the approved portal and soul objective are not redesigned.

The revision uses original imagegen V2 turf/paving, roadside sprite props and
planted tower artwork. Catapult masonry/support is a SINGLE stationary sprite;
a separate spoon and loaded stone articulate smoothly on the existing shot
clock. The actual cup supplies the sampled release socket. This supersedes
D-041's whole-building frame swaps, which visibly changed the foundation.
This is still a 2D articulated sprite presentation, not a new 3D tower pipeline.
Original assets remain retained; no new mechanics or package is authorized.

## D-043 — Keep the playable map inside the tactical camera (2026-10-15)

Status: player-authorized implementation; visual review pending.
The owner does not want the expanded non-walkable visual border to appear as a
cut-off map edge. In the Stage 3 tactical view, follow the hero while the whole
visible ground footprint fits inside the playable map. At the edge, stop camera
translation and let the hero move toward the screen edge. Recalculate the clamp
from the current orthographic size, aspect and fixed view angle as zoom changes.
Retain the 100×100 map, decorative border as unseen backing, hero controls,
zoom range and all gameplay rules. This supersedes D-019's always-centered hero
and visible-border approach for the tactical scene only; the Stage 2 camera
mode remains unchanged.
After live review revealed that the hero's body could extend past the screen
edge, Stage 3 movement stops one cell inside the authored center bounds. This
is visual clearance for the full hero silhouette at all tactical zoom levels;
the map, resource positions and Stage 2 movement remain unchanged.

## D-044 — Preserve all references in the public project repository (2026-10-15)

Status: explicitly accepted by the owner for the Mac development handoff.
The existing GitHub repository is public. After being told that anyone can
view and download its contents, the owner chose to save the full project there,
including historic visual references with unknown source/license. The eight
files under `concepts_visual_scene/` and the player-supplied image under
`art/references/` are indexed in `concepts_visual_scene/README.md` with their
rights status; they remain outside Unity `Assets/` and are not runtime art.
This decision records storage and visibility only. It does not establish a
license, authorize copying any reference into the game or change the Windows
MVP target. The current public Git `main` is the Mac handoff source. An older
local ZIP snapshot predates the later removal of optional QA recordings.

## D-045 — Winding invasion-route concepts and larger-world direction

Status: I route shape accepted by the owner, 2026-10-15; Unity implementation
verified on Mac (EditMode 46/46, PlayMode 42/42). Owner tested and accepted the
changed road; the separate grass-appearance question and pacing assessment remain.
The owner conditionally authorized moving the
portal from the upper-right to the lower-left if the map supports it; inspection
confirmed data-driven spawning and camera-facing portal artwork, so that option
is used. The alternative upper-right redraw is not needed.
Current owner direction (latest correction): a SINGLE HORIZONTAL DOUBLE
RECTANGULAR ZIGZAG, uniform about SEVEN tower ground footprints wide, portal
at one end and throne at the other, no branches. Portal is now lower-left;
the throne occupies the far right endpoint. Keep the B Cartoon 2D
gameplay-strip paving/grass style. This supersedes the earlier organic-curve,
variable 7–8/3-width and later single vertical five-wide studies below.
The current candidate is `i_horizontal_double_zigzag_7wide.png`; exact guide,
prompts and provenance: `art/source/horizontal_double_zigzag_7wide_2026-10-15_*`.
The original SVG specifies width 7 with nine segments/eight square turns;
the generated illustration guided the measured Unity map. Acceptance applies
to I's direction and route shape; the owner subsequently tested the live road
and confirmed it is fine. This does not settle the separate grass-art question.

Implementation uses the existing `Stage3PlanarCorner` asset and builder, with
seven-cell rectilinear strips in the existing 100×100 world. Portal and throne
positions, reservations and road masks are authored together. The old nearby
landmark pair is replaced by the long route; the initial roughly 2x endpoint
distance was an exploration target, not the scale of this selected seven-wide
double-zigzag blockout. Actual direct separation is 91.24 cells; unopposed
route is 195 steps (196 visited cells). This larger scale remains subject to
live pacing review, without silently changing wave/speed/economy tuning.
Enemy navigation has an opt-in map flag restricting it to gray cells; hero
walkability remains unchanged on green resource land. Tower detours, full-block
siege, economy, waves, input and camera orientation remain existing rules.

Earlier exploration history:
Decision: first compare original ImageGen references for a winding gray
invasion/building zone between the existing portal and soul objective. The
portal-to-objective distance measured in a straight line should be roughly
twice the present test layout; winding travel adds further distance. Current
anchors (46,44)/(39,39) are 8.60 cells apart, so the first comparison targets
17.20 cells direct separation. These are design targets, not measured pixels
or an implemented map.

The owner described the future world scale using Satisfactory/Factorio as
an ambition for a large explorable world. This selects neither final world
bounds nor factory/logistics mechanics. Treat the defense zone as one district
within that future world. References compare A wide S, B broad zigzag and C
large arc/detour under `art/concepts/invasion_route_review_2026-10-15/`.
Prompts/source hashes are in `art/source/invasion_route_concepts_2026-10-15_*`.

This authorizes exploring a replacement for D-034/D-041's straight diagonal
layout; the live scene remains unchanged until a candidate is selected.
Current enemy navigation permits green terrain, so a future implementation
must explicitly keep enemies inside the intended invasion zone to prevent
shortcuts, while retaining hero access to resource land and dynamic local
tower detours/siege. No waypoint/patrol system, world streaming architecture,
new content or balance change is selected by these concepts. Stage 3 stays open.

Owner clarification, 2026-10-15: this is the original player-shaped/mazing TD
contract. The gray surface must be a wide construction battlefield in which
the player places alternating rows of towers to lengthen enemy travel while
leaving a connected passage. Width comes first; the terrain's large, gentle
bend must accommodate several rows and turning space. A narrow pre-authored
serpentine road does not meet that intent. The first A/B/C illustrations were
too road-like, and A's earlier recommendation is withdrawn.

Revised references in the same concept folder: `d1_open_building_field.png`
and `d2_player_built_maze.png` show the open field and the same field with
alternating tower barriers. Gold arrows in D2 explain the intended example
flow only. Prompts/provenance: `art/source/maze_field_width_review_2026-10-15_*`.
A 12–16-cell usable width is the agent's initial study hypothesis, not an
accepted size or measured image geometry. The roughly 2x direct endpoint
separation remains a target for blockout. Example tower counts do not set
economy or wave tuning. Full-block/siege behavior from D-009 remains intact;
the illustrated defense deliberately leaves an open route. No live map changed.

Further owner correction, 2026-10-15: D1/D2 are rejected as an oval/plaza-like
field. The requested gray area is still a NATURAL WINDING WIDE ROAD. Preserve
organic, substantial bends and uneven shoulders together with enough width
for tower rows and enemy passages; neither a skinny pre-made maze nor a broad
straight arena is the target. The earlier phrase "too road-like" overstated
the width correction and must not guide implementation. New candidate E,
`e_natural_wide_road.png`, restores an asymmetrical meander and worn irregular
edges. Exact prompt/provenance: `art/source/natural_wide_road_2026-10-15_*`.
The owner subsequently rejected E as still insufficiently wide. The explicit
width target is now 7–8 full tower footprints SIDE BY SIDE ACROSS broad
sections, with short narrow sections allowed to fit about 3. Measure usable
gray ground transversely to the local road direction; length along a bend
does not establish width. Keep natural meanders and smooth width transitions.
This supersedes the agent's 12–16-cell hypothesis, not the tower-made maze or
full-block/siege rules. New F scale study: `f_variable_width_road.png`, with
countable 8/3-tower overlays; prompts/provenance:
`art/source/variable_width_road_2026-10-15_*`. The painted perspective is not a
measured blockout and F has no owner acceptance. The owner next requested
the original B cartoon board's lower gameplay-strip road as the reference
to inspect: `art/concepts/sprite_style_review_2026-10-15/b_cartoon_2d.png`.
The owner confirmed that B's road is the desired visual direction, then
requested a uniform roughly five-wide rectangular zigzag with no branches.
H followed that single/vertical study; the next correction widened it to
seven and requested TWO zigzags arranged horizontally. I is the current
candidate for that correction, using original geometry guidance to prevent
branching and confusing tower counts. Those concept rounds ended before the
owner acceptance and runtime implementation described at the top of D-045.

## D-046 — Mac trackpad pinch zoom

Status: requested and clarified by the owner, 2026-10-15. Implementation and
automated Mac checks passed; the owner tested the physical gesture in the live
Game view and confirmed "Да, работает".
Two-finger spread zooms in and pinch zooms out through the existing tactical
camera smoothing and original 0.5×..2× size limits (currently 2.5–10).
Mouse wheel remains available. No camera rotation or movement change is selected.

A narrow original AppKit native plug-in reads app-local magnify events only;
the Editor accepts them while the live Game view is focused and under the
pointer. Focus loss, pause, Play exit and domain reload clear pending input.
Universal ARM64/x86_64 source, build script and macOS-only importer metadata
are kept in the project. Windows/Linux exclude this library and native calls.
This supports Mac development and does not change the Windows/Steam MVP target.
Native checks and Unity PlayMode 43/43 passed; the live Console shows 0 errors
and 0 warnings. Tests verify native loading and zoom contracts separately from
physical gesture delivery, which is now confirmed by the owner and live input
diagnostics. Rebuild/wiring and evidence are linked from
`gamedesign/features/STAGE3_TACTICAL_VIEW.md` and the current handoff.

## D-047 — Bottom-left minimap now, wider UI later

Status: explicitly requested by the owner, 2026-10-15; implemented and verified
on Mac (PlayMode 46/46 and interactive visual/input review). Owner appearance
feedback remains open.
Add the minimap in the lower-left corner as one useful UI element during the
current mechanics/visual work. This does not start a wider HUD redesign or
select the final UI production stack.

Show the authored road/world, live hero, portal/throne, available resources,
towers/enemies and the main camera's ground footprint. Keep the map orientation
fixed, adapt to viewport size and update from existing run state, including
depletion/removal/restart. Use a passive map for this first version; no new
camera-pan/movement command, exploration/fog rule or game tuning is selected.
The panel blocks world placement and camera zoom through its area. Source of
truth and acceptance: `gamedesign/features/STAGE3_MINIMAP.md`.

## D-048 — Resource sprite previews and remove direction labels

Status: owner-requested, 2026-10-15; labels removed and live-verified.
The owner approved all three sprite previews and explicitly requested runtime
integration. Implemented with exact PNG copies; Mac PlayMode 46/46 and live
review of all three sources passed. Owner feedback on the live result remains open.
Remove floating resource labels such as `Stone W+A` / `Wood W+D` and their
keyboard-direction suffixes in the temporary resource readout. Resource counts,
distance readout, nearby E-gather prompt and minimap markers retain their roles.

Prepare previews now for cartoon sprites of the existing wood, stone and metal
sources. The owner clarified that the voice-transcribed timing means "now,
previews". Keep D-037/D-038's selected identities: compact conifer B, pale
granite boulder A and dark iron outcrop A with rusty seams. Match the current
cartoon B sprite style and fixed tactical angle. The initial request covered
previews; the subsequent approval authorizes using these exact images as the
three existing resource visuals in the game. Preserve resource/economy/map
rules, fixed camera, terrain and original concepts/models. New resource types
or gameplay tuning are not selected.
Candidates: `art/concepts/resource_sprite_review_2026-10-15/`; exact prompts,
generation/refinement sources and hashes: `art/source/resource_sprite_preview_2026-10-15_*`.

## D-049 — Expand green world around the accepted road to 250×250

Status: explicitly accepted by the owner, 2026-10-16; implemented and verified
on Windows (Map/Navigation EditMode 47/47, Stage3/Integration/RunState PlayMode
47/47, successful Development build and standalone startup).
Expand the existing Stage 3 map from 100×100 to 250×250 by adding green land
around the current road. Keep the road and combat area unchanged. Bounds grow
75 cells on each side with no rescaling or translation of the existing cells.
Preserve the accepted double zigzag, its seven-cell width, build masks and
reservations, portal/throne, hero start, all three resource anchors and rules.
New forest permits hero movement but neither combat towers nor enemy routes.
Camera bounds and the passive minimap cover the larger world using existing
behavior; camera angle and zoom limits remain as approved.
This supersedes D-034's world dimensions and D-045's 100×100 world bounds,
while retaining D-045's accepted route (1,582 gray cells; 195 unopposed steps).
New resource placements, content, mechanics, streaming and balance changes
require their own current requirement. Stage 3 remains open.

## D-050 — Keep the combat district in the lower-left world corner

Status: explicitly requested by the owner, 2026-10-16; implemented and verified
on Windows (Map/Navigation EditMode 47/47; Stage3/Integration/RunState PlayMode
47/47; successful build and startup). Owner game is open for review.
The road from portal to throne and the whole combat district must sit in the
lower-left corner of the 250×250 world. Preserve the accepted road layout and
all existing object/resource/hero coordinates by keeping the original 100×100
map's lower/left bounds. Expand green land to the right and upward instead of
symmetrically around that district. New playable/camera bounds:
(-50,-50)..(199,199). The gray footprint lies wholly in the lower-left quadrant;
its left/bottom green margins are the same as before the world expansion.
This supersedes only D-049's placement of the expanded bounds. The 250×250
size, 1,582 gray cells, seven-cell road width, 195-step route, existing anchors,
build/navigation/economy rules, fixed view and zoom settings remain unchanged.
No content or resource redistribution is authorized by this composition fix.

## D-051 — Curved seven-wide invasion road from the owner's sketch

Status: explicitly requested and clarified by the owner, 2026-10-16;
implemented and verified on Windows (EditMode 47/47, PlayMode 47/47, final
route/render 1/1, successful build and visible startup). Owner review remains.
The owner confirmed red left = enemy portal, blue right = defended throne.
Replace the rectangular zigzag with the sketched smooth route: upper-left
portal, first low bend, central rise, second low bend, upper-right throne.
Keep about seven-cell width and the lower-left district of the 250×250 world.
This supersedes D-045's rectilinear geometry and D-050's preserved endpoint
coordinates/road hash; D-050's world bounds and district placement remain.

Author original cubic geometry in the existing Editor builder and bake it to
the current map-cell format. The runtime camera, art, gray-only enemy policy,
tower-made detours/siege, resources and economy/combat tuning remain existing.
Portal (-42,12) and throne (55,12) follow the new endpoints; the right arm/goal
clear the unchanged metal node. Hero/resource coordinates remain unchanged.
The map bootstrap already positions landmarks from data; no serialized scene
reference or format migration, runtime spline layer, waypoint system or package.
The new route's pacing is measured separately; do not preserve the old 195-step
length by tuning speeds or waves. Source/provenance:
`art/source/curved_road_2026-10-16_reference.json`; the user image is a reference
outside `Assets/`, not a runtime texture. Stage 3 remains open.

## D-052 — Separate development tools from the player HUD

Status: owner-requested 2026-10-16; implemented and verified on Windows:
EditMode 29/29, Stage3/Integration/RunState PlayMode 52/52, final menu 5/5,
successful Development build and actual HUD/menu frame inspection. Owner
appearance feedback and the wider Stage 3 gate remain open.
The owner requested wave start, pause, speed up to x10, gold/material grants
and cleanup of temporary diagnostic windows. Clarification explicitly selects
two independent multipliers: hero movement and game simulation. Keep their
normal x1 defaults and the existing gameplay/catalog rules. Debug grants are
session tools; run restart resets inventory through the normal rules.

Use a hidden-by-default development menu and retain necessary player-facing
resources, wave/throne state, construction/gathering hints and outcome/restart
in a cleaner compact HUD. Remove technical source/distance tables and raw debug
text from normal play. Existing IMGUI is sufficient for this scoped Stage 3
iteration; this does not select the final commercial UI production stack.
Player HUD is also visible in non-development builds; cheats are development-only.
This supersedes D-047's deferral of this particular HUD cleanup and D-048's
retention of the temporary distance table. Minimap, camera and map rules remain.
Acceptance/wiring: `gamedesign/features/STAGE3_DEBUG_MENU.md`.

## D-053 — Preview large resource districts and extraction buildings

Status: owner requested visual references now, 2026-10-16; three original
concepts prepared. Appearance approval and runtime implementation remain open.
The owner is considering more enemies/waves/towers alongside peaceful
production. For now create references for (1) a large forest with an edge
sawmill and visible forest renewal, (2) a substantial stone deposit with a
quarry, (3) a stone-cut descending iron mine with an extraction station.
Show the resource district clearly larger than its building, with legible
material and production identities in the existing cartoon isometric direction.
Future placement should relate buildings to the appropriate resource district;
no placement/range/economy rule or content quota is selected by these pictures.

Deliverables: art/concepts/resource_districts_2026-10-16/. Exact prompts,
original generated paths, PNG dimensions/hashes and source/rights are in
art/source/resource_districts_2026-10-16_generation.json. Text-only built-in
imagegen generation; project art inspected for style, no third-party inputs.
These are previews outside Unity Assets, not approved sprites/models or scenes.
No exact footprint is approved: 50x50 means 2,500 cells, whereas the owner's
other spoken estimate was 200–300; choose size explicitly before placement.
This authorizes concept exploration without changing D-032's MVP boundary or
activating passive production/automation. A one-building pilot and a later
combat counter pair are recommendations, not accepted implementation tasks.

## D-054 — Procedural resource scenery from modular sprites

Status: owner requested implementation after the procedural-sprite explanation,
2026-10-16; explicitly selected 200–300 cells per initial district.
Implemented and verified: Map/Navigation EditMode 54/54, final Stage3/Integration/
RunState PlayMode 55/55, rendered two-seed variants and hero visibility. Owner
appearance feedback remains open.
Create varied forests, connected stone masses and iron excavations at world
creation using reusable individual sprite pieces, seeded placement/shape and
variation. Complete D-053 concept pictures are visual references, not repeated
runtime zone images. Exact size is data-controlled for later growth.
Keep the accepted combat road/objectives, existing manual resource nodes and
rules. This request activates resource-zone generation/presentation only;
passive production/building placement, costs, forest renewal mechanics and
underground traversal still require their feature brief. A normal run restart
retains this world; creating a new scene/world chooses a new seed. A seed can
reproduce the same scenery; save-file work is not included.
Source of truth: gamedesign/features/PROCEDURAL_RESOURCE_DISTRICTS.md.
This advances D-053 beyond preview-only for the resource district visuals;
D-032's commercial gameplay scope remains unchanged.

## D-055 — Hero collision for procedural resource districts

Status: owner requested 2026-10-16; implemented and Windows-verified.
Packed stone and iron districts must stop the hero at their ground contour.
Forests remain walkable between individual tree trunks; the owner allowed a
simpler fallback if needed, but per-tree ground circles fit the existing seeded
assembly. Canopies and transparent sprite rectangles do not become walls.
Use the generated footprint plus ground bases for stone/iron and the exact
seeded tree anchors for trunks. Preserve the camera, authored road, gathering
nodes, enemy movement and production scope. Continuous movement collision
must work with both debug speed multipliers, allow sliding, replace itself on
world regeneration and retain the same world on run restart. A solid generated
under the current hero recovers the hero to nearby free ground.
This supersedes D-054's collision deferral; production, renewal and underground
traversal still need separate briefs. Source of truth:
gamedesign/features/PROCEDURAL_RESOURCE_DISTRICTS.md.

Verification: Map/Navigation EditMode 63/63; final Stage3/Integration/RunState
PlayMode 58/58; real contact/forest frames and rendered hero visibility checked.
Windows build rebuilt and opened with a fresh random world, clean Gameplay/
Tactical Ready and unchanged map hash. Owner contact feel/appearance review
and wider Stage 3 gate remain open.

## D-056 — Structural resource scenery and stable partial occlusion

Status: owner requested refinement after D-055 live review, 2026-10-16;
explicitly selected procedural 3D relief/base plus painted sprites.
Implemented and Windows-verified: Map/Navigation EditMode 69/69; final
Stage3/Integration/RunState PlayMode 61/61, including nine district scenarios.
Real two-seed close views and paired hero visibility checked; controlled
walking/profile samples show no collision recovery jumps or hot-path managed
allocations. Windows build rebuilt/opened with clean Gameplay/Tactical Ready
and unchanged authored map hash. Owner appearance review remains open.
Replace disconnected random mine pieces with coherent depths, walls, landings
and connected stairs; make the cavity visible. Stone gets varied coherent
terraces; forest gets size-aware gaps and varied tree ages/scales. Blend ground
patches into the actual grass surface. Obstacles retain partial visibility and
reveal the hero with a local, eased transition rather than instant near-erasure.
Measure suspected forest microstutters separately from the confirmed alpha
toggle. Keep seed variation, initial district footprint, XY hero rules and the
existing road/economy boundaries. No production or underground hero traversal.
This supersedes D-054's sprite-only relief representation and D-055's abrupt
visibility fade; collision intent remains. Source of truth:
gamedesign/features/PROCEDURAL_RESOURCE_DISTRICTS.md.

## D-057 — Reference-faithful authored resource landmarks and locked building points

Status: Accepted by explicit owner correction, 2026-10-16; implemented, owner visual review open.
The owner rejected the D-056/Astra result artistically: a smooth grey terrain
core with rocks scattered on its edge does not match the original D-053 images.
The Mac 13/13 EditMode, 9/9 PlayMode and 3/3 contact checks establish technical
behavior only; they are not appearance acceptance. Preserve those results as
historical evidence, not as the current target.

Create three coherent compound landmark categories: forest, pale stepped stone
mass and dark iron excavation. Match the original D-053 composition, volume,
material and detail closely. Authored fixed local composition is explicitly
allowed when random assembly harms fidelity; safe seeded world placement may
remain random. Do not retain a generic relief core underneath a painted version
or require shape variation at the expense of the reference. The implementation
may use a coherent bitmap/layered artwork under the existing fixed camera.
Reduce the previous large objects by approximately 25 percent in linear size;
visual, ground footprint/collision and local anchor scale together, controlled
by runtime project data. Existing small gathering nodes are unchanged.

Large landmarks are not gathered with E. Each owns one authored locked local
building point with a matching type: forest/sawmill, stone/quarry, iron/iron
mine. This authorizes the spatial/type anchor contract only. Building models,
construction UI, prices, rates, production, renewal and underground traversal
are not approved. Existing small trees/stone/ore retain manual E gathering.
Road, map, targets, camera and economy remain unchanged. D-055 ground collision
intent remains, with authored contours/trunks replacing arbitrary mesh-derived
contours where needed. This supersedes D-054's mandatory modular random shape,
initial size and prohibition of coherent zone images, and D-056's mandated
procedural relief representation. Source of truth remains
`gamedesign/features/PROCEDURAL_RESOURCE_DISTRICTS.md`.

## D-058 — Constructible resource generators and categorized bottom build menu

Status: Accepted by explicit owner request, 2026-10-16; implemented on Mac.
The owner accepted the D-057 appearance ("теперь то, теперь оно") and requested
the matching sawmill, quarry workshop and iron extraction station as buildable
generators. Add a simple bottom menu with Towers (existing Rapid/Longshot) and
Generators (these three buildings). Use their existing locked matching points,
one building per point. Preserve the accepted landmark artwork and scale.

The owner explicitly delegated provisional prices and production rates. Keep
them in the production data asset, with the selected cost/output visible in UI.
Generators consume the shared inventory once at successful construction and
produce their corresponding resource over active game time. Pause/terminal
states freeze them, restart clears them, and regeneration invalidates old sites.
Keep manual E gathering on the original small nodes and the existing tower
placement/payment/road policy. No logistics, research, upgrades, workers,
renewal mechanics, selling or persistent/offline production is authorized.

This supersedes D-057's building/production/UI deferral only for this narrow
prototype. Details: `gamedesign/features/RESOURCE_PRODUCTION_BUILDINGS.md`.

## D-059 — Visible construction sites, red/green ghosts and affordability gating

Status: Accepted by explicit owner request, 2026-10-16.
The owner reported that construction positions and placement feedback were
unclear. Mark free generator points in advance with distinct ground icons:
axe/log for sawmills, pick/stone for quarries and pick/ore for iron mines. Show
the actual tower or building silhouette semitransparently while positioning,
green only for a valid purchase location and red when rejected. Include its
ground outline. No yellow/orange placement state is authorized.

Unaffordable cards must be disabled rather than letting the player select an
unfunded purchase. Show every price and retain an explanation on hover; use
the same affordability gate for public selection and existing tower hotkeys.
Recheck funds at commit. Preserve the existing costs, output rates, placement
policy, locked points and accepted landmark/building artwork. These are
readability and input refinements to D-058, not additional building content or
economy. Details: `gamedesign/features/RESOURCE_PRODUCTION_BUILDINGS.md`.

2026-10-17 appearance feedback: the owner finds the current pad glyphs hard to
read and requests a minimal vector concept for approval first. D-059 technical
passes do not establish glyph-art acceptance. The original SVG proposal under
`art/concepts/placement_icons_2026-10-17/` is preview-only; neither the proposed
ingot pictogram nor its integration has been accepted yet.
The parent v1 was subsequently rejected for distorted shapes/proportions. By
explicit owner request GPT-6 Astra supplied approval_sheet_astra_v1.svg/png in
that concept directory. It is the current proposal, still awaiting owner art
approval; neither this concept revision nor technical checks imply acceptance.
Following owner feedback on diagram tilt/missing safe zones, the current sheet
is approval_sheet_astra_v2: real projected pad bounds and a dashed proposed
12% icon inset, with glyph geometry preserved. This is review layout only.

## D-060 — Approved Astra resource-pad vector marks and safe bounds

Status: Accepted by explicit owner instruction, 2026-10-17; implemented on Mac.
The owner approved approval_sheet_astra_v2 and requested integration plus a
commit of all accumulated changes. Use the original Astra axe/log, pick/fractured
stone and pick/metal-bar vector geometry, preserving transparent negative space.
The bar is a metal pictogram, not a new resource tier or processing mechanic.

Apply the same actual world XY/camera projection to the full point.Clearance,
inner icon safe bounds and uniformly fitted glyph. Preserve the approved 12%
layout inset per edge, neutral thin outer contour and subtle dashed inner safe
border without diagram labels in the game. This inset is an artwork margin,
not additional collision or a change to construction eligibility. Preserve the
existing pad positions/sizes, landmark scale, production values, manual E,
marker lifecycle and red/green placement/affordability contracts.

Source of truth: `gamedesign/features/RESOURCE_PRODUCTION_BUILDINGS.md`.
Approved visual/source: `art/concepts/placement_icons_2026-10-17/approval_sheet_astra_v2.png`,
`art/source/placement_icons_astra_v2_2026-10-17.json`.

## D-061 — Tower construction cells and grid-aligned tower facing

Accepted 2026-10-17. The owner requested visible cell divisions on the gray
invasion zone when selecting a tower, and a 45-degree turn of all current tower
art because the existing corner-facing placement looks sideways.

Show the actual gray, walkable/buildable cell boundaries during tower placement,
including while the selected card is under the pointer. Hide them on cancel,
pause/end/reset and generator selection. Grass and reserved cells do not gain
construction marks. Occupancy, hero/enemy protection and full-price checks retain
their existing red/green cursor feedback and rules.

Rapid and Longshot now use edited sibling front-facing body sprites with the
depicted object turned around its vertical axis. Upright posts remain upright;
the fixed camera and game grid do not rotate. Preserve original tower files,
and Longshot's separate spoon/stone animation.
Preview and built towers share the same sprite set. No controls, tower types,
prices, collision or map changes are authorized by this visual correction.

Owner follow-up, same day: the first integration retained the previous size and
foot offset, leaving a large lower-cell gap and overlapping neighboring towers.
This supersedes retaining that height/registration: fit the complete visible body
width within one cell and anchor its visible bottom at the front cell edge with
a small inset. Scale uniformly, keep preview/built registration identical, fit
the contact shadow and preserve the separate stone's size through release.

## D-062 — Large-world survival TD and time rewind after throne loss

Status: accepted by explicit owner clarification, 2026-10-17. Runtime retry
implemented and locally qualified on macOS/Metal, 2026-10-19. Windows remains
unqualified; evidence: art/source/defeat_rewind_2026-10-19_verification.json.

The product combines tower defense with survival, exploration of a huge world
and base development. RimWorld and Factorio are references for world scale and
development; the owner gave approximate alternative cell counts, not an exact
map requirement. Current 250×250 bounds remain prototype data. World size,
population, additional survival systems and the minimum commercial slice need
their own concrete scope; no new architecture or content counts are chosen here.

The owner corrected the initial wording: the red portal remains the enemy
entrance; the blue throne remains the defended, loss-critical objective. The
hero has been sent to protect it, for a reason left unspecified. On destruction,
light rays emerge from the throne and build to a fully white screen, followed
by a readable «Повторить» action. No extra lore is established.

The owner explicitly selected a snapshot immediately before the failed wave
starts, with the full preparation timer on retry. Both manual early start and
automatic timer expiry establish the same kind of checkpoint before the first
spawn. Retry restores world/resources/buildings/hero from that checkpoint,
returns to Preparation for the failed wave, and grants its full configured
preparation duration. It does not use the remaining or previously skipped
preparation time, or reset to the beginning of the game. Attempts are unlimited.
An infinite sequence of different waves and cross-session saves are not decided
by this clarification. Detailed target: gamedesign/features/STAGE2_RUN_STATE.md.

This supersedes treating the huge-world/survival layer as merely a distant
optional direction in GAME_VISION, D-032's assumption that the unchanged short
finite TD run defines the complete commercial scope, and whole-run reset as
the target player-facing defeat retry. Historical first-playable acceptance and
the current reset implementation remain evidence, not proof of this new loop.
D-004's limits on Factorio-style logistics/automation and persistent roguelite
meta, D-006's non-combat hero/tools and D-007's tutorial scope remain in force.

Owner follow-up, 2026-10-19: each actual HP loss emits white radial dome rays
and screen fractures. The owner approved the generated storyboard, requiring
nonfatal fractures to disappear quickly. Total damage strengthens future
pulses. Show fractures only while the dome is visible; every hit, including
offscreen hits, gives a small short screen shake. Fatal whiteout and retry work
from any camera position. Logical camera/follow/picking remain stable.
Continuing-day retry grants the full normal morning preparation for the same
failed night; no skipped-time income. Capture all currently implemented world
owners, including dwelling, workshop/unlocks, individual resource stock and
production progress. Source concept/provenance:
art/source/defeat_rewind_2026-10-19_generation.json.

## D-063 — Research workshop, paired travel gates and future city

Status: direction and concept preparation authorized by the owner, 2026-10-17.
First images are proposals; no final visual or runtime implementation approved
by their generation alone.

The next peaceful building being designed is a research building. The owner
delegated naming; current name is «Исследовательская мастерская». Research
opens further buildings, towers and possibly other capabilities. Exact unlocks,
costs, durations, placement and UI rules are not yet selected. This moves the
research-building direction from a generic later candidate into active design;
it does not modify the implemented D-058 generators or invent new tower roles.

The owner also requested imagegen references for travel between two gates on
the same map: a base-side teleport accessible through research and a second
teleport found in the world. «Путевые врата» is the proposed travel name;
«Телепортация» as the first research and visiting the world-side gate before
first travel are recommendations for the initial slice, not settled runtime
rules. Whether base gates exist dormant or are constructed after research is
still open. Keep both roles distinct from the enemy portal and defended throne.

The future world-side destination can be a city with a merchant and other NPCs.
The owner's follow-up corrected speech recognition: **the city does not yet
exist and must not be created now**. Treat it as future context only; no city
scene, city concept generation, NPC set or trade economy is authorized now.

Original concept images and full generation records:
art/concepts/research_and_travel_2026-10-17/ and
art/source/research_and_travel_2026-10-17_generation.json.
Active briefs: gamedesign/features/RESEARCH.md and WORLD_TELEPORTS.md.
Future research/travel state must participate in D-062's world rewind. This
concept pass does not introduce persistent roguelite meta or a logistics system.

Owner appearance follow-up, 2026-10-17: «Исследовательская мастерская пойдет»
accepts the first research-workshop concept appearance. The paired upright
teleport arches are rejected. Teleports must be **circles drawn horizontally
on the ground**, with ground-plane markings rather than a vertical portal like
the enemy entrance. This supersedes the proposed gate shape and «Путевые врата»
wording; use «Круги телепортации». Workshop pixels are preserved. A new paired
ground-circle v2 reference is prepared for review; its generation does not
establish visual acceptance or runtime integration.

Further owner clarification, 2026-10-17: no appearance difference is wanted
between base and world teleports; use the same visual. The ground-circle idea
is broadly suitable, but the owner requests another reference composed of one
large circle, symmetric circles of different diameters on its circumference,
and circles at the center. Small ornate details are unsuitable at the game's
zoomed-out isometric view. This supersedes the fresh/weathered distinction
and rune-heavy v2 treatment. V3 is a simple universal circle-only proposal with
restrained glow. Exact projected geometry and in-game readability remain for
asset preparation/verification; the new image is not yet visually accepted.

Owner reference follow-up, 2026-10-17: the circle-only v3 is too bare. The owner
provided an ornamental circular magic-seal reference and requested a more
attractive design. This relaxes the circle-only restriction: round hierarchy
can include geometric links, arc accents and a few bold invented symbols.
Previous shared appearance, ground-plane orientation and zoomed-out readability
remain required. V4 is an original master-pattern proposal; final acceptance,
ground projection and gameplay-scale verification remain pending.
The supplied reference is archived in art/references/teleport_seal_user_reference_2026-10-17.png
as inspiration only, with external source/license unknown; do not trace it or
use its pixels as runtime artwork. Prompts/rights are in the generation record.

Owner approval and implementation clarification, 2026-10-17: «Да, теперь оно.
Approve. делай теперь это в игре» accepts v4 and authorizes the playable slice.
The base circle must **appear only after studying it in the Research Workshop**;
it is not a visible dormant endpoint before research. Both endpoints use v4.
This supersedes the earlier pending-approval and open base-activation wording.

First implemented slice: one ready workshop at the base, contextual E performs
the one-time «Телепортация» research and makes the base circle visible. Personally
discover the remote circle before paired travel. E inside either circle moves
the hero to its counterpart with a safe-arrival check and requires release before
another trip. Research/travel are currently instant and free for this prototype;
workshop construction, research prices/durations and additional technologies
remain open. They work in preparation/wave and are blocked by pause, terminal,
construction and debug UI. Full ResetRun/world regeneration clears progress.
Future D-062 snapshot integration remains pending with the rewind itself.
The city/NPC boundary remains unchanged. Runtime data and original image copies:
Assets/Stage3/WorldTravel/; contracts: RESEARCH.md and WORLD_TELEPORTS.md.

Owner construction follow-up, 2026-10-17: add the Research Workshop to the build
menu so the player constructs it before studying Teleportation. Owner selected
«Пока бесплатно» for its construction. This supersedes the ready base workshop
in the previous slice. The Research tab offers one workshop; it is placed on
free green ground with preview/LMB and Esc/RMB cancellation, without overlapping
hero, road, resource districts, solids or reserved teleport/landmark sites.
Research remains a separate E action at the constructed workshop. Full reset
or world regeneration removes the workshop/collision/marker and clears unlocks.
Other placement modes are exclusive. No additional technology or currency added.

## D-064 — Hero bow, passive wooden barricade and dense horde prototype

Status: implemented and verified as a provisional slice, 2026-10-18.

The owner approved a first slice with an automatic bow targeting the nearest
enemy in range, a non-attacking wooden road barricade, an easy first wave
that the hero can clear, and dense later hordes that require defenses.
Provisional bow statistics, barricade cost/health and horde count/cadence are
explicitly delegated to the implementer, with gameplay verification.
Portal enemies continue toward the throne and attack blocking defenses;
only future open-world enemies target the hero. Hero health/death belongs
to that later slice. Respawning at the throne is a recommendation, not an
accepted penalty or restore rule. The pending D-062 rewind remains separate.

The owner approved barricade A (blunt timber palisade), explicitly requiring
its front to face the camera as current towers do. It occupies an ordinary
gray road cell, redirects routes, has health and can be destroyed on full
blockage, and never attacks. Existing maze/siege/placement transactions remain.
All visual decisions/appearance and visual Unity work are delegated to GPT-6
Astra after owner-approved ImageGen sketches; the main agent performs ImageGen
and nonvisual gameplay integration. The existing hero marker remains temporary;
no final archer artwork is authorized without its own approved sketch.

Day/night, ruins/world enemies, discovered blueprints researched at the workshop
and tutorial quests are follow-up work. About twenty minutes of daylight and
nightly hordes were discussed, not selected as current runtime cadence.
There is no forced scripted second-wave defeat or tutorial lock in this slice.
This supersedes D-006's non-combat hero restriction; its tool-scope limits remain.
Contracts: gamedesign/features/HERO_COMBAT.md and WOODEN_BARRICADES.md.
Verification and remaining limits: production/WORKSTREAMS.md, 2026-10-18 handoff;
art/source/hero_combat_slice_2026-10-18_verification.json. Current enemy movement
still forms an overlapping column; a wide multi-row horde is not implemented.

## D-065 — Continuing days, night hordes and buildable dwelling

Status: implemented and verified as a prototype, 2026-10-18.

The owner requested about twenty minutes of daytime for exploration and
preparation, followed by a night horde. Clarified: night ends immediately after
the whole horde is defeated; only day can be skipped. After the first three
nights, days continue and hordes grow. The former finite three-wave victory
remains a legacy prototype contract, not the current Stage 3 survival loop.

The player builds a dwelling on free green ground for ten wood. E at the door
immediately ends the remaining day without awarding skipped-time production.
At night rest is unavailable. Ordinary active time and pause retain the shared
game clock; the normal remote early-wave button/Space no longer skips a day.
Development force-start remains available for QA. Prices and horde tuning live
in Unity data; future wave rewind is still the separate pending D-062 contract.

The owner selected ImageGen hero C, dwelling B and the cool readable night
concept in art/concepts/day_night_and_home_2026-10-18/. C is a versatile fantasy
hero, not an archer class. The bow is initial equipment, separate from body art;
future magic/other weapon development is intended but not implemented here.
All visual implementation and actual-frame QA belong to GPT-6 Astra after this
approval. The main agent owns ImageGen and nonvisual logic/integration.
Contract: gamedesign/features/DAY_NIGHT.md. Source/approval/provenance:
art/source/day_night_and_home_2026-10-18_generation.json.
Verification/limits: art/source/day_night_2026-10-18_verification.json and the
current production/WORKSTREAMS.md handoff. Manual survival balance remains open.

## D-066 — Fantasy player-interface concepts and separate windows

Date: 2026-10-18. Status: accepted direction and runtime implementation approved.

The owner requested ImageGen prototypes before Unity UI implementation and
assigned all visual decisions/implementation to GPT-6 Astra. Separate windows
serve inventory/equipment, construction, research, hero development and a
pannable/zoomable regional map. Visible buttons complement shortcuts; the
working proposal is I / B / P / O / M respectively.

The future HUD has compact resource icons/counts upper left, day plus ordinary
HH:MM upper right, a local minimap lower left and a compact hero console with
window buttons above it. The owner removed the night countdown and throne HP
from that HUD; throne health belongs above its world object. Local E prompts
remain, including the existing day-only home skip. These are target layouts,
not runtime changes in the prototype task.

The first charcoal-panel style was explicitly rejected. Complete redesign of
both maps and HUD was then authorized, replacing the earlier request to keep
the prototype minimap. Of three original fantasy ImageGen directions, the owner
selected C «Лунный фарфор»: pearl ceramic, muted plum, silver, turquoise and
curving crescent/petal silhouettes. This approves the visual theme; window
layout sheets and exact dimensions remain review proposals.

Inventory slot/capacity examples and the unnamed development-node diagram do
not define new mechanics. Current weapon stats may illustrate the compact
hero console; hero HP/mana/XP are not invented. Example 08:30 does not choose
the clock mapping or sunrise/sunset rules. Research remains local at the built
workshop; no remote research, prices/times/queue or extra technologies are
authorized. Keep existing approved world assets when implementing thumbnails.

Follow-up: the owner explicitly authorized implementation of the window
concepts, then required a fullwidth lower HUD extending from the minimap to the
right screen edge. The hero/bow/stats block becomes narrow with vertical stats;
future ability slots occupy the middle, and red HP / blue mana / orange XP
reserves occupy the right. The owner explicitly approved both new ImageGen
fullwidth normal and construction sketches: «Да, внедряй оба варианта».

Construction is a nonmodal ribbon replacing lower hero/status contents while
the five window buttons remain above. It has Defense/Production/Base categories,
actual building cards and selected-building details. Selecting a card begins
world placement while keeping the ribbon open. B opens the ribbon without
automatically selecting a tower. All window changes cancel stale placement.
HP/mana/XP and ability slots show unavailable reserves; no mechanics/values,
skills, levels or inventory capacity are invented. Existing IMGUI is retained.

Contract: gamedesign/features/PLAYER_INTERFACE.md. Source prompts/rights and
approvals: art/source/interface_runtime_2026-10-18_generation.json. Existing
world art is reused; generated interface thumbnail variations are not new
world-asset approval.

## D-067 — In-world clock and daylight during an overrunning horde

Date: 2026-10-18. Status: accepted clarification of D-065 during UI integration.

The player approved ordinary in-world hours: twenty active preparation minutes
normally progress from 08:00 to 20:00. Night advances at the same speed. They
explicitly rejected stopping at 07:59: an uncleared horde can continue through
08:00 into daylight. They confirmed no overlapping hordes and clarified that
the home cannot skip time while any horde is ongoing.

The earlier D-065 early-clearance contract remains: winning before morning
advances to dawn. If clearance happens after dawn, retain the forward-moving
clock and begin the full twenty-minute preparation period; do not rewind hours.
The following horde therefore may start later than 20:00 in this exceptional
case. Local early day skip advances to night without simulating production.
Pause freezes the clock. Reset returns to calendar day 1 at 08:00.

Run preparation/active-horde phases continue to own spawning/rest availability;
the clock owns displayed hours/calendar day and daylight. Daylight does not
start a second preparation or horde behind an unfinished one. No horde tuning,
hard time limit, forced kill, new survival statistic or night skip is introduced.
Contract: gamedesign/features/DAY_NIGHT.md and PLAYER_INTERFACE.md.

## D-068 — Correct interface proportions, safe areas and pointer-centered map zoom

Date: 2026-10-18. Status: accepted user correction of D-066 implementation.

The owner rejected the first runtime result: serif typography, stretched raster
ornaments, displaced icons, insufficient content/screen padding and missing
hover feedback. Keep the approved lunar-porcelain direction and existing UI
stack, but use a legible Cyrillic sans-serif font, preserve decoration/icon
proportions, provide safe content bounds and visible interactive states.
This directly authorizes corrections to the approved layout; no new art
direction or gameplay system is introduced. Astra owns their visual execution.

The hero must be unmistakable on the map. Mouse-wheel map zoom retains the
world position under the pointer; a two-touch pinch uses its moving midpoint.
Existing world bounds still constrain panning. Plus/minus buttons retain their
center-based behavior. These controls do not move the world camera or hero.
Contract and verification: gamedesign/features/PLAYER_INTERFACE.md.

## D-069 — Full-screen pages and icon-only HUD navigation

Date: 2026-10-18. Status: revised ImageGen concepts approved for implementation
by the owner's explicit «делай». GPT-6 Astra owns visual Unity implementation.

Inventory I, Research P, Hero development O and Map M occupy the complete
screen with an opaque lunar-porcelain surface, shared four-tab navigation,
resource/day-clock strip and close controls. The ordinary world HUD/minimap
is not drawn beneath these pages. The Inventory V2 concept is the common shell.
Keep existing modal input blocking and continuing simulation; this presentation
change does not introduce automatic pause or additional gameplay systems.

The owner explicitly rejected a separate full-screen construction catalogue.
B retains the current nonmodal construction ribbon and world placement flow;
there is no Construction tab in the full-screen header. Purchases and local E
interactions keep their existing rules.

The world's five HUD menu buttons contain only their recognizable icon and
hotkey: bag I, hammer B, book P, hero silhouette O and map M. Remove word labels,
retain visible interaction states and readable keys. Research remains the
existing Teleportation overview with a passive local-E instruction. Hero
development and other unimplemented systems remain visibly unavailable.

This supersedes the centered modal-window presentation of D-066/D-068 and
their labelled HUD access buttons. Approved previews/provenance:
art/concepts/fullscreen_interface_2026-10-18/ and
art/source/fullscreen_interface_2026-10-18_generation.json.
Contract: gamedesign/features/PLAYER_INTERFACE.md. Verification is recorded
in the current production/WORKSTREAMS.md handoff, separately from old results.

## D-070 — World image sampling, production-pad outline and consistent map gestures

Date: 2026-10-18. Status: owner explicitly authorized the first repair queue.

Improve zoom sampling within the accepted cartoon/isometric presentation,
remove only the inner dashed production-pad border, and preserve the outer
clearance line, icon margins, fixed camera and gameplay footprints. Preserve
original PNG pixels. Astra owns render/import quality and actual-frame review.

Map M must use the same Mac pinch/spread magnification as the world. Moving
two fingers together pans the regional map; a physical mouse wheel still zooms.
The app-local native gesture queue has one polling owner and routes to the
visible surface without changing the world camera beneath M. Header/HUD areas
and lifecycle/focus changes must not leak or replay gestures. Existing anchored
map limits, touchscreen input, mouse drag and modal blockers remain.

This supersedes D-060's visible inner dashed pad safe border and D-068/D-069's
incomplete Mac-trackpad behavior. Synthetic touchscreen coverage alone is not
physical trackpad verification. Current results belong in WORKSTREAMS and
art/source/quality_gestures_2026-10-18_verification.json.

## D-071 — Approved manual resources, natural tree groups and tool/motion slice

Date: 2026-10-18. Status: owner approved all six selected ImageGen sheets and
authorized runtime implementation after D-070, with the tree correction below.

The owner wants more individually harvestable resources across the test world.
Stone and iron ore have small/medium/large forms, with provisional reserves
5/35/120. Each completed tool strike gives one resource immediately and consumes
one reserve unit; leaving preserves the remaining reserve. Visual depletion
must track remaining stock through coherent stages. Existing small artwork
anchors the new forms. Trees keep ordinary size and occur individually or in
close groups of one to eight, distinct from the large auto-production forest.
Owner clarification: choose the group count and each tree position randomly,
with irregular natural spacing; no rows, grids, equal intervals or authored
orchard arrangement. Trees remain separate ordinary-sized harvestable objects.
The owner explicitly waived another tree sketch and authorized in-game testing.

Show an axe for wood and a pickaxe for stone/metal, equipped by the accepted
hero C, with windup/contact/recovery harvesting poses. Hero and enemy walking
needs more natural continuous motion within the current art direction. First
review concept/pose sheets; static images do not prove a final smooth animation.
Astra owns visual direction and implementation after owner appearance approval.

In the future larger world, manual gathering precedes auto-production; its
research is acquired from a trader NPC in a city. City/trader economy, research
prices and unlock flow are future work. Current auto-production stays available
in the test environment. City/trader/research gates remain future work. D-070 remains the completed
repair queue; the approved D-071 runtime slice now covers nodes, tools and motion.

## D-072 — Manual resource collision and harvest target markers

Date: 2026-10-18. Status: accepted by the owner; implemented and verified locally
2026-10-19. Windows player verification remains open.

Ordinary wood, stone and metal sources block the hero at their physical footing:
tree trunks and the current mineral stock stage. Keep passage between separate
trees. Depleted sources open the site; recovery waits for an overlapping hero
to leave before restoring collision. Reuse the existing swept movement solver.

Show an axe above wood or a pickaxe above stone/metal when that exact source can
be harvested with E. The visual and input consume the same target selection;
foreground fade does not disable either. Hide the marker during UI/build/pause
and terminal blocking. Reuse approved native tool vectors and the lunar HUD
palette; no new art concept or tool/progression controls are introduced.

## D-073 — Creep spawn stalls, mineral reach and hero progression direction

Date: 2026-10-19. Status: owner requested the two bug repairs and delegated a
hero progression concept. Repairs are implemented and locally qualified on
Mac/Metal; the progression roster remains proposed. Evidence:
art/source/movement_reach_2026-10-19_verification.json.

Remove unintended creep stops without bypassing road routing, siege or pause.
Medium and large stone/ore must be harvestable at a comfortable distance from
their actual current footprint; collision, stock, reward and exact E-marker
selection stay coherent. Small nodes/trees and legacy scenes keep their reach.

The owner activates design of hero abilities, passives and separate gathering /
exploration advancement. Confirmed: level-one Blink is a short cursor-targeted
spell, with four example distance ranks 300/500/800/1000 and further improvement.
Existing travel circles remain separate. Five or six active slots including
Blink are permitted; the concrete six-spell roster, starting-point rules,
profession rewards, experience curve and scale conversion are proposed in
gamedesign/features/HERO_PROGRESSION.md. Those choices are not shipped mechanics or an
approved appearance. Future tool advancement is now an active design topic;
this does not introduce cities, crafting tiers, hero health or permanent meta.

The proposed progression must participate in the existing D-062 pre-wave
checkpoint; failed-attempt XP or purchases cannot survive a retry. Visual
implementation still follows owner-approved ImageGen and GPT-6 Astra ownership.

## D-074 — Level-100 progression, exploration combat and daytime gates

Equipment-slot/quality proposals below are historical: D-075 replaces eight
combat slots with six and removes SS, leaving eight quality steps. D-075 also
activates forge/base/tower advancement. Read the later decision for current scope.

Date: 2026-10-19. Status: owner direction accepted; detailed budgets and new
runtime contracts below are design proposals. No XP/gates/items/rune runtime
is delivered by this decision. Supersedes the D-073 proposed AoE spell roster
and the target behavior of both D-065 clock skips; existing implementation
remains until a verified replacement.

Accepted owner requirements:

- Hero level cap is 100 for the current scope, expandable later. Hero magic
  deals modest mainly single-target damage with minimal splash for exploration
  encounters; towers handle dense comparable night hordes. Existing level-one
  cursor Blink direction/range ranks 300/500/800/1000 stay accepted.
- Night hordes continue and become stronger by threat level, later potentially
  more numerous. After clearing a tier, daytime gates may be opened/closed
  repeatedly to emit that same tier continuously for XP. The stream closes
  automatically before the next required night; exact lead time is open.
- Owner explicitly confirms removing **both** clock skips: dwelling day rest
  and the early jump to dawn on a night victory. Do not silently replace the
  dwelling's removed action with healing, storage or respawn.
- Future cleared ruins automatically grant random loot such as equipment,
  blueprints, temporary buff scrolls and permanent runes. Equipment has levels
  1–100 and qualities F through SSS. Hero and tower runes are separate;
  gathering runes are supported as a possible additional category.

Recommended scope, not accepted counts: 12 four-rank actives with six equipped
including Blink; 48 three-rank passive nodes; three professions with 12
three-rank nodes each. Free Blink I plus 99 hero points makes choices necessary.
Proposed rune library: 40 types (16 hero / 16 tower / 8 gathering/exploration),
nine qualities F/E/D/C/B/A/S/SS/SSS, no rune levels, best learned quality per
type instead of unlimited permanent stacking. Eight combat equipment slots
and two separate working tools are proposed, not approved UI changes.

Proposed gate safeguards: closing stops only new spawns; living enemies remain,
cadence persists through toggles, concurrent farming population is bounded.
19:30 cutoff before 20:00 night is a candidate, not tuned fact. XP/drops from
old tiers do not become endgame rewards solely from hero level. Night duration
without skips requires pacing evidence. The next mandatory horde does not
silently accumulate behind an unfinished required horde.

D-062 night retry remains accepted. Daytime loss timing and snapshots with
already living farm enemies require an explicit extension before runtime work.
XP, items, rune learning, rewards/random state and professions must rewind
together; permanent runes are current-world progression, not cross-world meta.
Future hero death in ruins needs a separate rule. New appearance still follows
ImageGen approval and GPT-6 Astra ownership.

Canonical contracts: gamedesign/features/HERO_PROGRESSION.md,
PORTAL_CONTROL.md and EQUIPMENT_AND_RUNES.md. The only implementation sequence
is the D-074 block in gamedesign/DEVELOPMENT_PLAN.md. The 100-level planning
ceiling does not automatically make the entire catalogue a next-build/MVP gate.

## D-075 — Three-boss victory and hero/base advancement

Date: 2026-10-19. Status: accepted owner direction with proposed implementation
details. Design only; no boss, forge, selection or upgrade runtime is delivered.
Supersedes earlier candidate equipment slots/SS quality and deferral of these
named progression topics. Existing technical scenes remain the baseline.

Accepted requirements and clarifications:

- Three distinct large, strong bosses are the campaign goal; defeating all
  three ends the game. The owner has not chosen world encounters versus
  separate portal summons. Names, order, levels, mechanics and replay rule
  are not assigned by this decision.
- TD is the main combat XP source through many enemies; open-world camps/
  ruins give less XP and higher valuable-drop chances. Equipment/weapon drops
  exist in the target for both portal and world enemies. Higher quality is
  rarer. Bandits/orcs/undead are examples, not three required new factions.
- Six combat slots: helmet, chest, arms, legs, weapon, shield. Bow is physical,
  staff is magical. Owner explicitly confirms item level 1–100 remains separate
  from quality. Sequence: F/E/D/C/B/A/S/SSS; **no SS**, maximum **SSS** now.
  SSSS is not current content. Shield compatibility/handedness remains open.
- Rune direction D-074 is retained with eight qualities, not nine. Owner art
  brief: stone fantasy shield-like runes; F grey, intermediate palette includes
  red without an assigned rank, S gold, SSS exceptionally rich gold. Other
  palette/SSS details require a sketch, not inferred approval.
- Forge begins at F and upgrades its quality limit. It creates gear/weapons
  and reforges an existing item step by step up to that limit. Crafting/better
  pickaxes and axes is explicitly included, distinct from profession mastery.
  “Heavenly forge” is a possible later presentation, not an approved appearance.
- Sawmill/quarry/iron-mine production should be upgradeable. Existing workshop
  research plus paid local installation is recommended; an extra upgrade-only
  building has not been chosen. Workshop itself also advances; hut purpose/
  upgrades remain undecided.
- New tower types require buying research from a city merchant, then studying
  it in the player's workshop. Purchase, study and paid construction are
  distinct; cities/merchants and additional tower count remain future work.
- Towers support **both paid upgrading and drag merging** of two equal-level
  towers. Clicking selects a built object; towers expose stats, upgrade, move
  and sale at **50%**. Same-type merging and refund accounting are proposals.
- All own buildings need a compact nonmodal card above HUD, with actual relevant
  stats, supported upgrades and a link to their full-screen functional section.
  Workshop links to Research; forging page is a proposed new context screen.
  This is an explicit addition to D-069's full-screen page/world-building split.
- Initial construction costs wood/stone/metal, **not gold**. The owner's later
  clarification permits **gold plus resources for upgrades and research**;
  trade is another sink. Gold from creeps remains income. Do not misread the
  earlier resource-only statement as prohibiting gold in all improvements.
- Throne has exactly **three max-HP upgrades, +5 each**, +15 over the initial
  maximum. Current-health transfer and prices are not specified by the owner.

Proposed budgets: five levels per tower type, six forge stages (quality caps
F/D/B/A/S/SSS), five production stages and three workshop stages. These are
recommendations, unlike the confirmed three throne upgrades. The D-074
12/48/36 ability/talent budget stays a concept; rune budget stays 40 types
(16 hero / 16 tower / 8 gathering/exploration), now 320 quality combinations.
Specific percentages, recipes, costs, unlocks and runtime catalogs are not fixed.

Proposed safeguards: real invested-cost ledger for half refunds after paid
upgrades/merges, state-preserving move and HP/cooldown transfer, atomic payments,
no reward duplication/quality reroll through D-062. Forge/tool/production/
workshop stages and throne count/current/max HP join that same world snapshot.
Boss attempt/simultaneous outcome rules require a separate explicit contract.
Source of truth: BOSSES_AND_VICTORY.md, EQUIPMENT_AND_RUNES.md,
TOWER_UPGRADES.md, RESEARCH.md, RESOURCE_PRODUCTION_BUILDINGS.md,
OBJECTIVES.md, PLAYER_INTERFACE.md and STAGE2_ECONOMY.md.

Astra reviewed a textual card brief and proposed layout/SSS treatment only;
no ImageGen sheet, new asset or actual-frame qualification was produced.
Owner approval of future sketches precedes Astra's visual implementation.
DEVELOPMENT_PLAN.md contains the only current ordered implementation sequence.

## D-076 — Building selection first, ImageGen approval before Astra implementation

Appearance candidates below were sent back for D-077 revision, not approved.
The selection-first priority and ImageGen-before-implementation workflow remain.

Date: 2026-10-19. Status: owner prioritization/workflow accepted; appearance
choice pending. Start with selection and the above-HUD building card before
the prior progression-first sequence. Produce ImageGen sketches, present them
for approval, then implement through GPT-6 Astra. No approval is inferred from
generating or internally reviewing a picture.

Candidates: horizontal A, right-card B and shared building-content/state sheet
in art/concepts/building_selection_2026-10-19/. Full built-in-tool prompts,
references and hashes are in art/source/building_selection_2026-10-19_generation.json.
PLAYER_INTERFACE.md owns the approval state and input/content contract;
DEVELOPMENT_PLAN.md owns the reordered current work. These images do not
implement upgrades/merging or authorize unchosen prices/extra buildings.

## D-077 — One continuous HUD and drag-only building movement

Date: 2026-10-19. Status: owner layout/input direction accepted; new ImageGen
appearance still pending approval. Replaces the compact/floating D-076 card
geometry and the separate Move button proposed under D-075.

The bottom HUD must be one continuous large surface: minimap, hero/weapon,
navigation icons, ability reserves and status inside a shared backing/frame,
without world gaps or floating navigation. Selecting a building adds a long
strip of the same width immediately above, with purposeful use of its width.
Keep the current Lunar Porcelain style and original game identity.

Remove the Move button. Dragging the world building is the movement input;
drop on a compatible equal-level tower is merging. Valid empty-cell drop,
incompatible/cancel handling and phase/health/cost rules remain governed by
TOWER_UPGRADES.md; a picture does not implement any of those operations.

Two v3 states (normal and selected) are in
art/concepts/building_selection_2026-10-19/hud_unified_{normal,selected}_v3.png.
Full prompts, reference rights/hashes and Astra review are in
art/source/unified_hud_2026-10-19_generation.json. Astra found both suitable
to show; selected stack is taller than the brief's guide, so owner should
judge world coverage. Owner has not approved v3; no implementation yet.
No new prices, slots, abilities or gameplay values are assigned here.

## D-078 — Separate navigation ribbon and numeric, vivid hero bars

Date: 2026-10-19. Status: owner revision requirements and v4 appearance
approved explicitly («Утверждаю v4»). Supersedes navigation placement/five-slot geometry of
D-077 v3, while retaining the continuous HUD and world-drag input direction.

From top to bottom: full-width selected-building strip, narrow full-width
I/B/P/O/M ribbon, main minimap/hero/spells/status dock. The main center is for
spells and must accommodate possible expansion to ten; ten simultaneously
equipped spells and new hotkeys are not confirmed by this capacity request.

Owner follow-up during implementation: visible labels are **HP / MP / XP**,
not the full words. Match approved v4 proportions, ornamented adjoining frames,
medallion sizes and beveled bars; the simplified first runtime is not final.

Health/mana/experience have pleasant vivid colored fills and current/maximum
numbers. The concept uses red HP, cyan-blue mana, orange XP. Sample values
80/100, 45/60, 350/1000 are explicitly illustrative, not runtime tuning.
These hero systems are not implemented; implementation must use real data.

Two built-in ImageGen v4 states and full prompts/references/hashes/Astra review:
art/source/hud_ribbon_2026-10-19_generation.json. Astra found both suitable
for owner review; owner then approved v4 on 2026-10-19. Astra implemented the
native appearance and main chat integrated selection/input/live data. The
owner returned the first simplified runtime for closer v4 conformity, now
corrected. HP/MP/XP display disabled 0/0 + «Не открыто» until actual sources
exist. Upgrade/sale buttons are disabled; move/merge remains a separate slice.
25 PlayMode tests, native pointer buttons, Mac build and three-resolution
player frames are recorded in art/source/hud_v4_2026-10-19_verification.json.
No scene/data/art migration; Windows qualification is unavailable locally.

## D-079 — Activate tower and barricade upgrade, merge and sale

Date: 2026-10-19. Status: owner-requested runtime implemented and locally
qualified; scope clarified as towers and barricades for this upgrade slice.
D-078 approved appearance stays. Evidence: art/source/building_actions_2026-10-19_verification.json.

The selected card buys a level for resources; dragging two equal-level towers
merges them, and sale returns half the cost. Implementation uses matching type
as well as level, retaining the recipient cell. Actual construction/upgrade
investments of both donors are combined; refunds floor each currency separately,
without conversion. A removed object cannot pay again. Paid upgrades and merging
preserve health fraction and attack readiness; merging keeps the slower readiness
of the two. Already launched projectiles keep their original attack values.

The current safe movement rule is free empty-cell drag during preparation only;
merge/paid upgrade/sale also work during an active wave. Invalid/cancelled drops
leave both world and inventory unchanged. Pause, terminal state, modal pages and
reset/rewind cancel drag. These implementation choices are provisional gameplay
rules, not a broader approval of moving siege blockers during combat.

Runtime tier rows/prices live in the three existing active Stage3 defense catalog
assets. They currently provide five editable prototype levels, not a permanent
content cap or a newly approved balance curve. Stage3 Rapid construction is
explicitly migrated from legacy five-gold surcharge to material-only pricing;
Stage2 historical catalogs/scenes and their tests retain their original prices.
No new research gate or per-level art. Other building upgrades remain planned.
Level, HP, cooldown, cell and actual investments participate in D-062 rewind.

## D-080 — One-click build cards and world selection with construction open

Date: 2026-10-19. Status: owner correction implemented; native player flow passed.
Final three-resolution Astra visual review passed; evidence and handoff:
art/source/construction_selection_fix_2026-10-19_verification.json.
The separate «Выбрать» button in defense/production/base is redundant: clicking
a build card already selects its blueprint. Remove the extra button.

Construction is nonmodal. A finished owned building can be selected and dragged
while its dock stays open; the existing selected strip is displayed above it.
Clicking a finished building takes priority over an armed blueprint, cancelling
all draft kinds before any placement driver can pay/create behind its silhouette.
Clicking empty ground with an armed blueprint continues normal placement. Modal
pages/pause/loss still block world actions; UI presses cannot become world drags.

D-079 previous qualification omitted the ordinary build-with-dock-open path;
its rule/closed-dock action checks do not prove this repaired input path. Add
actual pointer tests and native catalogue→select→move→merge→upgrade→sell coverage
with B open. Native testing exposed down/move/up coalesced in a single Update;
retain the press origin from Input System events so fast gestures still work.
FastMouse merging is disabled while this scene binding owns input and its previous
runtime setting is restored on disable/unbind. Serialized InputSettings do not change.
No changes to tower prices, levels or combat/rewind rules.

## D-081 — Blue merge preview and silver tower levels

Date: 2026-10-19. Status: direct owner appearance instruction implemented.
Valid tower-on-tower merge feedback is light blue, distinct from green ordinary
construction. Invalid drops retain red feedback. Existing matching-type/equal-level
eligibility and all upgrade transactions are unchanged.

Built towers display a small gray/silver metal tag above their visible silhouette,
labelled Level 1, Level 2, etc., using the actual current level. It stays readable
through camera zoom and updates after either paid upgrade or merge. No new tier
classification, ranks, tower artwork or progression rules are introduced.
This is the owner's concrete correction to existing v4 presentation; Astra owns
its native visual implementation and actual-frame review.
Local qualification: 4/4 focused PlayMode, Mac/Metal build and 45 actual player
frames at 720p/900p/1080p with zoom 2.5/5/10. Windows remains unverified.
Evidence: art/source/tower_merge_levels_2026-10-19_verification.json.

## D-082 — Compact silver LVL tags and generated upgrade icon

Date: 2026-10-19. Status: implemented and locally qualified, performance limits below. Supersedes D-081 tag text,
size and flat-gray finish; keeps its blue merge semantics and actual-level source.
Use LVL N with smaller type/padding and a bright reflective silver tag. Upgrade
uses the original generated silver/cyan chevron explicitly approved by the owner
(«Да, внедряй»), replacing the code-drawn arrow inside the existing action frame.
ImageGen provenance: art/source/upgrade_icon_2026-10-19_generation.json.

The owner also reports periodic stalls during hero traversal. Profile sustained
movement in the actual player, including world/GUI/allocations/render wait, before
selecting a fix. Existing rendering, collision and gameplay contracts must hold;
no chunk-loading or other architecture is inferred from the symptom alone.
The owner confirms ordinary walking, not F3 acceleration. Measured fixes cache
unchanged HUD content/bevels and resource sprite state. PC quality now uses
vSyncCount=1 to pace presentation at the display refresh rate instead of an
unbounded render loop; Mobile quality is unchanged. This does not establish
that every platform-specific presentation stall is eliminated. Evidence and
remaining qualification: art/source/tower_lvl_performance_2026-10-19_verification.json.

## D-083 — Immediate prototype startup without visible graybox

Date: 2026-10-19. Status: implemented and locally qualified.
The owner defers main menu/loading interaction to preserve quick iteration.
The player still enters the game directly, without an extra Play action.
Fix the existing startup order so final approved world and HUD are prepared
before the first game-camera render. Legacy primitive/marker/debug presentation
must not flash; runtime procedural generation remains supported. No new visual
design, global loading manager or blanket shader-warmup requirement is approved.
The current Stage3 playable scene is also first in ordinary Editor Build Settings;
legacy Stage2/Stage1 entries and explicit-path tests remain available.
Five focused PlayMode cases pass; three fresh Mac/Metal processes pass all24
initial camera-render observations beginning at Unity frame1. Astra first/eighth
frame review passes at720p/900p/1080p. Windows remains unverified without module.
Evidence: art/source/direct_start_2026-10-19_verification.json.

## D-084 — Deferred scorched landscape and night-lit invasion runes

Date: 2026-10-19. Status: owner direction accepted for a distant visual pass,
after the necessary gameplay functionality is in place; not current runtime work.
Rework the landscape and enemy invasion road. The road should read as scorched,
undead-corrupted earth, with etched rune markings along its entire length.
At night these runes glow red. This concerns the invasion route's ground
markings, separately from collectible equipment/progression runes.

Exact landscape treatment, rune artwork and glow intensity are deferred to
ImageGen sketches, owner appearance approval and GPT-6 Astra implementation.
Current route geometry, navigation, building rules and runtime art remain as
they are until that work is activated; no new layout is specified here.
Canonical ordering: gamedesign/DEVELOPMENT_PLAN.md; visual brief:
gamedesign/ART_DIRECTION.md. No new menu/loading work is introduced.

## D-085 — First hero-progression slice, concepts before runtime

Date: 2026-10-19. Status: scope and three concepts approved by owner; first playable slice implemented.
Exception: the old level-up panel is superseded by D-086.
Start hero XP from hero/owned-tower kills, levels up to100, cursor Blink and
first passives. The owner explicitly requests interface/effect sketches before
implementation. Preserve approved v4 HUD/fullscreen identity and heroC.
No additional attack spell, health/death, profession, equipment or main-menu
work is activated in this slice. All broader progression budgets remain proposals.

The three approved initial passives are personal single-target damage, attack
speed and movement speed, initially with three ranks each. Free BlinkI, later
point rewards/costs and global tower credit are implemented as first playable
tuning. Runtime values live in the progression Unity asset; artwork examples
do not lock balance.
Blink keeps the previously confirmed300/500/800/1000 ranges and cursor direction;
ten drawn HUD cells do not fix the final equipped-skill count or new hotkeys.

Full-screen O page, v4 HUD integration and Blink/XP/level-up FX are generated
through built-in ImageGen and reviewed by Astra before presentation. Explicit
owner appearance approval then precedes Astra visual implementation/main gameplay
integration, including D-062 checkpoint of XP/points/ranks/cooldowns. References,
full prompts and output hashes: art/source/hero_progression_2026-10-19_generation.json.

## D-086 — Divine level-up imprint: polygonal seal and energy waves

Date: 2026-10-19. Status: final polygon_v3 explicitly approved and implemented; accepted choir/echo cue, hammer deferred by owner.
D-085 development/HUD/Blink/XP/passive concepts are approved. The old restrained
level-up sparks are superseded. The owner wants an epic divine stamp with the
new level and a magical energy wave, plus a metallic/gold sound accent and an
ethereal female vocal layered threefold with echo, almost a choir.

The heavy circular gold medallion concepts were rejected. After viewing the
owner's Enshrouded reference, the direction is a lighter translucent heraldic
imprint: an ORIGINAL unusual, left-right symmetrical polygonal shield/token.
Broaden its lateral ornament, diversify the central silhouette with sharp cuts,
facets and angled projections. Keep the central contour crisp; outer effects
must blur into broad feathered energy waves, not hard circuit-like lines.
Text is exactly three elements: new-level heading, new level number, awarded
points. Delete the extra O/«Развитие» prompt and its key icon. This is transient
nonmodal feedback; it is not a separate window or a damaging ground spell.

Reference is inspiration only, with no third-party runtime bytes or traced
shape. Owner approved polygon_v3: «Мне нравится эффект повышения уровня. Внедряй».
Main produces original transparent runtime layers; Astra implements this
approved appearance. Current approved progression work proceeds independently. The accepted audio now uses the CC0 three-part female
chord (bone666138, Freesound274121) and stereo echoes only. After rejecting the
light/muffled/gong/anvil-tail attempts, the owner explicitly said to leave the
reward sound without a hammer. Skyrim quest-completion and Elden Ring level-up/
legendary-item cues were subsequent listening references. The owner selected
the third link, Elden Ring Player Level Up (k1avFgP63Ng), as the precise direction.
Original candidate v5 was rejected as entirely unlike the reference and was
never imported. The agent had not listened/compared: technical waveform checks
are not perceptual qualification. The owner finally reconfirmed: keep the choir without an impact and commit/push
all current changes. Impact work is deferred; runtime keeps accepted voice/echo. No
proprietary reference audio is downloaded, sampled or shipped.
ImageGen produces no audio; provenance/mix source is in
art/source/hero_levelup_audio_2026-10-19/. Subjective sound approval is not implied
by visual approval.
Prompts, generated references and approval state are recorded in
art/source/hero_progression_2026-10-19_generation.json.

## D-087 — Retry rewinds time while preserving current world progress

Date: 2026-10-19. Status: explicit owner correction; supersedes D-062 full-world
restoration for the active continuing-day Stage3 game and D-074 snapshot proposal.
The owner rejects losing the day's development and the resulting progression
trap. Keep current inventory, earned XP/points/ranks, research/discovery,
resource stock/recovery, production and current buildings/upgrades/sales.
Return morning with preparation for the same pending night tier, restore the
throne and clear failed-attempt enemies/shots/transient actions. No simulated
income is added by changing the clock. Ordinary subsequent ticks produce normally.
Current damage/destruction/spending are current world state, not an old snapshot.
New enemies in a new attempt award normal XP/gold when killed; no persistent
cross-attempt kill suppression. Legacy finite Stage2 fixtures retain old retry.
The earlier request to snapshot living creeps is superseded: they are not restored
by this time-only recovery. D-074 gates and removal of both skips remain active.

## D-088 — Dwelling sleep until ten game minutes before the night horde

Date: 2026-10-19. Status: owner explicitly restores an intentional day skip.
This supersedes D-074's complete removal of dwelling time advancement: the
owner clarifies one Sleep action, not repeated ten-minute rests. Sleep advances
to ten in-game minutes before the next20:00 horde (19:50), leaving preparation.
Block it during any battle: open daytime stream, live creeps/pending attacks or
mandatory horde. No early-dawn skip after victory. The existing local dwelling
interaction and selected-building card carry Sleep; no separate window/layout.
Clock change grants no simulated skipped-time production/recovery. A later
sleep cannot move time backwards. D-087 time-only defeat retry still applies.

## D-089 — One base item per type, quality-specific appearance; forge concepts first

Date: 2026-10-19. Status: owner scope brief and all four final concepts approved;
no runtime implementation delivered by the concept step.
The owner activates ImageGen concepts for the item inventory, forge building and
forge interface. Initially each item type has one basic family, not named,
elemental or special-affix alternatives. Numerical level1–100 is separate from
quality F/E/D/C/B/A/S/SSS. Increasing quality changes the actual item/icon design:
rough primitive brown/wooden/simple materials become iron/steel and increasingly
refined red/gold ornament. Do not merely recolor the same rarity border or draw
100 different icons for numerical levels. Apply the same rule across equipment
and tools. More item families wait for later content/balance work.
The sword is the owner's example of this principle; it does not settle a new
melee combat system. Existing agreed bow/staff, four armor parts, shield and
separate axe/pickaxe remain the concept coverage. Preserve existing
fullscreen Lunar Porcelain language and fixed world-camera readability.
Main produces ImageGen; Astra owns visual review and subsequent approved Unity
appearance. Prices, percentages, item stats and forge-stage budgets are not
approved by illustrative concept labels. D-087 keeps current items/forge progress
on defeat; no old full-world rollback is reintroduced.

Owner acceptance,2026-10-19: «Утверждаю все четыре». This approves the selected
inventory_items_v2, forge_building_v1, forge_interface_v2 and
item_quality_progression_v1, including their layout/appearance and shown quality
palette/lineage. Stat numbers remain illustrative; prices/stage caps are not
assigned. Version hashes/prompts and acceptance are in
art/source/equipment_forge_2026-10-19_generation.json. Do not request approval
again for these unchanged designs.

## D-090 — Three scroll-gated forge stages; bow and wand only

Date:2026-10-19. Status: direct human clarification during approved D-089 implementation.
Supersedes six/eight proposed forge stages, forge-made SSS, and the shield/staff
coverage in D-075/D-089. Five combat slots: head/chest/arms/legs/weapon, plus
separate pickaxe/axe. Weapons are bow and short magic wand; no sword or shield.
Keep eight item qualities F/E/D/C/B/A/S/SSS and independent numerical1–100.
Forge building has exactly three stages:1 F–D,2 F–B,3 adds A–S while retaining
lower qualities. Upgrading the building requires special forge scrolls ONLY;
there is no resource/gold substitute. These scrolls have no current loot source.
SSS is exclusive to future ultra-rare drops and cannot be crafted/reforged.
The three operation tabs remain Create/Level/Reforge. New items start F1;
reforging advances one quality while preserving item ID/numerical level. Trial
costs/stats live in Stage3Equipment data, not illustrative concept labels.
Building is local, paid in wood/stone/metal; world selection opens its service
and explains missing upgrade scroll. D-087 keeps current items, equipped state,
forge/stage and paid costs on defeat; only full Reset starts over.

## D-091 — Visible equipped bow/tools and bag separate from loadout

Date:2026-10-20. Status: direct human corrections to D-089/D-090 presentation.
The owner reports crooked bow/world equipment missing. Correct actual held bow,
pickaxe and axe only; preserve hero armor/clothing appearance. Quality-specific
bow uses proper facing projections/grip, and actual equipped axe/pick replaces
the baked starter tool during harvesting, preserving existing poses/strike facts.
No armor redressing, new weapon/content family, gameplay stats/costs or spells.
The owner additionally requires equipped items to appear only in their loadout
slots, never duplicated in the left bag grid. Equip removes the item from the
bag; unequip returns it. Replacing a weapon returns the previous one to bag.
This is one owned instance moving between views, not deletion/copy/payment or
new inventory capacity. Selection/details can inspect either location.

## D-092 — Gathering mastery and approved profession page

Date:2026-10-20. Status: accepted by owner: «Утверждаю, внедряй через Astra».
Approved concepts: gatherer_miner_v3 and gatherer_logger_v2. Existing fullscreen
O gains inner combat/profession tabs. Рудокоп (stone+metal/pickaxe) and Лесоруб
(wood/axe) have independent mastery1–100, actual-material XP, points and first
strength/tempo/reach talents, three ranks each. These are separate from hero
combat XP and equipment quality/level. Only actual manual material grants earn
profession XP; automatic production, cancelled/empty work and refunds do not.
Talents combine with actual equipped tool stats, consume the same node stock
and cannot replace a missing tool. Reach scales permitted edge gap, never
collision/model size. Rank/tool changes cancel unfinished work until release.
The page shows independent numerical XP/points, three talent cards, one action,
current→next results and the unchanged equipped tool. Main owns gameplay and
ImageGen; GPT-6 Astra owns native UI. D-087 keeps CURRENT mastery/points/ranks
after defeat; full Reset clears them. XP curve/effect sizes are trial data in
Stage3Professions, not illustrative concept values. Larger trees, explorer,
base upgrades and scroll sources remain later slices. Scene/GUID/packages kept.

## D-093 — Five paid production stages, prototype without research gates

Date:2026-10-20. Status: accepted by owner: «Да,5ступеней, пока без исследований».
Sawmill/quarry/iron mine have five per-building stages, producing1/2/3/4/5units
per cycle. Existing intervals remain5/7/10seconds. Upgrade each owned building
through the existing selected card, paid with materials and gold. New building
construction still costs only materials and starts at level1. Current prototype
allows all paid stages without research; the future technology/trader/workshop
route remains planned. No new upgrade centre or blanket free upgrades.
Upgrade pays once, retains existing fractional cycle and never grants immediate
production. The actual instance's output rises; other buildings stay unchanged.
D-087 retry keeps CURRENT level/progress/investment; full Reset removes buildings.
Existing building art, footprint, collision, pads, HUD/card geometry and prices
of new construction remain intact. No generator sale/merge/relocation, profession
XP from automation, offline or skipped-time income. Prices are trial data in
the existing ProductionCatalog; older5-stage/research recommendations are
superseded only for this approved prototype access and stage/output choice.

## D-094 — Three paid throne reinforcements through the existing card

Date:2026-10-20. Status: implementation of the owner's accepted D075 three
upgrades of+5max HP, requested as the next base-growth slice.
Each purchase adds5current HP as well, preserving absolute existing damage:
3/5→8/10. With current base5HP, caps are5/10/15/20; no fourth transaction.
Use the existing D078 selected-throne card and shared materials/gold wallet.
Prices are trial runtime data in HordeRun, not new research or a repair system.
Active preparation/wave permits paid upgrades; pause/modal/loss/reentrant payment
and insufficient funds reject unchanged. No upgrade may resurrect zero HP.
D087 retry preserves CURRENT upgrade count/spending and restores the upgraded
maximum; full Reset returns count0/base HP. Historical finite snapshots restore
count/HP/wallet together. Existing dome/damage/defeat visual rules and geometry
stay intact; effects use effective HP maximum. No new building or visual asset.

## D-095 — First fantasy-city concepts, one initial active merchant

Date:2026-10-20. Status: concept scope accepted by the owner's new request.
Prepare2–3significantly different city prototypes; choose three original ImageGen
studies consistent with the existing fantasy/isometric art. They are alternative
directions for the first city, not three approved cities. It has NPC inhabitants;
only one merchant will be active initially. Other NPC functionality is deferred.
This supersedes D063's earlier instruction not to prepare a city at that time
for the current concept phase. Appearance, location, trade UI/catalog/prices and
runtime implementation are not approved by this request alone. Main generates
and documents; Astra owns visual direction/review and later approved visuals.

Owner appearance selection: first concept A with orange terracotta roofs,
timber/cream-plaster houses and open square. Merchant stands visibly near the
entrance on the square's near-right edge. Astra concept PASS all three; A chosen
for later implementation. Source: art/concepts/city_2026-10-20/a_market_square_v1.png.
Exact dimensions/counts/site/stock and decorative edge water are not runtime rules.

Owner follow-up: make the selected city larger and its merchant shop more
distinctive, replacing the tent with a permanent building. Preserve the orange
roof/timber direction for housing; only one active merchant remains. A_v2 permanent
shop still blended into the background according to the owner. A_v3 proposes a
unique cobalt-blue hipped roof, open commercial front and large attached scales
sign with clear paved apron. This revised shop appearance is pending owner review,
not approved merely because A_v1 was chosen.

Owner accepted A_v3: «Пойдет», then requested the merchant window. Expanded town
and distinctive cobalt-roof permanent shop are the selected visual direction.

## D-096 — Merchant window requested, concept before visual implementation

Date:2026-10-20. Status: owner request after A_v3 city approval.
Prepare the merchant window in the current fullscreen interface family, using
existing gold and clear goods/description/price/purchase action. ImageGen concept
and owner appearance approval precede Astra's visual Unity work. The accepted
future merchant→research workshop route is relevant; a generic window request
does not fix catalogue/prices or add sellback, refresh, currencies, NPC services
or forge-scroll sources. Concept values are illustrative; runtime transactions
and access need actual rules/binding/tests. CITY_AND_TRADE.md owns this slice.

Owner rejected first storefront-style window and clarified two simultaneous
inventories: the player's bag and merchant's goods, like RPG trade screens. V2
must emphasize two item grids and compact identity portraits, with contextual
buy/sell selection; actual stock/sale values remain illustrative, not approved
balance or the tower half-refund rule. BG3/DOS2 are visual-pattern inspiration
only; no third-party UI art or additional barter/reputation mechanics are adopted.

Owner correction: merchant header displays gold only; wood/stone/metal are not
trading currencies and must be hidden in this window. A possible later trade
resource is not chosen and adds no current placeholder or currency.

## D-097 — Quality marking belongs to all item UI, not just trading

Date:2026-10-20. Status: requirement and final appearance accepted by owner;
implemented in existing item UI by Astra, local qualification recorded below.
Every equipment item UI presentation needs a rank-colored cell background and/or
border in addition to its quality letter and level. Start rough brown/grey and
progress toward gold; retain8ranks F/E/D/C/B/A/S/SSS, no SS. This supplements
rank-specific item art, without recoloring the hero/world or changing stats.
Astra proposes inner quality border/light wash and separate outer cyan selection.
Apply consistently in bag/loadout/forge/trading/actual tool previews after ImageGen
appearance approval. Ungraded research scrolls/empty slots stay neutral; generic
category symbols do not gain fake item ranks. EQUIPMENT_AND_RUNES.md owns scope.

The owner rejected blue-grey D versus blue C and muted gold, requesting one
all-item/all-quality sheet. Approved final64-cell board: «Во, это уже другое дело».
D is saturated GREEN, C royal BLUE; S visibly polished precious gold with crisp
bevels/reflections over dark warm backing. SSS is radiant WHITE-HOT GOLD over
dark plum with local halo/glints, sharp item contours retained. This replaces
earlier subdued/no-glow/steel-D proposals. Existing rank-specific art/stats stay;
the catalogue is approval material, not a new runtime atlas or stock table.

Owner correction after seeing runtime: the flat opaque dark blue/red/brown fills
and thick gold bars are rejected. Earlier native visual PASS is superseded for
appearance. Reproduce the approved sheet itself: painted multitone gradients,
subtle surface texture, thin shaped/luminous metal framing and rich S/SSS glow.
No new appearance proposal is needed to restore the already approved reference.

Main prepared eight empty skins by ImageGen editing that original; Astra gave
asset PASS and replaced the procedural rendering with the actual raster. One
borrowed Resources atlas uses measured pixel bounds, painted centres and thin
frame patches; no per-item texture generation or synthetic sparkle overlay.
Pixel-rounded edges and explicit scroll target/UV clipping remain. Existing
item atlases, gameplay, input positions and world art remain current. The narrow
quality-art importer preserves PNG bytes and generates normal Unity metadata;
no scene/Inspector/package wiring. Trading/city runtime will reuse this treatment.

After correction: 17 relevant PlayMode checks pass, Mac build succeeds, Astra
strict-reference visual PASS on 27 native views at720/900/1080p. Main actual
selection/equip/forge scroll works; ordinary startup restored without QA grants.
Windows remains unverified; tiny HUD cells retain hue/edge/radiance while losing
some engraving detail. Owner final runtime acceptance is not claimed by local QA.
Source/rights: art/source/item_quality_cells_2026-10-20_skins_generation.json.
Qualification: art/source/item_quality_cells_2026-10-20_verification.json.

Owner additionally requires safe areas and centring for every item: the bow was
too close to the frame, while helmet/arms/legs had vertical drift. Use baked
UI-only visible alpha bounds for all 64 type/rank pairs and proportional safe
padding, with uniform aspect-preserving fit. Preserve source artwork, held-world
pivots, skin appearance and input; no hand-tuned per-item UI displacement.

## D-098 — City services readable as interactive; paired circle in the city

Date:2026-10-20. Status: owner requirement accepted; updated visual concept
pending review. City/NPC/trade runtime remains outside the current item fix.

Local interactive services must be recognizable as clickable. Initially the sole
service is the merchant shop: add a visible colored contour and a small attractive
colored illustrated role icon above it. The icon must read as a merchant, with
painted fantasy detail rather than a generic square/circle/flat vector placeholder.
Future service roles will use their own icons; do not mark decorative housing or
invent additional active NPC services. Keep the approved cobalt-roof shop identity.

The city must contain the remote endpoint of the existing paired ground-circle
teleport. Reuse the approved seal and base-to-remote pair, not a third circle,
enemy portal or vertical arch. Research/discovery and existing travel gates remain
the contract. This supersedes the old deferred-city placement in WORLD_TELEPORTS.md
for the active concept; actual world placement follows the city's ready layout.

Main prepares the ImageGen city revision from approved A_v3 and the existing seal;
Astra directs/reviews and later implements approved appearance. Exact contour,
icon and city integration are shown before visual implementation. No merchant
stock, prices, extra currency or new travel network is inferred.

Current proposal: A_v5, compact plum/gold scales badge above the roof, thin
turquoise outer shop contour, original-style flat seal off the gate axis. Astra
concept PASS; owner appearance approval remains pending. A_v4 oversized cues
remain revision history. Exact prompts/references/output hashes:
art/source/city_2026-10-20_generation.json. No runtime city or portal placement
is changed by this concept delivery.

## D-099 — Temple-side city teleport and one opened-destination menu

Date:2026-10-20. Status: owner behavior/placement requirement accepted;
ImageGen city A_v6 and teleport-menu v1 passed Astra concept review; owner then
approved implementation («делай»). Modular city/native menu/network are integrated,
and locally qualified on Mac/Metal.16/16 focused PlayMode and native mouse/E
roundtrip plus three-resolution visual evidence are recorded in
art/source/city_runtime_2026-10-20_verification.json. Windows remains unverified.

The city teleport belongs beside a modest temple in a quiet side court, not on
the central square, market route or entrance axis. Preserve the approved ground
seal and the shop's distinct architecture/interaction cues. The temple is a
setting for the portal, not a newly approved NPC service, religion or progression
system. D-098's A_v5 square placement is superseded.

Clicking a travel portal opens the same destination menu everywhere: at the
base, in this city and at future world/city points. The player chooses among
opened teleports and explicitly confirms travel. This supersedes the pair-only
product limit and immediate counterpart hop in D-063/D-098; the existing
two-point runtime is migrated to that common menu; there are still only two
initial points in shipped data. No future town
count or third current runtime endpoint is invented.

Retain research and personal-discovery gates: unknown destinations are not
travel options. Distinguish the current point from the selected destination;
travelling to the current point is disabled. Closing the menu does not travel,
and arrival requires new deliberate input before another transition. Preserve
existing local proximity/mode/clear-arrival validation, free same-world travel
and D-087 current-progress retry. Existing E may open the same menu as the
mouse; it no longer bypasses selection with an instant hop.

The v1 proposal uses the Lunar Porcelain panel on subdued world context,
two opened rows «База»/«Город», «Вы здесь», destination preview, route and one
«Телепортироваться» action plus close. No resource bar, fee, fuel, cooldown,
new currency, fake locked city or enemy-invasion control. Exact prompts and
source/output hashes: art/source/city_2026-10-20_generation.json and
art/source/teleport_menu_2026-10-20_generation.json. WORLD_TELEPORTS owns the
travel contract; CITY_AND_TRADE owns setting; PLAYER_INTERFACE owns presentation.

## D-100 — First finite merchant inventory and gold buy/sell

Date:2026-10-20. Status: owner requested implementation of the approved shop
window and explicitly accepted the first stock/sale/restock proposal. Implemented
and locally Mac-qualified41/41PlayMode,21native frames, actual mouse buy/sell and
Astra visual PASS; Windows remains unverified.

Click the existing city shop to open both player and merchant inventory grids.
Buy and sell existing equipment for gold; exclude equipped items from the bag
and sale. Reuse D096's dual-inventory composition, D097's current painted skins
and D091's item identity/visible bounds. Wallet shows gold only.

First stock is exactly one of each8existing types,F/LVL1. Sale pays half current
purchase value; no automatic replenishment. Type/quality/level trial prices are
serialized in Stage3Merchant. Sold items remain finite merchant stock and retain
identity; purchasing does not equip automatically. No new scroll source, rune,
resource trade or item family is approved here. Local transaction validation,
D087 current-progress retry and separate full reset remain in force. Contract:
CITY_AND_TRADE.md. Qualification: art/source/merchant_2026-10-20_verification.json.

## D-101 — Medieval magical blueprint is the research/upgrade document base

Date:2026-10-20. Status: owner category and final rect_categories_shop_v3
appearance approved («апрув»); runtime implementation not yet performed.

Research documents should resemble medieval magical fantasy engineering plans,
not rolled spell scrolls or modern blueprints on blue paper. Begin with a common
base item/icon, then derive documents for research and building improvements
(workshop/forge and later others), new towers and weapon research. Spell scrolls
may serve future magic acquisition; that possibility is not a shipped system.

This clarifies the intended future appearance/category of the earlier generic
research/forge scroll wording. It does not change D090 forge stages/caps/SSS
restrictions, choose a source/price/consumption rule, or make every current paid
building upgrade require a blueprint. Current runtime scroll-counter naming and
merchant equipment-only stock remain until a ready approved implementation.
Main generates concepts/provenance; Astra directs/reviews and implements visuals
only after owner selection. Contract: RESEARCH.md and EQUIPMENT_AND_RUNES.md.
Owner correction after context review: tilted/unfolded forms and the later modern
white-sheet mockup are rejected. Reuse the earlier light paper texture/drafting
pattern in a straight rectangular sheet fitted to fixed-size cells/rows; metadata
or icon height must never resize/reflow other cells. Distinguish unlock,
construction and upgrade uses. NO equipment F–SSS or independent LVL1–100 on
blueprints: an upgrade number means TARGET BUILDING LEVEL. Examples4–5/forgeIV
do not extend the current D090 three-stage forge. Owner then resumed generation
and explicitly requires the SAME earlier aged paper colour, texture and ink style,
not plain white paper. One shared rectangular shell has TWO primary categories:
opening new content (including construction access) and upgrading existing
content. Owner approved rect_categories_shop_v3: same-position cyan spark for
opening and muted brass chevron for upgrade, with target-stage II/III only;
no item quality classes or item LVL. Approval covers shell/material/marks/fixed
cell treatment, not the illustrative catalogue,100gold price, stock/source,
consumption or runtime forge-counter migration. Standalone runtime art and a
ready implementation brief remain; do not request appearance approval again.
Source: art/source/research_blueprints_2026-10-20_generation.json.

## D-102 — Purchased blueprints are studied in a permanent single-level workshop

Date:2026-10-20. Status: owner requested runtime implementation and accepted the
first catalog in an explicit question reply: Longshot build unlock, wand recipe,
forge targetII/III. No new tower/weapon type is approved by this slice.

The research workshop itself has exactly one level and no improvement action,
superseding D075's proposed workshop advancement. Purchase at a merchant,
research at the workshop and use of the resulting building/recipe permission are
separate actions. D101 approved rectangular paper/category appearance remains.
Owner approved the three-column study window, then known/history concepts:
study consumes the owned paper; knowledge remains under «Изучено». Duplicate
papers in trade/bag show a small silver/teal checkmark and «Изучено» detail pill,
without changing paper appearance or blocking trade. Repeat study spends nothing.
Use «Открывает улучшение кузницы»; actual upgrade remains a separate action in the
forge, with no second document/legacy-scroll spend. Scope/rules and current status:
RESEARCH.md/DEVELOPMENT_PLAN.md; visuals/provenance:
art/source/research_workshop_2026-10-20_generation.json.

## D-103 — Hero starting vitals, ranked Blink and test-wave launch

Accepted2026-10-20: owner requests100HP/100MP at start. Blink has4ranks
(initialI plus3upgrades): mana100/75/50/25 and cooldown5/4/3/2seconds;
confirmed recovery1HP/s and5MP/s per active game second. Existing300/500/800/1000
ranges and point costs remain. Failed/blocked casts spend neither resource nor
cooldown. Current retry preserves current mana; full Reset restores full vitals.
No hero damage source/death is added by the starting-vitals slice.

Owner requests working F3 next-wave launch for testing. Ordinary Stage3 start
stays clock-gated; explicit development command moves clock to next pending
night without skipped-time economy/regen, closes daytime gate and retains live
creeps/shots. Pause/active wave/terminal block duplicate launches. Existing
camera max10(size2xstart5) is extended to15(3x) on both wheel and pinch, close
limit2.5 and starting5 unchanged. Measured top-edge body clipping is corrected
by existing hero edge clearance1→1.5, preserving strict camera ground bounds
and art/angle/scale. Existing HUD layout/artwork stay; Astra binds
real numeric bars/costs to the current approved presentation.

Owner further requests a Debug WTF toggle: all currently implemented active
abilities (currently Blink) consume no mana and have ZERO cooldown. The earlier
0.5s phrasing was explicitly corrected. Toggle clears an existing cooldown;
turning off restores ordinary costs/rank timings; full Reset/HUD disable clears
WTF. It does not bypass pause, modal UI or collision/landing validation, grant
ranks/XP or refund spent mana. No extra active spell is invented.

## D-104 — Hero death and recoverable bag, accepted next feature

Accepted2026-10-20: portal-path enemies attack throne/blockers and must NOT
hurt the hero. Future exploration enemies can hurt/kill the hero. Death gives
free reincarnation at the base portal, removes half the XP accumulated within
the current level and never lowers that level. Only the BAG drops: equipped
armor/weapon/tools stay on the hero. Currency/resource counters are separate
from the bag; no additional currency/resource penalty was requested.

Dropped owned instances remain recoverable at the death location, visible in
world and on the map, with a world direction indicator leading to the body.
Owner explicitly chose implementation as the NEXT SEPARATE STEP, after current
HP/MP/Blink/debug/zoom work and an approved marker concept. No current death,
corpse drop/claim, respawn or new exploration-enemy source is implemented by
D103. During the approved D104 implementation, routine details are: lose
floor(currentXP/2), retain all earned levels/points/ranks/Blink cooldown; restore
HP/MP at immediate safe base-anchor respawn without unlocking travel research.
Every nonempty drop persists until recovered, including across further deaths
and throne retries; empty bag creates no marker; full Reset clears all drops.
E pickup claims the interaction frame and requires fresh press after death/retry.
F3 development death command adds no items/XP and uses these same rules.

Latest owner correction: label «Выпавшие вещи», recovery «E · Забрать вещи».
Offscreen cue is a compact text tooltip with a small arrow on the destination
side, rotating through8bearings and clamped clear of existing HUD/top cards.
Large standalone arrows/medallion pointers and earlier labels are superseded.
Owner approved final world_marker_v3/map_marker_v2/offscreen_tooltip_v4 and
subsequent native implementation; qualification is in the canonical plan.

## D-105 — Unbounded daytime flow of the last CLEARED night's difficulty

Accepted2026-10-20, clarified by owner: daytime gates repeat only the last WON
night's difficulty, never a newly attempted/lost night. D074 last-cleared tier
selection remains correct. D087 retry returns morning before the same pending
night and keeps current world progression; it does not unlock the failed tier.
Before the first night victory there is no cleared tier to farm.

Daytime is an endless stream until manual close or19:30 cutoff before20:00.
Repeat that cleared tier's spawn interval, HP/siege multipliers and existing
creep type; do NOT copy its finite night enemy count. No total-spawn quota or
old24-live-enemy throttle: both would interrupt the requested ongoing stream.
Closing/cutoff stop only new spawns; living enemies/shots remain. Reopening
preserves remaining spawn time; closing cannot bank a catch-up burst. Types
remain the current single type; future typed profiles are not invented here.
Infinite flow requires positive profile intervals; legacy finite instant waves
remain valid with daytime gates disabled. Old serialized daytime cadence/cap
fields remain only for compatibility and do not control the current flow.

Owner also delegates a modest first-horde buff. Current HordeRun first profile
gets more enemies and shorter spacing, mirrored in its builder; later night
profiles, HP growth, clocks, XP/gold and retry rules stay. Trial tuning lives in
runtime data. No new UI design, resource, package or scene migration.

## D-106 — Ten-minute day and three-minute continuously spawning night

Accepted2026-10-20 during D104 concept work: daylight lasts10active real minutes,
night3at normal speed. Pause/simulation speed apply normally. Clock hours stay
08:00–20:00 day and20:00–08:00 night; separate rates map these to600/180seconds.
Night spawns are time-driven, never limited by authored enemy count. New spawns
stop at dawn; living enemies/shots remain. Victory/tier advancement requires
dawn plus all remaining threats cleared. No early win before dawn, no stacked
mandatory nights while an earlier one is unresolved. D105 daytime stream stays
last WON difficulty, with19:30 cutoff; D088 sleep still ends19:50 outside combat.
D087 retry preserves current progress and returns full morning for same tier.

Current Stage3 explicitly opts into timed nights via nightDurationSeconds=180;
zero retains finite legacy fixtures/catalogs. Day duration stays in existing
wave preparation data, set600. Old finite count remains legacy data/forecast
only in Stage3, not a spawn limit. Existing per-tier intervals/HP/current type
are retained initially: longer spawning sharply increases density (tier2 at
0.12s produces about1500 creeps/night), so live economy/defense balance remains
trial tuning to qualify, not an automatically approved new tower balance.
No new visual UI, package, manager, scene migration or automatic time skip.

## D-107 — Centered analog cycle and shaped top HUD

Accepted2026-10-20: replace floating resource/HH:MM corners with a slim shaped
upper HUD attached to screen top. Compact resources left, day count/development
access right, small complete day/night dial centered, whole rim inside screen.
Keep world visible under tapered/concave bridges; no massive empty ivorybar.
Owner approved v2 silhouette and selected variantA smoke-glass after v7ratio
correction. DAY10minutes is10/13ofcircle, NIGHT3minutes is3/13≈83degrees;
this latest ratio supersedes the earlier10:30-to3 illustrative sector request.
Dawn starts at3o'clock, clockwise through277degree day to~12:14, then83degree
night backto3. Arrow follows continuous13minute cycle, pauses/retry/sleep with
world clock; entering night sector starts mandatory stream by existing D106rules.

Soft atmospheric transition, no sharp radialpiechart boundary. Mutedgreyolive/
ivory daylight, almostblack/charcoal night with restrained darkbloodredmiasma;
no cheerfulpurple/starfield or landscape/ruins/creature illustrations in dial.
Astra owns native visual implementation using approved A assets/native existing
UI style; main owns cycle fraction/binding/tests/provenance. No new clock rules,
currencies, controls or UI stack. Reference/approval/prompts:
art/source/cycle_top_hud_2026-10-20_generation.json.

## D-108 — Temple mage and purple ogre appearance; orc scale revision

Supersession: D110 cancels temple mage/quest-site direction; approved orc/ogre
appearance remains. Prior approvals below are historical.

Status: Accepted appearance, 2026-10-21; concept-only work.
Owner response to the v2 sheets: «Орку чуть-чуть меньше сделать. остальное ок».
Temple mage v2 (long silver hair/beard, ivory/plum/gold ceremonial robe and
cross-shaped staff) and purple ogre v2 are approved. Retain their appearance;
reduce the orc slightly from v2 while keeping him larger than hero C.

The owner also asks to see the NPC with temple/portal in map context. D099
already supplies both city elements; show a separate courtyard placement study
with mage beside the steps and open door/street/portal approaches. Final orc
scale and placement initially awaited review. Follow-up owner reply «Утверждаю
размещение и размер орка» approves orc_ogre_comparison_v5 and
temple_courtyard_placement_v1, completing the appearance approval.
«Хорад/Хород» was dictation, not a city name.
Main owns ImageGen/provenance; Astra owns visual direction/review and later
approved visual implementation. Quest rules/rewards/combat slice are still open.
Reference: art/source/quest_npc_world_enemies_2026-10-21_generation.json;
CITY_AND_TRADE.md records the current concept state and D099 runtime evidence.

## D-109 — Temple quest window and separate open-world encounter spots

Supersession: D110 replaces the temple quest-giver/window with an adventurers'
guild board. Separate camp/ruin spots and future-enemy scope remain accepted.

Status: Accepted intent, 2026-10-21; window appearance pending concept approval.
Clicking the city temple mage should open a window for selecting/receiving
quests. First prepare an ImageGen sketch in the current fullscreen UI style;
main generates, Astra directs/reviews the appearance. No new global shortcut
or quest-system implementation is inferred from this concept request.

The world will contain distinct encounter spots such as camps and ruins with
different enemy kinds. Current orcs and ogres occupy separate spots, not one
mixed encounter. Goblins and other undead are future content examples, not
current asset/content requirements. Exact camp count/placement, populations,
reset/respawn, objectives, quest lifetime and reward values are still open.
OBJECTIVES.md owns the active quest brief, HERO_COMBAT.md the world-enemy intent.

## D-110 — Adventurers' guild, physical contract board and rank trials

Status: Accepted direction/progression, 2026-10-21; appearances awaiting sketches.
Owner explicitly cancels the temple/elder quest-giver and rejects the form-like
quest window. Replace that quest-service concept with an adventurers' guild:
paper orders pinned to a beautiful in-world board, with location and reward.
Group/filter orders by F/E/D/C/B/A/S rank through game-like rank boards/tabs;
show available categories and distinguish unavailable access. No SS/SSS quest
category or extra currency is inferred from equipment quality.

The player starts with an F guild/adventurer badge. Three successful current-rank
quests unlock the ability to accept one next-rank trial. Completing that trial
grants the new rank/badge and access to all orders of that rank. Repeat toward S;
three completions alone do not promote. This is personal guild progression,
not a request for guild-building upgrades. No tier above S is assigned.

Orc and ogre appearances and separate world spots remain; the temple mage and
its quest-site appearance from D108 are superseded. Existing city temple/portal
runtime is not silently changed; first sketch a guild replacement in that corner,
keeping the nearby teleport endpoint/network. Main generates through ImageGen;
Astra directs/reviews appearances and later implements approved native visuals.
Exact rewards, whether repeat orders count, trial contents/failure/retry, multiple
active contracts, token inventory behavior and encounter refresh remain open.
OBJECTIVES.md owns the active brief. These decisions do not implement quests.

## D-111 — Approved guild implementation, repeat orders and board turn-in

Status: Accepted,2026-10-21. Owner approved board_f_v1, trial_e_v1 and final
city_v4_open_temple with «делай / апрув». Keep BOTH green-roof guild/board and
small original temple; teleport has its own open public temple forecourt, with
no front fences/gates or road blockage. Temple elder quest NPC remains canceled.

Repeat orders count toward the three current-rank successes. Accepting a new
order refills/resets its typed spot; selecting/opening/turning in never refills.
After clearing, return to the guild board to turn in and collect the reward.
No automatic contract reward/promotion at the camp. Guild ranks remain F…S.
Main owns plain gameplay/AI, integration/data/tests; Astra owns approved native
city/board/actor visuals. First slice reuses separate orc/ogre spots with data
profiles for each rank and one next-rank trial. One active order per spot avoids
resetting another active objective. Exact balance is trial serialized data.
Retry preserves CURRENT guild/contracts/camp state; full reset returns F.

D111 follow-up: owner makes the outdoor notice board purely decorative: smaller,
slightly right of the guild entrance, clear/even spacing. Click the GUILD BUILDING
to open the contract-board menu. Local actions use the free entrance approach,
not the decorative board. No separate board interaction or new shortcut.

D111 second placement refinement: the owner still found the board in front of
the entry and too prominent. Move it substantially farther right, parallel to
the entrance, reduce it again and mute its opaque colors. It remains decoration;
Guild building click and free entrance interaction anchor are unchanged.

## D-112 — Visible city navigation independent of teleport discovery

Status: Accepted corrective request,2026-10-21. Owner cannot find city on the
HUD minimap or M. The city has its own persistent navigation marker from world
initialization, independent of unopened/research-gated city teleport. M initially
shows the complete map with a city landmark; later user zoom/pan is
retained. Local minimap depicts city in range or its direction at the rim.
This is navigation only: no automatic discovery, research, teleport or map-click
travel. Marker follows actual generated CityOrigin and survives full gameplay
reset while the city exists; regeneration updates it and ClearCity removes it.
Existing approved city art/UI is reused by Astra; no new currency/control/art
family. Main owns marker data/lifecycle and relevant tests/native qualification.

D112 owner refinement: remove «Город» and «Вы здесь» captions from M; keep
recognizable city/hero icons, the safe overlap offset and minimap bearing. Red
world points currently represent individual living enemies, not static camp icons.

## D-113 — Starter teleport construction and readable clock

Accepted owner request,2026-10-21. Teleportation is the one research available
from the beginning without a merchant. Use the owner's allowed direct-study
option: a pinned starter blueprint entry in the workshop, not a removable bag
instance. It cannot be sold, thrown away or dropped on death. Studying locally
unlocks a construction card; it no longer creates a base circle automatically.
Player places one circle on valid free ground. Current prototype research/build
remain free; no new prices are inferred. Existing discovered destination rules
remain. D087 retry preserves current knowledge/circle/location; Reset clears.
Future tutorial and user map pins/active guidance remain deferred.
Remove the automatic direction-to-city on the minimap; show a local city icon
only in its actual crop. M stays icon-only and known from start. Increase the
approved analog dial to about twice its former size, keeping10:3 and make the
painted hand brighter. Existing face/shape/style retained.

## D-114 — Guild interaction and stationary-only hero bow

Accepted owner correction,2026-10-21. Quest acceptance must work from the normal
building approach. Service proximity follows the actual guild ground footprint
and existing interaction reach, rather than one tiny displaced point. Far
preview stays readable and asks to approach the GUILD, never the decorative
board; remote turn-in remains blocked. Accept resets only its typed camp and
turn-in still requires a return to the building.
Hero automatic bow cannot start a shot while ground-moving in any direction.
Cooldown continues during walking; released arrows still land. No forward-fire
moving skill is introduced; that remains a possible future idea. Walking visuals
immediately follow movement instead of holding a previous shot pose.

## D-115 — City fidelity correction and open world-camp concepts

Accepted owner request,2026-10-21. Current city layout/density fails the approved
city_v4_open_temple appearance. Preserve existing merchant and guild buildings;
match organic populated blocks, connected streets, gardens and set dressing
using original approved modular art, with a separate open temple portal court.
No new service/NPC is assigned. Native review against the approved reference is
required; this is a correction, not acceptance of a new city direction.
Prepare ImageGen concepts for separate orc and purple-ogre spots. Initial v1
closed camps were rejected. Required v2: a WIDE OPEN encounter territory with
multiple broad escape/reposition routes for stationary-only shooting. Camp/ruin
props belong beside the combat field, not enclosing it or funneling the entrance.
Concepts do not assign enemy populations, loot, new systems or runtime scenery
until owner appearance approval. Owner subsequently approved both open v2
concepts («Да, оба подходят»); environment runtime remains a later step after
this sketch request, with no new population/loot tuning implied.

## D-116 — Approved open camp environments in the world

Accepted implementation request,2026-10-21. Implement both approved orc_camp_v2
and ogre_camp_v2 as separate world environments. Keep wide open fighting fields
and several broad retreat/reposition routes for stationary-only bow combat.
Modular hide/timber orc props and ancient-stone ogre ruins cluster to one side;
no enclosing fences, gates, central collision clutter or baked actor backdrop.
Reserve entire encounter territories before additional resource generation.
Substantial visible props share actual ground footprints; existing species,
population, combat tuning, quest repeat/turn-in and retry rules are unchanged.
Main owns ImageGen/site/lifecycle/collision/tests; Astra owns native visuals.
No new quest, NPC, currency or loot source is inferred from this environment task.

## D-117 — Fixed authored city layout and renewed reference correction

Accepted owner correction,2026-10-21. Previous D115 local visual PASS was NOT
owner appearance acceptance: current city still fails reference layout/structure.
A separate Astra must correct actual arrangement against city_v4_open_temple.
This city has a fixed authored internal composition, NOT random filling or
procedurally shuffled blocks. Future cities get individual authored layouts and
styles. Selecting one clear world location for the whole city remains allowed;
internal buildings/streets/courts/planting keep their designed relationships.
Keep original service art and mechanics; enlarged guild to the left, temple
and its open portal court to the right, shared civic space/well and compact
housing quarters must read as the approved reference. No new city service.
Require centred native views for honest comparison; do not equate tests with
owner visual approval.

## D-118 — Quest guidance, confirmed field feedback and world corrections

Accepted owner request2026-10-21; both ImageGen directions then appearance-
approved. Accepting a guild order shows its destination on M/minimap and tracks
one chosen order. Cleared order points to local guild turn-in; selection may
switch between accepted orders. No autopath/remote reward or unconditional city
pointer. Add compact actual HP and hit/floating damage feedback to BOTH world
neutrals and ordinary red-portal enemies, including tower hits. MP only if an
actual enemy mana source exists; current orc/ogre/skeleton profiles have none.
Manual axe/pickaxe feedback shows actual +material grants, chips/dust/spark;
there is no fabricated resource HP or damage. Cancelled strikes pay nothing.

All visible wild trees/ore/natural rocks are harvestable, including authored
camp shoulders; city planting and auto-production districts remain exempt.
Preserve already-approved scenery pixels/positions and bind ordinary finite
stock/tool/mastery/depletion/recovery/collision instead of duplicate renderers.
Camp units stand in a compact group near their authored shelter/ruins; contact
or damage to one alerts its living same-camp generation, lethal first hits too.
Existing separate camp identities, population/rewards/leash and open retreats
remain. Teleport click/E opens existing shared explicit destination menu from
anywhere on/at the seal, using the painted ground for picking; discovery,
research, local checks, chosen destination and confirm remain mandatory.
Construction must not reserve invisible unbuilt workshop/circle sites or
already-depleted manual nodes. Reclaimed building ground blocks resource
regrowth while occupied; real visible resources/terrain/footing and approach
constraints still apply. Clear cause-specific hints replace generic red denial.

Main owns rules/events/tests/integration; Astra owns approved native UI/feedback
and unchanged-art resource adapter. No new damage balance, quest reward source,
NPC/service, package, scene migration or saved-game format implied.

## D-119 — Hero development expands to ten actives and twenty passives

Owner approves the tree-based Lunar Porcelain direction and requests10active
combat skills and about20passive combat talents, superseding older12/48
proposal. Gathering/adventurer trees may be smaller. Categories are Бой / Добыча
/ Авантюрист; workshop research remains separate. Only the O dock icon is
replaced with a branching skill-tree sign; other dock icons remain.

HERO_PROGRESSION.md contains a concrete named10/20catalogue for review, smaller
gathering/adventurer proposals, point budget and explicit implementation gaps.
Counts/appearance approval do not turn illustrated crits, split arrows, new
rewards or unimplemented controls into current gameplay. Current earned ranks,
separate miner/logger mastery and existing hero points must survive migration.
Owner then explicitly requests fire AND lightning splash with25–30second
initial cooldown falling by ranks to minimum15seconds, preserving tower
necessity against dense hordes. Also requests a trained Mobility talent that
unlocks attacking while moving; starting heroes keep stationary-only launch.
These supersede older blanket no-splash and unconditional stationary constraints
only after the relevant skills are learned. Revised catalogue replaces the
flame-arrow/mark proposal with Fireball/Lightning, keeping10active total.
Owner returned v2's grid and active/passive tabs: use ONE progressively
connected graph with large active/small passive nodes, showing prerequisites.
Remove confusing left specialization navigation; no new class selection.
Owner then rejects all left legend/info cards as wasted space: remove the
sidebar entirely, put level/XP/points in one compact header and expand/recompose
the working graph across the reclaimed width. Current v5compact-tree ImageGen
awaits appearance review before Astra full-page work.
Active skill composition/input/effects and trial tuning remain the next ready
implementation brief, separate from D118field fixes.

## D-120 — Bow/staff loadout, eleven actives and accessory expansion

Date:2026-10-22. Status: owner APPROVED tree_v1/hud_v2/equipment_v2, named5+5,
Q/Z/1–5/F1–F4/F10 and protection/amulet composition. Implemented in the current
Stage3 profile; local qualification: art/source/hero_loadout_2026-10-22_verification.json.
Windows/Steam and sustained balance remain unverified.

Bow and long staff occupy two equipped positions, with one active weapon.
Exactly11actives: universal BlinkQ and five skills per weapon on contextual1–5.
Z/clickable thumbnails switch active weapon without resetting cadence/cooldowns,
refunding MP or stacking both weapons' stats. Wrong-weapon abilities cannot cast.
Armed targeting/unfinished aim cancels without spend on switching. Both equipped
instances remain excluded from bag/trade and are retained through hero death.
Staff preserves internal Wand/WandRecipe IDs, purchased paper→local one-level
workshop study→separate forge craft. Equipping an acquired staff needs no recipe
knowledge. No enemy loot drops are added by this slice.

O is ONE progressive mixed graph: shared Blink root left between bow/staff
routes,11large actives and20small passives, no left legend/sidebar. Four shared,
eight bow and eight staff passives preserve the three original passive IDs/ranks.
Learned moving attacks work for automatic bow/staff, while aimed shot needs
stopping. Fireball/lightning start25/30s and fall to15s, bounded small radius and
four targets; towers remain necessary for dense matched-tier hordes. Four active
ranks/three passive ranks and actual level/prerequisite gates use the existing
99hero points, no new level cap/point income. Three implemented adventurer
bonuses affect actual contract XP/gold and world-only kill XP. Gathering keeps
its independent existing mastery/points; larger earlier lists remain proposals.

Twelve gear positions retain four armor/two tools and add distinct staff,
amulet, two rings and «Магическая защита». «Защитная печать» replaces rejected
«Оберег»: standalone arcane covering, constant incoming-damage reduction with
NO MP cost, bounded before active barrier absorption. «Амулет натиска» increases
hero damage; a sample ring raises maximum MP without refilling on equip.
Accessories use ordinary F/E/D/C/B/A/S/SSS andLVL1–100, existing forge stage caps,
no forged SSS. Around40future amulet effects remain future scope. Values live
in Stage3Equipment/Stage3Merchant; existing armor scope is unchanged.

Four fixed consumable positions use F1HP/F2MP/F3mixed/F4reserved, debug moves
toF10. Owner explicitly sets HP100gold→100HP, MP100gold→100MP, mixed250gold→
100ofEACH. Finite initial stock5each, no auto-restock, shared20s cooldown.
Buy/sell ONE bottle with actual bag stacks; no spend at full relevant vitals,
no free bottles on Reset. Potions remain bag items, including death/recovery;
quickslots never create an equipped duplicate. Neutral cells/no equipment rank
or level. Transactions/effects/points are guarded against partial/reentrant spend.

All new visuals are Astra-owned native implementation from approved concepts
and original ImageGen atlases; current painted quality frames are reused.
Official GW2/ESO references inform structure only. Main owns ImageGen, rules,
data/integration and qualification. Retry preserves current development/items/
stock/vitals/cooldowns; Reset clears new ranks/buffs and restores starter state.
No new scene/package/global manager/save format. Specs: HERO_PROGRESSION.md,
EQUIPMENT_AND_RUNES.md, HERO_COMBAT.md and RESEARCH.md.

## Pending Product Decisions


- Текущая карта Stage 3 — 250×250 по D-049, боевая часть в нижнем левом углу
  по D-050, форма дороги уточнена рисунком и подтверждением D-051.
  По D-062 продукт включает выживание, огромный мир и развитие базы; точный
  размер, состав систем и минимальный проверяемый коммерческий срез открыты.
  Темп длинного маршрута и будущие места исследования/добычи ещё оцениваются.
  Первые три авторские орды продолжаются усиливающимися ночами по D-065;
  параметры остаются пробными. Старые размеры и
  P-004/P-008 анализы остаются историческими свидетельствами в feature specs.
  D-074/D-087/D-088 уже реализуют дневной фарм, непрерывную ночь, сон до19:50
  вне боя и повтор с сохранением текущего развития. Cutoff и темп длинной ночи
  остаются пробным балансом; старый D-065 описывает историческое поведение.
- D-039–D-041 повторно проверяют спрайтовое производство для выбранных объектов;
  D-027 остаётся историческим решением о 3D, камера сохраняется. Прежний состав
  MVP D-032 уточняется по D-062; вид каждой следующей модели и бюджет массового
  производства открыты.
- Финальный UI stack и необходимость сохранения незаконченного забега
  решаются, когда начнётся соответствующая работа. Старые P-005/P-006 не
  блокируют первый играбельный билд. D-087 заменил полный снимок сохранением
  текущего прогресса при повторе; сохранение между запусками этим не определено.
- Текущий срез исследований, города и торговли уже реализован по D099–D102:
  одна мастерская без улучшений, конечный запас документов, отдельные покупка,
  изучение и постройка/крафт. Дальнейшие источники документов, новые городские
  услуги и расширение дерева исследований остаются будущими решениями.
- D092 реализует профессии добычи; D104 определяет смерть и возврат вещей;
  D120 — текущие навыки, очки, два оружия, аксессуары и зелья. Старые пометки
  об отсутствии этих систем больше не действуют. Более крупные деревья добычи,
  руны/их лимиты, новые эффекты аксессуаров и дроп требуют отдельного среза.
- D093 уже задаёт пять ступеней генераторов; D094 — три улучшения трона;
  D079 — перемещение/слияние/продажу башен. Финальные цены и темп развития
  остаются пробным балансом, а не нерешённым числом ступеней мастерской.
  Место боя и реализация трёх боссов по D075 ещё открыты. Бриф второго врага
  S2-AI-01 не является активным заданием. Windows/Steam и длительные забеги
  D120 ещё требуют проверки; локальная Mac-квалификация их не заменяет.
