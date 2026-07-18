# math_2d.py
from __future__ import annotations

import math
from dataclasses import dataclass
from typing import Optional

EPSILON = 1e-9


# -----------------------------------------------------------------------------
# Fallback models (caso você não tenha models.Point2D etc.)
# -----------------------------------------------------------------------------
try:
    from models import Point2D, SuspensionGeometry2D  # seu projeto
except Exception:
    @dataclass
    class Point2D:
        x: float
        y: float

    @dataclass
    class SuspensionGeometry2D:
        track_width: float  # mm
        upper_in: Point2D
        upper_out: Point2D
        lower_in: Point2D
        lower_out: Point2D
        s1: float = 0.0
        s2: float = 0.0
        camber_out_deg: float = 0.0
        camber_in_deg: float = 0.0


@dataclass
class LineCoefficients:
    a: float
    b: float
    c: float


@dataclass
class RollCenterResult:
    ic: Optional[Point2D]
    h_ro: Optional[float]   # altura do RC (mm)


@dataclass
class CamberGainResult:
    camber_gain_deg_per_mm: float
    camber_at_ext_deg: float
    camber_at_comp_deg: float


# -----------------------------------------------------------------------------
# Geometria 2D (reta / interseção)
# -----------------------------------------------------------------------------
def get_line_coefficients(p1: Point2D, p2: Point2D) -> LineCoefficients:
    # ax + by + c = 0 passando por p1 e p2
    a = p1.y - p2.y
    b = p2.x - p1.x
    c = p1.x * p2.y - p2.x * p1.y
    return LineCoefficients(a, b, c)


def solve_intersection(l1: LineCoefficients, l2: LineCoefficients) -> Optional[Point2D]:
    det = l1.a * l2.b - l2.a * l1.b
    if abs(det) < EPSILON:
        return None
    x = (l1.b * l2.c - l2.b * l1.c) / det
    y = (l1.c * l2.a - l2.c * l1.a) / det
    return Point2D(x, y)


# -----------------------------------------------------------------------------
# Roll Center (método geométrico clássico em 2D)
# -----------------------------------------------------------------------------
def calculate_instant_center(geo: SuspensionGeometry2D) -> Optional[Point2D]:
    """
    IC = interseção das extensões do braço superior e inferior (vista frontal 2D).
    """
    l_up = get_line_coefficients(geo.upper_in, geo.upper_out)
    l_lo = get_line_coefficients(geo.lower_in, geo.lower_out)
    return solve_intersection(l_up, l_lo)


def calculate_roll_center(geo: SuspensionGeometry2D) -> RollCenterResult:
    """
    Roll Center (um lado):
      1) acha IC (interseção dos braços)
      2) traça reta IC -> contato do pneu (aprox. em x=track/2, y=0)
      3) RC é interseção dessa reta com o plano central x=0
    """
    ic = calculate_instant_center(geo)
    if ic is None:
        return RollCenterResult(ic=None, h_ro=None)

    # contato do pneu no lado direito (aprox.)
    # Se você estiver usando lado esquerdo, geo.upper_in.x etc. já podem ser negativos.
    # Aqui pegamos o "sinal" do lado pelo x do outboard inferior.
    sign = 1.0 if geo.lower_out.x >= 0 else -1.0
    x_contact = sign * (geo.track_width / 2.0)
    y_contact = 0.0
    contact = Point2D(x_contact, y_contact)

    l_ic_to_tire = get_line_coefficients(ic, contact)

    # linha do centro do carro: x = 0 -> (1)x + (0)y + 0 = 0
    l_center = LineCoefficients(1.0, 0.0, 0.0)
    rc = solve_intersection(l_ic_to_tire, l_center)

    if rc is None:
        return RollCenterResult(ic=ic, h_ro=None)

    return RollCenterResult(ic=ic, h_ro=rc.y)


# -----------------------------------------------------------------------------
# Camber gain (modelo simples usando entrada do usuário)
# -----------------------------------------------------------------------------
def calculate_camber_gain(geo: SuspensionGeometry2D) -> CamberGainResult:
    """
    Modelo simples para o teu app:
      camber_out_deg em s1 (extensão)
      camber_in_deg  em s2 (compressão)
      ganho ~ (camber_in - camber_out) / (s1 + s2) [deg/mm]
    """
    total = float(geo.s1) + float(geo.s2)
    if abs(total) < EPSILON:
        gain = 0.0
    else:
        gain = (float(geo.camber_in_deg) - float(geo.camber_out_deg)) / total

    return CamberGainResult(
        camber_gain_deg_per_mm=gain,
        camber_at_ext_deg=float(geo.camber_out_deg),
        camber_at_comp_deg=float(geo.camber_in_deg),
    )