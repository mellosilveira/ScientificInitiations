from typing import Optional
from models import (
    Point2D, LineCoefficients, SuspensionGeometry2D, 
    RollCenterResult, CamberGainResult, EPSILON
)
from utils import to_deg

def get_line_coefficients(p1: Point2D, p2: Point2D) -> LineCoefficients:
    """
    Calculates coefficients a, b, c for the general line equation: ax + by + c = 0
    Derived from the two-point form: (y - y1) = m(x - x1).
    """
    a = p1.y - p2.y
    b = p2.x - p1.x
    c = p1.x * p2.y - p2.x * p1.y
    return LineCoefficients(a, b, c)

def solve_intersection(l1: LineCoefficients, l2: LineCoefficients) -> Optional[Point2D]:
    """
    Finds the intersection point of two lines using Cramer's Rule (Determinants).
    Returns None if the determinant is zero (Lines are parallel).
    """
    det = l1.a * l2.b - l2.a * l1.b
    
    # Check for parallel lines (Instant Center at infinity)
    if abs(det) < EPSILON:
        return None
        
    x = (l1.b * l2.c - l2.b * l1.c) / det
    y = (l1.c * l2.a - l2.c * l1.a) / det
    return Point2D(x, y)

def calculate_roll_center(geo: SuspensionGeometry2D) -> RollCenterResult:
    """
    Calculates the Instant Center (IC) and Roll Center (RC) using Reimpell's geometric method.
    """
    # 1. Get coefficients for arm lines
    l_sup = get_line_coefficients(geo.upper_in, geo.upper_out)
    l_inf = get_line_coefficients(geo.lower_in, geo.lower_out)
    
    # 2. Find Instant Center (IC)
    ic = solve_intersection(l_sup, l_inf)
    
    # Handle parallel arms case
    if ic is None:
        return RollCenterResult(None, None, None)
    
    # Wheel contact center (W) in Front View
    # Assumes symmetrical vehicle, W is at half track width on the ground (y=0)
    w_x = geo.track_width / 2.0
    w_y = 0.0
    
    # 3. Calculate Roll Center (h_ro)
    # The Roll Center lies on the line connecting the IC and the Wheel Center (W).
    # We need the height where this line crosses x=0.
    
    if abs(ic.x - w_x) < EPSILON:
        h_ro = None
        q = None
    else:
        # Slope of FVSA
        m = (ic.y - w_y) / (ic.x - w_x)
        
        # Line Eq: y - wy = m(x - wx). Solving for x=0 gives h_ro.
        h_ro = m * (0 - w_x) + w_y
        
        # 4. Calculate q-factor (Reimpell stability metric)
        # q = p * bf / h_ro^2 (approximation)
        p = ic.y - w_y # Vertical distance IC to ground
        q = (p * geo.track_width) / (h_ro**2) if abs(h_ro) > EPSILON else 0.0
        
    return RollCenterResult(ic, h_ro, q)

def calculate_camber_gain(geo: SuspensionGeometry2D) -> CamberGainResult:
    """
    Calculates kinematic camber gain indicators based on wheel travel.
    """
    if abs(geo.track_width) < EPSILON:
        return CamberGainResult(0.0, 0.0, None)
    
    # d_phi: Approximate body roll angle based on suspension travel difference
    d_phi_rad = (geo.s1 + geo.s2) / geo.track_width
    d_phi_deg = to_deg(d_phi_rad)
    
    # d_gamma: Average camber change
    d_gamma = (geo.camber_out_deg - geo.camber_in_deg) / 2.0
    
    # k_gamma ratio
    k_gamma = (d_gamma / d_phi_deg) if abs(d_phi_deg) > EPSILON else 0.0
    
    return CamberGainResult(d_phi_deg, d_gamma, k_gamma)