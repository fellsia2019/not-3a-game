"""Original sculptural resource models: approved stone A / wood B / ore A.
Broad fractured planes, rounded edges, volumetric boughs, baked original albedo.
Concept pixels are never sampled. Blender 5.2 --background --python this_file.
Optional arguments after --: stone, wood, metal (otherwise builds all three).
"""
from pathlib import Path
from itertools import combinations
import json
import math
import random
import sys
import bpy
import bmesh
import numpy as np
from mathutils import Vector
from mathutils.noise import noise_vector

ROOT = Path(__file__).resolve().parents[2]
ASSETS = ROOT / 'Assets/Stage3/VisualPilot'
SOURCE = ROOT / 'art/source'
REVIEW = ROOT / 'art/review'
PARTS = []
if str(SOURCE) not in sys.path:
    sys.path.insert(0, str(SOURCE))


def mesh_object(name, vertices, faces, mat):
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    ob = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(ob)
    mesh.materials.append(mat)
    for face in mesh.polygons: face.use_smooth = True
    PARTS.append(ob)
    return ob


def graph(name):
    mat = bpy.data.materials.new(name); mat.use_nodes = True
    n, l = mat.node_tree.nodes, mat.node_tree.links
    n.clear()
    out = n.new('ShaderNodeOutputMaterial'); emit = n.new('ShaderNodeEmission')
    l.new(emit.outputs[0], out.inputs['Surface'])
    return mat, n, l, emit


def noise(n, l, vector, scale, detail=3):
    node = n.new('ShaderNodeTexNoise')
    node.inputs['Scale'].default_value = scale
    node.inputs['Detail'].default_value = detail
    l.new(vector, node.inputs['Vector'])
    return node


def ramp(n, l, fac, stops):
    node = n.new('ShaderNodeValToRGB')
    for i, (position, color) in enumerate(stops):
        el = node.color_ramp.elements[i] if i < 2 else node.color_ramp.elements.new(position)
        el.position = position; el.color = (*color, 1)
    l.new(fac, node.inputs[0])
    return node.outputs[0]


def mix(n, l, a, b, fac, mode='MIX'):
    node = n.new('ShaderNodeMixRGB'); node.blend_type = mode
    for socket, value in ((node.inputs[1], a), (node.inputs[2], b)):
        if isinstance(value, tuple): socket.default_value = (*value, 1)
        else: l.new(value, socket)
    if isinstance(fac, (int, float)): node.inputs[0].default_value = fac
    else: l.new(fac, node.inputs[0])
    return node.outputs[0]


def ambient(n, l, color, distance=.12, strength=.62):
    ao = n.new('ShaderNodeAmbientOcclusion'); ao.inputs['Distance'].default_value = distance
    ao.samples = 24
    return mix(n, l, color, ao.outputs['Color'], strength, 'MULTIPLY')


def scalar(n, l, operation, a, b=0):
    node=n.new('ShaderNodeMath'); node.operation=operation
    for socket,value in ((node.inputs[0],a),(node.inputs[1],b)):
        if isinstance(value,(int,float)): socket.default_value=value
        else: l.new(value,socket)
    return node.outputs[0]


def rock_material(ore=False):
    mat, n, l, emit = graph('Original ironstone strata' if ore else 'Original weathered granite')
    coord = n.new('ShaderNodeNewGeometry').outputs['Position']
    colors = [(.15, (.095, .111, .13)), (.84, (.225, .242, .259))] if ore else [
        (.18, (.34, .325, .29)), (.82, (.64, .62, .565))]
    base = ramp(n, l, noise(n, l, coord, 4.8).outputs['Fac'], colors)
    grain = ramp(n, l, noise(n, l, coord, 105, 2).outputs['Fac'], [(.18, (.64, .62, .59)), (.82, (1, 1, 1))])
    base = mix(n, l, base, grain, .22, 'MULTIPLY')
    # Warped fracture fields stay continuous across UV islands and mesh faces.
    warp = noise(n, l, coord, 6.5, 2)
    scale = n.new('ShaderNodeVectorMath'); scale.operation = 'SCALE'
    l.new(warp.outputs['Color'], scale.inputs[0]); scale.inputs[3].default_value = .18
    add = n.new('ShaderNodeVectorMath'); add.operation = 'ADD'
    l.new(coord, add.inputs[0]); l.new(scale.outputs['Vector'], add.inputs[1])
    crack = n.new('ShaderNodeTexVoronoi'); crack.feature = 'DISTANCE_TO_EDGE'
    crack.inputs['Scale'].default_value = 2.5 if ore else 3.1
    l.new(add.outputs['Vector'], crack.inputs['Vector'])
    if ore:
        # Two irregular mineral seams follow the cleaved direction. Avoid an
        # all-over cellular web that reads as painted orange outlines.
        separate=n.new('ShaderNodeSeparateXYZ'); l.new(coord,separate.inputs[0])
        diagonal=scalar(n,l,'ADD',separate.outputs['X'],scalar(n,l,'MULTIPLY',separate.outputs['Z'],-.28))
        diagonal=scalar(n,l,'ADD',diagonal,scalar(n,l,'MULTIPLY',separate.outputs['Y'],.31))
        diagonal=scalar(n,l,'ADD',diagonal,scalar(n,l,'MULTIPLY',noise(n,l,coord,12,4).outputs['Fac'],.16))
        seam_a=scalar(n,l,'ABSOLUTE',scalar(n,l,'ADD',diagonal,.13))
        seam_b=scalar(n,l,'ABSOLUTE',scalar(n,l,'ADD',diagonal,-.31))
        distance=scalar(n,l,'MINIMUM',seam_a,seam_b)
        mask = ramp(n, l, distance, [(.006, (1, 1, 1)), (.022, (.95,.95,.95)), (.052, (0, 0, 0))])
        rust = ramp(n, l, noise(n, l, coord, 20, 4).outputs['Fac'], [
            (.18, (.17, .058, .021)), (.58, (.38, .135, .041)), (.86, (.49, .224, .085))])
        base = mix(n, l, base, rust, mask)
    else:
        mask = ramp(n, l, crack.outputs['Distance'], [(.001, (1, 1, 1)), (.009, (0, 0, 0))])
        gate = ramp(n, l, noise(n, l, coord, 3.5, 1).outputs['Fac'], [(.44, (0, 0, 0)), (.57, (1, 1, 1))])
        mask = mix(n, l, mask, gate, 1, 'MULTIPLY')
        base = mix(n, l, base, (.19, .18, .16), mask)
    l.new(ambient(n, l, base), emit.inputs['Color'])
    # Keep a relief shader alongside the albedo emitter for the tangent bake.
    surface=n.new('ShaderNodeBsdfPrincipled'); surface.name='Relief bake surface'
    fine=n.new('ShaderNodeBump'); fine.inputs['Distance'].default_value=.0025
    fine.inputs['Strength'].default_value=.55
    l.new(noise(n,l,coord,48,3).outputs['Fac'],fine.inputs['Height'])
    relief=n.new('ShaderNodeBump'); relief.invert=True
    relief.inputs['Distance'].default_value=.018 if ore else .008
    relief.inputs['Strength'].default_value=.8
    l.new(mask,relief.inputs['Height']); l.new(fine.outputs['Normal'],relief.inputs['Normal'])
    l.new(relief.outputs['Normal'],surface.inputs['Normal'])
    return mat


def foliage_material():
    mat, n, l, emit = graph('Original layered pine needles')
    attr = n.new('ShaderNodeVertexColor'); attr.layer_name = 'BoughTint'
    coord = n.new('ShaderNodeNewGeometry').outputs['Position']
    grain = ramp(n, l, noise(n, l, coord, 42, 2).outputs['Fac'], [(.2, (.7, .76, .66)), (.8, (1, 1, 1))])
    base = mix(n, l, attr.outputs['Color'], grain, .28, 'MULTIPLY')
    l.new(ambient(n, l, base, .12, .66), emit.inputs['Color'])
    return mat


def bark_material():
    mat, n, l, emit = graph('Original warm longitudinal bark')
    geom = n.new('ShaderNodeNewGeometry')
    stretch = n.new('ShaderNodeVectorMath'); stretch.operation = 'MULTIPLY'
    l.new(geom.outputs['Position'], stretch.inputs[0]); stretch.inputs[1].default_value = (4, 4, .38)
    color = ramp(n, l, noise(n, l, stretch.outputs[0], 10).outputs['Fac'], [
        (.25, (.075, .026, .008)), (.45, (.20, .075, .021)), (.65, (.35, .153, .049)), (.85, (.42, .208, .081))])
    l.new(ambient(n, l, color, .10, .7), emit.inputs['Color'])
    return mat


def poly_rock(name, center, scale, seed, mat, bevel=.035, lean=0):
    """Intersect half-spaces to sculpt broad asymmetric faces, not ico spheres."""
    rng = random.Random(seed); planes = []
    for i in range(8):
        angle = math.tau*i/8 + rng.uniform(-.10, .10)
        planes.append((np.array([math.cos(angle), math.sin(angle), rng.uniform(.03, .32)]), rng.uniform(.85, 1.02)))
    planes.extend([(np.array([.10, -.07, 1]), 1), (np.array([0, 0, -1]), 1)])
    for i in range(5):
        angle = math.tau*i/5 + .26
        planes.append((np.array([math.cos(angle)*.75, math.sin(angle)*.75, 1]), rng.uniform(1.19, 1.37)))
    points = []; normals = np.array([p[0] for p in planes]); distances = np.array([p[1] for p in planes])
    for indices in combinations(range(len(planes)), 3):
        a = normals[list(indices)]
        if abs(np.linalg.det(a)) < .0001: continue
        p = np.linalg.solve(a, distances[list(indices)])
        if np.all(normals @ p <= distances+.00001) and not any(np.linalg.norm(p-q)<.0001 for q in points): points.append(p)
    bm = bmesh.new()
    for p in points:
        bm.verts.new((center[0]+(p[0]+lean*p[2])*scale[0], center[1]+p[1]*scale[1], center[2]+p[2]*scale[2]))
    bmesh.ops.convex_hull(bm, input=list(bm.verts), use_existing_faces=False)
    bmesh.ops.dissolve_limit(bm, angle_limit=.0001, verts=list(bm.verts), edges=list(bm.edges))
    mesh = bpy.data.meshes.new(name); bm.to_mesh(mesh); bm.free()
    ob = bpy.data.objects.new(name, mesh); bpy.context.collection.objects.link(ob); mesh.materials.append(mat)
    bpy.context.view_layer.objects.active = ob; ob.select_set(True)
    for face in mesh.polygons: face.use_smooth = True
    mod = ob.modifiers.new('Soft weathered edges', 'BEVEL'); mod.width = bevel; mod.segments = 4
    mod.use_clamp_overlap = False
    bpy.ops.object.modifier_apply(modifier=mod.name)
    # Resample the sculpture before soft erosion: broad faces survive while
    # highlights roll across real rounded edges instead of flat polygon seams.
    ob.data.remesh_voxel_size = min(scale)*.047
    bpy.ops.object.voxel_remesh()
    mod = ob.modifiers.new('Weathered transitions', 'SMOOTH'); mod.factor = .85; mod.iterations = 4
    bpy.ops.object.modifier_apply(modifier=mod.name)
    for vertex in ob.data.vertices:
        vertex.co += noise_vector(vertex.co*14+Vector((seed,0,0))) * min(scale)*.013
    mod = ob.modifiers.new('Keep sculpture silhouette', 'DECIMATE'); mod.ratio = .20
    bpy.ops.object.modifier_apply(modifier=mod.name)
    for face in ob.data.polygons: face.use_smooth = True
    ob.select_set(False); PARTS.append(ob)


def trunk(mat):
    vertices, faces = [], []; segments = 32
    rings = [(0,.22),(.08,.18),(.24,.125),(.55,.11),(.95,.085),(1.40,.055),(1.85,.02)]
    for z, radius in rings:
        for i in range(segments):
            a = math.tau*i/segments
            r = radius*(1+.06*math.sin(a*9+z*3)+.035*math.cos(a*13-z*4))
            vertices.append((r*math.cos(a)+.015*math.sin(z*3),r*math.sin(a),z))
    for j in range(len(rings)-1):
        for i in range(segments):
            k=j*segments+i; nxt=j*segments+(i+1)%segments
            faces.append((k,nxt,nxt+segments,k+segments))
    faces.extend([tuple(reversed(range(segments))),tuple(range((len(rings)-1)*segments,len(rings)*segments))])
    mesh_object('Fluted tapering trunk',vertices,faces,mat)
    for i in range(7):
        a=i*math.tau/7+.13; vertices,faces=[],[]
        for j in range(9):
            t=j/8; r=.07+.25*t; z=.27*(1-t)**1.8+.012; width=.073*(1-t)+.012
            for k in range(12):
                phi=math.tau*k/12
                vertices.append((r*math.cos(a)-math.sin(a)*width*math.cos(phi),r*math.sin(a)+math.cos(a)*width*math.cos(phi),z+width*.65*math.sin(phi)))
        for j in range(8):
            for k in range(12): faces.append((j*12+k,j*12+(k+1)%12,(j+1)*12+(k+1)%12,(j+1)*12+k))
        faces.extend([tuple(reversed(range(12))),tuple(range(96,108))])
        mesh_object('Root %02d'%i,vertices,faces,mat)


def needle_lobe(name, angle, start_radius, end_radius, start_z, end_z, width, mat, shade):
    """Sculpted curved conifer finger, convex top rib and gently curled tip."""
    verts,faces,tints=[],[],[]; rows,sides=17,12
    direction=Vector((math.cos(angle),math.sin(angle),0)); side=Vector((-math.sin(angle),math.cos(angle),0))
    for j in range(rows):
        t=j/(rows-1); radius=start_radius+(end_radius-start_radius)*t
        z=start_z+(end_z-start_z)*t+.05*math.sin(math.pi*t)+.085*t**5
        half=max(width*math.sin(math.pi*(.025+.974*t))**.45*(1-.15*t),.0018)
        for k in range(sides):
            phi=math.tau*k/sides; upper=math.sin(phi)
            p=direction*radius+side*(half*math.cos(phi))+Vector((0,0,z+upper*half*(.24 if upper<0 else .42)))
            verts.append(p)
            tint=shade*(.70+.24*t+.12*max(upper,0)+.04*max(upper,0)**6)
            tints.append((.155*tint,.315*tint,.070*tint,1))
    for j in range(rows-1):
        for k in range(sides): faces.append((j*sides+k,j*sides+(k+1)%sides,(j+1)*sides+(k+1)%sides,(j+1)*sides+k))
    faces.extend([tuple(reversed(range(sides))),tuple(range((rows-1)*sides,rows*sides))])
    ob=mesh_object(name,verts,faces,mat)
    attr=ob.data.color_attributes.new(name='BoughTint',type='FLOAT_COLOR',domain='POINT')
    for i,color in enumerate(tints): attr.data[i].color=color


def granite_material():
    """Quiet warm/cool stone paint, without an all-over procedural crack web."""
    mat,n,l,emit=graph('Granite authored planes')
    coord=n.new('ShaderNodeNewGeometry').outputs['Position']
    tint=n.new('ShaderNodeVertexColor'); tint.layer_name='GranitePlanes'
    # Project-original imagegen texture; the concept is a palette/style
    # reference only. Box projection is baked to the one runtime UV atlas.
    paint=n.new('ShaderNodeTexImage')
    paint.image=bpy.data.images.load(str(SOURCE/'stone_granite_painted_albedo.png'),check_existing=True)
    paint.image.pack(); paint.projection='BOX'; paint.projection_blend=.22
    l.new(coord,paint.inputs['Vector'])
    color=mix(n,l,paint.outputs['Color'],tint.outputs['Color'],.20)
    color=mix(n,l,color,(1.10,1.10,1.10),1,'MULTIPLY')
    l.new(ambient(n,l,color,.16,.72),emit.inputs['Color'])
    surface=n.new('ShaderNodeBsdfPrincipled'); surface.name='Relief bake surface'
    bump=n.new('ShaderNodeBump'); bump.inputs['Distance'].default_value=.0011
    bump.inputs['Strength'].default_value=.38
    l.new(noise(n,l,coord,65,3).outputs['Fac'],bump.inputs['Height'])
    l.new(bump.outputs['Normal'],surface.inputs['Normal'])
    return mat


def authored_granite(name,center,size,mat,angle=0,variant=0):
    # Explicit independent fracture planes. No ring levels or common bevel
    # belt: each cut terminates at a different height and has a different area.
    profiles={
        0:[(1,.18,.10,.45),(-1,.08,-.04,.42),(.10,1,-.05,.39),(.07,-1,.04,.37),
           (.7,-1,-.10,.665),(-.75,-1,.04,.56),(1,.83,.12,.58),(-1,.83,.07,.55),
           (.03,.07,1,.49),(-.16,-1,.74,.69),(.90,-.60,.70,.72),
           (-1,.12,.55,.60),(.22,1,.75,.58),(-.7,-.5,-.75,.63),(-.462,.507,.573,.50)],
        2:[(1,.2,-.05,.43),(-1,.05,.18,.46),(.05,1,.1,.37),(-.2,-1,.06,.38),
           (.32,.12,1,.46),(-.8,-1,.1,.48),(1,-.5,.4,.49),(.6,1,.4,.55),
           (-1,.3,.8,.58),(.8,-.3,-.7,.55)],
        3:[(1,-.15,.09,.44),(-1,.05,-.1,.39),(.1,1,.07,.40),(.25,-1,-.04,.40),
           (-.2,-.08,1,.43),(.8,-1,.25,.56),(-1,-.5,.44,.54),(.8,1,.1,.51),
           (.8,.1,.7,.54),(-.1,1,-.7,.59)],
        4:[(1,.1,.13,.49),(-1,.08,-.11,.44),(.2,1,.09,.40),(-.18,-1,-.13,.41),
           (.08,.04,1,.48),(-1,-.7,.45,.58),(.8,-1,.63,.60),(1,.7,-.1,.59),
           (.1,1,.6,.52),(-.6,.1,-.8,.54),(-.8,-.4,.8,.59),(.65,-.8,.7,.61),(.05,-1,-.5,.53),(-.7,-.5,-.8,.56)],
        5:[(1,.15,-.08,.45),(-1,.06,.18,.40),(.3,1,-.07,.43),(.05,-1,.1,.41),
           (.30,-.09,1,.44),(.9,-1,.25,.53),(-1,-.9,.5,.56),(.8,1,.5,.64),
           (-.1,.8,.85,.63),(.7,-.2,-.7,.54)],
        6:[(1,.12,.2,.43),(-1,.18,-.1,.39),(.2,1,.1,.35),(-.1,-1,.1,.38),
           (.35,.2,1,.40),(-.8,-1,.3,.48),(1,.8,.4,.52)]}
    cuts=profiles[variant]+[(0,0,-1,.5)]
    normals=np.array([p[:3] for p in cuts]); distances=np.array([p[3] for p in cuts])
    bm=bmesh.new(); ca,sa=math.cos(angle),math.sin(angle)
    bmesh.ops.create_icosphere(bm,subdivisions=5,radius=1)
    for v in bm.verts:
        ray=np.array(v.co)
        dots=normals@ray
        t=min(distances[i]/d for i,d in enumerate(dots) if d>0)
        p=Vector(ray*t)
        # Coherent displacement moves a continuous surface, including corners,
        # without tearing thin bevel strips apart along conflicting normals.
        p+=noise_vector(p*5.4+Vector((variant*2.3,1.4,.7)))*.024
        p+=noise_vector(p*13.7+Vector((variant,0,1)))*.007
        x,y,z=p
        x,y=x*size[0],y*size[1]
        v.co=(center[0]+x*ca-y*sa,center[1]+x*sa+y*ca,center[2]+max(0,z+.5)*size[2])
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
    mesh=bpy.data.meshes.new(name); bm.to_mesh(mesh); bm.free()
    ob=bpy.data.objects.new(name,mesh); bpy.context.collection.objects.link(ob)
    ob.data.materials.append(mat); PARTS.append(ob)
    for face in mesh.polygons: face.use_smooth=True
    bpy.context.view_layer.objects.active=ob; ob.select_set(True)
    mod=ob.modifiers.new('Unequal weathered surface patches','DECIMATE'); mod.ratio=.19
    bpy.ops.object.modifier_apply(modifier=mod.name)
    ob.select_set(False)
    return ob


def granite_fractures(mat,ob):
    """Sparse surface fractures, projected onto this actual sculpture for bake."""
    bpy.context.view_layer.update()
    paths=[]
    for projection,path in [
        ('top',[(-.30,.38),(-.24,.30),(-.23,.23),(-.13,.20),(-.09,.16)]),
        ('front',[(.25,.78),(.19,.73),(.08,.71),(.06,.65),(-.01,.62)])]:
        pts=[]
        dense=[]
        for start,end in zip(path,path[1:]):
            for step in range(8):
                t=step/8
                dense.append((start[0]*(1-t)+end[0]*t,start[1]*(1-t)+end[1]*t))
        dense.append(path[-1])
        for a,b in dense:
            origin=Vector((a,-2,b)) if projection=='front' else Vector((a,b,2))
            direction=Vector((0,1,0)) if projection=='front' else Vector((0,0,-1))
            hit,p,_,_=ob.ray_cast(origin,direction)
            if hit: pts.append(p)
        if len(pts)>1: paths.append(pts)
    n,l=mat.node_tree.nodes,mat.node_tree.links
    coord=n.new('ShaderNodeNewGeometry').outputs['Position']
    # Bake coordinates after the agreed front-facing export turn.
    unturn=n.new('ShaderNodeVectorMath'); unturn.operation='MULTIPLY'
    unturn.inputs[1].default_value=(-1,-1,1); l.new(coord,unturn.inputs[0])
    coord=unturn.outputs[0]
    distances=[]
    for path in paths:
        for j,(a,b) in enumerate(zip(path,path[1:])):
            d=b-a
            sub=n.new('ShaderNodeVectorMath'); sub.operation='SUBTRACT'
            l.new(coord,sub.inputs[0]); sub.inputs[1].default_value=a
            dot=n.new('ShaderNodeVectorMath'); dot.operation='DOT_PRODUCT'
            l.new(sub.outputs[0],dot.inputs[0]); dot.inputs[1].default_value=d
            t=scalar(n,l,'DIVIDE',dot.outputs['Value'],d.length_squared)
            t=scalar(n,l,'MINIMUM',scalar(n,l,'MAXIMUM',t,0),1)
            scale=n.new('ShaderNodeVectorMath'); scale.operation='SCALE'
            scale.inputs[0].default_value=d; l.new(t,scale.inputs[3])
            rem=n.new('ShaderNodeVectorMath'); rem.operation='SUBTRACT'
            l.new(sub.outputs[0],rem.inputs[0]); l.new(scale.outputs[0],rem.inputs[1])
            length=n.new('ShaderNodeVectorMath'); length.operation='LENGTH'; l.new(rem.outputs[0],length.inputs[0])
            u=(j+.5)/(len(path)-1)
            width=.0012+.0055*math.sin(math.pi*u)**.7*(.8+.2*math.cos(u*19))
            distances.append(scalar(n,l,'DIVIDE',length.outputs['Value'],width))
    distance=distances[0]
    for d in distances[1:]: distance=scalar(n,l,'MINIMUM',distance,d)
    mask=ramp(n,l,distance,[(.15,(1,1,1)),(.75,(.8,.8,.8)),(1.35,(0,0,0))])
    emit=next(node for node in n if node.type=='EMISSION')
    base=emit.inputs['Color'].links[0].from_socket
    l.new(mix(n,l,base,(.145,.15,.145),mask),emit.inputs['Color'])
    surface=n['Relief bake surface']
    fine=surface.inputs['Normal'].links[0].from_socket
    relief=n.new('ShaderNodeBump'); relief.invert=True
    relief.inputs['Distance'].default_value=.009; relief.inputs['Strength'].default_value=.8
    l.new(mask,relief.inputs['Height']); l.new(fine,relief.inputs['Normal'])
    l.new(relief.outputs['Normal'],surface.inputs['Normal'])


def granite_plane_tint(ob):
    ob.data.update()
    colors=ob.data.color_attributes.new(name='GranitePlanes',type='FLOAT_COLOR',domain='POINT')
    for vertex in ob.data.vertices:
        up=max(vertex.normal.z,0); right=max(vertex.normal.x,0)
        # Continuous vertex tint must agree with smooth shading. Per-face
        # corner colors were baking a false triangle pattern into the albedo.
        rgb=(.56+.10*up-.09*right,.545+.10*up-.055*right,.49+.115*up+.004*right)
        colors.data[vertex.index].color=(*rgb,1)


def build_stone():
    mat=granite_material()
    main=authored_granite('Granite main fractured mass',(0,.04,0),(1.0,.75,1.0),mat,-.60)
    authored_granite('Low left shoulder',(-.345,.18,0),(.55,.68,.62),mat,-.18,2)
    authored_granite('Low right shoulder',(.345,.16,0),(.55,.66,.55),mat,.17,3)
    front=authored_granite('Large front left broken stone',(-.445,-.25,0),(.53,.59,.43),mat,-.13,4)
    authored_granite('Small front right broken stone',(.43,-.24,0),(.43,.46,.31),mat,.24,5)
    authored_granite('Small left chip',(-.59,.015,0),(.16,.23,.14),mat,.18,6)
    for ob in PARTS:
        for v in ob.data.vertices:
            v.co.x*=1.07
            v.co.z-=.03
        ob.data.update()
    granite_fractures(mat,main)
    for ob in PARTS:
        granite_plane_tint(ob)
        # Unity's FBX + ground pivots show the authored -Y front only after
        # this turn. The two foreground fragments must face the Game camera.
        ob.rotation_euler.z=math.pi


def build_ore():
    from resource_ore_model import build
    PARTS.extend(build())


def build_wood():
    from resource_wood_model import build
    PARTS.extend(build())


def join_and_bake(label):
    bpy.ops.object.select_all(action='DESELECT')
    for ob in PARTS: ob.select_set(True)
    bpy.context.view_layer.objects.active=PARTS[0]; bpy.ops.object.join()
    ob=bpy.context.object; ob.name=label.replace('Resource','')
    bpy.context.scene.cursor.location=(0,0,0); bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.mesh.normals_make_consistent(inside=False)
    bpy.ops.uv.smart_project(angle_limit=math.radians(66),island_margin=.004)
    bpy.ops.object.mode_set(mode='OBJECT')
    scene=bpy.context.scene; scene.render.engine='CYCLES'; scene.cycles.samples=16
    atlas=bpy.data.images.new(label+'Albedo',width=2048,height=2048,alpha=False)
    if 'Wood' in label:
        # Thin UV islands need a foliage-colored background through the mip
        # chain. Black unused texels otherwise show as dotted branch edges.
        atlas.generated_color=(.155,.315,.070,1)
    for mat in ob.data.materials:
        node=mat.node_tree.nodes.new('ShaderNodeTexImage'); node.image=atlas; mat.node_tree.nodes.active=node
    scene.render.bake.margin=16; scene.render.bake.margin_type='EXTEND'
    scene.render.bake.use_clear='Wood' not in label
    bpy.ops.object.bake(type='EMIT')
    atlas.filepath_raw=str(ASSETS/(label+'Albedo.png')); atlas.file_format='PNG'; atlas.save()
    normal=None
    if all(mat.node_tree.nodes.get('Relief bake surface') for mat in ob.data.materials):
        normal=bpy.data.images.new(label+'Normal',width=2048,height=2048,alpha=False)
        normal.colorspace_settings.name='Non-Color'
        for mat in ob.data.materials:
            nodes,links=mat.node_tree.nodes,mat.node_tree.links
            links.new(nodes['Relief bake surface'].outputs[0],nodes.get('Material Output').inputs['Surface'])
            node=nodes.new('ShaderNodeTexImage'); node.image=normal; nodes.active=node
        # Tangent normals are deterministic; more path samples do not improve
        # their precision and make these authored fracture shaders slow to bake.
        scene.cycles.samples=1
        bpy.ops.object.bake(type='NORMAL')
        normal.filepath_raw=str(ASSETS/(label+'Normal.png')); normal.file_format='PNG'; normal.save()
    final=bpy.data.materials.new(label+' baked original albedo'); final.use_nodes=True
    p=final.node_tree.nodes.get('Principled BSDF'); p.inputs['Roughness'].default_value=.88
    node=final.node_tree.nodes.new('ShaderNodeTexImage'); node.image=atlas
    final.node_tree.links.new(node.outputs['Color'],p.inputs['Base Color'])
    if normal:
        tex=final.node_tree.nodes.new('ShaderNodeTexImage'); tex.image=normal
        mapping=final.node_tree.nodes.new('ShaderNodeNormalMap')
        final.node_tree.links.new(tex.outputs['Color'],mapping.inputs['Color'])
        final.node_tree.links.new(mapping.outputs['Normal'],p.inputs['Normal'])
    ob.data.materials.clear(); ob.data.materials.append(final)
    for face in ob.data.polygons: face.material_index=0
    bpy.ops.export_scene.fbx(filepath=str(ASSETS/(label+'.fbx')),use_selection=True,
        object_types={'MESH'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,apply_scale_options='FBX_SCALE_ALL')
    ob.data.calc_loop_triangles()
    return ob,{'asset':label,'meshes':1,'triangles':len(ob.data.loop_triangles),'materials':1,'albedo':[2048,2048],
        'normal':[2048,2048] if normal else None,
        'bounds':[[round(min(v.co[i] for v in ob.data.vertices),3),round(max(v.co[i] for v in ob.data.vertices),3)] for i in range(3)]}


def render_review(label,ob,shape_only=False):
    scene=bpy.context.scene; scene.cycles.samples=32; scene.cycles.use_denoising=True
    scene.world.color=(.21,.24,.29)
    # Revised sources all use the same authored-front export turn. Review
    # them through the actual tactical pitch rather than another studio side.
    lights=[('Key',(3,4,7),750,(1,.92,.81)),('Fill',(-4,-1,5),160,(.65,.77,1))]
    for name,pos,power,color in lights:
        data=bpy.data.lights.new(name,'AREA'); data.energy=power; data.size=4; data.color=color
        light=bpy.data.objects.new(name,data); scene.collection.objects.link(light); light.location=pos
        light.rotation_euler=(Vector((0,0,.8))-light.location).to_track_quat('-Z','Y').to_euler()
    bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.026))
    ground=bpy.data.materials.new('Review ground'); ground.diffuse_color=(.18,.235,.12,1)
    bpy.context.object.data.materials.append(ground)
    data=bpy.data.cameras.new('Review camera'); cam=bpy.data.objects.new('Review camera',data); scene.collection.objects.link(cam)
    target=Vector((0,0,1.02 if 'Wood' in label else .43))
    cam.location=target+Vector((0,4,5.713))
    cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler()
    data.type='ORTHO'; data.ortho_scale=2.85 if 'Wood' in label else 2.15
    scene.camera=cam; scene.render.resolution_x=900; scene.render.resolution_y=900; scene.render.resolution_percentage=100
    scene.view_settings.view_transform='AgX'
    scene.render.filepath=str(REVIEW/(label+('_shape.png' if shape_only else '_blender.png')))
    if not shape_only:
        bpy.context.preferences.filepaths.save_version=0; bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/(label+'.blend')))
    bpy.ops.render.render(write_still=True)


def main():
    selected=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []; stats=[]
    for kind,label,builder in [('stone','StoneResourceSource',build_stone),('wood','WoodResourceSource',build_wood),('metal','IronOreResourceSource',build_ore)]:
        if selected and kind not in selected: continue
        bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False); PARTS.clear()
        builder()
        if '--shape' in selected:
            for mat in {m for ob in PARTS for m in ob.data.materials}:
                nodes,links=mat.node_tree.nodes,mat.node_tree.links
                p=nodes.get('Relief bake surface')
                if p:
                    emit=next(n for n in nodes if n.type=='EMISSION')
                    links.new(emit.inputs['Color'].links[0].from_socket,p.inputs['Base Color'])
                    p.inputs['Roughness'].default_value=.9
                    links.new(p.outputs[0],nodes.get('Material Output').inputs['Surface'])
            render_review(label,PARTS[0],True)
            print('S3_RESOURCE_SHAPE_ONLY '+label,flush=True)
            continue
        ob,stat=join_and_bake(label); render_review(label,ob); stats.append(stat)
        print('S3_RESOURCE_MODEL '+json.dumps(stat),flush=True)
    stats_file=REVIEW/'resource_source_blender_stats.json'
    if '--shape' in selected: sys.exit(0)
    if selected and stats_file.exists():
        updated={s['asset']:s for s in json.loads(stats_file.read_text())}; updated.update({s['asset']:s for s in stats}); stats=list(updated.values())
    stats_file.write_text(json.dumps(stats,indent=2))
    print('S3_RESOURCE_BLENDER_OK '+json.dumps(stats),flush=True)


if __name__ == '__main__':
    main()
