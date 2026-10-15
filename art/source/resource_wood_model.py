"""Original sculpted conifer B source, authored for the Stage 3 resource node.

The approved concept is a shape and palette reference only. Geometry and the
paint fields below are original, with no sampled concept or third-party pixels.
`build()` returns meshes without modifying the main driver's PARTS list.
Blender --background --python resource_wood_model.py -- --preview
"""
from pathlib import Path
import math
import random
import sys

import bpy
from mathutils import Vector

sys.path.insert(0, str(Path(__file__).resolve().parent))
from build_resource_sources import graph, noise, ramp, mix, ambient, scalar

ROOT = Path(__file__).resolve().parents[2]


def mesh_object(name, vertices, faces, material):
    data = bpy.data.meshes.new(name)
    data.from_pydata(vertices, [], faces)
    data.update()
    obj = bpy.data.objects.new(name, data)
    bpy.context.collection.objects.link(obj)
    data.materials.append(material)
    for face in data.polygons:
        face.use_smooth = True
    return obj


def foliage_material():
    mat, nodes, links, emit = graph('Original conifer bough paint')
    color = nodes.new('ShaderNodeVertexColor')
    color.layer_name = 'ConiferPaint'
    coord = nodes.new('ShaderNodeVertexColor')
    coord.layer_name = 'ConiferLocal'
    stretch = nodes.new('ShaderNodeVectorMath')
    stretch.operation = 'MULTIPLY'
    stretch.inputs[1].default_value = (4.0, 19.0, 6.0)
    links.new(coord.outputs['Color'], stretch.inputs[0])
    brush = ramp(nodes, links, noise(nodes, links, stretch.outputs[0], 3, 2).outputs['Fac'], [
        (.25, (.58, .66, .47)), (.50, (.84, .89, .73)), (.75, (1.06, 1.07, .91))])
    paint = mix(nodes, links, color.outputs['Color'], brush, .32, 'MULTIPLY')
    links.new(ambient(nodes, links, paint, .08, .44), emit.inputs['Color'])
    return mat


def bark_material():
    mat, nodes, links, emit = graph('Original sculpted cedar bark paint')
    coord = nodes.new('ShaderNodeNewGeometry').outputs['Position']
    separate=nodes.new('ShaderNodeSeparateXYZ'); links.new(coord,separate.inputs[0])
    angle=scalar(nodes,links,'ARCTAN2',separate.outputs['Y'],separate.outputs['X'])
    groove=scalar(nodes,links,'ADD',scalar(nodes,links,'MULTIPLY',angle,7),
                  scalar(nodes,links,'MULTIPLY',separate.outputs['Z'],1.8))
    groove=scalar(nodes,links,'MULTIPLY',scalar(nodes,links,'COSINE',groove),.5)
    groove=scalar(nodes,links,'ADD',groove,.5)
    paint=ramp(nodes,links,groove,[(.03,(.072,.021,.006)),(.17,(.17,.055,.014)),
                                   (.40,(.30,.115,.032)),(.85,(.37,.159,.050))])
    stretch = nodes.new('ShaderNodeVectorMath')
    stretch.operation = 'MULTIPLY'
    stretch.inputs[1].default_value = (3.5, 3.5, .23)
    links.new(coord, stretch.inputs[0])
    grain = ramp(nodes, links, noise(nodes, links, stretch.outputs[0], 11, 3).outputs['Fac'], [
        (.20, (.69,.62,.48)),(.80,(1.05,1.03,.95))])
    paint=mix(nodes,links,paint,grain,.16,'MULTIPLY')
    links.new(ambient(nodes, links, paint, .09, .5), emit.inputs['Color'])
    return mat


def catmull_closed(points, steps=3):
    """Soft leaf edges, with unequal natural tips authored in the outline."""
    out = []
    count = len(points)
    for i in range(count):
        a, b, c, d = [Vector(points[j % count]) for j in (i-1, i, i+1, i+2)]
        for j in range(steps):
            t = j / steps
            p = .5 * ((2*b) + (-a+c)*t + (2*a-5*b+4*c-d)*t*t + (-a+3*b-3*c+d)*t*t*t)
            out.append(p)
    return out


def bough(name, angle, start, length, zroot, ztip, width, material, seed):
    """One thick continuous bough, with five subtly unequal tapering fingers.

    Side tips are cut into the boundary of one curved volume; they are not
    repeated separate petals. Smooth upper ridges slope out from the branch.
    """
    rng = random.Random(seed)
    outline = [
        (0, 0), (.20, -.19), (.39, -.42), (.69, -.49),
        (.875, -.435), (.77, -.255), (.955, -.275), (1.06, -.195),
        (.88, -.07), (1.08, -.055), (1.16, .015), (.90, .10),
        (1.025, .25), (.94, .31), (.755, .245), (.84, .425),
        (.65, .46), (.38, .39), (.18, .17)]
    # Every bough has its own small asymmetry without destroying its sweep.
    outline = [(x * rng.uniform(.96, 1.04), y * rng.uniform(.95, 1.05)) for x, y in outline]
    boundary = catmull_closed(outline)
    # Catmull interpolation may briefly reverse angular order around a narrow
    # notch. The fan must advance monotonically around its center; otherwise
    # each radial ring folds into a thin reversed strip. Preserve every authored
    # point and its position, changing only the order used to connect them.
    boundary.sort(key=lambda point: math.atan2(point.y, point.x-.54))
    count, rings = len(boundary), 6
    verts, colors, coords, faces = [], [], [], []
    direction = Vector((math.cos(angle), math.sin(angle), 0))
    lateral = Vector((-math.sin(angle), math.cos(angle), 0))
    center = Vector((.54, 0))
    shade = rng.uniform(.93, 1.075)
    phase = rng.random() * 5
    for surface in (1, -1):
        for row in range(rings):
            r = (row + .025) / (rings - .975)
            for point in boundary:
                p = center + (point - center) * r
                x, y = p
                base = zroot + (ztip-zroot) * x + .052*math.sin(math.pi*x) + .025*min(x,1.1)**6
                shell = max(0, 1-r*r)**.6
                thickness = width * (.155 if surface > 0 else .055) * shell
                # Broad curving ribs soften the fan, with the center strongest.
                ridge = .010 * math.cos(y * 11 + x * 1.5 + phase) * shell
                z = base + surface*thickness + (ridge if surface > 0 else 0)
                v = direction*(start+length*x) + lateral*(width*y) + Vector((0, 0, z))
                verts.append(v)
                light = (0.82 + .18*x + .04*shell) * shade
                if surface < 0:
                    light *= .70
                colors.append(((.061+.078*x)*light, (.143+.122*x)*light, (.024+.030*x)*light, 1))
                coords.append((x, y+.5, seed*.071, 1))
        offset = (0 if surface > 0 else rings*count)
        # Center is a tiny polygon, avoiding a visible fan of long triangles.
        faces.append(tuple(offset + i for i in (range(count) if surface > 0 else reversed(range(count)))))
        for row in range(rings-1):
            for i in range(count):
                quad = (offset+row*count+i, offset+row*count+(i+1)%count,
                        offset+(row+1)*count+(i+1)%count, offset+(row+1)*count+i)
                faces.append(quad if surface < 0 else tuple(reversed(quad)))
    for i in range(count):
        faces.append(((rings-1)*count+i, (rings-1)*count+(i+1)%count,
                      (2*rings-1)*count+(i+1)%count, (2*rings-1)*count+i))
    # Both shells meet at a shared edge. Weld the known duplicate perimeter
    # analytically, retaining the authored vertex paint and removing zero-area
    # side quads before UV baking or export.
    bottom_edge = (2*rings-1)*count
    remap = lambda i: i-(rings*count) if i>=bottom_edge else i
    faces = [tuple(remap(i) for i in face) for face in faces]
    faces = [face for face in faces if len(set(face))>=3]
    verts, colors, coords = verts[:-count], colors[:-count], coords[:-count]
    obj = mesh_object(name, verts, faces, material)
    # The thin closed perimeter is a normal boundary: averaging the top and
    # bottom there tilts top edge normals down and causes a dark rim under
    # directional lighting. Keep smooth faces on each side,
    # but export separate corner normals across this geometric leaf edge.
    edge_start, edge_end = (rings-1)*count, rings*count
    for edge in obj.data.edges:
        if all(edge_start <= vertex < edge_end for vertex in edge.vertices):
            edge.use_edge_sharp = True
    obj.data.update()
    shell_faces = 1+(rings-1)*count
    if any(face.normal.z<=0 for face in list(obj.data.polygons)[:shell_faces]):
        raise ValueError(name+': upper bough shell has a reversed or vertical face')
    if any(face.normal.z>=0 for face in list(obj.data.polygons)[shell_faces:2*shell_faces]):
        raise ValueError(name+': lower bough shell has a reversed or vertical face')
    for attrname, values in [('ConiferPaint', colors), ('ConiferLocal', coords)]:
        attr = obj.data.color_attributes.new(name=attrname, type='FLOAT_COLOR', domain='POINT')
        for i, value in enumerate(values):
            attr.data[i].color = value
    return obj


def sculpted_trunk(material):
    verts, faces = [], []
    sides, rows = 40, 27
    for j in range(rows):
        t = j/(rows-1)
        z = t*1.68
        r = .087 + .050*(1-t) + .057*math.exp(-z*11)
        for i in range(sides):
            a = math.tau*i/sides
            flutes = .012*math.cos(a*7 + z*1.8) + .005*math.cos(a*13-z*2)
            radius = r + flutes*(1-t*.7)
            verts.append((radius*math.cos(a)+.025*math.sin(t*3), radius*math.sin(a), z))
    for j in range(rows-1):
        for i in range(sides):
            faces.append((j*sides+i,j*sides+(i+1)%sides,(j+1)*sides+(i+1)%sides,(j+1)*sides+i))
    faces.extend([tuple(reversed(range(sides))), tuple(range((rows-1)*sides,rows*sides))])
    parts = [mesh_object('Warm tapered fluted trunk', verts, faces, material)]
    for root in range(7):
        angle = root*math.tau/7+.11
        verts, faces = [], []
        for j in range(12):
            t = j/11
            r = .085 + (.23+.025*math.sin(root*6))*t
            z = .23*(1-t)**1.6+.017
            width = .068*(1-t)**.70 + .009
            for k in range(14):
                phi=math.tau*k/14
                v=Vector((r*math.cos(angle),r*math.sin(angle),z))
                v+=Vector((-math.sin(angle),math.cos(angle),0))*(width*math.cos(phi))
                v.z+=width*.58*math.sin(phi)
                verts.append(v)
        for j in range(11):
            for k in range(14):
                faces.append((j*14+k,j*14+(k+1)%14,(j+1)*14+(k+1)%14,(j+1)*14+k))
        faces.extend([tuple(reversed(range(14))),tuple(range(154,168))])
        parts.append(mesh_object('Sculpted ground root %d'%root,verts,faces,material))
    return parts


def build():
    """Return only original meshes. Source up=Z, grounded at origin."""
    parts = sculpted_trunk(bark_material())
    mat = foliage_material()
    rng = random.Random(90623)
    # Unequal tier spacing and bough counts preserve visible overlapping fans.
    tiers = [
        (1.20, .79, .545, .61, 6, .12),
        (1.51, 1.075, .46, .515, 6, .59),
        (1.76, 1.365, .355, .42, 5, .15),
        (1.99, 1.66, .24, .32, 5, .71)]
    for tier, (rootz, tipz, length, width, count, offset) in enumerate(tiers):
        for branch in range(count):
            angle = math.tau*branch/count + offset + rng.uniform(-.07,.07)
            parts.append(bough('Tier %d sculpted bough %d'%(tier,branch), angle,
                               .018, length*rng.uniform(.94,1.04),
                               rootz+rng.uniform(-.018,.018),tipz+rng.uniform(-.04,.025),
                               width*rng.uniform(.93,1.05),mat,100+tier*23+branch))
    # One closed terminal shoot joins the narrow upper boughs to a living tip.
    verts, faces, colors, coords = [], [], [], []
    rows, sides = 14, 45
    for row in range(rows):
        t = row/(rows-1)
        for i in range(sides):
            a = math.tau*i/sides
            r = max(.001,.125*(1-t)**1.58*(1+.18*math.cos(a*5)))
            z = 1.96+.33*t-.047*math.cos(a*5)*(1-t)**4
            verts.append((r*math.cos(a),r*math.sin(a),z))
            colors.append((.095+.035*t,.215+.040*t,.040+.013*t,1))
            coords.append((t,a*.12,5.8,1))
    for row in range(rows-1):
        for i in range(sides):
            faces.append((row*sides+i,row*sides+(i+1)%sides,(row+1)*sides+(i+1)%sides,(row+1)*sides+i))
    faces.extend([tuple(reversed(range(sides))),tuple(range((rows-1)*sides,rows*sides))])
    shoot = mesh_object('Closed tapered conifer crown',verts,faces,mat)
    for edge in shoot.data.edges:
        if all(vertex<sides for vertex in edge.vertices):
            edge.use_edge_sharp=True
    shoot.data.update()
    if any(face.normal.z<=0 for face in list(shoot.data.polygons)[:(rows-1)*sides]):
        raise ValueError('Conifer crown contains an inward-facing upper face')
    for attrname, values in [('ConiferPaint',colors),('ConiferLocal',coords)]:
        attr=shoot.data.color_attributes.new(name=attrname,type='FLOAT_COLOR',domain='POINT')
        for i,value in enumerate(values): attr.data[i].color=value
    parts.append(shoot)
    for obj in parts:
        # Match the established FBX root/ground pivot chain in Unity.
        obj.rotation_euler.z = math.pi
    return parts


def preview():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    parts = build()
    for mat in {m for obj in parts for m in obj.data.materials}:
        nodes, links=mat.node_tree.nodes,mat.node_tree.links
        emit=next(node for node in nodes if node.type=='EMISSION')
        p=nodes.new('ShaderNodeBsdfPrincipled')
        p.inputs['Roughness'].default_value=.88
        links.new(emit.inputs['Color'].links[0].from_socket,p.inputs['Base Color'])
        links.new(p.outputs[0],nodes.get('Material Output').inputs['Surface'])
    scene=bpy.context.scene
    scene.render.engine='CYCLES'; scene.cycles.samples=32; scene.cycles.use_denoising=True
    scene.world.color=(.21,.24,.29)
    for name,pos,power,color in [('Key',(3,4,7),750,(1,.92,.81)),('Fill',(-4,-1,5),160,(.65,.77,1))]:
        data=bpy.data.lights.new(name,'AREA');data.energy=power;data.size=4;data.color=color
        light=bpy.data.objects.new(name,data);scene.collection.objects.link(light);light.location=pos
        light.rotation_euler=(Vector((0,0,1))-light.location).to_track_quat('-Z','Y').to_euler()
    bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.027))
    ground=bpy.data.materials.new('Candidate neutral ground');ground.diffuse_color=(.18,.21,.15,1)
    bpy.context.object.data.materials.append(ground)
    data=bpy.data.cameras.new('Candidate camera');cam=bpy.data.objects.new('Candidate camera',data);scene.collection.objects.link(cam)
    target=Vector((0,0,.94));cam.location=target+Vector((0,4,5.713))
    cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();data.type='ORTHO';data.ortho_scale=2.55
    scene.camera=cam;scene.render.resolution_x=900;scene.render.resolution_y=900;scene.render.resolution_percentage=100
    scene.view_settings.view_transform='AgX'
    scene.render.filepath=str(ROOT/'art/review/WoodResourceSource_candidate.png')
    bpy.context.preferences.filepaths.save_version=0
    bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'art/review/WoodResourceSource_candidate.blend'))
    tri=0
    for obj in parts:
        obj.data.calc_loop_triangles();tri+=len(obj.data.loop_triangles)
    print('CONIFER_CANDIDATE_TRIANGLES',tri,flush=True)
    points=[obj.matrix_world @ vertex.co for obj in parts for vertex in obj.data.vertices]
    print('CONIFER_CANDIDATE_BOUNDS',[(round(min(p[i] for p in points),4),round(max(p[i] for p in points),4)) for i in range(3)],flush=True)
    bpy.ops.render.render(write_still=True)


if __name__=='__main__':
    preview()
