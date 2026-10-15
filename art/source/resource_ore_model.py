"""Original Stage 3 iron ore A: dark fractured masses with recessed ironstone.

build() returns mesh objects only. It does not export, bake, or modify a Unity
asset. Source front is -Y; the returned objects carry the Rz(pi) export turn
needed by the existing FBX/ground-pivot convention. All source materials have
an Emission albedo and a Principled node named 'Relief bake surface'.

Preview: Blender --background --python resource_ore_model.py -- --preview
Geometry and mineral materials are project-authored. Dark stone paint is an
original imagegen output; its exact prompt/provenance is stored beside it.
"""
from pathlib import Path
import json
import math
import sys
import bpy
import bmesh
import numpy as np
from mathutils import Vector
from mathutils.noise import noise_vector

sys.dont_write_bytecode = True
SOURCE = Path(__file__).resolve().parent
ROOT = SOURCE.parents[1]
if str(SOURCE) not in sys.path:
    sys.path.insert(0, str(SOURCE))
from build_resource_sources import graph, noise, ramp, mix, ambient


def _stone_material():
    mat,n,l,emit=graph('Ore original dark slate paint')
    coord=n.new('ShaderNodeNewGeometry').outputs['Position']
    paint=n.new('ShaderNodeTexImage')
    paint.image=bpy.data.images.load(str(SOURCE/'ore_slate_painted_albedo.png'),check_existing=True)
    paint.image.pack();paint.projection='BOX';paint.projection_blend=.25
    scale=n.new('ShaderNodeVectorMath');scale.operation='SCALE';scale.inputs[3].default_value=1.25
    l.new(coord,scale.inputs[0]);l.new(scale.outputs[0],paint.inputs['Vector'])
    color=mix(n,l,paint.outputs['Color'],(1.65,1.70,1.78),1,'MULTIPLY')
    color=mix(n,l,color,(.145,.17,.205),.25)
    tint=n.new('ShaderNodeVertexColor');tint.layer_name='OreTint'
    color=mix(n,l,color,tint.outputs['Color'],.18)
    l.new(ambient(n,l,color,.11,.35),emit.inputs['Color'])
    p=n.new('ShaderNodeBsdfPrincipled');p.name='Relief bake surface';p.inputs['Roughness'].default_value=.92
    bump=n.new('ShaderNodeBump');bump.inputs['Distance'].default_value=.0017;bump.inputs['Strength'].default_value=.35
    l.new(noise(n,l,coord,72,3).outputs['Fac'],bump.inputs['Height']);l.new(bump.outputs[0],p.inputs['Normal'])
    return mat


def _rust_material():
    mat,n,l,emit=graph('Ore exposed recessed iron mineral')
    coord=n.new('ShaderNodeNewGeometry').outputs['Position']
    patch=noise(n,l,coord,17,4)
    color=ramp(n,l,patch.outputs['Fac'],[
        (.17,(.15,.040,.012)),(.43,(.265,.078,.023)),
        (.64,(.38,.142,.043)),(.86,(.46,.205,.067))])
    grain=ramp(n,l,noise(n,l,coord,82,2).outputs['Fac'],[(.2,(.58,.47,.36)),(.8,(1,1,1))])
    color=mix(n,l,color,grain,.24,'MULTIPLY')
    l.new(ambient(n,l,color,.075,.35),emit.inputs['Color'])
    p=n.new('ShaderNodeBsdfPrincipled');p.name='Relief bake surface';p.inputs['Roughness'].default_value=.96
    broad=n.new('ShaderNodeBump');broad.inputs['Distance'].default_value=.005;broad.inputs['Strength'].default_value=.38
    l.new(patch.outputs['Fac'],broad.inputs['Height'])
    fine=n.new('ShaderNodeBump');fine.inputs['Distance'].default_value=.0015;fine.inputs['Strength'].default_value=.4
    l.new(noise(n,l,coord,96,3).outputs['Fac'],fine.inputs['Height']);l.new(broad.outputs[0],fine.inputs['Normal'])
    l.new(fine.outputs[0],p.inputs['Normal'])
    return mat


# Unequal cleavage surfaces. Z is centered during the radial projection, and
# moved to a flat grounded footprint only after the profile is constructed.
_PROFILES = {
    'spire':[(1,-.10,-.09,.44),(-1,.13,.12,.47),(.1,1,.06,.43),(-.14,-1,.05,.42),
             (.22,.13,1,.50),(0,0,-1,.50),(-.75,-1,.15,.63),(1,-.52,.19,.59),
             (.83,.71,.13,.60),(-.79,.72,.07,.65),(-1,-.17,.90,.70),
             (.84,-.27,.9,.73),(.11,.82,.85,.68),(-.63,-.92,-.3,.65)],
    'front':[(1,.12,.10,.46),(-1,-.12,.30,.46),(.12,1,-.06,.41),(-.04,-1,.02,.42),
             (-.07,.16,1,.47),(0,0,-1,.50),(-.7,-1,.07,.62),(.85,-.65,.13,.59),
             (.87,.77,.10,.64),(-.86,.58,.11,.60),(-.9,-.2,.72,.69),
             (.94,-.21,.78,.70),(.05,-.85,.70,.71),(.27,1,.73,.68),
             (-.58,-.7,-.5,.59)],
    'shoulder':[(1,-.17,.08,.44),(-1,.05,.05,.42),(.1,1,-.04,.42),(.17,-1,.04,.45),
                (.25,-.12,1,.44),(0,0,-1,.50),(.72,-1,.12,.61),(-.8,-.69,.22,.58),
                (.71,.8,.12,.57),(-.9,.59,.21,.62),(-.75,-.2,.79,.66),(.85,.35,.87,.70)],
    'low':[(1,.18,.09,.45),(-1,-.05,.12,.44),(.06,1,.08,.43),(-.13,-1,-.06,.42),
           (.07,-.13,1,.46),(0,0,-1,.50),(-.81,-1,.19,.65),(.91,-.69,.23,.58),
           (.81,.71,.17,.61),(-.94,.60,.03,.59),(-.6,-.5,.77,.67),(.55,.8,.77,.66)],
    'core':[(1,.02,.1,.48),(-1,.04,.07,.48),(.03,1,.04,.45),(.02,-1,.02,.44),
            (.14,-.08,1,.44),(0,0,-1,.50),(-.8,-1,.08,.69),(.9,-.7,.07,.67),
            (.7,.85,.13,.62),(-.7,.8,.04,.64),(-.7,-.3,.8,.68),(.7,.4,.85,.67)]}


def _rock(name,profile,center,size,angle,seed,material,parts,roughness=.019):
    planes=_PROFILES[profile]
    normals=np.array([p[:3] for p in planes]);distances=np.array([p[3] for p in planes])
    bm=bmesh.new();bmesh.ops.create_icosphere(bm,subdivisions=4,radius=1)
    ca,sa=math.cos(angle),math.sin(angle)
    for v in bm.verts:
        ray=np.array(v.co);dots=normals@ray
        distance=min(distances[i]/d for i,d in enumerate(dots) if d>0)
        p=Vector(ray*distance)
        p+=noise_vector(p*6.1+Vector((seed*.71,2.1,1.4)))*roughness
        p+=noise_vector(p*15.3+Vector((.7,seed*.37,1.0)))*roughness*.27
        x,y,z=p;x*=size[0];y*=size[1]
        v.co=(center[0]+x*ca-y*sa,center[1]+x*sa+y*ca,center[2]+max(0,z+.5)*size[2])
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
    mesh=bpy.data.meshes.new(name);bm.to_mesh(mesh);bm.free()
    ob=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(ob);mesh.materials.append(material)
    for f in mesh.polygons:f.use_smooth=True
    bpy.context.view_layer.objects.active=ob;ob.select_set(True)
    decimate=ob.modifiers.new('Preserve broken masses','DECIMATE');decimate.ratio=.57
    bpy.ops.object.modifier_apply(modifier=decimate.name)
    for f in ob.data.polygons:f.use_smooth=True
    ob.data.update()
    tint=ob.data.color_attributes.new(name='OreTint',type='FLOAT_COLOR',domain='POINT')
    for v in ob.data.vertices:
        n=v.normal;up=max(n.z,0);left=max(-n.x,0)
        tint.data[v.index].color=(.145+.075*up+.02*left,.165+.065*up+.017*left,.198+.045*up,1)
    ob.select_set(False);parts.append(ob)
    return ob


def build():
    """Create the approved ore-A visual. Return owned mesh objects only."""
    parts=[];slate=_stone_material();rust=_rust_material()
    # This continuous ironstone interior is exposed only inside the spaces
    # between the separately broken dark pieces; veins have real depth.
    _rock('Recessed rusty interior','core',(.015,.088,-.006),(.65,.555,.93),-.10,4,rust,parts,.029)
    _rock('Left high leaning dark mass','spire',(-.157,.112,0),(.54,.52,1.06),-.14,11,slate,parts)
    _rock('Forward broken central dark mass','front',(.152,-.105,0),(.50,.49,.97),.13,23,slate,parts)
    _rock('Right rear dark shoulder','shoulder',(.341,.225,0),(.41,.53,.71),-.21,35,slate,parts)
    _rock('Lower left dark shoulder','shoulder',(-.375,-.047,0),(.40,.43,.54),.18,47,slate,parts)
    # The low foreground stones expose iron internally between small cleaves.
    _rock('Left foreground iron interior','low',(-.435,-.292,-.009),(.43,.245,.295),-.16,53,rust,parts)
    _rock('Left foreground upper dark cleave','low',(-.487,-.246,0),(.39,.27,.375),-.25,59,slate,parts)
    _rock('Left foreground lower dark cleave','low',(-.406,-.392,0),(.37,.235,.265),.13,67,slate,parts)
    _rock('Right foreground iron interior','low',(.484,-.268,-.012),(.32,.235,.24),.20,73,rust,parts)
    _rock('Right foreground top dark cleave','low',(.481,-.226,0),(.34,.23,.30),.28,79,slate,parts)
    _rock('Right foreground lower dark cleave','low',(.498,-.406,0),(.28,.195,.19),-.11,83,slate,parts)
    _rock('Small loose front chip','low',(.183,-.505,0),(.215,.20,.135),-.40,89,slate,parts)
    _rock('Small left loose chip','low',(-.599,-.024,0),(.18,.225,.20),.22,97,slate,parts)
    for ob in parts:
        ob.rotation_euler.z=math.pi
    return parts


def _preview():
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
    parts=build();scene=bpy.context.scene
    for material in {m for ob in parts for m in ob.data.materials}:
        nodes,links=material.node_tree.nodes,material.node_tree.links
        emit=next(n for n in nodes if n.type=='EMISSION');p=nodes['Relief bake surface']
        links.new(emit.inputs['Color'].links[0].from_socket,p.inputs['Base Color'])
        links.new(p.outputs[0],nodes.get('Material Output').inputs['Surface'])
    for name,pos,power,color in [('Warm key',(3,4,7),780,(1,.925,.82)),('Soft fill',(-4,-1,5),180,(.68,.80,1))]:
        data=bpy.data.lights.new(name,'AREA');data.energy=power;data.size=4;data.color=color
        ob=bpy.data.objects.new(name,data);scene.collection.objects.link(ob);ob.location=pos
        ob.rotation_euler=(Vector((0,0,.5))-ob.location).to_track_quat('-Z','Y').to_euler()
    scene.world.color=(.21,.24,.29)
    bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.012))
    ground=bpy.data.materials.new('Review ground');ground.diffuse_color=(.22,.25,.18,1);ground.use_nodes=True
    ground.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.22,.25,.18,1)
    ground.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.95
    bpy.context.object.data.materials.append(ground)
    data=bpy.data.cameras.new('Actual tactical pitch');cam=bpy.data.objects.new('Actual tactical pitch',data);scene.collection.objects.link(cam)
    target=Vector((0,0,.46));cam.location=target+Vector((0,4,5.713))
    cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();data.type='ORTHO';data.ortho_scale=2.03;scene.camera=cam
    scene.render.engine='CYCLES';scene.cycles.samples=48;scene.cycles.use_denoising=True
    scene.render.resolution_x=1000;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
    scene.view_settings.view_transform='AgX';scene.render.image_settings.file_format='PNG'
    review=ROOT/'art/review';scene.render.filepath=str(review/'IronOreResourceSource_candidate.png')
    bpy.context.preferences.filepaths.save_version=0
    bpy.ops.wm.save_as_mainfile(filepath=str(review/'IronOreResourceSource_candidate.blend'))
    bounds=[]
    for axis in range(3):
        values=[(ob.matrix_world@v.co)[axis] for ob in parts for v in ob.data.vertices]
        bounds.append([round(min(values),4),round(max(values),4)])
    triangles=0
    for ob in parts:ob.data.calc_loop_triangles();triangles+=len(ob.data.loop_triangles)
    stats={'triangles':triangles,'parts_before_join':len(parts),'materials_before_bake':2,'bounds':bounds,
           'size':[round(v[1]-v[0],4) for v in bounds],'preview_only':True}
    (review/'IronOreResourceSource_candidate.json').write_text(json.dumps(stats,indent=2))
    bpy.ops.render.render(write_still=True)
    print('ORE_CANDIDATE_READY '+json.dumps(stats),flush=True)


if __name__=='__main__' and '--preview' in sys.argv:
    _preview()
