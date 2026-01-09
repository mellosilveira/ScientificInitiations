import math

RAD_PER_DEG = math.pi / 180.0

def to_rad(deg: float) -> float:
    """Converts Degrees to Radians."""
    return deg * RAD_PER_DEG

def to_deg(rad: float) -> float:
    """Converts Radians to Degrees."""
    return rad / RAD_PER_DEG

def safe_float(value: str, default: float = 0.0) -> float:
    """
    Safely converts a string from UI entry to float.
    Handles empty strings and comma/dot decimal separators.
    """
    try:
        clean_val = value.strip().replace(',', '.')
        return default if not clean_val else float(clean_val)
    except (ValueError, AttributeError):
        return default