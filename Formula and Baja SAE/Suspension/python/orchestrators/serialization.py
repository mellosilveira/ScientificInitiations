from __future__ import annotations

from typing import Dict, Any



# Serialização (HP + presets)


class SerializationOrchestrator:
    """
    Organiza hardpoints/presets para salvar/carregar de forma consistente.
    (Não depende de Tkinter: é puro domínio.)
    """

    @staticmethod
    def hardpoints_to_dict(hp_dict: dict) -> Dict[str, Any]:
        out = {}
        for k, v in hp_dict.items():
            if isinstance(v, (tuple, list)) and len(v) == 3:
                out[k] = (float(v[0]), float(v[1]), float(v[2]))
            else:
                try:
                    out[k] = float(v)
                except Exception:
                    out[k] = v
        return out

    @staticmethod
    def hardpoints_from_dict(data: Dict[str, Any]) -> Dict[str, Any]:
        return dict(data)
