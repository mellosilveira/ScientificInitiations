import math
from ..models.lines import LineCoefficients2D
from ..models.results import RollCenterResult2D, CamberGainResult
from ..models.suspension import Suspension2D
from ..models.constants import EPSILON
from utils import to_deg

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