from dataclasses import dataclass
import math
from typing import Tuple
from constants import EPSILON
import numpy as np

@dataclass(frozen=True)
class Point2D:
    """Represents a point in the 2D plane (e.g., Front View X-Y)."""
    x: float
    y: float

    @property
    def array(self) -> np.ndarray:
        """Returns NumPy array [x, y]."""
        return np.array([self.x, self.y])

    def as_tuple(self) -> Tuple[float, float]:
        return (self.x, self.y)


@dataclass(frozen=True)
class Point3D:
    """
    Represents a point in 3D space (X, Y, Z).
    Includes vector math capabilities (subtraction, addition, normalization).
    """
    x: float
    y: float
    z: float

    # --- Vector Math Overloads ---
    def __sub__(self, other: 'Point3D') -> 'Point3D':
        """Vector subtraction: p3 = self - other"""
        return Point3D(self.x - other.x, self.y - other.y, self.z - other.z)

    def __iadd__ (self, other: 'Point3D') -> 'Point3D':
        """Vector addition: p3 = self + other"""
        return Point3D(self.x + other.x, self.y + other.y, self.z + other.z)

    def __add__(self, other: 'Point3D') -> 'Point3D':
        """Vector addition: p3 = self + other"""
        return Point3D(self.x + other.x, self.y + other.y, self.z + other.z)
    
    def __mul__(self, scalar: float) -> 'Point3D':
        """Scalar multiplication: p2 = self * scalar"""
        return Point3D(self.x * scalar, self.y * scalar, self.z * scalar)

    def __truediv__(self, scalar: float) -> 'Point3D':
        """Scalar division: p2 = self / scalar"""
        if abs(scalar) < EPSILON:
            raise ValueError("Division by zero in Point3D")
        return Point3D(self.x / scalar, self.y / scalar, self.z / scalar)

    # --- Properties ---
    @property
    def array(self) -> np.ndarray:
        """Returns NumPy array [x, y, z] for matrix calculations."""
        return np.array([self.x, self.y, self.z])
    
    def project_to_2d(self, plane: str = 'XY') -> Point2D:
        """Projects the point to the Frontal Plane (drops Z coordinate)."""
        if plane == 'XY':
            return Point2D(self.x, self.y)
        if plane == 'XZ':
            return Point2D(self.x, self.z)
        if plane == 'YZ':
            return Point2D(self.y, self.z)
        return Point2D(0, 0)

@dataclass(frozen=True)
class Vector3D:
    """
    Representa um vetor direção no espaço 3D.
    Diferente de Ponto, representa magnitude e direção.
    """
    x: float
    y: float
    z: float
    
    @staticmethod
    def from_points(p1: Point3D, p2: Point3D) -> 'Vector3D':
        return Vector3D(
            x = p2.x - p1.x,
            y = p2.y - p1.y,
            z = p2.z - p1.z
        )

    @property
    def array(self) -> np.ndarray:
        return np.array([self.x, self.y, self.z])

    @property
    def magnitude(self) -> float:
        return math.sqrt(self.x**2 + self.y**2 + self.z**2)

    def normalize(self) -> 'Vector3D':
        m = self.magnitude
        if m < EPSILON:
            return Vector3D(0, 0, 0)
        return Vector3D(self.x / m, self.y / m, self.z / m)

    # Produto Escalar (Dot Product)
    def dot(self, other: 'Vector3D') -> float:
        return self.x * other.x + self.y * other.y + self.z * other.z

    # Produto Vetorial (Cross Product)
    def cross_product(self, other: 'Vector3D') -> 'Vector3D':
        return Vector3D(
            self.y * other.z - self.z * other.y,
            self.z * other.x - self.x * other.z,
            self.x * other.y - self.y * other.x
        )

    def is_zero(self) -> bool:
        return self.magnitude < EPSILON
    
    def __mul__(self, scalar: float) -> 'Vector3D':
        return Vector3D(self.x * scalar, self.y * scalar, self.z * scalar)