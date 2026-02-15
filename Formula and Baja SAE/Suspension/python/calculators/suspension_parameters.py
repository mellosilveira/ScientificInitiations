import math
from typing import Tuple
from ..models.primitives import Point3D, Vector3D
from ..models.lines import LineCoefficients3D
from ..models.suspension import Suspension
from ..models.numerical import SolverParameters
from ..models.results import RollCenterResult, TireAnglesResult, KingpingResult, LongitudinalResult, AlignmentMetricsResult, SteeringMetricsResult
from ..models.constants import EPSILON
from .math import cot, acot
from trigonometry import rodrigues_rotation
from numerical_solvers import find_root

def calculate_lateral_instantaneous_center(geo: Suspension) -> tuple[Point3D, Vector3D, float]:
    reference_point = geo.tire_contact
    if geo.brake_on_shaft:
        # TODO: APLICAR RAIO EFETIVO DO PNEU (DEFORMAÇÃO NO EIXO Y)
        reference_point += Point3D(0, geo.tire_diameter / 2, 0)
    
    lateral_instantaneous_center = geo.upper_arm.outer_line.intersect(geo.lower_arm.outer_line)
    vector = Vector3D.from_points(lateral_instantaneous_center, reference_point)
    tan_angle = vector.y / vector.z
    return [lateral_instantaneous_center, vector, tan_angle]

# TODO: APLICAR RAIO EFETIVO DO PNEU (DEFORMAÇÃO NO EIXO Y) PARA CAMBER
def calculate_tire_angles(geo: Suspension) -> TireAnglesResult:
    return TireAnglesResult(
        # 1. Camber (Plano Frontal XY)
        camber = math.atan(geo.camber_gap / geo.tire_diameter), 
        # 2. Toe (Convergência - Plano XZ)
        toe = math.atan2(geo.toe_distance, geo.tire_diameter)
    )

# TODO: APLICAR RAIO EFETIVO DO PNEU (DEFORMAÇÃO NO EIXO Y)
def calculate_kingping_parameters(geo: Suspension) -> KingpingResult:
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
def calculate_roll_center_parameters(geo: Suspension, vehicle_center_line: LineCoefficients3D, track_width: float) -> RollCenterResult:
    # 1. Define lines for Upper and Lower arms
    line_sup = LineCoefficients3D.from_points(geo.upper_arm.centroid_inner, geo.upper_arm.outer)
    line_inf = LineCoefficients3D.from_points(geo.lower_arm.centroid_inner, geo.lower_arm.outer)
    
    # 2. Find Instant Center (IC)
    front_instantaneous_center = line_sup.intersect(line_inf)
    if front_instantaneous_center is None:
        return RollCenterResult(None, None, None)
    
    # 3. Find Roll Center (RC)
    line_ic_tr = LineCoefficients3D.from_points(geo.tire_contact, front_instantaneous_center)
    roll_center = line_ic_tr.intersect(vehicle_center_line)
    if roll_center is None:
        return RollCenterResult(0, 0, None)
    
    h_rc = roll_center.y
    q_factor = ((front_instantaneous_center.y - geo.tire_contact.y) * track_width) / (h_rc ** 2) if abs(h_rc) > EPSILON else 0.0
    return RollCenterResult(front_instantaneous_center, roll_center, q_factor)

def calculate_anti_dive_parameters(geo: Suspension, gravity_center: Point3D, wheelbase: float, brake_percentage: float) -> LongitudinalResult:
    lateral_instantaneous_center, ic_vector, tan_angle = calculate_lateral_instantaneous_center(geo)
    
    # IMPORTANTE: aqui estamos chamando de anti_dive para simplificar a implementação, mas quando o cálculo estiver sendo feito para a traseira,
    # na verdade, o nome correto é anti_lift, sendo assim, quando ocorre frenagem, os nomes corretos são:
    # - dianteira: anti dive
    # - traseira: anti lift
    anti_dive = (brake_percentage * tan_angle / (gravity_center.y / wheelbase)) * 100
    return LongitudinalResult(
        percentage = anti_dive, 
        instantaneous_center = lateral_instantaneous_center,
        vector = ic_vector, 
        angle = math.atan(tan_angle)
    )

def calculate_anti_squat_parameters(geo: Suspension, gravity_center: Point3D, wheelbase: float) -> LongitudinalResult:
    lateral_instantaneous_center, ic_vector, tan_angle = calculate_lateral_instantaneous_center(geo)
    
    # IMPORTANTE: aqui estamos chamando de anti_squat para simplificar a implementação, mas quando o cálculo estiver sendo feito para a dianteira,
    # na verdade, o nome correto é anti_lift, sendo assim, quando ocorre aceleração, os nomes corretos são:
    # - dianteira: anti lift
    # - traseira: anti squat
    anti_squat = (tan_angle / (gravity_center.y / wheelbase)) * 100
    return LongitudinalResult(
        percentage = anti_squat, 
        instantaneous_center = lateral_instantaneous_center,
        vector = ic_vector, 
        angle = math.atan(tan_angle)
    )

def calculate_alignment_metrics(
    geo: Suspension,
    vehicle_center_line: LineCoefficients3D, 
    gravity_center: Point3D, 
    wheelbase: float, 
    track_width: float, 
    brake_percentage: float
) -> AlignmentMetricsResult:
    """
    Calcula ângulos de alinhamento estático usando geometria vetorial 3D.
    Substitui projeções manuais por operações com Vector3D.
    """
    return AlignmentMetricsResult(
        tire_angles = calculate_tire_angles(geo),
        kingpin_parameters = calculate_kingping_parameters(geo),
        roll_center_prameters = calculate_roll_center_parameters(geo, vehicle_center_line, track_width),
        anti_dive_parameters = calculate_anti_dive_parameters(geo, gravity_center, wheelbase, brake_percentage),
        anti_squat_parameters = calculate_anti_squat_parameters(geo, gravity_center, wheelbase)
    )

def calculate_steering_metrics(wheelbase: float, kingpin_track: float, inner_angle: float) -> SteeringMetricsResult:
    # Validações de segurança para evitar divisão por zero
    if abs(wheelbase) < EPSILON or abs(inner_angle) < EPSILON:
        return SteeringMetricsResult(ackermann_percentage = 0.0, inner_angle = 0.0, ideal_outer_angle = 0.0, turning_radius = 0.0)

    cot_in = cot(inner_angle)
    ideal_outer_angle = acot(cot_in + (kingpin_track / wheelbase))
    turning_radius = wheelbase / math.sin(inner_angle)

    # 6. Retornar os Resultados
    # Como este é o cálculo "Ideal", a porcentagem teórica é 100%.
    return SteeringMetricsResult(
        ackermann_percentage = 100.0,
        turning_radius = turning_radius,
        inner_angle = inner_angle,
        outer_angle = 0.0, #TODO: TENTAR CALCULAR
        ideal_outer_angle = ideal_outer_angle
    )


# def transform_to_rear_left_origin(point: Point3D, geo: VehicleGeometry) -> Point3D:
#     """
#     Transforms point to Global Frame (Rear Left Wheel = 0,0,0).
#     Local Z=0 is Front Axle.
#     """
#     trans_x = geo.track_rear / 2.0
#     trans_y = 0.0
#     trans_z = geo.wheelbase
#     return Point3D(point.x + trans_x, point.y, point.z + trans_z)


# def generate_full_car_plot_points(geo: VehicleGeometry, ro: Optional[Point3D]):
#     """Generates transformed points for 3D plotting."""
#     fr_points = {
#         "Sup In": geo.sup_in, "Sup Out": geo.sup_out,
#         "Inf In": geo.inf_in, "Inf Out": geo.inf_out,
#         "Tie In": geo.tie_rod_in, "Tie Out": geo.tie_rod_out
#     }

#     fl_points = {k: Point3D(-p.x, p.y, p.z) for k, p in fr_points.items()}

#     transformed = {}

#     def add_corner(prefix, p_dict):
#         for n, p in p_dict.items():
#             transformed[f"{prefix} {n}"] = transform_to_rear_left_origin(p, geo)

#     add_corner("FR", fr_points)
#     add_corner("FL", fl_points)

#     # Wheels
#     fr_c = Point3D(geo.track_front / 2, geo.tire_radius, 0)
#     fl_c = Point3D(-geo.track_front / 2, geo.tire_radius, 0)
#     rr_c = Point3D(geo.track_rear / 2, geo.tire_radius, -geo.wheelbase)
#     rl_c = Point3D(-geo.track_rear / 2, geo.tire_radius, -geo.wheelbase)

#     transformed["FR Wheel"] = transform_to_rear_left_origin(fr_c, geo)
#     transformed["FL Wheel"] = transform_to_rear_left_origin(fl_c, geo)
#     transformed["RR Wheel"] = transform_to_rear_left_origin(rr_c, geo)
#     transformed["RL Wheel"] = transform_to_rear_left_origin(rl_c, geo)

#     if ro:
#         transformed["Roll Center"] = transform_to_rear_left_origin(ro, geo)

#     return transformed