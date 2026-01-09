import math
from typing import Optional
from models import (
    Point2D, Point3D, ForceVector, ForceResult3D, 
    SuspensionGeometry3D, AlignmentResult, EPSILON
)
from utils import to_deg
import utils
import math_2d

def vector_len(p1: Point3D, p2: Point3D) -> float:
    """Calculates Euclidean distance between two 3D points."""
    return math.hypot(p2.x - p1.x, p2.y - p1.y, p2.z - p1.z)

def calculate_alignment(geo: SuspensionGeometry3D) -> AlignmentResult:
    """
    Calculates static alignment angles (Camber, Caster, Toe) from 3D points.
    """
    # 1. Projected Instant Center (Front View X-Y)
    sup_in_2d = Point2D(geo.sup_in.x, geo.sup_in.y)
    sup_out_2d = Point2D(geo.sup_out.x, geo.sup_out.y)
    inf_in_2d = Point2D(geo.inf_in.x, geo.inf_in.y)
    inf_out_2d = Point2D(geo.inf_out.x, geo.inf_out.y)
    
    l_sup = math_2d.get_line_coefficients(sup_in_2d, sup_out_2d)
    l_inf = math_2d.get_line_coefficients(inf_in_2d, inf_out_2d)
    ic = math_2d.solve_intersection(l_sup, l_inf)
    
    # 2. Camber (Front Plane X-Y)
    dx = geo.spindle_sup.x - geo.spindle_inf.x
    dy = geo.spindle_sup.y - geo.spindle_inf.y
    camber = to_deg(math.atan(dx/dy)) if abs(dy) > EPSILON else 0.0
    
    # 3. Caster (Side Plane Z-Y)
    dz_cast = geo.spindle_sup.z - geo.spindle_inf.z
    caster = to_deg(math.atan(dz_cast/dy)) if abs(dy) > EPSILON else 0.0
    
    # 4. Toe (Top Plane X-Z)
    dx_toe = geo.toe_front.x - geo.toe_rear.x
    dz_toe = geo.toe_front.z - geo.toe_rear.z
    toe = to_deg(math.atan(dx_toe/dz_toe)) if abs(dz_toe) > EPSILON else 0.0
    
    return AlignmentResult(camber, caster, toe, ic)

def calculate_forces(geo: SuspensionGeometry3D) -> Optional[ForceResult3D]:
    """
    Calculates the Axial Force in suspension arms based on a longitudinal input.
    Uses stiffness ratios to distribute load between upper and lower arms.
    """
    l_sup = vector_len(geo.sup_in, geo.sup_out)
    l_inf = vector_len(geo.inf_in, geo.inf_out)
    
    if l_sup < EPSILON or l_inf < EPSILON:
        return None
        
    # Calculate Z-axis direction cosine
    ez_sup = (geo.sup_out.z - geo.sup_in.z) / l_sup
    ez_inf = (geo.inf_out.z - geo.inf_in.z) / l_inf
    
    # Equilibrium equation: F_sup_z + F_inf_z = Fx_tire
    denom = (geo.stiffness_ratio_sup * ez_sup) + (geo.stiffness_ratio_inf * ez_inf)
    
    if abs(denom) < EPSILON:
        return None # Geometry cannot react to longitudinal force
        
    k_force = geo.fx_tire / denom
    fax_sup = k_force * geo.stiffness_ratio_sup
    fax_inf = k_force * geo.stiffness_ratio_inf
    
    # Helper to decompose vector
    def decompose(p_in, p_out, fax, length):
        dx = p_out.x - p_in.x
        dy = p_out.y - p_in.y
        dz = p_out.z - p_in.z
        return ForceVector(
            fx=fax * (dx/length),
            fy=fax * (dy/length),
            fz=fax * (dz/length),
            axial=fax,
            length=length
        )
        
    vec_sup = decompose(geo.sup_in, geo.sup_out, fax_sup, l_sup)
    vec_inf = decompose(geo.inf_in, geo.inf_out, fax_inf, l_inf)
    
    # Sum of forces
    vec_total = ForceVector(
        fx=vec_sup.fx + vec_inf.fx,
        fy=vec_sup.fy + vec_inf.fy,
        fz=vec_sup.fz + vec_inf.fz,
        axial=0.0, length=0.0
    )
    
    return ForceResult3D(vec_sup, vec_inf, vec_total)

def calculate_anti_dive(fx: float, fy_total: float, h_cg: float, wheelbase: float) -> Optional[float]:
    """
    Calculates Anti-Dive percentage.
    Formula: % = (Fy_suspension / Fz_transfer_needed) * 100
    """
    if abs(wheelbase) < EPSILON: 
        return None
    
    # Theoretical load transfer required for 100% anti-dive
    fz_needed = fx * (h_cg / wheelbase)
    
    if abs(fz_needed) < EPSILON:
        return 0.0
        
    return (fy_total / fz_needed) * 100.0