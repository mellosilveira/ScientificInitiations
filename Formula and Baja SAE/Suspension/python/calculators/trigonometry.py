from ..models.primitives import Point3D, Vector3D
import math

def cot(angle: float) -> float:
    return 1.0 / math.tan(angle)

def acot(cot: float) -> float:
    return math.atan(1 / cot)

def rodrigues_rotation(point: Point3D, axis_origin: Point3D, axis_dir: Vector3D, angle_rad: float) -> Point3D:
    """
    Rotaciona um ponto 3D ao redor de um eixo arbitrário usando a Fórmula de Rodrigues.
    
    :param point: Ponto a ser rotacionado (ex: Tie Rod Outer).
    :param axis_origin: Um ponto pertencente ao eixo (ex: Lower Ball Joint).
    :param axis_dir: Vetor unitário da direção do eixo (Vetor Kingpin).
    :param angle_rad: Ângulo de rotação em radianos.
    :return: Nova posição do ponto rotacionado.
    """
    # Vetor do ponto até a origem do eixo
    v = Vector3D.from_points(point, axis_origin)
    
    cos_a = math.cos(angle_rad)
    sin_a = math.sin(angle_rad)
    
    cross_kv = axis_dir.cross_product(v)
    dot_kv = axis_dir.dot(v)
    
    # Fórmula vetorial de Rodrigues:
    # V_rot = v*cos(theta) + (k x v)*sin(theta) + k*(k . v)*(1 - cos(theta))
    term1 = v * cos_a
    term2 = cross_kv * sin_a
    term3 = axis_dir * (dot_kv * (1.0 - cos_a))
    v_rot = term1 + term2 + term3
    
    # Retorna o ponto transladado de volta
    return axis_origin + v_rot