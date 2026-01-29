from typing import Optional
from models import (
    Point2D, Point3D, LineCoefficients, SuspensionGeometry2D,
    RollCenterResult, CamberGainResult, EPSILON
)
from utils import to_deg


def get_line_coefficients(p1: Point2D, p2: Point2D) -> LineCoefficients:
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


def calculate_roll_center(geo: SuspensionGeometry2D) -> RollCenterResult:
    l_sup = get_line_coefficients(geo.upper_in, geo.upper_out)
    l_inf = get_line_coefficients(geo.lower_in, geo.lower_out)

    ic = solve_intersection(l_sup, l_inf)

    if ic is None:
        return RollCenterResult(None, None, None, None)

    w_x = geo.track_width / 2.0
    w_y = 0.0

    if abs(ic.x - w_x) < EPSILON:
        ro = None
        h_ro = None
        q = None
    else:
        m = (ic.y - w_y) / (ic.x - w_x)
        h_ro = m * (0 - w_x) + w_y

        # 3D Point for Ro (Z=0 in local 2D frame)
        ro = Point3D(0.0, h_ro, 0.0)

        p = ic.y - w_y
        q = (p * geo.track_width) / (h_ro ** 2) if abs(h_ro) > EPSILON else 0.0

    return RollCenterResult(ic, ro, h_ro, q)


def calculate_camber_gain(geo: SuspensionGeometry2D) -> CamberGainResult:
    if abs(geo.track_width) < EPSILON:
        return CamberGainResult(0.0, 0.0, None)

    d_phi_rad = (geo.s1 + geo.s2) / geo.track_width
    d_phi_deg = to_deg(d_phi_rad)
    d_gamma = (geo.camber_out_deg - geo.camber_in_deg) / 2.0
    k_gamma = (d_gamma / d_phi_deg) if abs(d_phi_deg) > EPSILON else 0.0

    return CamberGainResult(d_phi_deg, d_gamma, k_gamma)