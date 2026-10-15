// Join unmodified, actual Unity camera frames into a looping review recording.
const fs = require('node:fs');
const path = require('node:path');
const sharp = require(process.env.NOT3A_SHARP_MODULE || 'sharp');
const folder = path.resolve('Artifacts/S3-SKELETON-SPRITE/frames');
const frames = fs.readdirSync(folder).filter(name => /^frame_[0-9]{3}\.png$/.test(name))
  .sort().map(name => path.join(folder, name));
if (frames.length !== 60) throw new Error('Expected 60 actual Game-camera frames.');
sharp(frames, { join: { animated: true } })
  .gif({ loop: 0, delay: frames.map(() => 67), colours: 192, effort: 5, dither: .4 })
  .toFile('art/review/skeleton_sprite_portal_walk.gif')
  .then(info => console.log(JSON.stringify(info)))
  .catch(error => { console.error(error); process.exitCode = 1; });
