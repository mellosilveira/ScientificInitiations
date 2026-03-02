import scipy.optimize as opt
from typing import Callable
from ..models.numerical import SolverParameters

def find_root(objective_func: Callable[[float], float], params: SolverParameters) -> float:
    """
    Finds the root of a 1D scalar function using Brent's method.
    
    :param objective_func: Function taking a float (radians) and returning a float (error).
    :param params: SolverParameters defining bounds and tolerances.
    :return: The root value (in radians) where the error is within tolerance.
    """
    try:
        # brentq is highly reliable for bounded root finding
        result = opt.root_scalar(
            f=objective_func,
            bracket=[params.min_val, params.max_val],
            xtol=params.tolerance,
            maxiter=params.max_iter,
            method=params.method if params.method else 'brentq'
        )
        
        if result.converged:
            return result.root
        else:
            raise RuntimeError(f"Solver did not converge after {params.max_iter} iterations.")
            
    except ValueError:
        # Occurs if f(min) and f(max) have the same sign (no root in the bracket)
        # This means the geometry binds (e.g., suspension cannot physically travel that far)
        raise RuntimeError("Geometry binding: Target value is physically unreachable within the given bounds.")