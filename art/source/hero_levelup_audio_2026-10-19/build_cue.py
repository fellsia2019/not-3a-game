"""D086 v4: one recorded hard anvil strike, prolonged same-metal tail, approved choir.

Requires numpy (bundled workspace Python), no Unity package.
Decode preserved MP3s to PCM16 WAV with afconvert first.
Usage: python3 build_cue.py choir.wav anvil_386107.wav cue.wav impact_preview.wav
Unpitched contact is preserved. Only the anvil's ringing tail is stretched.
No gong, bell, synthesized bass body, low-pass damping or additional strike.
"""
import hashlib
import json
import math
import sys
import wave
from pathlib import Path
import numpy as np

choir_path, anvil_path, cue_path, preview_path = map(Path, sys.argv[1:5])
rate = 44100
duration = 3.2
count = round(rate * duration)


def read_pcm(path):
    with wave.open(str(path), "rb") as stream:
        if stream.getsampwidth() != 2:
            raise ValueError("Expected decoded PCM16 WAV.")
        source_rate = stream.getframerate()
        samples = np.frombuffer(stream.readframes(stream.getnframes()), dtype="<i2").astype(float)
        samples = samples.reshape(-1, stream.getnchannels()) / 32768
    if samples.shape[1] == 1:
        samples = np.repeat(samples, 2, axis=1)
    if source_rate != rate:
        positions = np.arange(round(len(samples) * rate / source_rate)) * source_rate / rate
        samples = np.column_stack([np.interp(positions, np.arange(len(samples)), samples[:, c]) for c in range(2)])
    return samples


def stretch_tail(samples, factor):
    """Phase vocoder: longer decay with original metal frequencies, no pitch shift."""
    size, hop = 2048, 256
    window = np.hanning(size)
    padded = np.pad(samples, ((size // 2, size // 2 + size), (0, 0)))
    starts = range(0, len(padded) - size, hop)
    spectrum = np.stack([np.fft.rfft(padded[p:p + size] * window[:, None], axis=0) for p in starts], axis=1)
    steps = np.arange(0, spectrum.shape[1] - 1, 1 / factor)
    advance = (2 * np.pi * hop / size * np.arange(size // 2 + 1))[:, None]
    phase = np.angle(spectrum[:, 0])
    result = np.zeros(((len(steps) - 1) * hop + size, 2))
    weight = np.zeros(len(result))
    for output_frame, position in enumerate(steps):
        i, fraction = int(position), position % 1
        magnitude = (1 - fraction) * np.abs(spectrum[:, i]) + fraction * np.abs(spectrum[:, i + 1])
        frame = np.fft.irfft(magnitude * np.exp(1j * phase), n=size, axis=0) * window[:, None]
        p = output_frame * hop
        result[p:p + size] += frame
        weight[p:p + size] += window ** 2
        delta = np.angle(spectrum[:, i + 1]) - np.angle(spectrum[:, i]) - advance
        delta -= 2 * np.pi * np.round(delta / (2 * np.pi))
        phase += advance + delta
    result /= np.maximum(weight[:, None], 1e-8)
    begin = size // 2
    return result[begin:begin + round(len(samples) * factor)]


anvil = read_pcm(anvil_path)
# Published rhythmic clip repeats one hit at 0/3/6/9 seconds; use the first only.
anvil = anvil[:round(.75 * rate)]
anvil /= np.max(np.abs(anvil))
impact = np.zeros((count, 2))
prefix = round(.085 * rate)
impact[:prefix] = anvil[:prefix]
tail_start = round(.065 * rate)
tail = stretch_tail(anvil[tail_start:], 3.0)
length = min(len(tail), count - tail_start)
tail = tail[:length]
times = np.arange(length) / rate
# Lift recorded decay gently; contact remains the loudest event.
tail *= (1 + .70 * np.minimum(times, 1.3))[:, None]
blend = np.minimum(1, np.arange(length) / (prefix - tail_start))[:, None]
impact[tail_start:tail_start + length] *= 1 - blend
impact[tail_start:tail_start + length] += tail * blend
end_fade = np.minimum(1, (length - 1 - np.arange(length)) / (.16 * rate))
impact[tail_start:tail_start + length] *= end_fade[:, None]

choir = read_pcm(choir_path)
choir /= max(np.max(np.abs(choir)), .001)
vocal = np.zeros_like(impact)
for delay, gain, swap in [(0, .66, False), (.13, .17, True), (.29, .105, False), (.47, .065, True)]:
    offset = round(delay * rate)
    length = min(len(choir), count - offset)
    times = np.arange(length) / rate
    attack = np.minimum(1, times / .055)
    release = np.where(times < 1.65, 1, .5 + .5 * np.cos(np.pi * np.clip(times - 1.65, 0, 1)))
    release[times >= 2.65] = 0
    source = choir[:length, ::-1] if swap else choir[:length]
    vocal[offset:offset + length] += source * (gain * attack * release)[:, None]


def write_cue(values, path):
    values = values - values.mean(axis=0)
    values *= .84 / np.max(np.abs(values))
    frames = np.arange(count)
    edge = np.minimum(1, np.minimum(frames / (rate * .0005), (count - 1 - frames) / (rate * .01)))
    values *= edge[:, None]
    pcm = np.rint(values * 32767).astype("<i2")
    path.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(path), "wb") as stream:
        stream.setparams((2, 2, rate, count, "NONE", "not compressed"))
        stream.writeframes(pcm.tobytes())
    print(json.dumps({"file": str(path), "seconds": duration, "rate": rate, "channels": 2,
        "peakDbfs": round(20 * math.log10(np.max(np.abs(pcm.astype(float))) / 32768), 2),
        "clippedSamples": int(np.count_nonzero(np.abs(pcm.astype(float)) >= 32767)),
        "tailStretch": 3.0, "sha256": hashlib.sha256(path.read_bytes()).hexdigest()}))


write_cue(impact, preview_path)
write_cue(impact + vocal, cue_path)
write_cue(vocal, cue_path.with_name("hero_levelup_voice_only.wav"))
