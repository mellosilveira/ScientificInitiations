from typing import Optional
from primitives import Point2D, Point3D, Vector3D
from dataclasses import dataclass
from constants import EPSILON

@dataclass(frozen=True)
class LineCoefficients2D:
    """
    Represents the general line equation in a plane: ax + by + c = 0.
    Useful for finding intersections (Instant Center).
    """
    a: float
    b: float
    c: float

    def __init__(self, p1: Point2D, p2: Point2D):
        """
        Calculates coefficients a, b, and c based on two points.
        Eq: (y1 - y2)x + (x2 - x1)y + (x1*y2 - x2*y1) = 0
        """
        self.a = p1.y - p2.y
        self.b = p2.x - p1.x
        self.c = p1.x * p2.y - p2.x * p1.y

    def intersect(self, other: 'LineCoefficients2D') -> Optional[Point2D]:
        """
        Calculates the intersection point with another line using Cramer's rule.
        Returns None if lines are parallel.
        """
        determinant = self.a * other.b - other.a * self.b

        if abs(determinant) < 1e-9:  # Tolerance for floating point parallel check
            return None

        x = (self.b * other.c - other.b * self.c) / determinant
        y = (other.a * self.c - self.a * other.c) / determinant
        
        return Point2D(x, y)

    def __repr__(self):
        return f"Line: {self.a:.2f}x + {self.b:.2f}y + {self.c:.2f} = 0"


@dataclass(frozen=True)
class LineCoefficients3D:
    """
    Represents a line in 3D space.
    Defined by an Origin Point and a Direction Vector.
    """
    origin: Point3D
    direction: Vector3D
    
    @staticmethod
    def from_points(p1: Point3D, p2: Point3D) -> 'LineCoefficients3D':
        return LineCoefficients3D(
            origin = p1,
            direction = Vector3D.from_points(p1, p2).normalize()
        )
        
    def intersect(self, other: 'LineCoefficients3D') -> Optional[Point3D]:
        """
        Calcula a interseção exata usando o algoritmo:
        1. Checar Paralelismo.
        2. Resolver sistema para X e Y.
        3. Verificar consistência em Z.
        """
        # --- Passo 1: Checar Paralelismo ---
        # Se o produto vetorial for (0,0,0), os vetores são colineares (paralelos).
        cross = self.direction.cross_product(other.direction)
        if cross.is_zero():
            return None # Retas paralelas

        # --- Passo 2: Resolver o sistema linear 2x2 (usando planos X e Y) ---
        # Sistema:
        # t * d1.x - s * d2.x = p2.x - p1.x  (Eq X)
        # t * d1.y - s * d2.y = p2.y - p1.y  (Eq Y)
        
        # Delta das origens (Lado direito da equação)
        dx = other.origin.x - self.origin.x
        dy = other.origin.y - self.origin.y
        
        # Determinante da matriz dos coeficientes (para X e Y)
        # Matriz: [[d1.x, -d2.x], [d1.y, -d2.y]]
        det = self.direction.x * (-other.direction.y) - (-other.direction.x) * self.direction.y
        
        # CUIDADO: Se as retas forem verticais (perpendiculares ao plano XY), 
        # o determinante será 0 mesmo que elas se cruzem.
        if abs(det) < 1e-9:
            # Nota: Em um código de produção robusto, você tentaria resolver usando X-Z ou Y-Z aqui.
            return None 

        # Regra de Cramer para encontrar 't' (parâmetro da Reta 1)
        # Substitui primeira coluna pelo vetor de resultados (dx, dy)
        det_t = dx * (-other.direction.y) - (-other.direction.x) * dy
        t = det_t / det
        
        # Regra de Cramer para encontrar 's' (parâmetro da Reta 2)
        # Substitui segunda coluna pelo vetor de resultados (dx, dy)
        det_s = self.direction.x * dy - self.direction.y * dx
        s = det_s / det

        # --- Passo 3: Verificar Interseção no Eixo Z ---
        # Calcula Z para a Reta 1 usando 't'
        z1 = self.origin.z + t * self.direction.z
        
        # Calcula Z para a Reta 2 usando 's'
        z2 = other.origin.z + s * other.direction.z
        
        # Se a diferença for insignificante, as retas se tocam
        if abs(z1 - z2) < 1e-5: # Tolerância para ponto flutuante
            x = self.origin.x + t * self.direction.x
            y = self.origin.y + t * self.direction.y
            return Point3D(x, y, z1)
        
        return None # Retas são reversas (Skew)