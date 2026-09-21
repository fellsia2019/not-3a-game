# Development Plan

Canonical status: **Stage 1 complete and human-accepted on 2026-09-21 after
D-009/D-011. Stage 2 is not active.**

Каждый этап закрывается только после реализации, проверки, обновления
документации и фиксации оставшихся рисков. Перепрыгивать к массовому контенту до
вертикального среза нельзя.

## Stage 0 — Foundation And Concept Lock

- [x] Зафиксировать high concept, MVP boundary и post-MVP идеи.
- [x] Выбрать достижимое визуальное направление и разобрать референсы.
- [x] Добавить правила агентов, структуру документации и feature template.
- [x] Утвердить GAME_VISION и роль героя без прямого боя.
- [x] Перенести обучение и progression инструментов в post-MVP.
- [ ] Подтвердить оставшиеся P-002–P-006 и P-008 перед затрагивающей их
  durable реализацией.
- [x] Инициализировать Git и проверить, что generated Unity folders игнорируются.
- [ ] Зафиксировать рабочее название, владельца IP и источники/лицензии референсов.
- [ ] Решить, оставляем ли experimental `com.unity.pipeline`.

Gate: проект воспроизводимо открывается; видение, MVP/non-goals и ближайший
прототип согласованы; исходники защищены системой контроля версий.

## Stage 1 — Technical Graybox Prototype

Status: **Complete and human-accepted, 2026-09-21.**

Сохранить проверенные movement/gathering/objective/run-state части
`gamedesign/features/GRAYBOX_CORE_LOOP.md`, но заменить fixed route на
`gamedesign/features/DYNAMIC_ENEMY_PATHING.md`: размещение башен изменяет путь,
зигзаг увеличивает пройденную дистанцию, полная блокировка переводит врагов в
атаку на мешающие башни, а после разрушения маршрут восстанавливается. Проверить
это на репрезентативном компактном map slice; полный размер карты не входит в
Stage 1.

Gate: **Passed.** Цикл «добыча → строительство лабиринта →
динамический маршрут или разрушение блокера → защита» реализован до
win/lose/restart; EditMode 8/8 покрывает path search, deterministic zigzag и
blocker selection, PlayMode 4/4 — placement, win/loss/restart и восстановление
прохода после разрушения башни. Windows x64 Development Build собран и запущен
без attributable startup errors. Human playtest подтвердил читаемость,
maze-building, динамический маршрут и разрушение полной стены.

## Stage 2 — First Playable / Vertical Slice

Перенести проверенный maze-loop на масштабируемую data-driven карту. Первый
кандидат — 100x100 клеток; 200x200 оценивается только сравнительным performance и
travel-time тестом. Реализовать серый invasion corridor и зелёную территорию
лесного биома как разные поверхности в одной плоскости, дальностное размещение
дерева/камня/металла, три роли башен, набор
врагов, несколько волн, фиксированные gather/build objectives, первую
репрезентативную графику, звук, feedback и понятный UI. Создать один законченный
забег без отдельного tutorial/onboarding.

Gate: внешний игрок понимает игру без разработчика; визуал и UX показывают
целевое качество; core loop заслуживает продолжения по результатам playtest.

## Stage 3 — MVP Alpha

Зафиксировать размер карты по данным Stage 2, наполнить её в пределах MVP,
закрыть утверждённые фичи и контент, настройки, controls, save policy, базовую
доступность, баланс и pathing edge cases. Не расширять scope новыми биомами или
системами.

Gate: feature complete; весь контент доступен; нет известных блокеров полного
прохождения; сборка и данные переживают обновление.

## Stage 4 — Beta And Polish

Только исправления, UX, производительность, совместимость разрешений/железа,
аудиомикс, баланс, локализация выбранных языков и clean-machine QA. Зафиксировать
content lock и процедуру сборки.

Gate: release-candidate build стабилен, не теряет данные, не имеет новых console
errors и проходит полный regression checklist.

## Stage 5 — Steam Readiness

Начать Steamworks заранее: юридические/банковские/налоговые данные, app fee,
store assets/copy, content survey, pricing, depot/branches, launch options и
SteamPipe. Опубликовать Coming Soon только после стабилизации визуала и core
feature set. Отправить store page, затем near-final build на review с запасом.

Gate: страница и build одобрены Valve; Coming Soon выдержал обязательный срок;
игра ставится и запускается через Steam на чистом Windows ПК; rollback/hotfix
проверены.

Актуальные на 2026-09-20 ограничения, которые нужно перепроверить перед релизом:
[$100 за приложение и возврат fee после порога выручки](https://partner.steamgames.com/doc/gettingstarted/appfee),
[30 дней после оплаты fee и минимум 2 недели Coming Soon](https://partner.steamgames.com/doc/gettingstarted/onboarding),
[отдельные review store page и near-final build](https://partner.steamgames.com/doc/store/releasing),
[закладывать минимум 7 рабочих дней на каждый review](https://partner.steamgames.com/doc/store/review_process)
и загружать depots/branches через
[SteamPipe](https://partner.steamgames.com/doc/sdk/uploading).

## Stage 6 — Release And First Patches

Заморозить RC, сделать backup, финальный smoke/regression, нажать Release,
наблюдать crash reports/отзывы, выпускать только проверенные hotfixes и вести
короткие patch notes.

Gate: критические launch issues закрыты, стабильность подтверждена, backlog
разделён на patches и post-MVP.

## Stage 7 — Post-MVP Decision

По данным playtest/продаж решить отдельно: обучение, tiers инструментов,
автоматизация и логистика, roguelite/meta-прогрессия, новые карты/биомы,
Steam Deck/Linux/macOS. Каждое направление проходит новый readiness gate; оно
не считается обещанным контентом.

## Documentation At Each Stage

- Общая правда о продукте меняется только в `GAME_VISION.md` и `DECISIONS.md`.
- Для активной крупной механики создаётся один файл в `gamedesign/features/` по
  шаблону; мелкие правки отдельного документа не требуют.
- Техническая архитектура документируется после появления устойчивого контракта,
  а не до прототипа.
- Текущий статус и gate обновляются здесь; отдельные roadmap/QA-дубликаты не
  создаются.
- Балансные числа после реализации живут в ScriptableObjects/данных; документ
  объясняет правило и intent, но не дублирует каждое значение.

Первые feature specs создаются не заранее, а при входе задачи в работу, примерно
в таком порядке: player/camera, map/enemy route, resource gathering,
building/towers, waves/throne/run state, затем HUD/menus. Враги, башни и ресурсы
сначала описываются внутри соответствующей системы, а не отдельным файлом на
каждый предмет.
