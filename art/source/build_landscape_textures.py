"""Rebuild the superseded procedural compacted-earth fallback for comparison.

The active grass and earth albedos are original imagegen-painted assets saved
in Assets. This script never overwrites them. Foliage and rocks require
separate 3D meshes.
"""

from math import cos, floor, sin
from pathlib import Path
from random import Random

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[2]
SIZE = 512


def lattice(seed: int, x: int, y: int, period: int) -> float:
    x %= period
    y %= period
    value = (x * 374761393 + y * 668265263 + seed * 2246822519) & 0xFFFFFFFF
    value = ((value ^ (value >> 13)) * 1274126177) & 0xFFFFFFFF
    return ((value ^ (value >> 16)) & 0xFFFF) / 65535.0


def noise(seed: int, x: float, y: float, period: int) -> float:
    u, v = x * period, y * period
    ix, iy = floor(u), floor(v)
    fx, fy = u - ix, v - iy
    fx = fx * fx * (3.0 - 2.0 * fx)
    fy = fy * fy * (3.0 - 2.0 * fy)
    left = lattice(seed, ix, iy, period) * (1.0 - fy)
    left += lattice(seed, ix, iy + 1, period) * fy
    right = lattice(seed, ix + 1, iy, period) * (1.0 - fy)
    right += lattice(seed, ix + 1, iy + 1, period) * fy
    return left * (1.0 - fx) + right * fx


def clamp(value: float) -> int:
    return max(0, min(255, round(value)))


def make_ground(grass: bool) -> Image.Image:
    image = Image.new("RGB", (SIZE, SIZE))
    pixels = image.load()
    for py in range(SIZE):
        y = py / SIZE
        for px in range(SIZE):
            x = px / SIZE
            broad = noise(19 if grass else 83, x, y, 3) - 0.5
            middle = noise(41 if grass else 107, x, y, 11) - 0.5
            fine = noise(71 if grass else 139, x, y, 43) - 0.5
            grain = noise(173 if grass else 193, x, y, 97) - 0.5
            if grass:
                # Distinct pigment strokes survive the tactical camera's mipmaps.
                shade = 54 * broad + 39 * middle + 25 * fine + 8 * grain
                cool = 18 * middle - 9 * fine
                color = (153 + shade * 0.78 - cool * .5,
                         190 + shade + cool * .4, 76 + shade * .42)
            else:
                # Cool compacted earth. Cell guides are flat marks, not grooves.
                shade = 25 * broad + 24 * middle + 18 * fine + 6 * grain
                distance = min(px % 128, 128 - px % 128, py % 128, 128 - py % 128)
                guide = 10.0 * (1.0 - distance / 2.5) if distance < 2.5 else 0.0
                color = (119 + shade - guide, 117 + shade * 0.96 - guide,
                         108 + shade * 0.80 - guide)
            pixels[px, py] = tuple(clamp(channel) for channel in color)
    # Short, sharp pigment marks supply the readable surface grain. Raised
    # blades, flowers, bushes and rocks are separate meshes in Unity.
    painter = ImageDraw.Draw(image)
    rng = Random(314159 if grass else 271828)
    for _ in range(1450 if grass else 800):
        px, py = rng.randrange(SIZE), rng.randrange(SIZE)
        angle = rng.random() * 6.28318530718
        length = rng.uniform(5.0, 11.0) if grass else rng.uniform(4.0, 8.0)
        dx, dy = cos(angle) * length, sin(angle) * length * .48
        current = pixels[px, py]
        change = rng.choice((-18, -14, 11, 16)) if grass else rng.choice((-9, 8))
        color = tuple(clamp(channel + change) for channel in current)
        for offset_x in (-SIZE, 0, SIZE):
            for offset_y in (-SIZE, 0, SIZE):
                x1, y1 = px + offset_x, py + offset_y
                painter.line((x1, y1, x1 + dx, y1 + dy), fill=color,
                             width=2 if grass else 1)
    return image


def main() -> None:
    fallback = ROOT / "art" / "source" / "landscape_earth_fallback.png"
    make_ground(False).save(fallback, optimize=True)


if __name__ == "__main__":
    main()
