import math
from dataclasses import dataclass
from typing import Tuple, Optional, Dict

# ==========================================
# DATA STRUCTURES (IMMUTABLE)
# ==========================================

@dataclass(frozen=True)
class Point2D:
    x: float
    y: float

@dataclass(frozen=True)
class Point3D:
    x: float
    y: float
    z: float

@dataclass(frozen=True)
class SuspensionGeometry2D:
    track_width: float
    upper_in: Point2D
    upper_out: Point2D
    lower_in: Point2D
    lower_out: Point2D

@dataclass(frozen=True)
class RollCenterResult:
    ic: Optional[Point2D]  # Instant Center
    h_ro: Optional[float]  # Roll Center Height
    q_factor: Optional[float]

# ==========================================
# PURE HELPER FUNCTIONS
# ==========================================

def to_rad(deg: float) -> float:
    return deg * math.pi / 180.0

def to_deg(rad: float) -> float:
    return rad * 180.0 / math.pi

def calculate_line_coeffs(p1: Point2D, p2: Point2D) -> Tuple[float, float, float]:
    """
    Returns coefficients (a, b, c) for the line equation ax + by + c = 0.
    """
    a = p1.y - p2.y
    b = p2.x - p1.x
    c = p1.x * p2.y - p2.x * p1.y
    return a, b, c

def solve_intersection(l1: Tuple[float, float, float], 
                       l2: Tuple[float, float, float]) -> Optional[Point2D]:
    """
    Calculates the intersection of two lines given their coefficients.
    Returns None if lines are parallel (determinant is zero).
    """
    a1, b1, c1 = l1
    a2, b2, c2 = l2
    det = a1 * b2 - a2 * b1
    
    if abs(det) < 1e-9:
        return None
    
    x = (b1 * c2 - b2 * c1) / det
    y = (c1 * a2 - c2 * a1) / det
    return Point2D(x, y)

def vector_magnitude(p1: Point3D, p2: Point3D) -> float:
    return math.hypot(p2.x - p1.x, p2.y - p1.y, p2.z - p1.z)

# ==========================================
# 2D GEOMETRY LOGIC
# ==========================================

def calculate_roll_center(geo: SuspensionGeometry2D) -> RollCenterResult:
    """
    Calculates Instant Center (IC) and Roll Center height (h_ro) for a Double Wishbone.
    """
    # 1. Get Line Coefficients
    line_upper = calculate_line_coeffs(geo.upper_in, geo.upper_out)
    line_lower = calculate_line_coeffs(geo.lower_in, geo.lower_out)
    
    # 2. Find Instant Center (IC)
    ic = solve_intersection(line_upper, line_lower)
    
    if ic is None:
        return RollCenterResult(None, None, None)
    
    # 3. Calculate Roll Center
    # Wheel center (W) is at (track/2, 0)
    w_x = geo.track_width / 2.0
    w_y = 0.0
    
    # FVSA (Front View Swing Arm) is the line IC -> W
    # We need the intersection of FVSA with the center line (x=0)
    # Slope m = (y_ic - y_w) / (x_ic - x_w)
    
    if abs(ic.x - w_x) < 1e-9:
        h_ro = None
        q = None
    else:
        m = (ic.y - w_y) / (ic.x - w_x)
        # Equation: y - y_w = m(x - x_w). Set x = 0.
        h_ro = m * (0 - w_x) + w_y
        
        # Q factor (Reimpell)
        p = ic.y - w_y
        q = (p * geo.track_width) / (h_ro**2) if abs(h_ro) > 1e-9 else 0.0
        
    return RollCenterResult(ic, h_ro, q)

def calculate_camber_gain(track: float, s1: float, s2: float, 
                          camber_out: float, camber_in: float) -> Dict[str, float]:
    """
    Calculates kinematic camber gain metrics.
    """
    if abs(track) < 1e-9:
        return {}
        
    d_phi_rad = (s1 + s2) / track
    d_phi_deg = to_deg(d_phi_rad)
    
    d_gamma = (camber_out - camber_in) / 2.0
    
    k_gamma = (d_gamma / d_phi_deg) if abs(d_phi_deg) > 1e-9 else 0.0
    
    return {
        "roll_angle_deg": d_phi_deg,
        "camber_change_deg": d_gamma,
        "camber_factor": k_gamma
    }

# ==========================================
# 3D FORCE LOGIC
# ==========================================

def calculate_3d_arm_forces(p_sup_in: Point3D, p_sup_out: Point3D,
                            p_inf_in: Point3D, p_inf_out: Point3D,
                            fx_tire: float, 
                            stiffness_sup: float = 1.0, 
                            stiffness_inf: float = 1.0) -> Optional[Dict]:
    """
    Distributes longitudinal tire force into wishbone axial forces based on stiffness.
    """
    
    # Calculate lengths
    len_sup = vector_magnitude(p_sup_in, p_sup_out)
    len_inf = vector_magnitude(p_inf_in, p_inf_out)
    
    if len_sup < 1e-9 or len_inf < 1e-9:
        return None

    # Z-axis Unit Vectors (Longitudinal component)
    dz_sup = p_sup_out.z - p_sup_in.z
    dz_inf = p_inf_out.z - p_inf_in.z
    
    e_z_sup = dz_sup / len_sup
    e_z_inf = dz_inf / len_inf
    
    # Force Equilibrium in Z: F_sup_z + F_inf_z = Fx_tire
    # F_sup_axial * e_z_sup + F_inf_axial * e_z_inf = Fx_tire
    # Assume F distributed by stiffness: F_axial ~ k * stiffness
    
    denominator = (stiffness_sup * e_z_sup) + (stiffness_inf * e_z_inf)
    
    if abs(denominator) < 1e-9:
        return None
        
    k_force = fx_tire / denominator
    f_axial_sup = k_force * stiffness_sup
    f_axial_inf = k_force * stiffness_inf
    
    def decompose(p_in, p_out, f_axial, length):
        dx = p_out.x - p_in.x
        dy = p_out.y - p_in.y
        dz = p_out.z - p_in.z
        return {
            "Fx": f_axial * (dx / length),
            "Fy": f_axial * (dy / length),
            "Fz": f_axial * (dz / length),
            "Axial": f_axial
        }

    return {
        "upper": decompose(p_sup_in, p_sup_out, f_axial_sup, len_sup),
        "lower": decompose(p_inf_in, p_inf_out, f_axial_inf, len_inf)
    }

# ==========================================
# STABILITY LOGIC
# ==========================================

def calculate_stability(mass: float, ay: float, track: float, h_cg: float, h_ro: float):
    g = 9.81
    
    # Geometric load transfer (due to Roll Center)
    # dFz_geo = m * ay * h_ro / track
    
    # Elastic load transfer (due to body roll)
    # dFz_elas = m * ay * (h_cg - h_ro) / track
    
    d_fz_total = mass * ay * (h_cg / track)
    
    # Static Stability Factor
    ssf = (track / 2.0) / h_cg if h_cg > 0 else 0
    ay_critical = ssf * g
    
    # Roll Moment
    h_roll_arm = h_cg - h_ro
    m_roll = mass * ay * (h_roll_arm / 1000.0) # Convert mm arm to m
    
    return {
        "dFz_total": d_fz_total,
        "roll_moment_Nm": m_roll,
        "ay_critical": ay_critical,
        "h_roll_arm": h_roll_arm
    }