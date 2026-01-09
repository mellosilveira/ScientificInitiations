import math

def to_rad(deg: float) -> float:
    """Converts Degrees to Radians."""
    return deg * math.pi / 180.0

def to_deg(rad: float) -> float:
    """Converts Radians to Degrees."""
    return rad * 180.0 / math.pi

def safe_float(value: str, default: float = 0.0) -> float:
    """
    Safely converts a string from UI entry to float.
    Handles empty strings and comma/dot decimal separators.
    """
    try:
        clean_val = value.strip().replace(',', '.')
        if not clean_val:
            return default
        return float(clean_val)
    except (ValueError, AttributeError):
        return default

def format_value(val: float, precision: int = 1) -> str:
    return f"{val:.{precision}f}"