"""Original construction-site pictograms by Astra. Approval art only; no Unity writes.
Run this script, qlmanage its printed temporary square SVG, then crop with the
existing CoreGraphics preview utility. All negative-space cuts stay transparent.
"""
from pathlib import Path
from xml.sax.saxutils import escape

HERE = Path(__file__).resolve().parent
INK = '#eee3c5'
BG = '#243030'
# Tools are constructed upright, then rotated as complete rigid objects.
# The handle stays a straight tapered wooden shaft, seated in the head's eye.
# Pick: one long narrow point and a short, flat chisel end; no arch/crescent.
HANDLE = '<path d="M124 43H136L138 174Q131 180 124 174Z" fill="white"/>'
PICK = '''<g transform="rotate(28 130 50)">
''' + HANDLE + '''
<path d="M49 57 83 39 115 33 145 34 177 43 190 48 185 61 169 55 143 49 116 47 85 49Z" fill="white"/>
<path d="M123 32H137V50H123Z" fill="white"/>
<path d="M126 39H134V44H126Z" fill="black"/>
</g>'''
AXE = '''<g transform="rotate(28 130 50)">
''' + HANDLE + '''
<path d="M108 33H139L177 21Q185 48 177 77L139 59H108Z" fill="white"/>
<path d="M126 39H134V52H126Z" fill="black"/>
<path d="M169 29Q175 49 169 68" fill="none" stroke="black" stroke-width="4"/>
</g>'''
# Cylinder with a single clear end-grain ring; no tiny bark decoration.
LOG = '''
<path d="M64 184H191C217 184 217 232 191 232H64Z" fill="white"/>
<ellipse cx="64" cy="208" rx="22" ry="24" fill="white"/>
<ellipse cx="64" cy="208" rx="14" ry="17" fill="none" stroke="black" stroke-width="5"/>
<path d="M91 196H186" stroke="black" stroke-width="5"/>
'''
# A single irregular quarry block, deliberately unlike the regular metal bar.
STONE = '''
<path d="M55 212 80 182 116 164 148 178 165 212 145 237H75Z M178 217 194 210 211 224 205 237H179L171 229Z" fill="white"/>
<path d="M82 184 108 202 116 169M108 202 143 181M108 202 91 232M110 203 159 214" fill="none" stroke="black" stroke-width="6" stroke-linejoin="round"/>
'''
# A restrained cast iron bar: broad top, trapezoidal end and long face.
# Pictographic proposal, not a new smelting feature/resource tier.
INGOT = '''
<path d="M57 208 72 181 170 172 199 197 193 230 82 239Z" fill="white"/>
<path d="M73 184 94 209 195 199M94 209 84 235M60 208 91 210" fill="none" stroke="black" stroke-width="6" stroke-linejoin="round"/>
'''
GLYPHS = {'sawmill': AXE+LOG, 'quarry': PICK+STONE, 'iron_mine': PICK+INGOT}
LABELS = [('sawmill','Лесопилка','Топор и бревно'),('quarry','Каменоломня','Кирка и колотый камень'),('iron_mine','Железная шахта','Кирка и металлический брусок*')]
serial=0
def glyph(name,x,y,size,color=INK,sy=1):
    global serial
    serial+=1
    ident=f'cut_{serial}'
    return f'<g transform="translate({x} {y}) scale({size/256} {size/256*sy})"><defs><mask id="{ident}" maskUnits="userSpaceOnUse" x="0" y="0" width="256" height="256" style="mask-type:luminance">{GLYPHS[name]}</mask></defs><rect width="256" height="256" fill="{color}" mask="url(#{ident})"/></g>'
def txt(x,y,s,size=16,color=INK,weight=400,anchor='start'):
    return f'<text x="{x}" y="{y}" font-size="{size}" fill="{color}" font-weight="{weight}" text-anchor="{anchor}">{escape(s)}</text>'
def svg(body,w,h,title):
    return f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" viewBox="0 0 {w} {h}" role="img"><title>{escape(title)}</title><style>text{{font-family:Arial,Helvetica,sans-serif}}</style>{body}</svg>\n'
for name,label,_ in LABELS:
    (HERE/f'{name}_astra_v1.svg').write_text(svg(glyph(name,0,0,256),256,256,label))
p=[f'<rect width="1260" height="1030" fill="#172120"/>',txt(48,46,'РЕСУРСНЫЕ ПЛОЩАДКИ',14,'#a5b3a5',700),txt(1212,46,'ASTRA · ЭСКИЗ 02',14,'#a5b3a5',700,'end'),txt(48,91,'Лес · камень · металл',34,INK,700),txt(48,126,'Прямые рукояти · узнаваемые инструменты · один цвет и прозрачные просветы',17,'#a9b6ae')]
for i,(name,label,hint) in enumerate(LABELS):
    x=48+i*396
    p += [f'<rect x="{x}" y="154" width="372" height="464" rx="16" fill="{BG}"/>',txt(x+24,187,f'0{i+1}',13,'#a5b3a5',700),glyph(name,x+66,189,240),txt(x+186,458,label,25,INK,700,'middle'),txt(x+186,485,hint,15,'#a9b6ae',400,'middle'),f'<path d="M{x+24} 505H{x+348}" stroke="#41504b"/>']
    for j,size in enumerate([32,48,64]):
        cx=x+78+j*108
        p += [glyph(name,cx-size/2,553-size/2,size),txt(cx,599,f'{size} px',12,'#a9b6ae',400,'middle')]
p += [txt(48,657,'НАКЛОН НАЗЕМНОЙ МЕТКИ',14,'#a5b3a5',700),txt(48,684,'Схема при 55°: тот же рисунок, сжатие по вертикали ×0,819. Это не кадр Unity.',16,'#a9b6ae')]
for i,(name,_,_) in enumerate(LABELS):
    x=48+i*396
    p += [f'<rect x="{x}" y="707" width="372" height="224" rx="16" fill="#73765d"/>',f'<path d="M{x+32} 806  {x+155} 729 {x+340} 805 {x+216} 893Z" fill="#878467"/>',glyph(name,x+112,729,148,INK,.819),txt(x+186,909,'148 × 121 px · схема проекции',12,'#ece7cf',400,'middle')]
p += [txt(48,966,'* Брусок — вариант знака металла для согласования, без новой игровой механики.',14,'#a9b6ae'),txt(48,998,'Авторская векторная геометрия · 17.10.2026 · только эскизы, без изменений Unity',13,'#82948b')]
body=''.join(p)
(HERE/'approval_sheet_astra_v1.svg').write_text(svg(body,1260,1030,'Ресурсные площадки — Astra, эскиз 02'))
Path('/tmp/not-3a-placement-icons-astra-preview.svg').write_text(svg(body,1260,1260,'Ресурсные площадки — Astra'))
print('/tmp/not-3a-placement-icons-astra-preview.svg')
