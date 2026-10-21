# Feature: Исследовательская мастерская

## D113 current override — starter teleport blueprint

Pinned «Телепортационный круг» is always available in the workshop from start,
without merchant/bag ownership, so it cannot be sold, discarded or dropped.
Local explicit study consumes the one-time research action and records knowledge;
it unlocks construction, not an automatically built circle. The other four
purchased papers keep D102 consumption/trade/history. Workshop remains one level.


## D-102 — implemented blueprint trade and study, locally verified

Owner requests runtime blueprint purchase and a working research-workshop window.
The workshop is ALWAYS ONE LEVEL, with no upgrade; this supersedes the D075
workshop-stage proposal below. Owner accepted the first four documents: Longshot
construction access, wand forge recipe, forge target stageII and target stageIII.
No new tower/weapon type is introduced. Actual item prices are trial catalog data.

Prototype contract: a purchased unequippable document is an owned instance in the
normal bag, sold/bought for gold through current finite merchant trade. Studying
locally consumes one owned paper once and records current-world knowledge;
no study timer or second currency charge. ForgeIII study needs studied forgeII,
not an already upgraded physical forge. Studying never automatically constructs
or upgrades anything. Learned forge permissions replace generic scroll spending
in this opt-in Stage3 profile; pressing the forge upgrade action applies the
allowed next stage without consuming another document. D090 caps D/B/S remain.
Longshot construction and wand crafting check knowledge in rules, not only UI.
Already acquired weapons can still be equipped; research gates the forge recipe.

Retry preserves current paper ownership, trades and knowledge; full Reset clears
knowledge/papers and restores finite stock. Shared monotonic IDs and one item
mutation guard preserve atomic ownership/gold across equipment and documents.
No disk-save format is introduced. Teleportation stays a separate existing free
research. P/selected workshop opens inspection; studying requires the built
workshop's local interaction radius and live unpaused/nonterminal mode.

Main owns rules/data/binding/tests; Astra owns art imports/HUD/native visuals.
D101 shell/categories and the three-column study-screen are owner-approved.
The owner also approved the silver/teal checkmark for known duplicates and the
«Изучено» knowledge-history tab. Physical papers disappear on successful study;
history records stay even with no owned copy. A known duplicate in bag or trade
keeps its normal drawing and gains the shared checkmark and «Изучено» detail pill.
It can still be traded; repeat study cannot consume it or award another unlock.
Use «Открывает улучшение кузницы», never «Разрешает улучшить». Native grid is
4x3 fixed equal cells with14% paper inset, independent of ImageGen raster errors.
Source/approval: art/source/research_workshop_2026-10-20_generation.json.
Acceptance: buy→bag→local study→correct unlock; duplicate/absent/wrong-order/far/
paused/loss actions preserve items/gold; fixed UI cells and no blueprint ranks/LVL;
workshop has no upgrade; retry/reset and old equipment/teleport flow regressions pass.

Verification2026-10-20:83 distinct rules/PlayMode checks pass after updating old
fixtures to explicitly acquire research prerequisites. Mac development build
681976675bytes;18native frames at720/900/1080 receive Astra visual PASS. Actual
mouse shop open/buy200/sell100 and local E→selectForgeII→Study→consumed/history
pass. No attributable runtime console errors. Scene/Inspector: no scene change;
existing Stage3PlayerInterfaceBinding opt-in binds Resources/Stage3Blueprints and
Stage3BlueprintArt. Stage3BlueprintInstaller.Install creates catalog/imported art.
Windows, final economy, future document/drop/restock sources remain unverified/
unassigned; no persistence format introduced. Evidence:
art/source/research_blueprints_2026-10-20_verification.json.

## D-101 blueprint item — historical appearance approval, implemented under D102

Owner clarifies the document category: a medieval magical fantasy ENGINEERING
BLUEPRINT, distinct from a rolled spell scroll. First prepare two examples of
one common base appearance; future documents may use it for research/new towers/
weapons and upgrades of workshop/forge/other buildings. Possible magic-acquisition
scrolls remain a separate future idea.

Astra directs/reviews; main generates original ImageGen art/provenance. The owner
rejected both A tilted parchment and B unfolded3panel plan, including their shop
mockups. Their technical concept checks remain historical, not owner approval.
The following modern white-sheet/category mockup was also rejected. Latest owner
direction is EXACTLY the earlier aged ivory/amber paper colour/texture and plum
drafting style, made rectangular to fit an ordinary fixed cell, with no tilt or
unfolded wings. Owner approved the final rect_categories_shop_v3 with «апрув».

Use one common shell with TWO functional categories: opening new content (such
as tower/weapon or construction access) and upgrading existing content. Approved
marking: cyan spark seal for
opening; muted brass upward chevron for upgrade, at the same location. Context
shows tower/wand openings and forge-II/III upgrades in fixed neutral cells, with
no equipment-quality frames/labels. Diagrams are illustrative, not new unlock data.
Do NOT use equipment ranks F–SSS or independent item LVL1–100 for blueprints.
An upgrade document number denotes the TARGET BUILDING LEVEL (forge→II/III),
not a level/quality of the document. Owner's4–5/forgeIV examples explain this
meaning; they do not increase D090 forge's three current stages or create new
items. All cells/rows/metadata areas stay fixed: content fits its cell and never
shifts, resizes or squeezes lower items. Previous C/D and LVL10/5 labels are
rejected, not tuning proposals. Previous100gold is still only a display example.
The mockups add no stock/ownership/payment rules or approved unlock catalogue.

No runtime item/source/price/consumption/stock is implemented by these concepts.
D100 merchant still sells equipment only; D090 forge keeps current counter and
three stage caps. A later ready brief handles any migration from generic scroll
wording to actual blueprint instances, including source/usage and one-time payment.
The appearance gate is complete. Next prepare standalone runtime art and a ready
item/source/price/usage brief; no second approval of this same shell/category
treatment is required. Source/prompts/rights:
art/source/research_blueprints_2026-10-20_generation.json.

## D-075 target: purchased technologies and workshop upgrades

Новый целевой путь для типов башен: купить исследование у торговца в будущем
городе → изучить в мастерской → открыть строительство. Покупка свитка/чертежа,
его изучение и оплата готовой постройки — разные действия. Не создавать
бесплатную башню при открытии технологии. Число новых башен и городов пока
не назначено. Город, торговец и торговый интерфейс ещё не реализованы.

Historical D075 workshop-stage proposal is superseded by D102's permanent
single-level workshop. The following paragraph is historical:
Мастерская тоже получает собственные улучшения. Предлагается начать с
трёх ступеней здания: базовые, развитые и поздние технологии; конкретные
границы каталога ещё не выбраны. Повышение мастерской расширяет доступный
предел исследований, не изучает все технологии само. Мастерская/апгрейды
генераторов и кузница имеют разные функции.

Первичная постройка зданий стоит дерево/камень/металл. Улучшения мастерской,
кузницы и других зданий, а также исследования могут требовать золото и
ресурсы. Покупки у торговца — ещё одно применение золота с крипов.
Текущая бесплатная мастерская и бесплатная мгновенная телепортация ниже —
старый тестовый срез, не утверждённая экономика нового дерева.

Для автодобычи рекомендуется получать допуск/следующую ступень в существующей
мастерской, затем оплачивать ап конкретной лесопилки/каменоломни/рудника.
Покупка допуска к автодобыче у городского торговца D-071 сохраняется целью;
в тестовой среде она ещё открыта сразу. См. RESOURCE_PRODUCTION_BUILDINGS.md.

Клик по зданию открывает полосу над единым HUD по D-077: уровень/доступные
возможности, апгрейд и кнопка «Перейти к исследованиям». Кнопка открывает
полноэкранный P; покупка технологии не выполняется самим открытием окна.
Дальность и условия платного изучения нужно определить до реализации:
нынешний P только информирует, нынешнее изучение требует локального E.
Новая карточка не объявляет удалённое изучение уже работающим.

Ступень мастерской, приобретённые/изученные технологии, доступы и расходы
входят в checkpoint. Не допускать двойного списания или открытия будущей
технологии после отката. Вид/полноэкранные действия требуют ImageGen/Astra.
Ниже — реализованный D-063 без этого дерева/апгрейдов.

Status: first research/travel slice implemented, D-063, 2026-10-17; verification
recorded in production/WORKSTREAMS.md. The owner accepted the workshop appearance,
approved seal v4 for the game and clarified that research must create the base circle.

## Purpose And Accepted Direction

Рабочее название — **«Исследовательская мастерская»**. Это мирная постройка
для развития базы: исследования открывают новые возможности строительства.
Одно из направлений — доступ к базовому телепорту, связанному с найденным в
мире. Конкретные дополнительные башни, здания и улучшения ещё не выбраны.

Мастерская связывает вложение ресурсов в развитие с исследованием большого
мира и будущими вариантами обороны по D-062. Она не выдаёт боевых умений;
автоматический стартовый лук героя добавлен отдельно по D-064.

## Implemented First Slice

Первое исследование — **«Телепортация»**. До его завершения базового круга
нет в игровом мире; после завершения он появляется. Вторую точку игрок находит
в мире, добравшись до неё обычным путём. Связанный контракт —
`WORLD_TELEPORTS.md`.

Мастерской в начале нет. Открыть **B → «База»** в текущем интерфейсе, выбрать
**«Исследовательская мастерская»**, указать свободное место на зелёной земле
и подтвердить ЛКМ. Esc или ПКМ отменяет размещение. Игрок подтвердил:
**строительство пока бесплатное**. В текущем срезе нужна одна мастерская;
после постройки её карточка показывает «Построена» и повторное размещение закрыто.
Окно **P** показывает состояние исследования; покупка и удалённое изучение
через него не вводятся. D-066/D-068: `PLAYER_INTERFACE.md`.
Затем подойти к её передней стороне и нажать E — выполнить однократное исследование.
Подсказка находится в существующей строке действий HUD. Исследование сейчас
мгновенное и бесплатное; это первый срез, не утверждённый баланс.
Оно доступно в подготовке и во время волны, заблокировано при паузе, финале
забега, размещении постройки и открытом отладочном меню. Удержание E не выдаёт
результат повторно и не переносится через сброс/выход из блокирующего режима.

Для последующих обсуждений направления — новые мирные постройки, новые башни
и другие возможности развития. Это группы идей, а не утверждённое дерево
технологий или обязательные вкладки интерфейса. Открытие возможности постройки
не должно молча означать бесплатное создание готового здания.

Размещение использует клеточную привязку, одинаковый спрайт/опору в preview
и готовом здании. Предпросмотр показывает цвет доступности и границы площадки.
Нельзя занять дорогу, границу карты, героя, ресурсный район/его площадку,
другую коллизию, ручной источник, трон, вход врагов или место любого круга
(включая скрытый базовый). Перед мастерской остаётся доступная точка исследования.
Выбор башни/генератора, смена категории, пауза, отладочное меню и конец забега
отменяют размещение. Клик по UI/миникарте не строит на земле под интерфейсом.
При одновременных отмене и подтверждении выигрывает отмена.

Стоимость/длительность исследования и дополнительные открытия остаются
для следующего отдельного решения.
Новые валюты, расходуемые чертежи, tiers и очередь исследований не введены.

## Approved Appearance

Один компактный деревянно-каменный дом с терракотовой кровлей, открытой книгой,
чертежами и измерительным прибором. Он должен читаться как место исследований
при обычном масштабе и сочетаться с текущими ресурсными зданиями.

Первый imagegen-концепт: `../../art/concepts/research_and_travel_2026-10-17/research_workshop_v1.png`.
Игрок одобрил этот вид: «Исследовательская мастерская пойдет». Концепт сохранён
без перегенерации. Оригинальные PNG-байты импортированы в
`Assets/Stage3/WorldTravel/ResearchWorkshop.png`; видимый нижний край спрайта
совмещён с передней стороной коллизии, ширина/место задаются WorldTravelData.asset.
Полный промпт,
источник, права и хеш — `../../art/source/research_and_travel_2026-10-17_generation.json`.

## Integration And Observable Acceptance

- До собственной постройки нет мастерской, её коллизии/маркера и исследования.
  Строительство не завершает исследование автоматически и не меняет инвентарь.
- Завершённое исследование один раз создаёт видимый базовый круг; без него
  круг отсутствует. Исследование не списывает и не выдаёт ресурсы в этом срезе.
- Открытия относятся к текущему миру. Полный ResetRun и регенерация мира
  удаляют мастерскую/её коллизию и очищают исследование/обнаружение. D-062
  сохраняет её положение, исследование и обнаружение непосредственно перед
  волной; повтор восстанавливает их вместе с коллизией/кругами без новой оплаты
  и убирает изменения неудачной попытки. Контракт — STAGE2_RUN_STATE.md.
- Исследования, генераторы и башни имеют разные функции; текущие три генератора
  сохраняют принятые роли и данные до отдельного runtime-изменения.
- Привязка: Stage3TacticalPresentation.worldTravelData в Stage3Rapid3DGamePilot;
  данные/материалы находятся в Assets/Stage3/WorldTravel/. Команда восстановления
  привязки: Tools > Stage 3 > World Travel > Build and Wire, только вне Play.
- Проверки покрывают E, однократность, отсутствие побочных эффектов в инвентаре,
  мышь/preview, границы, занятые места, блокирующие режимы, полный сброс,
  безопасные места и регенерацию мира.
