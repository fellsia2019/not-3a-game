"""Original Longshot stone catapult. Blender 5.2: build, bake atlas, export FBX, render.
Target: art/concepts/longshot_stone_catapult_option_a.png. No third-party assets.
Run: blender --background --python art/source/build_longshot_catapult.py
"""
from pathlib import Path
import json, math, random
import bpy
from mathutils import Vector, Matrix

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/Stage3/VisualPilot'
REVIEW=ROOT/'art/review'
SOURCE=ROOT/'art/source/longshot_catapult.blend'
random.seed(31)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
parts=[]

def material(name,dark,light,kind='wood'):
    mat=bpy.data.materials.new(name)
    mat.diffuse_color=(*light,1)
    mat.use_nodes=True
    n,l=mat.node_tree.nodes,mat.node_tree.links
    n.clear()
    out=n.new('ShaderNodeOutputMaterial')
    emit=n.new('ShaderNodeEmission')
    l.new(emit.outputs[0],out.inputs['Surface'])
    coord=n.new('ShaderNodeAttribute')
    coord.attribute_name='grain_coords'
    scale=n.new('ShaderNodeVectorMath')
    scale.operation='MULTIPLY'
    scale.inputs[1].default_value=(3.5,3.5,.20) if kind=='wood' else (2.8,2.8,2.8)
    l.new(coord.outputs['Vector'],scale.inputs[0])
    noise=n.new('ShaderNodeTexNoise')
    noise.inputs['Scale'].default_value=4
    noise.inputs['Detail'].default_value=2.5
    noise.inputs['Roughness'].default_value=.72
    l.new(scale.outputs[0],noise.inputs['Vector'])
    ramp=n.new('ShaderNodeValToRGB')
    ramp.color_ramp.elements[0].position=.27
    ramp.color_ramp.elements[0].color=(*dark,1)
    ramp.color_ramp.elements[1].position=.75
    ramp.color_ramp.elements[1].color=(*light,1)
    l.new(noise.outputs['Fac'],ramp.inputs[0])
    color=ramp.outputs[0]
    if kind=='stone':
        vor=n.new('ShaderNodeTexVoronoi')
        vor.feature='DISTANCE_TO_EDGE'
        vor.inputs['Scale'].default_value=3.2
        l.new(coord.outputs['Vector'],vor.inputs['Vector'])
        cracks=n.new('ShaderNodeValToRGB')
        cracks.color_ramp.elements[0].position=.015
        cracks.color_ramp.elements[0].color=(.26,.24,.22,1)
        cracks.color_ramp.elements[1].position=.038
        cracks.color_ramp.elements[1].color=(1,1,1,1)
        l.new(vor.outputs['Distance'],cracks.inputs[0])
        mix=n.new('ShaderNodeMixRGB')
        mix.blend_type='MULTIPLY'
        mix.inputs[0].default_value=.8
        l.new(color,mix.inputs[1])
        l.new(cracks.outputs[0],mix.inputs[2])
        color=mix.outputs[0]
    ao=n.new('ShaderNodeAmbientOcclusion')
    ao.inputs['Distance'].default_value=.22
    ao.samples=16
    mix=n.new('ShaderNodeMixRGB')
    mix.blend_type='MULTIPLY'
    mix.inputs[0].default_value=.72
    l.new(color,mix.inputs[1])
    l.new(ao.outputs['Color'],mix.inputs[2])
    l.new(mix.outputs[0],emit.inputs['Color'])
    return mat

wood=material('Honey timber grain',(.105,.025,.006),(.48,.185,.046))
cut=material('Golden cut timber grain',(.17,.044,.010),(.62,.27,.074))
reds=[material('Red cedar %d'%i,(.16+.013*i,.007+.002*i,.004),
               (.55+.025*i,.046+.006*i,.019+.003*i)) for i in range(4)]
iron=material('Forged iron',(.065,.070,.080),(.19,.20,.23),'metal')
rivet_mat=material('Iron rivets',(.20,.21,.23),(.43,.45,.47),'metal')
stone=material('Carved footing stone',(.23,.23,.25),(.47,.45,.43),'stone')

def finish(ob,mat,bevel=.015):
    ob.data.materials.append(mat)
    attr=ob.data.attributes.new('grain_coords','FLOAT_VECTOR','POINT')
    dims=ob.dimensions
    for v in ob.data.vertices:
        attr.data[v.index].vector=tuple(v.co[i]/max(dims[i],.001) for i in range(3))
    if bevel:
        mod=ob.modifiers.new('Hand softened edges','BEVEL')
        mod.width=bevel
        mod.segments=2
        ob.modifiers.new('Face weighted normals','WEIGHTED_NORMAL')
    parts.append(ob)
    return ob

def block(name,loc,dims,mat=wood,bevel=.015):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
    ob=bpy.context.object
    ob.name=name
    ob.dimensions=dims
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return finish(ob,mat,bevel)

def beam(name,a,b,width,depth=None,mat=wood,bevel=.015):
    a,b=Vector(a),Vector(b)
    ob=block(name,(a+b)*.5,(width,depth or width,(b-a).length),mat,bevel)
    ob.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler()
    return ob

def rivet(loc,normal,radius=.025):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=8,ring_count=4,radius=1,location=loc)
    ob=bpy.context.object
    ob.name='Hammered iron rivet'
    ob.scale=(radius,radius,radius*.50)
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    ob.rotation_euler=Vector(normal).to_track_quat('Z','Y').to_euler()
    return finish(ob,rivet_mat,0)

def collar(x,y,z,width,height=.11):
    block('Iron timber collar',(x,y,z),(width,width,height),iron,.012)
    for sign in (-1,1):
        rivet((x+sign*(width*.5+.003),y,z),(sign,0,0))
        rivet((x,y+sign*(width*.5+.003),z),(0,sign,0))

def rotated(p,side):
    x,y,z=p
    a=side*math.pi/2
    return (x*math.cos(a)-y*math.sin(a),x*math.sin(a)+y*math.cos(a),z)

# Original geometry for the approved bowl-catapult concept. Coordinates are Z-up.
def cylinder(name,a,b,radius,mat=iron,vertices=16):
    a,b=Vector(a),Vector(b)
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=radius,depth=(b-a).length,location=(a+b)*.5)
    ob=bpy.context.object
    ob.name=name
    ob.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler()
    return finish(ob,mat,.009)

def chiseled(name,loc,dims,mat=stone,bevel=.025):
    ob=block(name,loc,dims,mat,bevel)
    for v in ob.data.vertices:
        v.co += Vector((random.uniform(-.009,.009),random.uniform(-.009,.009),random.uniform(-.006,.006)))
    return ob

def ring(name,center,radius,width,height,mat,segments=16):
    # Hollow horizontal band; the cup stays visibly open, even without its stone.
    coords=[]
    for z in (-height*.5,height*.5):
        for r in (radius-width,radius):
            for i in range(segments):
                angle=2*math.pi*i/segments
                coords.append((center[0]+r*math.cos(angle),center[1]+r*math.sin(angle),center[2]+z))
    faces=[]
    for i in range(segments):
        j=(i+1)%segments
        faces.extend([(i,j,segments+j,segments+i),
                      (2*segments+i,3*segments+i,3*segments+j,2*segments+j),
                      (segments+i,segments+j,3*segments+j,3*segments+i),
                      (i,2*segments+i,2*segments+j,j)])
    mesh=bpy.data.meshes.new(name)
    mesh.from_pydata(coords,[],faces); mesh.update()
    ob=bpy.data.objects.new(name,mesh); bpy.context.collection.objects.link(ob)
    return finish(ob,mat,.006)

# Compact stone emplacement: staggered visible masonry, no large solid cube.
joint_stone=material('Recessed stone mortar',(.12,.12,.13),(.23,.22,.21),'rock')
block('Recessed mortar backing',(0,0,.295),(1.38,1.38,.53),joint_stone,.015)
for row in range(3):
    z=.105+row*.197
    for side in range(4):
        lengths=[.25,.34,.34,.25] if row%2 else [.39,.40,.39]
        cursor=-sum(lengths)*.5
        for j,length in enumerate(lengths):
            p=rotated((cursor+length*.5,-.66,z),side)
            ob=chiseled('Staggered masonry course %d block %d'%(row,j),p,(length-.014,.27,.183),stone,.022)
            ob.rotation_euler[2]=side*math.pi/2
            cursor+=length
for sx in (-1,1):
    for sy in (-1,1):
        x,y=sx*.67,sy*.67
        ob=chiseled('Splayed corner foundation',(x,y,.14),(.43,.43,.28),stone,.045)
        for v in ob.data.vertices:
            if v.co.z>0: v.co.x*=.82; v.co.y*=.82
        chiseled('Upper corner dressed stone',(x,y,.435),(.32,.32,.30),stone,.036)
        beam('Corner timber post',(x,y,.57),(x,y,1.13),.165,.165,cut,.019)
        collar(x,y,.955,.209,.115)
        collar(x,y,.63,.203,.08)
for x in range(5):
    for y in range(5):
        chiseled('Paved deck tile',((x-2)*.247,(y-2)*.247,.605),(.237,.237,.095),stone,.012)
for side in range(4):
    beam('Lower parapet stretcher',rotated((-.67,-.67,.69),side),rotated((.67,-.67,.69),side),.11,.10,wood,.010)
    beam('Golden handrail',rotated((-.67,-.67,.96),side),rotated((.67,-.67,.96),side),.10,.12,cut,.012)
    for t in (-.40,-.14,.14,.40):
        beam('Short parapet baluster',rotated((t,-.67,.70),side),rotated((t,-.67,.94),side),.075,.07,wood,.008)

# Twin braced A-frames and the iron axle. The raised arm is a single clear silhouette.
machine_start=len(parts)
for x in (-.34,.34):
    for y in (-.46,.43):
        beam('Trestle splayed leg',(x,y,.65),(x,0,1.40),.20,.21,cut,.021)
        block('Iron anchored shoe',(x,y,.71),(.255,.235,.10),iron,.01)
        rivet((x,y,.769),(0,0,1),.027)
    beam('Trestle bearer',(x,-.50,.68),(x,.48,.68),.23,.11,wood,.015)
    # Metal straps across front legs, with exposed heads.
    beam('Front leg iron strap',(x,-.32,.96),(x,-.27,1.045),.242,.242,iron,.009)
    rivet((x,-.384,.99),(0,-1,.5),.028)
cylinder('Long forged pivot axle',(-.51,0,1.37),(.51,0,1.37),.105,iron,20)
for x in (-.41,.41):
    cylinder('Axle bearing', (x-.045,0,1.37),(x+.045,0,1.37),.147,iron,20)
for sign in (-1,1):
    cylinder('Axle end pin',(sign*.51,0,1.37),(sign*.56,0,1.37),.072,rivet_mat,12)
arm_a=Vector((0,-.16,1.05)); arm_b=Vector((0,.57,2.65))
arm_start=len(parts)
beam('Long throwing arm',arm_a,arm_b,.205,.185,cut,.025)
direction=(arm_b-arm_a).normalized()
for t in (.19,.60,.92):
    p=arm_a.lerp(arm_b,t)
    beam('Arm forged binding',p-direction*.043,p+direction*.043,.246,.22,iron,.011)
    rivet(tuple(p+Vector((.132,0,0))),(1,0,0),.028)
    rivet(tuple(p-Vector((.132,0,0))),(-1,0,0),.028)
arm_parts=list(parts[arm_start:])
# Torsion rope, modeled as a continuous thick coil, rather than a painted stripe.
curve=bpy.data.curves.new('Twisted torsion cord','CURVE'); curve.dimensions='3D'
curve.resolution_u=1; curve.bevel_depth=.024; curve.bevel_resolution=1
spline=curve.splines.new('POLY'); spline.points.add(255)
for i,p in enumerate(spline.points):
    t=i/255; angle=t*math.pi*2*13
    p.co=(-.27+.54*t,.115*math.cos(angle),1.075+.115*math.sin(angle),1)
cord=bpy.data.objects.new('Torsion rope bundle',curve); bpy.context.collection.objects.link(cord)
bpy.context.view_layer.objects.active=cord; cord.select_set(True)
bpy.ops.object.convert(target='MESH'); cord=bpy.context.object
rope=material('Coarse tan cord',(.18,.11,.050),(.49,.34,.17),'stone')
finish(cord,rope,0); cord.select_set(False)

# Staved throwing bowl, thick orange rim, metal hoop and a faceted round stone.
cup=(0,.60,2.64)
cup_start=len(parts)
for i in range(16):
    a0=2*math.pi*(i+.035)/16; a1=2*math.pi*(i+.965)/16
    coords=[]
    for z,r in ((-.10,.155),(.11,.255)):
        for rr in (r-.045,r):
            for a in (a0,a1): coords.append((cup[0]+rr*math.cos(a),cup[1]+rr*math.sin(a),cup[2]+z))
    mesh=bpy.data.meshes.new('Bowl stave')
    mesh.from_pydata(coords,[],[(0,1,3,2),(4,6,7,5),(0,4,5,1),(2,3,7,6),(0,2,6,4),(1,5,7,3)]); mesh.update()
    ob=bpy.data.objects.new('Individual bowl stave',mesh); bpy.context.collection.objects.link(ob)
    finish(ob,cut,.006)
ring('Heavy bowl iron hoop',(0,.60,2.65),.243,.030,.075,iron)
ring('Bowl golden rim',(0,.60,2.755),.263,.051,.045,cut)
for i in range(8):
    a=i*2*math.pi/8
    rivet((.247*math.cos(a),.60+.247*math.sin(a),2.65),(math.cos(a),math.sin(a),0),.021)
arm_parts.extend(parts[cup_start:])
boulder_mat=material('Faceted granite boulder',(.26,.255,.25),(.56,.54,.50),'rock')
bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=3,radius=.228,location=(0,.60,2.806))
rock=bpy.context.object; rock.name='Loaded stone projectile'
for v in rock.data.vertices: v.co*=random.uniform(.94,1.045)
finish(rock,boulder_mat,0)
# Keep the foundation aligned to its cell; turn the machine enough to expose
# its axle and bowl from the fixed front-facing tactical camera.
machine_rotation=Matrix.Rotation(math.radians(24),4,'Z')
for ob in parts[machine_start:]: ob.matrix_world=machine_rotation @ ob.matrix_world

# Build an atlas once, retain editable parts, and export one runtime mesh.
OUT.mkdir(parents=True,exist_ok=True); REVIEW.mkdir(parents=True,exist_ok=True)
scene=bpy.context.scene; scene.render.engine='CYCLES'; scene.cycles.samples=16
source_collection=bpy.data.collections.new('Editable construction (hidden)')
scene.collection.children.link(source_collection)
for ob in parts:
    duplicate=ob.copy(); duplicate.data=ob.data.copy(); source_collection.objects.link(duplicate)
    duplicate.select_set(False); duplicate.hide_set(True)
source_collection.hide_render=True; source_collection.hide_viewport=True
bpy.ops.object.select_all(action='DESELECT')
for ob in parts:
    bpy.context.view_layer.objects.active=ob; ob.select_set(True)
    for mod in list(ob.modifiers): bpy.ops.object.modifier_apply(modifier=mod.name)
    if ob in parts[machine_start:]:
        group=ob.vertex_groups.new(name='LoadedStone' if ob == rock else 'CatapultArm' if ob in arm_parts else 'CatapultFrame')
        group.add(list(range(len(ob.data.vertices))),1.0,'REPLACE')
    ob.select_set(False)
for ob in parts: ob.select_set(True)
bpy.context.view_layer.objects.active=parts[0]; bpy.ops.object.join()
model=bpy.context.object; model.name='Longshot Stone Bowl Catapult'
bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.uv.smart_project(angle_limit=math.radians(66),island_margin=.004)
bpy.ops.object.mode_set(mode='OBJECT')
atlas=bpy.data.images.new('LongshotCatapultAlbedo',width=2048,height=2048,alpha=False)
for mat in model.data.materials:
    tex=mat.node_tree.nodes.new('ShaderNodeTexImage'); tex.image=atlas; mat.node_tree.nodes.active=tex
scene.render.bake.margin=8; bpy.ops.object.bake(type='EMIT')
atlas.filepath_raw=str(OUT/'LongshotCatapultAlbedo.png'); atlas.file_format='PNG'; atlas.save()
final=bpy.data.materials.new('LongshotCatapultAtlas'); final.use_nodes=True
bsdf=final.node_tree.nodes.get('Principled BSDF'); bsdf.inputs['Roughness'].default_value=.79
tex=final.node_tree.nodes.new('ShaderNodeTexImage'); tex.image=atlas
final.node_tree.links.new(tex.outputs['Color'],bsdf.inputs['Base Color'])
model.data.materials.clear(); model.data.materials.append(final)
for polygon in model.data.polygons: polygon.material_index=0
model.scale=(.52,)*3; bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
# Preserve baked UVs and shared atlas while giving the arm and loaded stone real pivots.
runtime_parts=[model]
for group_name in ('LoadedStone','CatapultArm','CatapultFrame'):
    bpy.ops.object.select_all(action='DESELECT'); model.select_set(True)
    bpy.context.view_layer.objects.active=model
    model.vertex_groups.active_index=model.vertex_groups[group_name].index
    bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='DESELECT')
    bpy.ops.object.vertex_group_select(); bpy.ops.mesh.separate(type='SELECTED')
    bpy.ops.object.mode_set(mode='OBJECT')
    part=next(o for o in bpy.context.selected_objects if o != model)
    part.name=group_name; runtime_parts.append(part)
model.name='CatapultBase'
stone_part=runtime_parts[1]; arm_part=runtime_parts[2]
bpy.ops.object.select_all(action='DESELECT'); arm_part.select_set(True)
bpy.context.view_layer.objects.active=arm_part
scene.cursor.location=machine_rotation @ (Vector((0,0,1.37))*.52)
bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
stone_world=stone_part.matrix_world.copy(); stone_part.parent=arm_part; stone_part.matrix_world=stone_world
aim=bpy.data.objects.new('CatapultAim',None); scene.collection.objects.link(aim)
aim.location=(0,0,0)
for part in (arm_part,runtime_parts[3]):
    part_world=part.matrix_world.copy(); part.parent=aim; part.matrix_world=part_world
bpy.ops.object.select_all(action='DESELECT')
for part in runtime_parts: part.select_set(True)
aim.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(OUT/'LongshotCatapult.fbx'),use_selection=True,
    object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,
    apply_scale_options='FBX_SCALE_ALL',use_mesh_modifiers=True)

def light(name,location,energy,size,color):
    data=bpy.data.lights.new(name,'AREA'); data.energy=energy; data.shape='DISK'; data.size=size; data.color=color
    ob=bpy.data.objects.new(name,data); scene.collection.objects.link(ob); ob.location=location
    ob.rotation_euler=(Vector((0,0,.8))-ob.location).to_track_quat('-Z','Y').to_euler()
light('Warm softbox',(-3,-4,6),350,4,(1,.88,.75))
light('Cool fill',(4,-1,3),100,4,(.77,.85,1))
light('Rim',(1,4,5),350,3,(1,.87,.73))
scene.world.color=(.08,.08,.08)
camera_data=bpy.data.cameras.new('Concept orthographic angle')
camera=bpy.data.objects.new('Concept orthographic angle',camera_data); scene.collection.objects.link(camera)
camera.location=(3.5,-5.0,3.6)
camera.rotation_euler=(Vector((0,0,.77))-camera.location).to_track_quat('-Z','Y').to_euler()
camera_data.type='ORTHO'; camera_data.ortho_scale=1.95; scene.camera=camera
scene.cycles.samples=32; scene.cycles.use_denoising=True
scene.render.resolution_x=1024; scene.render.resolution_y=1024; scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'; scene.render.image_settings.color_mode='RGBA'
scene.view_settings.view_transform='AgX'; scene.render.film_transparent=False
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.008)); ground=bpy.context.object
ground.name='Review backdrop only'; mat=bpy.data.materials.new('Neutral warm grey'); mat.use_nodes=True
mat.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.075,.082,.092,1)
ground.data.materials.append(mat)
scene.render.filepath=str(REVIEW/'longshot_catapult_blender.png')
SOURCE.parent.mkdir(parents=True,exist_ok=True); bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE))
bpy.ops.render.render(write_still=True)
for part in runtime_parts: part.data.calc_loop_triangles()
stats={'revision':2,'vertices':sum(len(o.data.vertices) for o in runtime_parts),'triangles':sum(len(o.data.loop_triangles) for o in runtime_parts),
       'renderers':4,'materials':1,'atlas':[2048,2048],'construction_parts':len(parts),'animated_parts':['CatapultAim','CatapultArm','LoadedStone']}
(REVIEW/'longshot_catapult_stats.json').write_text(json.dumps(stats,indent=2))
print('S3_LONGSHOT_BLENDER_OK '+json.dumps(stats))
