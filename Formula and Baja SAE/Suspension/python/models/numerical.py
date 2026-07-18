from dataclasses import dataclass

@dataclass(frozen=True)
class SolverParameters:
    """Configuration for numerical solvers (e.g., Brent's method)."""
    min_val: float        # Lower bound (in radians)
    max_val: float        # Upper bound (in radians)
    tolerance: float      # Acceptable error margin (e.g., 1e-6 mm)
    max_iter: int         # Maximum iterations
    method: str