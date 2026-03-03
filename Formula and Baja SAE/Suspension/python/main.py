import json
import csv
import math
import tkinter as tk
from tkinter import ttk, messagebox, filedialog

import matplotlib.pyplot as plt
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg, NavigationToolbar2Tk

# =============================================================================
# IMPORTS DE ARQUITETURA (Refatorado para usar APENAS o Orquestrador)
# =============================================================================
try:
    from orchestrators.suspension import SuspensionOrchestrator
except ImportError as e:
    print(f"ERRO CRÍTICO: Orquestrador não encontrado ({e}). A aplicação não funcionará.")
    SuspensionOrchestrator = None

try:
    import ui_ux.ui_components as uic
except Exception as e:
    uic = None

try:
    import ui_ux.visualization as vis
except Exception as e:
    vis = None

def safe_num(s: str) -> float:
    if s is None: raise ValueError("Valor vazio.")
    s = str(s).strip().replace(",", ".")
    if s == "": raise ValueError("Valor vazio.")
    return float(s)

class App:
    def __init__(self, root: tk.Tk):
        self.root = root
        self.root.title("Simas Turbo - Minh'Pik")
        self.root.geometry("1920x1080")

        self.var_vis3d_mode = tk.StringVar(value="AUTO")  
        if uic is not None:
            uic.setup_style()

        self.free_view = False
        self._view_lock_cid = None

        self.last_2d_results = {"front": None, "rear": None}   
        self.last_3d_results = None
        self.last_panhard = {"front": None, "rear": None}

        self.entries = {}
        self.entries_hp = {}
        self.hp_keys = []

        self.var_corner = tk.StringVar(value="FR")              
        self.var_front_type = tk.StringVar(value="DOUBLE_A")    
        self.var_rear_type = tk.StringVar(value="RIGID")        

        self.UI_LABEL = {
            "bf": "Bitola  [mm]", 
            "wb": "Entre-eixos [mm]",
            "s1": "Curso Extensão (s1) [mm]", 
            "s2": "Curso Compressão (s2) [mm]",
            "cam_o": "Cambagem Extensão [deg]", 
            "cam_i": "Cambagem Compressão [deg]",
            "spindle_sup_z": "Spindle Sup Z [mm]", 
            "spindle_inf_z": "Spindle Inf Z [mm]",
            "toe_f_x": "Toe Front X [mm]", 
            "toe_r_x": "Toe Rear X [mm]",
            "mass": "Massa [kg]", 
            "hcg": "AlturaCG.(hCG) [mm]",
            "ay": "Aceleração lateral [m/s²]", 
            "fx": "Força longitudinal pneu (Fx) [N]",
            "rs": "Scrub Radius [mm]", 
            "h_min": "CG mín [mm]", 
            "h_max": "CG máx [mm]",
            "m_min": "Massa mín [kg]", 
            "m_max": "Massa máx [kg]", "step": "Passo varredura",
            "front_share": "Distribuição lateral no eixo dianteiro [0..1]",
            "pf_ch_x": "Panhard Fr — Chassi X [mm]", 
            "pf_ch_y": "Panhard Fr — Chassi Y [mm]", 
            "pf_ch_z": "Panhard Fr — Chassi Z [mm]",
            "pf_ax_x": "Panhard Fr — Eixo X [mm]", 
            "pf_ax_y": "Panhard Fr — Eixo Y [mm]", 
            "pf_ax_z": "Panhard Fr — Eixo Z [mm]",
            "pf_ref_ax_x": "Ref FRO (Eixo) X [mm]", 
            "pf_ref_ax_y": "Ref FR (Eixo) Y [mm]", 
            "pf_ref_ax_z": "Ref FR (Eixo) Z [mm]",
            "pf_ref_ch_x": "Ref FR (Chassi) X [mm]", 
            "pf_ref_ch_y": "Ref FR (Chassi) Y [mm]", 
            "pf_ref_ch_z": "Ref FR (Chassi) Z [mm]",
            "pr_ch_x": "Panhard Rr — Chassi X [mm]", 
            "pr_ch_y": "Panhard Rr — Chassi Y [mm]", 
            "pr_ch_z": "Panhard Rr — Chassi Z [mm]",
            "pr_ax_x": "Panhard Rr — Eixo X [mm]", 
            "pr_ax_y": "Panhard Rr — Eixo Y [mm]", 
            "pr_ax_z": "Panhard Rr — Eixo Z [mm]",
            "pr_ref_ax_x": "Ref RR (Eixo) X [mm]", 
            "pr_ref_ax_y": "Ref RR (Eixo) Y [mm]", 
            "pr_ref_ax_z": "Ref RR (Eixo) Z [mm]",
            "pr_ref_ch_x": "Ref RR (Chassi) X [mm]", 
            "pr_ref_ch_y": "Ref RR (Chassi) Y [mm]", 
            "pr_ref_ch_z": "Ref RR (Chassi) Z [mm]",
            "load": "Carga aplicada F [N]", 
            "ang_sup": "Ângulo base braço superior [deg]",
            "ang_inf": "Ângulo base braço inferior [deg]", 
            "limit": "Limite de força [N]",
            "ksup": "Distribuição no braço sup (Ksup) [0..1]",
            "amin": "Varredura ângulo mínima [deg]", 
            "amax": "Varredura ângulo máxima [deg]",
        }

        self._build_topbar()
        self.paned = tk.PanedWindow(root, orient="horizontal", sashwidth=10, bg="#121212")
        self.paned.pack(fill="both", expand=True)

        if uic is not None:
            self.sidebar_container = uic.ScrollableFrame(self.paned, width=410)
            self.paned.add(self.sidebar_container, minsize=400, width=650)
            sidebar_parent = self.sidebar_container.scrollable_window
        else:
            self.sidebar_container = ttk.Frame(self.paned)
            self.paned.add(self.sidebar_container, minsize=400, width=420)
            sidebar_parent = self.sidebar_container

        self.content_area = ttk.Notebook(self.paned)
        self.paned.add(self.content_area, minsize=700)

        self._init_sidebar_inputs(sidebar_parent)
        self._init_result_tabs()
        self._build_statusbar()

    # =========================================================================
    # HELPERS DE UI (Wrappers para self.entries)
    # =========================================================================
    def _create_entry(self, parent, key, label, default, row, col, width=10, tip=None):
        label_txt = self.UI_LABEL.get(key, label)
        ttk.Label(parent, text=label_txt).grid(row=row, column=col, sticky="e", padx=3, pady=2)
        e = ttk.Entry(parent, width=width)
        e.insert(0, str(default))
        e.grid(row=row, column=col + 1, sticky="w", padx=3, pady=2)
        self.entries[key] = e
        if tip and uic is not None:
            uic.ToolTip(e, tip)
        return e

    def _read(self, key) -> float:
        if key not in self.entries:
            raise KeyError(f"Campo não encontrado: {key}")
        return safe_num(self.entries[key].get())

    def _set(self, key, value):
        e = self.entries.get(key)
        if not e: return
        e.delete(0, tk.END)
        e.insert(0, str(value))

    def _set_status(self, msg: str):
        if hasattr(self, "status_var"):
            self.status_var.set(msg)

    def _safe_call(self, fn, name: str):
        try:
            fn()
            return True
        except Exception as e:
            self._set_status(f"Erro em {name}: {e}")
            messagebox.showerror(f"Erro em {name}", str(e))
            return False

    def _corner_name(self, corner: str) -> str:
        corner = (corner or "").strip().upper()
        return {"FR": "FR ", "FL": "FL ", "RR": "RR ", "RL": "RL "}.get(corner, corner)

    def _point_name(self, p: str) -> str:
        return {
            "Sup In": "UCA — Inboard", 
            "Sup Out": "UCA — Outboard", 
            "Inf In": "LCA — Inboard",
            "Inf Out": "LCA — Outboard", 
            "Toe In": "Tie Rod — Inboard", 
            "Toe Out": "Tie Rod — Outboard",
            "Damper In": "Damper — Inboard", 
            "Damper Out": "Damper — Outboard"
        }.get(p, p)

    def _hp_pretty_label(self, corner: str, p: str) -> str:
        return f"{self._corner_name(corner)} | {self._point_name(p)}"

    def _hp_key(self, corner: str, point: str) -> str:
        return f"{corner} {point}"

    def _hp_get(self, lbl: str):
        ex, ey, ez = self.entries_hp[lbl]
        return safe_num(ex.get()), safe_num(ey.get()), safe_num(ez.get())

    def _hp_set(self, lbl: str, x: float, y: float, z: float):
        ex, ey, ez = self.entries_hp[lbl]
        ex.delete(0, tk.END)
        ex.insert(0, str(x))
        ey.delete(0, tk.END)
        ey.insert(0, str(y))
        ez.delete(0, tk.END)
        ez.insert(0, str(z))

    # =========================================================================
    # TOPBAR E LAYOUT LATERAL
    # =========================================================================
    def _build_topbar(self):
        bar = ttk.Frame(self.root)
        bar.pack(fill="x")
        ttk.Label(bar, text="Ações rápidas").pack(side="left", padx=(6, 16))
        actions = [
            ("Calcular 2D (F/R)", self._calc_2d_front_rear),
            ("Calcular 3D", self._calc_3d),
            ("Visual 3D", self._calc_full_3d),
            ("Panhard (Momento)", self._calc_panhard_moments),
            ("CG Sweep", self._run_cg_sweep),
            ("Mass Sweep", self._run_mass_sweep),
            ("Estrutura", self._run_opt),
        ]
        for txt, cmd in actions:
            ttk.Button(bar, text=txt, command=cmd).pack(side="left", padx=4)
        ttk.Separator(bar, orient="vertical").pack(side="left", fill="y", padx=10)
        ttk.Button(bar, text="Salvar preset", command=self._save_preset).pack(side="left", padx=4)
        ttk.Button(bar, text="Carregar preset", command=self._load_preset).pack(side="left", padx=4)
        ttk.Button(bar, text="Reset defaults", command=self._reset_defaults).pack(side="left", padx=4)

    def _build_statusbar(self):
        self.status_var = tk.StringVar(value="Pronto.")
        bar = ttk.Frame(self.root)
        bar.pack(fill="x", side="bottom")
        ttk.Label(bar, textvariable=self.status_var).pack(side="left", padx=8, pady=3)

    def _init_sidebar_inputs(self, parent):
        f_geo = ttk.LabelFrame(parent, text="1) Geometria (Hardpoints) [mm]")
        f_geo.pack(fill="x", padx=6, pady=6)
        self._create_entry(f_geo, "bf", "Bitola (bf):", 1250, 0, 0, width=12)
        self._create_entry(f_geo, "wb", "Entre-eixos (wb):", 1550, 0, 2, width=12)
        
        ttk.Label(f_geo, text="Canto p/ cálculo 3D:").grid(row=1, column=0, sticky="e", padx=3, pady=2)
        cb = ttk.Combobox(f_geo, values=["FR", "FL", "RR", "RL"], textvariable=self.var_corner, width=6, state="readonly")
        cb.grid(row=1, column=1, sticky="w", padx=3, pady=2)

        ttk.Label(f_geo, text="Tipo DIANTEIRO:").grid(row=1, column=2, sticky="e", padx=3, pady=2)
        cbf = ttk.Combobox(f_geo, values=["DOUBLE_A", "RIGID"], textvariable=self.var_front_type, width=10, state="readonly")
        cbf.grid(row=1, column=3, sticky="w", padx=3, pady=2)

        ttk.Label(f_geo, text="Tipo TRASEIRO:").grid(row=2, column=2, sticky="e", padx=3, pady=2)
        cbr = ttk.Combobox(f_geo, values=["DOUBLE_A", "RIGID"], textvariable=self.var_rear_type, width=10, state="readonly")
        cbr.grid(row=2, column=3, sticky="w", padx=3, pady=2)

        ttk.Label(f_geo, text="X").grid(row=6, column=1)
        ttk.Label(f_geo, text="Y").grid(row=6, column=2)
        ttk.Label(f_geo, text="Z").grid(row=6, column=3)

        corners = ["FR", "FL", "RR", "RL"]
        pts = ["Sup In", "Sup Out", "Inf In", "Inf Out", "Toe In", "Toe Out", "Damper In", "Damper Out"]
        base_defaults = {
            "Sup In": (450, 420, 200), "Sup Out": (625, 390, 300),
            "Inf In": (430, 210, 200), "Inf Out": (625, 190, 300),
            "Toe In": (520, 260, 120), "Toe Out": (650, 260, 180),
            "Damper In": (380, 520, 150), "Damper Out": (610, 430, 260),
        }

        r = 7
        self.hp_keys = []
        for corner in corners:
            ttk.Label(f_geo, text=f"== {self._corner_name(corner)} ==", font=("Arial", 9, "bold")).grid(row=r, column=0, columnspan=4, sticky="w", pady=(10, 2))
            r += 1
            for p in pts:
                lbl = self._hp_key(corner, p)
                dx, dy, dz = base_defaults[p]
                if corner in ("FL", "RL"): dx = -dx
                ttk.Label(f_geo, text=self._hp_pretty_label(corner, p) + ":").grid(row=r, column=0, sticky="e")
                ex = ttk.Entry(f_geo, width=8); ex.insert(0, str(dx)); ex.grid(row=r, column=1)
                ey = ttk.Entry(f_geo, width=8); ey.insert(0, str(dy)); ey.grid(row=r, column=2)
                ez = ttk.Entry(f_geo, width=8); ez.insert(0, str(dz)); ez.grid(row=r, column=3)
                self.entries_hp[lbl] = (ex, ey, ez)
                self.hp_keys.append(lbl)
                r += 1

        f_kin = ttk.LabelFrame(parent, text="2) Cinemática & Alinhamento")
        f_kin.pack(fill="x", padx=6, pady=6)
        self._create_entry(f_kin, "s1", "Curso Ext:", 40, 0, 0)
        self._create_entry(f_kin, "s2", "Curso Int:", 30, 0, 2)
        self._create_entry(f_kin, "cam_o", "Cam Ext:", -1.5, 1, 0)
        self._create_entry(f_kin, "cam_i", "Cam Int:", -4.0, 1, 2)
        self._create_entry(f_kin, "spindle_sup_z", "Spindle Sup Z:", 300, 3, 0)
        self._create_entry(f_kin, "spindle_inf_z", "Spindle Inf Z:", 300, 3, 2)
        self._create_entry(f_kin, "toe_f_x", "Toe F X:", 600, 4, 0)
        self._create_entry(f_kin, "toe_r_x", "Toe R X:", 600, 4, 2)

        f_dyn = ttk.LabelFrame(parent, text="3) Dinâmica Veicular")
        f_dyn.pack(fill="x", padx=6, pady=6)
        self._create_entry(f_dyn, "mass", "Massa (kg):", 260, 0, 0)
        self._create_entry(f_dyn, "hcg", "Altura CG:", 450, 0, 2)
        self._create_entry(f_dyn, "ay", "Ay (m/s²):", 9.8, 1, 0)
        self._create_entry(f_dyn, "fx", "Fx Pneu (N):", 1200, 2, 0)
        self._create_entry(f_dyn, "rs", "Scrub (mm):", 50, 2, 2)
        self._create_entry(f_dyn, "h_min", "CG Min:", 250, 4, 0)
        self._create_entry(f_dyn, "h_max", "CG Max:", 650, 4, 2)
        self._create_entry(f_dyn, "m_min", "Massa Min:", 180, 5, 0)
        self._create_entry(f_dyn, "m_max", "Massa Max:", 300, 5, 2)
        self._create_entry(f_dyn, "step", "Passo:", 25, 6, 0)

        f_pan = ttk.LabelFrame(parent, text="4) Panhard (Front/Rear) + Momentos (suportes)")
        f_pan.pack(fill="x", padx=6, pady=6)
        ttk.Label(f_pan, text="Distribuição lateral (fração no eixo dianteiro):").grid(row=0, column=0, sticky="e", padx=3, pady=2)
        self._create_entry(f_pan, "front_share", "Front share:", 0.55, 0, 2, width=10)
        ttk.Separator(f_pan, orient="horizontal").grid(row=1, column=0, columnspan=4, sticky="ew", pady=(8, 8))
        ttk.Label(f_pan, text="FRONT PANHARD", font=("Arial", 9, "bold")).grid(row=2, column=0, columnspan=4, sticky="w")
        self._create_entry(f_pan, "pf_ch_x", "Chassi X:", -250, 3, 0, width=9)
        self._create_entry(f_pan, "pf_ch_y", "Chassi Y:", 260, 3, 2, width=9)
        self._create_entry(f_pan, "pf_ch_z", "Chassi Z:", 775, 4, 0, width=9)
        self._create_entry(f_pan, "pf_ax_x", "Eixo X:", 250, 4, 2, width=9)
        self._create_entry(f_pan, "pf_ax_y", "Eixo Y:", 240, 5, 0, width=9)
        self._create_entry(f_pan, "pf_ax_z", "Eixo Z:", 775, 5, 2, width=9)
        ttk.Label(f_pan, text="Ref FRONT (p/ momento)", font=("Arial", 9, "bold")).grid(row=6, column=0, columnspan=4, sticky="w", pady=(6, 0))
        self._create_entry(f_pan, "pf_ref_ax_x", "Ref eixo X:", 0, 7, 0, width=9)
        self._create_entry(f_pan, "pf_ref_ax_y", "Ref eixo Y:", 220, 7, 2, width=9)
        self._create_entry(f_pan, "pf_ref_ax_z", "Ref eixo Z:", 775, 8, 0, width=9)
        self._create_entry(f_pan, "pf_ref_ch_x", "Ref chassi X:", 0, 8, 2, width=9)
        self._create_entry(f_pan, "pf_ref_ch_y", "Ref chassi Y:", 300, 9, 0, width=9)
        self._create_entry(f_pan, "pf_ref_ch_z", "Ref chassi Z:", 775, 9, 2, width=9)
        ttk.Separator(f_pan, orient="horizontal").grid(row=10, column=0, columnspan=4, sticky="ew", pady=(8, 8))
        ttk.Label(f_pan, text="REAR PANHARD", font=("Arial", 9, "bold")).grid(row=11, column=0, columnspan=4, sticky="w")
        self._create_entry(f_pan, "pr_ch_x", "Chassi X:", 250, 12, 0, width=9)
        self._create_entry(f_pan, "pr_ch_y", "Chassi Y:", 260, 12, 2, width=9)
        self._create_entry(f_pan, "pr_ch_z", "Chassi Z:", -775, 13, 0, width=9)
        self._create_entry(f_pan, "pr_ax_x", "Eixo X:", -250, 13, 2, width=9)
        self._create_entry(f_pan, "pr_ax_y", "Eixo Y:", 240, 14, 0, width=9)
        self._create_entry(f_pan, "pr_ax_z", "Eixo Z:", -775, 14, 2, width=9)
        ttk.Label(f_pan, text="Ref REAR (p/ momento)", font=("Arial", 9, "bold")).grid(row=15, column=0, columnspan=4, sticky="w", pady=(6, 0))
        self._create_entry(f_pan, "pr_ref_ax_x", "Ref eixo X:", 0, 16, 0, width=9)
        self._create_entry(f_pan, "pr_ref_ax_y", "Ref eixo Y:", 220, 16, 2, width=9)
        self._create_entry(f_pan, "pr_ref_ax_z", "Ref eixo Z:", -775, 17, 0, width=9)
        self._create_entry(f_pan, "pr_ref_ch_x", "Ref chassi X:", 0, 17, 2, width=9)
        self._create_entry(f_pan, "pr_ref_ch_y", "Ref chassi Y:", 300, 18, 0, width=9)
        self._create_entry(f_pan, "pr_ref_ch_z", "Ref chassi Z:", -775, 18, 2, width=9)

        f_opt = ttk.LabelFrame(parent, text="5) Otimização Estrutural")
        f_opt.pack(fill="x", padx=6, pady=6)
        self._create_entry(f_opt, "load", "Carga F [N]:", 3000, 0, 0)
        self.var_static = tk.IntVar(value=0)
        ttk.Checkbutton(f_opt, text="+ Estática", variable=self.var_static).grid(row=0, column=2, sticky="w")
        self.var_auto_arm_angles = tk.IntVar(value=1)
        ttk.Checkbutton(f_opt, text="Usar ângulos automáticos", variable=self.var_auto_arm_angles).grid(row=1, column=0, columnspan=4, sticky="w")
        self._create_entry(f_opt, "ang_sup", "Ang Base Sup:", 10, 2, 0)
        self._create_entry(f_opt, "ang_inf", "Ang Base Inf:", 20, 2, 2)
        self._create_entry(f_opt, "limit", "Limite (N):", 8000, 3, 0)
        self._create_entry(f_opt, "ksup", "K Sup:", 0.5, 3, 2)
        self._create_entry(f_opt, "amin", "Varredura Min:", 0, 4, 0)
        self._create_entry(f_opt, "amax", "Varredura Max:", 40, 4, 2)

    # =========================================================================
    # ROTEAMENTO VIA ORQUESTRADOR
    # Nenhuma matemática ocorre no Main.py!
    # =========================================================================

    def _calc_2d_front_rear(self):
        if not SuspensionOrchestrator: return
        ui_data = self._collect_preset()
        
        # O orquestrador deve receber o dicionário e devolver pontos prontos para plotagem
        res_front = SuspensionOrchestrator.run_2d_analysis(ui_data, "FRONT")
        res_rear = SuspensionOrchestrator.run_2d_analysis(ui_data, "REAR")

        self.last_2d_results["front"] = {"h_ro": res_front.get("h_ro")}
        self.last_2d_results["rear"] = {"h_ro": res_rear.get("h_ro")}

        self._plot_2d_axis(self.ax_2d_front, res_front, "FRONT")
        self._plot_2d_axis(self.ax_2d_rear, res_rear, "REAR")
        self._update_2d_summary_label()
        
        self.canvas_2d.draw()
        self.content_area.select(self.tab_2d)
        self._set_status("✅ 2D Calculado (Via Orquestrador).")

    def _plot_2d_axis(self, ax, res: dict, axis_name: str):
        ax.clear()
        if res.get("mode") == "RIGID":
            ax.plot([res["xl"], res["xr"]], [res["yl"], res["yr"]], "o-", linewidth=2, label="Eixo rígido")
            ax.plot(0, res["h_ro"], "*", markersize=14, label="RC")
        else:
            for side in ["R", "L"]:
                geo = res.get(side, {})
                if not geo: continue
                col = "tab:blue" if side == "R" else "tab:red"
                ax.plot([geo["up_in_x"], geo["up_out_x"]], [geo["up_in_y"], geo["up_out_y"]], "o--", color=col)
                ax.plot([geo["low_in_x"], geo["low_out_x"]], [geo["low_in_y"], geo["low_out_y"]], "o-", color=col)
                if geo.get("ic_x"):
                    ax.plot(geo["ic_x"], geo["ic_y"], "s", color=col)
            
            if res.get("h_ro") is not None:
                ax.plot(0, res["h_ro"], "*", markersize=12, color="k")

        ax.axvline(0, linestyle=":", color="k")
        ax.set_title(f"{axis_name} 2D")
        ax.grid(True)
        ax.legend(fontsize=8)
        ax.set_aspect("equal", adjustable="datalim")

    def _update_2d_summary_label(self):
        f = self.last_2d_results.get("front", {}).get("h_ro")
        r = self.last_2d_results.get("rear", {}).get("h_ro")
        f_str = f"{f:.1f} mm" if f is not None else "--"
        r_str = f"{r:.1f} mm" if r is not None else "--"
        self.lbl_2d_res.config(text=f"FRONT h_Ro={f_str} | REAR h_Ro={r_str}")

    def _calc_panhard_moments(self):
        if not SuspensionOrchestrator: return
        ui_data = self._collect_preset()
        
        results = SuspensionOrchestrator.run_panhard_analysis(ui_data)
        
        for i in self.tree_panhard.get_children():
            self.tree_panhard.delete(i)

        for axis in ["FRONT", "REAR"]:
            r = results.get(axis, {})
            if not r: continue
            self.tree_panhard.insert("", "end", values=(
                axis, f"{r['Fy_axle']:.1f}", f"{r['F_axial']:.1f}", f"{r['ux']:.3f}",
                f"{r['Mx_ax']:.1f}", f"{r['My_ax']:.1f}", f"{r['Mz_ax']:.1f}",
                f"{r['Mx_ch']:.1f}", f"{r['My_ch']:.1f}", f"{r['Mz_ch']:.1f}"
            ))
            self.last_panhard[axis.lower()] = r

        self._set_status("✅ Panhard calculada (Via Orquestrador).")
        self.content_area.select(self.tab_panhard)

    def _calc_3d(self):
        if not SuspensionOrchestrator: return
        ui_data = self._collect_preset()
        
        res = SuspensionOrchestrator.run_3d_analysis(ui_data, self.var_corner.get())
        if res:
            self.lbl_3d_res.config(text=(
                f"Canto: {self.var_corner.get()}\n"
                f"F Axial Sup: {res.get('f_sup_axial', 0):.1f} N\n"
                f"F Axial Inf: {res.get('f_inf_axial', 0):.1f} N"
            ))
        self._set_status("✅ 3D calculado (Via Orquestrador).")
        self.content_area.select(self.tab_3d)

    def _run_cg_sweep(self):
        if not SuspensionOrchestrator: return
        h_ro = self.last_2d_results.get("front", {}).get("h_ro") or 100.0
        ui_data = self._collect_preset()
        
        results = SuspensionOrchestrator.run_cg_sweep(ui_data, h_ro)
        
        for i in self.tree_cg.get_children():
            self.tree_cg.delete(i)
        
        h_vals, dfz_vals, m_vals = [], [], []
        for r in results:
            self.tree_cg.insert("", "end", values=(f"{r['h_cg']:.1f}", f"{r['d_fz']:.1f}", f"{r['fz_int']:.1f}", f"{r['m_roll']:.1f}"))
            h_vals.append(r["h_cg"]); dfz_vals.append(r["d_fz"]); m_vals.append(r["m_roll"])

        self.ax_cg1.clear(); self.ax_cg2.clear()
        self.ax_cg1.plot(h_vals, dfz_vals, "o-"); self.ax_cg1.set_title("Transf. Carga vs CG"); self.ax_cg1.grid(True)
        self.ax_cg2.plot(h_vals, m_vals, "o-"); self.ax_cg2.set_title("Momento Roll vs CG"); self.ax_cg2.grid(True)
        self.canvas_cg.draw()
        self.content_area.select(self.tab_dyn)

    def _run_mass_sweep(self):
        if not SuspensionOrchestrator: return
        h_ro = self.last_2d_results.get("front", {}).get("h_ro") or 100.0
        ui_data = self._collect_preset()
        
        results = SuspensionOrchestrator.run_mass_sweep(ui_data, h_ro)
        
        for i in self.tree_mass.get_children():
            self.tree_mass.delete(i)
            
        for r in results:
            self.tree_mass.insert("", "end", values=(
                f"{r['mass']:.0f}", f"{r['m_roll']:.1f}", f"{r['d_fz']:.1f}",
                f"{r['ssf']:.3f}", f"{r['ay_crit']:.2f}", f"{r['margin']:.2f}"
            ))
        self.content_area.select(self.tab_dyn)

    def _run_opt(self):
        if not SuspensionOrchestrator: return
        ui_data = self._collect_preset()
        
        # Deixando o Orquestrador extrair os ângulos diretamente dos hardpoints via ui_data
        results = SuspensionOrchestrator.run_structural_optimization(ui_data, self.var_auto_arm_angles.get())
        
        for i in self.tree_opt.get_children():
            self.tree_opt.delete(i)
            
        angles, f_sup, f_inf = [], [], []
        for r in results:
            self.tree_opt.insert("", "end", values=(f"{r['angle_delta']}", f"{r['force_sup']:.1f}", f"{r['force_inf']:.1f}", r['status']))
            angles.append(r['angle_delta'])
            f_sup.append(r['force_sup'])
            f_inf.append(r['force_inf'])

        self.ax_opt.clear()
        self.ax_opt.plot(angles, f_sup, label="F Sup", marker="o")
        self.ax_opt.plot(angles, f_inf, label="F Inf", marker="o")
        self.ax_opt.axhline(self._read("limit"), linestyle="--", color="r")
        self.ax_opt.grid(True)
        self.ax_opt.legend()
        self.canvas_opt.draw()
        self.content_area.select(self.tab_opt)

    # =========================================================================
    # VIEW CONTROL E SETUP (MANTIDOS DA UI ORIGINAL)
    # =========================================================================
    def _calc_full_3d(self):
        # A Plotagem Visual 3D pode permanecer na UI consultando os Dicionários ou via Orquestrador.
        pass

    def _toggle_free_view(self):
        self.free_view = not self.free_view
        self._apply_view_mode()
        self.btn_free_view.config(text="Visualização: LIVRE (3D)" if self.free_view else "Visualização: FIXA (YZ)")

    def _apply_view_mode(self):
        if not hasattr(self, "ax_vis3d"): return
        if self._view_lock_cid:
            self.canvas_vis3d.mpl_disconnect(self._view_lock_cid)
            self._view_lock_cid = None
        if self.free_view:
            self.ax_vis3d.mouse_init()
        else:
            self.ax_vis3d.mouse_init(rotate_btn=None)
            self._view_lock_cid = self.canvas_vis3d.mpl_connect("button_release_event", lambda e: self._enforce_yz_view())
            self._enforce_yz_view()

    def _enforce_yz_view(self):
        if self.free_view: return
        self.ax_vis3d.view_init(elev=0, azim=0)
        self.canvas_vis3d.draw_idle()

    def _reset_view(self):
        self.ax_vis3d.view_init(elev=20, azim=-60) if self.free_view else self.ax_vis3d.view_init(0, 0)
        self.canvas_vis3d.draw_idle()

    def _init_result_tabs(self):
        self.tab_2d = ttk.Frame(self.content_area); self.content_area.add(self.tab_2d, text="Resultados 2D")
        self.tab_3d = ttk.Frame(self.content_area); self.content_area.add(self.tab_3d, text="Resultados 3D")
        self.tab_vis3d = ttk.Frame(self.content_area); self.content_area.add(self.tab_vis3d, text="Visualização 3D")
        self.tab_panhard = ttk.Frame(self.content_area); self.content_area.add(self.tab_panhard, text="Panhard")
        self.tab_dyn = ttk.Frame(self.content_area); self.content_area.add(self.tab_dyn, text="Dinâmica")
        self.tab_opt = ttk.Frame(self.content_area); self.content_area.add(self.tab_opt, text="Otimização")

        self._setup_res_2d(self.tab_2d); self._setup_res_3d(self.tab_3d)
        self._setup_res_vis3d(self.tab_vis3d); self._setup_res_panhard(self.tab_panhard)
        self._setup_res_dyn(self.tab_dyn); self._setup_res_opt(self.tab_opt)

    def _setup_res_2d(self, parent):
        card = ttk.Frame(parent); card.pack(fill="x", padx=12, pady=10)
        self.lbl_2d_res = ttk.Label(card, text="Aguardando cálculo...", font=("Courier", 10)); self.lbl_2d_res.pack(anchor="w")
        self.fig_2d, (self.ax_2d_front, self.ax_2d_rear) = plt.subplots(1, 2, figsize=(10, 4))
        self.canvas_2d = FigureCanvasTkAgg(self.fig_2d, master=parent); self.canvas_2d.get_tk_widget().pack(fill="both", expand=True, padx=12)

    def _setup_res_3d(self, parent):
        card = ttk.Frame(parent); card.pack(fill="x", padx=12, pady=10)
        self.lbl_3d_res = ttk.Label(card, text="Aguardando cálculo...", font=("Courier", 10)); self.lbl_3d_res.pack(anchor="w")

    def _setup_res_vis3d(self, parent):
        ctrl = ttk.Frame(parent, padding=8); ctrl.pack(fill="x")
        ttk.Label(ctrl, text="Modo Visual 3D:").pack(side="left")
        cbm = ttk.Combobox(ctrl, values=["AUTO", "DOUBLE_A", "RIGID"], textvariable=self.var_vis3d_mode, width=10, state="readonly"); cbm.pack(side="left", padx=6)
        ttk.Button(ctrl, text="GERAR DIAGRAMA", command=self._calc_full_3d).pack(side="left", padx=8)
        ttk.Button(ctrl, text="Reset View", command=self._reset_view).pack(side="right")
        self.btn_free_view = ttk.Button(ctrl, text="Visualização: FIXA (YZ)", command=self._toggle_free_view); self.btn_free_view.pack(side="right", padx=8)
        self.var_grid_step = tk.DoubleVar(value=50.0)
        ttk.Scale(ctrl, from_=10, to=100, variable=self.var_grid_step, orient="horizontal").pack(side="left", padx=10)
        self.fig_vis3d = plt.figure(figsize=(5, 5))
        self.ax_vis3d = self.fig_vis3d.add_subplot(111, projection="3d")
        self.canvas_vis3d = FigureCanvasTkAgg(self.fig_vis3d, master=parent); self.canvas_vis3d.get_tk_widget().pack(fill="both", expand=True)
        NavigationToolbar2Tk(self.canvas_vis3d, parent).pack(fill="x")
        self._apply_view_mode()

    def _setup_res_panhard(self, parent):
        card = ttk.Frame(parent); card.pack(fill="x", padx=12, pady=10)
        self.lbl_panhard_res = ttk.Label(card, text="Aguardando cálculo...", font=("Courier", 10)); self.lbl_panhard_res.pack(anchor="w")
        cols = ("Eixo", "Fy_axle", "F_axial", "ux", "Mx_ax", "My_ax", "Mz_ax", "Mx_ch", "My_ch", "Mz_ch")
        self.tree_panhard = ttk.Treeview(parent, columns=cols, show="headings", height=10)
        for c in cols: self.tree_panhard.heading(c, text=c)
        self.tree_panhard.pack(fill="both", expand=True, padx=12, pady=(0, 12))

    def _setup_res_dyn(self, parent):
        nb = ttk.Notebook(parent); nb.pack(fill="both", expand=True)
        f_cg = ttk.Frame(nb); nb.add(f_cg, text="CG Sweep")
        f_mass = ttk.Frame(nb); nb.add(f_mass, text="Mass Sweep")
        self.fig_cg, (self.ax_cg1, self.ax_cg2) = plt.subplots(1, 2, figsize=(8, 3))
        self.canvas_cg = FigureCanvasTkAgg(self.fig_cg, master=f_cg); self.canvas_cg.get_tk_widget().pack(fill="both", expand=True)
        self.tree_cg = ttk.Treeview(f_cg, columns=("CG", "dFz", "Fz_int", "M_Roll"), show="headings", height=5)
        for c in ("CG", "dFz", "Fz_int", "M_Roll"): self.tree_cg.heading(c, text=c)
        self.tree_cg.pack(fill="x")
        cols_m = ("Massa", "M_Roll", "dFz", "SSF", "Ay_Crit", "Margem")
        self.tree_mass = ttk.Treeview(f_mass, columns=cols_m, show="headings")
        for c in cols_m: self.tree_mass.heading(c, text=c)
        self.tree_mass.pack(fill="both", expand=True)

    def _setup_res_opt(self, parent):
        paned = tk.PanedWindow(parent, orient="vertical"); paned.pack(fill="both", expand=True)
        f_tab = ttk.Frame(paned)
        self.tree_opt = ttk.Treeview(f_tab, columns=("dAng", "FSup", "FInf", "Sts"), show="headings")
        for c in ("dAng", "FSup", "FInf", "Sts"): self.tree_opt.heading(c, text=c)
        self.tree_opt.pack(fill="both", expand=True)
        paned.add(f_tab, minsize=150)
        f_plt = ttk.Frame(paned)
        self.fig_opt, self.ax_opt = plt.subplots(figsize=(5, 3))
        self.canvas_opt = FigureCanvasTkAgg(self.fig_opt, master=f_plt); self.canvas_opt.get_tk_widget().pack(fill="both", expand=True)
        paned.add(f_plt)

    # =========================================================================
    # PERSISTÊNCIA E UTILITÁRIOS
    # =========================================================================
    def _collect_preset(self) -> dict:
        data = {k: e.get() for k, e in self.entries.items()}
        hp = {lbl: {"x": v[0].get(), "y": v[1].get(), "z": v[2].get()} for lbl, v in self.entries_hp.items()}
        data["_hardpoints"] = hp
        data["_corner"] = self.var_corner.get()
        data["_front_type"] = self.var_front_type.get()
        data["_rear_type"] = self.var_rear_type.get()
        data["_static"] = getattr(self, "var_static", tk.IntVar(value=0)).get()
        data["_vis3d_mode"] = self.var_vis3d_mode.get()
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

    def _save_preset(self):
        path = filedialog.asksaveasfilename(defaultextension=".json", filetypes=[("JSON", "*.json")])
        if path:
            with open(path, "w", encoding="utf-8") as f:
                json.dump(self._collect_preset(), f, indent=2, ensure_ascii=False)

    def _load_preset(self):
        path = filedialog.askopenfilename(filetypes=[("JSON", "*.json")])
        if path:
            with open(path, "r", encoding="utf-8") as f:
                self._apply_preset(json.load(f))

if __name__ == "__main__":
    root = tk.Tk()
    app = App(root)
    root.mainloop()