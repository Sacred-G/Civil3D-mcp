# WASD Standard Symbol Legend & Abbreviations

Reference data for identifying Miami-Dade Water and Sewer Department (WASD) standard
plan-sheet symbols and abbreviations. Drawings produced against the WASD template (see
any sheet titled `WASD REFERENCES`, with `LEGEND` / `ABBREVIATIONS` blocks in the PLAN
layout) reuse this exact symbology, flattened as a PDF underlay rather than as AutoCAD
blocks or editable text — that's why `acad_list_block_references` and
`acad_list_text_entities` won't find it, and why raw shapes (arcs, hatches, ellipses,
solids) must be matched by eye/geometry against this list instead.

Source: Miami-Dade WASD, *"CAD Standards for Pipeline Design & Topographic Drawings"*
(June 10, 2024), Exhibits A.1–A.8, B.1–B.3, C.1–C.2, D.1–D.4 (symbol legend) and
Appendix B / Tables 3.7.1–3.7.2 (abbreviations).
<https://www.miamidade.gov/resources/water/documents/donation/part-6/wasd-cad-manual-june-2024.pdf>

A bilingual (English/Spanish), print-formatted version of this same data lives at
[`docs/reference/WASD-Legend-Abbreviations-EN-ES.pdf`](WASD-Legend-Abbreviations-EN-ES.pdf).

## How to use this when QC'ing a drawing

1. If `acad_list_text_entities` / `acad_list_block_references` come back empty near a
   `WASD REFERENCES`, `LEGEND`, or `ABBREVIATIONS` title block, check the layers of
   nearby geometry — a flattened WASD legend shows up as many small `Hatch`/`Arc`/
   `Ellipse`/`Circle` entities on layers like `PDF##_Solid Fills` / `PDF##_Geometry`.
2. Use `acad_list_shape_entities` with `nearX`/`nearY`/`nearRadius` centered on the
   symbol in question to pull its raw geometry, then compare the shape (circle+bowtie,
   triangle, H-blade, etc.) against the **Symbol Description** column below to identify
   the **Feature**.
3. Cross-reference any abbreviation found in a callout (e.g. `MHSA`, `F.H.`, `M.J.`)
   against the Abbreviations tables below.
4. Existing vs. Proposed: most water/sewer appurtenances (Exhibits A.5–A.8) are drawn
   thin/open for *Existing* and bold/solid-filled for *Proposed* — the same Feature name
   covers both variants.

## LEGEND

| Exhibit | Category | Feature | Symbol Description |
|---|---|---|---|
| A.1 (1 of 2) | General | Bar Scale: Horizontal 1"=20' | Horizontal bar scale graphic, ticks at 0'-10'-20'-40', labeled "SCALE BAR" |
| A.1 (1 of 2) | General | Bar Scale: Horizontal 1"=40' | Horizontal bar scale graphic, ticks at 0'-20'-40'-80', labeled "SCALE BAR" |
| A.1 (1 of 2) | General | Bar Scale: Horizontal 1"=60' | Horizontal bar scale graphic, ticks at 0'-30'-60'-120', labeled "SCALE BAR" |
| A.1 (1 of 2) | General | North Arrow | Horizontal arrow flanked by two short flags, with a small "Z"-style compass tick in the middle |
| A.1 (1 of 2) | General | Logo: Miami-Dade County | Miami-Dade County "D" swirl emblem with wordmark and tagline |
| A.1 (1 of 2) | General | Benchmark | Circle bisected by a horizontal line with a center dot (target/benchmark symbol) |
| A.1 (1 of 2) | General | Core Boring | Circle divided into quarters with two opposite quadrants filled solid black (pie-style) |
| A.1 (1 of 2) | General | Subsurface Utility Engineering | Small square split into four quadrants in a checkerboard black/white pattern |
| A.1 (1 of 2) | General | Iron Pin (Found and Set) | Small double concentric circle (ring within a ring) |
| A.1 (1 of 2) | General | Station | Triangle with a small circle centered near its apex |
| A.1 (2 of 2) | General | Section Corner | Solid black four-pointed "maltese cross" mark |
| A.1 (2 of 2) | General | Property Corner Arrow | Freehand curved line ending in an arrowhead, pointing up-right |
| A.1 (2 of 2) | General | Project Location Arrow | Curved arrow with a small hatched/scribble mark at the tail, pointing down-right |
| A.2 (1 of 3) | Topographic Features | Tree | Solid black radiating starburst of branching lines (tree canopy) |
| A.2 (1 of 3) | Topographic Feature | Palm | Snowflake-like radiating star (palm frond symbol) |
| A.2 (1 of 3) | Topographic Feature | Shrub | Irregular scalloped/scribbled circular outline |
| A.2 (1 of 3) | Topographic Feature | Hedge | Row of overlapping scalloped semicircles (cloud-like line of bumps) |
| A.2 (1 of 3) | Topographic Feature | Mailbox | Simple line drawing of a mailbox on a post |
| A.2 (1 of 3) | Topographic Feature | Bollards | Small circle with cross/spoke marks inside |
| A.2 (1 of 3) | Topographic Feature | Chainlink Fence | Line with alternating "X" marks: —x—x—x—x— |
| A.2 (1 of 3) | Topographic Feature | Wood Fence (inline) | Long-dash line |
| A.2 (1 of 3) | Topographic Feature | Wood Fence | Long-dash line (same style as inline variant) |
| A.2 (1 of 3) | Topographic Feature | Iron Fence | Line with double-tick marks at intervals: —//—//—//— |
| A.2 (2 of 3) | Topographic Feature | Guardrail | Straight line with small circles (dots) at intervals |
| A.2 (2 of 3) | Topographic Feature | Treeline | Scalloped wavy line (row of connected arcs) |
| A.2 (2 of 3) | Topographic Feature | Traffic Sign | Small circle atop a vertical post line |
| A.2 (2 of 3) | Topographic Feature | Traffic Signal Pole | Long horizontal arm with a circle at one end and a small rectangle at the other |
| A.2 (2 of 3) | Topographic Feature | Street Light Pole | Circle with short radiating lines around it (sun/starburst) |
| A.2 (2 of 3) | Topographic Feature | Concrete Pole | Rectangle with rounded hook-shaped ends on both sides (capsule with brackets) |
| A.2 (2 of 3) | Topographic Feature | Pedestrian Crossing Light | Circle flanked by bracket/parenthesis marks on both sides |
| A.2 (2 of 3) | Topographic Feature | Bench | Rectangle outline with short vertical end pieces (bench/table shape) |
| A.2 (2 of 3) | Topographic Feature | Flood Light | Small fan of radiating lines atop a short stem |
| A.2 (2 of 3) | Topographic Feature | Traffic Box | Rectangle with letters "TR" inside |
| A.2 (3 of 3) | Topographic Feature | Traffic Line | Line broken by the text label "TRAF" |
| A.3 | Paving | Proposed Pavement | Solid light-gray filled rectangle |
| A.3 | Paving | Concrete | Stippled/speckled dot-pattern fill |
| A.3 | Paving | Patch | Diagonal 45-degree hatch-line fill |
| A.3 | Paving | Ground | Cross-hatched basket-weave pattern fill |
| A.3 | Paving | Pavers | Brick-coursing pattern fill (rows of small rectangles) |
| A.4 (1 of 2) | Utilities | Gas Line | Line broken by the text label "GAS" |
| A.4 (1 of 2) | Utilities | Gas Manhole | Circle with letter "G" inside |
| A.4 (1 of 2) | Utilities | Gas Valve | Bowtie/hourglass shape (two triangles point-to-point) |
| A.4 (1 of 2) | Utilities | Underground Electrical Line | Line broken by the text label "E/U" |
| A.4 (1 of 2) | Utilities | Overhead Electric Line | Line broken by the text label "OH" |
| A.4 (1 of 2) | Utilities | Utility Pole | Circle flanked by two curved bracket marks (crossarm brackets) |
| A.4 (1 of 2) | Utilities | Guy Pole | Small curved bracket/parenthesis shape |
| A.4 (1 of 2) | Utilities | Light Box | Small rectangle containing an "LT" abbreviation with a vertical divider |
| A.4 (1 of 2) | Utilities | Telephone Line | Line broken by the text label "TEL" |
| A.4 (1 of 2) | Utilities | Telephone Manhole | Circle with letter "T" inside |
| A.4 (2 of 2) | Utilities | Telephone Box | Small square with "T" inside |
| A.4 (2 of 2) | Utilities | Cable TV Line | Line broken by the text label "CATV" |
| A.4 (2 of 2) | Utilities | Cable TV Box | Small square with "TV" inside |
| A.4 (2 of 2) | Utilities | Electric Manhole | Circle with letter "E" inside |
| A.4 (2 of 2) | Utilities | Electric Box | Small square with letter "E" inside |
| A.5 (1 of 4) | Water Appurtenances | Water Meter | Existing: thin-outline square with "W"; Proposed: bold-outline square with "W" |
| A.5 (1 of 4) | Water Appurtenances | Dual Water Meter | Existing: two thin-outline "W" squares side by side on a base line; Proposed: same, bold outline |
| A.5 (1 of 4) | Water Appurtenances | Water Manhole | Existing: thin circle with "W"; Proposed: bold/thick circle with "W" |
| A.5 (1 of 4) | Water Appurtenances | Fire Hydrant | Existing: open/unfilled hydrant symbol (circle with two side nozzles); Proposed: solid black filled hydrant symbol |
| A.5 (1 of 4) | Water Appurtenances | Water Well | Existing: double concentric circle with "W" inside; Proposed: none shown |
| A.5 (1 of 4) | Water Appurtenances | Sprinkler | Existing: small filled dot inside a circle; Proposed: none shown |
| A.5 (1 of 4) | Water Appurtenances | Horizontal 11.25° Fitting | Proposed only: short horizontal bar with vertical end ticks (H-shaped fitting) |
| A.5 (1 of 4) | Water Appurtenances | 11.25° Fitting (rotated down) | Proposed only: H-shape with one end curved downward |
| A.5 (1 of 4) | Water Appurtenances | 11.25° Fitting (rotated up) | Proposed only: H-shape with one end curved upward (mirrored) |
| A.5 (1 of 4) | Water Appurtenances | Horizontal 22.5° Fitting | Proposed only: H-shape with a slight angled branch |
| A.5 (2 of 4) | Water Appurtenances | 22.5° Fitting (rotated down) | Proposed only: H-shape variant, branch curving down |
| A.5 (2 of 4) | Water Appurtenances | 22.5° Fitting (rotated up) | Proposed only: H-shape variant, branch curving up |
| A.5 (2 of 4) | Water Appurtenances | Horizontal 45° Fitting | Proposed only: H-shape with 45-degree angled branch |
| A.5 (2 of 4) | Water Appurtenances | 45° Fitting (rotated down) | Proposed only: H-shape, 45-degree branch angled down |
| A.5 (2 of 4) | Water Appurtenances | 45° Fitting (rotated up) | Proposed only: H-shape, 45-degree branch angled up |
| A.5 (2 of 4) | Water Appurtenances | Horizontal 90° Fitting | Proposed only: H-shape with perpendicular (90-degree) branch |
| A.5 (2 of 4) | Water Appurtenances | 90° Fitting (rotated down) | Proposed only: H-shape, 90-degree branch down |
| A.5 (2 of 4) | Water Appurtenances | 90° Fitting (rotated up) | Proposed only: H-shape, 90-degree branch up |
| A.5 (2 of 4) | Water Appurtenances | Vertical Bend (looking down) | Proposed only: H-shape with short vertical tick, bend oriented down |
| A.5 (2 of 4) | Water Appurtenances | Vertical Bend (looking up) | Proposed only: H-shape with short vertical tick, bend oriented up |
| A.5 (3 of 4) | Water Appurtenances | Vertical Offset | Proposed only: H-shape with double parallel end ticks |
| A.5 (3 of 4) | Water Appurtenances | Cross | Proposed only: plus/cross shape with end bars on all four arms |
| A.5 (3 of 4) | Water Appurtenances | Tee | Proposed only: T-shaped fitting symbol with end bars |
| A.5 (3 of 4) | Water Appurtenances | Tee (rotated down) | Proposed only: T-shape rotated, branch down |
| A.5 (3 of 4) | Water Appurtenances | Tee (rotated up) | Proposed only: T-shape rotated, branch up (mirrored) |
| A.5 (3 of 4) | Water Appurtenances | WYE | Proposed only: Y-shaped branch fitting symbol with end bars |
| A.5 (3 of 4) | Water Appurtenances | Solid Sleeve | Proposed only: two short parallel horizontal bars (double tick, no connecting line) |
| A.5 (3 of 4) | Water Appurtenances | Plug | Existing: open/outline triangle pointing left; Proposed: solid filled black triangle pointing left |
| A.5 (3 of 4) | Water Appurtenances | Plug W/ Flush Valve Outlet | Existing: outline triangle with small attached circle; Proposed: solid triangle with small filled circle attached |
| A.5 (3 of 4) | Water Appurtenances | Air Release Valve | Existing: small open circle; Proposed: small solid filled circle |
| A.5 (4 of 4) | Water Appurtenances | Cap | Existing: open bracket/hook shape; Proposed: same bracket shape, bolder |
| A.5 (4 of 4) | Water Appurtenances | Cap W/ Flush Valve Outlet | Proposed only: bracket shape with small filled circle attached |
| A.5 (4 of 4) | Water Appurtenances | Reducer | Existing: outline elongated lens/diamond shape; Proposed: solid filled lens/diamond shape |
| A.5 (4 of 4) | Water Appurtenances | Water Valve | Existing: open bowtie/hourglass shape; Proposed: solid filled bowtie shape |
| A.5 (4 of 4) | Water Appurtenances | Check Valve | Existing: open zigzag "N"-like valve symbol; Proposed: same zigzag, bold/solid |
| A.5 (4 of 4) | Water Appurtenances | Butterfly Valve | Existing: thin open zigzag "N"-like symbol; Proposed: same zigzag symbol, bold |
| A.5 (4 of 4) | Water Appurtenances | Ball Valve | Existing: circle with a vertical bar through it (open); Proposed: solid filled circle with vertical bar |
| A.6 | Sewer Appurtenances | Sewer Manhole | Existing: thin circle with "S"; Proposed: bold circle with "S" |
| A.6 | Sewer Appurtenances | Clean Out | Existing: thin circle with "CO"; Proposed: bold circle with "CO" |
| A.6 | Sewer Appurtenances | Sewer Valve | Existing: open bowtie/hourglass shape; Proposed: solid filled bowtie shape |
| A.7 | Storm | Storm Sewer Manhole | Existing only: circle with "SS" inside (no proposed symbol shown) |
| A.7 | Storm | Catch Basin | Existing only: small square subdivided into a cross-hatched grid (no proposed symbol shown) |
| A.8 (1 of 2) | Profile | Air Release Valve | Existing: horizontal line ending in an open/outline triangle arrowhead; Proposed: solid filled triangle arrowhead |
| A.8 (1 of 2) | Profile | Plug W/ Flushing Valve Outlet | Existing: line with a hooked/curled start, dashed middle, open arrowhead end; Proposed: hooked start, solid line, solid arrowhead |
| A.8 (1 of 2) | Profile | Bend | Proposed only: thin angled vertical parallelogram/blade shape |
| A.8 (1 of 2) | Profile | Offset | Proposed only: two overlapping angled parallelogram blades |
| A.8 (1 of 2) | Profile | Valve | Existing: open double-line vertical bowtie/hourglass symbol; Proposed: same shape solid/bold |
| A.8 (1 of 2) | Profile | Tapping Sleeve | Proposed only: small vertical oval/ellipse outline with cross tick marks |
| A.8 (1 of 2) | Profile | Solid Sleeve | Proposed only: short vertical bar with perpendicular end caps ("I"-beam shape) |
| A.8 (1 of 2) | Profile | Tee (Front View) | Existing: thin vertical bar with small cross ticks; Proposed: bold vertical bar with cross ticks |
| A.8 (1 of 2) | Profile | Tee (Side View) | Proposed only: narrow vertical sliver/blade shape |
| A.8 (1 of 2) | Profile | Bend Horizontal | Proposed only: thin curved vertical sliver shape |
| A.8 (2 of 2) | Profile | Concentric Reducer | Existing: outline vertical lens/blade shape; Proposed: solid filled vertical blade shape |
| A.8 (2 of 2) | Profile | Eccentric Reducer | Existing: outline asymmetric vertical lens/blade shape; Proposed: solid filled asymmetric blade shape |
| B.1 | Text Style | Match Mark | Sample text "CONTINUED SHEET P-X" — layer O-NOTES-TITL, style WASD-NOTE |
| B.1 | Text Style | Pavement Label | Italic sample text "ASPHALT PAVEMENT" — layer V-SITE-TEXT_1, WASD-SIMPLEX |
| B.1 | Text Style | General Notes Title | Large sample text "General Notes" — layer O-NOTES-TITL, WASD-SIMPLEX |
| B.1 | Text Style | General Notes Text Body | Sample numbered note paragraph — layer O-NOTES, WASD-SIMPLEX |
| B.1 | Text Style | Topo Annotation | Sample "Ø1.0' PALM" next to a palm-tree symbol — layer V-SITE-TEXT_1 |
| B.1 | Text Style | Utility Annotation Plan View | Sample inline line labels "X"G." and "X"S.S." |
| B.2 | Text Style | Main Street/Avenue | Large sample text "SW 88th ST" — layer C-ROAD-NAME |
| B.2 | Text Style | Side Street/Avenue | Sample text "SW 137th AVE" — layer C-ROAD-NAME |
| B.2 | Text Style | Property Block Number and Plat Book Info | Sample "RICHMOND HEIGHTS ESTATES THIRD ADDITION", "PB. 68, PG. 34", "BLOCK 30" |
| B.2 | Text Style | Lot Number | Sample text "Lot 11" — layer V-PROP-LNUM |
| B.2 | Text Style | Property House Number | Sample "14830" in a boxed rectangle — layer V-PROP-HNUM |
| B.3 | Text Style | Water/Wastewater Proposed & Existing Title | Sample callouts: "PROP. 8" D.I. WATER MAIN"; "TO BE PLACED OUT OF SERVICE"; "PROP. 8" PVC-C900 FORCE MAIN"; "24" F.M. ABANDONED" |
| B.3 | Text Style | Water/Wastewater Plan Callout | Sample leader callouts: "STA. 20+67 (O/S 12.97' LT.) PROP. 6" M.J. SOLID SLEEVE REST. W/GLANDS" |
| B.3 | Text Style | Existing Water/Wastewater Line Label | Inline labels "X" D.I. W.M." and "X" F.M." |
| B.3 | Text Style | Wastewater Invert Information | Sample multi-line note: "MHSA / RIM ELEV.=7.53' / 6" CLAY (N) INV. ELEV.=2.81' / ... / BOTTOM ELEV.=2.75'" |
| C.1 | Plan View | ROW Line | Long-dash line labeled "R/W" — layer V-RWAY, linetype PHANTOM2 |
| C.1 | Plan View | ROW Centerline | Dash line with small centerline tick label — layer V-ROAD-CNTR, CENTER2 |
| C.1 | Plan View | Property Line | Dash-dot line with small "P/L" tick — layer V-PROP-LINE, PHANTOM2 |
| C.1 | Plan View | Property Lot Line | Short evenly-spaced dash line — layer V-PROP-LOTL, HIDDEN2 |
| C.1 | Plan View | Easement Line | Evenly-spaced dash line — layer V-ESMT, DASHED2 |
| C.1 | Plan View | ROW Limited Access | Line with double-tick marks at intervals — layer V-ROAD-LMTD-ACCS |
| C.1 | Plan View | Topographic (EOP, Sidewalk, Curb, etc.) | Dashed line labeled "E/P" — layer V-TOPO, DASHED2 |
| C.1 | Plan View | Monument Line | Dash line with small "M" tick label — layer V-ROAD-MONL, CENTER2 |
| C.1 | Plan View | Section Line | Dash line with small "S" tick label — layer V-ROAD-SECL, CENTER2 |
| C.1 | Plan View | Baseline | Solid continuous line with small "B" tick label — layer V-ROAD-BASL |
| C.2 | Plan View | Existing Concrete Sidewalk | Two parallel dashed lines, sample text "5' CONCRETE SWK" |
| C.2 | Plan View | Existing Curb and Gutter | Double parallel dashed lines labeled "E/P", sample text "2' CONC. CURB & GUTTER" |
| C.2 | Plan View | Existing Asphalt Pavement | Two dashed lines labeled "E/P" bracketing sample text "ASPHALT PAVEMENT" |
| C.2 | Plan View | Existing Driveway (All Types) | Trapezoid dashed outline with codes DWYA (Asphalt) / DWYB (Brick) / DWYC (Concrete) |
| D.1 | Plan View | Existing Water Service | Dashed line — layer WServiceLine_WASD / C-WATR-PIPE-EXST |
| D.1 | Plan View | Existing Water Main | Dashed line labeled "X" D.I. WM" — layer WDistribution_WASD / C-WATR-PIPE-EXST |
| D.2 | Plan View | Proposed Water Service | Solid thin line — layer WServiceLine_WASD / C-WATR-PIPE-PROP |
| D.2 | Plan View | Proposed Water Main | Solid thick line — layer WDistribution_WASD / C-WATR-PIPE-PROP |
| D.2 | Plan View | Proposed Water Main Centerline, 20" & Larger | Double line: solid heavy edge-of-pipe pair with a center dashed centerline |
| D.3 | Plan View | Existing Sewer Lateral | Dashed line — layer SSewerLateral_WASD / C-SSWR-PIPE-EXST |
| D.3 | Plan View | Existing Gravity Main | Dashed line labeled "X"San." — layer SGravityMain_WASD / C-SSWR-PIPE-EXST |
| D.3 | Plan View | Existing Force Main | Dashed line labeled "X"F.M." — layer SForceMain_WASD / C-SSWR-PIPE-EXST |
| D.4 | Plan View | Proposed Sanitary Lateral | Solid thin line — layer SSewerLateral_WASD / C-SSWR-PIPE-PROP |
| D.4 | Plan View | Proposed Force Main | Solid thick line — layer SForceMain_WASD / C-SSWR-PIPE-PROP |
| D.4 | Plan View | Proposed Gravity Main | Solid thick line — layer SGravityMain_WASD / C-SSWR-PIPE-PROP |
| D.4 | Plan View | Force/Gravity Main Centerline, 20" & Larger | Double line: solid heavy edge-of-pipe pair with a center dashed centerline |

## C-310 PROJECT COVER-SHEET LEGEND (T25-06.212)

Additional Water & Sewer symbols found on the T25-06.212 C-310 cover-sheet legend
itself (not in the general WASD manual's Exhibits A.1-D.4 above). This drawing's own
Existing/Proposed tone convention is **gray = Existing, solid black = Proposed** (not
line-weight alone) — the bilingual PDF's icons for these rows, and the recolored icons
for the Exhibit rows above, both follow that convention.

| Sheet | Category | Feature | Symbol Description |
|---|---|---|---|
| C-310 | Water & Sewer Symbols | Flushing Valve Outlet | Existing: gray valve/nozzle symbol with a small open pennant flag; Proposed: same symbol solid black with filled flag |
| C-310 | Water & Sewer Symbols | Water Main (W.M.) | Existing: gray line labeled "⌀, (MATERIAL) W.M."; Proposed: solid black bold line, same label |
| C-310 | Water & Sewer Symbols | Gate and Plug Valve | Existing: gray open bowtie/hourglass shape; Proposed: solid black filled triangle (plug) symbol |
| C-310 | Water & Sewer Symbols | Meter (Single Service) — Variant 1 | Existing: gray line with a short vertical tick; Proposed: solid black bold vertical tick |
| C-310 | Water & Sewer Symbols | Meter (Single Service) — Variant 2 | Existing: gray line with a small U-shaped bracket; Proposed: solid black bold U-shaped bracket |
| C-310 | Water & Sewer Symbols | Meter (Single Service) — Variant 3 | Existing: gray line with a small open triangle; Proposed: solid black filled triangle |
| C-310 | Water & Sewer Symbols | Tee, Cross | Existing: gray line with a small vertical tick; Proposed: solid black bold vertical tick with cross mark |
| C-310 | Water & Sewer Symbols | Fire Hydrant | Existing: gray open hydrant outline; Proposed: solid black filled hydrant symbol |
| C-310 | Water & Sewer Symbols | Sanitary Sewer (SAN.) | Existing: gray line labeled "⌀, SAN. (MATERIAL)"; Proposed: solid black bold line labeled "PROPOSED ⌀, SAN." |
| C-310 | Water & Sewer Symbols | San. Manhole | Existing: gray open circle outline; Proposed: solid black filled circle labeled "M.H." |
| C-310 | Water & Sewer Symbols | Backflow Preventer | Existing: gray double zigzag "N-H-N" assembly symbol; Proposed: same symbol solid black and bold |
| C-310 | Water & Sewer Symbols | Check Valve | Existing: gray open zigzag "N"-like valve symbol; Proposed: same zigzag solid black and bold |
| C-310 | Water & Sewer Symbols | Bend Other Than 90° | Existing: gray subtle zigzag/wave bend symbol; Proposed: same symbol solid black and bold |

## ABBREVIATIONS

### Survey / point description keys (Appendix B)

| Abbreviation | Meaning |
|---|---|
| AGV | Above Ground Vault |
| ASP | Asphalt |
| CRBBK | Back of Curb |
| BP | Base Point |
| BL | Baseline |
| BGV | Below Ground Vault |
| BNCH | Bench |
| BM | Bench Mark |
| BOL | Bollard |
| BOX | Box (unknown/generic) |
| BR | Bridge |
| BLDG | Building |
| BOH | Building Overhang |
| BSH | Bush |
| CTVB | Cable TV Box |
| CNPY | Canopy |
| CBA | Catch Basin |
| CO | Clean Out |
| CONC | Concrete |
| SLC | Concrete Slab |
| CONTROL | Control Point |
| XS | Cross Section |
| CU | Culvert |
| CRB | Curb |
| DWY | Driveway |
| DW | Dry Well |
| EP | Edge of Pavement |
| MHE | Electric Manhole |
| BOXE | Electric Box |
| PAE | Electric Panel |
| EMB | Embankment |
| FENC | Fence |
| FH | Fire Hydrant |
| FEW | Fire Well |
| FP | Flag Pole |
| FND | Found Nail and Disk |
| MHFPL | FPL Manhole |
| MGA | Gas Main |
| GASMH | Gas Manhole |
| TG | Gas Tank |
| VG | Gas Valve |
| GE | Gate |
| GRS | Grass |
| GRND | Ground |
| GRL | Guardrail |
| GUY | Guy Pole |
| GWA | Guy Wire Anchor |
| HACH | Hatch |
| HE | Hedge |
| VIC | Irrigation Control Valve |
| LS | Landscaping |
| BOXLI | Light Box |
| PLI | Light Pole |
| MBX | Mailbox |
| MP | Mile Post |
| MWL | Monitoring Well |
| OW | Overhead Wires |
| PLM | Palm Tree |
| PMR | Parking Meter |
| PAH | Patch |
| PEDX | Pedestrian Crossing |
| POST | Post |
| PP | Power Pole |
| PMP | Pump |
| POP | Pump Out Pipe |
| RRS | Railroad Crossing Signal |
| RRT | Railroad Tracks |
| RAMP | Ramp |
| CPL | Sanitary Clean Out Plug |
| LSAN | Sanitary Lateral |
| MSAN | Sanitary Manhole |
| MHSA | Sanitary Sewer Manhole |
| VS | Sewer Valve |
| SWK | Sidewalk |
| SGN | Sign |
| SPK | Sprinkler |
| STR | Stairs |
| STA | Station Point |
| STP | Stop Sign |
| MHSS | Storm Sewer Manhole |
| APX | Street Apex |
| TANT | Telemetry Antenna |
| MHT | Telephone Manhole |
| BOXT | Telephone Box |
| PNLT | Telephone Panel |
| SBT | Telephone Splice Box |
| TBM | Temporary Benchmark |
| TOPS | Top of Bank |
| TOE | Top of Slope |
| BTRS | Traffic Signal Box |
| PTRS | Traffic Signal Panel |
| PTL | Traffic Signal Pole |
| TRE | Tree |
| TRELINE | Tree Line |
| PU | Utility Pole |
| VGU | Valley Gutter |
| VST | Vent Stack |
| WAL | Wall |
| WF | Water Faucet |
| MW | Water Main |
| MHWA | Water Manhole |
| MRE / WMR | Water Meter Existing |
| VW / WV | Water Valve |
| YLI | Yard Light |

### General drafting / callout abbreviations (manual body, Tables 3.7.1 & 3.7.2, Appendix A)

| Abbreviation | Meaning |
|---|---|
| STA. | Station |
| PROP. | Proposed |
| EXIST. / EX. | Existing |
| INV. | Invert |
| INV. ELEV. | Invert Elevation |
| ELEV. | Elevation |
| RIM ELEV. | Rim Elevation |
| O/S | Offset |
| LT. / RT. | Left / Right |
| N= | Northing |
| E= | Easting |
| RP | Reference Point |
| PK | PK Nail (survey nail and disk marker) |
| W/ | With |
| W/GLANDS | With (Restraint) Glands |
| REST. | Restrained |
| M.J. | Mechanical Joint |
| F.V.O. | Flush Valve Outlet |
| A.R.V. | Air Release Valve |
| F.H. | Fire Hydrant |
| FLG. | Flange |
| ASSY. | Assembly |
| STD. | Standard |
| TYP. | Typical |
| R/W | Right of Way |
| D.I. | Ductile Iron |
| PVC | Polyvinyl Chloride (pipe material, e.g. PVC-C900) |
| F.M. | Force Main |
| W.M. | Water Main |
| SAN. / S.S. | Sanitary Sewer |
| CATV | Cable Television |
| E/U | Underground Electric Line |
| OH | Overhead (Electric) Line |
| GAS | Gas Line |
| TEL | Telephone Line |
| TRAF | Traffic Line |
| TV | Cable TV (Box) |
| C.B. | Catch Basin |
| PI | Point of Intersection |
| PC | Point of Curvature |
| PT | Point of Tangency |
| NAD83 | North American Datum 1983 (horizontal datum) |
| NGVD29 | National Geodetic Vertical Datum of 1929 (vertical datum) |
| C/C | Center/Center text justification |
| L/C | Left/Center text justification |
| WASD | (Miami-Dade County) Water and Sewer Department |
