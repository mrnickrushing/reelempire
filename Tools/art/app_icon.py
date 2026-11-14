#!/usr/bin/env python3
"""The app icon, in the game's cut-paper sticker look: a koi leaping out of paper waves in front of a sun burst, with
coins. The fish is the free koi model (art-src/free/fish/koi.glb) rendered by Tools/blender/icon_render.py.
  python3 -I Tools/art/app_icon.py        ->  Assets/Icon/icon.png (1024, no transparency, as the App Store wants)
"""
import colorsys, json, math, os, subprocess, sys, tempfile
from PIL import Image, ImageDraw, ImageFilter, ImageChops

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..")
OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.join(ROOT, "Assets", "Icon", "icon.png")
N, SS = 1024, 2                      # drawn at twice the size, then reduced
W = N * SS
INK = (22, 44, 82, 255)


def fish():
    tmp = tempfile.mkdtemp()
    out = os.path.join(tmp, "koi.png")
    json.dump([dict(name="koi", files=[os.path.join(ROOT, "art-src", "free", "fish", "koi.glb")], out=out, yaw=118, pitch=6, size=1500)],
              open(os.path.join(tmp, "j.json"), "w"))
    subprocess.run(["blender", "-b", "--factory-startup", "-P", os.path.join(ROOT, "Tools", "blender", "icon_render.py"), "--", os.path.join(tmp, "j.json")],
                   capture_output=True, text=True, check=True)
    im = Image.open(out).convert("RGBA")
    px = im.load()
    for y in range(im.height):
        for x in range(im.width):
            r, g, b, a = px[x, y]
            if a < 235:                                   # the floor's shadow and the soft edge
                px[x, y] = (r, g, b, 0); continue
            h, s, v = colorsys.rgb_to_hsv(r / 255, g / 255, b / 255)
            if v < 0.2: continue                          # the eye
            if s < 0.14: v = min(1, v * 1.2 + 0.14); s *= 0.3            # white scales: cleaner
            elif 0.06 < h < 0.2 and s < 0.6: h, s, v = 0.125, 0.9, min(1, v * 1.5 + 0.12)   # sand fins: gold
            else: h, s, v = 0.035, min(1, s * 1.35 + 0.15), min(1, v * 1.25 + 0.05)         # red: bright vermilion
            r, g, b = colorsys.hsv_to_rgb(h, s, v)
            px[x, y] = (int(r * 255), int(g * 255), int(b * 255), 255)
    im = im.crop(im.getbbox())
    return im


def outline(im, width, colour):
    """The picture's shape grown by `width`, in one colour."""
    a = im.split()[3].filter(ImageFilter.MaxFilter(1 + 2 * (width // 3)))
    for _ in range(2): a = a.filter(ImageFilter.MaxFilter(1 + 2 * (width // 3)))
    a = a.filter(ImageFilter.GaussianBlur(2)).point(lambda v: 255 if v > 110 else 0).filter(ImageFilter.GaussianBlur(1.2))
    o = Image.new("RGBA", im.size, colour); o.putalpha(a)
    return o


def sticker(im, white=26, ink=14, pad=70):
    big = Image.new("RGBA", (im.width + pad * 2, im.height + pad * 2), (0, 0, 0, 0)); big.alpha_composite(im, (pad, pad))
    out = Image.new("RGBA", big.size, (0, 0, 0, 0))
    out.alpha_composite(outline(big, white + ink, INK)); out.alpha_composite(outline(big, white, (255, 255, 255, 255))); out.alpha_composite(big)
    return out


def shadowed(layer, dx, dy, alpha=110):
    sh = Image.new("RGBA", layer.size, (10, 30, 70, 255)); sh.putalpha(layer.split()[3].point(lambda v: v * alpha // 255))
    out = Image.new("RGBA", layer.size, (0, 0, 0, 0)); out.alpha_composite(sh, (dx, dy)); out.alpha_composite(layer)
    return out


def background():
    im = Image.new("RGBA", (W, W), (0, 0, 0, 255)); px = im.load()
    cx, cy = W * 0.5, W * 0.46
    top, mid, low = (255, 214, 64), (255, 170, 44), (255, 122, 60)
    for y in range(W):
        for x in range(W):
            d = min(1, math.hypot(x - cx, y - cy) / (W * 0.75))
            c = [top[i] + (mid[i] - top[i]) * min(1, d * 2) if d < 0.5 else mid[i] + (low[i] - mid[i]) * (d - 0.5) * 2 for i in range(3)]
            ray = (math.atan2(y - cy, x - cx) / (2 * math.pi) * 16 + 0.25) % 1 < 0.5
            k = 1.0 if ray else 0.9
            add = 26 if ray else 0
            px[x, y] = (min(255, int(c[0] * k + add)), min(255, int(c[1] * k + add)), min(255, int(c[2] * k + add * 0.6)), 255)
    return im


def wave(y0, amp, period, phase, fill, foam=True):
    """One paper wave across the bottom: scalloped crest, ink edge, a pale lip of foam."""
    layer = Image.new("RGBA", (W, W), (0, 0, 0, 0)); d = ImageDraw.Draw(layer)
    pts = [(x, y0 - amp * abs(math.sin((x / W * period + phase) * math.pi))) for x in range(-20, W + 21, 6)]
    d.polygon(pts + [(W + 20, W + 20), (-20, W + 20)], fill=fill)
    if foam: d.line([(x, y + 26) for x, y in pts], fill=(255, 255, 255, 150), width=22, joint="curve")
    d.line(pts, fill=INK, width=20, joint="curve")
    return layer


def coin(size):
    c = Image.new("RGBA", (size, size), (0, 0, 0, 0)); d = ImageDraw.Draw(c); e = size * 0.07
    d.ellipse([0, 0, size - 1, size - 1], fill=INK)
    d.ellipse([e, e, size - 1 - e, size - 1 - e], fill=(255, 196, 40, 255))
    d.ellipse([e * 2.3, e * 2.3, size - 1 - e * 2.3, size - 1 - e * 2.3], fill=(255, 224, 96, 255))
    d.ellipse([e * 3.0, e * 3.4, size - 1 - e * 2.3, size - 1 - e * 2.3], fill=(255, 208, 60, 255))
    # the fish stamped on it, as on the game's coin
    m = size / 2; b = size * 0.2
    d.ellipse([m - b * 1.15, m - b * 0.7, m + b * 0.75, m + b * 0.7], fill=(224, 140, 20, 255))
    d.polygon([(m + b * 0.55, m), (m + b * 1.35, m - b * 0.7), (m + b * 1.35, m + b * 0.7)], fill=(224, 140, 20, 255))
    d.arc([e * 2.6, e * 2.6, size - 1 - e * 2.6, size - 1 - e * 2.6], 200, 262, fill=(255, 250, 220, 255), width=max(3, int(size * 0.035)))
    return c


def sparkle(d, x, y, r, col=(255, 255, 255, 255)):
    d.polygon([(x, y - r), (x + r * 0.24, y - r * 0.24), (x + r, y), (x + r * 0.24, y + r * 0.24), (x, y + r), (x - r * 0.24, y + r * 0.24), (x - r, y), (x - r * 0.24, y - r * 0.24)], fill=col)


def main():
    im = background()
    im.alpha_composite(wave(W * 0.70, 70, 4.0, 0.3, (38, 172, 196, 255)))
    # coins flung up behind the fish
    for x, y, s, rot in ((0.84, 0.17, 270, -18), (0.15, 0.20, 230, 20), (0.36, 0.09, 130, 8)):
        c = shadowed(coin(s).rotate(rot, resample=Image.BICUBIC, expand=True), 10, 14)
        im.alpha_composite(c, (int(W * x - c.width / 2), int(W * y - c.height / 2)))
    f = fish()
    k = W * 0.80 / f.width
    f = f.resize((int(f.width * k), int(f.height * k)), Image.LANCZOS)
    f = f.transpose(Image.FLIP_LEFT_RIGHT) if False else f
    f = f.rotate(20, resample=Image.BICUBIC, expand=True)
    st = shadowed(sticker(f), 16, 24, 120)
    im.alpha_composite(st, (int(W * 0.49 - st.width / 2), int(W * 0.50 - st.height / 2)))
    # the eye's shine
    im.alpha_composite(wave(W * 0.83, 80, 3.0, 0.75, (24, 132, 190, 255)))
    im.alpha_composite(wave(W * 0.95, 60, 5.0, 0.1, (20, 96, 168, 255), foam=False))
    d = ImageDraw.Draw(im)
    for x, y, r in ((0.08, 0.42, 40), (0.93, 0.40, 34), (0.60, 0.07, 30), (0.24, 0.34, 18)):
        sparkle(d, W * x, W * y, r * SS)
    for x, y, r in ((0.16, 0.66, 20), (0.86, 0.64, 26), (0.24, 0.60, 12), (0.78, 0.585, 14)):      # splash drops
        d.ellipse([W * x - r * SS, W * y - r * SS, W * x + r * SS, W * y + r * SS], fill=(255, 255, 255, 255), outline=INK, width=10)
    im = im.resize((N, N), Image.LANCZOS).convert("RGB")
    im.save(OUT)
    print("icon", OUT)


main()
