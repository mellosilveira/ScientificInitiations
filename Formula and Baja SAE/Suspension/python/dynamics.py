import math
from typing import Iterator, Optional
from models import (
    SuspensionParameters, SuspensionIteratorParameters, SuspensionResult,
    ForceAngleParameters, ForceAngleIteratorParameters, ForceAngleResult, 
    EPSILON, GRAVITY 
)

def calculate_baja_sim_row(params: SuspensionParameters) -> SuspensionResult:
    """
    Performs a parameter sweep on CG Height (h_cg).
    Uses a Generator (yield) to return rows lazily, similar to C# yield return.
    """
    
    # 1. Roll Moment (Nm)
    roll_arm_m = (params.h - params.h_ro) / 1000.0
    m_roll = params.mass * params.ay * roll_arm_m
    
    # 2. Load Transfer (N)
    d_fz = params.mass * params.ay * (params.h / params.track)
    
    # 3. Vertical Loads (N)
    fz_static = (params.mass * GRAVITY ) / 4.0
    fz_ext = fz_static + (d_fz / 2.0)
    fz_int = fz_static - (d_fz / 2.0)
    
    # 4. Spindle Moment (Nm)
    fy_wheel = (params.mass * params.ay) / 2.0
    lever_arm_m = (params.scrub_radius + params.clearance) / 1000.0
    m_sp = fy_wheel * lever_arm_m
    
    return SuspensionResult(params.h, m_roll, d_fz, fz_ext, fz_int, m_sp)

def calculate_baja_sweep(params: SuspensionIteratorParameters) -> Iterator[SuspensionResult]:
    """
    Performs a parameter sweep on CG Height (h_cg).
    Uses a Generator (yield) to return rows lazily, similar to C# yield return.
    """
    current_h = params.h_min
    
    # Use EPSILON for float comparison in loop condition
    while current_h <= params.h_max + EPSILON:
        iteration_params = SuspensionParameters(
            h=current_h,
            mass=params.mass,
            ay=params.ay,
            track=params.track,
            h_ro=params.h_ro,
            scrub_radius=params.scrub_radius,
            clearance=params.clearance)
        # Yield return equivalent
        yield calculate_baja_sim_row(iteration_params)
        
        current_h += params.step

def calculate_force_vs_angle(params: ForceAngleParameters) -> Optional[ForceAngleResult]:
    """
    Simulates axial forces as the suspension arm angles change relative to the horizontal.
    Useful for checking structural limits when designing the A-arm angle.
    """
    # Normalize stiffness K if user didn't sum to 1.0
    if abs(k_sup + k_inf - 1.0) > 1e-5:
        total_k = k_sup + k_inf
        k_sup /= total_k
        k_inf /= total_k
        
        current_sup = params.angle_sup_base + params.ang
        current_inf = params.angle_inf_base + params.ang
        
        # Convert to radians
        rad_sup = math.radians(current_sup)
        rad_inf = math.radians(current_inf)
        
        cos_rad_sup = math.cos(rad_sup)
        cos_rad_inf = math.cos(rad_inf)

        # Avoid division by zero (cos(90) = 0)
        if abs(cos_rad_sup) < EPSILON or abs(cos_rad_inf) < EPSILON:
            return None
            
        # F_axial = (Load * k) / cos(angle)
        f_ax_sup = (params.f_load * k_sup) / cos_rad_sup
        f_ax_inf = (params.f_load * k_inf) / cos_rad_inf
        f_total = f_ax_sup + f_ax_inf
        
        status = "LIMIT" if (abs(f_ax_sup) > params.limit or abs(f_ax_inf) > params.limit) else "OK"
        
        return ForceAngleResult(
            angle_delta=params.ang,
            force_sup=f_ax_sup,
            force_inf=f_ax_inf,
            force_total=f_total,
            status=status
        )

def calculate_force_vs_angle_sweep(params: ForceAngleIteratorParameters) -> Iterator[ForceAngleResult]:
    """
    Simulates axial forces as the suspension arm angles change relative to the horizontal.
    Useful for checking structural limits when designing the A-arm angle.
    """
    # Normalize stiffness K if user didn't sum to 1.0
    if abs(k_sup + k_inf - 1.0) > 1e-5:
        total_k = k_sup + k_inf
        k_sup /= total_k
        k_inf /= total_k
        
    for d_ang in range(params.ang_min, params.ang_max + 1, params.step):
        iteration_params = ForceAngleParameters(
            f_load=params.f_load,
            angle_sup_base=params.angle_sup_base,
            angle_inf_base=params.angle_inf_base,
            k_sup=k_sup,
            k_inf=k_inf,
            limit=params.limit,
            ang=d_ang)
        
        result = calculate_force_vs_angle(iteration_params)
        if result is not None:
            yield result