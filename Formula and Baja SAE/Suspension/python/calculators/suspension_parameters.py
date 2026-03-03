import math
from typing import Tuple
from ..models.primitives import Point3D, Vector3D
from ..models.lines import LineCoefficients3D
from ..models.suspension import Suspension
from ..models.numerical import SolverParameters
from ..models.results import RollCenterResult, TireAnglesResult, KingpingResult, LongitudinalResult, AlignmentMetricsResult, SteeringMetricsResult
from ..models.constants import EPSILON
from numerical_solvers import find_root
from trigonometry import rodrigues_rotation, cot, acot

def get_effective_radius(geo: Suspension) -> float:
    """Calcula o raio considerando a deformação estática do pneu."""
    deformation = getattr(geo, 'static_tire_deformation', 0.0)
    return max(1.0, (geo.tire_diameter / 2.0) - deformation)

def get_effective_diameter(geo: Suspension) -> float:
    return get_effective_radius(geo) * 2.0

def calculate_lateral_instantaneous_center(geo: Suspension) -> tuple[Point3D, Vector3D, float]:
    reference_point = geo.tire_contact
    if geo.brake_on_shaft:
        reference_point += Point3D(0, get_effective_radius(geo), 0)
    
    lateral_instantaneous_center = geo.upper_arm.outer_line.intersect(geo.lower_arm.outer_line)
    vector = Vector3D.from_points(lateral_instantaneous_center, reference_point)
    tan_angle = vector.y / vector.z
    return [lateral_instantaneous_center, vector, tan_angle]

def calculate_camber_angle(geo: Suspension) -> float:
    return math.atan(geo.camber_gap / get_effective_diameter(geo))

def calculate_tire_angles(geo: Suspension) -> TireAnglesResult:
    return TireAnglesResult(
        camber = calculate_camber_angle(geo), 
        toe = math.atan2(geo.toe_distance, geo.tire_diameter)
    )

def calculate_kingping_parameters(geo: Suspension) -> KingpingResult:
    if geo.kingpin_vector.magnitude < EPSILON:
        return KingpingResult(
            axis = LineCoefficients3D(origin = geo.upper_arm.outer, direction = Vector3D(0, 1, 0)),
            inclination = 0.0,
            scrub_radius = 0.0,
            mechanical_trail = 0.0,
            caster = 0.0)

    axis = LineCoefficients3D(origin = geo.upper_arm.outer, direction = geo.kingpin_vector.normalize())
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
    # TODO: APLICAR CORREÇÃO DE DEFORMAÇÃO ESTÁTICA DO NO PONTO DE CONTATO PARA MAIOR REALISMO
    t_ground = -axis.origin.y / axis.direction.y
    ground_point = axis.origin + (axis.direction * t_ground)
    
    return KingpingResult(
        axis,
        inclination = math.atan2(geo.kingpin_vector.x, -geo.kingpin_vector.y),
        scrub_radius = geo.tire_contact.x - ground_point.x,
        mechanical_trail = ground_point.z - geo.tire_contact.z,
        caster = math.atan2(geo.kingpin_vector.z, -geo.kingpin_vector.y)
    )

def calculate_roll_center_parameters(geo: Suspension, vehicle_center_line: LineCoefficients3D, track_width: float) -> RollCenterResult:
    line_sup = LineCoefficients3D.from_points(geo.upper_arm.centroid_inner, geo.upper_arm.outer)
    line_inf = LineCoefficients3D.from_points(geo.lower_arm.centroid_inner, geo.lower_arm.outer)
    
    front_instantaneous_center = line_sup.intersect(line_inf)
    if front_instantaneous_center is None:
        return RollCenterResult(None, None, None)
    
    line_ic_tr = LineCoefficients3D.from_points(geo.tire_contact, front_instantaneous_center)
    roll_center = line_ic_tr.intersect(vehicle_center_line)
    
    if roll_center is None:
        return RollCenterResult(0, 0, None)
    
    h_rc = roll_center.y
    q_factor = ((front_instantaneous_center.y - geo.tire_contact.y) * track_width) / (h_rc ** 2) if abs(h_rc) > EPSILON else 0.0
    return RollCenterResult(front_instantaneous_center, roll_center, q_factor)

def calculate_anti_dive_parameters(geo: Suspension, gravity_center: Point3D, wheelbase: float, brake_percentage: float) -> LongitudinalResult:
    lateral_instantaneous_center, ic_vector, tan_angle = calculate_lateral_instantaneous_center(geo)
    anti_dive = (brake_percentage * tan_angle / (gravity_center.y / wheelbase)) * 100
    return LongitudinalResult(
        percentage = anti_dive, 
        instantaneous_center = lateral_instantaneous_center,
        vector = ic_vector, 
        angle = math.atan(tan_angle)
    )

def calculate_anti_squat_parameters(geo: Suspension, gravity_center: Point3D, wheelbase: float) -> LongitudinalResult:
    lateral_instantaneous_center, ic_vector, tan_angle = calculate_lateral_instantaneous_center(geo)
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
    return AlignmentMetricsResult(
        tire_angles = calculate_tire_angles(geo),
        kingpin_parameters = calculate_kingping_parameters(geo),
        roll_center_prameters = calculate_roll_center_parameters(geo, vehicle_center_line, track_width),
        anti_dive_parameters = calculate_anti_dive_parameters(geo, gravity_center, wheelbase, brake_percentage),
        anti_squat_parameters = calculate_anti_squat_parameters(geo, gravity_center, wheelbase)
    )

def calculate_steering_metrics(geo: Suspension, wheelbase: float, inner_angle: float) -> SteeringMetricsResult:
    """
    APLICADO: Solução Analítica Exata para o Mecanismo de Ackermann sem iterações.
    Utiliza lei dos cossenos projetada no plano XY para extrair o ângulo real da roda externa.
    """
    kp_inner_x = geo.lower_arm.outer.x
    tie_out_x = geo.tie_rod.outer.x
    tie_in_x = geo.tie_rod.inner.x
    
    kingpin_track = abs(kp_inner_x * 2.0)
    
    steering_arm_vec = Vector3D.from_points(geo.lower_arm.outer, geo.tie_rod.outer)
    R = math.sqrt(steering_arm_vec.x**2 + steering_arm_vec.z**2) 
    L = math.sqrt((tie_out_x - tie_in_x)**2 + (geo.tie_rod.outer.z - geo.tie_rod.inner.z)**2)
    
    if abs(wheelbase) < EPSILON or abs(inner_angle) < EPSILON or R < EPSILON:
        return SteeringMetricsResult(ackermann_percentage = 0.0, inner_angle = 0.0, ideal_outer_angle = 0.0, turning_radius = 0.0)

    cot_in = cot(inner_angle)
    ideal_outer_angle = acot(cot_in + (kingpin_track / wheelbase))
    turning_radius = wheelbase / math.sin(inner_angle)

    alpha_0 = math.asin(abs(geo.tie_rod.outer.z - geo.lower_arm.outer.z) / R)
    theta_i_arm = alpha_0 + inner_angle
    
    new_tie_out_inner_x = kp_inner_x - R * math.cos(theta_i_arm)
    delta_rack = new_tie_out_inner_x - tie_in_x
    new_tie_in_outer_x = -tie_in_x + delta_rack
    
    D = math.sqrt((new_tie_in_outer_x - (-kp_inner_x))**2 + (geo.tie_rod.inner.z - geo.lower_arm.outer.z)**2)
    
    if D > (R + L) or D < abs(R - L):
        outer_angle = 0.0 
    else:
        gamma = math.acos((R**2 + D**2 - L**2) / (2 * R * D))
        phi = math.atan2(geo.tie_rod.inner.z - geo.lower_arm.outer.z, abs(new_tie_in_outer_x - (-kp_inner_x)))
        theta_o_arm = gamma + phi
        outer_angle = theta_o_arm - alpha_0

    percentage = (outer_angle / ideal_outer_angle) * 100.0 if ideal_outer_angle > EPSILON else 0.0

    return SteeringMetricsResult(
        ackermann_percentage = percentage,
        turning_radius = turning_radius,
        inner_angle = inner_angle,
        outer_angle = outer_angle,
        ideal_outer_angle = ideal_outer_angle
    )

def calculate_camber_gain(
    geo: Suspension, 
    bump_travel_y: float, 
    solver_params: SolverParameters
) -> Tuple[float, float, float]:
    """
    Simulates vertical suspension travel solving the spatial 4-bar linkage.
    Returns: (Static_Camber, New_Camber, Camber_Gain_deg_per_mm)
    """
    # Eixos de rotação no chassi
    l_axis_orig = geo.lower_arm.fore_inner
    l_axis_dir = geo.lower_arm.inner_vector.normalize()
    
    u_axis_orig = geo.upper_arm.fore_inner
    u_axis_dir = geo.upper_arm.inner_vector.normalize()
    
    # Comprimento rígido do Kingpin
    lbj_0 = geo.lower_arm.outer
    ubj_0 = geo.upper_arm.outer
    original_kingpin_length = geo.kingpin_vector.magnitude
    
    static_camber = calculate_camber_angle(geo)
    
    if abs(bump_travel_y) < EPSILON:
        return static_camber, static_camber, 0.0

    # SOLVER 1: Rotate Lower Arm to target Y
    def lower_arm_error(theta_low_rad: float) -> float:
        new_lbj = rodrigues_rotation(lbj_0, l_axis_orig, l_axis_dir, theta_low_rad)
        current_travel = new_lbj.y - lbj_0.y
        return current_travel - bump_travel_y
    
    theta_low_solution = find_root(lower_arm_error, solver_params)
    new_lbj = rodrigues_rotation(lbj_0, l_axis_orig, l_axis_dir, theta_low_solution)

    # SOLVER 2: Rotate Upper Arm to match Kingpin length
    def upper_arm_error(theta_up_rad: float) -> float:
        new_ubj = rodrigues_rotation(ubj_0, u_axis_orig, u_axis_dir, theta_up_rad)
        current_kp_length = Vector3D.from_points(new_ubj, new_lbj).magnitude
        return current_kp_length - original_kingpin_length

    theta_up_solution = find_root(upper_arm_error, solver_params)
    new_ubj = rodrigues_rotation(ubj_0, u_axis_orig, u_axis_dir, theta_up_solution)

    # Calculate new camber
    new_kp_vec = new_lbj - new_ubj
    vec_front = Vector3D(new_kp_vec.x, new_kp_vec.y, 0).normalize()
    new_camber = math.degrees(math.atan2(vec_front.x, -vec_front.y))
    
    camber_gain = (new_camber - static_camber) / bump_travel_y

    return static_camber, new_camber, camber_gain

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