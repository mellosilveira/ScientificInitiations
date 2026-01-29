import math
from typing import Iterator, Optional
from models import (
    SuspensionParameters, SuspensionCGScanParameters, SuspensionResult,
    SuspensionMassScanParameters, MassScanResult, LoadTransferComponents,
    ForceAngleParameters, ForceAngleIteratorParameters, ForceAngleResult,
    EPSILON, GRAVITY
)


# ==========================================
# 1. CG SWEEP LOGIC
# ==========================================

def calculate_baja_sim_row(params: SuspensionParameters) -> SuspensionResult:
    """Calculates a single row for CG sweep."""
    # 1. Roll Moment (Nm)
    # Lever arm = CG Height - Roll Center Height
    roll_arm_m = (params.h - params.h_ro) / 1000.0
    m_roll = params.mass * params.ay * roll_arm_m

    # 2. Load Transfer (N)
    d_fz = params.mass * params.ay * (params.h / params.track)

    # 3. Vertical Loads (N)
    fz_static = (params.mass * GRAVITY) / 4.0
    fz_ext = fz_static + (d_fz / 2.0)
    fz_int = fz_static - (d_fz / 2.0)

    # 4. Spindle Moment (Nm)
    fy_wheel = (params.mass * params.ay) / 2.0
    lever_arm_m = (params.scrub_radius + params.clearance) / 1000.0
    m_sp = fy_wheel * lever_arm_m

    return SuspensionResult(params.h, m_roll, d_fz, fz_ext, fz_int, m_sp)


def calculate_baja_cg_sweep(params: SuspensionCGScanParameters) -> Iterator[SuspensionResult]:
    """Generator: Iterates through CG heights."""
    current_h = params.h_min
    while current_h <= params.h_max + EPSILON:
        # Here we use SuspensionParameters which caused your error
        p = SuspensionParameters(
            h=current_h, mass=params.mass, ay=params.ay, track=params.track,
            h_ro=params.h_ro, scrub_radius=params.scrub_radius, clearance=params.clearance
        )
        yield calculate_baja_sim_row(p)
        current_h += params.h_step


# ==========================================
# 2. MASS SWEEP LOGIC
# ==========================================

def calculate_mass_scan_row(m: float, params: SuspensionMassScanParameters) -> MassScanResult:
    """Calculates a single row for Mass sweep."""
    h_bo = (params.h_cg - params.h_ro) / 1000.0
    m_roll = m * params.ay * h_bo
    d_fz = m * params.ay * (params.h_cg / params.track)

    if params.h_cg > EPSILON:
        ssf = (params.track / 2.0) / params.h_cg
        ay_crit = ssf * GRAVITY
        margin = params.ay / ay_crit if ay_crit > EPSILON else 0.0
    else:
        ssf, ay_crit, margin = 0.0, 0.0, 0.0

    return MassScanResult(m, m_roll, d_fz, ssf, ay_crit, margin)


def calculate_mass_sweep(params: SuspensionMassScanParameters) -> Iterator[MassScanResult]:
    """Generator: Iterates through Mass values."""
    current_m = params.m_min
    while current_m <= params.m_max + EPSILON:
        yield calculate_mass_scan_row(current_m, params)
        current_m += params.m_step


def calculate_load_transfer_components(mass: float, ay: float, track: float, h_cg: float,
                                       h_ro: float) -> LoadTransferComponents:
    """Decomposes Load Transfer into Geometric and Elastic components."""
    if track < EPSILON: return LoadTransferComponents(0, 0, 0)
    d_fz_total = mass * ay * (h_cg / track)
    d_fz_geo = mass * ay * (h_ro / track)
    d_fz_el = mass * ay * ((h_cg - h_ro) / track)
    return LoadTransferComponents(d_fz_total, d_fz_geo, d_fz_el)


# ==========================================
# 3. STRUCTURAL OPTIMIZATION LOGIC
# ==========================================

def calculate_force_vs_angle_step(params: ForceAngleParameters) -> Optional[ForceAngleResult]:
    """Calculates a single step for Force vs Angle."""
    k_sup = params.k_sup
    k_inf = params.k_inf

    # Normalize stiffness K
    if abs(k_sup + k_inf - 1.0) > 1e-5:
        total = k_sup + k_inf
        k_sup /= total
        k_inf /= total

    current_sup = params.angle_sup_base + params.ang
    current_inf = params.angle_inf_base + params.ang

    rad_sup = math.radians(current_sup)
    rad_inf = math.radians(current_inf)

    if abs(math.cos(rad_sup)) < EPSILON or abs(math.cos(rad_inf)) < EPSILON:
        return None

    f_ax_sup = (params.f_load * k_sup) / math.cos(rad_sup)
    f_ax_inf = (params.f_load * k_inf) / math.cos(rad_inf)
    f_total = f_ax_sup + f_ax_inf

    status = "LIMITE" if (abs(f_ax_sup) > params.limit or abs(f_ax_inf) > params.limit) else "OK"

    return ForceAngleResult(params.ang, f_ax_sup, f_ax_inf, f_total, status)


def calculate_force_vs_angle_sweep(params: ForceAngleIteratorParameters) -> Iterator[ForceAngleResult]:
    """Generator: Iterates through angle deltas."""
    for d_ang in range(params.ang_min, params.ang_max + 1, params.step):
        p = ForceAngleParameters(
            f_load=params.f_load, angle_sup_base=params.angle_sup_base,
            angle_inf_base=params.angle_inf_base, k_sup=params.k_sup, k_inf=params.k_inf,
            limit=params.limit, ang=float(d_ang)
        )
        res = calculate_force_vs_angle_step(p)
        if res: yield res