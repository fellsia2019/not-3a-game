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

## Pending Product Decisions

- Текущая карта Stage 3 — 250×250 по D-049, дорога принята D-045.
  Темп длинного маршрута и будущие места исследования/добычи ещё оцениваются.
  Три волны — текущий кандидат, не окончательный баланс. Старые размеры и
  P-004/P-008 анализы остаются историческими свидетельствами в feature specs.
- D-039–D-041 повторно проверяют спрайтовое производство для выбранных объектов;
  D-027 остаётся историческим решением о 3D, камера сохраняется. Состав коммерческого
  MVP принят D-032; вид каждой следующей модели и бюджет массового
  производства открыты.
- Финальный UI stack и необходимость сохранения незаконченного забега
  решаются, когда начнётся соответствующая работа. Старые P-005/P-006 не
  блокируют первый играбельный билд.
- Позднее отдельно определить правила мирных зданий на зелёной зоне,
  исследований и улучшений. Прежний P-007 о tool progression остаётся поздней
  идеей. Бриф второго врага S2-AI-01 не является активным заданием.
