import math
from typing import Optional
from ..models.primitives import Point2D
from ..models.lines import LineCoefficients2D
from ..models.results import RollCenterResult2D, CamberGainResult
from ..models.suspension import Suspension2D
from ..models.constants import EPSILON
from utils import to_deg

def calculate_roll_center(geo: Suspension2D, vehicle_center_line: Optional[LineCoefficients2D] = None) -> RollCenterResult2D:
    # 1. Define lines for Upper and Lower arms
    line_sup = LineCoefficients2D(geo.upper_arm.inner, geo.upper_arm.outer)
    line_inf = LineCoefficients2D(geo.lower_arm.inner, geo.lower_arm.outer)
    
    # 2. Find Instant Center (IC)
    ic = line_sup.intersect(line_inf)
    if ic is None:
        return RollCenterResult2D(None, None, None)
    
    # 3. Find Roll Center (RC)
    line_ic_tr = LineCoefficients2D(geo.tire_contact, ic)
    line_center = vehicle_center_line if vehicle_center_line else LineCoefficients2D(1, 0, 0)
    rc = line_ic_tr.intersect(line_center)
    if rc is None:
        return RollCenterResult2D(0, 0, None)
    
    h_ro = rc.y
    p = ic.y - geo.tire_contact.y
    q = (p * geo.track_width) / (h_ro ** 2) if abs(h_ro) > EPSILON else 0.0
    return RollCenterResult2D(ic, rc, q)


def calculate_camber_gain(geo: Suspension2D) -> CamberGainResult:
    """
    Calculates kinematic camber gain metrics based on input targets.
    Formula: k_gamma = d_gamma / d_phi
    """
    # Kinematic Roll Angle calculation (Small angle approx or atan)
    # d_phi ~ atan[(s1 + s2) / track]
    d_phi_rad = math.atan((geo.s1 + geo.s2) / geo.track_width)
    d_phi_deg = to_deg(d_phi_rad)
    
    # Camber Change (Input based)
    # The user inputs the camber at full bump (out) and full droop (in).
    # d_gamma is the average change from static.
    d_gamma = (geo.camber_out_deg - geo.camber_in_deg) / 2.0
    
    # Camber Gain Factor (deg/deg)
    k_gamma = (d_gamma / d_phi_deg) if abs(d_phi_deg) > EPSILON else 0.0
    
    return CamberGainResult(d_phi_deg, d_gamma, k_gamma)