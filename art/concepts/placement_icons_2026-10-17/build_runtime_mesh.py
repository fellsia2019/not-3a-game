"""Deterministic offline SVG mask -> triangle source, using only Python stdlib.
Supports only the explicit approved SVG subset, rejects anything unknown.
Curves/ellipses flattened to <=0.05 SVG-unit deviation. Planar vertical slabs
split at every vertex/intersection, then painter-ordered intervals reproduce
white/black luminance mask booleans. No rasterization or runtime SVG parser.
"""
import math,re,json,hashlib
from pathlib import Path
import xml.etree.ElementTree as ET
HERE=Path(__file__).resolve().parent; ROOT=HERE.parents[2]
EPS=1e-8; TOL=.05

def curve(points):
    a,b=points[0],points[-1]; dx,dy=b[0]-a[0],b[1]-a[1]; length=math.hypot(dx,dy)
    deviation=max(abs((p[0]-a[0])*dy-(p[1]-a[1])*dx)/max(length,EPS) for p in points[1:-1])
    if deviation<=TOL:return [a,b]
    levels=[points]
    while len(levels[-1])>1:levels.append([((p[0]+q[0])/2,(p[1]+q[1])/2) for p,q in zip(levels[-1],levels[-1][1:])])
    return curve([x[0] for x in levels])[:-1]+curve([x[-1] for x in levels[::-1]])
def path(d):
    tokens=re.findall(r'[MLHVQCZmlhvqcz]|[-+]?(?:\d*\.\d+|\d+)(?:e[-+]?\d+)?',d)
    i=0;command=None;cursor=(0,0);parts=[];points=[];start=None
    while i<len(tokens):
        if tokens[i].isalpha():command=tokens[i];i+=1
        if command in 'Zz':
            if points:parts.append((points,True));points=[]
            cursor=start;command=None;continue
        relative=command.islower(); c=command.upper(); n={'M':2,'L':2,'H':1,'V':1,'Q':4,'C':6}[c]
        values=list(map(float,tokens[i:i+n]));i+=n
        def point(k):return (values[k]+(cursor[0] if relative else 0),values[k+1]+(cursor[1] if relative else 0))
        if c in ('M','L'):
            p=point(0)
            if c=='M':
                if points:parts.append((points,False))
                points=[];start=p;command='l' if relative else 'L'
            points.append(p);cursor=p
        elif c=='H':cursor=(values[0]+(cursor[0] if relative else 0),cursor[1]);points.append(cursor)
        elif c=='V':cursor=(cursor[0],values[0]+(cursor[1] if relative else 0));points.append(cursor)
        else:
            control=[cursor]+[point(k) for k in range(0,n,2)]
            points.extend(curve(control)[1:]);cursor=control[-1]
    if points:parts.append((points,False))
    return parts

def ellipse(cx,cy,rx,ry):
    count=max(24,math.ceil(math.pi/math.acos(1-TOL/max(rx,ry))))
    return [(cx+rx*math.cos(i*2*math.pi/count),cy+ry*math.sin(i*2*math.pi/count)) for i in range(count)]
def cross(a,b):return a[0]*b[1]-a[1]*b[0]
def sub(a,b):return (a[0]-b[0],a[1]-b[1])
def intersection(a,b,c,d):
    r=sub(b,a);s=sub(d,c);den=cross(r,s)
    if abs(den)<EPS:return None
    t=cross(sub(c,a),s)/den;u=cross(sub(c,a),r)/den
    if -EPS<t<1+EPS and -EPS<u<1+EPS:return (a[0]+t*r[0],a[1]+t*r[1])

def shapes(svg):
    root=ET.parse(svg).getroot();mask=next(x for x in root.iter() if x.tag.endswith('mask'));out=[]
    def walk(e,angle=0,origin=(0,0)):
        if e.get('transform'):
            m=re.fullmatch(r'rotate\(([-\d.]+) ([-\d.]+) ([-\d.]+)\)',e.get('transform'));assert m
            angle=float(m[1]);origin=(float(m[2]),float(m[3]))
        def transform(p):
            c=math.cos(math.radians(angle));s=math.sin(math.radians(angle));x=p[0]-origin[0];y=p[1]-origin[1]
            return (origin[0]+x*c-y*s,origin[1]+x*s+y*c)
        def add(rings,white):out.append(([list(map(transform,r)) for r in rings],white))
        tag=e.tag.split('}')[-1]
        if tag in ('mask','g'):
            for child in e:walk(child,angle,origin)
            return
        assert tag in ('path','ellipse'),tag
        if tag=='ellipse':
            cx,cy,rx,ry=[float(e.get(k)) for k in ('cx','cy','rx','ry')]
            parts=[(ellipse(cx,cy,rx,ry),True)]
        else:parts=path(e.get('d'))
        if e.get('fill','black')!='none':add([p for p,_ in parts],e.get('fill','black')=='white')
        if not e.get('stroke'):return
        width=float(e.get('stroke-width','1'));radius=width/2;white=e.get('stroke')=='white'
        if tag=='ellipse':
            add([ellipse(cx,cy,rx+radius,ry+radius),ellipse(cx,cy,rx-radius,ry-radius)],white);return
        for points,closed in parts:
            pp=points+[points[0]] if closed else points
            normals=[]
            for a,b in zip(pp,pp[1:]):
                dx,dy=b[0]-a[0],b[1]-a[1];length=math.hypot(dx,dy)
                n=(-dy/length*radius,dx/length*radius);normals.append(n)
                add([[(a[0]-n[0],a[1]-n[1]),(b[0]-n[0],b[1]-n[1]),(b[0]+n[0],b[1]+n[1]),(a[0]+n[0],a[1]+n[1])]],white)
            for j in range(1,len(pp)-1):
                p=pp[j];n0,n1=normals[j-1],normals[j]
                if e.get('stroke-linejoin')=='round':add([ellipse(*p,radius,radius)],white)
                else:
                    # Default SVG miter joint, limited to4xhalfwidth.
                    dot=n0[0]*n1[0]+n0[1]*n1[1];factor=radius*radius/(radius*radius+dot)
                    m=((n0[0]+n1[0])*factor,(n0[1]+n1[1])*factor)
                    if math.hypot(*m)>4*radius:m=n1
                    for sign in (-1,1):add([[p,(p[0]+sign*n0[0],p[1]+sign*n0[1]),(p[0]+sign*m[0],p[1]+sign*m[1]),(p[0]+sign*n1[0],p[1]+sign*n1[1])]],white)
    walk(mask)
    return out

def tessellate(paints):
    edges=[]; groups=[];xs=[]
    for rings,white in paints:
        group=[]
        for ring in rings:
            for a,b in zip(ring,ring[1:]+ring[:1]):
                xs.extend((a[0],b[0]))
                if abs(a[0]-b[0])<EPS:continue
                group.append(len(edges));edges.append((a,b))
        groups.append((group,white))
    for i,(a,b) in enumerate(edges):
        for c,d in edges[i+1:]:
            hit=intersection(a,b,c,d)
            if hit:xs.append(hit[0])
    xs=sorted(set(round(x,8) for x in xs));verts=[];tri=[]
    def y(edge,x):
        a,b=edges[edge];return a[1]+(x-a[0])*(b[1]-a[1])/(b[0]-a[0])
    for x0,x1 in zip(xs,xs[1:]):
        if x1-x0<1e-6:continue
        mid=(x0+x1)/2;events=[]
        for gi,(group,white) in enumerate(groups):
            for edge in group:
                a,b=edges[edge]
                if min(a[0],b[0])<mid<max(a[0],b[0]):events.append((y(edge,mid),gi,edge))
        events.sort();inside=set();active=False;begin=None;i=0
        while i<len(events):
            height=events[i][0];edge=events[i][2]
            while i<len(events) and abs(events[i][0]-height)<1e-7:
                gi=events[i][1]
                if gi in inside:inside.remove(gi)
                else:inside.add(gi)
                i+=1
            visible=groups[max(inside)][1] if inside else False
            if visible and not active:begin=edge
            if active and not visible:
                quad=[(x0,y(begin,x0)),(x1,y(begin,x1)),(x1,y(edge,x1)),(x0,y(edge,x0))]
                base=len(verts);verts.extend(quad)
                for t in [(0,1,2),(0,2,3)]:
                    if abs(cross(sub(quad[t[1]],quad[t[0]]),sub(quad[t[2]],quad[t[0]])))>1e-8:tri.extend(base+k for k in t)
            active=visible
    # Weld to6decimal SVGunits; keeps output compact without changing silhouette.
    unique=[];lookup={};indices=[]
    for index in tri:
        key=tuple(round(v,6) for v in verts[index])
        if key not in lookup:lookup[key]=len(unique);unique.append(key)
        indices.append(lookup[key])
    return unique,indices

if __name__=='__main__':
    content=['// Generated from approved Astra SVGs by art/concepts/placement_icons_2026-10-17/build_runtime_mesh.py.\n// Do not hand edit. SVG coordinates; black mask regions are actual mesh holes.\nnamespace Not3A.Stage2.Defense\n{\n    internal static class Stage2ApprovedPadGeometry\n    {']
    metadata=[]
    for name,label in [('sawmill','Sawmill'),('quarry','Quarry'),('iron_mine','IronMine')]:
        source=HERE/f'{name}_astra_v1.svg';v,t=tessellate(shapes(source))
        content.append(f'        internal static readonly float[] {label}Vertices = {{\n'+''.join('            '+','.join(f'{n:.6f}f' for xy in v[i:i+6] for n in xy)+',\n' for i in range(0,len(v),6))+'        };')
        content.append(f'        internal static readonly int[] {label}Triangles = {{\n'+''.join('            '+','.join(map(str,t[i:i+24]))+',\n' for i in range(0,len(t),24))+'        };')
        metadata.append({'source':str(source.relative_to(ROOT)),'source_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'vertices':len(v),'triangles':len(t)//3})
    content.append('    }\n}\n')
    target=ROOT/'Assets/Stage2/Defense/Runtime/Stage2ApprovedPadGeometry.cs';target.write_text('\n'.join(content))
    (ROOT/'Artifacts/S3-PLACEMENT-ICONS-20261017/tessellation.json').write_text(json.dumps({'curve_max_deviation_svg_units':TOL,'method':'Painter-ordered mask booleans via exact line arrangement and vertical slab triangulation','icons':metadata},indent=2)+'\n')
    print(metadata)
