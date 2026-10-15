"""Original Stage 3 portal, approved concept B4 (intact dark stone / red vortex).
Run with Blender 5.2 --background --python art/source/build_entrance_portal.py.
All geometry and texture pixels are authored here; no third-party runtime art.
"""
from pathlib import Path
import bpy, math, json, random
from mathutils import Matrix, Vector
from mathutils.noise import noise

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Assets/Stage3/VisualPilot'
REVIEW = ROOT / 'art/review'
random.seed(63)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
parts = []

def material(name, color, emission=0):
    m = bpy.data.materials.new(name); m.use_nodes = True
    m.diffuse_color = (*color, 1)
    p = m.node_tree.nodes.get('Principled BSDF')
    p.inputs['Base Color'].default_value = (*color, 1)
    p.inputs['Roughness'].default_value = .82
    p.inputs['Emission Color'].default_value = (*color, 1)
    p.inputs['Emission Strength'].default_value = emission
    return m

def stone_material(index):
    m = material('Charcoal slate %d' % index, (.16, .18, .21))
    n, l = m.node_tree.nodes, m.node_tree.links
    n.clear(); output = n.new('ShaderNodeOutputMaterial'); emit = n.new('ShaderNodeEmission')
    l.new(emit.outputs[0], output.inputs['Surface'])
    geom = n.new('ShaderNodeNewGeometry')
    noise = n.new('ShaderNodeTexNoise'); noise.inputs['Scale'].default_value = 19
    noise.inputs['Detail'].default_value = 2.4
    l.new(geom.outputs['Position'], noise.inputs['Vector'])
    ramp = n.new('ShaderNodeValToRGB')
    tone = .82 + index*.065
    ramp.color_ramp.elements[0].position = .2
    ramp.color_ramp.elements[0].color = (.09*tone, .11*tone, .135*tone, 1)
    ramp.color_ramp.elements[1].position = .8
    ramp.color_ramp.elements[1].color = (.29*tone, .32*tone, .36*tone, 1)
    l.new(noise.outputs['Fac'], ramp.inputs[0])
    ao = n.new('ShaderNodeAmbientOcclusion'); ao.inputs['Distance'].default_value = .06; ao.samples=16
    mix = n.new('ShaderNodeMixRGB'); mix.blend_type='MULTIPLY'; mix.inputs[0].default_value=.6
    l.new(ramp.outputs[0], mix.inputs[1]); l.new(ao.outputs['Color'], mix.inputs[2])
    l.new(mix.outputs[0], emit.inputs['Color'])
    return m

stones = [stone_material(i) for i in range(5)]
red = material('Crimson inset gems', (.7, .015, .004), 1.4)

def finish(ob, name, mat, group, bevel=.012):
    ob.name=name; ob.data.materials.append(mat); ob['group']=group
    if bevel:
        mod=ob.modifiers.new('Intact softened cut edge','BEVEL'); mod.width=bevel; mod.segments=3
        ob.modifiers.new('Weighted stone normals','WEIGHTED_NORMAL')
    parts.append(ob); return ob

def block(name, loc, scale, mat, bevel=.012):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
    ob=bpy.context.object; ob.scale=scale
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return finish(ob,name,mat,'PortalStone',bevel)

# The approved revision owns a 3x3 footprint, matching the objective.
# The arch is complete; narrow intentional joints never become missing stones.
center_z=.565; outer=.455; inner=.330; depth=.17
segments=14
for i in range(segments):
    start=i*math.tau/segments+.005
    end=(i+1)*math.tau/segments-.005
    count=6; verts=[]
    for y in (-depth/2, depth/2):
        for r in (inner,outer):
            for j in range(count+1):
                a=start+(end-start)*j/count
                verts.append((r*math.cos(a),y,center_z+r*math.sin(a)))
    row=count+1; faces=[]
    for j in range(count):
        faces.extend([(j,j+1,row+j+1,row+j),
                      (2*row+j,3*row+j,3*row+j+1,2*row+j+1),
                      (j,2*row+j,2*row+j+1,j+1),
                      (row+j,row+j+1,3*row+j+1,3*row+j)])
    faces.extend([(0,row,3*row,2*row),
                  (count,2*row+count,3*row+count,row+count)])
    mesh=bpy.data.meshes.new('Fitted arch segment'); mesh.from_pydata(verts,[],faces); mesh.update()
    ob=bpy.data.objects.new('Complete ring stone %02d'%i,mesh); bpy.context.collection.objects.link(ob)
    finish(ob,ob.name,stones[i%5],'PortalStone',.008)

block('Foundation slab',(0,0,.045),(.95,.95,.09),stones[2])
for side in (-1,1):
    block('Intact outer buttress',(side*.355,.015,.185),(.235,.37,.28),stones[1])
    block('Buttress cap',(side*.365,-.012,.315),(.20,.30,.06),stones[3],.009)
block('Entrance lower step',(0,-.25,.075),(.55,.23,.065),stones[3],.009)
block('Entrance upper step',(0,-.16,.135),(.55,.18,.065),stones[1],.008)

# Three red diamond inlays in complete stone seats; no skeleton ornament or lore.
for a in (math.pi/2, math.pi*.95, math.pi*.05):
    x=.391*math.cos(a); z=center_z+.391*math.sin(a)
    seat=block('Diamond stone setting',(x,-.100,z),(.095,.032,.095),stones[0],.006)
    seat.rotation_euler.y=math.pi/4
    mesh=bpy.data.meshes.new('Faceted diamond inlay')
    mesh.from_pydata([(x-.028,-.12,z),(x,-.12,z+.043),(x+.028,-.12,z),
                     (x,-.12,z-.043),(x,-.143,z)],[],[(0,1,4),(1,2,4),(2,3,4),(3,0,4)])
    mesh.update(); ob=bpy.data.objects.new('Red diamond',mesh); bpy.context.collection.objects.link(ob)
    finish(ob,'Red inset',red,'PortalInlays',0)

# Flat circular portal aperture with UVs. Runtime shader adds movement only.
n=96; verts=[(0,-.09,center_z)]
for i in range(n):
    a=math.tau*i/n; verts.append((inner*math.cos(a),-.09,center_z+inner*math.sin(a)))
faces=[(0,i+1,(i+1)%n+1) for i in range(n)]
mesh=bpy.data.meshes.new('Portal aperture'); mesh.from_pydata(verts,[],faces); mesh.update()
uv=mesh.uv_layers.new(name='Portal UV')
for poly in mesh.polygons:
    for loop in poly.loop_indices:
        co=mesh.vertices[mesh.loops[loop].vertex_index].co
        uv.data[loop].uv=(.5+co.x/(2*inner),.5+(co.z-center_z)/(2*inner))
ob=bpy.data.objects.new('PortalVortex',mesh); bpy.context.collection.objects.link(ob)

# Preview shader texture is original analytic smoke; the Unity shader animates it.
size=512; img=bpy.data.images.new('Original red vortex',width=size,height=size,alpha=False)
pixels=[]
for y in range(size):
    for x in range(size):
        px=(x+.5)/size*2-1; py=(y+.5)/size*2-1
        r=math.sqrt(px*px+py*py); a=math.atan2(py,px)
        twist=a-4.8*math.log(r+.16)
        qx=r*math.cos(twist); qy=r*math.sin(twist)
        cloud=sum(noise(Vector((qx*f,qy*f,5.7)))*amp
                  for f,amp in ((3,.55),(7,.28),(17,.12),(41,.05)))
        filament=1-abs(cloud*2.6)
        core=min(1,max(0,(r-.045)/.28)); edge=max(0,1-abs(r-.965)*35)
        spiral=.58+.42*math.sin(a*2+math.log(r+.16)*7+cloud*3)
        intensity=(.055+max(0,filament)**7*.60+max(0,cloud+.25)*.65)*spiral*core+.5*edge
        pixels.extend((max(.008,intensity*.95),max(.001,intensity**3*.08),.002+max(0,intensity)*.009,1))
img.pixels.foreach_set(pixels); img.filepath_raw=str(OUT/'EntrancePortalVortex.png'); img.file_format='PNG'; img.save()
vortex=material('Red vortex preview',(.5,.005,.003),1)
p=vortex.node_tree.nodes.get('Principled BSDF'); tex=vortex.node_tree.nodes.new('ShaderNodeTexImage'); tex.image=img
vortex.node_tree.links.new(tex.outputs['Color'],p.inputs['Base Color'])
vortex.node_tree.links.new(tex.outputs['Color'],p.inputs['Emission Color'])
finish(ob,'PortalVortex',vortex,'PortalVortex',0)

# Keep the 3x3 foundation aligned to the build grid. Turn the upright arch,
# buttresses, steps and aperture together toward the diagonal enemy route.
for ob in parts:
    if ob.name != 'Foundation slab':
        ob.matrix_world = Matrix.Rotation(math.pi/4, 4, 'Z') @ ob.matrix_world

for ob in parts:
    bpy.context.view_layer.objects.active=ob
    for mod in list(ob.modifiers): bpy.ops.object.modifier_apply(modifier=mod.name)
runtime=[]
groups={group:[ob for ob in parts if ob['group']==group]
        for group in ('PortalStone','PortalInlays','PortalVortex')}
for group in ('PortalStone','PortalInlays','PortalVortex'):
    members=groups[group]
    bpy.ops.object.select_all(action='DESELECT')
    for ob in members: ob.select_set(True)
    bpy.context.view_layer.objects.active=members[0]
    if len(members)>1: bpy.ops.object.join()
    ob=members[0]; ob.name=group
    bpy.context.scene.cursor.location=(0,0,0); bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    # Ensure closed solid surfaces face outward.
    bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.mesh.normals_make_consistent(inside=False); bpy.ops.object.mode_set(mode='OBJECT')
    ob.scale=(3.1,3.1,3.1)
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    runtime.append(ob)

scene=bpy.context.scene; scene.render.engine='CYCLES'; scene.cycles.samples=16
stone=runtime[0]
bpy.ops.object.select_all(action='DESELECT'); stone.select_set(True); bpy.context.view_layer.objects.active=stone
bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.uv.smart_project(angle_limit=math.radians(60),island_margin=.006)
bpy.ops.object.mode_set(mode='OBJECT')
atlas=bpy.data.images.new('EntrancePortalStone',width=1024,height=1024,alpha=False)
for m in stone.data.materials:
    node=m.node_tree.nodes.new('ShaderNodeTexImage'); node.image=atlas; m.node_tree.nodes.active=node
scene.render.bake.margin=6; bpy.ops.object.bake(type='EMIT')
atlas.filepath_raw=str(OUT/'EntrancePortalStone.png'); atlas.file_format='PNG'; atlas.save()
final=material('Original baked charcoal slate',(.2,.2,.2))
node=final.node_tree.nodes.new('ShaderNodeTexImage'); node.image=atlas
final.node_tree.links.new(node.outputs['Color'],final.node_tree.nodes.get('Principled BSDF').inputs['Base Color'])
stone.data.materials.clear(); stone.data.materials.append(final)
for p in stone.data.polygons:p.material_index=0
bpy.ops.object.select_all(action='DESELECT')
for ob in runtime:ob.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(OUT/'EntrancePortal.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,apply_scale_options='FBX_SCALE_ALL')

scene.world.color=(.13,.15,.19); scene.cycles.samples=32; scene.cycles.use_denoising=True
for name,pos,power,color in [('Key',(-6,-9,15),3150,(1,.90,.80)),('Fill',(9,3,9),1170,(.55,.68,1))]:
    data=bpy.data.lights.new(name,'AREA'); data.energy=power; data.size=9; data.color=color
    ob=bpy.data.objects.new(name,data); scene.collection.objects.link(ob); ob.location=pos
    ob.rotation_euler=(Vector((0,0,1.4))-ob.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.mesh.primitive_plane_add(size=100,location=(0,0,-.006))
bpy.context.object.data.materials.append(material('Review ground',(.13,.14,.15)))
data=bpy.data.cameras.new('Tactical review camera'); cam=bpy.data.objects.new('Tactical review camera',data)
scene.collection.objects.link(cam); cam.location=(5.1,-9,8.7)
cam.rotation_euler=(Vector((0,0,1.43))-cam.location).to_track_quat('-Z','Y').to_euler()
data.type='ORTHO'; data.ortho_scale=5.6; scene.camera=cam
scene.render.resolution_x=1000; scene.render.resolution_y=1000; scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX'
scene.render.filepath=str(REVIEW/'entrance_portal_blender.png')
bpy.context.preferences.filepaths.save_version=0
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'art/source/entrance_portal.blend'))
bpy.ops.render.render(write_still=True)
for ob in runtime:ob.data.calc_loop_triangles()
points=[o.matrix_world @ v.co for o in runtime for v in o.data.vertices]
extent=[max(p[i] for p in points)-min(p[i] for p in points) for i in range(3)]
stats={'concept':'B4','triangles':sum(len(o.data.loop_triangles) for o in runtime),'meshes':len(runtime),'reserved_cells':[3,3],'footprint_cells':extent[:2],'height_cells':extent[2],'stone_texture':[1024,1024],'vortex_texture':[512,512],'source':'Original Blender geometry, procedural baked slate and analytic red vortex'}
(REVIEW/'entrance_portal_stats.json').write_text(json.dumps(stats,indent=2))
print('S3_PORTAL_BLENDER_OK '+json.dumps(stats))
