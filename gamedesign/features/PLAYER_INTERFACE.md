# Feature: Player Interface

Status: D-066/D-068, runtime correction implemented and verified, 2026-10-18. The owner
chose C «Лунный фарфор», authorized implementation of the window sheets and
explicitly approved both revised fullwidth normal/construction HUD sketches.
Owner: GPT-6 Astra owns visual decisions, layouts and subsequent visual
implementation; the primary agent performs ImageGen, integration and records.
Dependencies: existing scene-owned HUD/input protection, resource wallet,
construction catalogs, research/workshop, hero equipment and world map.

## Accepted direction

Separate windows serve inventory/equipment, construction, research, hero
development and the large map. Each has a visible access button as well as a
keyboard shortcut: I, B, P, O and M respectively. The same key toggles its
window, another key switches windows, and Esc closes. Construction is a
nonmodal ribbon; the other windows block player movement/world input without
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
The day-only dwelling skip contract from D-065 is unchanged.

The owner first asked to preserve the prototype minimap, then explicitly
allowed complete redesign of both map and minimap. A future large world uses
a local minimap and an M window with panning/zooming over a regional viewport;
the entire eventual world need not fit on screen at once. No new geography,
fog-of-war, discovery or travel rules are authorized by these drawings.

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
storage and slot behavior remain unavailable. Hero development is an inactive
schematic reserve; its nodes do not define an approved skill tree, cost model,
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

Stage3FantasyHud and its window partials own drawing, layout and window/map
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

Current evidence and limitations:
art/source/interface_correction_2026-10-18_verification.json.
The initial report interface_runtime_2026-10-18_verification.json is historical;
the owner's annotated feedback reopened its insufficient visual quality gate.
