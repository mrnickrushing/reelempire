#!/usr/bin/env python3
"""Reel Empire's music, made here from nothing but arithmetic (so there is nothing to license): a breezy harbour
loop of marimba, a plucked bass, soft pads and a shaker by day; a slower lullaby of the same by night. Writes Assets/Resources/Audio/Music/<name>.ogg.

    python3 Tools/art/music.py            # every track
Needs numpy, scipy and ffmpeg.
"""
import os, subprocess, sys, tempfile, wave
import numpy as np
from scipy.signal import fftconvolve, butter, lfilter

SR = 44100
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "Assets", "Resources", "Audio", "Music")
NOTE = {"C": 0, "C#": 1, "Db": 1, "D": 2, "Eb": 3, "E": 4, "F": 5, "F#": 6, "G": 7, "Ab": 8, "A": 9, "Bb": 10, "B": 11}

def hz(name, octave): return 440.0 * 2 ** ((NOTE[name] + (octave - 4) * 12 - 9) / 12.0)
def lowpass(x, cut, order=2):
    b, a = butter(order, cut / (SR / 2), "low"); return lfilter(b, a, x)
def highpass(x, cut, order=2):
    b, a = butter(order, cut / (SR / 2), "high"); return lfilter(b, a, x)

def env(n, attack, release, hold=None):
    """A soft envelope over n samples: rise, hold, fall."""
    e = np.ones(n)
    a = min(n, int(attack * SR)); r = min(n, int(release * SR))
    if a > 0: e[:a] = np.sin(np.linspace(0, np.pi / 2, a)) ** 2
    if r > 0: e[-r:] *= np.cos(np.linspace(0, np.pi / 2, r)) ** 2
    return e

def pad(freq, seconds, rng):
    """Three slightly detuned voices with a few soft harmonics, slowly breathing."""
    n = int(seconds * SR); t = np.arange(n) / SR
    out = np.zeros((n, 2))
    for k, det in enumerate((-0.004, 0.0, 0.005)):
        f = freq * (1 + det)
        ph = rng.uniform(0, 2 * np.pi)
        v = np.sin(2 * np.pi * f * t + ph) + 0.35 * np.sin(2 * np.pi * 2 * f * t + ph * 2) + 0.12 * np.sin(2 * np.pi * 3 * f * t) + 0.05 * np.sin(2 * np.pi * 4 * f * t)
        v *= 1 + 0.12 * np.sin(2 * np.pi * (0.11 + 0.05 * k) * t + ph)
        pan = 0.5 + (k - 1) * 0.35
        out[:, 0] += v * (1 - pan); out[:, 1] += v * pan
    return out * env(n, 1.4, 1.8)[:, None]

def keys(freq, seconds, vel=1.0):
    """A marimba: a quick wooden knock, a pure tone under it and the bar's fourth harmonic dying fast."""
    n = int(seconds * SR); t = np.arange(n) / SR
    v = np.sin(2 * np.pi * freq * t) * np.exp(-t * 5.5)
    v += 0.45 * np.sin(2 * np.pi * freq * 4 * t) * np.exp(-t * 22)
    v += 0.18 * np.sin(2 * np.pi * freq * 10 * t) * np.exp(-t * 60)
    v *= env(n, 0.002, 0.06)
    pan = 0.5 + 0.3 * np.sin(freq * 0.37)
    return np.stack([v * (1 - pan), v * pan], axis=1) * vel

def bass(freq, seconds):
    n = int(seconds * SR); t = np.arange(n) / SR
    v = np.sin(2 * np.pi * freq * t) + 0.3 * np.sin(2 * np.pi * 2 * freq * t) * np.exp(-t * 4)
    v *= np.exp(-t * 1.3) * env(n, 0.012, 0.12)
    return np.stack([v, v], axis=1)

def brush(seconds, rng, bright=6000):
    n = int(seconds * SR); t = np.arange(n) / SR
    v = highpass(rng.standard_normal(n), bright) * np.exp(-t * 32)
    return np.stack([v, v * 0.8], axis=1)

def kick(seconds=0.35):
    n = int(seconds * SR); t = np.arange(n) / SR
    f = 46 + 70 * np.exp(-t * 28)
    v = np.sin(2 * np.pi * np.cumsum(f) / SR) * np.exp(-t * 11)
    return np.stack([v, v], axis=1)

def room(x, rng, seconds=2.6, wet=0.3):
    """A hall: the dry sound plus itself played through a burst of fading noise."""
    n = int(seconds * SR); t = np.arange(n) / SR
    ir = rng.standard_normal((n, 2)) * np.exp(-t * 2.6)[:, None]
    ir = np.stack([lowpass(ir[:, 0], 5200), lowpass(ir[:, 1], 5200)], axis=1)
    ir /= np.sqrt((ir ** 2).sum(axis=0))
    w = np.stack([fftconvolve(x[:, c], ir[:, c]) for c in range(2)], axis=1)
    out = np.zeros_like(w); out[:len(x)] += x * (1 - wet); out += w * wet
    return out

def add(mix, at, sound, gain=1.0):
    i = max(0, int(at * SR))
    j = min(len(mix), i + len(sound))
    if i < len(mix): mix[i:j] += sound[:j - i] * gain

def track(name, bpm, chords, seed, bright=1.0, drums=True):
    """chords: one per bar, each (root note, octave, [intervals in semitones], [melody steps])."""
    rng = np.random.default_rng(seed)
    beat = 60.0 / bpm; bar = beat * 4
    length = bar * len(chords)
    tail = 5.0
    mix = np.zeros((int((length + tail) * SR), 2))
    for b, (root, octv, tones, tune) in enumerate(chords):
        t0 = b * bar
        base = hz(root, octv)
        # Pad: the whole chord, held a little past the bar so the changes melt.
        for iv in tones: add(mix, t0, pad(base * 2 ** (iv / 12.0), bar + 1.2, rng), 0.05)
        # Bass on one and the push before three.
        add(mix, t0, bass(base / 2, beat * 2.2), 0.3)
        add(mix, t0 + beat * 2.5, bass(base / 2, beat * 1.4), 0.2)
        # Keys: a slow broken chord, never the same twice, with room to breathe.
        order = list(tones) + [tones[0] + 12]
        steps = [0, 1.5, 2, 3, 3.5] if b % 2 == 0 else [0.5, 1, 2.5, 3]
        for k, s in enumerate(steps):
            iv = order[(k * 2 + b) % len(order)]
            add(mix, t0 + s * beat + rng.uniform(-0.012, 0.012), keys(base * 2 * 2 ** (iv / 12.0), beat * 2.4, rng.uniform(0.6, 1.0)), 0.16 * bright)
        # A line on top, a few notes a bar.
        for s, iv in tune:
            add(mix, t0 + s * beat, keys(base * 4 * 2 ** (iv / 12.0), beat * 3, 0.9), 0.15 * bright)
        if drums:
            for q in range(8):
                add(mix, t0 + q * beat / 2 + (0.03 if q % 2 else 0), brush(0.2, rng, 7000 if q % 2 else 5200), 0.045 if q % 2 else 0.028)
            add(mix, t0, kick(), 0.34); add(mix, t0 + beat * 2.5, kick(), 0.2)
            add(mix, t0 + beat, brush(0.3, rng, 2400), 0.07); add(mix, t0 + beat * 3, brush(0.3, rng, 2400), 0.08)
    mix = room(mix, rng)
    # Wrap whatever rings past the end back onto the start: the loop has no seam.
    n = int(length * SR)
    loop = mix[:n].copy(); extra = mix[n:]; loop[:len(extra)] += extra[:n]
    loop = np.stack([lowpass(loop[:, 0], 9000), lowpass(loop[:, 1], 9000)], axis=1)
    # (filtering the wrapped loop twice round keeps the filter's own start-up out of the seam)
    loop /= np.abs(loop).max() / 0.72
    os.makedirs(OUT, exist_ok=True)
    with tempfile.NamedTemporaryFile(suffix=".wav", delete=False) as f: tmp = f.name
    with wave.open(tmp, "wb") as w:
        w.setnchannels(2); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes((np.clip(loop, -1, 1) * 32767).astype("<i2").tobytes())
    dst = os.path.join(OUT, name + ".ogg")
    subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", tmp, "-c:a", "libvorbis", "-q:a", "4", dst], check=True)
    os.unlink(tmp)
    print(name, "%.0f s" % length, "%d KB" % (os.path.getsize(dst) // 1024))

MAJ7, MIN7, DOM7, SUS = [0, 4, 7, 11], [0, 3, 7, 10], [0, 4, 7, 10], [0, 5, 7, 10]
MAJ, MIN, ADD9, SIX = [0, 4, 7], [0, 3, 7], [0, 4, 7, 14], [0, 4, 7, 9]

# Day: a sunny morning on the quay, in G. Sixteen bars, a tune that rises and comes home.
day = [
    ("G", 3, ADD9, [(0, 7), (1, 9), (2, 12)]), ("E", 3, MIN7, [(0.5, 7), (2, 3), (3, 7)]), ("C", 3, SIX, [(0, 4), (1.5, 7), (3, 9)]), ("D", 3, SUS, [(1, 7), (2.5, 5)]),
    ("G", 3, ADD9, [(0, 12), (1, 9), (2, 7), (3, 4)]), ("B", 2, MIN7, [(0, 7), (2, 10)]), ("C", 3, ADD9, [(0.5, 7), (2, 4), (3, 2)]), ("D", 3, DOM7, [(1, 4), (2, 7), (3.5, 10)]),
    ("E", 3, MIN7, [(0, 7), (1, 10), (2, 12)]), ("C", 3, MAJ7, [(0.5, 7), (2.5, 4)]), ("G", 3, SIX, [(0, 4), (1, 7), (2, 9), (3, 7)]), ("D", 3, SUS, [(1.5, 5), (3, 7)]),
    ("C", 3, ADD9, [(0, 7), (1, 4), (2, 2)]), ("A", 2, MIN7, [(0.5, 7), (2, 10), (3, 7)]), ("D", 3, DOM7, [(0, 4), (1.5, 7), (3, 10)]), ("G", 3, ADD9, [(0, 7), (2, 12)]),
]
# Night: the same harbour asleep, in E minor. Twelve bars, slow, no drums.
night = [
    ("E", 3, MIN7, [(0, 7)]), ("C", 3, MAJ7, [(2, 4)]), ("A", 2, MIN7, [(1, 10)]), ("B", 2, SUS, [(2, 7)]),
    ("E", 3, MIN7, [(0, 10), (3, 7)]), ("G", 2, ADD9, [(2, 4)]), ("A", 2, MIN7, [(0, 7)]), ("B", 2, DOM7, [(2.5, 4)]),
    ("C", 3, MAJ7, [(0, 7)]), ("A", 2, MIN7, [(2, 3)]), ("D", 3, SUS, [(1, 7)]), ("B", 2, SUS, [(2, 5)]),
]
which = sys.argv[1:] or ["day", "night"]
if "day" in which: track("day", 100, day, 21)
if "night" in which: track("night", 70, night, 33, bright=0.8, drums=False)
