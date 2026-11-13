#!/usr/bin/env python3
"""The fish: free models (Quaternius's Cute Fish pack and others, CC0; a few CC BY from poly.pizza) fetched into
art-src/free/fish, a picture of each rendered by Blender, and one picture per species made from its model's picture
with the species' own colouring: Assets/Resources/UI/Species/<species id>.png (dots as underscores).

  python3 -I Tools/art/fish.py fetch | render | species | all
"""
import colorsys, json, os, re, subprocess, sys, time, urllib.request
ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SRC = os.path.join(ROOT, "art-src", "free", "fish")
OUT = os.path.join(ROOT, "Assets", "Resources", "UI", "Species")
UA = {"User-Agent": "Mozilla/5.0 asset-pipeline"}
MODELS = {  # base name: poly.pizza id
    "tuna": "1tycAcvSVW", "koi": "qyGtRmhgzl", "goldfish": "qS6CgsWFAh", "snapper": "BRKfDKwPTv", "clown": "769fHo3eEB", "catfish": "mtd9QK5yCe",
    "cowfish": "VMJluStc59", "idol": "M0UX3rVvNX", "sunfish": "UpDxLf8yue", "swordfish": "7hMOlBjln0", "lionfish": "czsz9Baw86", "puffer": "UKHtgpxTOk",
    "angler": "MRjSlwCjHM", "butterfly": "s2MkBeSzGy", "shark": "sZR8AMLMz5", "goblin": "JQrBevTzgD", "piranha": "F7bCnF1BFf", "tang": "D36zsP5ESu",
    "bluetang": "TQaMo8GTJl", "turbot": "E8NjhhdvSU", "flatfish": "X5EjEA6fMF", "grouper": "Q2GNw9TRzF", "flowerhorn": "ExfO0SGWmf", "gramma": "e8u2OQ9Quz",
    "mandarin": "h6M5zlF5Yx", "tetra": "l6AhogdZHe", "cardinal": "YyZyVhg2Jq", "blobfish": "7Jh8vsARfN", "crab": "Gs3yfsV5lB", "squid": "gReSPnGdkr",
    "octopus": "9-b6-yqrwEe", "lobster": "7JIU-w5So3a", "shrimp": "5HI1OYII63P", "eel": "eRb-GgLCnA7",
}
VIEW = {"crab": 20, "octopus": 20, "lobster": 60, "shrimp": 60, "squid": 60}       # yaw for the picture; fish default below
# Species -> (model, hue shift in degrees, saturation x, brightness x)
S = {
    "lake": {"Bluegill": ("tang", 150, .8, 1), "Yellow Perch": ("snapper", 45, 1, 1.05), "Roach": ("tetra", 0, .5, 1.05), "Crappie": ("butterfly", 200, .3, .95), "Bream": ("idol", 20, .35, 1),
             "Tench": ("grouper", 90, .7, .8), "Rudd": ("goldfish", -15, .9, 1), "Largemouth Bass": ("grouper", 110, .8, .85), "Common Carp": ("koi", 30, .5, .8), "Walleye": ("snapper", 60, .5, .8),
             "Channel Catfish": ("catfish", 0, .6, .9), "Northern Pike": ("swordfish", 110, .7, .8), "Mirror Carp": ("koi", 25, .8, 1), "Burbot": ("catfish", 30, .5, .7), "Muskellunge": ("swordfish", 80, .6, .7), "Golden Koi": ("koi", 20, 1.4, 1.25)},
    "river": {"Brown Trout": ("tuna", 190, .6, .85), "Chub": ("tetra", 30, .4, .9), "Dace": ("tetra", 180, .4, 1.1), "Grayling": ("tuna", 240, .3, 1), "Barbel": ("catfish", 20, .7, 1), "Smallmouth Bass": ("grouper", 70, .7, .8),
              "Minnow": ("tetra", 0, 1, 1), "Rainbow Trout": ("tuna", 120, .9, 1.05), "River Eel": ("eel", 0, 1, 1), "Zander": ("snapper", 80, .4, .9), "Brook Char": ("tuna", 170, 1, .9), "Atlantic Salmon": ("tuna", 150, .5, 1.1),
              "Steelhead": ("tuna", 0, .5, 1.1), "Wels Catfish": ("catfish", -20, .5, .6), "White Sturgeon": ("goblin", 0, .4, 1), "Ghost Salmon": ("tuna", 0, .1, 1.5)},
    "coast": {"Mackerel": ("tuna", -20, 1, 1), "Herring": ("tetra", 200, .3, 1.15), "Pollock": ("snapper", 110, .4, .9), "Whiting": ("snapper", 200, .2, 1.15), "Flounder": ("flatfish", 0, 1, 1), "Shore Crab": ("crab", 100, .7, .8),
              "Sardine": ("tetra", 210, .5, 1.1), "Sea Bass": ("grouper", 200, .3, 1), "Atlantic Cod": ("grouper", 50, .5, .9), "Brown Crab": ("crab", 15, .8, .8), "Squid": ("squid", 0, 1, 1), "Lobster": ("lobster", 0, 1, 1),
              "Turbot": ("turbot", 0, 1, 1), "Conger Eel": ("eel", 180, .5, .7), "Halibut": ("flatfish", 60, .5, .8), "Blue Lobster": ("lobster", 215, 1.2, 1)},
    "reef": {"Clownfish": ("clown", 0, 1, 1), "Blue Tang": ("bluetang", 0, 1, 1), "Butterflyfish": ("butterfly", 0, 1, 1), "Parrotfish": ("grouper", 150, 1.1, 1.1), "Snapper": ("snapper", 0, 1, 1), "Damselfish": ("tang", 200, 1, 1),
             "Wrasse": ("gramma", 0, 1, 1), "Angelfish": ("idol", 0, 1, 1), "Grouper": ("grouper", 0, 1, 1), "Reef Octopus": ("octopus", 0, 1, 1), "Triggerfish": ("cowfish", 0, 1, 1), "Lionfish": ("lionfish", 0, 1, 1),
             "Barracuda": ("swordfish", 180, .3, 1.1), "Moray Eel": ("eel", 60, 1, .9), "Giant Trevally": ("sunfish", 0, .4, 1.1), "Lantern Seahorse": ("mandarin", 0, 1, 1.1)},
    "deep": {"Skipjack Tuna": ("tuna", 0, 1, 1), "Mahi-Mahi": ("flowerhorn", 60, 1, 1.1), "Bonito": ("tuna", 30, .7, .95), "Flying Fish": ("cardinal", 200, .8, 1.1), "Albacore": ("tuna", -10, .5, 1.15), "Amberjack": ("grouper", 30, .8, 1.1),
             "Wahoo": ("swordfish", 20, .7, 1), "Yellowfin Tuna": ("tuna", 200, 1.1, 1.1), "Sailfish": ("swordfish", 0, 1, 1), "Giant Squid": ("squid", -20, 1.2, .9), "Mako Shark": ("shark", 0, 1, 1), "Swordfish": ("swordfish", -30, .6, .8),
             "Bluefin Tuna": ("tuna", 10, 1.3, .8), "Oarfish": ("eel", 200, .4, 1.3), "Blue Marlin": ("swordfish", 10, 1.4, .9), "Anglerfish King": ("angler", 0, 1, 1)},
    "arctic": {"Arctic Char": ("tuna", 150, 1.1, 1), "Capelin": ("tetra", 190, .3, 1.2), "Polar Cod": ("snapper", 190, .2, 1.1), "Lumpfish": ("puffer", 0, 1, 1), "Snow Crab": ("crab", 20, .4, 1.3), "Redfish": ("snapper", -10, 1.2, 1),
               "Northern Shrimp": ("shrimp", 0, 1, 1), "Greenland Halibut": ("flatfish", 200, .3, .7), "King Crab": ("crab", 0, 1.2, 1), "Wolffish": ("blobfish", 180, .5, .8), "Arctic Skate": ("turbot", 190, .4, 1),
               "Sablefish": ("grouper", 220, .3, .5), "Giant Pacific Octopus": ("octopus", -20, 1.2, .9), "Toothfish": ("piranha", 200, .3, .8), "Greenland Shark": ("goblin", 200, .3, .7), "Aurora Char": ("tuna", 100, 1.5, 1.3)},
}


def fetch():
    os.makedirs(SRC, exist_ok=True)
    man_path = os.path.join(SRC, "manifest.json")
    man = json.load(open(man_path)) if os.path.exists(man_path) else {}
    for name, pid in MODELS.items():
        dst = os.path.join(SRC, name + ".glb")
        if os.path.exists(dst) and name in man: continue
        page = urllib.request.urlopen(urllib.request.Request("https://poly.pizza/m/" + pid, headers=UA), timeout=60).read().decode("utf-8", "ignore")
        glb = re.search(r"https://static\.poly\.pizza/[^\"]+\.glb", page).group(0)
        m = re.search(r'"model":(\{.*?"Animated":(?:true|false)\})', page).group(1)
        g = lambda k: (re.search(r'"%s":"?([^",}]*)' % k, m) or [None, None])[1]
        lic = g("Licence") or ""
        if not (lic.startswith("CC0") or lic.startswith("CC-BY") or lic.startswith("CC BY")) or "NC" in lic or "SA" in lic or "ND" in lic:
            print("REFUSED", name, lic); continue
        open(dst, "wb").write(urllib.request.urlopen(urllib.request.Request(glb, headers=UA), timeout=120).read())
        man[name] = {"id": pid, "title": g("Title"), "author": (re.search(r'"Username":"([^"]*)"', m) or [None, ""])[1], "licence": lic}
        json.dump(man, open(man_path, "w"), indent=1)
        print("fish", name, man[name]); time.sleep(0.4)


def render(only=None):
    jobs = [dict(name=n, files=[os.path.join(SRC, n + ".glb")], out=os.path.join(SRC, "pic", n + ".png"), yaw=VIEW.get(n, 125), pitch=12)
            for n in MODELS if (not only or n in only) and os.path.exists(os.path.join(SRC, n + ".glb"))]
    os.makedirs(os.path.join(SRC, "pic"), exist_ok=True)
    jf = os.path.join(SRC, "jobs.json"); json.dump(jobs, open(jf, "w"))
    r = subprocess.run(["blender", "-b", "--factory-startup", "-P", os.path.join(ROOT, "Tools", "blender", "icon_render.py"), "--", jf], capture_output=True, text=True)
    print(len([j for j in jobs if os.path.exists(j["out"])]), "of", len(jobs), "pictures", (r.stdout + r.stderr)[-300:] if r.returncode else "")


def species():
    from PIL import Image
    os.makedirs(OUT, exist_ok=True)
    n = 0
    for harbour, table in S.items():
        for name, (base, hue, sat, val) in table.items():
            src = os.path.join(SRC, "pic", base + ".png")
            if not os.path.exists(src): print("no picture for", base); continue
            im = Image.open(src).convert("RGBA")
            box = im.getbbox()
            if box: im = im.crop(box)
            side = max(im.size); sq = Image.new("RGBA", (side, side), (0, 0, 0, 0)); sq.paste(im, ((side - im.width) // 2, (side - im.height) // 2)); im = sq.resize((256, 256), Image.LANCZOS)
            if True:
                px = im.load()
                for y in range(256):
                    for x in range(256):
                        r, g, b, a = px[x, y]
                        if a == 0: continue
                        h, s, v = colorsys.rgb_to_hsv(r / 255, g / 255, b / 255)
                        r2, g2, b2 = colorsys.hsv_to_rgb((h + hue / 360) % 1, min(1, s * sat * 1.3), min(1, v * val * 1.12))
                        px[x, y] = (int(r2 * 255), int(g2 * 255), int(b2 * 255), a)
            sid = harbour + "_" + name.lower().replace(" ", "_").replace("'", "")
            im.save(os.path.join(OUT, sid + ".png")); n += 1
    print(n, "species pictures in", OUT)


def credits():
    man = json.load(open(os.path.join(SRC, "manifest.json")))
    by = {}
    for n, m in man.items(): by.setdefault((m["author"], m["licence"]), []).append(m["title"])
    for (a, l), ts in sorted(by.items()): print("- **%s** (%s, poly.pizza): %s." % (a, l, ", ".join(sorted(set(ts)))))


cmd = sys.argv[1] if len(sys.argv) > 1 else "all"
if cmd in ("fetch", "all"): fetch()
if cmd in ("render", "all"): render(sys.argv[2:] or None)
if cmd in ("species", "all"): species()
if cmd == "credits": credits()
