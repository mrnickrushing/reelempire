#!/usr/bin/env python3
"""Paints the interface art into Assets/Resources/UI/Fx: python3 -I Tools/art/ui_art.py
The look: cut paper and stickers. Cream panels and bright candy buttons, every shape with a thick navy outline and a
hard drop shadow. Buttons, badges and bars are painted in greys so the game tints them."""
import math, os
from PIL import Image, ImageDraw, ImageFilter
ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT = os.path.join(ROOT, "Assets", "Resources", "UI", "Fx")
os.makedirs(OUT, exist_ok=True)
S = 4                                   # supersampling
INK = (22, 50, 79)
PAPER, PAPER2 = (255, 246, 227), (243, 226, 192)


def canvas(w, h): return Image.new("RGBA", (w * S, h * S), (0, 0, 0, 0))
def save(im, name, w, h): im.resize((w, h), Image.LANCZOS).save(os.path.join(OUT, name + ".png")); print(name)
def rr(d, box, r, fill): d.rounded_rectangle([b * S for b in box], r * S, fill=fill)


def slab(name, w, h, r, fill, lip=None, line=7, drop=7, shade=None, gloss=False):
    """A rounded shape: hard shadow, ink outline, fill, an optional darker lip along the bottom (a button's edge)."""
    im = canvas(w, h); d = ImageDraw.Draw(im)
    rr(d, (2, 2 + drop, w - 2, h - 1), r, INK + (70,))                      # the shadow, straight down
    rr(d, (2, 2, w - 2, h - 2 - drop), r, INK + (255,))                    # the outline
    inner = (2 + line, 2 + line, w - 2 - line, h - 2 - drop - line)
    if lip:
        rr(d, inner, r - line, lip + (255,))
        rr(d, (inner[0], inner[1], inner[2], inner[3] - 9), r - line, fill + (255,))
    else:
        rr(d, inner, r - line, fill + (255,))
    if shade:                                                              # a band of the second paper tone along the bottom
        m = canvas(w, h); md = ImageDraw.Draw(m); rr(md, inner, r - line, (255, 255, 255, 255))
        band = canvas(w, h); bd = ImageDraw.Draw(band); bd.rectangle([0, int((inner[3] - 16) * S), w * S, h * S], fill=shade + (255,))
        im.paste(band, (0, 0), Image.composite(band, canvas(w, h), m).split()[3])
    if gloss:
        rr(d, (inner[0] + 9, inner[1] + 6, inner[2] - 9, inner[1] + 13), 4, (255, 255, 255, 110))
    save(im, name, w, h)


slab("panel", 192, 192, 40, PAPER, shade=PAPER2, line=8, drop=9)
slab("card", 128, 128, 26, (255, 255, 255), line=6, drop=0)               # a row inside a panel (tinted)
slab("button", 160, 144, 36, (255, 255, 255), lip=(196, 196, 196), line=7, drop=6, gloss=True)
slab("pill", 140, 132, 62, PAPER, line=7, drop=6)
slab("tag", 96, 92, 26, PAPER, line=6, drop=5)
slab("plate", 128, 120, 30, (255, 255, 255), line=6, drop=5)              # tinted slabs (the guide, toasts)
slab("well", 64, 64, 20, (255, 255, 255), line=5, drop=0)                 # a bar's track / fill (tinted)

# A round badge (tinted) and its plain disc.
im = canvas(128, 136); d = ImageDraw.Draw(im)
d.ellipse([4 * S, 12 * S, 124 * S, 132 * S], fill=INK + (70,)); d.ellipse([4 * S, 4 * S, 124 * S, 124 * S], fill=INK + (255,))
d.ellipse([11 * S, 11 * S, 117 * S, 117 * S], fill=(196, 196, 196, 255)); d.ellipse([11 * S, 11 * S, 117 * S, 109 * S], fill=(255, 255, 255, 255))
save(im, "badge", 128, 136)

# The banner the harbour's name hangs on: a ribbon with swallow-tail ends (tinted).
im = canvas(256, 96); d = ImageDraw.Draw(im)
def ribbon(off, col):
    x0, x1, y0, y1 = 6, 250, 8 + off, 74 + off
    pts = [(x0, y0), (x1, y0), (x1 - 22, (y0 + y1) / 2), (x1, y1), (x0, y1), (x0 + 22, (y0 + y1) / 2)]
    d.polygon([(x * S, y * S) for x, y in pts], fill=col)
ribbon(8, INK + (70,)); ribbon(0, INK + (255,))
x0, x1, y0, y1 = 20, 236, 15, 67
d.polygon([(p[0] * S, p[1] * S) for p in [(x0, y0), (x1, y0), (x1 - 15, 41), (x1, y1), (x0, y1), (x0 + 15, 41)]], fill=(255, 255, 255, 255))
d.polygon([(p[0] * S, p[1] * S) for p in [(x0 + 5, y1 - 9), (x1 - 5, y1 - 9), (x1, y1), (x0, y1)]], fill=(205, 205, 205, 255))
save(im, "ribbon", 256, 96)

# The coin: gold, with a fish stamped on it.
im = canvas(128, 132); d = ImageDraw.Draw(im)
d.ellipse([4 * S, 10 * S, 124 * S, 130 * S], fill=INK + (80,)); d.ellipse([4 * S, 4 * S, 124 * S, 124 * S], fill=INK + (255,))
d.ellipse([11 * S, 11 * S, 117 * S, 117 * S], fill=(230, 150, 30, 255)); d.ellipse([11 * S, 11 * S, 117 * S, 110 * S], fill=(255, 197, 51, 255))
d.ellipse([22 * S, 20 * S, 106 * S, 102 * S], outline=(255, 226, 130, 255), width=4 * S)
fish = (214, 132, 22, 255)
d.ellipse([34 * S, 46 * S, 84 * S, 78 * S], fill=fish); d.polygon([(80 * S, 62 * S), (100 * S, 46 * S), (100 * S, 78 * S)], fill=fish)
d.ellipse([44 * S, 55 * S, 51 * S, 62 * S], fill=(255, 226, 130, 255))
save(im, "coin", 128, 132)

# The gem: a cut stone in sea-glass purple.
im = canvas(128, 132); d = ImageDraw.Draw(im)
def poly(pts, col, off=0): d.polygon([(x * S, (y + off) * S) for x, y in pts], fill=col)
outer = [(28, 10), (100, 10), (124, 44), (64, 122), (4, 44)]
poly(outer, INK + (80,), 6); poly(outer, INK + (255,))
poly([(33, 18), (95, 18), (113, 44), (15, 44)], (176, 150, 255, 255)); poly([(15, 44), (113, 44), (64, 110)], (123, 97, 255, 255))
poly([(47, 44), (81, 44), (64, 110)], (150, 126, 255, 255)); poly([(47, 18), (81, 18), (81, 44), (47, 44)], (214, 200, 255, 255))
poly([(15, 44), (47, 44), (64, 110)], (98, 74, 224, 255))
save(im, "gem", 128, 132)

# The star for milestones.
im = canvas(128, 132); d = ImageDraw.Draw(im)
def star(cx, cy, r1, r2, col):
    pts = []
    for i in range(10):
        a = -math.pi / 2 + i * math.pi / 5; r = r1 if i % 2 == 0 else r2
        pts.append(((cx + math.cos(a) * r) * S, (cy + math.sin(a) * r) * S))
    d.polygon(pts, fill=col)
star(64, 72, 62, 30, INK + (80,)); star(64, 66, 62, 30, INK + (255,)); star(64, 66, 49, 22, (255, 197, 51, 255)); star(64, 62, 30, 13, (255, 226, 130, 255))
save(im, "star", 128, 132)

# The TAP bubble: a white speech bubble with its point at the bottom.
im = canvas(128, 144); d = ImageDraw.Draw(im)
def bubble(off, col, grow):
    rr(d, (6 - grow, 6 - grow + off, 122 + grow, 108 + grow + off), 34 + grow, col)
    d.polygon([((46 - grow) * S, (100 + off) * S), ((82 + grow) * S, (100 + off) * S), (64 * S, (136 + grow + off) * S)], fill=col)
bubble(5, INK + (70,), 0); bubble(0, INK + (255,), 0); bubble(0, (255, 255, 255, 255), -7)
save(im, "bubble", 128, 144)

# The up arrow on level signs (white, tinted).
im = canvas(64, 64); d = ImageDraw.Draw(im)
d.polygon([(32 * S, 8 * S), (58 * S, 36 * S), (42 * S, 36 * S), (42 * S, 56 * S), (22 * S, 56 * S), (22 * S, 36 * S), (6 * S, 36 * S)], fill=(255, 255, 255, 255))
save(im, "up", 64, 64)

# Sun rays behind prizes and on the loading screen (white, tinted, soft).
im = canvas(512, 512); d = ImageDraw.Draw(im)
for i in range(12):
    a0 = i * math.pi / 6; a1 = a0 + math.pi / 12
    d.polygon([(256 * S, 256 * S), ((256 + math.cos(a0) * 400) * S, (256 + math.sin(a0) * 400) * S), ((256 + math.cos(a1) * 400) * S, (256 + math.sin(a1) * 400) * S)], fill=(255, 255, 255, 255))
im = im.resize((512, 512), Image.LANCZOS)
mask = Image.new("L", (512, 512), 0); md = ImageDraw.Draw(mask)
for r in range(256, 0, -2): md.ellipse([256 - r, 256 - r, 256 + r, 256 + r], fill=int(255 * (1 - r / 256) ** 0.8))
im.putalpha(Image.composite(im.split()[3], Image.new("L", (512, 512), 0), mask).point(lambda v: v) if False else Image.eval(Image.merge("L", [im.split()[3]]), lambda v: v))
a = im.split()[3]; from PIL import ImageChops
im.putalpha(ImageChops.multiply(a, mask))
im.save(os.path.join(OUT, "rays.png")); print("rays")
