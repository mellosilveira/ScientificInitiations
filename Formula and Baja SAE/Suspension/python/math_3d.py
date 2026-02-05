import math
from typing import Optional
from models import (
    Point2D, Point3D, ForceVector, ForceResult3D,
    SuspensionGeometry3D, AlignmentResult, VehicleGeometry,
    KingpinResult, SteeringResult, SuspensionGeometry2D,
    EPSILON
)
from utils import to_deg, to_rad
import math_2d


# =========================================================
# VECTOR MATH HELPERS
# =========================================================

def vec_sub(p1: Point3D, p2: Point3D) -> Point3D:
    return Point3D(p1.x - p2.x, p1.y - p2.y, p1.z - p2.z)


def vector_len(p1: Point3D, p2: Point3D) -> float:
    return math.hypot(p2.x - p1.x, p2.y - p1.y, p2.z - p1.z)


# =========================================================
# 3D CALCULATIONS (Forces, Alignment, Anti-dive)
# =========================================================

def calculate_3d_roll_center(geo_vehicle: VehicleGeometry) -> Optional[Point3D]:
    """
    Helper to visualize Roll Center in 3D.
    Uses 2D logic to find height (Y), then places it at the axle centerline (Z).
    """
    # Project 3D points to 2D plane (X-Y) for Reimpell calc
    geo_2d = SuspensionGeometry2D(
        track_width=geo_vehicle.track_front,
        upper_in=Point2D(geo_vehicle.sup_in.x, geo_vehicle.sup_in.y),
        upper_out=Point2D(geo_vehicle.sup_out.x, geo_vehicle.sup_out.y),
        lower_in=Point2D(geo_vehicle.inf_in.x, geo_vehicle.inf_in.y),
        lower_out=Point2D(geo_vehicle.inf_out.x, geo_vehicle.inf_out.y),
        s1=0, s2=0, camber_out_deg=0, camber_in_deg=0
    )

    res_2d = math_2d.calculate_roll_center(geo_2d)

    if res_2d.h_ro is None:
        return None

    # Assume Roll Center Z is the average of wheel centers Z
    z_avg = (geo_vehicle.sup_out.z + geo_vehicle.inf_out.z) / 2.0

    return Point3D(0.0, res_2d.h_ro, z_avg)


def calculate_alignment(geo: SuspensionGeometry3D) -> AlignmentResult:
    """
    Calculates static alignment angles (Camber, Caster, Toe) by projecting 3D vectors.
    """
    # 1. Projected Instant Center (Front View X-Y) for reference
    sup_in_2d = Point2D(geo.sup_in.x, geo.sup_in.y)
    sup_out_2d = Point2D(geo.sup_out.x, geo.sup_out.y)
    inf_in_2d = Point2D(geo.inf_in.x, geo.inf_in.y)
    inf_out_2d = Point2D(geo.inf_out.x, geo.inf_out.y)

    l_sup = math_2d.get_line_coefficients(sup_in_2d, sup_out_2d)
    l_inf = math_2d.get_line_coefficients(inf_in_2d, inf_out_2d)
    ic = math_2d.solve_intersection(l_sup, l_inf)

    # 2. Camber (Front Plane X-Y projection of kingpin axis)
    dx = geo.spindle_sup.x - geo.spindle_inf.x
    dy = geo.spindle_sup.y - geo.spindle_inf.y
    camber = to_deg(math.atan(dx / dy)) if abs(dy) > EPSILON else 0.0

    # 3. Caster (Side Plane Z-Y projection)
    # Convention: Top leaning back (negative Z if Z is front) is positive caster.
    dz_cast = geo.spindle_sup.z - geo.spindle_inf.z
    caster = to_deg(math.atan(-dz_cast / dy)) if abs(dy) > EPSILON else 0.0

    # 4. Toe (Top Plane X-Z projection of wheel face)
    dx_toe = geo.toe_front.x - geo.toe_rear.x
    dz_toe = geo.toe_front.z - geo.toe_rear.z
    toe = to_deg(math.atan(dx_toe / dz_toe)) if abs(dz_toe) > EPSILON else 0.0

    return AlignmentResult(camber, caster, toe, ic)


def calculate_forces(geo: SuspensionGeometry3D) -> Optional[ForceResult3D]:
    """
    Calculates Axial Forces in A-arms.
    Distributes longitudinal tire force (Fx) based on stiffness ratios.
    """
    l_sup = vector_len(geo.sup_in, geo.sup_out)
    l_inf = vector_len(geo.inf_in, geo.inf_out)

    if l_sup < EPSILON or l_inf < EPSILON: return None

    # Z-axis direction cosine
    ez_sup = (geo.sup_out.z - geo.sup_in.z) / l_sup
    ez_inf = (geo.inf_out.z - geo.inf_in.z) / l_inf

    # Equilibrium: F_sup_z + F_inf_z = Fx_tire
    denom = (geo.stiffness_ratio_sup * ez_sup) + (geo.stiffness_ratio_inf * ez_inf)
    if abs(denom) < EPSILON: return None

    k_force = geo.fx_tire / denom
    fax_sup = k_force * geo.stiffness_ratio_sup
    fax_inf = k_force * geo.stiffness_ratio_inf

    def decompose(p_in, p_out, fax, length):
        dx = p_out.x - p_in.x
        dy = p_out.y - p_in.y
        dz = p_out.z - p_in.z
        return ForceVector(
            fx=fax * (dx / length),
            fy=fax * (dy / length),
            fz=fax * (dz / length),
            axial=fax, length=length
        )

    vec_sup = decompose(geo.sup_in, geo.sup_out, fax_sup, l_sup)
    vec_inf = decompose(geo.inf_in, geo.inf_out, fax_inf, l_inf)

    # Total reaction at chassis mounts
    vec_total = ForceVector(
        fx=vec_sup.fx + vec_inf.fx,
        fy=vec_sup.fy + vec_inf.fy,
        fz=vec_sup.fz + vec_inf.fz,
        axial=0.0, length=0.0
    )

    return ForceResult3D(vec_sup, vec_inf, vec_total)


def calculate_anti_dive(fx: float, fy_total: float, h_cg: float, wheelbase: float) -> Optional[float]:
    """
    Anti-Dive % = (Fy_suspension / Fz_transfer_needed) * 100
    """
    if abs(wheelbase) < EPSILON: return None
    fz_needed = fx * (h_cg / wheelbase)
    if abs(fz_needed) < EPSILON: return 0.0
    return (fy_total / fz_needed) * 100.0


# =========================================================
# DYNAMICS HELPERS (Kingpin, Scrub, Plotting)
# =========================================================

def calculate_kingpin_metrics(geo: VehicleGeometry) -> KingpinResult:
    # Kingpin Vector
    v_kp = vec_sub(geo.sup_out, geo.inf_out)
    dx, dy, dz = v_kp.x, v_kp.y, v_kp.z

    # KPI (Front View Angle)
    kpi = to_deg(math.atan(dx / dy)) if abs(dy) > EPSILON else 90.0

    # Caster (Side View Angle - Top back is positive)
    caster = to_deg(math.atan(-dz / dy)) if abs(dy) > EPSILON else 0.0

    # Scrub Radius (Ground Intercept Y=0)
    slope_x = dx / dy if abs(dy) > EPSILON else 0
    x_ground = geo.inf_out.x - (geo.inf_out.y * slope_x)

    x_wheel_center = geo.track_front / 2.0
    scrub_radius = x_wheel_center - x_ground

    # Mechanical Trail
    slope_z = dz / dy if abs(dy) > EPSILON else 0
    z_ground = geo.inf_out.z - (geo.inf_out.y * slope_z)

    z_wheel = (geo.sup_out.z + geo.inf_out.z) / 2.0
    mech_trail = z_ground - z_wheel

    return KingpinResult(abs(kpi), caster, scrub_radius, mech_trail)


def calculate_ackermann(geo: VehicleGeometry, steer_angle_inner_deg: float = 20.0) -> SteeringResult:
    """Calculates Ackermann percentage."""
    if abs(geo.wheelbase) < EPSILON:
        return SteeringResult(0, 0, 0, 0)

    rad_in = to_rad(steer_angle_inner_deg)
    if abs(math.sin(rad_in)) < EPSILON: return SteeringResult(0, 0, 0, 0)

    # Theoretical outer angle for 100% Ackermann
    cot_in = 1.0 / math.tan(rad_in)
    cot_out_theory = cot_in + (geo.track_front / geo.wheelbase)
    angle_out_theory = to_deg(math.atan(1.0 / cot_out_theory))

    turning_radius = geo.wheelbase / math.sin(rad_in)

    return SteeringResult(100.0, turning_radius / 1000.0, steer_angle_inner_deg, angle_out_theory)


def transform_to_rear_left_origin(point: Point3D, geo: VehicleGeometry) -> Point3D:
    """
    Transforms point to Global Frame (Rear Left Wheel = 0,0,0).
    Local Z=0 is Front Axle.
    """
    trans_x = geo.track_rear / 2.0
    trans_y = 0.0
    trans_z = geo.wheelbase
    return Point3D(point.x + trans_x, point.y, point.z + trans_z)


def generate_full_car_plot_points(geo: VehicleGeometry, ro: Optional[Point3D]):
    """Generates transformed points for 3D plotting."""
    fr_points = {
        "Sup In": geo.sup_in, "Sup Out": geo.sup_out,
        "Inf In": geo.inf_in, "Inf Out": geo.inf_out,
        "Tie In": geo.tie_rod_in, "Tie Out": geo.tie_rod_out
    }

    fl_points = {k: Point3D(-p.x, p.y, p.z) for k, p in fr_points.items()}

    transformed = {}

    def add_corner(prefix, p_dict):
        for n, p in p_dict.items():
            transformed[f"{prefix} {n}"] = transform_to_rear_left_origin(p, geo)

    add_corner("FR", fr_points)
    add_corner("FL", fl_points)

    # Wheels
    fr_c = Point3D(geo.track_front / 2, geo.tire_radius, 0)
    fl_c = Point3D(-geo.track_front / 2, geo.tire_radius, 0)
    rr_c = Point3D(geo.track_rear / 2, geo.tire_radius, -geo.wheelbase)
    rl_c = Point3D(-geo.track_rear / 2, geo.tire_radius, -geo.wheelbase)

    transformed["FR Wheel"] = transform_to_rear_left_origin(fr_c, geo)
    transformed["FL Wheel"] = transform_to_rear_left_origin(fl_c, geo)
    transformed["RR Wheel"] = transform_to_rear_left_origin(rr_c, geo)
    transformed["RL Wheel"] = transform_to_rear_left_origin(rl_c, geo)

    if ro:
        transformed["Roll Center"] = transform_to_rear_left_origin(ro, geo)

    return transformed