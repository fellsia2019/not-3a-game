# D-107 current upperHUD override — approved2026-10-20

D113 current override: no offscreen city pointer on minimap. Only a local city
icon, while M remains a known city/hero icon without captions. Analog clock is
120px at900p with a brighter original hand and unchanged10:3 timing. Starter
teleport record is clearly labelled in workshop; study opens a Base build card,
using existing approved artwork and UI stack.


D112: city landmark uses existing painted house/ivory-plum marker, no «Город»/«Вы здесь» captions on M after owner refinement and a rim direction on local minimap when outside its crop. First M zoom fits the entire map; reopening retains player zoom/pan. Reuses existing gesture/input stack; no map-click travel. Main marker lifecycle, Astra native presentation. Mac qualification:29 distinct latest PlayMode PASS,28 native frames at720/900/1080 and ordinary M input; Windows unverified. Evidence: art/source/city_map_2026-10-21_verification.json.


D111 latest owner correction: the small world notice board is decoration only,
placed substantially farther right of the guild door with a free approach,
smaller and muted to avoid attracting attention. Click the guild BUILDING
to open the contract-board window; local actions use GuildInteractionPosition.


D111 implementation update,2026-10-21: approved Guild board native window opens from outdoor city board. Physical rank seals F…S, personal badge and three successes, pinned order papers/location/reward, detail and contextual accept/in-progress/claim action. Two visible papers scroll for further orders/trial; preview never accepts. Actions require nearby board; no new hotkey. Both city guild and open temple portal court retained. Astra owns native visuals; main binds guild state. Local qualification in art/source/guild_2026-10-21_verification.json.


Replaces floating resource/HH:MM corner cards with shaped upperedgeHUD: compact
resourcesleft, daycount/developmentF3right, small whole analogdial centered.
Thin cornice and tapered/concave bridges leave world visible between groups.
Owner approved v2silhouette and A smoke-glass v7. Abstractgreyolive/ivory day,
nearblack/darkbloodredmiasma night, broadsoftfade, no piechart divider/pictures.
Normal-time day10min/night3:night3/13circle≈83°, day277°, dawnat3o'clock,
continuous clockwisehand. Main supplies Run.DayNightCycleProgress and phase
through readonlyHUDdelegates; Astra draws native assets in existingUIstack.
Pause/sleep/debuglaunch/retry/reset follow world clock, no separate HUDtimer.
Existing fullscreen navigation/trade-onlygold/mousehitregions must remain.
Qualified2026-10-21:36EditMode+75sharedPlayMode PASS,25nativephase/UIframes
AstraPASS; actual ordinary F3 keyboard toggles. Windows/mouse clicks not claimed.
Sourceart/source/cycle_top_hud_2026-10-20_generation.json; evidence
art/source/cycle_top_hud_2026-10-21_verification.json.

# Feature: Player Interface

## D-110: physical guild board — active appearance concepts

Owner replaces the temple/elder quest UI with a fullscreen physical contract
board: dark wood/brass, pinned paper orders, location/reward, personal guild
badge and engraved rank-board filters F/E/D/C/B/A/S. Current rank F plus three
successful same-rank orders opens ONE next-rank trial; new rank/full board only
after passing it. This page is deliberately material/world-like, replacing the
rejected plain-form proposal. Existing other fullscreen interfaces remain current.
OBJECTIVES.md owns accepted rules and open gameplay details. Main generates,
Astra directs/reviews the appearance; no native UI or quest binding added yet.

## D-109: temple NPC quest window — superseded by D110

Clicking the temple mage is intended to open a fullscreen quest-selection window
matching the current Lunar Porcelain style. V1 proposes equal orc/ogre quest
cards left, selected objective/place/action right and compact mage portrait in
the header. Familiar navigation/Esc, no new quest shortcut, wallet, rewards or
numeric targets in this sketch. The NPC and its courtyard placement are already
approved. OBJECTIVES.md owns the active concept/remaining gameplay brief;
art/source/temple_quests_2026-10-21_generation.json records source and review.
No native UI or quest input binding is added yet.

## D-103: live vitals and Blink facts

Existing approved HP/MP tracks now show actual numeric current/maximum values
and fractions from HeroProgression: initially100/100 each. HP recovers1/s and
MP5/s during active simulation. The inactive0/0 fallback remains only without
a progression source. Existing ruby/cyan artwork, geometry and XP track remain.
Blink rank rows show actual mana/cooldown; the current HUD tooltip reflects
effective0MP/0s during Debug WTF. Cooldown ring uses the duration of the actual
cast, even if the rank changes. Astra owns these presentation changes.
Verification: art/source/hero_vitals_2026-10-20_verification.json.

## D-100: fullscreen merchant — implemented and locally qualified

Clicking the city shop opens the owner-approved dual-inventory Lunar Porcelain
layout: player's bag left, merchant stock right, compact portraits and a central
selected-item panel with stats, price and one contextual Buy/Sell action.
Six columns and independent scrolling on both sides retain large grids; use
current D097 actual painted skins and shared visible-silhouette safe fitting,
with cyan selection separate from quality. Existing global tabs/Esc remain;
the trade wallet shows gold only, hiding wood/stone/metal. Footer returns to city.

UI only sends selected side/instance ID; binding supplies both current views,
prices, balance and action availability. Equipped items never appear in sale bag.
No selected/empty/unfunded/far state must trigger a transaction. Failures explain
why the action is disabled. Navigation/close consumes input; no remote shop button
or automatic pause. World threats/time continue while modal blocks hero/build
input. Actual local transaction/lifetime rules belong to CITY_AND_TRADE.

No new PNG required: current item atlases/painted skins, existing hero portrait,
city merchant head crop and painted scales badge supply the appearance. Native
review PASS on21frames at720/900/1080 covers buy/sell/unfunded/far/empty/scroll
and gold-only wallet. Round masks and pixel-safe five-row viewport repair the
initial portrait/scroll defects.41/41 focused PlayMode and actual mouse buy/sell
pass; Windows unverified. Evidence: art/source/merchant_2026-10-20_verification.json.

## D-099: shared teleport destination menu — implemented and locally qualified

Clicking an available nearby travel circle opens the same menu at base, city
and future opened world points. Existing E opens this same menu, replacing the
instant counterpart hop. Opening/closing never travels by itself.
Travel gates, discovery, free same-world movement and arrival validation belong
to WORLD_TELEPORTS.md; enemy-invasion gate controls remain separate.

The approved ImageGen reference uses a centred Lunar Porcelain panel over subdued
world context, plum header «Телепорты»/«Esc · Закрыть», no gameplay HUD/resource
bar/global tabs. Two opened rows illustrate the initial scope: selected «База»
has cyan selection, current «Город» has «Вы здесь». Right: selected point name,
small arrival preview, route «Город → База», one «Телепортироваться» action.
There are no invented locked towns, fees/currency/cooldown or extra controls.
Further opened points extend the same list; there is no separate city-only menu.
Selecting the current point disables travel with «Вы уже здесь». Revalidate
eligibility on confirmation; consume the source click/E and require fresh input
after arrival. No hidden map click or immediate second transition.

Source: art/concepts/teleport_menu_2026-10-20/teleport_menu_v1.png; exact prompt,
references, rights and Astra concept PASS:
art/source/teleport_menu_2026-10-20_generation.json. Owner approved both appearances and requested implementation. Astra built native
UI from existing Lunar Porcelain assets; main binds click/E to the same opened
registry and revalidates confirmation. Five ImageGen runtime sources provide
modular city sprites/paving and measured arrival illustrations; no concept
screenshot is shipped as an interactive window. Normal gameplay remains live
while modal UI blocks hero/build input. Native menu/network qualification passed:
16/16 focused PlayMode,16 city/menu frames at720/900/1080p, actual mouse/E travel,
Esc cancellation and current-location disabled action. Previews fit proportionally
inside thin frames; current-location reasons stay inside the footer. Evidence:
art/source/city_runtime_2026-10-20_verification.json; Windows unverified.

## D-094: throne reinforcement uses the existing selected card

The selected throne shows «УсиленияN/3», one current/max HP stat, live next price
and HP result preserving existing damage (3/5→8/10). The existing Upgrade button
pays once; after3upgrades it shows «Максимальное усиление» with no next price.
Pause/modal/loss or missing materials/gold block the action. This uses the D078
card above both regular and construction docks; no additional control/HP bar or
cupola art change. Rules/retry: OBJECTIVES.md and STAGE2_RUN_STATE.md.

## D-093: generator upgrades reuse the existing selected card

No new window or visual layout: existing D078 card shows each generator's real
level1–5, output per cycle, original interval and next output/whole resource+gold
price. Its existing Upgrade action invokes the owned-instance transaction.
At maximum the button is disabled with «Максимальный уровень» and no price;
unfunded state explains «Недостаточно ресурсов». Card works above both regular
and construction HUD. No generator sell/move/merge/Research action is added.
World building art/footprint and tower-only LVL badges remain unchanged.
Native appearance is reviewed by Astra using existing approved assets.

## D-092: gathering professions inside fullscreen development

Approved miner_v3/logger_v2 are implemented by Astra in native O inner tabs:
«Боевые навыки» retains existing Blink/passives; «Профессии» shows independent
miner/logger mastery/XP/points, three ranked talents, current→next actual harvest
facts and the equipped tool. One action spends that profession's point. Tool
quality/level never change from talent learning; forging stays in its service.
HP/MP, main HUD, armor and global navigation remain unchanged. Data/upgrade
validation belongs to gameplay binding, not OnGUI. No screenshot background
is used: only eight owned ImageGen medallions are imported. See HERO_PROGRESSION
and Stage3Professions data for rules/trial tuning; native QA is recorded in handoff.

## D-087/D-088: daytime gates, time-only retry and dwelling Sleep

Existing local interaction hint shows E at the invasion gate: open/close the
last cleared threat tier, or explain first-victory/night lock. Gate control is
local and respects modal/pause/debug/build-placement input blocks.
The existing selected-house strip has a «Сон» action in its existing action
slot, with the existing dwelling icon. No new window, layout or artwork.
It uses the same dwelling rule as local E: near the entrance, outside any fight,
advance to19:50. Its live disabled reason includes active enemies after closing
the daytime gate. The defeat screen says progress is kept and a new preparation
day begins; active Stage3 no longer promises to restore a full world snapshot.
See PORTAL_CONTROL.md for rules and migration/qualification evidence.

## D-085: first hero-progression concepts approved, implementation active

The owner activates XP/levels up to100, cursor Blink and first passives, with
ImageGen interface/effect approval first. O proposal replaces the unavailable
body with hero/XP/points, four Blink ranks and three personal passive cards.
The v4 HUD retains its exact dock/ribbon/ten-cell capacity, adding a small LVL
tag, one Blink slot/cooldown and live numeric orange XP. HP/MP stay inactive0/0.
See HERO_PROGRESSION.md for the proposed point budget and three-passive scope.
No hotkey, extra active spell or ten-equipped-skill contract is set by these
images. Final concepts/prompts: art/source/hero_progression_2026-10-19_generation.json.
D-085 appearance and the three passives are approved and being integrated.
The revised level-up appearance D-086 polygon_v3 is now also explicitly approved. Q casts Blink
at the world cursor; slot click arms then world LMB casts, RMB/Esc cancels.

## D-080: construction and finished-building selection correction

One click on a defense/production/base card arms the blueprint; no separate
Select button. The existing control hint occupies its space. Construction stays
nonmodal and open when choosing a finished owned building. The selected strip
remains above the construction dock, with working card actions/world drag.

A built object wins its mouse press over any armed tower/production/workshop/home
draft: cancel placement before the placement drivers can buy beneath the sprite.
The card appears on click release (or once a drag threshold is crossed), so it
cannot catch the initiating world mouse-down as an Upgrade/Sell press. Empty-
ground placement and UI click blocking keep their previous rules. Modal pages/
pause/loss/reset still cancel selection/drag. D-080 fixes the previously untested
normal path in D-079. Press origin is retained from Input System events even
when down/motion/up arrive in a single rendered frame. The active binding
temporarily disables redundant mouse-event merging because FastMouse can merge
a press into subsequent held motion before onEvent sees it; unbind/disable
restores the original runtime setting (no serialized settings change).
Modal/disabled lifecycle drops the transient input. The batched gesture regression and native player flow pass:24 unique PlayMode,
final4 input cases; actual catalogue click→select→move→merge→upgrade→sale with
B open. Mac build and46 frames at720p/900p/1080p; Windows remains unverified.
Evidence: art/source/construction_selection_fix_2026-10-19_verification.json.

## D-083: direct prototype startup

The owner defers a main menu and interactive loading flow. Launch the existing
prototype directly, with no extra Play click. The first game-camera frame must
already have final approved hero/landmarks/terrain and the bound current HUD;
legacy markers/debug ground/HUD/minimap must not flash. TacticalPresentation
Start(-500) explicitly initializes existing rules, then synchronously builds
its presentation before HUD binding(-300). No hidden wait/menu or global loader.
Runtime procedural resources remain part of the same preparation. Ordinary
Editor Build Settings now lists current Stage3 first, preserving older fixtures.

## D-079: tower/barricade actions

D-082 revises the world tag to compact reflective-silver LVL N (48×17 UI units,
font11, gap4) and replaces the code arrow with the owner-approved original
ImageGen silver/cyan chevron inside the existing upgrade medallion. Same layout,
same live level/action states. Provenance: art/source/upgrade_icon_2026-10-19_generation.json.

Sustained walking qualification found avoidable HUD allocations/draw calls.
Reuse slice coordinates/text measurement, cache unchanged displayed values, and
use a bounded resize-aware bevel-texture cache released on destruction. These
preserve v4 appearance/input and do not add a UI stack or loading system.

The approved v4 card now shows real level, next-level HP/damage, full resource
price and exact half-investment refund. Upgrade/sale are live for towers and
barricades; unaffordable/max-level actions explain their disabled state. Prices
update as inventory changes. Upgrade/merge preserve battle state; sale clears
selection immediately. No Move button or layout redesign.

World drag retains the original tower until release. D-081 uses light blue for
a compatible merge, green for a valid empty-cell move and red for an invalid
drop. Small silver LVL tags (D-082) show the current level above built towers without
adding hit targets or tier classes. Visible tower silhouettes are valid
recipient targets above their ground cells. Equal type + level merge, free empty
cell moves during preparation, and cancellation/lifecycle are defined by
TOWER_UPGRADES.md. UI presses cannot initiate world drag; drag blocks movement,
gathering and camera zoom. D-062 retry clears the transient gesture.
Other building upgrade buttons stay unavailable until their own gameplay slice.
This section supersedes the historical D-078 disabled-action statements below.

## D-078: implemented HUD and owned-building selection

2026-10-19: актуальная правка владельца к D-077. Вертикальный порядок сверху
вниз: **полоса выбранного здания → узкая полноширинная навигационная лента
I/B/P/O/M → основной HUD**. Общая подложка непрерывна, между уровнями нет
просветов до мира. Без выбора здания остаются лента и основной HUD.

Центр основной панели отдан способностям, навигация туда не входит. Эскиз
показывает резерв до десяти ячеек двумя рядами по пять: владелец допускает
такое расширение, но число одновременно экипированных умений/новые клавиши
этим не утверждены. Остальные группы — миникарта, герой/оружие и статусы.

Подписи шкал по уточнению владельца во время внедрения: **HP, MP, XP**.
Сохранить пропорции и орнаментированные состыкованные рамки v4, крупные
медальоны и фигурные полосы. Упрощённый первый runtime возвращён на доводку.

Здоровье, мана и опыт показывают **текущее / максимум числами** вместе с
приятным ярким заполнением: здоровье рубиново-красное, мана голубая, опыт
тёплый оранжевый. Числа стоят на постоянном месте и читаются на заполненной
и пустой частях. На эскизах 80/100, 45/60 и 350/1000 — только подписанные
снаружи примеры; механик HP/маны/XP сейчас нет, баланс не назначен. Будущий
интерфейс получает реальные значения из игровых данных, не из этих картинок.

- [Обычный HUD v4](../../art/concepts/building_selection_2026-10-19/hud_ribbon_normal_v4.png).
- [Выбранная башня v4](../../art/concepts/building_selection_2026-10-19/hud_ribbon_selected_v4.png).

Два кадра созданы встроенным ImageGen. Astra проверила порядок, десять
резервов, числа/цвета, непрерывность и отсутствие Move: блокирующих проблем
нет. **Владелец утвердил v4: «Утверждаю v4» (2026-10-19)**. Astra внедрила
внешний вид и довела его после замечания о больших отличиях. Используются
родные ассеты; случайные генеративные изменения не перенесены в игру.
Промпты/входы/хеши/проверка: ../../art/source/hud_ribbon_2026-10-19_generation.json.
Целевые действия карточек D-077/D-075 сохраняются; их реализация разделена ниже.

### Current runtime and verification, 2026-10-19

Основной HUD, отдельная лента и выбранная полоса состыкованы, с родными
рамками, увеличенными круглыми элементами и фигурными цветными шкалами.
HP/MP/XP показывают **0 / 0** и общую подпись «Не открыто»: это отключённое
числовое состояние интерфейса, не реальные максимумы героя. Десять ячеек
пусты; новые умения, клавиши и лимит экипировки не добавлены.

Свежий клик по готовой своей башне/частоколу, генератору, мастерской, жилищу
или трону выбирает объект и показывает его реальные текущие сведения.
Есть родная миниатюра и бирюзовые углы вокруг модели. Параметры башни,
включая убывающий HP, читаются из живого объекта; выпуск и интервал — из
production entry; здоровье трона — из run. Уровней зданий пока нет: подпись
В D-078 «Базовая постройка» обозначала отсутствие рангов; D-079 показывает реальный уровень башни/частокола.

Кнопка мастерской открывает полноэкранные исследования; покупка остаётся
локальным E. Платный ап и продажа **отключены с причиной**, цены/возврат не
выдуманы. Move-кнопки нет; игровое перетаскивание/слияние ещё не добавлено.
Хижина показывает своё существующее E, не меняет пока старый цикл D-065.

Клик завершается при отпускании; drag или нажатие, начатое на UI, не выбирает
здание за панелью. Весь док/лента/карточка блокируют world pointer. Свободный
мир, Esc/ПКМ или × снимают выбор. Открытие окна/стройки, пауза, F3, поражение,
сброс, rewind, регенерация мира и удаление цели очищают временный выбор.
Карточка немодальна и не останавливает ход времени или управление героем.

Проверено: 25/25 PlayMode; реальный Mac/Metal player, 27 кадров в трёх
разрешениях 1280×720/1600×900/1920×1080. × и «Исследования» проверены обычными
нативными кликами. Последняя правка после тестов выровняла только baseline
подписей production, затем сборка/все три набора кадров повторены. Windows
не проверен: на хосте нет WindowsStandaloneSupport. Новых относимых ошибок
в финальных player-сценариях нет; сцена/Inspector/сохранённые data не менялись.
Astra приняла финальные реальные кадры: блокирующих визуальных дефектов нет.

- [Обычный игровой HUD](../../art/review/hud_v4_game_2026-10-19.png).
- [Карточка башни в игре](../../art/review/building_card_v4_game_2026-10-19.png).
- [Проверки, исходные файлы и хеши](../../art/source/hud_v4_2026-10-19_verification.json).


## D-077: historical unified HUD, revised by D-078

2026-10-19: владелец переработал бриф. Миникарта, герой/оружие, I/B/P/O/M,
пять нынешних резервов способностей и статусы находятся в **одном сплошном
нижнем HUD** с общей подложкой/внешней рамкой. Между группами нет разрывов
до мира; внутренние разделители допустимы. Навигация больше не плавает
отдельно над панелью. При выборе здания непосредственно сверху появляется
длинная полоса той же ширины, плотно состыкованная с основным HUD.

- [Единый HUD, обычное состояние](../../art/concepts/building_selection_2026-10-19/hud_unified_normal_v3.png).
- [Тот же HUD с выбранной башней](../../art/concepts/building_selection_2026-10-19/hud_unified_selected_v3.png).

Кнопка **«Переместить» убрана по запросу владельца**. Перемещение начинается
перетаскиванием здания в мире; drop на допустимое свободное место перемещает,
на совместимую башню того же уровня — сливает, недопустимый drop возвращает
исходное состояние. Условия совместимости/фазы/цена и атомарность —
TOWER_UPGRADES.md; сам эскиз не реализует эти механики.

Полоса Rapid содержит имя/уровень, реальные урон/дальность/интервал/HP,
«Улучшить» и «Продать», с прочерком будущей цены и возвратом 50%. Герой
сохраняет текущие параметры лука, HP/мана/XP остаются прочерками. Основные
параметры и пятислотовый резерв не меняются только ради заполнения ширины.

Две версии v3 созданы встроенным ImageGen по брифу Astra. Astra проверила
читаемость/параметры/общий фон и отсутствие Move: подходят для согласования,
блокирующих дефектов нет. Выбранное состояние получилось заметно высоким;
владелец ещё оценивает обзор мира. **Финальная внешность v3 не утверждена**.
Промпты/входы/хеши/обзор: ../../art/source/unified_hud_2026-10-19_generation.json.
Код/сцена/игровые данные не менялись; оригинальные ассеты остаются источником
для будущей реализации, а не пиксели генеративного макета.

## D-076: historical compact-card candidates, superseded by D-077

2026-10-19: владелец выбрал следующий срез — выбор готовых зданий и карточка
над HUD; сначала ImageGen-эскизы, затем внедрение через Astra после одобрения.
Две альтернативы и общий лист содержания созданы встроенным ImageGen:

- [А: горизонтальная карточка](../../art/concepts/building_selection_2026-10-19/card_A_horizontal_v2.png).
- [Б: карточка справа](../../art/concepts/building_selection_2026-10-19/card_B_right_v2.png).
- [Содержание и состояния](../../art/concepts/building_selection_2026-10-19/building_cards_states_v2.png).

А — низкая карточка слева над навигацией; Б — более высокая справа. Оба
используют текущий «Лунный фарфор» и оставляют миникарту/I/B/P/O/M доступными.
Лист показывает башню, мастерскую/переход в исследования, будущую кузницу,
генератор, трон и недоступный ап с причиной. Цены будущих операций не заданы.
Числа башни/лесопилки/исходного трона взяты из текущих data assets; уровни
и новые действия — иллюстрация будущего поведения, не уже работающий UI.

Это генеративные макеты, а не runtime-кадры. При внедрении переиспользовать
исходные HUD/спрайты/шрифты, не импортировать полноэкранную картинку или
переносить случайный дрейф старых элементов/надписей. Выбор варианта и
утверждение прежней внешности не получены: владелец запросил D-077 выше.
Полные промпты, ссылки
на входы и хеши: ../../art/source/building_selection_2026-10-19_generation.json.
Unity-код, игровые цены и механики апа этим шагом не менялись.
GPT-6 Astra проверила финальные файлы: оба пригодны для согласования,
рекомендован А. На листе «Усилить +5 HP» означает **+5 максимального HP**,
не лечение; при реализации подписать это явно. Drag/слияние, реальные цены
и предпросмотр следующего уровня этим листом не проверены.

## D-075 target: shared-card actions, partially implemented by D-078

Клик по своему готовому зданию открывает **одну полосу над общим HUD**.
Это принятое владельцем дополнение к полноэкранным I/P/O/M, а не отдельное
полноэкранное строительство. Карточка немодальна; мир не останавливается.
В ней имя/реальный уровень, актуальные параметры, поддерживаемые действия
и переход к соответствующему подробному разделу.

| Объект | Данные и действия |
| --- | --- |
| Башня | Урон, дальность, темп, HP, уровень; платный ап, продажа 50%; перемещение/слияние перетаскиванием |
| Исследовательская мастерская | Уровень и предел доступных технологий; улучшить, «Перейти к исследованиям» |
| Кузница | Уровень и максимум качества; улучшить, «Открыть ковку» |
| Лесопилка/каменоломня/рудник | Ресурс, выпуск за цикл, интервал, уровень; улучшить, перейти к связанной технологии |
| Трон | Текущий/максимальный HP, число апов 0/3–3/3; улучшить максимум на 5 |
| Хижина | Только определённые сведения; функция после отмены сна/апгрейд пока не выбраны |

Не добавлять фиктивную скорость исследования, очереди или бонус хижины ради
заполнения карточки. Цена и изменение апа, точный возврат продажи видны до
платежа. Первичное строительство стоит ресурсы; апы/исследования могут включать
золото. Операции башен — TOWER_UPGRADES.md, кузница — EQUIPMENT_AND_RUNES.md.

«Исследования» открывают существующую полноэкранную P. Выделение конкретной
технологии предлагается. Для кузницы Astra предлагает контекстный полноэкранный
экран ковки в общей оболочке, без новой постоянной вкладки/горячей клавиши;
его содержимое/внешность ещё не утверждены. Переход для другого здания
появляется вместе с соответствующим работающим разделом, не ведёт в пустоту.
Открытие страницы не выполняет покупку и не разрешает автоматически удалённое
изучение: текущий локальный E/информирующий P меняются отдельным контрактом.

Клик выбора отличается от drag здания; один drag обслуживает перемещение
и совместимое слияние, отдельной Move-кнопки нет. Один жест не двигает
героя/камеру и не подтверждает стройку под
карточкой. В полноэкранном разделе карточка скрыта. Пауза/поражение/сброс
и открытие страницы очищают незавершённые действия; нужны свежий клик/E.
Удалённый объект не оставляет старый выбор и повторную оплату.

Историческое предложение Astra до D-077: горизонтальная карточка справа от
миникарты, выше HUD и ряда I/B/P/O/M; миниатюра/имя слева, параметры в центре,
действия справа. Сохранить «Лунный фарфор», читаемую кириллицу и пропорции
орнамента. Это ещё не одобренный ImageGen-эскиз. Проверить 1280×720,
1600×900 и 1920×1080: карточка/действия читаются, навигация/миникарта доступны,
поле боя не перекрыто большой панелью. Этот компактный layout заменён D-077;
актуальны два v4-концепта D-078 выше, runtime пока не изменён.

## Historical D-069 full-screen baseline

Status: D-069 full-screen revision implemented; local Mac verification, 2026-10-18.
D-078 above supersedes its bottom-HUD geometry and status placeholders.
D-066/D-068 runtime correction below remains the historical verified baseline. The owner
chose C «Лунный фарфор», authorized implementation of the window sheets and
explicitly approved both revised fullwidth normal/construction HUD sketches.
Owner: GPT-6 Astra owns visual decisions, layouts and subsequent visual
implementation; the primary agent performs ImageGen, integration and records.
Dependencies: existing scene-owned HUD/input protection, resource wallet,
construction catalogs, research/workshop, hero equipment and world map.

## Accepted direction

D-069 supersedes the old centered windows: Inventory I, Research P, Hero
development O and Map M use one opaque edge-to-edge full-screen shell. Its
four-tab header, compact resource/day-clock strip and footer remain visible
across pages; world HUD/minimap/development access do not draw through it.
Inventory V2 supplies the common lunar-porcelain composition: resource stock,
large existing hero/equipment and current bow values. Research keeps the one
Teleportation entry and a passive local-E instruction; it offers no remote
research action. Development remains unavailable. Map uses almost the entire
content region for actual map data and retains anchored pan/zoom.

Construction B is explicitly excluded from full-screen navigation: the owner
kept the existing ribbon and placement in the world. HUD access has five compact
buttons showing only the existing bag/hammer/book/hero/map icon plus I/B/P/O/M;
there are no visible word labels on those HUD buttons. Full-screen tabs retain
their readable names. Existing key toggle, switch, close, lifecycle, input
blocking and continuing-simulation behavior remain; automatic pause is not added.
Approved previews and source rights/prompts:
art/concepts/fullscreen_interface_2026-10-18/ and
art/source/fullscreen_interface_2026-10-18_generation.json.
Current verification belongs to the D-069 handoff, not the older counts below.

Four full-screen pages serve inventory/equipment, research, hero development
and the large map. The HUD also exposes the separate in-world construction
mode. Visible access icons complement shortcuts I, B, P, O and M. The same
key toggles its page/mode, another key switches, and Esc closes. Construction
is a nonmodal ribbon; the four pages block player movement/world input without
pausing the simulation. Automatic combat and production keep their existing
rules. Closing a window requires fresh E before a held local action can resume.

The lower HUD starts immediately to the right of the local minimap and reaches
the right screen edge. A narrow hero/weapon block has vertically arranged
current weapon stats. Future ability slots occupy the middle; the right holds
red health, blue mana and orange experience reserves. Unavailable values show
«—», with no invented stats or abilities. Window access buttons sit above it.
Resource icons/counts
belong in the upper left. Day number and an ordinary in-world HH:MM clock
belong in the upper right. Remove the night countdown and throne health from
the HUD. The owner's intended throne health display is over the throne;
that world-space indicator is not part of this interface slice.

B replaces the hero/ability/status contents with categories Defense/Production/
Base, actual building cards and a right-side selected-building detail. Selecting
a card starts ordinary world placement without closing the ribbon or opening
a modal window. Opening/switching/closing panels cancels stale placements.
Hovering/clicking the ribbon never pays or builds behind it; confirmation needs
a fresh world click. Built workshop and home retain their one-instance rules.

Keep useful local interaction hints, including E at the dwelling and workshop.
The dwelling rest behavior now follows D-088 above.

The owner first asked to preserve the prototype minimap, then explicitly
allowed complete redesign of both map and minimap. A future large world uses
a local minimap and an M window with panning/zooming over a regional viewport;
the entire eventual world need not fit on screen at once. No new geography,
fog-of-war, discovery or travel rules are authorized by these drawings.

## Defeat and retry — D-062

Actual throne HP loss produces a short render-only shake. If the dome is visible
and no opaque full-screen page covers it, transient white radial fractures
originate at its soul; they fade completely. Damage away from the dome uses
shake only. Greater accumulated damage strengthens the next pulse. Fatal
fractures/whiteout cover every page and hide ordinary HUD, independent of camera
position; simulation and world input stop, while the unscaled transition finishes.
The lunar retry screen reads «Ещё одна попытка», «Вернитесь к подготовке и
измените исход», «Повторить волну». Button and R become actionable only after
the transition. D-087 now keeps current world/hero progress, restores the throne,
clears failed threats and returns morning with preparation for the same pending
night tier. Pages/effects/placements clear and E requires release. The old full
checkpoint remains only in legacy Stage2. See `PORTAL_CONTROL.md`.

## Appearance and prototype content

D-068 corrects the implementation after the owner rejected actual runtime
proportions and readability. Use Cyrillic sans-serif typography, preserve
ornament/icon aspect ratios, keep both screen and internal content safe areas,
and show hover/pressed/selected/disabled feedback on clickable controls.
Stretch only plain frame connectors; jewels/corners remain proportional.
Icons and text belong inside the ornament-safe content bounds.

The hero needs a distinct map marker and a readable local label. Wheel zoom
keeps the world point under the cursor; pinch zoom keeps the point between
touches and follows midpoint movement. World bounds may constrain the anchor
at map edges. Button +/- zoom remains centered. Neither action changes the
world camera. One-finger map dragging pans the same regional viewport.

The original restrained charcoal panels were explicitly rejected. The owner
selected C from three newly generated fantasy directions: pearly ceramic,
muted plum enamel, silver contours, turquoise accents and curved crescent/petal
silhouettes. This is an original material theme, not a hero class or magic rule.
Keep material richness and readable text while reducing ornament at small sizes.

The owner rejected the empty right side, then approved the fullwidth revision
with compact vertical bow stats, ability reserves and the three colored tracks.
Current bow values come from HeroCombat.Catalog. Hero HP, mana, XP, levels and
death are not current systems; the visible reserves do not invent their mechanics.

Inventory shows the actual shared resource wallet and current hero/bow. Item
storage and slot behavior remain unavailable. Hero development shows the
existing hero and an unavailable state; it introduces no skill tree, cost model,
progression currency or ability list.

Construction illustrates the current Rapid, Longshot and passive palisade,
with prices from project data, plus category navigation. Research illustrates
the existing Teleportation entry and local workshop E access. Additional
technologies, research time/cost, queues and remote research are not introduced.
Use the existing approved world art in implementation; thumbnail variations in
generated UI sketches do not authorize redesigning buildings or teleport seals.

Clock rules are recorded in D-067 and DAY_NIGHT.md: ordinary preparation maps
08:00–20:00 over twenty active minutes. Night time advances at the same rate.
Early clearance advances to dawn; an ongoing horde can pass 08:00 into daylight.
After a daytime clearance the clock does not move backward, and the full
preparation period still starts. Overlapping hordes and day skip during an
ongoing horde are prohibited. Pause freezes both time and the existing simulation.

## Review and implementation gate

ImageGen created concepts, the owner approved the concrete revised layouts,
and Astra implemented their presentation. The existing IMGUI stack is retained.
No package migration, inventory/progression models or new currencies are added.

Stage3FantasyHud and its page partials own drawing, layout and page/map
navigation. Stage3PlayerInterfaceBinding connects optional input blockers and
legacy purchase/local-E callbacks; legacy scenes retain their original HUD.
Stage2Minimap keeps its cached terrain/markers while its old drawing is hidden.
The new map pans/zooms/recenters independently of the world camera. The local
minimap follows the hero over a local region; the M window can zoom out further.

Stage3FantasyHudArtImporter.Import copies original generated PNG bytes and
creates Unity-native sprite rectangles/borders. Existing approved world sprites
are reused. Stage3PlayerInterfaceBuilder.BuildAndWire imports/wires only this UI
in the saved Stage3Rapid3DGamePilot scene. BuildWindowsPlayer writes
Builds/WindowsInterface/Not3AInterface.exe. No manual Inspector work is required.

The ordinary HUD is replaced; existing development/outcome controls remain on
the legacy component. An opt-in Development-only NOT3A_INTERFACE_CAPTURE_DIRECTORY
records real visible-player OnGUI frames without grants or camera/time changes.
NOT3A_INTERFACE_CAPTURE_INTERACTION=1 additionally records automated pointer
states through the real Input System mouse control; requested/actual positions
are logged. The opt-in capture restores the background input policy and returns
to the normal HUD. It does not simulate clicks or override a UI hover flag.

Concepts: art/concepts/interface_2026-10-18/ and interface_runtime_2026-10-18/.
Full prompts, source images, rights and approval states:
art/source/interface_runtime_2026-10-18_generation.json.
Official game references informed navigation/hierarchy only; no third-party
interface pixels or traced assets were used.

Verification for D-068: PlayMode 16/16 (seven interface/map cases, four building
menu cases and five HUD/debug regressions). Two off-center anchors survive
zoom in/out with maximum measured world error below 0.000008; invalid values,
zoom limits, world bounds and camera independence pass. Synthetic Touchscreen
events exercise moving-midpoint pinch and single-finger pan.

Astra accepted proportional frames, sans-serif, safe content, hero marker and
hover/disabled presentation across 1280x720, 1600x900 and 1920x1080. Final V4
contains 39 native full-player PNGs. Pressed styles are code-reviewed, with no
separate mouse-down capture; physical touch hardware and a separate complete
manual drag/wheel playthrough are not claimed. Existing gameplay/inactive
reserves remain unchanged. No Inspector migration is needed.

Historical D-068 evidence and limitations:
art/source/interface_correction_2026-10-18_verification.json.
The initial report interface_runtime_2026-10-18_verification.json is historical;
the owner's annotated feedback reopened its insufficient visual quality gate.

## D-070 Mac-trackpad correction, 2026-10-18

The world camera remains the single poller of the app-local AppKit queue.
The existing binding supplies a viewport hit-test and map callback. While M
owns the pointer, pinch ratios route to anchored map zoom and precise scrolling
routes to map translation in backing pixels; world camera zoom is untouched.
A coarse mouse wheel still reaches the existing IMGUI anchored zoom. The
native monitor classifies precise scrolling (including precise mouse devices)
separately and consumes it only for map panning. Enter/leave, focus and disable
clear pending gestures. Existing touchscreen midpoint pan/pinch is unchanged.
This fixes the Mac-trackpad gap not covered by D-068's synthetic Touchscreen.
Physical trackpad hardware verification is not implied by native/unit tests.

## D-069 implementation and verification

CurrentWindowRect covers the complete backbuffer. FullscreenHeaderRect,
FullscreenWalletRect, FullscreenFooterRect and FullscreenTabRect describe the
shared shell; the Construction tab is Rect.zero. HudMenuButtonRect describes
the five compact world buttons. MenuButtonRect returns the currently visible
navigation. The modal blocker protects all four screen corners. Existing
construction, payment, input lifecycle and map math retain their contracts.

Runtime selects one installed Cyrillic sans-serif face (Segoe UI first on
Windows, Arial first on Mac), with a built-in fallback. Metal/Linear drawing
corrects texture and solid-color output in the existing owned shader; Windows
retains its prior shader encoding. Original PNGs, scene wiring, imports and
packages are unchanged. No Inspector migration is required.

PlayMode 16/16 passed on Unity 6000.6.2f1 / macOS / Metal, including exclusive
keyboard pages, full-screen input protection, continuing day time, local-E
re-arming, actual construction purchase and anchored cursor/synthetic touch map
regressions. The saved-scene Mac Development player built successfully via
Tools > Stage 3 > Player Interface > Build Local Mac Review Player, at
Builds/MacFullscreenInterface/Not3AFullscreenInterface.app. This is local review
output; Windows/Steam remains the product target.

The V4 player produced 39 actual full-backbuffer captures at 1280x720, 1600x900
and 1920x1080. Native keyboard I/P/O/M, Esc and in-world B were also observed.
Astra accepted all 18 main V3 frames and the nine V4 HUD/construction/hover
frames; V4 changes only turquoise hover/selected alpha from .22 to .50.
The final V4 player is open on Inventory at 1600x900. No new attributable
game/font/shader errors were found in the final build and runtime logs.
Automated coordinate-click smoke was unavailable because the native UI tool
returned noWindowsAvailable; this is not claimed as a manual mouse playthrough.
Hover capture uses the pointer-only Input System method described above;
pressed and physical touch have no separate capture. The host has only the
MacStandaloneSupport module, so current Windows build/runtime validation is
degraded and the older D-068 Windows evidence does not validate this revision.
Current evidence: art/source/fullscreen_interface_2026-10-18_verification.json.


## D-089/D-090 item inventory and local forge

Approved inventory/forge layouts now bind real item instances through the existing
IMGUI fullscreen stack. Inventory has five combat slots and two tools; no shield
or sword, short wand replaces staff. Actual owned items scroll without invented
capacity. Right action equips/unequips the selected instance; level/quality/stats
come from equipment data. Armor explicitly says hero protection is not opened.
Construction Base has third Forge card; blueprint remains in world. Local E or
selected Forge card opens the service, with neutral global tabs and Esc-only
footer. Create/Level/Reforge show current/outcome, actual cost and active cap;
quality reforge retains numerical level. SSS is ultra-rare loot only. Selected
building strip shows stage/cap/scroll count, disabled upgrade reason without a
special scroll and Open Forge action. No new global hotkey/window for building.
Astra owns approved presentation and world/held-weapon art; main owns validated
binding and rules. See EQUIPMENT_AND_RUNES and runtime provenance.


D-091 correction (2026-10-20): bag grid excludes every equipped item. Slots
and right details use the same full owned instance lookup, so inspecting an
already equipped item still works. Equip/replace/unequip immediately updates
both views; all three starter items leave the initial bag empty. Empty bag
means no spare items, not no owned equipment. No capacity/drop change.
