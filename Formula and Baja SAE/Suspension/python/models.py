from dataclasses import dataclass
from typing import Optional

# ==========================================
# COORDINATE SYSTEM CONVENTION
# ==========================================
# X: Lateral (Positive = Right/Outboard relative to center)
# Y: Vertical (Positive = Up/Ground)
# Z: Longitudinal (Positive = Front)
# Units: Millimeters (mm), Newtons (N), Degrees (deg)

# Constants
EPSILON = 1e-9
GRAVITY = 9.81


# ==========================================
# GEOMETRY PRIMITIVES
# ==========================================

@dataclass(frozen=True)
class Point2D:
    """Represents a point in a 2D plane (e.g., Front View)."""
    x: float
    y: float


@dataclass(frozen=True)
class Point3D:
    """Represents a point in 3D space."""
    x: float
    y: float
    z: float

    def to_list(self):
        return [self.x, self.y, self.z]


@dataclass(frozen=True)
class LineCoefficients:
    """
    Represents the implicit line equation: ax + by + c = 0.
    """
    a: float
    b: float
    c: float


# ==========================================
# SUSPENSION INPUT MODELS
# ==========================================

@dataclass(frozen=True)
class SuspensionGeometry2D:
    """Inputs for 2D Kinematic analysis (Reimpell Method)."""
    track_width: float
    upper_in: Point2D
    upper_out: Point2D
    lower_in: Point2D
    lower_out: Point2D
    s1: float  # Travel outer
    s2: float  # Travel inner
    camber_out_deg: float
    camber_in_deg: float


@dataclass(frozen=True)
class SuspensionGeometry3D:
    """Inputs for 3D Force analysis and Alignment."""
    sup_in: Point3D
    sup_out: Point3D
    inf_in: Point3D
    inf_out: Point3D
    spindle_sup: Point3D
    spindle_inf: Point3D
    toe_front: Point3D
    toe_rear: Point3D
    stiffness_ratio_sup: float
    stiffness_ratio_inf: float
    fx_tire: float


@dataclass(frozen=True)
class VehicleGeometry:
    """
    Full vehicle geometry for 3D plotting and global calculations.
    """
    wheelbase: float
    track_front: float
    track_rear: float
    sup_in: Point3D
    sup_out: Point3D
    inf_in: Point3D
    inf_out: Point3D
    tie_rod_in: Point3D
    tie_rod_out: Point3D
    tire_radius: float


# --- DYNAMICS PARAMETERS ---

@dataclass(frozen=True)
class SuspensionParameters:
    """Parameters for a single iteration of CG simulation."""
    h: float
    mass: float
    ay: float
    track: float
    h_ro: float  # Height of Roll Center
    scrub_radius: float
    clearance: float


@dataclass(frozen=True)
class SuspensionCGScanParameters:
    """Parameters for the CG Height sweep simulation."""
    h_min: float
    h_max: float
    h_step: float
    mass: float
    ay: float
    track: float
    h_ro: float
    scrub_radius: float
    clearance: float


@dataclass(frozen=True)
class SuspensionMassScanParameters:
    """Parameters for the Mass sweep simulation."""
    m_min: float
    m_max: float
    m_step: float
    h_cg: float
    ay: float
    track: float
    h_ro: float


# --- OPTIMIZATION PARAMETERS ---

@dataclass(frozen=True)
class ForceAngleParameters:
    """Parameters for a single iteration of Force vs Angle."""
    f_load: float
    angle_sup_base: float
    angle_inf_base: float
    k_sup: float
    k_inf: float
    limit: float
    ang: float


@dataclass(frozen=True)
class ForceAngleIteratorParameters:
    """Parameters for the sweep optimization Force vs Angle."""
    f_load: float
    angle_sup_base: float
    angle_inf_base: float
    k_sup: float
    k_inf: float
    limit: float
    ang_min: int
    ang_max: int
    step: int


# ==========================================
# CALCULATION RESULTS
# ==========================================

@dataclass(frozen=True)
class RollCenterResult:
    """Output of the Reimpell 2D geometric analysis."""
    ic: Optional[Point2D]  # Instant Center of Rotation
    ro: Optional[Point3D]  # Roll Center (3D Point)
    h_ro: Optional[float]  # Height scalar (for backward compatibility if needed)
    q_factor: Optional[float]  # Reimpell's curvature factor


@dataclass(frozen=True)
class CamberGainResult:
    d_phi_deg: float
    d_gamma_k: float
    k_gamma: Optional[float]


@dataclass(frozen=True)
class ForceVector:
    fx: float
    fy: float
    fz: float
    axial: float
    length: float


@dataclass(frozen=True)
class ForceResult3D:
    upper: ForceVector
    lower: ForceVector
    total: ForceVector


@dataclass(frozen=True)
class AlignmentResult:
    camber: Optional[float]
    caster: Optional[float]
    toe: Optional[float]
    ic_frontal: Optional[Point2D]


@dataclass(frozen=True)
class SteeringResult:
    ackermann_percentage: float
    turning_radius: float
    inner_angle: float
    outer_angle: float


@dataclass(frozen=True)
class KingpinResult:
    kpi_deg: float
    caster_deg: float
    scrub_radius: float
    mechanical_trail: float


@dataclass(frozen=True)
class SuspensionResult:
    """Data row for CG simulation."""
    h_cg: float
    m_roll: float
    d_fz: float
    fz_ext: float
    fz_int: float
    m_sp: float


@dataclass(frozen=True)
class MassScanResult:
    """Data row for Mass simulation."""
    mass: float
    m_roll: float
    d_fz: float
    ssf: float
    ay_crit: float
    margin: Optional[float]


@dataclass(frozen=True)
class LoadTransferComponents:
    """Breakdown of load transfer."""
    d_fz_total: float
    d_fz_geo: float
    d_fz_el: float


@dataclass(frozen=True)
class ForceAngleResult:
    angle_delta: float
    force_sup: float
    force_inf: float
    force_total: float
    status: str