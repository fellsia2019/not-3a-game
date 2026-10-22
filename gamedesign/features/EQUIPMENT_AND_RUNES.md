# Equipment, forge, tools and runes

## D120 — implemented dual loadout and consumables,2026-10-22

Current Stage3 has12fixed positions: head/chest/arms/legs, bow, staff,
pickaxe/axe, amulet, two rings and magical protection. Bow AND staff remain
equipped; only one is active. Z or the clickable weapon thumbnails switches
active stats/1–5 skills. Neither switch nor replacement resets attack/skill
cadence, refunds MP, duplicates items or combines both weapon stats. Both count
as equipped for bag exclusion, trade rejection and death retention. Fixed-slot
UI uses EquippedAt; historical Equipped(Weapon) means the active weapon.

The long staff replaces the short wand's name/art, preserving Wand enum and
WandRecipe identity. Existing instances survive; current Stage3Equipment opts
into expandedLoadout, while legacy profiles default to their previous single
weapon behavior. Existing enum IDs0–7/slot IDs0–6 are preserved; new types and
slots are appended. No scene/GUID or disk-save migration. Staff acquisition:
buy the finite recipe paper, study locally at the one-level workshop, separately
craft at the forge. Knowledge gates crafting, not equipping an acquired item.
No new world drop source, loot table or random merchant restock.

Three accessory types are forgeable with the existing materials/gold/quality/
level rules: «Амулет натиска» increases hero damage; one ring increases maximum
MP without filling it on equip; «Защитная печать» supplies constant incoming
exploration-damage reduction without consuming MP. The latter is a standalone
arcane sign/personal covering, distinct from necklace/rings, physical armor or
rune sockets. Existing damage has no invented physical/magical resistances.
Reduction is bounded, then the active personal barrier absorbs remaining damage;
UI shows the actual scaled values. Two rings stack their own bonuses. Existing
four clothing slots retain their previous stat scope.

Accessories use F/E/D/C/B/A/S/SSS and item levels1–100; current forge has11types,
existing stages cap at D/B/S and never creates SSS. Around40future amulet effects
remain future scope. Runtime values live in Stage3Equipment/Stage3Merchant,
including accessory valuation; generated concept numbers are not balance.
Actual painted quality frames and measured safe-area bounds are shared across
bag/loadout/forge/merchant. Empty amulet is a pendant silhouette, rings circular.

F1/F2/F3 use HP/MP/mixed purple bottles; F4 stays reserved. These fixed type
positions reflect actual bag stacks, with no free grant or extra equipped copy.
Bag selection routes to the matching quick-use position. Prices/restoration
explicitly set by owner:100gold restores100HP,100gold restores100MP,250gold
restores100ofEACH. Merchant starts with5ofeach, no automatic restock; shared
20s cooldown, purchase/sale ONE bottle per transaction. Potions are neutral
ungraded consumables without equipment rank/level/forge/reforge. No consumption
when the relevant vitals are full; mixed works if either resource needs filling,
clamped to each maximum. Stock/count/gold/effect changes are atomic, with no
spend on invalid/paused/blocked actions. Potions remain BAG ownership and drop/
recover on death. Retry preserves current bottles/trades/cooldown; full Reset
restores finite merchant stock and gives the hero no free potion.

Scene wiring/imports: Stage3HeroLoadoutInstaller.Install creates/migrates the
skill catalog, imports original staff/accessory/potion and skill atlases into
existing ForgeArt, and saves current equipment/merchant data. Original PNGs,
asset GUIDs/scenes/packages and prior equipped instances are preserved.
Main owns rules/integration; Astra owns native art/layout/review. Evidence:
art/source/hero_loadout_2026-10-22_verification.json.

Historical D089–D102 baseline (D120 above owns current expanded loadout):

Status: D-089 concepts approved; D-090 runtime implemented and locally qualified. Five combat slots plus two tools, numerical1–100 and8qualities.
Bow and wand only; no sword/shield. Three forge stages D/B/S, special-scroll
building upgrades in legacy profiles; D102 opt-in replaces scroll payment with
studied target-stage blueprints and gates wand crafting. SSS exclusively future ultra-rare loot. Loot tables,
scroll sources and rune consumption are not implemented. Runtime tuning lives
in Stage3Equipment and Stage3ForgeBuilding data, subject to playtest.
Owner: Stage 3 Game Design. Dependencies: HERO_PROGRESSION.md,
BOSSES_AND_VICTORY.md, PORTAL_CONTROL.md, RESEARCH.md,
RESOURCE_PRODUCTION_BUILDINGS.md and STAGE2_RUN_STATE.md.

D102 implementation: four separate non-equippable documents are traded for gold,
then consumed at the single-level research workshop. ForgeII/III knowledge opens
the matching upgrade action, with no second document/scroll spend. Recipe study
gates crafting wands, not equipping an existing wand. D/B/S caps and SSS exclusion
remain. Known duplicates use neutral paper cells plus «Изучено», no item rank/LVL.
RESEARCH.md owns the current rules and verification; D101 text below is historical.

## D-101 future upgrade document appearance

The owner asks for a shared medieval magical blueprint base, including future
forge/workshop building upgrades and research. After rejecting tilted parchment
and unfolded3panel examples, the owner also rejected the modern white-sheet
mockup. Reuse the earlier aged ivory/amber colour/texture and plum drafting style
in a straight rectangular sheet. Two primary uses are opening new content
(including construction access) and improving existing content. Blueprints do not use
equipment F–SSS or independent LVL1–100; upgrade numbers denote target BUILDING
level. Mentioned4–5/forgeIV are examples, not an extension of current forge caps.
All inventory/trade cells and rows stay fixed regardless of icon/metadata.
Owner approved rect_categories_shop_v3 appearance/category treatment on2026-10-20.
This clarifies future item appearance; current
ForgeUpgradeScrolls storage/source and D090 D/B/S caps/SSS exclusion are unchanged
until a ready runtime brief. No runtime rank/LVL, generic upgrade prerequisite or
merchant stock is inferred. RESEARCH.md owns the document concept; source:
art/source/research_blueprints_2026-10-20_generation.json.

## D-100 merchant ownership and valuation

The first city shop trades existing equipment instances for gold, with8initial
F/LVL1items,50% sale and no automatic restock. Equipped instances remain only in
loadout and cannot be sold; spare bag items transfer to merchant stock with the
same ID/type/quality/level. Purchase transfers that same instance into the bag
without automatic equip. Sold higher-quality loot may be bought back; this does
not introduce a new SSS source. No research/forge scroll, rune or resource stock.
Trial type/quality/level prices live in Stage3Merchant data. D087 retry preserves
current trades and finite stock; full reset clears them with starter equipment.
CITY_AND_TRADE owns access/payment rules; current shared skins/safe areas apply
to both grids and large trade details.41/41 focused PlayMode plus21native frames
and actual mouse buy/sell locally qualified; Windows unverified. Evidence:
art/source/merchant_2026-10-20_verification.json.

## Global item-quality cell treatment — D-097, source-skin correction implemented

The owner requires quality to be visible through colored cell artwork everywhere
item UI is used. Eight ranks F/E/D/C/B/A/S/SSS remain separate from level 1–100.
The approved reference is the original 64-item catalogue:
art/concepts/item_quality_cells_2026-10-20/all_equipment_qualities_v2.png.

The initial procedural rendering was rejected by the owner: flat opaque dark
fills and thick rectangular gold bars did not reproduce that reference. Its
local visual PASS is superseded for appearance. The accepted target includes
painted multitone gradients, engraved texture, darker edges, thin shaped metal
rims and local luminous highlights. D stays green, C royal blue, B purple, A
ruby; S combines amber-to-near-black depth with fine polished gold, while SSS
combines plum/violet depth and radiant gold. Rank letters/LVL stay readable;
separate cyan selection preserves the quality artwork. Empty/ungraded scrolls
and generic category symbols remain neutral.

Main prepared eight empty painted skins by ImageGen editing the approved original.
Astra checked the raster against that source and implemented it directly in
Stage3FantasyHud.Quality. One shared imported Resources atlas has measured pixel
bounds per rank: the source is not an exact equal grid. Painted centres retain
multitone texture; eight surrounding frame patches preserve fine corners/edges.
Shared destination edges are pixel-rounded; custom texture draws retain explicit
geometry/UV clipping in bag and forge scroll regions. No procedural color plates,
synthetic gold rails or generated sparkle overlay remain. The HUD borrows the
single atlas and allocates no per-item or per-frame textures.

Apply it to bag/loadout/details, forge source/result/owned chips/rank chain,
actual profession tools and equipped HUD weapon. Existing rank-specific icon
atlases, gameplay/stats/actions/input geometry and world art stay current.
D100 merchant runtime reuses this treatment; SSS remains future
ultra-rare loot, never a forged/merchant-stock promise. No scene/Inspector wiring.

Source: art/concepts/item_quality_cells_2026-10-20/runtime/quality_skins_reference_v1.png.
Runtime: Assets/Stage3/PlayerInterface/Resources/Stage3EquipmentQualitySkins.png.
Import: Stage3EquipmentQualityArtImporter.BuildAssets, also automatic texture
preprocessing; original PNG bytes preserved, NPOT/sRGB/alpha/uncompressed/bilinear.
Generation, exact prompt and rights: art/source/item_quality_cells_2026-10-20_skins_generation.json.
Current correction qualification: art/source/item_quality_cells_2026-10-20_verification.json.
After correction, 17/17 PlayMode checks pass and Mac build succeeds. Astra strict
reference PASS covers 27 native frames at720/900/1080p; actual mouse equip/forge
scroll pass. Windows is unverified. Tiny HUD skins preserve hue/edge/radiance
with less engraving detail. Final owner appearance acceptance is not inferred.

Owner safe-area correction: every type/rank must fit by its actual visible
silhouette, not the full source tile's unequal transparent margins. Bake 64
UI-only alpha bounds into the existing ForgeArt asset; keep all source PNGs,
held pivots and world registration. Axes use only their two allowed source bands.
Use one aspect-preserving, centred fit and proportional inset from the painted
frame/ornaments, including large details, loadout, forge chips, professions and
HUD. No runtime pixel scans, per-item offsets or per-frame texture generation.
Qualification must include all 64 combinations and actual windows at three sizes.
The bake identifies the dominant connected alpha silhouette by overlap with the
old logical tile, then takes its complete bounds over the source PNG: old tile
edges must not truncate artwork. Up to two source pixels preserve the fringe,
reduced before a neighbouring silhouette enters. All 64 regions are validated
for foreign silhouettes, including axe masks. Shared pixel-rounded destination
edges keep the two axe bands from exposing a fractional-quad seam.
Implementation/qualification: art/source/item_safe_areas_2026-10-20_verification.json.
Final bake validates64/64; Mac build and all64 native projections at720/900/1080p
cover the correction.17existing PlayMode checks passed before the final Editor/
pixel-only repair; actual mouse details/equip/forge scroll also checked. Source
PNG hashes match prior provenance. Windows and owner appearance response remain.

## Initial content scope — D-089

Сейчас по одному базовому семейству каждого типа: без именных, стихийных и
особых вариантов. Качество F/E/D/C/B/A/S/SSS меняет саму иконку/внешность:
от грубого коричневого дерева/простых материалов к железу/стали, красной и
золотой отделке. Не ограничиваться перекрашиванием рамки. Числовой уровень
1–100 отдельный и не требует100 рисунков. Новые семейства — позднее, при
наполнении/балансе. Первоначальные эскизы содержали посох/щит. Прямое уточнение D-090 заменяет
их коротким жезлом и убирает щит вместе со слотом. Сейчас8типов: лук, жезл,
четыре части брони, кирка и топор;5боевых слотов и2рабочих.
Владелец утвердил все четыре эскиза: инвентарь v2, здание кузницы v1,
fullscreen кузницы v2 и лист качеств v1 («Утверждаю все четыре»). Компоновка,
вид здания и показанная палитра/развитие внешности приняты. Числа свойств,
цены этим не назначены. Ступени позднее заданы D-090; runtime-ассеты
подготовлены ImageGen, интерфейс/внешность внедряет Astra. Источники/версии:
art/source/equipment_forge_2026-10-19_generation.json.

## Reward sources

TD даёт основной поток боевого опыта благодаря числу врагов. Лагеря и руины
мира дают меньше опыта, зато повышенный шанс ценных находок. Бандиты, орки
и нежить названы возможными обитателями, не утверждённым списком фракций.
Предметы/оружие могут выпадать с врагов обеих групп. Чем выше качество,
тем реже оно встречается, а не тем хуже его характеристики.

После зачистки места автоматически выдаётся случайная награда: экипировка,
чертёж, временный свиток или постоянная руна. Для этой выдачи не нужен
дополнительный подбор/E. Награда видна и сразу записывается в инвентарь/журнал.
Одно завершение места платит один раз; повторный вход или сигнал смерти
не дублирует выдачу. Правила повторного заселения пока не выбраны.

Балансировать нужно ценные находки за сопоставимое время, не только шанс
на одного врага. Иначе тысячи убийств у базы обгонят десятки врагов лагеря
даже при лучшем шансе в лагере. Состав наград/чертежей и диапазоны качества
должны давать причину выходить в мир. Точные проценты ещё не выбраны.

До автоматического лута нужен предметный инвентарь и правило переполнения:
предмет не теряется из-за отсутствия места. Ключевая технология обороны
не должна зависеть от единственного редкого случайного чертежа; нужен
надёжный путь развития. Торговец/город и покупка исследований D-071 остаются
отдельной зависимостью. Новые валюты этим не вводятся.

## Equipment axes and slots

| Параметр | Подтверждённый контракт |
| --- | --- |
| Числовой уровень предмета | 1–100 |
| Качество | F → E → D → C → B → A → S → SSS; 8 ступеней |
| Места брони | Шлем, нагрудник, руки, ноги |
| Остальное боевое место | Оружие; всего5боевых слотов |
| Начальные виды оружия | Лук — физический урон; короткий жезл — магический |

Уровень и качество независимы. SSS первого уровня не обязан превосходить
F сотого. Уровень источника определяет допустимую добычу, включая качество
рун: простой фарм не выдаёт весь верхний каталог только из-за уровня героя
или числа повторов. Рост качества не означает стократного множителя урона.
Не нужно рисовать сто версий каждого базового предмета.

D-090 заменяет прежние6мест пятью, удаляя щит. Кирка/топор — отдельные
рабочие предметы. Один экземпляр оружия занимает общий слот лука/жезла;
смена оружия не закрепляет класс. Броня хранится/надевается и имеет свойство
брони, но HP/защита/смерть героя не открываются этим срезом. UI явно это
показывает, без вымышленных HP или обещания работающей защиты.

Предметы усиливают одиночную атаку, личную защиту и перемещение. Совместно
с навыками/рунами они не запускают массовые цепи или взрывы, заменяющие
башни против соответствующей плотной орды. Магический жезл сохраняет
этот предел, как и физический лук.

## Forge

Кузница создаёт новый предмет F1, повышает числовой уровень1–100 отдельным
действием и последовательно перековывает качество F→E→D→C→B→A→S.
Качество/числовой уровень/ID независимы: перековка не сбрасывает уровень,
повышение уровня не меняет качество. Успех гарантирован, случайной поломки
и новой валюты нет. SSS доступно только через будущий ультраредкий дроп.

D-090 задаёт ровно3ступени здания:

| Ступень | Доступные качества |
| --- | --- |
| 1 | F/E/D |
| 2 | F/E/D/C/B |
| 3 | прежние качества + A/S |

Улучшение на следующую ступень требует специальный свиток кузницы; материалы
и золото его не заменяют. Источника свитков ещё нет, обычная игра начинает
со ступени1. Найденный предмет выше предела сохраняется/надевается, но
текущая кузница его не улучшает. SSS не появляется в результате перековки.

Новая постройка оплачивается только деревом/камнем/металлом, выбирается
третьей карточкой группы «База» в B. Нужна свободная лесная площадка;
коллизия корпуса не закрывает вход. E у входа или кнопка выбранной карточки
открывает fullscreen-сервис. Издалека, в паузе, debug или другом модальном
окне сервис не работает. Нельзя оплатить чертёж кликом через UI. Отмена
не платит; успешное создание/улучшение списывает стоимость ровно один раз.
До оплаты интерфейс показывает исходный предмет, результат, цену и предел.
Цены/кривые пробные и сериализованы в Stage3Equipment; footprint/вход/цена
постройки — в Stage3ForgeBuilding. Новых руд/плавки/ремонта этим не добавлено.

Начальный лук, кирка и топор F1 становятся реальными надетыми предметами,
сохраняя доступность прежней атаки/добычи. Крафт не надевает новую вещь
автоматически. Снятое оружие отключает автоатаку, снятый инструмент — его
тип добычи; предмет остаётся в сумке. Одно применение инструмента выдаёт
не больше силы, остатка узла и свободного места. Смена инструмента отменяет
незавершённый удар; бесплатного дохода/атаки сменой экипировки нет.

Хороший дроп экономит изготовление и перековку. Ковка F страхует неудачные
находки, а не делает любые находки заведомо хуже. Поздние рецепты/ступени
можно связывать с исследованиями и миром, сохраняя достижимость развития.

## Crafted pickaxes and axes

Подтверждено создание более хороших кирок/топоров и апгрейд имеющихся.
Они относятся к кузнице, а не заменяются одной пассивкой героя. Для общего
каталога предлагаются те же оси 1–100 и F–SSS; точные свойства инструментов
ещё требуют игрового баланса.

Мастерство профессии, предмет-инструмент и исследование автодобычи — три
разных системы. Лучший инструмент увеличивает силу/темп ручного удара.
При силе 3 по остатку 2 сразу выдаются только 2 материала; стадия и коллизия
следуют остатку. Незавершённый удар не платит. Смена инструмента в процессе
не позволяет получить материал без полного действия.

Улучшение кирки не ускоряет автоматически железный рудник. Развитие трёх
производственных зданий — в RESOURCE_PRODUCTION_BUILDINGS.md. Ручная добыча
сохраняет смысл до покупки/изучения допуска к автодобыче D-071 и поиска
подходящего большого месторождения.

## Rune library

Сохраняется план **40 типов рун**, теперь с восемью качествами F–SSS,
без отдельного числового уровня. Это 320 комбинаций типа/качества в данных,
а не 320 уникальных рисунков. Добавление ста уровней каждой руне дало бы
32 000 комбинаций без доказанной пользы. Точные эффекты ещё не определены.

| Категория | Текущий бюджет | Получатель |
| --- | --- | --- |
| Герой | 16 типов | Личная атака/магия, выживание, перемещение |
| Башни | 16 типов | Урон, темп, дальность, стойкость своей обороны |
| Добыча и исследование | 8 типов | Кирка, топор, польза исследования |

Сначала вводятся несколько реальных эффектов каждой категории. Руны указывают
получателя; «урон героя» и «урон башен» не становятся общим скрытым множителем.
Кирка не передаёт свой бонус автодобыче, а дальность башен — блинку.
Руна расходуется при изучении и усиливает текущий мир независимо от надетой
вещи. Свиток — временный баф, отдельный от постоянной руны. Полный сброс
мира не сохраняет эту прокачку как roguelite-мету.

Сохраняется предложенное правило D-074: действует одно лучшее изученное
качество каждого типа; более сильная руна повышает эффект, такая же/хуже
не складывается бесконечно и не расходуется без пользы. Сто прибавок по 2%
дали бы +200% при суммировании или около 7,24 исходной силы при умножении.
Итоговый предел скорости/силы проверяется вместе с талантами и экипировкой.
Переработка дубликатов и новая валюта пока не выбраны.

## Owner rune appearance brief

Прямое пожелание D-075: каменные фантазийные руны, щитообразная форма.
F — простые серые; промежуточная шкала включает красные; S — золотые;
SSS — наиболее выразительное «суперзолото». Распределение остальных цветов
и трактовка SSS ещё не выбраны. SS не рисуется. Это бриф для ImageGen и
GPT-6 Astra, не новый утверждённый финальный ассет. Качество однозначно
указывается в данных/интерфейсе вместе с цветом.

## State and acceptance

По D-087 поражение сохраняет текущие предметы/инструменты, свойства,
кузницу/ступень, рецепты, чертежи, руны, свитки/сроки и журнал наград вместе
с текущими XP/ресурсами. Перемотка часов не возвращает прежний предмет и не
отменяет оплату. Одно событие выдачи не платит повторно; новые реальные
убийства новой попытки дают обычную награду. Дисковое сохранение и исход
смерти героя в мире пока не определены.

Приёмка текущего среза:5боевых мест +2инструмента; нет SS; уровень и качество независимы;
автоматическая награда видна и не теряется; источник ограничивает добычу;
кузница соблюдает предел и ступени, оплачивает операцию один раз; инструмент
расходует ровно запас узла; повтор сохраняет текущие предметы/ресурсы/здания
согласованно по D-087. Верхний доступный герой сохраняет ограничение против орды.

Новая внешность кузницы/рун, слоты и выдача проходят ImageGen → одобрение
владельца → GPT-6 Astra. Руны/дропы/города/сохранение на диск и визуальная смена надетой брони остаются следующими отдельными задачами.


## Runtime ownership and wiring — D-089/D-090

Stage2EquipmentCatalog/Equipment are plain data/rules; EquipmentDriver binds
current-world lifetime to combat and gathering. No global manager/new package.
Stage3 binding loads the three Resources assets and initializes the local forge;
existing scene GUIDs/prefabs/serialized fields remain intact. Installer:
Not3A.Stage3.PlayerInterface.Editor.Stage3EquipmentInstaller.Install.
The Forge Art importer creates an owned material/sprite and keeps generated PNG
bytes unchanged. Resources include all8qualities even when forge cap is lower.
TryReceiveLoot/TryReceiveForgeScrolls are validated future-source entry points,
not automatic drops or player cheats. Normal startup receives no armor,
scrolls or free construction materials. Native QA setup is opt-in development
player only via NOT3A_EQUIPMENT_FORGE_CAPTURE_DIRECTORY.


Qualification:91 unique latest results passed (67PlayMode,24EditMode). The first
cross-system run had one old clock assertion failing by1ms; its whole8-test class
passed after a .01sec tolerance, without a runtime clock change. Mac development
build and actual native E/mouse craft→level→reforge→equip passed. Astra reviewed
720/900/1080p; final ordinary-start inventory qualifies corrected axe UV. No new
attributable console errors. Windows player not qualified (module absent).
Evidence/source: art/source/equipment_forge_2026-10-19_verification.json.


## D-091 presentation correction —2026-10-20

The left bag contains only unequipped instances. Equipped armor/weapon/tools
appear in their actual slots; equip/unequip and replacement move the same item
between bag/loadout without duplication or loss. Full owned lookup remains
available for selected-item properties; the bag has a separate filtered view.

Bow/pickaxe/axe appearance must match the equipped quality on the actual hero.
The old generic inventory bow image cannot substitute for all world facings.
Additive bow projection sprites and tool-free harvest layers preserve existing
hero clothing/armor and work poses. Detached tool renderer follows authored grip
and strike animation, never displays a second baked starter. Missing tool hides
it, pause freezes, leaving harvesting restores the equipped weapon. Armor
appearance stays unchanged by explicit request. Sources/rights and final QA:
art/source/equipment_visible_2026-10-20_generation.json and verification.json.


D-091 qualification:20unique latest PlayMode results passed, final native
Mac/Metal equip/unequip/re-equip and held-E axe/pick passed; Astra final
world/inventory visual PASS. Empty-bag/3equipped-starter reset captured; ordinary
game relaunched without QA grants. No new attributable console errors. Windows
unverified. Exact artifacts/setup/limits are in the verification manifest.
