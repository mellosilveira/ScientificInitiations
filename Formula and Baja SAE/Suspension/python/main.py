import json
import csv
import math
import tkinter as tk
from tkinter import ttk, messagebox, filedialog

import matplotlib.pyplot as plt
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg, NavigationToolbar2Tk

# =============================================================================
# IMPORTS DOS MÓDULOS DE DOMÍNIO (Mantidos do seu projeto original)
# =============================================================================
try:
    import models
    import math_2d
    import math_3d
    import dynamics
    import utils
    import ui_strings
except ImportError as e:
    print(f"ERRO CRÍTICO: Faltando módulos de domínio ({e}). O programa pode não rodar.")

# =============================================================================
# NOVOS IMPORTS DE UI/UX (Refatoração)
# =============================================================================
import ui_components as uic
import visualization as vis


def safe_num(s: str) -> float:
    """Converte string para float aceitando vírgula e tratando erros."""
    if s is None:
        raise ValueError("Valor vazio.")
    s = str(s).strip().replace(",", ".")
    if s == "":
        raise ValueError("Valor vazio.")
    return float(s)


class App:
    def __init__(self, root: tk.Tk):
        self.root = root
        self.root.title("Suíte de Engenharia de Suspensão (Baja SAE) - Refactored")
        self.root.geometry("1400x900")

        # Configura estilo via ui_components
        uic.setup_style()

        # Estado: 3D view
        self.free_view = False
        self._view_lock_cid = None

        # Estado: resultados
        self.last_2d_results = None
        self.last_3d_results = None

        # Armazenamento de Entradas (Widgets)
        self.entries = {}
        self.entries_hp = {}
        self.hp_keys = []

        # Variável de controle principal
        self.var_corner = tk.StringVar(value="FR")

        # Construção da Interface
        self._build_topbar()

        self.paned = tk.PanedWindow(root, orient="horizontal", sashwidth=6, bg="#b0b0b0")
        self.paned.pack(fill="both", expand=True)

        # Sidebar (Scrollable via uic)
        self.sidebar_container = uic.ScrollableFrame(self.paned, width=410)
        self.paned.add(self.sidebar_container, minsize=400, width=420)

        # Área de Conteúdo (Notebook)
        self.content_area = ttk.Notebook(self.paned)
        self.paned.add(self.content_area, minsize=700)

        self._init_sidebar_inputs(self.sidebar_container.scrollable_window)
        self._init_result_tabs()
        self._build_statusbar()

    # =========================================================================
    # HELPERS DE UI (Wrappers para self.entries)
    # =========================================================================
    def _create_entry(self, parent, key, label, default, row, col, width=10, tip=None):
        ttk.Label(parent, text=label).grid(row=row, column=col, sticky="e", padx=3, pady=2)
        e = ttk.Entry(parent, width=width)
        e.insert(0, str(default))
        e.grid(row=row, column=col + 1, sticky="w", padx=3, pady=2)
        self.entries[key] = e
        if tip:
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
            try:
                messagebox.showerror(f"Erro em {name}", str(e))
            except Exception: pass
            return False

    # =========================================================================
    # HARDPOINTS LOGIC
    # =========================================================================
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

    def _get_point3d_from_input(self, label: str):
        x, y, z = self._hp_get(label)
        return models.Point3D(x, y, z)

    def _pair_corner(self, corner: str):
        c = (corner or "FR").strip().upper()
        if c.endswith("L"):
            return c[:-1] + "R", c
        return c, c[:-1] + "L"

    def _corner_points_2d(self, corner: str):
        c = (corner or "FR").strip().upper()
        ui_x, ui_y, _ = self._hp_get(self._hp_key(c, "Sup In"))
        uo_x, uo_y, _ = self._hp_get(self._hp_key(c, "Sup Out"))
        li_x, li_y, _ = self._hp_get(self._hp_key(c, "Inf In"))
        lo_x, lo_y, _ = self._hp_get(self._hp_key(c, "Inf Out"))
        return (
            models.Point2D(ui_x, ui_y), models.Point2D(uo_x, uo_y),
            models.Point2D(li_x, li_y), models.Point2D(lo_x, lo_y),
        )

    def _make_geo2d_for_corner(self, corner: str):
        upper_in, upper_out, lower_in, lower_out = self._corner_points_2d(corner)
        return models.SuspensionGeometry2D(
            track_width=self._read("bf"),
            upper_in=upper_in, upper_out=upper_out,
            lower_in=lower_in, lower_out=lower_out,
            s1=self._read("s1"), s2=self._read("s2"),
            camber_out_deg=self._read("cam_o"), camber_in_deg=self._read("cam_i"),
        )

    # =========================================================================
    # CONSTRUÇÃO DA UI (Top Bar, Sidebar)
    # =========================================================================
    def _build_topbar(self):
        bar = ttk.Frame(self.root, style="Top.TFrame")
        bar.pack(fill="x")

        ttk.Label(bar, text="Ações rápidas", style="Header.TLabel").pack(side="left", padx=(6, 16))

        # Botões de ação direta
        actions = [
            ("Calcular 2D", self._calc_2d),
            ("Calcular 3D", self._calc_3d),
            ("Visual 3D", self._calc_full_3d),
            ("CG Sweep", self._run_cg_sweep),
            ("Mass Sweep", self._run_mass_sweep),
            ("Estrutura", self._run_opt)
        ]
        for txt, cmd in actions:
            ttk.Button(bar, text=txt, style="Action.TButton" if "Visual" in txt or "3D" in txt else "TButton",
                       command=cmd).pack(side="left", padx=4)

        ttk.Separator(bar, orient="vertical").pack(side="left", fill="y", padx=10)

        # Botões de Persistência
        ttk.Button(bar, text="Salvar preset", command=self._save_preset).pack(side="left", padx=4)
        ttk.Button(bar, text="Carregar preset", command=self._load_preset).pack(side="left", padx=4)
        ttk.Button(bar, text="Reset defaults", command=self._reset_defaults).pack(side="left", padx=4)

        # Menus Dropdown (Direita)
        right = ttk.Frame(bar)
        right.pack(side="right", padx=8)

        # Menu Calcular
        btn_calc = ttk.Menubutton(right, text="Calcular ▾", style="Action.TButton")
        m_calc = tk.Menu(btn_calc, tearoff=0)
        btn_calc["menu"] = m_calc
        m_calc.add_command(label="Calcular TODOS", command=self._calc_all)
        m_calc.add_separator()
        for txt, cmd in actions:
            m_calc.add_command(label=txt, command=cmd)
        btn_calc.pack(side="left", padx=(0, 6))

        # Menu Visual
        btn_vis = ttk.Menubutton(right, text="Visual ▾")
        m_vis = tk.Menu(btn_vis, tearoff=0)
        btn_vis["menu"] = m_vis
        m_vis.add_command(label="Ir para Resultados 2D", command=lambda: self._goto_tab("2D"))
        m_vis.add_command(label="Ir para Visualização 3D", command=lambda: self._goto_tab("VIS3D"))
        m_vis.add_separator()
        m_vis.add_command(label="Dinâmica Veicular", command=lambda: self._goto_tab("DYN"))
        m_vis.add_command(label="Otimização", command=lambda: self._goto_tab("OPT"))
        btn_vis.pack(side="left")

    def _build_statusbar(self):
        self.status_var = tk.StringVar(value="Pronto.")
        bar = ttk.Frame(self.root)
        bar.pack(fill="x", side="bottom")
        ttk.Label(bar, textvariable=self.status_var, style="Status.TLabel").pack(side="left", padx=8, pady=3)

    def _init_sidebar_inputs(self, parent):
        # 1) Geometria
        sec_geo = uic.CollapsibleSection(parent, "1) Geometria (Hardpoints) MTOMBO [mm]")
        sec_geo.pack(fill="x", padx=6, pady=6)
        f_geo = sec_geo.body

        self._create_entry(f_geo, "bf", "Bitola (bf):", 1250, 0, 0, width=12,
                           tip="Distância entre centros das rodas (track). Use mm.")

        ttk.Label(f_geo, text="Canto p/ cálculo:").grid(row=0, column=2, sticky="e", padx=3, pady=2)
        cb = ttk.Combobox(f_geo, values=["FR", "FL", "RR", "RL"], textvariable=self.var_corner, width=6, state="readonly")
        cb.grid(row=0, column=3, sticky="w", padx=3, pady=2)
        uic.ToolTip(cb, "Define qual canto será usado nos cálculos de força e ângulos.")

        # --- Gerador Duplo A ---
        gen = uic.CollapsibleSection(f_geo, "Gerador Duplo A (Reimpell - parametrizado)")
        gen.grid(row=1, column=0, columnspan=4, sticky="ew", padx=0, pady=(10, 6))
        f_gen = gen.body

        # Entradas do Gerador
        self._create_entry(f_gen, "z_front", "Z eixo D:", 775, 0, 0, width=10)
        self._create_entry(f_gen, "z_rear", "Z eixo T:", -775, 0, 2, width=10)
        self._create_entry(f_gen, "LsF", "Front L sup:", 450, 1, 0, width=10)
        self._create_entry(f_gen, "LiF", "Front L inf:", 520, 1, 2, width=10)
        self._create_entry(f_gen, "ySupInF", "F y sup in:", 420, 2, 0, width=10)
        self._create_entry(f_gen, "ySupOutF", "F y sup out:", 390, 2, 2, width=10)
        self._create_entry(f_gen, "yInfInF", "F y inf in:", 210, 3, 0, width=10)
        self._create_entry(f_gen, "yInfOutF", "F y inf out:", 190, 3, 2, width=10)

        self._create_entry(f_gen, "LsR", "Rear L sup:", 430, 4, 0, width=10)
        self._create_entry(f_gen, "LiR", "Rear L inf:", 500, 4, 2, width=10)
        self._create_entry(f_gen, "ySupInR", "R y sup in:", 410, 5, 0, width=10)
        self._create_entry(f_gen, "ySupOutR", "R y sup out:", 380, 5, 2, width=10)
        self._create_entry(f_gen, "yInfInR", "R y inf in:", 200, 6, 0, width=10)
        self._create_entry(f_gen, "yInfOutR", "R y inf out:", 180, 6, 2, width=10)

        self._create_entry(f_gen, "dz_inboard", "dz inboard:", 80, 7, 0, width=10)
        self._create_entry(f_gen, "dz_toe", "dz toe:", 60, 7, 2, width=10)
        self._create_entry(f_gen, "dy_toe", "y toe:", 260, 8, 0, width=10)
        self._create_entry(f_gen, "dx_toe_in", "dx toe in:", 520, 8, 2, width=10)
        self._create_entry(f_gen, "dy_damper", "dy damper:", 100, 9, 0, width=10)
        self._create_entry(f_gen, "dx_damper_in", "dx damper in:", 380, 9, 2, width=10)

        ttk.Button(f_gen, text="Gerar Hardpoints + Plot", style="Action.TButton",
                   command=self._apply_double_a_reimpell).grid(row=10, column=0, columnspan=4, sticky="ew", pady=(8, 4))

        # --- Lista de Hardpoints (32 campos) ---
        ttk.Label(f_geo, text="X").grid(row=15, column=1)
        ttk.Label(f_geo, text="Y").grid(row=15, column=2)
        ttk.Label(f_geo, text="Z").grid(row=15, column=3)

        corners = ["FR", "FL", "RR", "RL"]
        pts = ["Sup In", "Sup Out", "Inf In", "Inf Out", "Toe In", "Toe Out", "Damper In", "Damper Out"]
        base_defaults = {
            "Sup In": (450, 420, 200), "Sup Out": (625, 390, 300),
            "Inf In": (430, 210, 200), "Inf Out": (625, 190, 300),
            "Toe In": (520, 260, 120), "Toe Out": (650, 260, 180),
            "Damper In": (380, 520, 150), "Damper Out": (610, 430, 260),
        }

        r = 16
        self.hp_keys = []
        for corner in corners:
            ttk.Label(f_geo, text=f"== {corner} ==", font=("Arial", 9, "bold")).grid(row=r, column=0, columnspan=4, sticky="w", pady=(10, 2))
            r += 1
            for p in pts:
                lbl = self._hp_key(corner, p)
                dx, dy, dz = base_defaults[p]
                if corner in ("FL", "RL"): dx = -dx # Mirror X default

                ttk.Label(f_geo, text=lbl + ":").grid(row=r, column=0, sticky="e")
                ex = ttk.Entry(f_geo, width=8); ex.insert(0, str(dx)); ex.grid(row=r, column=1)
                ey = ttk.Entry(f_geo, width=8); ey.insert(0, str(dy)); ey.grid(row=r, column=2)
                ez = ttk.Entry(f_geo, width=8); ez.insert(0, str(dz)); ez.grid(row=r, column=3)
                self.entries_hp[lbl] = (ex, ey, ez)
                self.hp_keys.append(lbl)
                r += 1

        frm_csv = ttk.Frame(f_geo)
        frm_csv.grid(row=r, column=0, columnspan=4, sticky="ew", pady=(10, 2))
        ttk.Button(frm_csv, text="Export CSV", command=self._export_hardpoints_csv).pack(side="left", padx=4)
        ttk.Button(frm_csv, text="Import CSV", command=self._import_hardpoints_csv).pack(side="left", padx=4)

        # 2) Cinemática
        sec_kin = uic.CollapsibleSection(parent, "2) Cinemática & Alinhamento")
        sec_kin.pack(fill="x", padx=6, pady=6)
        f_kin = sec_kin.body
        self._create_entry(f_kin, "s1", "Curso Ext:", 40, 0, 0)
        self._create_entry(f_kin, "s2", "Curso Int:", 30, 0, 2)
        self._create_entry(f_kin, "cam_o", "Cam Ext:", -1.5, 1, 0)
        self._create_entry(f_kin, "cam_i", "Cam Int:", -4.0, 1, 2)
        self._create_entry(f_kin, "spindle_sup_z", "Spindle Sup Z:", 300, 3, 0)
        self._create_entry(f_kin, "spindle_inf_z", "Spindle Inf Z:", 300, 3, 2)
        self._create_entry(f_kin, "toe_f_x", "Toe F X:", 600, 4, 0)
        self._create_entry(f_kin, "toe_r_x", "Toe R X:", 600, 4, 2)

        # 3) Dinâmica
        sec_dyn = uic.CollapsibleSection(parent, "3) Dinâmica Veicular")
        sec_dyn.pack(fill="x", padx=6, pady=6)
        f_dyn = sec_dyn.body
        self._create_entry(f_dyn, "mass", "Massa (kg):", 200, 0, 0)
        self._create_entry(f_dyn, "hcg", "Altura CG:", 450, 0, 2)
        self._create_entry(f_dyn, "ay", "Ay (m/s²):", 9.8, 1, 0)
        self._create_entry(f_dyn, "wb", "Entre-eixos:", 1550, 1, 2)
        self._create_entry(f_dyn, "fx", "Fx Pneu (N):", 1200, 2, 0)
        self._create_entry(f_dyn, "rs", "Scrub (mm):", 50, 2, 2)
        self._create_entry(f_dyn, "h_min", "CG Min:", 250, 4, 0)
        self._create_entry(f_dyn, "h_max", "CG Max:", 650, 4, 2)
        self._create_entry(f_dyn, "m_min", "Massa Min:", 180, 5, 0)
        self._create_entry(f_dyn, "m_max", "Massa Max:", 240, 5, 2)
        self._create_entry(f_dyn, "step", "Passo:", 25, 6, 0)

        # 4) Otimização
        sec_opt = uic.CollapsibleSection(parent, "4) Otimização Estrutural")
        sec_opt.pack(fill="x", padx=6, pady=6)
        f_opt = sec_opt.body
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
    # LÓGICA: GENERATOR (REIMPELL)
    # =========================================================================
    def _sign_from_corner(self, corner: str) -> float:
        return -1.0 if corner.upper().endswith("L") else 1.0

    def _axle_z_from_corner(self, corner: str) -> float:
        return self._read("z_front") if corner.upper().startswith("F") else self._read("z_rear")

    def _apply_double_a_reimpell(self):
        try:
            bf = self._read("bf")
            half_track = 0.5 * bf

            # Params
            LsF, LiF = self._read("LsF"), self._read("LiF")
            ySupInF, ySupOutF = self._read("ySupInF"), self._read("ySupOutF")
            yInfInF, yInfOutF = self._read("yInfInF"), self._read("yInfOutF")

            LsR, LiR = self._read("LsR"), self._read("LiR")
            ySupInR, ySupOutR = self._read("ySupInR"), self._read("ySupOutR")
            yInfInR, yInfOutR = self._read("yInfInR"), self._read("yInfOutR")

            dz_in, dz_toe = self._read("dz_inboard"), self._read("dz_toe")
            dy_toe, dx_toe_in = self._read("dy_toe"), self._read("dx_toe_in")
            dy_damper, dx_damper_in = self._read("dy_damper"), self._read("dx_damper_in")

            corners = ["FR", "FL", "RR", "RL"]
            for c in corners:
                sgn = self._sign_from_corner(c)
                z_axle = self._axle_z_from_corner(c)

                if c.startswith("F"):
                    Ls, Li = LsF, LiF
                    ySupIn, ySupOut = ySupInF, ySupOutF
                    yInfIn, yInfOut = yInfInF, yInfOutF
                else:
                    Ls, Li = LsR, LiR
                    ySupIn, ySupOut = ySupInR, ySupOutR
                    yInfIn, yInfOut = yInfInR, yInfOutR

                x_out = sgn * half_track
                z_out = z_axle

                x_sup_in = sgn * (half_track - Ls)
                x_inf_in = sgn * (half_track - Li)
                z_sup_in = z_axle - dz_in
                z_inf_in = z_axle + dz_in

                self._hp_set(self._hp_key(c, "Sup Out"), x_out, ySupOut, z_out)
                self._hp_set(self._hp_key(c, "Inf Out"), x_out, yInfOut, z_out)
                self._hp_set(self._hp_key(c, "Sup In"), x_sup_in, ySupIn, z_sup_in)
                self._hp_set(self._hp_key(c, "Inf In"), x_inf_in, yInfIn, z_inf_in)

                self._hp_set(self._hp_key(c, "Toe Out"), x_out, dy_toe, z_axle + dz_toe)
                self._hp_set(self._hp_key(c, "Toe In"), sgn * (half_track - dx_toe_in), dy_toe, z_axle - dz_toe)

                self._hp_set(self._hp_key(c, "Damper Out"), x_out, ySupOut + 0.5 * (dy_damper), z_axle)
                self._hp_set(self._hp_key(c, "Damper In"), sgn * (half_track - dx_damper_in), ySupIn + dy_damper, z_axle)

            self._set_status("Hardpoints Duplo A gerados.")
            self._safe_call(self._calc_2d, "Auto Calc 2D")
            self._safe_call(self._calc_full_3d, "Auto Calc 3D")

        except Exception as e:
            self._set_status(f"Erro Reimpell: {e}")
            messagebox.showerror("Erro", str(e))

    # =========================================================================
    # LÓGICA: CÁLCULOS (2D, 3D, OPT, DYN)
    # =========================================================================
    def _calc_all(self):
        ok2d = self._safe_call(self._calc_2d, "Calcular 2D")
        ok3d = self._safe_call(self._calc_3d, "Calcular 3D")
        okvis = self._safe_call(self._calc_full_3d, "Visual 3D")
        okcg = self._safe_call(self._run_cg_sweep, "CG Sweep")
        okm = self._safe_call(self._run_mass_sweep, "Mass Sweep")
        okopt = self._safe_call(self._run_opt, "Estrutura")

        if all([ok2d, ok3d, okvis, okcg, okm, okopt]):
            self._set_status("✅ TODOS os cálculos concluídos.")

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

            def fmt_ic(rc): return f"({rc.ic.x:.1f}, {rc.ic.y:.1f})" if getattr(rc, "ic", None) else "Paralelo"
            def fmt_hro(rc): return f"{rc.h_ro:.1f} mm" if getattr(rc, "h_ro", None) is not None else "--"

            self.lbl_2d_res.config(text=(
                f"Eixo: {corner_R[:-1]} | Seleção: {corner_sel}\n"
                f"DIR: IC {fmt_ic(rcR)} | h_Ro: {fmt_hro(rcR)}\n"
                f"ESQ: IC {fmt_ic(rcL)} | h_Ro: {fmt_hro(rcL)}\n"
                f"Rolagem (dPhi): {camR.d_phi_deg:.3f}° | Camber Gain: {camR.k_gamma:.3f}"
            ))

            ax = self.ax_2d
            ax.clear()

            def plot_corner(geo, rc, label, col):
                ax.plot([geo.upper_in.x, geo.upper_out.x], [geo.upper_in.y, geo.upper_out.y],
                        "o--", label=f"{label} Upper", color=col)
                ax.plot([geo.lower_in.x, geo.lower_out.x], [geo.lower_in.y, geo.lower_out.y],
                        "o-", label=f"{label} Lower", color=col)
                if getattr(rc, "ic", None):
                    ax.plot(rc.ic.x, rc.ic.y, "s", color=col)
                if getattr(rc, "h_ro", None) is not None:
                    ax.plot(0, rc.h_ro, "*", markersize=12, color=col)

            plot_corner(geoR, rcR, "DIR", "tab:blue")
            plot_corner(geoL, rcL, "ESQ", "tab:red")

            ax.axvline(0, linestyle=":", color="k")
            ax.set_title(f"Geometria 2D Frontal")
            ax.grid(True)
            ax.legend(fontsize=8)
            ax.set_aspect("equal", adjustable="datalim")
            self.canvas_2d.draw()
            self.content_area.select(self.tab_2d)

        except Exception as e:
            self._set_status(f"Erro 2D: {e}")
            raise e

    def _calc_3d(self):
        try:
            corner = (self.var_corner.get().strip() or "FR").upper()
            sup_in = self._get_point3d_from_input(self._hp_key(corner, "Sup In"))
            sup_out = self._get_point3d_from_input(self._hp_key(corner, "Sup Out"))
            inf_in = self._get_point3d_from_input(self._hp_key(corner, "Inf In"))
            inf_out = self._get_point3d_from_input(self._hp_key(corner, "Inf Out"))

            sup_out_x, sup_out_y, _ = self._hp_get(self._hp_key(corner, "Sup Out"))
            inf_out_x, inf_out_y, _ = self._hp_get(self._hp_key(corner, "Inf Out"))

            geo3d = models.SuspensionGeometry3D(
                sup_in=sup_in, sup_out=sup_out, inf_in=inf_in, inf_out=inf_out,
                spindle_sup=models.Point3D(sup_out_x, sup_out_y, self._read("spindle_sup_z")),
                spindle_inf=models.Point3D(inf_out_x, inf_out_y, self._read("spindle_inf_z")),
                toe_front=models.Point3D(self._read("toe_f_x"), 0, 0),
                toe_rear=models.Point3D(self._read("toe_r_x"), 0, 0),
                stiffness_ratio_sup=1.0, stiffness_ratio_inf=1.0,
                fx_tire=self._read("fx"),
            )

            forces = math_3d.calculate_forces(geo3d)
            self.last_3d_results = forces

            if forces:
                self.lbl_3d_res.config(text=(
                    f"Canto: {corner}\n"
                    f"F Axial Sup: {forces.upper.axial:.1f} N\n"
                    f"F Axial Inf: {forces.lower.axial:.1f} N"
                ))
            self.content_area.select(self.tab_3d)

        except Exception as e:
            self._set_status(f"Erro 3D: {e}")
            raise e

    def _calc_full_3d(self):
        """Plota os 32 hardpoints usando o módulo visualization."""
        try:
            ax = self.ax_vis3d
            ax.clear()

            corners = ["FR", "FL", "RR", "RL"]
            pts_types = ["Sup In", "Sup Out", "Inf In", "Inf Out", "Toe In", "Toe Out", "Damper In", "Damper Out"]
            
            pts = {}
            for c in corners:
                for p in pts_types:
                    key = self._hp_key(c, p)
                    try:
                        pts[key] = self._get_point3d_from_input(key)
                    except: pass

            for c in corners:
                col = "blue" if c.endswith("R") else "red"
                vis.plot_suspension_link(ax, pts.get(f"{c} Sup In"), pts.get(f"{c} Sup Out"), col, "--")
                vis.plot_suspension_link(ax, pts.get(f"{c} Inf In"), pts.get(f"{c} Inf Out"), col, "--")
                vis.plot_suspension_link(ax, pts.get(f"{c} Toe In"), pts.get(f"{c} Toe Out"), "green", "--")
                vis.plot_suspension_link(ax, pts.get(f"{c} Damper In"), pts.get(f"{c} Damper Out"), "purple", "-")

                # Plota bolinhas
                for p_name in pts_types:
                    pt = pts.get(f"{c} {p_name}")
                    if pt:
                        ax.scatter(pt.x, pt.z, pt.y, c="k", s=15)

            valid_pts = [p for p in pts.values() if p]
            step = float(getattr(self, "var_grid_step", tk.DoubleVar(value=50.0)).get())
            
            vis.draw_ground_mesh(ax, step=step)
            vis.autoscale_3d_equal(ax, valid_pts)

            ax.set_xlabel("X (Lateral)"); ax.set_ylabel("Z (Longitudinal)"); ax.set_zlabel("Y (Vertical)")
            
            self.canvas_vis3d.draw_idle()
            if not self.free_view:
                self._enforce_yz_view()
            
            self._set_status("Visualização 3D gerada.")
            self.content_area.select(self.tab_vis3d)

        except Exception as e:
            self._set_status(f"Erro Vis 3D: {e}")
            print(e)

    def _run_cg_sweep(self):
        try:
            h_ro = getattr(self.last_2d_results, "h_ro", 100.0)
            if h_ro is None: h_ro = 100.0

            params = models.SuspensionCGScanParameters(
                h_min=self._read("h_min"), h_max=self._read("h_max"), h_step=self._read("step"),
                mass=self._read("mass"), ay=self._read("ay"), track=self._read("bf"),
                h_ro=h_ro, scrub_radius=self._read("rs"), clearance=0.0
            )
            results = list(dynamics.calculate_baja_cg_sweep(params))

            for i in self.tree_cg.get_children(): self.tree_cg.delete(i)
            for r in results:
                self.tree_cg.insert("", "end", values=(f"{r.h_cg:.1f}", f"{r.d_fz:.1f}", f"{r.fz_int:.1f}", f"{r.m_roll:.1f}"))

            self.ax_cg1.clear(); self.ax_cg2.clear()
            self.ax_cg1.plot([r.h_cg for r in results], [r.d_fz for r in results], "o-")
            self.ax_cg1.set_title("Transf. Carga vs CG"); self.ax_cg1.grid(True)
            self.ax_cg2.plot([r.h_cg for r in results], [r.m_roll for r in results], "o-")
            self.ax_cg2.set_title("Momento Roll vs CG"); self.ax_cg2.grid(True)
            self.canvas_cg.draw()
            self.content_area.select(self.tab_dyn)

        except Exception as e:
            messagebox.showerror("Erro CG", str(e))

    def _run_mass_sweep(self):
        try:
            h_ro = getattr(self.last_2d_results, "h_ro", 100.0)
            if h_ro is None: h_ro = 100.0

            params = models.SuspensionMassScanParameters(
                m_min=self._read("m_min"), m_max=self._read("m_max"), m_step=self._read("step"),
                h_cg=self._read("hcg"), ay=self._read("ay"), track=self._read("bf"), h_ro=h_ro
            )
            results = list(dynamics.calculate_mass_sweep(params))

            for i in self.tree_mass.get_children(): self.tree_mass.delete(i)
            for r in results:
                margin = getattr(r, "margin", 0)
                self.tree_mass.insert("", "end", values=(
                    f"{r.mass:.0f}", f"{r.m_roll:.1f}", f"{r.d_fz:.1f}",
                    f"{r.ssf:.3f}", f"{getattr(r, 'ay_crit', 0):.2f}", f"{margin:.2f}"
                ))
            self.content_area.select(self.tab_dyn)

        except Exception as e:
            messagebox.showerror("Erro Mass", str(e))

    def _run_opt(self):
        try:
            if self.var_auto_arm_angles.get():
                ang_sup, ang_inf = self._compute_arm_angles_deg(self.var_corner.get())
                self._set("ang_sup", f"{ang_sup:.3f}"); self._set("ang_inf", f"{ang_inf:.3f}")
            else:
                ang_sup, ang_inf = self._read("ang_sup"), self._read("ang_inf")

            results = list(dynamics.calculate_force_vs_angle_sweep(
                models.ForceAngleIteratorParameters(
                    f_load=self._read("load"), angle_sup_base=ang_sup, angle_inf_base=ang_inf,
                    k_sup=self._read("ksup"), k_inf=1.0 - self._read("ksup"),
                    limit=self._read("limit"), ang_min=int(self._read("amin")), ang_max=int(self._read("amax")), step=5
                )
            ))

            for i in self.tree_opt.get_children(): self.tree_opt.delete(i)
            angles, f_sup, f_inf = [], [], []
            for r in results:
                self.tree_opt.insert("", "end", values=(f"{r.angle_delta}", f"{r.force_sup:.1f}", f"{r.force_inf:.1f}", r.status))
                angles.append(r.angle_delta); f_sup.append(r.force_sup); f_inf.append(r.force_inf)

            self.ax_opt.clear()
            self.ax_opt.plot(angles, f_sup, label="F Sup", marker="o")
            self.ax_opt.plot(angles, f_inf, label="F Inf", marker="o")
            self.ax_opt.axhline(self._read("limit"), linestyle="--", color="r")
            self.ax_opt.grid(True); self.ax_opt.legend()
            self.canvas_opt.draw()
            self.content_area.select(self.tab_opt)

        except Exception as e:
            messagebox.showerror("Erro Opt", str(e))

    def _compute_arm_angles_deg(self, corner: str):
        c = (corner or "FR").strip().upper()
        s_in = self._get_point3d_from_input(self._hp_key(c, "Sup In"))
        s_out = self._get_point3d_from_input(self._hp_key(c, "Sup Out"))
        i_in = self._get_point3d_from_input(self._hp_key(c, "Inf In"))
        i_out = self._get_point3d_from_input(self._hp_key(c, "Inf Out"))
        
        # Angulo com a horizontal (Y/X)
        asup = math.degrees(math.atan2(s_out.y - s_in.y, s_out.x - s_in.x))
        ainf = math.degrees(math.atan2(i_out.y - i_in.y, i_out.x - i_in.x))
        return asup, ainf

    # =========================================================================
    # VIEW CONTROL & TABS
    # =========================================================================
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

    def _goto_tab(self, name):
        tabs = {"2D": self.tab_2d, "3D": self.tab_3d, "VIS3D": self.tab_vis3d, "DYN": self.tab_dyn, "OPT": self.tab_opt}
        if name in tabs: self.content_area.select(tabs[name])

    # =========================================================================
    # SETUP DAS ABAS (Views)
    # =========================================================================
    def _init_result_tabs(self):
        self.tab_2d = ttk.Frame(self.content_area)
        self.tab_3d = ttk.Frame(self.content_area)
        self.tab_vis3d = ttk.Frame(self.content_area)
        self.tab_dyn = ttk.Frame(self.content_area)
        self.tab_opt = ttk.Frame(self.content_area)

        self.content_area.add(self.tab_2d, text="Resultados 2D")
        self.content_area.add(self.tab_3d, text="Resultados 3D")
        self.content_area.add(self.tab_vis3d, text="Visualização 3D")
        self.content_area.add(self.tab_dyn, text="Dinâmica Veicular")
        self.content_area.add(self.tab_opt, text="Otimização")

        self._setup_res_2d(self.tab_2d)
        self._setup_res_3d(self.tab_3d)
        self._setup_res_vis3d(self.tab_vis3d)
        self._setup_res_dyn(self.tab_dyn)
        self._setup_res_opt(self.tab_opt)

    def _setup_res_2d(self, parent):
        card = ttk.Frame(parent, style="Card.TFrame"); card.pack(fill="x", padx=12, pady=10)
        self.lbl_2d_res = ttk.Label(card, text="Aguardando cálculo...", font=("Courier", 10))
        self.lbl_2d_res.pack(anchor="w")
        
        self.fig_2d, self.ax_2d = plt.subplots(figsize=(5, 4))
        self.canvas_2d = FigureCanvasTkAgg(self.fig_2d, master=parent)
        self.canvas_2d.get_tk_widget().pack(fill="both", expand=True, padx=12)

    def _setup_res_3d(self, parent):
        card = ttk.Frame(parent, style="Card.TFrame"); card.pack(fill="x", padx=12, pady=10)
        self.lbl_3d_res = ttk.Label(card, text="Aguardando cálculo...", font=("Courier", 10))
        self.lbl_3d_res.pack(anchor="w")

    def _setup_res_vis3d(self, parent):
        ctrl = ttk.Frame(parent, padding=8); ctrl.pack(fill="x")
        ttk.Button(ctrl, text="GERAR DIAGRAMA", style="Action.TButton", command=self._calc_full_3d).pack(side="left")
        ttk.Button(ctrl, text="Reset View", style="Reset.TButton", command=self._reset_view).pack(side="right")
        self.btn_free_view = ttk.Button(ctrl, text="Visualização: FIXA (YZ)", command=self._toggle_free_view)
        self.btn_free_view.pack(side="right", padx=8)

        self.var_grid_step = tk.DoubleVar(value=50.0)
        ttk.Scale(ctrl, from_=10, to=100, variable=self.var_grid_step, orient="horizontal").pack(side="left", padx=10)

        self.fig_vis3d = plt.figure(figsize=(5, 5))
        self.ax_vis3d = self.fig_vis3d.add_subplot(111, projection="3d")
        self.canvas_vis3d = FigureCanvasTkAgg(self.fig_vis3d, master=parent)
        self.canvas_vis3d.get_tk_widget().pack(fill="both", expand=True)
        NavigationToolbar2Tk(self.canvas_vis3d, parent).pack(fill="x")
        self._apply_view_mode()

    def _setup_res_dyn(self, parent):
        nb = ttk.Notebook(parent); nb.pack(fill="both", expand=True)
        f_cg = ttk.Frame(nb); nb.add(f_cg, text="CG Sweep")
        f_mass = ttk.Frame(nb); nb.add(f_mass, text="Mass Sweep")

        # CG
        self.fig_cg, (self.ax_cg1, self.ax_cg2) = plt.subplots(1, 2, figsize=(8, 3))
        self.canvas_cg = FigureCanvasTkAgg(self.fig_cg, master=f_cg)
        self.canvas_cg.get_tk_widget().pack(fill="both", expand=True)
        
        self.tree_cg = ttk.Treeview(f_cg, columns=("CG", "dFz", "Fz_int", "M_Roll"), show="headings", height=5)
        for c in ("CG", "dFz", "Fz_int", "M_Roll"): self.tree_cg.heading(c, text=c)
        self.tree_cg.pack(fill="x")

        # Mass
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
        self.canvas_opt = FigureCanvasTkAgg(self.fig_opt, master=f_plt)
        self.canvas_opt.get_tk_widget().pack(fill="both", expand=True)
        paned.add(f_plt)

    # =========================================================================
    # PERSISTÊNCIA (JSON / CSV)
    # =========================================================================
    def _collect_preset(self) -> dict:
        data = {k: e.get() for k, e in self.entries.items()}
        hp = {lbl: {"x": v[0].get(), "y": v[1].get(), "z": v[2].get()} for lbl, v in self.entries_hp.items()}
        data["_hardpoints"] = hp
        data["_static"] = self.var_static.get()
        data["_corner"] = self.var_corner.get()
        return data

    def _apply_preset(self, data: dict):
        for k, e in self.entries.items():
            if k in data: e.delete(0, tk.END); e.insert(0, str(data[k]))
        
        hp = data.get("_hardpoints", {})
        for lbl, coords in hp.items():
            if lbl in self.entries_hp:
                ex, ey, ez = self.entries_hp[lbl]
                ex.delete(0, tk.END); ex.insert(0, str(coords.get("x", ex.get())))
                ey.delete(0, tk.END); ey.insert(0, str(coords.get("y", ey.get())))
                ez.delete(0, tk.END); ez.insert(0, str(coords.get("z", ez.get())))
        
        if "_static" in data: self.var_static.set(data["_static"])
        if "_corner" in data: self.var_corner.set(data["_corner"])

    def _save_preset(self):
        path = filedialog.asksaveasfilename(defaultextension=".json", filetypes=[("JSON", "*.json")])
        if path:
            with open(path, "w") as f: json.dump(self._collect_preset(), f, indent=2)

    def _load_preset(self):
        path = filedialog.askopenfilename(filetypes=[("JSON", "*.json")])
        if path:
            with open(path, "r") as f: self._apply_preset(json.load(f))

    def _export_hardpoints_csv(self):
        path = filedialog.asksaveasfilename(defaultextension=".csv", filetypes=[("CSV", "*.csv")])
        if path:
            rows = [["bf", self.entries["bf"].get(), "", ""]]
            for lbl in self.hp_keys:
                x, y, z = self._hp_get(lbl)
                rows.append([lbl, x, y, z])
            with open(path, "w", newline="") as f:
                csv.writer(f).writerows([["key", "x", "y", "z"]] + rows)

    def _import_hardpoints_csv(self):
        path = filedialog.askopenfilename(filetypes=[("CSV", "*.csv")])
        if path:
            try:
                with open(path, "r") as f:
                    for row in csv.DictReader(f):
                        k = row.get("key")
                        if k == "bf" and "x" in row: self._set("bf", row["x"])
                        elif k in self.entries_hp: self._hp_set(k, row.get("x", 0), row.get("y", 0), row.get("z", 0))
                self._set_status("CSV importado.")
            except Exception as e: messagebox.showerror("Erro CSV", str(e))

    def _reset_defaults(self):
        self._set_status("Defaults restaurados. (Implementar dicionário se necessário)")

if __name__ == "__main__":
    root = tk.Tk()
    app = App(root)
    root.mainloop()