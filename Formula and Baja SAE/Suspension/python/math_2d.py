from typing import Optional
from models import (
    Point2D, LineCoefficients, SuspensionGeometry2D, 
    RollCenterResult, CamberGainResult
)
from utils import to_deg

def get_line_coefficients(p1: Point2D, p2: Point2D) -> LineCoefficients:
    """Calculates a, b, c for ax + by + c = 0."""
    a = p1.y - p2.y
    b = p2.x - p1.x
    c = p1.x * p2.y - p2.x * p1.y
    return LineCoefficients(a, b, c)

def solve_intersection(l1: LineCoefficients, l2: LineCoefficients) -> Optional[Point2D]:
    det = l1.a * l2.b - l2.a * l1.b
    if abs(det) < 1e-9:
        return None
    x = (l1.b * l2.c - l2.b * l1.c) / det
    y = (l1.c * l2.a - l2.c * l1.a) / det
    return Point2D(x, y)

def calculate_roll_center(geo: SuspensionGeometry2D) -> RollCenterResult:
    l_sup = get_line_coefficients(geo.upper_in, geo.upper_out)
    l_inf = get_line_coefficients(geo.lower_in, geo.lower_out)
    
    ic = solve_intersection(l_sup, l_inf)
    
    if ic is None:
        return RollCenterResult(None, None, None)
    
    # Wheel center (W)
    w_x = geo.track_width / 2.0
    w_y = 0.0
    
    # FVSA line: IC -> W
    if abs(ic.x - w_x) < 1e-9:
        h_ro = None
        q = None
    else:
        m = (ic.y - w_y) / (ic.x - w_x)
        # Intersect with centerline (x=0)
        h_ro = m * (0 - w_x) + w_y
        
        # q factor
        p = ic.y - w_y
        q = (p * geo.track_width) / (h_ro**2) if abs(h_ro) > 1e-9 else 0.0
        
    return RollCenterResult(ic, h_ro, q)

def calculate_camber_gain(geo: SuspensionGeometry2D) -> CamberGainResult:
    if abs(geo.track_width) < 1e-9:
        return CamberGainResult(0.0, 0.0, None)
        
    d_phi_rad = (geo.s1 + geo.s2) / geo.track_width
    d_phi_deg = to_deg(d_phi_rad)
    
    d_gamma = (geo.camber_out_deg - geo.camber_in_deg) / 2.0
    
    k_gamma = (d_gamma / d_phi_deg) if abs(d_phi_deg) > 1e-9 else 0.0
    
    return CamberGainResult(d_phi_deg, d_gamma, k_gamma)