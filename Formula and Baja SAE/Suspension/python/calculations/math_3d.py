import math
from typing import Optional
from ..models.primitives import Point3D, Vector3D
from ..models.lines import LineCoefficients3D
from ..models.suspension import Suspension3D
from ..models.results import RollCenterResult3D, AlignmentMetricsResult3D, TireAnglesResult, KingpingResult
from ..models.constants import EPSILON
from utils import to_deg, to_rad
import math_2d

# =========================================================
# 3D CALCULATIONS (Alignment, Anti-dive)
# =========================================================

# TODO: APLICAR RAIO EFETIVO DO PNEU (DEFORMAÇÃO NO EIXO Y) PARA CAMBER
def calculate_tire_angles(geo: Suspension3D) -> TireAnglesResult:
    return TireAnglesResult(
        # 1. Camber (Plano Frontal XY)
        camber = math.atan(geo.camber_gap / geo.tire_diameter), 
        # 2. Toe (Convergência - Plano XZ)
        toe = math.atan2(geo.toe_distance, geo.tire_diameter)
    )

# TODO: APLICAR RAIO EFETIVO DO PNEU (DEFORMAÇÃO NO EIXO Y)
def calculate_kingping_parameters(geo: Suspension3D) -> KingpingResult:
    # Definir o Vetor Kingpin (Do Superior para o Inferior)
    # Direção: Para baixo (Gravity vector reference)
    kp_vec = Vector3D.from_points(geo.upper_arm.outer, geo.lower_arm.outer)
    
    # Validação de segurança
    if kp_vec.magnitude < EPSILON:
        return KingpingResult(
            axis = LineCoefficients3D(origin = geo.upper_arm.outer, direction = Vector3D(0, 1, 0)),
            inclination = 0.0,
            scrub_radius = 0.0,
            mechanical_trail = 0.0,
            caster = 0.0)

    # Criar o objeto de eixo para visualização/exportação
    axis = LineCoefficients3D(origin = geo.upper_arm.outer, direction = kp_vec.normalize())
    if abs(axis.direction.y) < EPSILON:
        return KingpingResult(
            axis,
            inclination = 0.0,
            scrub_radius = 0.0,
            mechanical_trail = 0.0,
            caster = 0.0)

    # Scrub Radius e Mechanical Trail: 
    # Interseção da Linha do Kingpin com o Plano do Chão (Y = 0)
    # Equação da reta: P(t) = P0 + t * V
    # Queremos Y=0:  0 = P0.y + t * V.y  =>  t = -P0.y / V.y
    t_ground = -axis.origin.y / axis.direction.y
    ground_point = axis.origin + (axis.direction * t_ground)
    
    return KingpingResult(
        axis,
        inclination = math.atan2(kp_vec.x, -kp_vec.y),
        # Scrub: Distância lateral do centro do pneu até a interseção
        # (Positivo se a interseção for para dentro do centro do pneu)
        scrub_radius = geo.tire_contact.x - ground_point.x,
        # Mechanical Trail: Distância longitudinal
        mechanical_trail = ground_point.z - geo.tire_contact.z,
        # 5. Caster (Plano Lateral Y-Z)
        # Projeta o vetor KPI no plano X=0
        # Convenção: Topo inclinado para trás (+Z) é Caster Positivo.
        # atan2(z, -y): Z positivo e Y negativo gera ângulo positivo
        caster = math.atan2(kp_vec.z, -kp_vec.y)
    )

# TODO: APLICAR RAIO EFETIVO DO PNEU (DEFORMAÇÃO NO EIXO Y)
def calculate_roll_center_parameters(geo: Suspension3D, vehicle_center_line: Optional[LineCoefficients3D] = None) -> RollCenterResult3D:
    # 1. Define lines for Upper and Lower arms
    line_sup = LineCoefficients3D.from_points(geo.upper_arm.centroid_inner, geo.upper_arm.outer)
    line_inf = LineCoefficients3D.from_points(geo.lower_arm.centroid_inner, geo.lower_arm.outer)
    
    # 2. Find Instant Center (IC)
    ic = line_sup.intersect(line_inf)
    if ic is None:
        return RollCenterResult3D(None, None, None)
    
    # 3. Find Roll Center (RC)
    line_ic_tr = LineCoefficients3D.from_points(geo.tire_contact, ic)
    line_center = vehicle_center_line if vehicle_center_line else LineCoefficients3D(origin = Point3D(0, 0, 0), direction = Vector3D(1, 0, 0))
    roll_center = line_ic_tr.intersect(line_center)
    if roll_center is None:
        return RollCenterResult3D(0, 0, None)
    
    h_ro = roll_center.y
    p = ic.y - geo.tire_contact.y
    q = (p * geo.track_width) / (h_ro ** 2) if abs(h_ro) > EPSILON else 0.0
    return RollCenterResult3D(ic, roll_center, q)

def calculate_alignment_metrics(geo: Suspension3D, vehicle_center_line: Optional[LineCoefficients3D] = None) -> AlignmentMetricsResult3D:
    """
    Calcula ângulos de alinhamento estático usando geometria vetorial 3D.
    Substitui projeções manuais por operações com Vector3D.
    """
    return AlignmentMetricsResult3D(
        kingpin_parameters = calculate_kingping_parameters(geo),
        tire_angles = calculate_tire_angles(geo),
        roll_center_prameters = calculate_roll_center_parameters(geo, vehicle_center_line)
    )

def calculate_anti_dive_parameters(geo: Suspension3D, gravity_center: Point3D, wheelbase: float, brake_percentage: float) -> Optional[float]:
    reference_point = geo.tire_contact
    if geo.brake_torque_on_motor_shaft:
        reference_point += Point3D(0, geo.tire_diameter / 2, 0)
    
    instantaneous_center = geo.upper_arm.outer_line.intersect(geo.lower_arm.outer_line)
    vector = Vector3D.from_points(instantaneous_center, geo.tire_contact)
    
    anti_dive = 
    
    if abs(wheelbase) < EPSILON:
        return None
    
    fz_needed = fx * (h_cg / wheelbase)
    return (fy_total / fz_needed) * 100.0 if abs(fz_needed) > EPSILON else 0.0

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