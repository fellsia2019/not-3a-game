"""Trim the transparent Blender render while preserving a small edge margin."""

from pathlib import Path
from PIL import Image

path = Path(__file__).resolve().parents[2] / "Assets/Stage3/VisualPilot/RapidWatchtowerSprite.png"
image = Image.open(path).convert("RGBA")
box = image.getbbox()
if box is None:
    raise RuntimeError("Rapid tower render is fully transparent")
margin = 12
image.crop((max(0, box[0] - margin), max(0, box[1] - margin),
            min(image.width, box[2] + margin), min(image.height, box[3] + margin))).save(path)
print(f"S3_RAPID_SPRITE_OK size={Image.open(path).size}")
