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

Status: Accepted as delegated working direction, 2026-09-20.  
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
flight части D-010; остальные положения D-010 о композиции карты сохраняются.
Reason: вертикальный traversal не усиливает основной gather/maze/defend loop и
создаёт лишнюю механику движения, визуальную неоднозначность и pathing scope.

## D-012 — MVP Asset Detail And Deferred Blender Work

Status: Accepted, 2026-09-24.
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
ещё не выполнены. Решение заменяет finite/no-recovery assumption только для
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
Это принятие правил, не runtime verification. Stage 1 и API S2-MAP-02 остаются
неизменными; их prototype assumption о строительстве уточняется для Stage 2/MVP.

## Pending Product Decisions

- P-004: review-ready proposal after S2-WAV-01, 2026-09-24. Current one-wave
  baseline is rejected as representative cadence: observed early-start win
  65.083 s; timed unopposed loss 219.484 s, including 180 s preparation.
  On 2026-09-24 the human delegated the proposed cadence choice and trial
  implementation ("делай как считаешь правильно и лучше всего"). The trial
  preserves towers/inventory/node recovery and throne health between waves,
  retains optional early start and grants no automatic reward or repair.
  Runtime tuning belongs to Stage2RunPrototype.asset. Final representative
  pacing remains open; the 15–25 minute vision is unchanged and unproven.
  Full rationale and
  recovery limits: [STAGE2_RUN_STATE.md](../gamedesign/features/STAGE2_RUN_STATE.md).
- P-005: save policy для незавершённого забега.
- P-006: финальный UI stack — UI Toolkit или uGUI.
- P-007 post-MVP: tool upgrades работают внутри забега или как meta-прогрессия.
- P-008: review-ready proposal after S2-MAP-03, 2026-09-24: keep 100x100 and
  existing masks for Stage 2; no 200x200 or mandatory topology rebuild.
  Live source travel is ordered, measured navigation/load is viable on the
  recorded Editor hardware, and three paid towers can fully block the
  entrance. Human acceptance is pending; this is not a permanent MVP-size
  lock or an external-player gate pass. The human's future rectangular-map,
  offset-throne, winding-gray-area and possible gray-only-building ideas are
  recorded as tentative options, not current implementation requirements.
  Evidence, limits and the 2026-09-24 clarification:
  [STAGE2_MAP.md](../gamedesign/features/STAGE2_MAP.md).
- S2-AI-01 scope proposal: baseline ground enemy plus one siege specialist,
  initially differing only in siege output. Brief delivered; human acceptance
  required before adding runtime profiles. See
  [STAGE2_ENEMY_ROLES.md](../gamedesign/features/STAGE2_ENEMY_ROLES.md).
