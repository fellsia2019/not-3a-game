"""Original D086 reward accent, candidate v5, alongside the accepted CC0 choir.

Owner-selected direction: Elden Ring Player Level Up (listening reference only).
No game audio, anvil, gong or other external effect is sampled.
Usage: python3 build_reward.py hero_levelup_voice_only.wav output_directory
Requires numpy, available in the bundled workspace Python. No Unity dependency.
"""
import hashlib
import json
import math
import sys
import wave
from pathlib import Path

import numpy as np

voice_path, directory = Path(sys.argv[1]), Path(sys.argv[2])
with wave.open(str(voice_path), "rb") as stream:
    if (stream.getnchannels(), stream.getsampwidth()) != (2, 2):
        raise ValueError("Expected the accepted stereo PCM16 choir mix.")
    rate = stream.getframerate()
    voice = np.frombuffer(stream.readframes(stream.getnframes()), "<i2").astype(float).reshape(-1, 2) / 32768
count = len(voice)
t = np.arange(count) / rate
rng = np.random.default_rng(8605)
accent = np.zeros_like(voice)

# Clustered mid/high modes have one common contact, rather than separate notes.
# Broad modal spacing avoids a pitched chime melody; individual modes settle quickly.
modes = [(481, .20, .85), (769, .18, .92), (1153, .16, .83),
         (1619, .115, .71), (2147, .080, .55), (2851, .045, .40),
         (3671, .025, .24), (4877, .012, .11)]
attack = 1 - np.exp(-t / .0015)
for i, (frequency, gain, decay) in enumerate(modes):
    for channel in range(2):
        offset = (-1 if channel == 0 else 1) * (.55 + i * .14)
        # Small initial tension release, not an octave drop or a bass thud.
        phase = 2 * np.pi * ((frequency + offset) * t + frequency * .014 * .035 * (1 - np.exp(-t / .035)))
        accent[:, channel] += gain * np.sin(phase + .17 * i) * attack * np.exp(-t / decay)


def band_noise(low, high):
    noise = rng.normal(size=count)
    spectrum = np.fft.rfft(noise)
    frequencies = np.fft.rfftfreq(count, 1 / rate)
    # Smooth shoulders prevent the ringing of a brick-wall filter.
    band = np.clip((frequencies - low * .65) / (low * .35), 0, 1)
    band *= np.clip((high * 1.3 - frequencies) / (high * .3), 0, 1)
    result = np.fft.irfft(spectrum * band, n=count)
    return result / max(np.std(result), 1e-8)


for channel in range(2):
    contact = band_noise(1150, 8500)
    air = band_noise(800, 5400)
    accent[:, channel] += contact * .095 * (1 - np.exp(-t / .0008)) * np.exp(-t / .016)
    accent[:, channel] += air * .035 * (1 - np.exp(-t / .018)) * np.exp(-t / .19)

# Diffuse room tail with no second discrete impact and no resonating gong sample.
ir_count = round(rate * 1.7)
ir_time = np.arange(ir_count) / rate
fft_size = 1 << (count + ir_count - 2).bit_length()
for channel in range(2):
    impulse = rng.normal(size=ir_count) * np.exp(-ir_time / .31)
    impulse[:round(.019 * rate)] = 0
    impulse *= 1 - np.exp(-np.maximum(0, ir_time - .019) / .012)
    impulse /= max(np.sqrt(np.sum(impulse ** 2)), 1e-8)
    wet = np.fft.irfft(np.fft.rfft(accent[:, 1 - channel], fft_size) * np.fft.rfft(impulse, fft_size), fft_size)[:count]
    accent[:, channel] += wet * .14

# The vocal arrangement itself is unchanged; leave the opening accent audible.
voice /= max(np.max(np.abs(voice)), .001)
mix = accent * 1.05 + voice * .52


def write(values, name):
    values = values - values.mean(axis=0)
    values *= .84 / np.max(np.abs(values))
    edge = np.minimum(1, np.minimum(t / .0005, (count - 1 - np.arange(count)) / (rate * .02)))
    values *= edge[:, None]
    pcm = np.rint(values * 32767).astype("<i2")
    path = directory / name
    directory.mkdir(parents=True, exist_ok=True)
    with wave.open(str(path), "wb") as stream:
        stream.setparams((2, 2, rate, count, "NONE", "not compressed"))
        stream.writeframes(pcm.tobytes())
    result = {"file": str(path), "seconds": count / rate, "rate": rate,
              "channels": 2, "peakDbfs": round(20 * math.log10(np.max(np.abs(pcm.astype(float))) / 32768), 2),
              "clippedSamples": int(np.count_nonzero(np.abs(pcm.astype(float)) >= 32767)),
              "sha256": hashlib.sha256(path.read_bytes()).hexdigest()}
    print(json.dumps(result))
    return result


results = [write(accent, "hero_levelup_accent_v5.wav"), write(mix, "hero_levelup_reward_v5.wav")]
(directory / "reward_v5_checks.json").write_text(json.dumps(results, indent=2) + "\n")
