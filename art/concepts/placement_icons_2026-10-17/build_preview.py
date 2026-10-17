"""Original SVG construction-site icon concept; preview only, no Unity import."""
from pathlib import Path
from xml.sax.saxutils import escape

HERE = Path(__file__).resolve().parent
INK = '#f2e6c5'
BG = '#202a2c'

PICK = '''
  <path d="M53 25 31 57" fill="none" stroke="currentColor" stroke-width="8" stroke-linecap="round"/>
  <path d="M14 36Q30 13 54 14Q75 15 87 37L68 27Q52 22 37 27Z" fill="currentColor"/>
'''
GLYPHS = {
    'sawmill': '''
  <path d="M59 23 29 63" fill="none" stroke="currentColor" stroke-width="8" stroke-linecap="round"/>
  <path d="m53 16 10-5q20 3 24 23L75 44Q69 31 53 31L47 25Z" fill="currentColor"/>
  <path d="M26 69h46q11 0 11 12T72 93H26Z" fill="currentColor"/>
  <ellipse cx="26" cy="81" rx="12" ry="12" fill="currentColor"/>
  <ellipse cx="26" cy="81" rx="7" ry="8" fill="none" stroke="var(--cut)" stroke-width="3.5"/>
  <path d="M47 75h23M47 86h16" fill="none" stroke="var(--cut)" stroke-width="3.5" stroke-linecap="round"/>
''',
    'quarry': PICK + '''
  <path d="m28 72 17-12 22 5 17 18-8 10H21l-5-9Z" fill="currentColor"/>
  <path d="m45 63-7 17 17 10M39 80l28-12M39 80l-18 5" fill="none" stroke="var(--cut)" stroke-width="3.5" stroke-linejoin="round"/>
''',
    'iron_mine': PICK + '''
  <path d="m24 64 48 0 14 16-8 13H15L9 80Z" fill="currentColor"/>
  <path d="M11 80h73M25 65l-7 15 5 13M72 65l6 15-5 13" fill="none" stroke="var(--cut)" stroke-width="3.5" stroke-linejoin="round"/>
'''
}
LABELS = [('sawmill', 'Лесопилка', 'Топор + бревно'),
          ('quarry', 'Каменоломня', 'Кирка + камень'),
          ('iron_mine', 'Железная шахта', 'Кирка + слиток')]

serial = 0
def symbol(name, x, y, size, color=INK, cut=BG, scale_y=1):
    global serial
    serial += 1
    ident = f'negative_space_{serial}'
    geometry = GLYPHS[name].replace('var(--cut)', '#000')
    return (f'<g transform="translate({x} {y}) scale({size/100} {size/100*scale_y})" '
            f'color="#fff"><defs><mask id="{ident}" maskUnits="userSpaceOnUse" '
            f'x="0" y="0" width="100" height="100" style="mask-type:luminance">'
            f'{geometry}</mask></defs><rect width="100" height="100" fill="{color}" '
            f'mask="url(#{ident})"/></g>')

def text(x, y, value, size=18, fill=INK, weight=400, anchor='start'):
    return (f'<text x="{x}" y="{y}" font-size="{size}" fill="{fill}" '
            f'font-weight="{weight}" text-anchor="{anchor}">{escape(value)}</text>')

def svg(body, w, h, title):
    return (f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" '
            f'viewBox="0 0 {w} {h}" role="img"><title>{escape(title)}</title>'
            '<style>text{font-family:Arial,Helvetica,sans-serif}</style>' + body + '</svg>\n')

# SVG masks give the resource facets and log grain real transparent negative space.
for name, label, _ in LABELS:
    (HERE / f'{name}_v1.svg').write_text(svg(symbol(name, 0, 0, 100), 100, 100, label))

parts = [f'<rect width="1260" height="1030" fill="#141d20"/>',
         text(48, 50, 'ЗНАКИ РЕСУРСНЫХ ПЛОЩАДОК', 15, '#b9bda9', 700),
         text(48, 99, 'Лес · камень · металл', 37, INK, 700),
         text(1212, 50, 'ЭСКИЗ 01 · ДЛЯ АПРУВА', 13, '#b9bda9', 700, 'end'),
         text(48, 133, 'Вектор · один светлый тон · крупные формы и открытые просветы', 17, '#abb5b3')]

for i, (name, label, hint) in enumerate(LABELS):
    x = 48 + i * 396
    parts += [f'<rect x="{x}" y="166" width="372" height="438" rx="18" fill="{BG}"/>',
              text(x+24, 202, f'0{i+1}', 13, '#a8b6af', 700),
              symbol(name, x+86, 219, 200),
              text(x+186, 458, label, 25, INK, 700, 'middle'),
              text(x+186, 487, hint, 16, '#a8b6af', 400, 'middle'),
              f'<path d="M{x+24} 508H{x+348}" stroke="#3a4849"/>']
    for k, sz in enumerate([32, 48, 64]):
        cx = x + 77 + k*108
        parts += [symbol(name, cx-sz/2, 543-sz/2, sz),
                  text(cx, 586, f'{sz} px', 12, '#a8b6af', 400, 'middle')]

parts += [text(48, 660, 'ПРОЕКЦИЯ НА ЗЕМЛЮ', 14, '#b9bda9', 700),
          text(48, 687, 'Схема наземной метки с наклоном камеры 55° — не кадр из игры', 16, '#abb5b3')]

for i, (name, _, _) in enumerate(LABELS):
    x = 48 + i*396
    ground = '#73775c'
    parts += [f'<rect x="{x}" y="714" width="372" height="244" rx="18" fill="{ground}"/>',
              f'<path d="M{x+24} 842  {x+156} 741 {x+350} 817 {x+218} 933Z" fill="#8b8766"/>']
    # Restrained schematic stones give scale without imitating game artwork.
    for dx, dy, r in [(29,38,3),(70,51,2),(294,42,3),(318,158,4),(47,181,3),(260,198,2)]:
        parts.append(f'<ellipse cx="{x+dx}" cy="{714+dy}" rx="{r*1.6}" ry="{r}" fill="#aaa68a" opacity=".5"/>')
    # sin(55 degrees)=.819: vertical ground-plane compression for the fixed camera.
    parts += [symbol(name, x+122, 755, 128, '#f2e6c5', '#8b8766', .819),
              text(x+186, 933, '128 × 105 px · наклон 55°', 12, '#e1dfc9', 400, 'middle')]

parts += [text(48, 1000, 'Оригинальные SVG-эскизы · 17.10.2026 · ожидают художественного согласования', 13, '#82918e')]
(HERE / 'approval_sheet_v1.svg').write_text(svg(''.join(parts), 1260, 1030, 'Ресурсные площадки — эскиз 01'))
# Quick Look uses a square thumbnail viewport. Pad only its temporary render source.
Path('/tmp/not-3a-placement-icons-preview.svg').write_text(svg(''.join(parts), 1260, 1260, 'Ресурсные площадки — эскиз 01'))
print(HERE / 'approval_sheet_v1.svg')
