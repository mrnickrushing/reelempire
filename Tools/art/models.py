#!/usr/bin/env python3
"""Converts the free kit models the game uses into Assets/Resources/Models/<id>.fbx (vertex-coloured, one unit on
their longest side, or one unit tall): python3 Tools/art/models.py [id...]   (fetch kits with kenney.py first:
watercraft-kit car-kit city-kit-industrial mini-characters pirate-kit survival-kit nature-kit)"""
import os, subprocess, sys
ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
F = os.path.join(ROOT, "art-src", "free")
G = "/Models/GLB format/"
WAT, CAR, IND, MINI, PIR, SUR, NAT = "watercraft-kit" + G, "car-kit" + G, "city-kit-industrial" + G, "mini-characters" + G, "pirate-kit" + G, "survival-kit" + G, "nature-kit/Models/GLTF format/"
MODELS = {  # id: (source, "size" | "height"[, pose, extra args...])
    "truck": (CAR + "delivery.glb", "size"), "van": (CAR + "van.glb", "size"),
    "hall": (IND + "building-m.glb", "size"), "market": (IND + "building-i.glb", "size"), "shed": (IND + "building-h.glb", "size"),
    "tank": (IND + "detail-tank.glb", "size"), "container": (IND + "shipping-container-a.glb", "size"),
    "pier": (PIR + "structure-platform-dock.glb", "size"), "pier_small": (PIR + "structure-platform-dock-small.glb", "size"),
    "stall": (PIR + "structure-roof.glb", "height"), "barrel": (PIR + "barrel.glb", "height"), "crate": (PIR + "crate.glb", "size"), "chest": (PIR + "chest.glb", "size"),
    "palm_a": (PIR + "palm-detailed-straight.glb", "height"), "palm_b": (PIR + "palm-detailed-bend.glb", "height"), "flag": (PIR + "flag-high-pennant.glb", "height"),
    "rocks_a": (PIR + "rocks-a.glb", "size"), "rocks_b": (PIR + "rocks-b.glb", "size"), "rocks_sand": (PIR + "rocks-sand-a.glb", "size"), "tower": (PIR + "tower-watch.glb", "height"),
    "box": (SUR + "box.glb", "size"), "box_open": (SUR + "box-large-open.glb", "size"), "bucket": (SUR + "bucket.glb", "height"), "fish": (SUR + "fish-large.glb", "size"), "signpost": (SUR + "signpost.glb", "height"),
    "tree_a": (NAT + "tree_default.glb", "height"), "tree_b": (NAT + "tree_detailed.glb", "height"), "tree_c": (NAT + "tree_cone.glb", "height"), "tree_d": (NAT + "tree_oak.glb", "height"),
    "tree_pine": (NAT + "tree_pineRoundA.glb", "height"), "tree_pine_b": (NAT + "tree_pineTallA.glb", "height"), "bush": (NAT + "plant_bushLarge.glb", "size"), "bush_b": (NAT + "plant_bushDetailed.glb", "size"),
    "rock_a": (NAT + "rock_largeA.glb", "size"), "rock_b": (NAT + "rock_largeC.glb", "size"), "rock_c": (NAT + "rock_tallB.glb", "size"), "rock_d": (NAT + "stone_largeB.glb", "size"),
    "lily": (NAT + "lily_large.glb", "size"), "log_stack": (NAT + "log_stackLarge.glb", "size"), "flowers": (NAT + "flower_yellowA.glb", "size"), "grass": (NAT + "grass_large.glb", "size"),
    "buoy": (WAT + "buoy-flag.glb", "height"),
    "fence": (NAT + "fence_simple.glb", "size"), "wheat": (NAT + "crops_wheatStageB.glb", "height"), "corn": (NAT + "crops_cornStageD.glb", "height"),
    "flowers_red": (NAT + "flower_redA.glb", "size"), "flowers_purple": (NAT + "flower_purpleA.glb", "size"), "mushrooms": (NAT + "mushroom_redGroup.glb", "size"),
    "stump": (NAT + "stump_old.glb", "size"), "tent": (NAT + "tent_detailedOpen.glb", "size"), "campfire": (NAT + "campfire_stones.glb", "size"), "canoe": (NAT + "canoe.glb", "size"),
}
# Single models from poly.pizza (Quaternius and MaverickFX, CC0), fetched by props.py.
P = "props/"
for mid, mode in (("big_barn", "size"), ("barn", "size"), ("open_barn", "size"), ("small_barn", "size"), ("silo_house", "height"), ("market_stalls", "size"),
                  ("market_stalls_b", "size"), ("market_stand", "height"), ("market_stand_b", "height"), ("dock_long", "size"), ("dock", "size"),
                  ("storage_hut", "size"), ("bucket_fish", "size"), ("lighthouse", "height"), ("container_red", "size"), ("pallet", "size")):
    MODELS[mid] = (P + mid + ".glb", mode)
BOATS = ["boat-row-small", "boat-row-large", "boat-fishing-small", "boat-sail-a", "boat-tug-a", "boat-tow-a", "boat-tug-c", "ship-cargo-a"]
for i, b in enumerate(BOATS): MODELS["boat_%d" % i] = (WAT + b + ".glb", "size")
# The dock hands: three kit people, each frozen in four walking strides and standing.
for i, k in enumerate(["character-male-a", "character-female-b", "character-male-d"]):
    for tag, pose in (("w0", "walk:f0"), ("w1", "walk:f0.25"), ("w2", "walk:f0.5"), ("w3", "walk:f0.75"), ("idle", "idle:f0")):
        MODELS["hand_%d_%s" % (i, tag)] = (MINI + k + ".glb", "height", pose)
out = os.path.join(ROOT, "Assets", "Resources", "Models")
os.makedirs(out, exist_ok=True)
for mid in (sys.argv[1:] or list(MODELS)):
    src, mode = MODELS[mid][0], MODELS[mid][1]
    extra = ((["--pose", MODELS[mid][2]] if MODELS[mid][2] else []) + list(MODELS[mid][3:])) if len(MODELS[mid]) > 2 else []
    r = subprocess.run(["blender", "-b", "--factory-startup", "-P", os.path.join(ROOT, "Tools", "blender", "model_import.py"), "--", os.path.join(F, src), os.path.join(out, mid + ".fbx"),
                        "--" + mode, "1"] + extra, capture_output=True, text=True)
    line = [l for l in r.stdout.splitlines() if "[free]" in l]
    print(mid, line[0].split("faces")[1] if line else "FAILED " + (r.stdout + r.stderr)[-300:])
