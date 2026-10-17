"""Read-only repository-derived camera/clearance diagram. Reuses v1 glyph SVG bytes.
Outputs only new v2 review artifacts; never imports or writes Unity assets.
"""
from pathlib import Path
from math import cos, radians
from xml.sax.saxutils import escape
import re, hashlib, json
HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[2]
INK='#eee3c5'; BG='#243030'; SAFE='#9bc5d5'
asset=ROOT/'Assets/Stage3/ResourceDistricts/ResourceDistrictArt.asset'
scene=ROOT/'Assets/Scenes/Stage3Rapid3DGamePilot.unity'
follow=ROOT/'Assets/Stage2/Integration/Runtime/Stage2CameraFollow.cs'
src=asset.read_text()
scale=float(re.search(r'^  landmarkScale: ([\d.]+)',src,re.M)[1])
landmarks=src.split('  landmarks:')[1].split('\n  trees:')[0]
clearances={}
for block in re.split(r'\n  - kind: ',landmarks)[1:]:
    kind=int(block.splitlines()[0]); m=re.search(r'productionClearance: \{x: ([\d.]+), y: ([\d.]+)\}',block)
    facing=float(re.search(r'productionFacingDegrees: ([\d.-]+)',block)[1]); assert facing==0
    clearances[kind]=tuple(float(v)*scale for v in m.groups())
angle=float(re.search(r'Quaternion.Euler\(([-\d.]+)f, 0f, 0f\)',follow.read_text())[1])
assert angle==-35 and '  orthographic: 1' in scene.read_text()
sy=cos(radians(angle)); px_per_world=120; inset=.12
LABELS=[('sawmill','Лесопилка',0),('quarry','Каменоломня',1),('iron_mine','Железная шахта',2)]
serial=0
def glyph(name,x,y,size,vertical=1):
    global serial
    serial+=1
    content=(HERE/f'{name}_astra_v1.svg').read_text()
    inner=content[content.index('<g '):content.rindex('</svg>')]
    inner=re.sub(r'cut_(\d+)',lambda m:f'v2_{serial}_cut_{m[1]}',inner)
    return f'<g transform="translate({x} {y}) scale({size/256} {size/256*vertical})">{inner}</g>'
def txt(x,y,s,size=16,color=INK,weight=400,anchor='start'):
    return f'<text x="{x}" y="{y}" font-size="{size}" fill="{color}" font-weight="{weight}" text-anchor="{anchor}">{escape(s)}</text>'
def svg(body,w,h):
    return f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" viewBox="0 0 {w} {h}"><title>Площадки: проекция и поля знака — Astra 02</title><style>text{{font-family:Arial,Helvetica,sans-serif}}</style>{body}</svg>\n'
def rect(cx,cy,w,h,color,stroke,fill='none',dash=''):
    # All dimensions are in world XY; same orthographic transform as the glyph.
    ww=w*px_per_world; hh=h*px_per_world*sy
    return f'<rect x="{cx-ww/2}" y="{cy-hh/2}" width="{ww}" height="{hh}" fill="{fill}" stroke="{color}" stroke-width="{stroke}" {dash}/>'
p=['<rect width="1260" height="1030" fill="#172120"/>',txt(48,44,'РЕСУРСНЫЕ ПЛОЩАДКИ',14,'#a5b3a5',700),txt(1212,44,'ASTRA · ПРАВКА ПРОЕКЦИИ',14,'#a5b3a5',700,'end'),txt(48,90,'Знаки внутри площадок',34,INK,700),txt(48,125,'Иконки сохранены. Исправлены наклон площадки и отступы от её границ.',17,'#a9b6ae')]
for i,(name,label,kind) in enumerate(LABELS):
    x=48+i*396; cx=x+186
    p += [f'<rect x="{x}" y="153" width="372" height="370" rx="16" fill="{BG}"/>',txt(x+24,185,f'0{i+1}',13,'#a5b3a5',700),glyph(name,cx-90,179,180),txt(cx,389,label,24,INK,700,'middle')]
    for j,size in enumerate([32,48,64]):
        xx=x+78+j*108
        p += [glyph(name,xx-size/2,448-size/2,size),txt(xx,504,f'{size} px',12,'#a9b6ae',400,'middle')]
p += [txt(48,566,'ПЛОЩАДКА В ПРОЕКЦИИ ИГРОВОЙ КАМЕРЫ',15,'#a5b3a5',700),txt(48,594,'Одинаковая проекция для границы, зоны отступов и знака. Масштаб всех трёх площадок общий.',16,'#a9b6ae')]
# Legend outside the examples so it cannot be mistaken for extra stamp artwork.
p += ['<path d="M48 626H89" stroke="#eee3c5" stroke-width="2"/>',txt(101,631,'Граница площадки под здание',14),f'<path d="M542 626H583" stroke="{SAFE}" stroke-width="2" stroke-dasharray="6 5"/>',txt(595,631,'Внутренняя safe-зона знака — предложение',14,SAFE)]
measurements=[]
for i,(name,label,kind) in enumerate(LABELS):
    x=48+i*396; cx=x+186; cy=773; w,h=clearances[kind]
    iw=w*(1-2*inset); ih=h*(1-2*inset)
    # Uniform fit in world space. Keep another5% of the short safe side around
    # the complete256px SVG artboard; tool shapes are never stretched to pad ratio.
    side=min(iw,ih)*.9; size=side*px_per_world
    p += [f'<rect x="{x}" y="655" width="372" height="246" rx="16" fill="#636c57"/>',rect(cx,cy,w,h,INK,2,'#75785f'),rect(cx,cy,iw,ih,SAFE,1.7,dash='stroke-dasharray="6 5"'),glyph(name,cx-size/2,cy-size*sy/2,size,sy),txt(cx,883,f'{w:g} × {h:g} · размер площадки в мире',12,'#ece7cf',400,'middle')]
    measurements.append({'name':name,'clearance_world':[w,h],'safe_area_world':[iw,ih],'glyph_artboard_world_side':side,'projected_clearance_pixels':[w*px_per_world,h*px_per_world*sy]})
p += [txt(48,933,'Safe-зона: отступ 12% с каждой стороны только для рисунка. Это не новая зона коллизии.',15,SAFE),txt(48,961,'Схема по данным проекта: камера без поворота вбок, сжатие по вертикали ×0,819. Не кадр Unity.',14,'#a9b6ae'),txt(48,997,'17.10.2026 · эскиз для согласования · брусок остаётся предложением знака металла',13,'#82948b')]
body=''.join(p)
(HERE/'approval_sheet_astra_v2.svg').write_text(svg(body,1260,1030))
Path('/tmp/not-3a-placement-icons-astra-v2-preview.svg').write_text(svg(body,1260,1260))
(HERE/'projection_astra_v2.json').write_text(json.dumps({'camera_euler_degrees':[angle,0,0],'projection':'screenX=worldX; screenY=cos(35deg)*worldY, up-positive before SVG Y inversion','y_scale':sy,'landmark_scale':scale,'pixels_per_world_unit':px_per_world,'safe_inset_fraction_per_edge_proposal':inset,'z':'All local diagram geometry on same XY plane. Constant stamp z=-0.25 affects screen translation, not size/shape; coordinates centered on pad.','runtime_marker_note':'Current Pad uses clearance*.85 and brackets at +/-.48; outer contour here is full ResourceProductionPoint.Clearance, not existing marker brackets.','measurements':measurements},indent=2)+'\n')
print('v2 SVG and exact projection metadata written')
