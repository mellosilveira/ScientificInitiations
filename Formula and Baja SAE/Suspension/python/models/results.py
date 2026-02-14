from primitives import Point2D, Point3D
from lines import LineCoefficients3D
from dataclasses import dataclass
from typing import Optional

@dataclass(frozen=True)
class TireAnglesResult:
    camber: float   # Tire inclination with y axis in XY plane.
    toe: float      # Tire inclination with z axis in XZ plane.

@dataclass(frozen=True)
class KingpingResult:
    axis: LineCoefficients3D
    inclination: float  
    scrub_radius: float
    mechanical_trail: float
    caster: float

@dataclass(frozen=True)
class RollCenterResult2D:
    """Result of 2D geometric analysis (Reimpell)."""
    instantaneous_center: Optional[Point2D]  # Instant Center
    roll_center_point: Optional[Point2D]    # Roll Center in plane
    q_factor: Optional[float]               # Suspension curvature factor

@dataclass(frozen=True)
class RollCenterResult3D:
    """Result of 3D geometric analysis."""
    instantaneous_center: Optional[Point3D]  # Instant Center
    roll_center_point: Optional[Point3D]    # Roll Center in plane
    q_factor: Optional[float]               # Suspension curvature factor
    
    def get_projected_2d(self, plane: str = 'XY') -> RollCenterResult2D:
        """
        Automatically generates the 2D geometry from 3D points.
        Uses the centroid of inboard points for the 2D projection of A-arms.
        """
        return RollCenterResult2D(
            instantaneous_center = self.instantaneous_center.project_to_2d(plane),
            roll_center_point = self.roll_center_point.project_to_2d(plane),
            q_factor = self.q_factor
        )

@dataclass(frozen=True)
class CamberGainResult:
    """Results from kinematic camber analysis."""
    d_phi_deg: float      # Roll angle
    d_gamma_deg: float    # Camber change
    k_gamma: float        # Camber gain factor (deg/deg)

@dataclass(frozen=True)
class AlignmentMetricsResult3D:
    """3D Geometry results (Static Alignment)."""
    tire_angles: TireAnglesResult
    kingpin_parameters: KingpingResult
    roll_center_prameters: RollCenterResult3D