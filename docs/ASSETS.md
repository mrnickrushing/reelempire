## Free CC0 / CC BY asset research for a 3D fishing idle tycoon (Unity URP, iPhone/iPad)

**How I checked:**
- **Sketchfab:** licence, triangle count, animation count and the textures inside the glb download all come from Sketchfab's own API (the same file you get from the Download button), not from search snippets. `glbTex 0` means the model uses plain material colours, which is normal for flat low-poly art. It does not mean the textures are missing. I did not download the Sketchfab files themselves, because that needs a login token.
- **Poly Pizza:** licence and triangle counts come from each model page. For a few models I downloaded the glb and confirmed the triangles and clip names myself.
- **Kenney:** model names come from the actual zip downloads.
- **Quaternius:** clip names come from the downloaded glb files.
- **Freesound:** links come from its CC0 licence filter. Open each sound page and confirm the licence before shipping.

---

## 1. Which look to build on

**Go low-poly stylised: flat or gradient-coloured, toy-like diorama.** It is the only direction where free assets cover the whole game in one consistent style, and it is also the look of the genre.

- **Low-poly coverage is wide and consistent.**
  - Quaternius's *Cute Fish* pack is CC0 and has 33 fish and sea animals in one style. Each is about 1.4–1.6k triangles with 6 clips: Swimming_Normal, Swimming_Fast, Swimming_Impulse, Attack, Death and **Out_Of_Water**, which fits the catch moment. The same pack has 5 rods, 6 lures and 4 dock pieces.
  - Kenney has boats, industrial buildings, conveyors, vehicles and audio.
  - KayKit has a city kit and 150+ free CC0 animations, including **fishing** and **working**.
  - All three families use material colours or a tiny palette texture, so they mix well and cost almost nothing on mobile.
- **Stylised-realistic does not hold together with free assets.**
  - The best CC0 realistic fish (ffishAsia-and-floraZia on Sketchfab) are Japanese freshwater species at 20–35k triangles each.
  - The marine species come from many different authors, with mismatched texture detail and 4K PBR maps.
  - Realistic boats and buildings would also each come from a different artist.
- **What makes it look really good is lighting and shading, not the models.** Put one shared toon/lit material (or a shared gradient palette) on everything. Add a warm sun with soft shadows, light bloom, mild colour grading and a strong stylised water shader. Fill gaps with CC BY models only where they are untextured or palette-textured.

**Kits to build on:** Quaternius (fish, docks, rods, nature, ships, characters, animation library), Kenney (watercraft, industrial, factory, cars, survival/pirate props, audio, UI) and KayKit (city and restaurant bits, character animations). All are CC0, so no credits are required.

---

## 2. Fish and sea creatures

### A. Main set: Quaternius Cute Fish pack (CC0, all animated, about 1.4–1.6k triangles, flat material colours, no textures)

Pack page: https://quaternius.com/packs/cutefish.html (FBX/OBJ/Blend). Individual glb downloads are on Poly Pizza:

| Species | Poly Pizza | Tris | Notes |
|---|---|---|---|
| Tuna | poly.pizza/m/1tycAcvSVW | 1446 | 6 clips (confirmed in glb) |
| Koi | poly.pizza/m/qyGtRmhgzl | 1494 | 6 clips (confirmed) |
| Goldfish | poly.pizza/m/qS6CgsWFAh | 1590 | Base for the **legendary golden fish**: gold metallic and emissive material, plus sparkle effects |
| Blue Goldfish | poly.pizza/m/imxZBHZ1yS | 1590 | |
| Red Snapper | poly.pizza/m/BRKfDKwPTv | 1416 | Recolour for bass, cod and perch |
| Clownfish | poly.pizza/m/769fHo3eEB | 1530 | Reef |
| Armored Catfish | poly.pizza/m/mtd9QK5yCe | 1466 | Catfish |
| Cowfish / Moorish Idol / Sunfish | VMJluStc59 / M0UX3rVvNX / UpDxLf8yue | ~1.4k | Reef and rare fish |
| Swordfish | poly.pizza/m/7hMOlBjln0 | — | Base for the **marlin**: add a sail-shaped dorsal fin and recolour |
| Lionfish, Puffer, Anglerfish, Butterfly Fish | czsz9Baw86, UKHtgpxTOk, MRjSlwCjHM, s2MkBeSzGy | — | |

The rest of the pack is listed on the GDevelop mirror: Betta, Blobfish, Black Lion Fish, Cardinal, Blue Tang, Coral Grouper, Flower Horn, Flatfish, Humphead, Goblin Shark, Mandarin, Parrot Fish, Piranha, Royal Gramma, Shark, Tang, Tetra and Worm.

**Market and crate fish (CC0, static, from Quaternius food kits):** Salmon 2BeZAlQ73h (896), Fish Mackerel uassUTYYae (888), Fish Tuna kwhaORGzCC (1040), Dead Tuna Rf86QRhYYc, Bucket of Fish G61jVPof0G (3860), plus Octopus/Salmon/Ebi Nigiri for a sushi stall.

**Salmon, trout, bass, cod, carp, pike and mackerel:** recolour the Quaternius body shapes (Red Snapper, Tuna, Koi) with per-species palettes. At tycoon camera distance this is the most consistent option.

### B. Filling the gaps (all 2–4 options checked)

| Need | Option | Link | Author / Licence | Tris | Anim | glb textures |
|---|---|---|---|---|---|---|
| Octopus | The Kraken (hand-painted, cute) | sketchfab.com/3d-models/55d900af1bfa460fb0f9304ab9fc594b | FabianvanDorst, BY | 665 | 5 clips | 4 @2048 |
| Octopus | Octopus (flat colours) | sketchfab.com/3d-models/f791893db35147a5bba081c7b04af8db | yifatshaik, BY | 786 | no | 0 (colours) |
| Octopus | Octopus | poly.pizza/m/9-b6-yqrwEe | Poly by Google, CC BY 3.0 | 754 | no | obj |
| Crab | Crab Enemy | poly.pizza/m/Gs3yfsV5lB | Quaternius, CC0 | 3144 | yes | — |
| Crab | Dancing Crab | sketchfab.com/3d-models/280863886fee409ab3c8168f07caa89f | ostapblendercg, BY | 3084 | 1 | 4 @1024 |
| Crab | Crab | poly.pizza/m/2DgM36qZW2u | Poly by Google, CC BY 3.0 | 624 | no | — |
| Lobster | Lobster | poly.pizza/m/7JIU-w5So3a | Poly by Google, CC BY 3.0 | 1588 | no | — |
| Lobster | Lobster | poly.pizza/m/30x21FMhpdn | jeremy, CC BY 3.0 | 716 | no | — |
| Lobster | Lobster (realistic, cooked) | sketchfab.com/3d-models/3ab14e5ce84141cda62c3405d8086157 | MrRat, BY | 5183 | no | 3 @2048 |
| Shrimp | Shrimp | poly.pizza/m/5HI1OYII63P | jeremy, CC BY 3.0 | 246 | no | — |
| Shrimp | Shrimp [Low Poly] | sketchfab.com/3d-models/652650059ff949b39bbed3bfc34d5e87 | Fecalis, BY | 2404 | no | 1 @512 |
| Squid | Squid | poly.pizza/m/gReSPnGdkr | Quaternius, CC0 | 1296 | no | — |
| Squid | Squid | poly.pizza/m/dr9BVsE5m_A | jeremy, CC BY 3.0 | 720 | no | — |
| Jellyfish | low-poly? jellyfish | sketchfab.com/3d-models/9dd7cfb4fb30436d82454e3d472d28b3 | n-, BY | 966 | 1 | 3 @256 |
| Jellyfish | Jellyfish | sketchfab.com/3d-models/5786a1af801b411497128c127a90305c | NZFawkes, BY | 636 | 1 | 1 @128 |
| Jellyfish | Jellyfish | poly.pizza/m/5PxIqPamrag | Poly by Google, CC BY 3.0 | 432 | no | — |
| Eel | Eel | poly.pizza/m/eRb-GgLCnA7 and /dDzthiG8nr9 | Poly by Google, CC BY 3.0 | 618 / 390 | no | — |
| Shark | Stylized Shark | sketchfab.com/3d-models/50b7659bcd4a4ca7b123a50618840332 | kasperromer, BY | 3182 | 5 | 1 @2048 |
| Shark | Sport Shark – Animated | sketchfab.com/3d-models/41066b83157540d992d6ad1998afa3db | iggy-design, BY | 1332 | 1 | 4 @1024 |
| Shark | Shark | poly.pizza/m/AyHTK3zUSG | Quaternius, CC0 | 644 | yes | — |
| Pike | Esox – Animated Fish | opengameart.org/content/esox-animated-fish | CDmir, **CC0** | 1632 | Idle, Swim slow, Swim fast | PBR maps |
| Koi (alt) | Koi Fish | sketchfab.com/3d-models/236859b809984f52b70c94fd040b9c59 | 7plus, BY | 522 | 1 | 2 @1024 |
| Golden fish (alt) | Golden Fish | sketchfab.com/3d-models/5e0cae17f48d4c53abca15eaebddc5fb | Jane_k, BY | 1958 | 2 | 3 @4096 |

Most static creatures (eel, squid, jellyfish) only need a vertex-shader sway. They don't need a rig.

### C. Realistic option (only if you change direction; none of these match each other)

| Species | Link | Author / Licence | Tris | Anim | glb textures |
|---|---|---|---|---|---|
| Rainbow Trout | sketchfab.com/3d-models/442ab10476d0486e8eb689941bc4ad6e | E.A.Cornell, BY | 18144 | 6 | 6 @2048 |
| Bull Trout | sketchfab.com/3d-models/c34b0bdf58be422ca42af1f5a6106a00 | E.A.Cornell, BY | 9116 | 3 | 3 @2048 |
| Bass | sketchfab.com/3d-models/2d664e9899ad46a787d5549ed8c42241 | simonustal, BY | 9856 | 1 | 1 @1024 |
| Tuna | sketchfab.com/3d-models/642c6515d893474a8fc8f129491efcac | dhjwdwd, BY | 7944 | 1 | 6 @4096 |
| Atlantic Mackerel (Swim/Idle/Death) | sketchfab.com/3d-models/eca6071c26fc4013b8c94a3f37a434c7 | 1shxxn, BY | 6002 | 3 | 2 @512 |
| Carp | sketchfab.com/3d-models/ef0c406d4ca84242a0464abecb7d7037 | huykhoi2407, BY | 7406 | 1 | 3 @4096 |
| Masu salmon | sketchfab.com/3d-models/ed687ab2f52843a38b6ce03a8d590675 | ffishAsia-and-floraZia, CC0 | 24893 | 1 | 1 @2048 |
| Catfish | sketchfab.com/3d-models/51e0cc7840c54790bfd674d70dfb6bab | ffishAsia-and-floraZia, CC0 | 28610 | 1 | 1 @2048 |
| Swordfish | sketchfab.com/3d-models/9f076eb541a44a96b65e53a4062db506 | Carlos.Maciel, BY | 7626 | no | 3 @2048 |
| Crystal Jellyfish | sketchfab.com/3d-models/38ac0d91213d447eb3366f615298ce8f | n-, BY | 10392 | 1 | 1 @2048 |

Smithsonian 3D CC0 fish are scanned museum specimens: very dense meshes and preserved-specimen colouring. Not worth it for this game.

---

## 3. Fishing gear and boats

| Item | Option | Link | Author / Licence | Tris | glb textures |
|---|---|---|---|---|---|
| **Boats (whole range)** | Kenney Watercraft Kit, 46 models | kenney.nl/assets/watercraft-kit | Kenney, CC0 | 118–1060 | 1 palette texture |
| | Models in it: boat-row-small/large (174), **boat-fishing-small** (237), boat-tug-a/b/c (471), boat-tow-a/b, boat-sail-a/b (386), boat-speed-a…j (156), boat-house-a…d (houseboats, 408), ship-cargo-a/b/c (1060), ship-ocean-liner, buoy (118), buoy-flag, cargo-container-a/b/c, cargo-pile-a/b | | | | |
| Ships | Quaternius Ships Pack: Boat 5UEl54KsuC (224), Sail Boat BgSZXwmm7k (1020), Small Ship W2vMzztgIi (5578), Sail Ship cIzO4MBPqI (3302), Cruise Ship yq9EKmEmfC (858) | quaternius.com/packs/ships.html | Quaternius, CC0 | | — |
| Hero trawler | Trawler (low-poly, made for a fishing game) | sketchfab.com/3d-models/421f3239115c44539686fcb62835f115 | JasperTobias, BY | 5253 | 1 @1024 |
| Fishing boat | Fishing Boat | sketchfab.com/3d-models/91aa32c513874d368f84eda14095068e | yorre.detavernier, BY | 1832 | 1 @2048 |
| Fishing boat (semi-real) | Fishing boat | sketchfab.com/3d-models/f4b38ccf5ffb46018aa7931c0e106654 | Pabooklas, BY | 7244 | 3 @4096 |
| Fishing boat | Fisher boat | sketchfab.com/3d-models/385b039e95754dca8d8c045ed0c515cf | Batuhan13, BY | 9138 | 17 @2048 |
| Yacht | Low-poly yacht (animated) | sketchfab.com/3d-models/35b5b137ca2d47698cc220a491dfff68 | dannzjs, BY | 5298 | 1 @1024 |
| Yacht | Yacht | poly.pizza/m/9znU2c-dTSR | Alex Safayan, CC BY 3.0 | 1206 | — |
| Rowboat (alt) | Victorian Row Boat & Lamp | sketchfab.com/3d-models/744a2da01dac469cbded105427bdbd11 | hate94neu, BY | 1260 | 7 @2048 |
| Rods and lures | Quaternius rods 0YAR0Lg58p (522), skqPvEIpve (308), lDlWQjn9Zg (910), 9AOHhRPHE7, aOabqWh68m; lures swjBRN0kVZ etc. | poly.pizza | Quaternius, CC0 | 300–1200 | — |
| Fishing props set | Fishing Supplies Set | sketchfab.com/3d-models/8742c7af23584443a42c24e52d25b60c | FrodoUndead, BY | 4209 | 3 @1024 |
| Nets | Fishing Net | poly.pizza/m/Z11I3AUBO9 | J-Toastie, CC BY 3.0 | 1864 | — |
| Nets | Fishing net (alpha card) | sketchfab.com/3d-models/cab12e62c8dd46cdbb6b250c2a3c1306 | alexmartire, BY | 112 | 4 @2048 |
| Crab pot | Crab Pot | sketchfab.com/3d-models/303844fbdc1d47f39f11bcc3a865afcd | jazz-the-giraffe, BY | 1220 | 3 @1024 |
| Fish crate | [LOW POLY] Fish Crate | sketchfab.com/3d-models/59c85ab9efec436fb017b4c7805f9770 | mangodemon, BY | 294 | 2 @512 |
| Buoys | Kenney buoy / buoy-flag; Inflatable Buoy sketchfab.com/3d-models/281e203b2b3a4461ba2661c82cd75a44 (ApprenticeRaccoon, BY, 800 tris, flat colours); life preserver poly.pizza/m/7n1vrlFN0GH (Poly by Google, CC BY) | | | | |
| Crates, barrels, bucket | Kenney Survival Kit (barrel, barrel-open, bucket, box, box-large, **fish**, **fish-large**, campfire-fishing-stand, signpost); Kenney Pirate Kit (barrel, crate, crate-bottles, chest) | kenney.nl/assets/survival-kit, /pirate-kit | Kenney, CC0 | small | 1 palette |
| Pallet, container | Pallet poly.pizza/m/MwOFAOgOhg (240); Container Red /vzzCNUB6Zn | | Quaternius, CC0 | | |
| Rope | Cable poly.pizza/m/aoNcGMnNiG (Quaternius, CC0); Rope /dkNzi6oDQj (J-Toastie, CC BY) | | | | |
| Lanterns | Lantern poly.pizza/m/CtHBJ1ufeW (264); Hanging Lantern /3jzk3YShv1 (472); Post Lantern /ZSQ65S4lEu | | Kay Lousberg (KayKit), CC0 | | |

---

## 4. Docks, harbour and buildings

| Item | Option | Link | Author / Licence | Tris |
|---|---|---|---|---|
| Pier and dock pieces | Dock Long bN9Oz3niNm (1688), Dock Wide XndOrGa7rN, Dock Stairs IaUtCxMAxg (628), Dock XViKoBh2UN (388), Docks F7twMHWPXY (7438), Port 4sE6lmhGPF (6672) | poly.pizza | Quaternius, CC0 | |
| Dock pieces | Kenney Pirate Kit: structure-platform-dock(-small), platform-planks, structure, structure-roof; Nature Kit bridge_wood* | kenney.nl | Kenney, CC0 | |
| Dock | Simple Wooden Pier or Dock | sketchfab.com/3d-models/5bf117cb734a46559d10971c3098165c | jungle_jim, BY (8 tex @1024) | 6300 |
| Fish market stall | Market Stand DGIM5HGISb (1548), hts7l0NZxW (1132), Market Stalls jSqGy8V0MQ (3896) | poly.pizza | Quaternius, CC0 | |
| Shops | Kenney City Kit Commercial: building-a…n, detail-awning(-wide), detail-parasol-a/b | kenney.nl/assets/city-kit-commercial | Kenney, CC0 | |
| Shops and streets | KayKit City Builder Bits (free tier 32+ models, cars included, 1024 gradient atlas) | kaylousberg.itch.io/city-builder-bits | Kay Lousberg, CC0 | |
| Warehouse / cannery | Kenney City Kit Industrial: building-a…t (700–2000), chimney-*, detail-tank(-large), water-tower, shipping-container-a/b/c | kenney.nl/assets/city-kit-industrial | Kenney, CC0 | |
| Processing line | Kenney Factory Kit: conveyor-* (dozens of variants), hopper-*, crane, crane-lift, catwalk-*, cog-*, box-* | kenney.nl/assets/factory-kit | Kenney, CC0 | |
| Lighthouse | Lighthouse, Low Poly (flat colours) | sketchfab.com/3d-models/8cbfa6920c8e45a9a9abf37adb74dbf8 | flaremedia, BY | 1029 |
| Lighthouse | Light House | poly.pizza/m/KRebJXIRb8 | MaverickFX, CC0 | 824 |
| Lighthouse | Village_lighthouse | sketchfab.com/3d-models/3eac61535005406885ad708b4eb375e9 | jakekieferwaddington, BY (14 tex) | 4430 |
| Boathouse / cabin | The Old Boathouse – Layout 1 | sketchfab.com/3d-models/c54c3f1529364f618c79d6323e669c08 | vonka, BY (3 @1024) | 3952 |
| Boathouse / cabin | Angler's house | sketchfab.com/3d-models/778b137d2cc148e6a2b362159bcd5213 | SAIKIRAN.PANDHINA, BY (4 @4096) | 6252 |
| Boathouse / cabin | Kenney watercraft boat-house-a…d; Quaternius Medieval Village or Farm Buildings packs for cabins | | CC0 | |
| Crane | Kenney factory crane; Crane poly.pizza/m/17aBNzIsVg | cg_world, CC0 | 5982 |
| Crane | Skipskranen (wooden harbour crane, 1798) | sketchfab.com/3d-models/bac9cbe7113c447b8705c6e894744ff3 | Tidvis, BY (6 @2048) | 6920 |
| Forklift | Forklift poly.pizza/m/DTQBuenKJY; Pallet Truck /snDq7v3GRO | KolosStudios, CC BY 3.0 | 1590 / 788 |
| Forklift | Forklift_TS | sketchfab.com/3d-models/20d69358f05d4d4d8c8447b013d11a97 | DanixsDesigner4774, BY (4 @512) | 948 |
| Delivery truck / van | Kenney Car Kit: delivery (2476), delivery-flat, truck (2082), truck-flat, van, garbage-truck, tractor | kenney.nl/assets/car-kit | Kenney, CC0 | |
| Delivery truck | Truck poly.pizza/m/cXw6oiFtZ8 (5546); Sushi Truck /OgrROxABGT (14442) | | Quaternius, CC0 | |

---

## 5. Environment

**Water:** there is no good free water texture. Write it as a URP shader:
- colour ramp from shallow to deep, driven by depth;
- two scrolling normal maps;
- foam where the water meets objects (depth intersection), broken up with noise;
- vertex waves;
- a toon-style highlight.

The castaway project's `Water.shader` already does refraction, absorption and caustics, so it can be the starting point.

**Ground textures:** use these only if you add surface detail; flat colours suit the low-poly look better.
- Poly Haven (CC0): `aerial_beach_01`, `coast_sand_01`, `sand_01`, `aerial_rocks_02`, `rock_face_03`, `aerial_grass_rock`, `snow_02`.
- ambientCG (CC0): `Ice002`, `Ice003`, `Snow014`.

| Item | Option | Licence | Tris |
|---|---|---|---|
| Trees, rocks, plants | Quaternius Stylized Nature MegaKit: 116 models (40 trees, 35 plants, 27 rocks). The free Standard tier is 60–70% of the pack. quaternius.com/packs/stylizednaturemegakit.html | CC0 | |
| Palms | Palm Tree poly.pizza/m/A6cKJYFsIb (3134), /DsrrAYmucG (3208), Palm Trees /VYslw9DEi6 (1920); Kenney Pirate Kit palm-bend/-straight/-detailed-* | CC0 | |
| Pines | Pine poly.pizza/m/igSu0cPoBz (1646), /79gmlLnweB, Pine Trees /oYtDty0fR6 | CC0 (Quaternius) | |
| Rocks, cliffs, beach | Kenney Nature Kit cliff_* in rock and stone variants (modular, waterfall pieces too); Pirate Kit rocks-sand-a/b/c, patch-sand(-foliage); Survival Kit rock-sand-* | CC0 | |
| Lake shore plants | Lilypad poly.pizza/m/TI6ukUlsLh (Quaternius, CC0, 372); Cattail /9uT74BMpRrl, Water lilies /birD7XV2A44 (Poly by Google, CC BY) | | |
| Coral and kelp | Coral Reef Set3 poly.pizza/m/UyswwdHFiL (MiniPoly, CC BY, 2846); Coral /4KUXdtDdgHR (817); Kelp /4cFllH6Iazk (Poly by Google, CC BY) | | |
| Ice floes | ice floes sketchfab.com/3d-models/f6542036575c4945b40ffe765b1f537c (JamesDadema, BY, 134); Iceberg poly.pizza/m/8sqTOfIjWml (236) and Iceberg 1 /edgjUwbVVtl (S. Paul Michael, 1250), both CC BY | | |
| Seagulls | Low-Poly Seagull Animated sketchfab.com/3d-models/bff3cd3d48e249e8b5afa7416c6c39f8 (jamiemcfarlane, BY, 1078, flat colours); Low-Poly Seagull rigged /985024328902444c8270c0f09acc897e (simonaskLDE, BY, 580); Seagulls animated /73aed843190a4dfda55f2b65cc0f8d63 (deathcow, BY, 808) | | |
| Pelicans | Brown pelican poly.pizza/m/2oAiRa4fave (1026); American White Pelican /2J2HY1oq8l2 (1128). Both Poly by Google, CC BY, static: flap them in a shader or with a simple rig. | | |

---

## 6. Characters and animation

| Option | Details | Licence |
|---|---|---|
| **Quaternius Ultimate Animated Character / Modular Men and Women packs** | Worker poly.pizza/m/Yg2bQZO6Hj (5240 tris in the glb, 24 clips: Idle, Idle_Neutral, Walk, Run, Interact, Wave, HitRecieve, Death…). Also Farmer /7pn3R6hPvE (fisherman after recolour and a hat), Business Man /JFrLIKqvCH and Man in Suit /mQnGoME1ez (managers), Casual /kZ3DmIoGip, Woman Casual /jpKRgGDxhk (customers) | CC0. Use the CC0 Worker upload; there is also a "CC BY" duplicate of it on Poly Pizza. |
| Kenney Mini Characters | character-worker (893 tris), male a–g, female a–f. 32 built-in clips: idle, walk, sprint, sit, drive, pick-up, interact-left/right, holding-both, emote-yes/no… | CC0 |
| **KayKit Character Animations** | 150+ free clips. The "Tool" set includes **fishing**, hammering, digging and generic working. Made for KayKit Rig_Medium; use them on other characters through Unity Humanoid retargeting. kaylousberg.itch.io/kaykit-character-animations | CC0 |
| Quaternius Universal Animation Library 1, free tier | 43 clips (the castaway project already has this file): Idle, Walk, Jog, Sprint, Interact, PickUp_Table, Fixing_Kneeling, Push_Loop, Driving_Loop, Sitting_*, Idle_Talking, Dance, Swim | CC0 |
| Quaternius Universal Animation Library 2, free tier | 42 clips, including HARVEST, IDLE_CARRY, **WALK_CARRY**, IDLE_LANTERN, IDLE_RAIL, OVERHAND_THROW (works as a cast), WAVE, CHEST_OPEN. A "fishing" category exists, but none of the named free clips is a fishing clip; it seems to be in the $14.99 Source tier (not confirmed). quaternius.itch.io/universal-animation-library-2 | CC0 |

For casting and reeling, use KayKit's fishing clips retargeted, or OVERHAND_THROW plus a rod held in the hand that bends in code.

---

## 7. UI: icons, fonts and sounds

**Icons**
- Best option: render item and fish icons from the 3D models themselves, like `ModelIcons` in castaway, so they match the game.
- Kenney Game Icons, Board Game Icons, UI Pack, UI Pack Adventure and Emotes Pack (kenney.nl/assets/…), all CC0.
- game-icons.net: about 4,000 SVG icons, CC BY 3.0. Credit each icon's author (Lorc, Delapouite, …).

**Fonts** (licence folders checked in the google/fonts repository)
- OFL: **Lilita One** (titles and numbers), **Fredoka** (UI), Titan One, Baloo 2, Carter One, Bangers, Nunito, Rubik (small text).
- Luckiest Guy and Chewy are Apache 2.0: permissive, but not OFL.
- Kenney Fonts are CC0.

**Kenney audio (CC0, file names from the zips)**
- Interface Sounds: click_001–005, confirmation_001–004, select_*, toggle_*, drop_*.
- UI Audio: click1–5, switch1–38.
- Casino Audio: chips-stack-1–6 and chips-handle-* make good coin-stack sounds.
- RPG Audio: handleCoins, handleCoins2, creak*, chop, footstep00–09.
- Impact Sounds: footstep_wood_*, impactPlank_*.
- Music Jingles: **jingles_STEEL00–16** (steel drum, good for level-ups and harbour), PIZZI, HIT, SAX.

**Freesound CC0** (freesound.org/people/USER/sounds/ID/)

| Sound | Options |
|---|---|
| Cash register | Zott820/209578 "Cash Register Purchase"; sidequesting/542568; deleted_user_96253/351304 "Cha Ching" |
| Reel | j1987/95564; esperri/118973 "FishingRod Winding"; tosha73/509902 "Spinning reel"; fennelliott/347840 |
| Cast | mwchristian95/725426 "Fishing Rod Cast – Swoosh"; mwchristian95/725425 "Reel Cast W/ Splash"; BranndyBottle/464697 |
| Splash | qubodup/442773 "Big Water Splash"; petenice/9508 |
| Seagulls | Soojay/462462 (clean wildtrack); Lydmakeren/510917; steaq/263786 (single call) |
| Waves | pulswelle/339517; Noted451/531015; straget/412308 |
| Coins | Jace/17502; rolandseer/443334; steaq/387232 "Badge Coin Win" |
| Level-up | qubodup/442943; suntemple/253177; Jofae/368651 |
| Boat engine | AugustSandberg/264864 "Marine diesel"; Bansemer/35499 "Diesel motor on Fishing Boat" |
| Harbour ambience | clif_creates/254130; L.Finck/636556; craigsmith/438158 "Harbor Docks Activity" |

---

## 8. Excluded or flagged

- **Alstra Infinite "Boats"** (itch.io): uses its own "Free Asset License", not Creative Commons.
- **"Stylized fishing boat"** (trivial.cat) is based on Shane Hunt's artwork. **"Hand-Painted Fish Market Stall"** (duckcracker02) is based on Natalia Danilchanka's concept. Both are CC BY, but they copy another artist's design.
- **"Great White Shark"** (SealifeFan3): the description embeds a Steam image, so it may be ripped from a game.
- **"Tree-Stump Chest [Free for non-commercial]"**: the title contradicts its CC BY tag.
- **Fan art and brands:** "Marlin (Finding Nemo)", Squidward and "Jelly Fish (Spongebob)", "Swordfish II", "Big Mouth Billy Bass", "Shrimpo (Dandy's World)", "Magikarp", and any model titled after COD.
- **Mixed licences:** some E.A.Cornell and ffishAsia models are BY-SA or CC BY (not CC0); check each one. ffishAsia's freshwater goby collection and the Fab "Japanese River Fish Pack" are CC BY 4.0.
- **CC BY credits needed:** every Poly by Google model (CC BY 3.0, credit the author) and every CC BY Sketchfab model.

---

## 9. Starting set: 44 assets

The look this implies: bright, saturated toy-diorama low-poly with flat or gradient materials re-coloured to one shared palette. Lighting is a warm sun, soft shadows, light bloom and ACES tonemapping, with stylised foamy turquoise water. Chunky silhouettes stay readable at the three-quarter tycoon camera. About 1.5k triangles per fish, under 1k for props and 2–6k for boats and buildings, so a full harbour stays well under 300k triangles.

**Fish (Quaternius CC0, animated):**
1. Tuna 1tycAcvSVW
2. Koi qyGtRmhgzl
3. Goldfish qS6CgsWFAh (legendary golden fish)
4. Red Snapper BRKfDKwPTv (also recoloured as bass, cod, salmon and trout)
5. Clownfish 769fHo3eEB
6. Armored Catfish mtd9QK5yCe
7. Swordfish 7hMOlBjln0 (also the marlin edit)
8. Puffer UKHtgpxTOk
9. Anglerfish MRjSlwCjHM
10. Lionfish czsz9Baw86

**More sea creatures:**

11. Shark AyHTK3zUSG (CC0)
12. Crab Enemy Gs3yfsV5lB (CC0)
13. Squid gReSPnGdkr (CC0)
14. Esox pike, OpenGameArt (CC0)
15. The Kraken octopus, Sketchfab 55d900af (CC BY)
16. low-poly jellyfish 9dd7cfb4 (CC BY)
17. Lobster poly.pizza/m/7JIU-w5So3a (CC BY)
18. Shrimp 5HI1OYII63P (CC BY)
19. Eel eRb-GgLCnA7 (CC BY)

**Gear:**

20. Quaternius fishing rods (5) and lures (6)
21. Bucket of Fish G61jVPof0G
22. Kenney Survival Kit: barrel, bucket, box, fish, fish-large
23. Crab Pot 303844fb (CC BY)
24. Fish Crate 59c85ab9 (CC BY)
25. Fishing Net Z11I3AUBO9 (CC BY)
26. KayKit lanterns CtHBJ1ufeW and 3jzk3YShv1

**Boats:**

27. Kenney Watercraft: boat-row-small, boat-fishing-small, boat-tug-a, boat-sail-a, boat-speed-a, ship-cargo-a, buoy, buoy-flag
28. Trawler 421f3239 (CC BY, the hero trawler)
29. Low-poly yacht 35b5b137 (CC BY)

**Harbour:**

30. Quaternius Dock Long, Dock Wide, Dock Stairs, Port
31. Quaternius Market Stand DGIM5HGISb
32. Kenney City Kit Industrial building-a…t (warehouse and cannery shells) with chimney and detail-tank
33. Kenney Factory Kit: conveyor, conveyor-corner, hopper-round, crane
34. Lighthouse KRebJXIRb8 (CC0)
35. Kenney Car Kit: delivery, truck, van
36. Forklift DTQBuenKJY (CC BY)
37. KayKit City Builder Bits (town edge)

**Nature:**

38. Quaternius Stylized Nature MegaKit (free tier)
39. Palm Tree A6cKJYFsIb and Pine igSu0cPoBz
40. Kenney Nature Kit cliff_* and Pirate Kit rocks-sand-*
41. Low-Poly Seagull Animated bff3cd3d (CC BY)

**People and UI:**

42. Quaternius Worker, Farmer, Business Man, Casual and Woman Casual
43. KayKit Character Animations (fishing and working) plus UAL1 and UAL2 free tiers, retargeted
44. Lilita One and Fredoka (OFL); Kenney Interface Sounds, Casino Audio, RPG Audio and Music Jingles (steel drum); the Freesound CC0 cast, reel, splash, gull, wave and cash-register picks above

Only items 15–19, 23–25, 28–29, 36 and 41 need credits.

Sources: [quaternius.com](https://quaternius.com/), [Cute Fish pack contents (GDevelop)](https://gdevelop.io/asset-store/free/3d-cute-fish-3d-cute-fish), [UAL2 on itch.io](https://quaternius.itch.io/universal-animation-library-2), [KayKit Character Animations](https://kaylousberg.itch.io/kaykit-character-animations), [KayKit City Builder Bits](https://kaylousberg.itch.io/city-builder-bits), [Kenney assets](https://kenney.nl/assets), [Poly Pizza](https://poly.pizza/), [Sketchfab Data API](https://api.sketchfab.com/v3/search), [Freesound](https://freesound.org/), [Poly Haven](https://polyhaven.com/), [ambientCG](https://ambientcg.com/), [OpenGameArt Esox](https://opengameart.org/content/esox-animated-fish), [Alstra Infinite Boats](https://alstrainfinite.itch.io/boats).
