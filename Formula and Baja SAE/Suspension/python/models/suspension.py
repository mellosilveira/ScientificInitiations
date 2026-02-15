from primitives import Point2D, Point3D
from lines import LineCoefficients3D
from component_structures import ThreePointArm, TwoPointLink2D, TwoPointLink3D
from dataclasses import dataclass

@dataclass(frozen=True)
class VehycleParameters:
    vehicle_center_line: LineCoefficients3D
    gravity_center: Point3D
    wheelbase: float
    front_brake_percentage: float
    rear_brake_percentage: float
    track_width: float
    kingpin_track: float

@dataclass(frozen=True)
class Suspension2D:
    """
    Suspension points considering 2D (Frontal Projection).
    Constructed manually or projected from the 3D model.
    """
    upper_arm: TwoPointLink2D
    lower_arm: TwoPointLink2D
    tie_rod: TwoPointLink2D
    damper: TwoPointLink2D
    tire_contact: Point2D
    
    # Geometric parameters for Reimpell analysis
    track_width: float
    s1: float  # Outer travel
    s2: float  # Inner travel
    camber_out_deg: float  # Camber at full bump (s1)
    camber_in_deg: float   # Camber at full droop (s2)

@dataclass(frozen=True)
class Suspension:
    """
    Suspension points considering 3D (The Master Model).
    Contains all hardpoints essential for kinematics and dynamics.
    """
    upper_arm: ThreePointArm
    lower_arm: ThreePointArm
    tie_rod: TwoPointLink3D
    damper: TwoPointLink3D
    tire_contact: Point3D
    tire_diameter: float
    camber_gap: float           # Distance between a vertical plumb line and the top/bottom of the rim.
    toe_distance: float         # If positive: toe-in. If negative: tou-out.
    acceleration_on_shaft: bool # Indicates if the acceleration torque acts on motor shaft.
    brake_on_shaft: bool        # Indicates if the brake torque acts on motor shaft.
    
    def get_projected_2d(self, plane: str = 'XY') -> Suspension2D:
        """
        Automatically generates the 2D geometry from 3D points.
        Uses the centroid of inboard points for the 2D projection of A-arms.
        """
        return Suspension2D(
            upper_arm=TwoPointLink2D(
                inner=self.upper_arm.centroid_inner.project_to_2d(plane),
                outer=self.upper_arm.outer.project_to_2d(plane)
            ),
            lower_arm=TwoPointLink2D(
                inner=self.lower_arm.centroid_inner.project_to_2d(plane),
                outer=self.lower_arm.outer.project_to_2d(plane)
            ),
            tie_rod=TwoPointLink2D(
                inner=self.tie_rod.inner.project_to_2d(plane),
                outer=self.tie_rod.outer.project_to_2d(plane)
            ),
            damper=TwoPointLink2D(
                inner=self.damper.inner.project_to_2d(plane),
                outer=self.damper.outer.project_to_2d(plane)
            ),
            tire_contact=self.tire_contact.project_to_2d(plane),
            track_width=0.0,
            s1=0.0, 
            s2=0.0, 
            static_camber=0.0
        )