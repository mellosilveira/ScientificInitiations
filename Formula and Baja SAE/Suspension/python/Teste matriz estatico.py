"""
APP TKINTER - SUSPENSÃO (2D/3D/Visual/Dinâmica/Otimização) + ABA NOVA "Equilíbrio (Matriz)"

✅ O que este arquivo faz:
- Mantém sua estrutura de app (abas 2D/3D/Visual/Dinâmica/Otimização)
- Adiciona UMA ABA NOVA: "Equilíbrio (Matriz)"
    - Carrega hardpoints automaticamente a partir de um .ipynb (parse de linhas: A = np.array([x,y,z]))
    - Monta matriz A (6x6) e vetor B (6x1) com entrada Fx,Fy,Fz
    - Resolve reações (R_G, R_K, R_E, R_D, R_J, R_H)
    - Mostra A, B e reações em uma caixa de texto
    - Mostra visualização 3D no matplotlib (pontos + membros)

⚠️ Dependências:
- numpy, matplotlib, tkinter
- seus módulos de domínio: models, math_2d, math_3d, dynamics, utils, ui_strings
"""

import json
import csv
import math
import re
from dataclasses import dataclass
from typing import Dict, List

import numpy as np
import tkinter as tk
from tkinter import ttk, messagebox, filedialog

import matplotlib.pyplot as plt
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg, NavigationToolbar2Tk

# Domain modules (precisam existir no seu projeto)
import models
import math_2d
import math_3d
import dynamics
import utils
import ui_strings


# =========================
# UI Helpers
# =========================

class ToolTip:
    def __init__(self, widget, text: str, wrap=260):
        self.widget = widget
        self.text = text
        self.wrap = wrap
        self.top = None
        widget.bind("<Enter>", self._show)
        widget.bind("<Leave>", self._hide)

    def _show(self, _event=None):
        if self.top or not self.text:
            return
        x = self.widget.winfo_rootx() + 20
        y = self.widget.winfo_rooty() + 20
        self.top = tk.Toplevel(self.widget)
        self.top.overrideredirect(True)
        self.top.attributes("-topmost", True)

        frm = ttk.Frame(self.top, padding=8, style="Tip.TFrame")
        frm.pack(fill="both", expand=True)

        lbl = ttk.Label(frm, text=self.text, wraplength=self.wrap, justify="left", style="Tip.TLabel")
        lbl.pack()

        self.top.geometry(f"+{x}+{y}")

    def _hide(self, _event=None):
        if self.top:
            self.top.destroy()
            self.top = None


class CollapsibleSection(ttk.Frame):
    def __init__(self, parent, title: str, *args, **kwargs):
        super().__init__(parent, *args, **kwargs)

        self.var_open = tk.BooleanVar(value=True)

        header = ttk.Frame(self)
        header.pack(fill="x")

        self.btn = ttk.Checkbutton(
            header,
            text=title,
            variable=self.var_open,
            command=self._toggle,
            style="Section.TCheckbutton"
        )
        self.btn.pack(side="left", fill="x", expand=True)

        self.body = ttk.Frame(self)
        self.body.pack(fill="x", expand=True)

    def _toggle(self):
        if self.var_open.get():
            self.body.pack(fill="x", expand=True)
        else:
            self.body.forget()


class ScrollableFrame(ttk.Frame):
    """Frame contendo canvas e scrollbar para permitir rolagem dos inputs."""
    def __init__(self, container, width=390, *args, **kwargs):
        super().__init__(container, *args, **kwargs)
        self.canvas = tk.Canvas(self, width=width, highlightthickness=0)
        scrollbar = ttk.Scrollbar(self, orient="vertical", command=self.canvas.yview)

        self.scrollable_window = ttk.Frame(self.canvas)
        self.scrollable_window.bind("<Configure>", lambda e: self.canvas.configure(scrollregion=self.canvas.bbox("all")))

        self.canvas.create_window((0, 0), window=self.scrollable_window, anchor="nw")
        self.canvas.configure(yscrollcommand=scrollbar.set)

        self.canvas.pack(side="left", fill="both", expand=True)
        scrollbar.pack(side="right", fill="y")

        self.canvas.bind("<Enter>", lambda e: self._bind_wheel())
        self.canvas.bind("<Leave>", lambda e: self._unbind_wheel())

    def _bind_wheel(self):
        self.canvas.bind_all("<MouseWheel>", self._on_mousewheel)

    def _unbind_wheel(self):
        self.canvas.unbind_all("<MouseWheel>")

    def _on_mousewheel(self, event):
        self.canvas.yview_scroll(int(-1 * (event.delta / 120)), "units")


def safe_num(s: str) -> float:
    """Aceita vírgula e ponto; levanta erro com mensagem boa."""
    if s is None:
        raise ValueError("Valor vazio.")
    s = str(s).strip().replace(",", ".")
    if s == "":
        raise ValueError("Valor vazio.")
    return float(s)


# ============================================================
#  ABA EQUILÍBRIO (MATRIZ) - Solver
# ============================================================

_POINT_RE = re.compile(
    r"""^\s*([A-Za-z]\w*)\s*=\s*np\.array\(\s*\[\s*([-\d\.]+)\s*,\s*([-\d\.]+)\s*,\s*([-\d\.]+)\s*\]\s*\)""",
    re.MULTILINE
)

def extract_points_from_ipynb(ipynb_path: str) -> Dict[str, np.ndarray]:
    with open(ipynb_path, "r", encoding="utf-8") as f:
        nb = json.load(f)

    points: Dict[str, np.ndarray] = {}
    for cell in nb.get("cells", []):
        src = "".join(cell.get("source", []))
        for name, xs, ys, zs in _POINT_RE.findall(src):
            points[name] = np.array([float(xs), float(ys), float(zs)], dtype=float)

    if not points:
        raise ValueError(
            "Não encontrei hardpoints no ipynb. "
            "Formato esperado: A = np.array([x, y, z])"
        )
    return points


def unit_vector(p_from: np.ndarray, p_to: np.ndarray, eps: float = 1e-12) -> np.ndarray:
    v = (p_to - p_from).astype(float)
    n = np.linalg.norm(v)
    if n < eps:
        raise ValueError("Vetor nulo entre pontos (hardpoints duplicados).")
    return v / n


@dataclass(frozen=True)
class ReactionMember:
    name: str
    from_point: str
    to_point: str
    app_point: str


DEFAULT_MEMBERS: List[ReactionMember] = [
    ReactionMember("R_G", "A", "G", "G"),
    ReactionMember("R_K", "A", "K", "K"),
    ReactionMember("R_E", "B", "E", "E"),
    ReactionMember("R_D", "B", "D", "D"),
    ReactionMember("R_J", "I", "J", "J"),
    ReactionMember("R_H", "Q", "H", "H"),
]


def build_A_matrix(points: Dict[str, np.ndarray],
                   members: List[ReactionMember],
                   moment_about: str = "O",
                   length_unit: str = "mm") -> np.ndarray:
    if moment_about not in points:
        raise KeyError(f"Ponto '{moment_about}' não existe nos hardpoints.")

    O = points[moment_about]
    n = len(members)

    U = np.zeros((3, n), dtype=float)
    M = np.zeros((3, n), dtype=float)

    mm_to_m = 1.0 / 1000.0 if (length_unit.lower() == "mm") else 1.0

    for j, mem in enumerate(members):
        for key in (mem.from_point, mem.to_point, mem.app_point):
            if key not in points:
                raise KeyError(f"Hardpoint '{key}' não encontrado (membro {mem.name}).")

        p_from = points[mem.from_point]
        p_to   = points[mem.to_point]
        p_app  = points[mem.app_point]

        u = unit_vector(p_from, p_to)         # direção
        r = (p_app - O) * mm_to_m             # braço (m)

        U[:, j] = u
        M[:, j] = np.cross(r, u)

    A = np.vstack([U, M])  # 6x6
    return A


def build_B_vector(Fx: float, Fy: float, Fz: float) -> np.ndarray:
    return np.array([[Fx], [Fy], [Fz], [0.0], [0.0], [0.0]], dtype=float)


def solve_reactions(A: np.ndarray, B: np.ndarray, members: List[ReactionMember]) -> Dict[str, float]:
    if A.shape != (6, 6):
        raise ValueError(f"A precisa ser 6x6 (modelo com 6 membros). Recebi {A.shape}.")
    if B.shape != (6, 1):
        raise ValueError(f"B precisa ser 6x1. Recebi {B.shape}.")

    x = np.linalg.solve(A, B).reshape(6)
    return {members[i].name: float(x[i]) for i in range(6)}


# =========================
# Main App
# =========================

class App:
    # -------------------------
    # Hardpoints helpers
    # -------------------------
    def _pair_corner(self, corner: str):
        """
        Retorna (corner_right, corner_left) do mesmo eixo.
        Ex:
          FR -> (FR, FL)
          FL -> (FR, FL)
          RR -> (RR, RL)
          RL -> (RR, RL)
        """
        c = (corner or "FR").strip().upper()
        if c.endswith("L"):
            cR = c[:-1] + "R"
            cL = c
        else:
            cR = c
            cL = c[:-1] + "L"
        return cR, cL

    def _corner_points_2d(self, corner: str):
        """Lê os 4 hardpoints 2D (x,y) de um canto."""
        c = (corner or "FR").strip().upper()
        ui_x, ui_y, _ = self._hp_get(self._hp_key(c, "Sup In"))
        uo_x, uo_y, _ = self._hp_get(self._hp_key(c, "Sup Out"))
        li_x, li_y, _ = self._hp_get(self._hp_key(c, "Inf In"))
        lo_x, lo_y, _ = self._hp_get(self._hp_key(c, "Inf Out"))
        return (
            models.Point2D(ui_x, ui_y),
            models.Point2D(uo_x, uo_y),
            models.Point2D(li_x, li_y),
            models.Point2D(lo_x, lo_y),
        )

    def _make_geo2d_for_corner(self, corner: str):
        """Monta um SuspensionGeometry2D para um canto específico, reaproveitável."""
        upper_in, upper_out, lower_in, lower_out = self._corner_points_2d(corner)
        return models.SuspensionGeometry2D(
            track_width=self._read("bf"),
            upper_in=upper_in,
            upper_out=upper_out,
            lower_in=lower_in,
            lower_out=lower_out,
            s1=self._read("s1"),
            s2=self._read("s2"),
            camber_out_deg=self._read("cam_o"),
            camber_in_deg=self._read("cam_i"),
        )

    def _safe_call(self, fn, name: str):
        """Executa uma ação com captura de erro sem quebrar a sequência."""
        try:
            fn()
            return True
        except Exception as e:
            self._set_status(f"Erro em {name}: {e}")
            try:
                messagebox.showerror(f"Erro em {name}", str(e))
            except Exception:
                pass
            return False

    def _calc_all(self):
        ok2d = self._safe_call(self._calc_2d, "Calcular 2D")
        ok3d = self._safe_call(self._calc_3d, "Calcular 3D")
        okvis = self._safe_call(self._calc_full_3d, "Visual 3D")
        okcg = self._safe_call(self._run_cg_sweep, "CG Sweep")
        okm = self._safe_call(self._run_mass_sweep, "Mass Sweep")
        okopt = self._safe_call(self._run_opt, "Estrutura")

        if all([ok2d, ok3d, okvis, okcg, okm, okopt]):
            self._set_status("✅ Calcular TODOS concluído.")
        else:
            self._set_status("⚠️ Calcular TODOS terminou com alguns erros (veja mensagens).")

    def _goto_tab(self, tab_name: str):
        tabs = {
            "2D": getattr(self, "tab_2d", None),
            "3D": getattr(self, "tab_3d", None),
            "VIS3D": getattr(self, "tab_vis3d", None),
            "DYN": getattr(self, "tab_dyn", None),
            "OPT": getattr(self, "tab_opt", None),
            "EQ": getattr(self, "tab_eq", None),
        }
        t = tabs.get(tab_name)
        if t is not None:
            self.content_area.select(t)

    def _hp_key(self, corner: str, point: str) -> str:
        return f"{corner} {point}"

    def _hp_get(self, lbl: str):
        if lbl not in self.entries_hp:
            raise KeyError(f"Hardpoint não encontrado: {lbl}")
        ex, ey, ez = self.entries_hp[lbl]
        return safe_num(ex.get()), safe_num(ey.get()), safe_num(ez.get())

    def _hp_set(self, lbl: str, x: float, y: float, z: float):
        if lbl not in self.entries_hp:
            raise KeyError(f"Hardpoint não encontrado: {lbl}")
        ex, ey, ez = self.entries_hp[lbl]
        ex.delete(0, tk.END); ex.insert(0, str(x))
        ey.delete(0, tk.END); ey.insert(0, str(y))
        ez.delete(0, tk.END); ez.insert(0, str(z))

    def _export_hardpoints_csv(self):
        path = filedialog.asksaveasfilename(defaultextension=".csv", filetypes=[("CSV", "*.csv")])
        if not path:
            return
        try:
            rows = []
            rows.append(["bf", self.entries["bf"].get(), "", ""])
            for lbl in getattr(self, "hp_keys", []):
                x, y, z = self._hp_get(lbl)
                rows.append([lbl, f"{x}", f"{y}", f"{z}"])

            with open(path, "w", newline="", encoding="utf-8") as f:
                w = csv.writer(f, delimiter=",")
                w.writerow(["key", "x", "y", "z"])
                w.writerows(rows)

            self._set_status("Hardpoints exportados para CSV.")
        except Exception as e:
            self._set_status(f"Erro export CSV: {e}")
            messagebox.showerror("Erro", str(e))

    def _import_hardpoints_csv(self):
        path = filedialog.askopenfilename(filetypes=[("CSV", "*.csv")])
        if not path:
            return
        try:
            got_any = False
            with open(path, "r", encoding="utf-8") as f:
                rdr = csv.DictReader(f)
                for row in rdr:
                    key = (row.get("key") or "").strip()
                    if not key:
                        continue
                    if key == "bf":
                        bf_val = row.get("x", "")
                        if bf_val:
                            self._set("bf", safe_num(bf_val))
                            got_any = True
                        continue
                    if key in self.entries_hp:
                        x = safe_num(row.get("x", "0"))
                        y = safe_num(row.get("y", "0"))
                        z = safe_num(row.get("z", "0"))
                        self._hp_set(key, x, y, z)
                        got_any = True

            if got_any:
                self._set_status("Hardpoints importados do CSV.")
            else:
                self._set_status("CSV lido, mas não encontrou chaves válidas.")
                messagebox.showwarning("Aviso", "Não encontrei chaves (key) válidas no CSV.")
        except Exception as e:
            self._set_status(f"Erro import CSV: {e}")
            messagebox.showerror("Erro", str(e))

    def _compute_arm_angles_deg(self, corner: str):
        sup_in_x, sup_in_y, _ = self._hp_get(self._hp_key(corner, "Sup In"))
        sup_out_x, sup_out_y, _ = self._hp_get(self._hp_key(corner, "Sup Out"))
        inf_in_x, inf_in_y, _ = self._hp_get(self._hp_key(corner, "Inf In"))
        inf_out_x, inf_out_y, _ = self._hp_get(self._hp_key(corner, "Inf Out"))

        ang_sup = math.degrees(math.atan2((sup_out_y - sup_in_y), (sup_out_x - sup_in_x)))
        ang_inf = math.degrees(math.atan2((inf_out_y - inf_in_y), (inf_out_x - inf_in_x)))
        return ang_sup, ang_inf

    # -------------------------
    # Init
    # -------------------------
    def __init__(self, root: tk.Tk):
        self.root = root
        self.root.title("Suíte de Engenharia de Suspensão (2D/3D/Visual/Dinâmica/Estrutura)")
        self.root.geometry("1400x900")

        # Estado: 3D view
        self.free_view = False
        self._view_lock_cid = None

        # Estado: resultados
        self.last_2d_results = None
        self.last_3d_results = None

        # Entradas
        self.entries = {}
        self.entries_hp = {}
        self.hp_keys = []

        # Corner usado nos cálculos 2D/3D/Opt
        self.var_corner = tk.StringVar(value="FR")

        # UI
        self._setup_style()
        self._build_topbar()

        self.paned = tk.PanedWindow(root, orient="horizontal", sashwidth=6, bg="#b0b0b0")
        self.paned.pack(fill="both", expand=True)

        self.sidebar_container = ScrollableFrame(self.paned, width=410)
        self.paned.add(self.sidebar_container, minsize=400, width=420)

        self.content_area = ttk.Notebook(self.paned)
        self.paned.add(self.content_area, minsize=700)

        self._init_sidebar_inputs(self.sidebar_container.scrollable_window)
        self._init_result_tabs()
        self._build_statusbar()

    # -------------------------
    # Style
    # -------------------------
    def _setup_style(self):
        style = ttk.Style()
        style.theme_use("clam")

        style.configure("TLabelFrame", font=("Arial", 10, "bold"))
        style.configure("TButton", font=("Arial", 9, "bold"), padding=6)
        style.configure("Action.TButton", foreground="#1f5cff")
        style.configure("Reset.TButton", foreground="#b00020")

        style.configure("Top.TFrame", padding=8)
        style.configure("Section.TCheckbutton", font=("Arial", 10, "bold"))
        style.configure("Card.TFrame", padding=10, relief="solid", borderwidth=1)

        style.configure("Tip.TFrame", relief="solid", borderwidth=1)
        style.configure("Tip.TLabel", font=("Arial", 9))

        style.configure("Status.TLabel", font=("Arial", 9))
        style.configure("Header.TLabel", font=("Arial", 11, "bold"))

    # -------------------------
    # Topbar + Statusbar
    # -------------------------
    def _build_topbar(self):
        bar = ttk.Frame(self.root, style="Top.TFrame")
        bar.pack(fill="x")

        ttk.Label(bar, text="Ações rápidas", style="Header.TLabel").pack(side="left", padx=(6, 16))

        ttk.Button(bar, text="Calcular 2D", style="Action.TButton", command=self._calc_2d).pack(side="left", padx=4)
        ttk.Button(bar, text="Calcular 3D", style="Action.TButton", command=self._calc_3d).pack(side="left", padx=4)
        ttk.Button(bar, text="Visual 3D", style="Action.TButton", command=self._calc_full_3d).pack(side="left", padx=4)
        ttk.Button(bar, text="CG Sweep", command=self._run_cg_sweep).pack(side="left", padx=4)
        ttk.Button(bar, text="Mass Sweep", command=self._run_mass_sweep).pack(side="left", padx=4)
        ttk.Button(bar, text="Estrutura", command=self._run_opt).pack(side="left", padx=4)

        ttk.Separator(bar, orient="vertical").pack(side="left", fill="y", padx=10)

        ttk.Button(bar, text="Salvar preset", command=self._save_preset).pack(side="left", padx=4)
        ttk.Button(bar, text="Carregar preset", command=self._load_preset).pack(side="left", padx=4)
        ttk.Button(bar, text="Reset defaults", command=self._reset_defaults).pack(side="left", padx=4)

        right = ttk.Frame(bar)
        right.pack(side="right", padx=8)

        btn_calc = ttk.Menubutton(right, text="Calcular ▾", style="Action.TButton")
        m_calc = tk.Menu(btn_calc, tearoff=0)
        btn_calc["menu"] = m_calc

        m_calc.add_command(label="Calcular TODOS", command=self._calc_all)
        m_calc.add_separator()
        m_calc.add_command(label="Calcular 2D", command=self._calc_2d)
        m_calc.add_command(label="Calcular 3D", command=self._calc_3d)
        m_calc.add_command(label="Calcular Visual 3D", command=self._calc_full_3d)
        m_calc.add_separator()
        m_calc.add_command(label="CG Sweep", command=self._run_cg_sweep)
        m_calc.add_command(label="Mass Sweep", command=self._run_mass_sweep)
        m_calc.add_separator()
        m_calc.add_command(label="Estrutura", command=self._run_opt)

        btn_calc.pack(side="left", padx=(0, 6))

        btn_vis = ttk.Menubutton(right, text="Visual ▾")
        m_vis = tk.Menu(btn_vis, tearoff=0)
        btn_vis["menu"] = m_vis

        m_vis.add_command(label="Ir para Resultados 2D", command=lambda: self._goto_tab("2D"))
        m_vis.add_command(label="Ir para Resultados 3D", command=lambda: self._goto_tab("3D"))
        m_vis.add_command(label="Ir para Visualização 3D", command=lambda: self._goto_tab("VIS3D"))
        m_vis.add_command(label="Ir para Dinâmica Veicular", command=lambda: self._goto_tab("DYN"))
        m_vis.add_command(label="Ir para Otimização", command=lambda: self._goto_tab("OPT"))
        m_vis.add_command(label="Ir para Equilíbrio (Matriz)", command=lambda: self._goto_tab("EQ"))

        btn_vis.pack(side="left")

    def _build_statusbar(self):
        self.status_var = tk.StringVar(value="Pronto.")
        bar = ttk.Frame(self.root)
        bar.pack(fill="x", side="bottom")
        ttk.Label(bar, textvariable=self.status_var, style="Status.TLabel").pack(side="left", padx=8, pady=3)

    def _set_status(self, msg: str):
        if hasattr(self, "status_var"):
            self.status_var.set(msg)

    # -------------------------
    # Entry builders
    # -------------------------
    def _create_entry(self, parent, key, label, default, row, col, width=10, tip=None):
        ttk.Label(parent, text=label).grid(row=row, column=col, sticky="e", padx=3, pady=2)
        e = ttk.Entry(parent, width=width)
        e.insert(0, str(default))
        e.grid(row=row, column=col + 1, sticky="w", padx=3, pady=2)
        self.entries[key] = e
        if tip:
            ToolTip(e, tip)
        return e

    def _read(self, key) -> float:
        if key not in self.entries:
            raise KeyError(f"Campo não encontrado: {key}")
        return safe_num(self.entries[key].get())

    def _set(self, key, value):
        e = self.entries.get(key)
        if not e:
            return
        e.delete(0, tk.END)
        e.insert(0, str(value))

    # -------------------------
    # Sidebar Sections
    # -------------------------
    def _init_sidebar_inputs(self, parent):
        sec_geo = CollapsibleSection(parent, "1) Geometria (Hardpoints) [mm]")
        sec_geo.pack(fill="x", padx=6, pady=6)
        f_geo = sec_geo.body

        self._create_entry(f_geo, "bf", "Bitola (bf):", 1250, 0, 0, width=12,
                           tip="Distância entre centros das rodas (track). Use mm.")

        ttk.Label(f_geo, text="Canto p/ cálculo:").grid(row=0, column=2, sticky="e", padx=3, pady=2)
        cb = ttk.Combobox(f_geo, values=["FR", "FL", "RR", "RL"], textvariable=self.var_corner, width=6, state="readonly")
        cb.grid(row=0, column=3, sticky="w", padx=3, pady=2)
        ToolTip(cb, "Define qual canto (FR/FL/RR/RL) será usado nos cálculos 2D/3D e ângulos automáticos.")

        ttk.Label(f_geo, text="X").grid(row=1, column=1)
        ttk.Label(f_geo, text="Y").grid(row=1, column=2)
        ttk.Label(f_geo, text="Z").grid(row=1, column=3)

        corners = ["FR", "FL", "RR", "RL"]
        pts = ["Sup In", "Sup Out", "Inf In", "Inf Out"]

        base_defaults = {
            "Sup In":  (450, 420, 200),
            "Sup Out": (625, 390, 300),
            "Inf In":  (430, 210, 200),
            "Inf Out": (625, 190, 300),
        }

        def mirror_x(x):
            return -x

        r = 2
        self.hp_keys = []

        for corner in corners:
            ttk.Label(f_geo, text=f"== {corner} ==", font=("Arial", 9, "bold"))\
                .grid(row=r, column=0, columnspan=4, sticky="w", pady=(10, 2))
            r += 1

            for p in pts:
                lbl = self._hp_key(corner, p)
                dx, dy, dz = base_defaults[p]
                if corner in ("FL", "RL"):
                    dx = mirror_x(dx)

                ttk.Label(f_geo, text=lbl + ":").grid(row=r, column=0, sticky="e")
                ex = ttk.Entry(f_geo, width=8); ex.insert(0, str(dx)); ex.grid(row=r, column=1)
                ey = ttk.Entry(f_geo, width=8); ey.insert(0, str(dy)); ey.grid(row=r, column=2)
                ez = ttk.Entry(f_geo, width=8); ez.insert(0, str(dz)); ez.grid(row=r, column=3)

                self.entries_hp[lbl] = (ex, ey, ez)
                self.hp_keys.append(lbl)

                ToolTip(ex, f"{lbl} X (mm)")
                ToolTip(ey, f"{lbl} Y (mm)")
                ToolTip(ez, f"{lbl} Z (mm)")
                r += 1

        frm_csv = ttk.Frame(f_geo)
        frm_csv.grid(row=r, column=0, columnspan=4, sticky="ew", pady=(10, 2))
        ttk.Button(frm_csv, text="Exportar Hardpoints CSV", command=self._export_hardpoints_csv).pack(side="left", padx=4)
        ttk.Button(frm_csv, text="Importar Hardpoints CSV", command=self._import_hardpoints_csv).pack(side="left", padx=4)

        sec_kin = CollapsibleSection(parent, "2) Cinemática & Alinhamento")
        sec_kin.pack(fill="x", padx=6, pady=6)
        f_kin = sec_kin.body

        self._create_entry(f_kin, "s1", "Curso Ext (mm):", 40, 0, 0)
        self._create_entry(f_kin, "s2", "Curso Int (mm):", 30, 0, 2)
        self._create_entry(f_kin, "cam_o", "Camber Ext (°):", -1.5, 1, 0)
        self._create_entry(f_kin, "cam_i", "Camber Int (°):", -4.0, 1, 2)

        ttk.Label(f_kin, text="Auxiliares 3D").grid(row=2, column=0, columnspan=4, pady=(8, 2), sticky="w")
        self._create_entry(f_kin, "spindle_sup_z", "Spindle Sup Z:", 300, 3, 0)
        self._create_entry(f_kin, "spindle_inf_z", "Spindle Inf Z:", 300, 3, 2)
        self._create_entry(f_kin, "toe_f_x", "Toe Front X:", 600, 4, 0)
        self._create_entry(f_kin, "toe_r_x", "Toe Rear X:", 600, 4, 2)

        sec_dyn = CollapsibleSection(parent, "3) Dinâmica Veicular (Baja)")
        sec_dyn.pack(fill="x", padx=6, pady=6)
        f_dyn = sec_dyn.body

        self._create_entry(f_dyn, "mass", "Massa (kg):", 200, 0, 0)
        self._create_entry(f_dyn, "hcg", "Altura CG (mm):", 450, 0, 2)
        self._create_entry(f_dyn, "ay", "Ay (m/s²):", 9.8, 1, 0)
        self._create_entry(f_dyn, "wb", "Entre-eixos (mm):", 1550, 1, 2)
        self._create_entry(f_dyn, "fx", "Fx Pneu (N):", 1200, 2, 0)
        self._create_entry(f_dyn, "rs", "Scrub (mm):", 50, 2, 2)

        ttk.Label(f_dyn, text="Varredura (Simulação)").grid(row=3, column=0, columnspan=4, pady=(8, 2), sticky="w")
        self._create_entry(f_dyn, "h_min", "CG Min:", 250, 4, 0)
        self._create_entry(f_dyn, "h_max", "CG Max:", 650, 4, 2)
        self._create_entry(f_dyn, "m_min", "Massa Min:", 180, 5, 0)
        self._create_entry(f_dyn, "m_max", "Massa Max:", 240, 5, 2)
        self._create_entry(f_dyn, "step", "Passo:", 25, 6, 0)

        sec_opt = CollapsibleSection(parent, "4) Otimização Estrutural")
        sec_opt.pack(fill="x", padx=6, pady=6)
        f_opt = sec_opt.body

        self._create_entry(f_opt, "load", "Carga F [N]:", 3000, 0, 0)
        self.var_static = tk.IntVar(value=0)
        ttk.Checkbutton(f_opt, text="+ Estática (mg/4)", variable=self.var_static)\
            .grid(row=0, column=2, columnspan=2, sticky="w")

        self.var_auto_arm_angles = tk.IntVar(value=1)
        ttk.Checkbutton(
            f_opt,
            text="Usar ângulos automáticos (do canto selecionado)",
            variable=self.var_auto_arm_angles
        ).grid(row=1, column=0, columnspan=4, sticky="w", pady=(2, 6))

        self._create_entry(f_opt, "ang_sup", "Ang Base Sup:", 10, 2, 0)
        self._create_entry(f_opt, "ang_inf", "Ang Base Inf:", 20, 2, 2)
        self._create_entry(f_opt, "limit", "Limite (N):", 8000, 3, 0)
        self._create_entry(f_opt, "ksup", "K Sup:", 0.5, 3, 2)
        self._create_entry(f_opt, "amin", "Varredura Min:", 0, 4, 0)
        self._create_entry(f_opt, "amax", "Varredura Max:", 40, 4, 2)

    # -------------------------
    # Tabs
    # -------------------------
    def _init_result_tabs(self):
        self.tab_2d = ttk.Frame(self.content_area)
        self.tab_3d = ttk.Frame(self.content_area)
        self.tab_vis3d = ttk.Frame(self.content_area)
        self.tab_dyn = ttk.Frame(self.content_area)
        self.tab_opt = ttk.Frame(self.content_area)
        self.tab_eq = ttk.Frame(self.content_area)

        self.content_area.add(self.tab_2d, text="Resultados 2D")
        self.content_area.add(self.tab_3d, text="Resultados 3D")
        self.content_area.add(self.tab_vis3d, text="Visualização 3D")
        self.content_area.add(self.tab_dyn, text="Dinâmica Veicular")
        self.content_area.add(self.tab_opt, text="Otimização")
        self.content_area.add(self.tab_eq, text="Equilíbrio (Matriz)")

        self._setup_res_2d(self.tab_2d)
        self._setup_res_3d(self.tab_3d)
        self._setup_res_vis3d(self.tab_vis3d)
        self._setup_res_dyn(self.tab_dyn)
        self._setup_res_opt(self.tab_opt)
        self._setup_res_eq(self.tab_eq)

    def _setup_res_2d(self, parent):
        card = ttk.Frame(parent, style="Card.TFrame")
        card.pack(fill="x", padx=12, pady=10)

        ttk.Label(card, text="Principais resultados", style="Header.TLabel").pack(anchor="w")
        self.lbl_2d_res = ttk.Label(card, text="Calcule para ver resultados...", font=("Courier", 11), justify="left")
        self.lbl_2d_res.pack(anchor="w", pady=(6, 0))

        self.fig_2d, self.ax_2d = plt.subplots(figsize=(6, 5))
        self.canvas_2d = FigureCanvasTkAgg(self.fig_2d, master=parent)
        self.canvas_2d.get_tk_widget().pack(fill="both", expand=True, padx=12, pady=8)

        txt = tk.Text(parent, height=6, bg="#f0f0f0", font=("Arial", 9))
        txt.insert("1.0", getattr(ui_strings, "SUMMARY_2D", ""))
        txt.config(state="disabled")
        txt.pack(fill="x", side="bottom")

    def _setup_res_3d(self, parent):
        card = ttk.Frame(parent, style="Card.TFrame")
        card.pack(fill="x", padx=12, pady=10)

        ttk.Label(card, text="Forças e alinhamento", style="Header.TLabel").pack(anchor="w")
        self.lbl_3d_res = ttk.Label(card, text="Calcule para ver resultados...", font=("Courier", 11), justify="left")
        self.lbl_3d_res.pack(anchor="w", pady=(6, 0))

        txt = tk.Text(parent, height=10, bg="#f0f0f0", font=("Arial", 9))
        txt.insert("1.0", getattr(ui_strings, "SUMMARY_3D", ""))
        txt.config(state="disabled")
        txt.pack(fill="x", side="bottom")

    def _setup_res_vis3d(self, parent):
        ctrl = ttk.Frame(parent, padding=8)
        ctrl.pack(fill="x")

        ttk.Button(ctrl, text="GERAR DIAGRAMA 3D", style="Action.TButton",
                   command=self._calc_full_3d).pack(side="left", fill="x", expand=True, padx=2)

        ttk.Button(ctrl, text="Resetar Visualização", style="Reset.TButton",
                   command=self._reset_view).pack(side="right", padx=2)

        self.btn_free_view = ttk.Button(ctrl, text="Visualização: FIXA (YZ)", command=self._toggle_free_view)
        self.btn_free_view.pack(side="right", padx=8)

        self.lbl_steer_data = ttk.Label(parent, text="Dados: --", font=("Courier", 10))
        self.lbl_steer_data.pack(pady=5)

        self.fig_vis3d = plt.figure(figsize=(6, 5))
        self.ax_vis3d = self.fig_vis3d.add_subplot(111, projection="3d")

        self.canvas_vis3d = FigureCanvasTkAgg(self.fig_vis3d, master=parent)
        self.canvas_vis3d.get_tk_widget().pack(fill="both", expand=True, padx=10, pady=(0, 0))

        toolbar = NavigationToolbar2Tk(self.canvas_vis3d, parent)
        toolbar.update()
        toolbar.pack(fill="x", padx=10, pady=(0, 8))

        self._apply_view_mode()
        self._reset_view()

    def _setup_res_dyn(self, parent):
        nb = ttk.Notebook(parent)
        nb.pack(fill="both", expand=True)

        f_cg = ttk.Frame(nb)
        nb.add(f_cg, text="Varredura de CG")

        self.fig_cg, (self.ax_cg1, self.ax_cg2) = plt.subplots(1, 2, figsize=(8, 3.5))
        self.canvas_cg = FigureCanvasTkAgg(self.fig_cg, master=f_cg)
        self.canvas_cg.get_tk_widget().pack(fill="both", expand=True, padx=10, pady=8)

        cols_cg = ("CG", "dFz", "Fz_int", "M_Roll")
        self.tree_cg = ttk.Treeview(f_cg, columns=cols_cg, show="headings", height=7)
        for c in cols_cg:
            self.tree_cg.heading(c, text=c)
            self.tree_cg.column(c, width=110, anchor="center")
        self.tree_cg.pack(fill="x", padx=10, pady=(0, 10))

        f_mass = ttk.Frame(nb)
        nb.add(f_mass, text="Varredura de Massa")

        cols_m = ("Massa", "M_Roll", "dFz", "SSF", "Ay_Crit", "Margem")
        self.tree_mass = ttk.Treeview(f_mass, columns=cols_m, show="headings")
        for c in cols_m:
            self.tree_mass.heading(c, text=c)
            self.tree_mass.column(c, width=120, anchor="center")
        self.tree_mass.pack(fill="both", expand=True, padx=10, pady=10)

        txt = tk.Text(parent, height=5, bg="#f0f0f0", font=("Arial", 9))
        txt.insert("1.0", getattr(ui_strings, "SUMMARY_BAJA", ""))
        txt.config(state="disabled")
        txt.pack(fill="x", side="bottom")

    def _setup_res_opt(self, parent):
        paned = tk.PanedWindow(parent, orient="vertical")
        paned.pack(fill="both", expand=True, padx=12, pady=10)

        f_tab = ttk.Frame(paned)
        cols = ("dAngle", "F_Sup", "F_Inf", "Status")
        self.tree_opt = ttk.Treeview(f_tab, columns=cols, show="headings")
        for c in cols:
            self.tree_opt.heading(c, text=c)
            self.tree_opt.column(c, width=120, anchor="center")
        self.tree_opt.pack(fill="both", expand=True)
        paned.add(f_tab, minsize=180)

        f_graph = ttk.Frame(paned)
        self.fig_opt, self.ax_opt = plt.subplots(figsize=(6, 4))
        self.canvas_opt = FigureCanvasTkAgg(self.fig_opt, master=f_graph)
        self.canvas_opt.get_tk_widget().pack(fill="both", expand=True)
        paned.add(f_graph)

    # ============================================================
    # ABA NOVA: Equilíbrio (Matriz)
    # ============================================================
    def _setup_res_eq(self, parent):
        self.eq_points = None
        self.eq_members = DEFAULT_MEMBERS

        top = ttk.Frame(parent, padding=10)
        top.pack(fill="x")

        ttk.Label(top, text="Arquivo (.ipynb):").grid(row=0, column=0, sticky="e", padx=4, pady=4)
        self.eq_ipynb_path = tk.StringVar(value="")
        ttk.Entry(top, textvariable=self.eq_ipynb_path, width=70).grid(row=0, column=1, sticky="ew", padx=4, pady=4)

        ttk.Button(top, text="Selecionar...", command=self._eq_pick_ipynb).grid(row=0, column=2, padx=4, pady=4)
        ttk.Button(top, text="Carregar pontos", style="Action.TButton", command=self._eq_load_points).grid(row=0, column=3, padx=4, pady=4)

        ttk.Label(top, text="Momento em:").grid(row=1, column=0, sticky="e", padx=4, pady=4)
        self.eq_moment_point = tk.StringVar(value="O")
        self.eq_moment_cb = ttk.Combobox(top, textvariable=self.eq_moment_point, values=["O"], state="readonly", width=10)
        self.eq_moment_cb.grid(row=1, column=1, sticky="w", padx=4, pady=4)

        ttk.Label(top, text="Fx (N):").grid(row=1, column=2, sticky="e", padx=4, pady=4)
        self.eq_fx = ttk.Entry(top, width=10); self.eq_fx.insert(0, "1000")
        self.eq_fx.grid(row=1, column=3, sticky="w", padx=4, pady=4)

        ttk.Label(top, text="Fy (N):").grid(row=1, column=4, sticky="e", padx=4, pady=4)
        self.eq_fy = ttk.Entry(top, width=10); self.eq_fy.insert(0, "-1000")
        self.eq_fy.grid(row=1, column=5, sticky="w", padx=4, pady=4)

        ttk.Label(top, text="Fz (N):").grid(row=1, column=6, sticky="e", padx=4, pady=4)
        self.eq_fz = ttk.Entry(top, width=10); self.eq_fz.insert(0, "1000")
        self.eq_fz.grid(row=1, column=7, sticky="w", padx=4, pady=4)

        ttk.Button(top, text="Montar A + Resolver", style="Action.TButton", command=self._eq_solve)\
            .grid(row=1, column=8, padx=8, pady=4)

        top.grid_columnconfigure(1, weight=1)

        main = tk.PanedWindow(parent, orient="horizontal")
        main.pack(fill="both", expand=True, padx=10, pady=(0, 10))

        left = ttk.Frame(main, padding=6)
        self.eq_txt = tk.Text(left, font=("Courier", 10))
        self.eq_txt.pack(fill="both", expand=True)
        main.add(left, minsize=420)

        right = ttk.Frame(main, padding=6)
        self.eq_fig = plt.figure(figsize=(6, 5))
        self.eq_ax = self.eq_fig.add_subplot(111, projection="3d")
        self.eq_canvas = FigureCanvasTkAgg(self.eq_fig, master=right)
        self.eq_canvas.get_tk_widget().pack(fill="both", expand=True)

        tb = NavigationToolbar2Tk(self.eq_canvas, right)
        tb.update()
        tb.pack(fill="x")

        main.add(right)

        self._eq_print("Aba Equilíbrio pronta. Selecione o .ipynb e clique em 'Carregar pontos'.")

    def _eq_print(self, msg: str):
        self.eq_txt.insert("end", msg + "\n")
        self.eq_txt.see("end")

    def _eq_pick_ipynb(self):
        path = filedialog.askopenfilename(filetypes=[("Jupyter Notebook", "*.ipynb")])
        if path:
            self.eq_ipynb_path.set(path)

    def _eq_load_points(self):
        path = (self.eq_ipynb_path.get() or "").strip()
        if not path:
            messagebox.showwarning("Aviso", "Selecione um arquivo .ipynb primeiro.")
            return

        try:
            self.eq_points = extract_points_from_ipynb(path)
            keys = sorted(self.eq_points.keys())

            self.eq_moment_cb["values"] = keys
            if self.eq_moment_point.get() not in self.eq_points:
                self.eq_moment_point.set("O" if "O" in self.eq_points else keys[0])

            self.eq_txt.delete("1.0", "end")
            self._eq_print(f"Pontos carregados: {len(keys)}")
            self._eq_print("Hardpoints: " + ", ".join(keys))

            self._eq_plot_geometry()
            self._set_status("Equilíbrio: hardpoints carregados.")
            self.content_area.select(self.tab_eq)

        except Exception as e:
            self._set_status(f"Erro ao carregar pontos: {e}")
            messagebox.showerror("Erro", str(e))

    def _eq_solve(self):
        if not self.eq_points:
            messagebox.showwarning("Aviso", "Carregue os pontos do .ipynb primeiro.")
            return

        try:
            Fx = safe_num(self.eq_fx.get())
            Fy = safe_num(self.eq_fy.get())
            Fz = safe_num(self.eq_fz.get())
            moment_about = (self.eq_moment_point.get() or "O").strip()

            A = build_A_matrix(self.eq_points, self.eq_members, moment_about=moment_about, length_unit="mm")
            B = build_B_vector(Fx, Fy, Fz)
            reactions = solve_reactions(A, B, self.eq_members)

            self.eq_txt.delete("1.0", "end")
            self._eq_print(f"Momento em: {moment_about}")
            self._eq_print(f"Força F = [{Fx:.3f}, {Fy:.3f}, {Fz:.3f}] N\n")
            self._eq_print("=== Matriz A (6x6) ===")
            self._eq_print(np.array2string(A, precision=6, suppress_small=True))
            self._eq_print("\n=== Vetor B (6x1) ===")
            self._eq_print(np.array2string(B, precision=6, suppress_small=True))
            self._eq_print("\n=== Reações (N) ===")
            for m in self.eq_members:
                self._eq_print(f"{m.name}: {reactions[m.name]: .6f}")

            self._eq_plot_geometry(reactions=reactions)
            self._set_status("Equilíbrio resolvido.")
            self.content_area.select(self.tab_eq)

        except Exception as e:
            self._set_status(f"Erro Equilíbrio: {e}")
            messagebox.showerror("Erro Equilíbrio", str(e))

    def _eq_plot_geometry(self, reactions: Dict[str, float] = None):
        ax = self.eq_ax
        ax.clear()

        pts = self.eq_points or {}
        if not pts:
            self.eq_canvas.draw_idle()
            return

        xs = [p[0] for p in pts.values()]
        ys = [p[1] for p in pts.values()]
        zs = [p[2] for p in pts.values()]

        # Mantém seu padrão visual: (X, Z, Y)
        ax.scatter(xs, zs, ys, s=30)

        for name, p in pts.items():
            ax.text(p[0], p[2], p[1] + 5, name, fontsize=8)

        for mem in self.eq_members:
            if mem.from_point in pts and mem.to_point in pts:
                p1 = pts[mem.from_point]
                p2 = pts[mem.to_point]
                ax.plot([p1[0], p2[0]], [p1[2], p2[2]], [p1[1], p2[1]], linewidth=2)
                if reactions and mem.name in reactions:
                    mid = 0.5 * (p1 + p2)
                    ax.text(mid[0], mid[2], mid[1], f"{mem.name}={reactions[mem.name]:.0f}N", fontsize=8)

        ax.set_title("Hardpoints + Membros (Equilíbrio)")
        ax.set_xlabel("X (lateral)")
        ax.set_ylabel("Z (longitudinal)")
        ax.set_zlabel("Y (vertical)")
        ax.grid(True)

        self.eq_canvas.draw_idle()

    # =========================
    # 3D view mode controls
    # =========================
    def _toggle_free_view(self):
        self.free_view = not self.free_view
        self._apply_view_mode()

        if self.free_view:
            self.btn_free_view.config(text="Visualização: LIVRE (3D)")
            self._set_status("Visualização livre ativada (rotação habilitada).")
        else:
            self.btn_free_view.config(text="Visualização: FIXA (YZ)")
            self._reset_view()
            self._set_status("Visualização fixa ativada (plano YZ, sem rotação).")

    def _apply_view_mode(self):
        if not hasattr(self, "ax_vis3d") or not hasattr(self, "canvas_vis3d"):
            return

        if self._view_lock_cid is not None:
            try:
                self.canvas_vis3d.mpl_disconnect(self._view_lock_cid)
            except Exception:
                pass
            self._view_lock_cid = None

        if self.free_view:
            try:
                self.ax_vis3d.mouse_init(rotate_btn=1, zoom_btn=3)
            except Exception:
                pass
        else:
            try:
                self.ax_vis3d.mouse_init(rotate_btn=None, zoom_btn=3)
            except Exception:
                pass

            self._view_lock_cid = self.canvas_vis3d.mpl_connect(
                "button_release_event",
                lambda evt: self._enforce_yz_view()
            )

    def _enforce_yz_view(self):
        if self.free_view:
            return
        if not hasattr(self, "ax_vis3d") or not hasattr(self, "canvas_vis3d"):
            return

        try:
            dist = self.ax_vis3d.dist
        except Exception:
            dist = None

        self.ax_vis3d.view_init(elev=0, azim=0)

        if dist is not None:
            try:
                self.ax_vis3d.dist = dist
            except Exception:
                pass

        self.canvas_vis3d.draw_idle()

    def _reset_view(self):
        if not hasattr(self, "ax_vis3d") or not hasattr(self, "canvas_vis3d"):
            return
        if self.free_view:
            self.ax_vis3d.view_init(elev=20, azim=-60)
        else:
            self.ax_vis3d.view_init(elev=0, azim=0)
        self.canvas_vis3d.draw_idle()

    # =========================
    # Domain gathering
    # =========================
    def _get_point3d_from_input(self, label: str):
        x, y, z = self._hp_get(label)
        return models.Point3D(x, y, z)

    def _get_geo2d(self):
        corner = self.var_corner.get().strip() or "FR"
        return self._make_geo2d_for_corner(corner)

    def _get_geo3d(self):
        corner = self.var_corner.get().strip() or "FR"

        sup_in = self._get_point3d_from_input(self._hp_key(corner, "Sup In"))
        sup_out = self._get_point3d_from_input(self._hp_key(corner, "Sup Out"))
        inf_in = self._get_point3d_from_input(self._hp_key(corner, "Inf In"))
        inf_out = self._get_point3d_from_input(self._hp_key(corner, "Inf Out"))

        sup_out_x, sup_out_y, _ = self._hp_get(self._hp_key(corner, "Sup Out"))
        inf_out_x, inf_out_y, _ = self._hp_get(self._hp_key(corner, "Inf Out"))

        return models.SuspensionGeometry3D(
            sup_in=sup_in,
            sup_out=sup_out,
            inf_in=inf_in,
            inf_out=inf_out,
            spindle_sup=models.Point3D(sup_out_x, sup_out_y, self._read("spindle_sup_z")),
            spindle_inf=models.Point3D(inf_out_x, inf_out_y, self._read("spindle_inf_z")),
            toe_front=models.Point3D(self._read("toe_f_x"), 0, 0),
            toe_rear=models.Point3D(self._read("toe_r_x"), 0, 0),
            stiffness_ratio_sup=1.0,
            stiffness_ratio_inf=1.0,
            fx_tire=self._read("fx"),
        )

    # =========================
    # Actions: 2D / 3D / Visual / Dynamics / Opt
    # =========================
    def _calc_2d(self):
        try:
            corner_sel = (self.var_corner.get().strip() or "FR").upper()
            corner_R, corner_L = self._pair_corner(corner_sel)

            geoR = self._make_geo2d_for_corner(corner_R)
            geoL = self._make_geo2d_for_corner(corner_L)

            rcR = math_2d.calculate_roll_center(geoR)
            rcL = math_2d.calculate_roll_center(geoL)
            camR = math_2d.calculate_camber_gain(geoR)

            self.last_2d_results = rcR if corner_sel.endswith("R") else rcL

            def fmt_ic(rc):
                return f"({rc.ic.x:.1f}, {rc.ic.y:.1f})" if getattr(rc, "ic", None) else "Paralelo"

            def fmt_hro(rc):
                h = getattr(rc, "h_ro", None)
                return f"{h:.1f} mm" if h is not None else "--"

            self.lbl_2d_res.config(text=(
                f"Eixo: {corner_R[:-1]}  |  Canto selecionado: {corner_sel}\n"
                f"== DIREITA ({corner_R}) ==\n"
                f"IC: {fmt_ic(rcR)} mm\n"
                f"Altura RC (h_Ro): {fmt_hro(rcR)}\n"
                f"q: {getattr(rcR, 'q_factor', '--')}\n"
                f"\n== ESQUERDA ({corner_L}) ==\n"
                f"IC: {fmt_ic(rcL)} mm\n"
                f"Altura RC (h_Ro): {fmt_hro(rcL)}\n"
                f"q: {getattr(rcL, 'q_factor', '--')}\n"
                f"\nRolagem (dPhi - ref. lado R): {camR.d_phi_deg:.3f} deg\n"
                f"Fator Camber (kGamma - ref. lado R): {camR.k_gamma:.3f}"
            ))

            ax = self.ax_2d
            ax.clear()

            def plot_corner(geo, rc, label_prefix, color):
                ax.plot([geo.upper_in.x, geo.upper_out.x], [geo.upper_in.y, geo.upper_out.y],
                        marker="o", label=f"{label_prefix} Braço Sup", color=color, linestyle="--")
                ax.plot([geo.lower_in.x, geo.lower_out.x], [geo.lower_in.y, geo.lower_out.y],
                        marker="o", label=f"{label_prefix} Braço Inf", color=color, linestyle="-")

                if getattr(rc, "ic", None):
                    ax.plot(rc.ic.x, rc.ic.y, marker="s", linestyle="None", label=f"{label_prefix} IC", color=color)

                h_ro_val = getattr(rc, "h_ro", None)
                if h_ro_val is not None:
                    ax.plot(0, h_ro_val, marker="*", linestyle="None", markersize=12,
                            label=f"{label_prefix} RC", color=color)

            plot_corner(geoR, rcR, f"{corner_R}", "tab:blue")
            plot_corner(geoL, rcL, f"{corner_L}", "tab:red")

            ax.axvline(0, linestyle=":", linewidth=1)
            ax.set_title(f"Vista Frontal 2D (dois lados): {corner_R} + {corner_L}")
            ax.set_xlabel("X lateral (mm)")
            ax.set_ylabel("Y vertical (mm)")
            ax.grid(True)
            ax.legend(ncols=2, fontsize=8)
            ax.set_aspect("equal", adjustable="datalim")

            self.canvas_2d.draw()
            self._set_status("2D calculado (mostrando esquerda e direita).")
            self.content_area.select(self.tab_2d)

        except Exception as e:
            self._set_status(f"Erro 2D: {e}")
            messagebox.showerror("Erro 2D", str(e))

    def _calc_3d(self):
        try:
            geo3d = self._get_geo3d()
            forces = math_3d.calculate_forces(geo3d)
            self.last_3d_results = forces

            if forces:
                self.lbl_3d_res.config(text=(
                    f"Canto: {self.var_corner.get()}\n"
                    "== FORÇAS AXIAIS ==\n"
                    f"Sup: {forces.upper.axial:.1f} N\n"
                    f"Inf: {forces.lower.axial:.1f} N\n"
                ))
                self._set_status("3D calculado com sucesso.")
                self.content_area.select(self.tab_3d)
            else:
                self._set_status("Erro: geometria 3D inválida.")
                self.lbl_3d_res.config(text="Erro: Geometria 3D inválida.")
        except Exception as e:
            self._set_status(f"Erro 3D: {e}")
            messagebox.showerror("Erro 3D", str(e))

    def _calc_full_3d(self):
        try:
            wb = self._read("wb")
            bf = self._read("bf")
            rs = self._read("rs")

            fr_sup_in = self._get_point3d_from_input(self._hp_key("FR", "Sup In"))
            fr_sup_out = self._get_point3d_from_input(self._hp_key("FR", "Sup Out"))
            fr_inf_in = self._get_point3d_from_input(self._hp_key("FR", "Inf In"))
            fr_inf_out = self._get_point3d_from_input(self._hp_key("FR", "Inf Out"))

            fl_sup_in = self._get_point3d_from_input(self._hp_key("FL", "Sup In"))
            fl_sup_out = self._get_point3d_from_input(self._hp_key("FL", "Sup Out"))
            fl_inf_in = self._get_point3d_from_input(self._hp_key("FL", "Inf In"))
            fl_inf_out = self._get_point3d_from_input(self._hp_key("FL", "Inf Out"))

            geo = models.VehicleGeometry(
                wb, bf, bf,
                fr_sup_in, fr_sup_out,
                fr_inf_in, fr_inf_out,
                models.Point3D(self._read("toe_f_x") - 100, 200, 100),
                models.Point3D(self._read("toe_f_x"), 200, 100),
                rs * 3
            )

            ro = math_3d.calculate_3d_roll_center(geo)
            pts = math_3d.generate_full_car_plot_points(geo, ro)
            kpi = math_3d.calculate_kingpin_metrics(geo)

            self.lbl_steer_data.config(
                text=f"KPI: {kpi.kpi_deg:.1f}° | Scrub: {kpi.scrub_radius:.1f} mm\n"
                     f"Ro Local: ({ro.x:.1f}, {ro.y:.1f}, {ro.z:.1f})"
            )

            ax = self.ax_vis3d
            ax.clear()

            def plot_link(p1_name, p2_name, color, style='-', linewidth=2):
                if p1_name in pts and p2_name in pts:
                    p1 = pts[p1_name]
                    p2 = pts[p2_name]
                    ax.plot([p1.x, p2.x], [p1.z, p2.z], [p1.y, p2.y],
                            color=color, linestyle=style, linewidth=linewidth)

            def plot_projection(p_name):
                if p_name in pts:
                    p = pts[p_name]
                    ax.plot([p.x, p.x], [p.z, p.z], [p.y, 0],
                            color='gray', linestyle=':', linewidth=0.8, alpha=0.5)

            plot_link("FR Sup In", "FR Sup Out", "blue", "--")
            plot_link("FR Inf In", "FR Inf Out", "blue", "--")
            plot_link("FR Sup Out", "FR Inf Out", "black", "-")
            plot_link("FR Tie In", "FR Tie Out", "green", "--")

            plot_link("FL Sup In", "FL Sup Out", "red", "--")
            plot_link("FL Inf In", "FL Inf Out", "red", "--")
            plot_link("FL Sup Out", "FL Inf Out", "black", "-")

            for name, p in pts.items():
                c = 'red' if "Roll" in name else 'black' if "Wheel" in name else 'blue'
                m = '*' if "Roll" in name else 'o'
                ax.scatter(p.x, p.z, p.y, c=c, marker=m, s=40)
                ax.text(p.x, p.z, p.y + 10, name, fontsize=8, color='black')
                if "Wheel" not in name and "Roll" not in name:
                    plot_projection(name)

            ax.set_xlabel('X (Lateral)')
            ax.set_ylabel('Z (Longitudinal)')
            ax.set_zlabel('Y (Vertical)')
            ax.set_zlim(bottom=0)

            self.canvas_vis3d.draw_idle()
            if not self.free_view:
                self._enforce_yz_view()

            self._set_status("Visualização 3D gerada.")
            self.content_area.select(self.tab_vis3d)

        except Exception as e:
            self._set_status(f"Erro Visual 3D: {e}")
            messagebox.showerror("Erro", str(e))

    def _run_cg_sweep(self):
        try:
            h_ro = getattr(self.last_2d_results, "h_ro", None) if self.last_2d_results else None
            if h_ro is None:
                h_ro = 100.0

            params = models.SuspensionCGScanParameters(
                h_min=self._read("h_min"),
                h_max=self._read("h_max"),
                h_step=self._read("step"),
                mass=self._read("mass"),
                ay=self._read("ay"),
                track=self._read("bf"),
                h_ro=h_ro,
                scrub_radius=self._read("rs"),
                clearance=0.0
            )
            results = list(dynamics.calculate_baja_cg_sweep(params))

            for i in self.tree_cg.get_children():
                self.tree_cg.delete(i)
            for r in results:
                self.tree_cg.insert("", "end", values=(
                    f"{r.h_cg:.1f}", f"{r.d_fz:.1f}", f"{r.fz_int:.1f}", f"{r.m_roll:.1f}"
                ))

            cgs = [r.h_cg for r in results]
            self.ax_cg1.clear()
            self.ax_cg2.clear()
            self.ax_cg1.plot(cgs, [r.d_fz for r in results], marker="o")
            self.ax_cg1.set_title("Transf. Carga vs CG")
            self.ax_cg1.grid(True)

            self.ax_cg2.plot(cgs, [r.m_roll for r in results], marker="o")
            self.ax_cg2.set_title("Momento Rolagem vs CG")
            self.ax_cg2.grid(True)

            self.canvas_cg.draw()
            self._set_status("Varredura de CG concluída.")
            self.content_area.select(self.tab_dyn)
        except Exception as e:
            self._set_status(f"Erro CG sweep: {e}")
            messagebox.showerror("Erro", str(e))

    def _run_mass_sweep(self):
        try:
            if not hasattr(self, "tree_mass"):
                raise RuntimeError("tree_mass não existe (aba Dinâmica não montada).")

            h_ro = getattr(self.last_2d_results, "h_ro", None) if self.last_2d_results else None
            if h_ro is None:
                h_ro = 100.0

            m_min = self._read("m_min")
            m_max = self._read("m_max")
            step = self._read("step")

            if step <= 0:
                raise ValueError("Passo (step) deve ser > 0.")
            if m_max < m_min:
                raise ValueError("Massa Max deve ser maior ou igual à Massa Min.")

            params = models.SuspensionMassScanParameters(
                m_min=m_min,
                m_max=m_max,
                m_step=step,
                h_cg=self._read("hcg"),
                ay=self._read("ay"),
                track=self._read("bf"),
                h_ro=h_ro
            )

            results = list(dynamics.calculate_mass_sweep(params))

            for i in self.tree_mass.get_children():
                self.tree_mass.delete(i)

            for r in results:
                margin = getattr(r, "margin", None)
                self.tree_mass.insert("", "end", values=(
                    f"{r.mass:.0f}",
                    f"{r.m_roll:.1f}",
                    f"{r.d_fz:.1f}",
                    f"{r.ssf:.3f}",
                    f"{getattr(r, 'ay_crit', 0):.2f}",
                    f"{margin:.2f}" if margin is not None else "--"
                ))

            self._set_status("Varredura de massa concluída.")
            self.content_area.select(self.tab_dyn)

        except Exception as e:
            self._set_status(f"Erro mass sweep: {e}")
            messagebox.showerror("Erro", str(e))

    def _run_opt(self):
        try:
            corner = self.var_corner.get().strip() or "FR"

            if getattr(self, "var_auto_arm_angles", tk.IntVar(value=1)).get() == 1:
                ang_sup, ang_inf = self._compute_arm_angles_deg(corner)
                self._set("ang_sup", f"{ang_sup:.3f}")
                self._set("ang_inf", f"{ang_inf:.3f}")
            else:
                ang_sup = self._read("ang_sup")
                ang_inf = self._read("ang_inf")

            results = list(dynamics.calculate_force_vs_angle_sweep(
                models.ForceAngleIteratorParameters(
                    f_load=self._read("load"),
                    angle_sup_base=ang_sup,
                    angle_inf_base=ang_inf,
                    k_sup=self._read("ksup"),
                    k_inf=1.0 - self._read("ksup"),
                    limit=self._read("limit"),
                    ang_min=int(self._read("amin")),
                    ang_max=int(self._read("amax")),
                    step=int(5)
                )
            ))

            for i in self.tree_opt.get_children():
                self.tree_opt.delete(i)

            angles, f_sup, f_inf = [], [], []
            limit = self._read("limit")

            for r in results:
                self.tree_opt.insert("", "end", values=(
                    f"{r.angle_delta}", f"{r.force_sup:.1f}", f"{r.force_inf:.1f}", r.status
                ))
                angles.append(r.angle_delta)
                f_sup.append(r.force_sup)
                f_inf.append(r.force_inf)

            self.ax_opt.clear()
            self.ax_opt.plot(angles, f_sup, label="Sup", marker="o")
            self.ax_opt.plot(angles, f_inf, label="Inf", marker="o")
            self.ax_opt.axhline(limit, linestyle="--")
            self.ax_opt.legend()
            self.ax_opt.grid(True)
            self.canvas_opt.draw()

            self._set_status("Análise estrutural concluída.")
            self.content_area.select(self.tab_opt)
        except Exception as e:
            self._set_status(f"Erro Opt: {e}")
            messagebox.showerror("Erro Opt", str(e))

    # =========================
    # Presets (JSON geral)
    # =========================
    def _collect_preset(self) -> dict:
        data = {k: e.get() for k, e in self.entries.items()}
        hp = {}
        for lbl, (ex, ey, ez) in self.entries_hp.items():
            hp[lbl] = {"x": ex.get(), "y": ey.get(), "z": ez.get()}
        data["_hardpoints"] = hp
        data["_static"] = int(getattr(self, "var_static", tk.IntVar(value=0)).get())
        data["_corner"] = self.var_corner.get()
        return data

    def _apply_preset(self, data: dict):
        for k, e in self.entries.items():
            if k in data:
                e.delete(0, tk.END)
                e.insert(0, str(data[k]))

        hp = data.get("_hardpoints", {})
        for lbl, coords in hp.items():
            if lbl in self.entries_hp:
                ex, ey, ez = self.entries_hp[lbl]
                ex.delete(0, tk.END); ex.insert(0, str(coords.get("x", ex.get())))
                ey.delete(0, tk.END); ey.insert(0, str(coords.get("y", ey.get())))
                ez.delete(0, tk.END); ez.insert(0, str(coords.get("z", ez.get())))

        if hasattr(self, "var_static"):
            self.var_static.set(int(data.get("_static", 0)))

        if "_corner" in data:
            try:
                self.var_corner.set(str(data["_corner"]))
            except Exception:
                pass

    def _save_preset(self):
        path = filedialog.asksaveasfilename(defaultextension=".json", filetypes=[("JSON", "*.json")])
        if not path:
            return
        try:
            with open(path, "w", encoding="utf-8") as f:
                json.dump(self._collect_preset(), f, indent=2, ensure_ascii=False)
            self._set_status("Preset salvo.")
        except Exception as e:
            self._set_status(f"Erro ao salvar preset: {e}")
            messagebox.showerror("Erro", str(e))

    def _load_preset(self):
        path = filedialog.askopenfilename(filetypes=[("JSON", "*.json")])
        if not path:
            return
        try:
            with open(path, "r", encoding="utf-8") as f:
                data = json.load(f)
            self._apply_preset(data)
            self._set_status("Preset carregado.")
        except Exception as e:
            self._set_status(f"Erro ao carregar preset: {e}")
            messagebox.showerror("Erro", str(e))

    def _reset_defaults(self):
        defaults = {
            "bf": 1250, "s1": 40, "s2": 30, "cam_o": -1.5, "cam_i": -4.0,
            "spindle_sup_z": 300, "spindle_inf_z": 300, "toe_f_x": 600, "toe_r_x": 600,
            "mass": 200, "hcg": 450, "ay": 9.8, "wb": 1550, "fx": 1200, "rs": 50,
            "h_min": 250, "h_max": 650, "m_min": 180, "m_max": 240, "step": 25,
            "load": 3000, "ang_sup": 10, "ang_inf": 20, "limit": 8000, "ksup": 0.5,
            "amin": 0, "amax": 40
        }
        for k, v in defaults.items():
            self._set(k, v)

        self.var_corner.set("FR")
        self._set_status("Defaults restaurados.")


if __name__ == "__main___":
    root = tk.Tk()
    app = App(root)
    root.mainloop()
