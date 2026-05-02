# One-off editable patch runner for Saves/scene/.../Gato87 - Dance - Heeeeey Mama.json
#
# - Removes ExpressionRandomizer plugins from Person
# - Disables named clothing items
# - Sets [CameraRig] and WindowCamera to eyeTargetControl world position and
#   yaw/pitch/roll from a LookAt toward Person main control (world up Y).
#
# Spine chain for eye target world pose matches typical VaM female torso order.
import json
import math
from pathlib import Path

SCENE_PATH = Path(
    r"d:\Games\VaM\Saves\scene\Gato87\Hey Mama -  Trap mix\Gato87 - Dance - Heeeeey Mama.json"
)

FC_SPINE_CHAIN = [
    "hipControl",
    "abdomenControl",
    "abdomen2Control",
    "chestControl",
    "neckControl",
    "headControl",
    "eyeTargetControl",
]

DISABLE_CLOTHING_IDS = frozenset(
    {"Alphakini Bra Sim", "Heat Up Panty Sim"}
)


def fv(d, key):
    return float(d[key])


def fmt_coord(x):
    """Format float as VaM-compatible numeric string; never return empty."""
    if x == 0.0:
        return "0"
    s = ("%0.15g" % x).rstrip("0").rstrip(".")
    return s if s else "0"


def euler_unity_deg_to_rotation_matrix(ex, ey, ez):
    """Unity-style Euler (degrees): apply rotations around Z, then X, then Y on a fresh basis."""
    ex = math.radians(ex)
    ey = math.radians(ey)
    ez = math.radians(ez)
    cx, sx = math.cos(ex * 0.5), math.sin(ex * 0.5)
    cy, sy = math.cos(ey * 0.5), math.sin(ey * 0.5)
    cz, sz = math.cos(ez * 0.5), math.sin(ez * 0.5)
    qx = sx * cy * cz - cx * sy * sz
    qy = cx * sy * cz + sx * cy * sz
    qz = cx * cy * sz - sx * sy * cz
    qw = cx * cy * cz + sx * sy * sz
    qx, qy, qz, qw = qx, qy, qz, qw
    xx, yy, zz = qx * qx, qy * qy, qz * qz
    xy, xz, yz = qx * qy, qx * qz, qy * qz
    wx, wy, wz = qw * qx, qw * qy, qw * qz
    m = [
        [1 - 2 * (yy + zz), 2 * (xy - wz), 2 * (xz + wy)],
        [2 * (xy + wz), 1 - 2 * (xx + zz), 2 * (yz - wx)],
        [2 * (xz - wy), 2 * (yz + wx), 1 - 2 * (xx + yy)],
    ]
    return m


def mat_vec(m, v):
    return [
        m[0][0] * v[0] + m[0][1] * v[1] + m[0][2] * v[2],
        m[1][0] * v[0] + m[1][1] * v[1] + m[1][2] * v[2],
        m[2][0] * v[0] + m[2][1] * v[1] + m[2][2] * v[2],
    ]


def mat_mul(a, b):
    out = [[0.0, 0.0, 0.0], [0.0, 0.0, 0.0], [0.0, 0.0, 0.0]]
    for i in range(3):
        for j in range(3):
            out[i][j] = a[i][0] * b[0][j] + a[i][1] * b[1][j] + a[i][2] * b[2][j]
    return out




def transform_compose(parent_m, parent_t, local_m, local_t):
    """parent * local for column-vector convention (p = Rl * pl + tl; R = Rp * Rl)."""
    r = mat_mul(parent_m, local_m)
    t = mat_vec(parent_m, local_t)
    t[0] += parent_t[0]
    t[1] += parent_t[1]
    t[2] += parent_t[2]
    return r, t


def get_storable_map(person):
    out = {}
    for s in person.get("storables", []):
        sid = s.get("id")
        if sid is not None:
            out[sid] = s
    return out


def fc_local_trs(st):
    lp = st["localPosition"]
    lr = st["localRotation"]
    local_t = [fv(lp, "x"), fv(lp, "y"), fv(lp, "z")]
    local_m = euler_unity_deg_to_rotation_matrix(fv(lr, "x"), fv(lr, "y"), fv(lr, "z"))
    return local_m, local_t


def find_atom(j, atom_id):
    for a in j.get("atoms", []):
        if a.get("id") == atom_id:
            return a
    return None


def main():
    path = SCENE_PATH
    backup = path.with_suffix(".json.bak-patch")
    if not backup.exists():
        backup.write_bytes(path.read_bytes())

    text = path.read_text(encoding="utf-8")
    data = json.loads(text)

    person = find_atom(data, "Person")
    assert person is not None, "Person atom missing"

    cmap = get_storable_map(person)
    ctrl = cmap.get("control")
    assert ctrl is not None

    gp = ctrl["position"]
    gr = ctrl["rotation"]
    person_m = euler_unity_deg_to_rotation_matrix(fv(gr, "x"), fv(gr, "y"), fv(gr, "z"))
    person_t = [fv(gp, "x"), fv(gp, "y"), fv(gp, "z")]

    wm = person_m
    wt = [person_t[0], person_t[1], person_t[2]]
    fc_chain = FC_SPINE_CHAIN
    for name in fc_chain:
        if name not in cmap:
            raise RuntimeError(
                "Missing FreeController %r needed for spine chain eye pose" % (name,)
            )
        lm, lt = fc_local_trs(cmap[name])
        wm, wt = transform_compose(wm, wt, lm, lt)

    eye_px, eye_py, eye_pz = wt

    cx = fv(ctrl["position"], "x")
    cy = fv(ctrl["position"], "y")
    cz = fv(ctrl["position"], "z")
    lx = cx - eye_px
    ly = cy - eye_py
    lz = cz - eye_pz
    horizontal = math.sqrt(lx * lx + lz * lz)
    pitch_rad = math.atan2(ly, horizontal)
    yaw_rad = math.atan2(lx, lz)
    ex = math.degrees(pitch_rad)
    ey = math.degrees(yaw_rad)
    ez = 0.0

    euler_x_str = fmt_coord(ex)
    euler_y_str = fmt_coord(ey)
    euler_z_str = fmt_coord(ez)

    pos_x_str = fmt_coord(eye_px)
    pos_y_str = fmt_coord(eye_py)
    pos_z_str = fmt_coord(eye_pz)

    rig = find_atom(data, "[CameraRig]")
    win = find_atom(data, "WindowCamera")
    assert rig is not None and win is not None

    rig["position"] = {"x": pos_x_str, "y": pos_y_str, "z": pos_z_str}
    rig["rotation"] = {"x": euler_x_str, "y": euler_y_str, "z": euler_z_str}

    win["position"] = {"x": pos_x_str, "y": pos_y_str, "z": pos_z_str}
    win["rotation"] = {"x": euler_x_str, "y": euler_y_str, "z": euler_z_str}
    win["containerPosition"] = {"x": pos_x_str, "y": pos_y_str, "z": pos_z_str}
    win["containerRotation"] = {"x": euler_x_str, "y": euler_y_str, "z": euler_z_str}

    for s in win.get("storables", []):
        if s.get("id") == "control":
            s["position"] = {"x": pos_x_str, "y": pos_y_str, "z": pos_z_str}
            s["rotation"] = {"x": euler_x_str, "y": euler_y_str, "z": euler_z_str}
            break

    data["monitorCameraRotation"] = {
        "x": euler_x_str,
        "y": euler_y_str,
        "z": euler_z_str,
    }

    for s in person.get("storables", []):
        if s.get("id") == "PluginManager" and "plugins" in s:
            pm = s["plugins"]
            to_del = [k for k, v in pm.items() if "ExpressionRandomizer" in str(v)]
            for k in to_del:
                del pm[k]
            break

    person["storables"] = [
        s
        for s in person["storables"]
        if "ExpressionRandomizer" not in s.get("id", "")
    ]

    for s in person.get("storables", []):
        if s.get("id") != "geometry":
            continue
        for item in s.get("clothing", []):
            cid = item.get("id")
            if cid in DISABLE_CLOTHING_IDS:
                item["enabled"] = "false"
        break

    out = json.dumps(data, ensure_ascii=False, separators=(", ", " : "), indent=3)
    if not out.rstrip().endswith("}"):
        raise RuntimeError("Serialized JSON does not end with }; aborting write")

    path.write_text(out + "\n", encoding="utf-8")
    print("Wrote", path)
    print("Eye world (approx):", pos_x_str, pos_y_str, pos_z_str)
    print("Rig euler deg:", euler_x_str, euler_y_str, euler_z_str)


if __name__ == "__main__":
    main()
