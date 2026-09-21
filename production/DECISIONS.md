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

## Pending Product Decisions

- P-002: правила строительства во время волны.
- P-003: истощение и восстановление ресурсных узлов.
- P-004: длительность подготовки, число волн и длина забега.
- P-005: save policy для незавершённого забега.
- P-006: финальный UI stack — UI Toolkit или uGUI.
- P-007 post-MVP: tool upgrades работают внутри забега или как meta-прогрессия.
- P-008: целевой размер карты (первый кандидат 100x100; 200x200 только после
  performance и travel-time проверки).
