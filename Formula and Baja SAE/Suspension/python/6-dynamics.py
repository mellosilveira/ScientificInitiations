import math
from typing import List
from models import BajaParameters, BajaSimRow, ForceAngleRow

def calculate_baja_sweep(params: BajaParameters) -> List[BajaSimRow]:
    """
    Performs a parameter sweep on CG Height (h_cg) to analyze vehicle stability.
    Returns a list of simulation rows for plotting.
    """
    results = []
    current_h = params.h_min
    g = 9.81
    
    while current_h <= params.h_max + 1e-9:
        # 1. Roll Moment (Nm)
        roll_arm_m = (current_h - params.h_ro) / 1000.0
        m_roll = params.mass * params.ay * roll_arm_m
        
        # 2. Load Transfer (N)
        d_fz = params.mass * params.ay * (current_h / params.track)
        
        # 3. Vertical Loads (N)
        fz_static = (params.mass * g) / 4.0
        fz_ext = fz_static + (d_fz / 2.0)
        fz_int = fz_static - (d_fz / 2.0)
        
        # 4. Spindle Moment (Nm)
        fy_wheel = (params.mass * params.ay) / 2.0
        lever_arm_m = (params.scrub_radius + params.clearance) / 1000.0
        m_sp = fy_wheel * lever_arm_m
        
        results.append(BajaSimRow(current_h, m_roll, d_fz, fz_ext, fz_int, m_sp))
        current_h += params.step
        
    return results

def calculate_force_vs_angle(
    f_load: float, 
    angle_sup_base: float, 
    angle_inf_base: float,
    k_sup: float,
    k_inf: float,
    limit: float,
    ang_min: int,
    ang_max: int,
    step: int
) -> List[ForceAngleRow]:
    """
    Simulates axial forces as the suspension arm angles change relative to the horizontal.
    Useful for checking structural limits when designing the A-arm angle.
    """
    results = []
    
    # Normalize stiffness K if user didn't sum to 1.0
    if abs(k_sup + k_inf - 1.0) > 1e-5:
        total_k = k_sup + k_inf
        k_sup /= total_k
        k_inf /= total_k
        
    for d_ang in range(ang_min, ang_max + 1, step):
        current_sup = angle_sup_base + d_ang
        current_inf = angle_inf_base + d_ang
        
        # Convert to radians
        rad_sup = math.radians(current_sup)
        rad_inf = math.radians(current_inf)
        
        # Avoid division by zero (cos(90) = 0)
        if abs(math.cos(rad_sup)) < 1e-9 or abs(math.cos(rad_inf)) < 1e-9:
            continue
            
        # F_axial = (Load * k) / cos(angle)
        f_ax_sup = (f_load * k_sup) / math.cos(rad_sup)
        f_ax_inf = (f_load * k_inf) / math.cos(rad_inf)
        f_total = f_ax_sup + f_ax_inf
        
        status = "LIMIT" if (abs(f_ax_sup) > limit or abs(f_ax_inf) > limit) else "OK"
        
        results.append(ForceAngleRow(
            angle_delta=float(d_ang),
            force_sup=f_ax_sup,
            force_inf=f_ax_inf,
            force_total=f_total,
            status=status
        ))
        
    return results