"""Original Rapid tower R2. Blender 5.2: build, bake atlas, export FBX, render.
Target: art/concepts/rapid_watchtower_approved.png. No third-party assets.
Run: blender --background --python art/source/build_rapid_watchtower.py
"""
from pathlib import Path
import json, math, random
import bpy
from mathutils import Vector, Matrix

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/Stage3/VisualPilot'
REVIEW=ROOT/'art/review'
SOURCE=ROOT/'art/source/rapid_watchtower.blend'
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

# Heavy splayed legs and masonry foundations.
for sx in (-1,1):
    for sy in (-1,1):
        x,y=sx*.53,sy*.53
        foot=block('Chamfered stone plinth',(x,y,.12),(.36,.36,.24),stone,.045)
        for v in foot.data.vertices:
            if v.co.z>0:
                v.co.x*=.87
                v.co.y*=.87
        leg=beam('Main splayed timber',(x,y,.23),(sx*.43,sy*.43,1.65),.19,.20,bevel=.022)
        leg.rotation_euler=Vector((0,0,1)).rotation_difference(Vector((-sx*.10,-sy*.10,1.42)).normalized()).to_euler()
        collar(x*.987,y*.987,.32,.226,.13)
        collar(sx*.449,sy*.449,1.38,.223,.105)
for side in range(4):
    def turn(p): return rotated(p,side)
    beam('Lower stretcher',turn((-.49,-.495,.50)),turn((.49,-.495,.50)),.105,.105)
    beam('Cross brace rising',turn((-.49,-.485,.57)),turn((.445,-.44,1.35)),.13,.10)
    beam('Cross brace falling',turn((.49,-.53,.57)),turn((-.445,-.49,1.35)),.13,.095)
    plate=block('Brace iron joint',turn((0,-.535,.98)),(.175,.04,.17),iron,.012)
    plate.rotation_euler[2]=side*math.pi/2
    rivet(turn((0,-.563,.98)),(math.sin(side*math.pi/2),-math.cos(side*math.pi/2),0),.029)
    beam('Lower deck bearer',turn((-.64,-.44,1.52)),turn((.64,-.44,1.52)),.16,.17,cut)

# Layered deck and open firing pavilion.
for n in range(9):
    beam('Deck board',(-.58+n*.145,-.635,1.67),(-.58+n*.145,.635,1.67),.135,.07,cut,.009)
for side in range(4):
    beam('Deck fascia',rotated((-.67,-.63,1.60),side),rotated((.67,-.63,1.60),side),.115,.15,cut)
for x in (-.50,.50):
    for y in (-.50,.50):
        beam('Pavilion upright',(x,y,1.65),(x,y,2.52),.145,.145,wood,.018)
        collar(x,y,2.00,.184,.10)
        block('Post deck tenon',(x,y,1.70),(.19,.19,.105),cut,.012)
for side in range(4):
    def turn(p): return rotated(p,side)
    for z in (1.78,2.025):
        beam('Balustrade rail',turn((-.49,-.50,z)),turn((.49,-.50,z)),.085,.085,cut,.010)
    for x in (-.32,-.16,0,.16,.32):
        beam('Balustrade slat',turn((x,-.50,1.80)),turn((x,-.50,2.005)),.066,.058,wood,.008)
    beam('Roof support lintel',turn((-.65,-.50,2.43)),turn((.65,-.50,2.43)),.14,.15,cut)

# Four overlapping courses of individual curved red cedar shingles.
def roof_z(t): return 2.43+.72*(1-t)**1.65+.035*t**8
def roof_point(t,u,side,zoffset=0):
    return rotated((u*.83*t,-.83*t,roof_z(t)+zoffset),side)
for row,(lo,hi) in enumerate(((.12,.38),(.32,.60),(.54,.81),(.75,1.0))):
    count=(4,6,9,12)[row]
    for side in range(4):
        for i in range(count):
            u0,u1=-1+2*i/count+.010,-1+2*(i+1)/count-.010
            end=hi+random.uniform(-.015,.012)
            coords=[]
            for t in (lo,(lo+end)*.5,end):
                for u in (u0,u1):
                    p=list(roof_point(min(t,1),u,side,.032*(3-row)))
                    if t>1:
                        p[0]*=t
                        p[1]*=t
                    coords.append(p)
            mesh=bpy.data.meshes.new('Curved shingle')
            mesh.from_pydata(coords,[],[(0,1,3,2),(2,3,5,4)])
            mesh.update()
            ob=bpy.data.objects.new('Red cedar shingle %d %d %d'%(side,row,i),mesh)
            bpy.context.collection.objects.link(ob)
            finish(ob,random.choice(reds),0)
            for v in mesh.vertices:
                mesh.attributes['grain_coords'].data[v.index].vector=((v.index%2)*.8,0,(v.index//2)*.5)
            sol=ob.modifiers.new('Shingle thickness','SOLIDIFY')
            sol.thickness=.034
            sol.offset=-1
            bev=ob.modifiers.new('Worn shingle edges','BEVEL')
            bev.width=.008
            bev.segments=2
            ob.modifiers.new('Shingle normals','WEIGHTED_NORMAL')

# Broad curved hip caps with metal straps and a wooden finial.
for sx in (-1,1):
    for sy in (-1,1):
        for lo,hi in ((.08,.34),(.32,.57),(.55,.80),(.78,1.10)):
            def p(t): return (sx*.83*t,sy*.83*t,roof_z(min(t,1))+.076)
            beam('Golden hip cap',p(lo),p(hi),.145,.09,cut,.017)
        t=.92
        a=Vector((sx*.83*t,sy*.83*t,roof_z(t)+.09))
        b=Vector((sx*.83*(t+.09),sy*.83*(t+.09),roof_z(1)+.085))
        beam('Hip iron strap',a,b,.177,.118,iron,.01)
        rivet(tuple((a+b)*.5+Vector((0,0,.068))),(0,0,1),.026)
block('Crown timber finial',(0,0,3.23),(.19,.19,.26),cut,.025)
collar(0,0,3.18,.227,.10)

OUT.mkdir(parents=True,exist_ok=True)
REVIEW.mkdir(parents=True,exist_ok=True)
scene=bpy.context.scene
scene.render.engine='CYCLES'
scene.cycles.samples=16

# Retain editable parts in a hidden source collection; runtime uses one mesh.
source_collection=bpy.data.collections.new('Editable construction (hidden)')
scene.collection.children.link(source_collection)
for ob in parts:
    duplicate=ob.copy()
    duplicate.data=ob.data.copy()
    source_collection.objects.link(duplicate)
    duplicate.select_set(False)
    duplicate.hide_set(True)
source_collection.hide_render=True
source_collection.hide_viewport=True
bpy.ops.object.select_all(action='DESELECT')
for ob in parts:
    bpy.context.view_layer.objects.active=ob
    ob.select_set(True)
    for mod in list(ob.modifiers): bpy.ops.object.modifier_apply(modifier=mod.name)
    ob.select_set(False)
for ob in parts: ob.select_set(True)
bpy.context.view_layer.objects.active=parts[0]
bpy.ops.object.join()
model=bpy.context.object
model.name='Rapid Watchtower R2'
bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
bpy.ops.object.mode_set(mode='EDIT')
bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.uv.smart_project(angle_limit=math.radians(66),island_margin=.004)
bpy.ops.object.mode_set(mode='OBJECT')
atlas=bpy.data.images.new('RapidWatchtowerAlbedo',width=2048,height=2048,alpha=False)
for mat in model.data.materials:
    tex=mat.node_tree.nodes.new('ShaderNodeTexImage')
    tex.image=atlas
    mat.node_tree.nodes.active=tex
scene.render.bake.margin=8
bpy.ops.object.bake(type='EMIT')
atlas.filepath_raw=str(OUT/'RapidWatchtowerAlbedo.png')
atlas.file_format='PNG'
atlas.save()
final=bpy.data.materials.new('RapidWatchtowerAtlas')
final.use_nodes=True
bsdf=final.node_tree.nodes.get('Principled BSDF')
bsdf.inputs['Roughness'].default_value=.77
tex=final.node_tree.nodes.new('ShaderNodeTexImage')
tex.image=atlas
final.node_tree.links.new(tex.outputs['Color'],bsdf.inputs['Base Color'])
model.data.materials.clear()
model.data.materials.append(final)
for polygon in model.data.polygons: polygon.material_index=0
model.scale=(.59,)*3
bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
bpy.ops.export_scene.fbx(filepath=str(OUT/'RapidWatchtower.fbx'),use_selection=True,
    object_types={'MESH'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,
    apply_scale_options='FBX_SCALE_ALL',use_mesh_modifiers=True)

def light(name,location,energy,size,color):
    data=bpy.data.lights.new(name,'AREA')
    data.energy=energy
    data.shape='DISK'
    data.size=size
    data.color=color
    ob=bpy.data.objects.new(name,data)
    scene.collection.objects.link(ob)
    ob.location=location
    ob.rotation_euler=(Vector((0,0,1))-ob.location).to_track_quat('-Z','Y').to_euler()
light('Warm softbox',(-3,-4,6),350,4,(1,.88,.75))
light('Cool fill',(4,-1,3),100,4,(.77,.85,1))
light('Rim',(1,4,5),350,3,(1,.87,.73))
scene.world.color=(.08,.08,.08)
camera_data=bpy.data.cameras.new('Concept orthographic angle')
camera=bpy.data.objects.new('Concept orthographic angle',camera_data)
scene.collection.objects.link(camera)
camera.location=(3.5,-5.0,3.6)
camera.rotation_euler=(Vector((0,0,.98))-camera.location).to_track_quat('-Z','Y').to_euler()
camera_data.type='ORTHO'
camera_data.ortho_scale=2.42
scene.camera=camera
scene.cycles.samples=32
scene.cycles.use_denoising=True
scene.render.resolution_x=1024
scene.render.resolution_y=1024
scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'
scene.render.image_settings.color_mode='RGBA'
scene.view_settings.view_transform='AgX'
scene.render.film_transparent=True
scene.render.filepath=str(OUT/'RapidWatchtowerSprite.png')
SOURCE.parent.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE))
bpy.ops.render.render(write_still=True)
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.008))
ground=bpy.context.object
ground.name='Review backdrop only'
mat=bpy.data.materials.new('Neutral warm grey')
mat.use_nodes=True
mat.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.075,.082,.092,1)
ground.data.materials.append(mat)
scene.render.film_transparent=False
scene.render.filepath=str(REVIEW/'rapid_watchtower_r2_blender.png')
bpy.ops.render.render(write_still=True)
model.data.calc_loop_triangles()
stats={'revision':2,'vertices':len(model.data.vertices),'triangles':len(model.data.loop_triangles),
       'renderers':1,'materials':1,'atlas':[2048,2048],
       'dimensions_m':list(model.dimensions),'construction_parts':len(parts)}
(REVIEW/'rapid_watchtower_r2_stats.json').write_text(json.dumps(stats,indent=2))
print('S3_RAPID_BLENDER_OK '+json.dumps(stats))
