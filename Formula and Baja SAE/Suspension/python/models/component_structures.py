from models.lines import LineCoefficients3D
from models.primitives import Point2D, Point3D, Vector3D
from dataclasses import dataclass

@dataclass(frozen=True)
class ThreePointArm:
    """
    Structure representing a double A-arm/wishbone.
    Contains 3 points: 2 on the chassis (inboard) and 1 on the upright (outboard).
    """
    fore_inner: Point3D  # Front chassis pivot
    aft_inner: Point3D   # Rear chassis pivot
    outer: Point3D       # Upright ball joint/Uniball

    @property
    def inner_vector(self) -> Vector3D:
        return Vector3D.from_points(self.fore_inner, self.aft_inner)

    @property
    def outer_line(self) -> LineCoefficients3D:
        return LineCoefficients3D(
            origin=self.outer,
            direction=self.inner_vector.normalize()
        )

    @property
    def vector_fore(self) -> Vector3D:
        """Vector from Chassis Fore to Upright (Outer - Fore)."""
        return Vector3D.from_points(self.fore_inner, self.outer)

    @property
    def vector_aft(self) -> Vector3D:
        """Vector from Chassis Aft to Upright (Outer - Aft)."""
        return Vector3D.from_points(self.aft_inner, self.outer)

    @property
    def centroid_inner(self) -> Point3D:
        """Geometric center of the inboard points."""
        return (self.fore_inner + self.aft_inner) / 2


@dataclass(frozen=True)
class TwoPointLink3D:
    """
    Structure for 2-point components in 3D space.
    Used for: Dampers (Shocks) and Steering Tie Rods.
    """
    inner: Point3D  # Chassis side / Steering rack
    outer: Point3D  # Suspension side / Upright

    @property
    def vector(self) -> Vector3D:
        """Vector from Inner to Outer."""
        return Vector3D.from_points(self.inner, self.outer)

    @property
    def length(self) -> float:
        return self.vector.magnitude


@dataclass(frozen=True)
class TwoPointLink2D:
    """Structure projected in 2D (Front View)."""
    inner: Point2D
    outer: Point2D
