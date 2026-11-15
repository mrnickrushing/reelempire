#!/usr/bin/env python3
"""The fish: free models (Quaternius's Cute Fish pack and others, CC0; a few CC BY from poly.pizza) fetched into
art-src/free/fish, a picture of each rendered by Blender, and one picture per species made from its model's picture
with the species' own colouring: Assets/Resources/UI/Species/<species id>.png (dots as underscores).

  python3 -I Tools/art/fish.py fetch | render | species | all     (ai: cuts the paid Higgsfield sheet; making it costs credits)
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
    # Their own shapes for the memorable fish (rare and up), so a pike isn't a swordfish in another colour.
    "trout": "2W2sKWYvk8k", "trout_b": "0HCLwTdxvz5", "halibut": "653otKgnnpx", "seahorse_b": "fkK7TgvkSVG", "kingfish": "e_Lkl7E-Tc8",
    "shark_e": "8Ke5qCnWxsZ", "eel_b": "9re2iW8tOnP", "eel_d": "dDzthiG8nr9", "manta": "yzD8b7ZHZm", "parrot": "lj0WFfJkbb",
    "fish_q": "XWl86YFtpF", "fish_g": "aEyLrUMMoUK", "fish_k": "HkUAXudvBt",
}
VIEW = {"crab": 20, "octopus": 20, "lobster": 60, "shrimp": 60, "squid": 60}       # yaw for the picture; fish default below
PITCH = {"halibut": 62, "manta": 68}       # flat fish and rays are seen from above
# Species -> (model, hue shift in degrees, saturation x, brightness x)
S = {
    "lake": {"Bluegill": ("tang", 150, .8, 1), "Yellow Perch": ("snapper", 45, 1, 1.05), "Roach": ("tetra", 0, .5, 1.05), "Crappie": ("butterfly", 200, .3, .95), "Bream": ("idol", 20, .35, 1),
             "Tench": ("grouper", 90, .7, .8), "Rudd": ("goldfish", -15, .9, 1), "Largemouth Bass": ("grouper", 110, .8, .85), "Common Carp": ("koi", 30, .5, .8), "Walleye": ("snapper", 60, .5, .8),
             "Channel Catfish": ("catfish", 0, .6, .9), "Northern Pike": ("kingfish", -105, 1.3, .9), "Mirror Carp": ("koi", 25, .8, 1), "Burbot": ("catfish", 30, .5, .7), "Muskellunge": ("kingfish", -175, 1.1, 1.05), "Golden Koi": ("koi", 0, 1, 1.25)},
    "river": {"Brown Trout": ("trout", 40, .7, .85), "Chub": ("tetra", 30, .4, .9), "Dace": ("tetra", 180, .4, 1.1), "Grayling": ("tuna", 240, .3, 1), "Barbel": ("catfish", 20, .7, 1), "Smallmouth Bass": ("grouper", 70, .7, .8),
              "Minnow": ("tetra", 0, 1, 1), "Rainbow Trout": ("trout", 0, 1, 1.05), "River Eel": ("eel", 0, 1, 1), "Zander": ("snapper", 80, .4, .9), "Brook Char": ("trout_b", -15, .75, .75), "Atlantic Salmon": ("fish_q", 0, .75, 1),
              "Steelhead": ("trout", 0, .4, 1.1), "Wels Catfish": ("catfish", -20, .5, .6), "White Sturgeon": ("sturgeon", 0, .77, .9), "Ghost Salmon": ("fish_q", 0, .1, 1.05)},
    "coast": {"Mackerel": ("tuna", -20, 1, 1), "Herring": ("tetra", 200, .3, 1.15), "Pollock": ("snapper", 110, .4, .9), "Whiting": ("snapper", 200, .2, 1.15), "Flounder": ("flatfish", 0, 1, 1), "Shore Crab": ("crab", 100, .7, .8),
              "Sardine": ("tetra", 210, .5, 1.1), "Sea Bass": ("grouper", 200, .3, 1), "Atlantic Cod": ("grouper", 50, .5, .9), "Brown Crab": ("crab", 15, .8, .8), "Squid": ("squid", 0, 1, 1), "Lobster": ("lobster", 0, 1, 1),
              "Turbot": ("turbot", 0, 1, 1), "Conger Eel": ("eel_d", 0, .3, .75), "Halibut": ("halibut", 0, 1, 1), "Blue Lobster": ("lobster", 215, 1.2, 1)},
    "reef": {"Clownfish": ("clown", 0, 1, 1), "Blue Tang": ("bluetang", 0, 1, 1), "Butterflyfish": ("butterfly", 0, 1, 1), "Parrotfish": ("parrot", 0, 1, 1), "Snapper": ("snapper", 0, 1, 1), "Damselfish": ("tang", 200, 1, 1),
             "Wrasse": ("gramma", 0, 1, 1), "Angelfish": ("idol", 0, 1, 1), "Grouper": ("grouper", 0, 1, 1), "Reef Octopus": ("octopus", 0, 1, 1), "Triggerfish": ("cowfish", 0, 1, 1), "Lionfish": ("lionfish", 0, 1, 1),
             "Barracuda": ("kingfish", 0, .25, 1.15), "Moray Eel": ("eel_b", 0, 1, .9), "Giant Trevally": ("trevally", 0, .77, .9), "Lantern Seahorse": ("seahorse_b", 0, 1, 1.1)},
    "deep": {"Skipjack Tuna": ("tuna", 0, 1, 1), "Mahi-Mahi": ("flowerhorn", 60, 1, 1.1), "Bonito": ("tuna", 30, .7, .95), "Flying Fish": ("cardinal", 200, .8, 1.1), "Albacore": ("tuna", -10, .5, 1.15), "Amberjack": ("grouper", 30, .8, 1.1),
             "Wahoo": ("kingfish", 0, 1.1, 1.05), "Yellowfin Tuna": ("tuna", 200, 1.1, 1.1), "Sailfish": ("swordfish", 0, 1, 1), "Giant Squid": ("squid", -20, 1.2, .9), "Mako Shark": ("shark", 0, 1, 1), "Swordfish": ("swordfish", -30, .6, .8),
             "Bluefin Tuna": ("tuna", 10, 1.3, .8), "Oarfish": ("oarfish", 0, .77, .9), "Blue Marlin": ("swordfish", 10, 1.4, .9), "Anglerfish King": ("angler", 0, 1, 1)},
    "arctic": {"Arctic Char": ("trout", 20, 1.1, 1), "Capelin": ("tetra", 190, .3, 1.2), "Polar Cod": ("snapper", 190, .2, 1.1), "Lumpfish": ("puffer", 0, 1, 1), "Snow Crab": ("crab", 20, .4, 1.3), "Redfish": ("fish_k", 0, 1.1, 1),
               "Northern Shrimp": ("shrimp", 0, 1, 1), "Greenland Halibut": ("halibut", 180, .3, .75), "King Crab": ("crab", 0, 1.2, 1), "Wolffish": ("blobfish", 180, .5, .8), "Arctic Skate": ("manta", 0, .4, 1),
               "Sablefish": ("fish_g", 0, .3, .55), "Giant Pacific Octopus": ("octopus", -20, 1.2, .9), "Toothfish": ("toothfish", 0, .77, .9), "Greenland Shark": ("shark_e", 0, .35, .7), "Aurora Char": ("trout", 100, 1.5, 1.05)},
}


# Legends that gleam: (hue, least saturation, saturation kept) painted over the whole fish.
GLEAM = {"Golden Koi": (0.115, 0.75, 1.0), "Ghost Salmon": (0.55, 0.0, 0.12), "Aurora Char": (0.45, 0.6, 1.0), "Blue Lobster": (0.6, 0.8, 1.0)}


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
    jobs = [dict(name=n, files=[os.path.join(SRC, n + ".glb")], out=os.path.join(SRC, "pic", n + ".png"), yaw=VIEW.get(n, 125), pitch=PITCH.get(n, 12))
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
            wash = GLEAM.get(name)
            if True:
                px = im.load()
                for y in range(256):
                    for x in range(256):
                        r, g, b, a = px[x, y]
                        if a == 0: continue
                        h, s, v = colorsys.rgb_to_hsv(r / 255, g / 255, b / 255)
                        if wash and v > 0.16: h, s = wash[0], max(wash[1], s * wash[2])      # the whole fish in one metal, the eye left dark
                        r2, g2, b2 = colorsys.hsv_to_rgb((h + hue / 360) % 1, min(1, s * sat * 1.3), min(1, v * val * 1.12))
                        px[x, y] = (int(r2 * 255), int(g2 * 255), int(b2 * 255), a)
            sid = harbour + "_" + name.lower().replace(" ", "_").replace("'", "")
            im.save(os.path.join(OUT, sid + ".png")); n += 1
    print(n, "species pictures in", OUT)


# Fish no free model fits, painted by Higgsfield (paid: one 2x2 concept sheet, 2.75 credits; ask before running). The
# sheet already looks like the book's renders, so each fish is cut out of it (white flooded away from the edges) and
# used as its picture: art-src/free/fish/pic/<name>.png, then `species` colours them like the others.
AI = ["sturgeon", "trevally", "toothfish", "oarfish"]
AI_SPLIT = (0.509, 0.503)       # where the quarters meet (the sturgeon's snout runs just past the middle)
AI_PROMPT = ("A 2x2 grid of four separate fish, each a clean low-poly flat-shaded 3D game model in the style of a cute mobile "
             "fishing game, seen exactly from the side facing right, whole body in frame with space around it, plain pure white "
             "background, soft even studio light, no text, no labels, no water. Top left: a white sturgeon, long grey body, "
             "pointed shovel snout with four whiskers, rows of bony plates along the back and sides, shark-like upturned tail. "
             "Top right: a giant trevally, deep silver-grey compressed body, steep blunt forehead, long sickle tail fork, dark "
             "dorsal fins. Bottom left: a Patagonian toothfish, heavy dark grey-brown cod-like body, big jaw with small teeth, "
             "pale eye. Bottom right: an oarfish, very long ribbon-like silver body with a bright red crest running the length "
             "of its back and red fin plumes on its head.")


def ai():
    from PIL import Image, ImageDraw
    sheet = os.path.join(SRC, "ai_sheet.png")
    if not os.path.exists(sheet):
        r = subprocess.run(["higgsfield", "generate", "create", "gpt_image_2_5", "--aspect_ratio", "1:1", "--quality", "high",
                            "--resolution", "2k", "--background", "opaque", "--prompt", AI_PROMPT, "--wait"], capture_output=True, text=True)
        url = next((l for l in reversed((r.stdout + r.stderr).splitlines()) if l.startswith("http")), None)
        if not url: raise SystemExit((r.stdout + r.stderr)[-500:])
        open(sheet, "wb").write(urllib.request.urlopen(urllib.request.Request(url, headers=UA), timeout=300).read())
        print("sheet", url)
    im = Image.open(sheet).convert("RGB"); w, h = im.size
    sx, sy = int(w * AI_SPLIT[0]), int(h * AI_SPLIT[1])
    boxes = [(0, 0, sx, sy), (sx, 0, w, sy), (0, sy, sx, h), (sx, sy, w, h)]
    os.makedirs(os.path.join(SRC, "pic"), exist_ok=True)
    man_path = os.path.join(SRC, "manifest.json"); man = json.load(open(man_path))
    for name, box in zip(AI, boxes):
        q = im.crop(box)
        # Flood the white background from the border into a mask, so pale bellies and highlights inside the fish stay.
        mask = Image.new("L", q.size, 0); px = q.load(); mk = mask.load(); qw, qh = q.size
        stack = [(x, y) for x in range(qw) for y in (0, qh - 1)] + [(x, y) for y in range(qh) for x in (0, qw - 1)]
        white = lambda p: min(p) > 232 and max(p) - min(p) < 18
        while stack:
            x, y = stack.pop()
            if mk[x, y] or not white(px[x, y]): continue
            mk[x, y] = 255
            if x > 0: stack.append((x - 1, y))
            if x < qw - 1: stack.append((x + 1, y))
            if y > 0: stack.append((x, y - 1))
            if y < qh - 1: stack.append((x, y + 1))
        alpha = mask.point(lambda v: 255 - v)
        out = q.convert("RGBA"); out.putalpha(alpha)
        out = out.crop(out.getbbox())
        # The soft shadow the rendered fish cast on the floor under them.
        from PIL import ImageFilter
        fw, fh = out.size; pad = fh // 6
        canvas = Image.new("RGBA", (fw + 2 * pad, fh + 2 * pad), (0, 0, 0, 0))
        shadow = Image.new("L", canvas.size, 0)
        ImageDraw.Draw(shadow).ellipse((pad + fw * 0.08, pad + fh * 0.93, pad + fw * 0.92, pad + fh * 1.07), fill=70)
        shadow = shadow.filter(ImageFilter.GaussianBlur(pad // 2))
        canvas.paste((40, 50, 60, 255), (0, 0), shadow)
        canvas.alpha_composite(out, (pad, pad))
        out = canvas.crop(canvas.getbbox())
        out.save(os.path.join(SRC, "pic", name + ".png"))
        man[name] = {"id": "higgsfield", "title": name.title(), "author": "Rushing Technologies (Higgsfield)", "licence": "own"}
        print("picture", name, out.size)
    json.dump(man, open(man_path, "w"), indent=1)


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
if cmd == "ai": ai()
