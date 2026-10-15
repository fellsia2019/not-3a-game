"""Original Stage 3 soul sanctuary, revision 3. Approved ice-dome concept.
Blender 5.2: blender --background --python art/source/build_soul_sphere.py
Original geometry and baked procedural stone; no external runtime assets.
"""
from pathlib import Path
import bpy, math, json, random
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/Stage3/VisualPilot'
REVIEW=ROOT/'art/review'
random.seed(41)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
parts=[]

def mat(name,color,alpha=1,emission=0):
    m=bpy.data.materials.new(name); m.use_nodes=True
    m.diffuse_color=(*color,alpha)
    p=m.node_tree.nodes.get('Principled BSDF')
    p.inputs['Base Color'].default_value=(*color,1)
    p.inputs['Roughness'].default_value=.24
    p.inputs['Alpha'].default_value=alpha
    p.inputs['Emission Color'].default_value=(*color,1)
    p.inputs['Emission Strength'].default_value=emission
    return m

def stone_mat(name,light):
    m=mat(name,light)
    n,l=m.node_tree.nodes,m.node_tree.links; n.clear()
    output=n.new('ShaderNodeOutputMaterial'); emit=n.new('ShaderNodeEmission')
    l.new(emit.outputs[0],output.inputs['Surface'])
    geom=n.new('ShaderNodeNewGeometry')
    noise=n.new('ShaderNodeTexNoise'); noise.inputs['Scale'].default_value=15
    noise.inputs['Detail'].default_value=3; noise.inputs['Roughness'].default_value=.7
    l.new(geom.outputs['Position'],noise.inputs['Vector'])
    ramp=n.new('ShaderNodeValToRGB')
    ramp.color_ramp.elements[0].position=.15
    ramp.color_ramp.elements[0].color=(*(v*.65 for v in light),1)
    ramp.color_ramp.elements[1].position=.82
    ramp.color_ramp.elements[1].color=(*(v*1.16 for v in light),1)
    l.new(noise.outputs['Fac'],ramp.inputs[0])
    ao=n.new('ShaderNodeAmbientOcclusion'); ao.inputs['Distance'].default_value=.26; ao.samples=16
    mix=n.new('ShaderNodeMixRGB'); mix.blend_type='MULTIPLY'; mix.inputs[0].default_value=.75
    l.new(ramp.outputs[0],mix.inputs[1]); l.new(ao.outputs['Color'],mix.inputs[2])
    # Carved slate has restrained surface fissures and light bevels, not blue emission.
    vor=n.new('ShaderNodeTexVoronoi'); vor.feature='DISTANCE_TO_EDGE'; vor.inputs['Scale'].default_value=9
    l.new(geom.outputs['Position'],vor.inputs['Vector'])
    seam=n.new('ShaderNodeValToRGB')
    seam.color_ramp.elements[0].position=.004; seam.color_ramp.elements[0].color=(.44,.48,.53,1)
    seam.color_ramp.elements[1].position=.026; seam.color_ramp.elements[1].color=(1,1,1,1)
    l.new(vor.outputs['Distance'],seam.inputs[0])
    fissure=n.new('ShaderNodeMixRGB'); fissure.blend_type='MULTIPLY'; fissure.inputs[0].default_value=.6
    l.new(mix.outputs[0],fissure.inputs[1]); l.new(seam.outputs[0],fissure.inputs[2])
    edge=n.new('ShaderNodeValToRGB')
    edge.color_ramp.elements[0].position=.45; edge.color_ramp.elements[0].color=(.53,.58,.65,1)
    edge.color_ramp.elements[1].position=.54; edge.color_ramp.elements[1].color=(1,1,1,1)
    l.new(geom.outputs['Pointiness'],edge.inputs[0])
    carved=n.new('ShaderNodeMixRGB'); carved.blend_type='MULTIPLY'; carved.inputs[0].default_value=.5
    l.new(fissure.outputs[0],carved.inputs[1]); l.new(edge.outputs[0],carved.inputs[2])
    l.new(carved.outputs[0],emit.inputs['Color'])
    return m
stones=[stone_mat('Slate %d'%i,(.255+i*.016,.285+i*.016,.33+i*.017)) for i in range(5)]
# Fresnel for Blender review; Unity uses an equivalent dedicated smooth-ice shader.
def ice_mat(name,color,edge):
    m=mat(name,color); n,l=m.node_tree.nodes,m.node_tree.links
    output=n.get('Material Output'); p=n.get('Principled BSDF')
    p.inputs['Roughness'].default_value=.12
    p.inputs['Emission Color'].default_value=(*color,1)
    p.inputs['Emission Strength'].default_value=.65
    transparent=n.new('ShaderNodeBsdfTransparent')
    fresnel=n.new('ShaderNodeLayerWeight'); fresnel.inputs['Blend'].default_value=.35
    ramp=n.new('ShaderNodeValToRGB')
    ramp.color_ramp.elements[0].color=(.012,.012,.012,1)
    ramp.color_ramp.elements[1].position=.85
    ramp.color_ramp.elements[1].color=(edge,edge,edge,1)
    l.new(fresnel.outputs['Fresnel'],ramp.inputs[0])
    mix=n.new('ShaderNodeMixShader'); l.new(ramp.outputs[0],mix.inputs[0])
    l.new(transparent.outputs[0],mix.inputs[1]); l.new(p.outputs[0],mix.inputs[2])
    l.new(mix.outputs[0],output.inputs['Surface'])
    return m
ice=ice_mat('Unbroken pale cyan ice',(.045,.36,.8),.7)
glass=ice_mat('Soul sphere blue glass',(.025,.28,.9),.85)
energy=mat('Cyan flowing energy',(.035,.62,1),1,2)
soul=mat('White blue soul',(.54,.95,1),1,3)

def finish(ob,name,material,group,bevel=0,smooth=False):
    ob.name=name; ob.data.materials.append(material); ob['runtime_group']=group
    if bevel:
        mod=ob.modifiers.new('Carved soft edge','BEVEL'); mod.width=bevel; mod.segments=2
        ob.modifiers.new('Weighted stone normals','WEIGHTED_NORMAL')
    if smooth:
        for face in ob.data.polygons: face.use_smooth=True
    parts.append(ob); return ob

def cyl(name,r,depth,z,material,group='StoneBase',bevel=.012):
    bpy.ops.mesh.primitive_cylinder_add(vertices=64,radius=r,depth=depth,location=(0,0,z))
    return finish(bpy.context.object,name,material,group,bevel)

def wedge(name,r0,r1,a0,a1,z0,z1,material):
    seg=4; vertices=[]
    for z in (z0,z1):
        for r in (r0,r1):
            for j in range(seg+1):
                a=a0+(a1-a0)*j/seg
                vertices.append((r*math.cos(a),r*math.sin(a),z))
    row=seg+1; faces=[]
    for j in range(seg):
        faces.extend([(j,row+j,row+j+1,j+1),
                      (2*row+j,2*row+j+1,3*row+j+1,3*row+j),
                      (j,j+1,2*row+j+1,2*row+j),
                      (row+j,3*row+j,3*row+j+1,row+j+1)])
    faces.extend([(0,2*row,3*row,row),(seg,row+seg,3*row+seg,2*row+seg)])
    # These radial loops were authored inside-out: top must face +Z in Blender.
    faces=[tuple(reversed(face)) for face in faces]
    mesh=bpy.data.meshes.new(name); mesh.from_pydata(vertices,[],faces); mesh.update()
    ob=bpy.data.objects.new(name,mesh); bpy.context.collection.objects.link(ob)
    return finish(ob,name,material,'StoneBase',.013)

# Diameter is three cells. Individual radial blocks replace the original cube rim.
cyl('Dark recessed foundation',1.47,.085,.047,stones[0])
for count,r0,r1,z0,z1 in ((24,1.21,1.48,.045,.17),(20,.72,1.20,.085,.195),(14,.46,.70,.17,.36)):
    phase=.11 if count==20 else 0
    for i in range(count):
        a=phase+math.tau*i/count
        wedge('Carved radial masonry',r0,r1,a+.007,a+math.tau/count-.007,z0,z1+random.uniform(-.008,.008),random.choice(stones))
cyl('Soul socket well',.474,.09,.224,stones[0])
cyl('Recessed bowl interior',.435,.025,.265,stones[0])

# Four tapered stone anchors, inset diamonds on their outward faces.
for i in range(4):
    a=math.pi/4+i*math.pi/2; radius=1.25
    center=Vector((radius*math.cos(a),radius*math.sin(a),0))
    bpy.ops.mesh.primitive_cube_add(size=1,location=center+Vector((0,0,.20)))
    plinth=bpy.context.object; plinth.dimensions=(.40,.39,.11); plinth.rotation_euler.z=a
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    finish(plinth,'Anchor stepped foot',stones[2],'StoneBase',.018)
    vertices=[]
    for z,width in ((.245,.30),(.63,.23)):
        for x,y in ((-1,-1),(1,-1),(1,1),(-1,1)):
            px=x*width*.5; py=y*width*.5
            vertices.append((center.x+px*math.cos(a)-py*math.sin(a),center.y+px*math.sin(a)+py*math.cos(a),z))
    mesh=bpy.data.meshes.new('Tapered anchor'); mesh.from_pydata(vertices,[],[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)]); mesh.update()
    ob=bpy.data.objects.new('Tapered runestone',mesh); bpy.context.collection.objects.link(ob)
    finish(ob,ob.name,stones[0],'StoneBase',.025)
    # Rhombus on outward side, embedded just above the stone face.
    normal=Vector((math.cos(a),math.sin(a),0)); tangent=Vector((-math.sin(a),math.cos(a),0))
    c=center+normal*.145+Vector((0,0,.424))
    pts=[c+Vector((0,0,.092)),c+tangent*.046,c-Vector((0,0,.092)),c-tangent*.046]
    mesh=bpy.data.meshes.new('Cyan seal inset'); mesh.from_pydata(pts,[],[(0,1,2,3),(3,2,1,0)]); mesh.update()
    ob=bpy.data.objects.new('Cyan inset seal',mesh); bpy.context.collection.objects.link(ob)
    finish(ob,ob.name,energy,'Energy')

def shell(name,r,center,hemisphere,material,group):
    seg=64; rows=24 if hemisphere else 40
    vertices=[]; faces=[]
    end=math.pi/2 if hemisphere else math.pi
    for row in range(rows+1):
        polar=end*row/rows
        for col in range(seg):
            a=math.tau*col/seg
            vertices.append((center[0]+r*math.sin(polar)*math.cos(a),center[1]+r*math.sin(polar)*math.sin(a),center[2]+r*math.cos(polar)))
    for row in range(rows):
        for col in range(seg):
            j=row*seg+col; k=row*seg+(col+1)%seg
            faces.append((j,j+seg,k+seg,k))
    mesh=bpy.data.meshes.new(name); mesh.from_pydata(vertices,[],faces); mesh.update()
    ob=bpy.data.objects.new(name,mesh); bpy.context.collection.objects.link(ob)
    return finish(ob,name,material,group,smooth=True)

dome=shell('Continuous smooth hemisphere',1.24,(0,0,.18),True,ice,'Dome')
# More clearance above the soul under the accepted steep tactical camera.
for vertex in dome.data.vertices: vertex.co.z=.18+(vertex.co.z-.18)*1.32
shell('Clear blue soul sphere',.49,(0,0,.84),False,glass,'Sphere')

# Organic, tapered soul tongues; the central silhouette is visible through both shells.
def tongue(name,points,material):
    vertices=[]; faces=[]; seg=16
    # Catmull-Rom interpolation gives rounded flame curls rather than angular wedges.
    controls=[Vector(p) for p in points]; curved=[]
    for j in range(len(controls)-1):
        p0=controls[max(0,j-1)]; p1=controls[j]; p2=controls[j+1]; p3=controls[min(len(controls)-1,j+2)]
        for k in range(8):
            t=k/8
            p=.5*((2*p1)+(-p0+p2)*t+(2*p0-5*p1+4*p2-p3)*t*t+(-p0+3*p1-3*p2+p3)*t*t*t)
            p.w=max(.001,p.w); curved.append(p)
    curved.append(controls[-1]); points=curved
    for x,y,z,r in points:
        for i in range(seg):
            a=math.tau*i/seg; vertices.append((x+r*math.cos(a),y+r*.62*math.sin(a),z))
    for row in range(len(points)-1):
        for i in range(seg):
            j=row*seg+i; k=row*seg+(i+1)%seg
            faces.append((j,k,k+seg,j+seg))
    mesh=bpy.data.meshes.new(name); mesh.from_pydata(vertices,[],faces); mesh.update()
    ob=bpy.data.objects.new(name,mesh); bpy.context.collection.objects.link(ob)
    return finish(ob,name,material,'Soul',smooth=True)
tongue('Imprisoned soul flame',[(0,0,.55,.015),(-.035,0,.61,.135),(.005,0,.72,.12),(.06,0,.82,.078),(-.015,0,.92,.051),(.025,0,1.045,.022),(-.005,0,1.13,.001)],energy)
tongue('White heart flame',[(-.014,-.04,.555,.009),(-.025,-.05,.61,.065),(.01,-.05,.69,.052),(-.012,-.05,.76,.019),(.04,-.05,.825,.001)],soul)
tongue('Curl of soul light',[(.06,.02,.59,.025),(.15,.02,.69,.045),(.145,.02,.79,.028),(.10,.02,.89,.015),(.145,.02,.98,.001)],energy)

# Soft tapered ribbons with UVs: no square leading blocks, no middle latitude ring.
def ribbon(name,radius,start,span,width,z,group,material,full=False):
    steps=96 if full else 64; vertices=[]; faces=[]
    for i in range(steps+1):
        t=i/steps; a=start+span*t
        taper=1 if full else max(.025,math.sin(math.pi*t)**.65)
        for across in (0,1):
            r=radius+(across-.5)*width*taper
            vertices.append((r*math.cos(a),r*math.sin(a),z))
    for i in range(steps): faces.append((2*i,2*i+1,2*i+3,2*i+2))
    mesh=bpy.data.meshes.new(name); mesh.from_pydata(vertices,[],faces); mesh.update()
    uv=mesh.uv_layers.new(name='Ribbon coordinates')
    for poly in mesh.polygons:
        for loop_index in poly.loop_indices:
            vi=mesh.loops[loop_index].vertex_index
            uv.data[loop_index].uv=(vi//2/steps,vi%2)
    ob=bpy.data.objects.new(name,mesh); bpy.context.collection.objects.link(ob)
    return finish(ob,name,material,group)
ribbon('Fine dome contact rim',1.24,0,math.tau,.018,.185,'Energy',energy,True)
ribbon('Socket blue seam',.715,0,math.tau,.014,.198,'Energy',energy,True)
for radius,phase,width,span in ((1.62,0,.16,1.52),(1.73,.32,.075,1.48),(1.58,.65,.025,1.15)):
    for i in range(3):
        ribbon('Horizontal flowing wave',radius,phase+i*math.tau/3,span,width,.065+(radius-1.58)*.04,'OrbitingWave',energy)

# Apply bevels, merge by draw role. Preserve actual Z-up mesh axes under FBX conversion.
for ob in parts:
    bpy.context.view_layer.objects.active=ob
    for mod in list(ob.modifiers): bpy.ops.object.modifier_apply(modifier=mod.name)
runtime=[]
groups={group:[ob for ob in parts if ob.get('runtime_group')==group]
        for group in ('StoneBase','Dome','Sphere','Soul','Energy','OrbitingWave')}
for group,members in groups.items():
    bpy.ops.object.select_all(action='DESELECT')
    for ob in members: ob.select_set(True)
    bpy.context.view_layer.objects.active=members[0]
    if len(members)>1: bpy.ops.object.join()
    ob=members[0]; ob.name=group; runtime.append(ob)
    bpy.context.scene.cursor.location=(0,0,0)
    bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)

scene=bpy.context.scene; scene.render.engine='CYCLES'; scene.cycles.samples=16
stone=runtime[0]
bpy.ops.object.select_all(action='DESELECT'); stone.select_set(True); bpy.context.view_layer.objects.active=stone
bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.uv.smart_project(angle_limit=math.radians(65),island_margin=.004)
bpy.ops.object.mode_set(mode='OBJECT')
atlas=bpy.data.images.new('SoulSanctuaryStone',width=1024,height=1024,alpha=False)
for m in stone.data.materials:
    tex=m.node_tree.nodes.new('ShaderNodeTexImage'); tex.image=atlas; m.node_tree.nodes.active=tex
scene.render.bake.margin=6; bpy.ops.object.bake(type='EMIT')
atlas.filepath_raw=str(OUT/'SoulSanctuaryStone.png'); atlas.file_format='PNG'; atlas.save()
final=mat('Baked original cut slate',(.4,.4,.4)); p=final.node_tree.nodes.get('Principled BSDF'); p.inputs['Roughness'].default_value=.8
tex=final.node_tree.nodes.new('ShaderNodeTexImage'); tex.image=atlas
final.node_tree.links.new(tex.outputs['Color'],p.inputs['Base Color'])
stone.data.materials.clear(); stone.data.materials.append(final)
for face in stone.data.polygons: face.material_index=0
bpy.ops.object.select_all(action='DESELECT')
for ob in runtime: ob.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(OUT/'SoulSphereIceDome.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,apply_scale_options='FBX_SCALE_ALL')

# Review under a tactical orthographic camera, with actual model geometry.
scene.cycles.samples=40; scene.cycles.use_denoising=True
scene.world.color=(.12,.15,.19)
for name,pos,power,color in [('Key',(-3,-4,6),450,(1,.89,.76)),('Blue fill',(4,2,5),250,(.50,.73,1))]:
    data=bpy.data.lights.new(name,'AREA'); data.energy=power; data.size=4; data.color=color
    ob=bpy.data.objects.new(name,data); scene.collection.objects.link(ob); ob.location=pos
    ob.rotation_euler=(Vector((0,0,.6))-ob.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.01)); ground=bpy.context.object
m=mat('Review ground',(.15,.16,.17)); ground.data.materials.append(m)
cam_data=bpy.data.cameras.new('Tactical camera'); cam=bpy.data.objects.new('Tactical camera',cam_data); scene.collection.objects.link(cam)
cam.location=(3,-5,5.4); cam.rotation_euler=(Vector((0,0,.5))-cam.location).to_track_quat('-Z','Y').to_euler()
cam_data.type='ORTHO'; cam_data.ortho_scale=4.4; scene.camera=cam
scene.render.resolution_x=1024; scene.render.resolution_y=1024; scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX'
scene.render.filepath=str(REVIEW/'soul_sphere_ice_dome_blender.png')
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'art/source/soul_sphere_ice_dome.blend'))
bpy.ops.render.render(write_still=True)
for ob in runtime: ob.data.calc_loop_triangles()
stats={'revision':3,'vertices':sum(len(o.data.vertices) for o in runtime),'triangles':sum(len(o.data.loop_triangles) for o in runtime),'renderers':len(runtime),'base_diameter_cells':2.96,'wave_diameter_cells':3.52,'height_cells':1.817,'stone_atlas':[1024,1024],'source':'Original project Blender geometry and baked procedural stone, no external assets'}
(REVIEW/'soul_sphere_ice_dome_stats.json').write_text(json.dumps(stats,indent=2))
print('S3_SOUL_SPHERE_BLENDER_OK '+json.dumps(stats))
