import math

def cot(angle: float) -> float:
    return 1.0 / math.tan(angle)

def acot(cot: float) -> float:
    return math.atan(1 / cot)