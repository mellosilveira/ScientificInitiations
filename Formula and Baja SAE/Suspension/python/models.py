from dataclasses import dataclass
from typing import Optional

# ==========================================
# COORDINATE SYSTEM CONVENTION
# ==========================================
# X: Lateral (Positive = Right/Outboard relative to center)
# Y: Vertical (Positive = Up)
# Z: Longitudinal (Positive = Front)
# Units: Millimeters (mm), Newtons (N), Degrees (deg)

# Constant for floating point comparisons to avoid precision errors
EPSILON = 1e-9

# Constant for gravity acceleration (m/s²)
g = 9.81

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

@dataclass(frozen=True)
class LineCoefficients:
    """
    Represents the implicit line equation: ax + by + c = 0.
    Used for determining intersections using linear algebra (determinants).
    """
    a: float
    b: float
    c: float

# ==========================================
# SUSPENSION INPUT MODELS
# ==========================================

@dataclass(frozen=True)
class SuspensionGeometry2D:
    """
    Inputs for 2D Kinematic analysis (Reimpell Method).
    Includes hardpoints and kinematic targets.
    """
    track_width: float      # Distance between tire contact patches centers
    upper_in: Point2D       # Upper Control Arm (UCA) chassis mount
    upper_out: Point2D      # UCA upright ball joint
    lower_in: Point2D       # Lower Control Arm (LCA) chassis mount
    lower_out: Point2D      # LCA upright ball joint
    
    # Kinematic inputs for Camber Gain calculation
    s1: float               # Suspension travel (bump/droop) of outer wheel
    s2: float               # Suspension travel of inner wheel
    camber_out_deg: float   # Static camber angle of outer wheel
    camber_in_deg: float    # Static camber angle of inner wheel

@dataclass(frozen=True)
class SuspensionGeometry3D:
    """
    Inputs for 3D Force analysis and Alignment.
    """
    # Suspension Hardpoints
    sup_in: Point3D
    sup_out: Point3D
    inf_in: Point3D
    inf_out: Point3D
    
    # Alignment Points (Kingpin Axis & Steering)
    spindle_sup: Point3D    # Upper kingpin point
    spindle_inf: Point3D    # Lower kingpin point
    toe_front: Point3D      # Point on the front face of the rim/tire
    toe_rear: Point3D       # Point on the rear face of the rim/tire
    
    # Force Calculation Parameters
    # Ratio determining how much longitudinal force each arm takes.
    stiffness_ratio_sup: float 
    stiffness_ratio_inf: float
    fx_tire: float          # Longitudinal force at the contact patch (e.g., Braking)

@dataclass(frozen=True)
class SuspensionParameters:
    """
    Parameters for the dynamic simulation.
    """
    h: float                # Minimum CG height to simulate
    mass: float             # Vehicle total mass
    ay: float               # Lateral acceleration (in g's or m/s^2)
    track: float            # Track width
    h_ro: float             # Roll Center height (from 2D calculation)
    scrub_radius: float     # Distance from kingpin axis intersection to contact patch center
    clearance: float        # Ground clearance (affects moment arm calculation)

@dataclass(frozen=True)
class SuspensionIteratorParameters:
    """
    Parameters for the dynamic sweep simulation (CG height variation).
    """
    h_min: float            # Minimum CG height to simulate
    h_max: float            # Maximum CG height to simulate
    h_step: float           # Increment step for CG height
    mass: float             # Vehicle total mass
    ay: float               # Lateral acceleration (in g's or m/s^2)
    track: float            # Track width
    h_ro: float             # Roll Center height (from 2D calculation)
    scrub_radius: float     # Distance from kingpin axis intersection to contact patch center
    clearance: float        # Ground clearance (affects moment arm calculation)

@dataclass(frozen=True)
class ForceAngleParameters:
    """
    Parameters for the Force vs Angle optimization analysis.
    """
    f_load: float           # Total lateral load to be resisted by the suspension
    angle_sup_base: float   # Base angle of the upper arm (degrees)
    angle_inf_base: float   # Base angle of the lower arm (degrees)
    k_sup: float            # Stiffness of the upper arm
    k_inf: float            # Stiffness of the lower arm
    limit: float            # Maximum allowable force in each arm
    ang: float              # Angle delta to analyze

@dataclass(frozen=True)
class ForceAngleIteratorParameters:
    """
    Parameters for the Force vs Angle optimization analysis.
    """
    f_load: float           # Total lateral load to be resisted by the suspension
    angle_sup_base: float   # Base angle of the upper arm (degrees)
    angle_inf_base: float   # Base angle of the lower arm (degrees)
    k_sup: float            # Stiffness of the upper arm
    k_inf: float            # Stiffness of the lower arm
    limit: float            # Maximum allowable force in each arm
    ang_min: int            # Minimum angle delta to analyze
    ang_max: int            # Maximum angle delta to analyze
    step: int               # Step size for angle delta

# ==========================================
# CALCULATION RESULTS
# ==========================================

@dataclass(frozen=True)
class RollCenterResult:
    """Output of the Reimpell 2D geometric analysis."""
    ic: Optional[Point2D]   # Instant Center of Rotation
    h_ro: Optional[float]   # Roll Center Height relative to ground
    q_factor: Optional[float] # Reimpell's curvature factor (stability metric)

@dataclass(frozen=True)
class CamberGainResult:
    d_phi_deg: float        # Body roll angle
    d_gamma_k: float        # Camber change
    k_gamma: Optional[float] # Camber gain factor

@dataclass(frozen=True)
class ForceVector:
    """Decomposed force vector for a specific suspension arm."""
    fx: float               # Lateral component
    fy: float               # Vertical component
    fz: float               # Longitudinal component
    axial: float            # Total force along the tube axis (Buckling load)
    length: float           # Length of the arm

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
    ic_frontal: Optional[Point2D] # Projected IC on the front plane

@dataclass(frozen=True)
class SuspensionResult:
    """Data row for one step of the CG simulation."""
    h_cg: float     # Height of CG
    m_roll: float   # Roll Moment
    d_fz: float     # Load Transfer
    fz_ext: float   # Vertical load on external wheel
    fz_int: float   # Vertical load on internal wheel
    m_sp: float     # Spindle Moment (Steering effort)

@dataclass(frozen=True)
class ForceAngleResult:
    """Stores result for the Force vs Angle optimization analysis."""
    angle_delta: float
    force_sup: float
    force_inf: float
    force_total: float
    status: str  # "OK" or "LIMIT"