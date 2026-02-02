import json
import tkinter as tk
from tkinter import ttk, messagebox, filedialog

import matplotlib.pyplot as plt
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg

# Domain modules
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

        # Mouse wheel only when pointer is over the canvas
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


# =========================
# Main App (UNIFICADO)
# =========================

class App:
    def __init__(self, root: tk.Tk):
        self.root = root
        self.root.title("Suíte de Engenharia de Suspensão (2D/3D/Visual/Dinâmica/Estrutura)")
        self.root.geometry("1400x900")

        self._setup_style()

        # Shared State
        self.last_2d_results = None
        self.last_3d_results = None

        # Entries
        self.entries = {}     # all entry widgets by key
        self.entries_hp = {}  # hardpoints grouped

        # Topbar
        self._build_topbar()

        # Main Layout
        self.paned = tk.PanedWindow(root, orient="horizontal", sashwidth=6, bg="#b0b0b0")
        self.paned.pack(fill="both", expand=True)

        # Sidebar
        self.sidebar_container = ScrollableFrame(self.paned, width=410)
        self.paned.add(self.sidebar_container, minsize=400, width=420)

        # Content
        self.content_area = ttk.Notebook(self.paned)
        self.paned.add(self.content_area, minsize=700)

        # Build UI
        self._init_sidebar_inputs(self.sidebar_container.scrollable_window)
        self._init_result_tabs()
        self._build_statusbar()

        # Default view
        self._reset_view()

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

    def _build_statusbar(self):
        self.status_var = tk.StringVar(value="Pronto.")
        bar = ttk.Frame(self.root)
        bar.pack(fill="x", side="bottom")
        ttk.Label(bar, textvariable=self.status_var, style="Status.TLabel").pack(side="left", padx=8, pady=3)

    def _set_status(self, msg: str):
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
        # 1) Geometria
        sec_geo = CollapsibleSection(parent, "1) Geometria (Hardpoints) [mm]")
        sec_geo.pack(fill="x", padx=6, pady=6)

        f_geo = sec_geo.body
        self._create_entry(f_geo, "bf", "Bitola (bf):", 1250, 0, 0, width=12,
                           tip="Distância entre centros das rodas (track). Use mm.")

        ttk.Label(f_geo, text="X").grid(row=1, column=1)
        ttk.Label(f_geo, text="Y").grid(row=1, column=2)
        ttk.Label(f_geo, text="Z").grid(row=1, column=3)

        labels = ["Sup In:", "Sup Out:", "Inf In:", "Inf Out:"]
        defaults = [
            (450, 420, 200),
            (625, 390, 300),
            (430, 210, 200),
            (625, 190, 300)
        ]

        for i, (lbl, (dx, dy, dz)) in enumerate(zip(labels, defaults), start=2):
            ttk.Label(f_geo, text=lbl).grid(row=i, column=0, sticky="e")
            ex = ttk.Entry(f_geo, width=7); ex.insert(0, str(dx)); ex.grid(row=i, column=1)
            ey = ttk.Entry(f_geo, width=7); ey.insert(0, str(dy)); ey.grid(row=i, column=2)
            ez = ttk.Entry(f_geo, width=7); ez.insert(0, str(dz)); ez.grid(row=i, column=3)
            self.entries_hp[lbl] = (ex, ey, ez)

            ToolTip(ex, f"{lbl} coordenada X (mm).")
            ToolTip(ey, f"{lbl} coordenada Y (mm).")
            ToolTip(ez, f"{lbl} coordenada Z (mm).")

        # 2) Cinemática
        sec_kin = CollapsibleSection(parent, "2) Cinemática & Alinhamento")
        sec_kin.pack(fill="x", padx=6, pady=6)
        f_kin = sec_kin.body

        self._create_entry(f_kin, "s1", "Curso Ext (mm):", 40, 0, 0, tip="Deslocamento vertical da roda externa (mm).")
        self._create_entry(f_kin, "s2", "Curso Int (mm):", 30, 0, 2, tip="Deslocamento vertical da roda interna (mm).")
        self._create_entry(f_kin, "cam_o", "Camber Ext (°):", -1.5, 1, 0, tip="Cambagem da roda externa (graus).")
        self._create_entry(f_kin, "cam_i", "Camber Int (°):", -4.0, 1, 2, tip="Cambagem da roda interna (graus).")

        ttk.Label(f_kin, text="Auxiliares 3D").grid(row=2, column=0, columnspan=4, pady=(8, 2), sticky="w")

        self._create_entry(f_kin, "spindle_sup_z", "Spindle Sup Z:", 300, 3, 0, tip="Z do ponto superior no spindle (mm).")
        self._create_entry(f_kin, "spindle_inf_z", "Spindle Inf Z:", 300, 3, 2, tip="Z do ponto inferior no spindle (mm).")
        self._create_entry(f_kin, "toe_f_x", "Toe Front X:", 600, 4, 0, tip="X do ponto frontal da barra de direção (mm).")
        self._create_entry(f_kin, "toe_r_x", "Toe Rear X:", 600, 4, 2, tip="X do ponto traseiro da barra de direção (mm).")

        # 3) Dinâmica
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

        # 4) Estrutura
        sec_opt = CollapsibleSection(parent, "4) Otimização Estrutural")
        sec_opt.pack(fill="x", padx=6, pady=6)
        f_opt = sec_opt.body

        self._create_entry(f_opt, "load", "Carga F [N]:", 3000, 0, 0)
        self.var_static = tk.IntVar(value=0)
        ttk.Checkbutton(f_opt, text="+ Estática (mg/4)", variable=self.var_static).grid(row=0, column=2, columnspan=2, sticky="w")

        self._create_entry(f_opt, "ang_sup", "Ang Base Sup:", 10, 2, 0)
        self._create_entry(f_opt, "ang_inf", "Ang Base Inf:", 20, 2, 2)
        self._create_entry(f_opt, "limit", "Limite (N):", 8000, 3, 0)
        self._create_entry(f_opt, "ksup", "K Sup:", 0.5, 3, 2)

        self._create_entry(f_opt, "amin", "Varredura Min:", 0, 4, 0)
        self._create_entry(f_opt, "amax", "Varredura Max:", 40, 4, 2)

    # -------------------------
    # Tabs (Resultados)
    # -------------------------
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
        # Barra de controle
        ctrl = ttk.Frame(parent, padding=8)
        ctrl.pack(fill="x")

        ttk.Button(ctrl, text="GERAR DIAGRAMA 3D (Ref: Pneu TR Esq)", style="Action.TButton",
                   command=self._calc_full_3d).pack(side="left", fill="x", expand=True, padx=2)

        ttk.Button(ctrl, text="Resetar Visualização", style="Reset.TButton",
                   command=self._reset_view).pack(side="right", padx=2)

        self.lbl_steer_data = ttk.Label(parent, text="Dados: --", font=("Courier", 10))
        self.lbl_steer_data.pack(pady=5)

        self.fig_vis3d = plt.figure(figsize=(6, 5))
        self.ax_vis3d = self.fig_vis3d.add_subplot(111, projection="3d")
        self.canvas_vis3d = FigureCanvasTkAgg(self.fig_vis3d, master=parent)
        self.canvas_vis3d.get_tk_widget().pack(fill="both", expand=True, padx=10, pady=10)

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

        top = ttk.Frame(parent, padding=(12, 0, 12, 0))
        top.place_forget()  # não usado, mas deixei caso você queira por botões aqui
        # (Você já tem no topbar)

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

    # =========================
    # Domain data gathering
    # =========================
    def _get_point3d_from_input(self, key):
        ex, ey, ez = self.entries_hp[key]
        return models.Point3D(safe_num(ex.get()), safe_num(ey.get()), safe_num(ez.get()))

    def _get_geo2d(self):
        return models.SuspensionGeometry2D(
            track_width=self._read("bf"),
            upper_in=models.Point2D(safe_num(self.entries_hp["Sup In:"][0].get()), safe_num(self.entries_hp["Sup In:"][1].get())),
            upper_out=models.Point2D(safe_num(self.entries_hp["Sup Out:"][0].get()), safe_num(self.entries_hp["Sup Out:"][1].get())),
            lower_in=models.Point2D(safe_num(self.entries_hp["Inf In:"][0].get()), safe_num(self.entries_hp["Inf In:"][1].get())),
            lower_out=models.Point2D(safe_num(self.entries_hp["Inf Out:"][0].get()), safe_num(self.entries_hp["Inf Out:"][1].get())),
            s1=self._read("s1"),
            s2=self._read("s2"),
            camber_out_deg=self._read("cam_o"),
            camber_in_deg=self._read("cam_i"),
        )

    def _get_geo3d(self):
        return models.SuspensionGeometry3D(
            sup_in=self._get_point3d_from_input("Sup In:"),
            sup_out=self._get_point3d_from_input("Sup Out:"),
            inf_in=self._get_point3d_from_input("Inf In:"),
            inf_out=self._get_point3d_from_input("Inf Out:"),
            spindle_sup=models.Point3D(
                safe_num(self.entries_hp["Sup Out:"][0].get()),
                safe_num(self.entries_hp["Sup Out:"][1].get()),
                self._read("spindle_sup_z")
            ),
            spindle_inf=models.Point3D(
                safe_num(self.entries_hp["Inf Out:"][0].get()),
                safe_num(self.entries_hp["Inf Out:"][1].get()),
                self._read("spindle_inf_z")
            ),
            toe_front=models.Point3D(self._read("toe_f_x"), 0, 0),
            toe_rear=models.Point3D(self._read("toe_r_x"), 0, 0),
            stiffness_ratio_sup=1.0,
            stiffness_ratio_inf=1.0,
            fx_tire=self._read("fx"),
        )

    # =========================
    # Actions: 2D
    # =========================
    def _calc_2d(self):
        try:
            geo2d = self._get_geo2d()
            rc_res = math_2d.calculate_roll_center(geo2d)
            cam_res = math_2d.calculate_camber_gain(geo2d)
            self.last_2d_results = rc_res

            ic_str = f"({rc_res.ic.x:.1f}, {rc_res.ic.y:.1f})" if getattr(rc_res, "ic", None) else "Paralelo"
            h_ro_val = getattr(rc_res, "h_ro", None)
            h_ro_str = f"{h_ro_val:.1f} mm" if h_ro_val is not None else "--"

            self.lbl_2d_res.config(text=(
                f"IC: {ic_str} mm\n"
                f"Altura RC (h_Ro): {h_ro_str}\n"
                f"fator-q: {getattr(rc_res, 'q_factor', '--')}\n"
                f"Rolagem (dPhi): {cam_res.d_phi_deg:.3f} deg\n"
                f"Fator Camber (kGamma): {cam_res.k_gamma:.3f}"
            ))

            self.ax_2d.clear()
            self.ax_2d.plot([geo2d.upper_in.x, geo2d.upper_out.x], [geo2d.upper_in.y, geo2d.upper_out.y],
                            marker="o", label="Superior")
            self.ax_2d.plot([geo2d.lower_in.x, geo2d.lower_out.x], [geo2d.lower_in.y, geo2d.lower_out.y],
                            marker="o", label="Inferior")

            if getattr(rc_res, "ic", None):
                self.ax_2d.plot(rc_res.ic.x, rc_res.ic.y, marker="s", linestyle="None", label="IC")

            # FIX: plota RC mesmo se h_ro for 0 ou negativo
            if h_ro_val is not None:
                self.ax_2d.plot(0, h_ro_val, marker="*", linestyle="None", markersize=12, label="RC")

            self.ax_2d.grid(True)
            self.ax_2d.legend()
            self.ax_2d.set_title("Geometria Vista Frontal")
            self.canvas_2d.draw()

            self._set_status("2D calculado com sucesso.")
            self.content_area.select(self.tab_2d)
        except Exception as e:
            self._set_status(f"Erro 2D: {e}")
            messagebox.showerror("Erro 2D", str(e))

    # =========================
    # Actions: 3D (forças/alinhamento)
    # =========================
    def _calc_3d(self):
        try:
            geo3d = self._get_geo3d()

            forces = math_3d.calculate_forces(geo3d)
            self.last_3d_results = forces

            # Alguns módulos podem não ter isso — protegemos:
            align = math_3d.calculate_alignment(geo3d) if hasattr(math_3d, "calculate_alignment") else None
            anti_dive = None
            if hasattr(math_3d, "calculate_anti_dive") and forces:
                fy_total = getattr(getattr(forces, "total", None), "fy", 0) or 0
                anti_dive = math_3d.calculate_anti_dive(
                    geo3d.fx_tire,
                    fy_total,
                    self._read("hcg"),
                    self._read("wb"),
                )

            if forces:
                lines = [
                    "== FORÇAS AXIAIS ==",
                    f"Sup: {forces.upper.axial:.1f} N",
                    f"Inf: {forces.lower.axial:.1f} N",
                ]
                if anti_dive is not None:
                    lines += ["", "== DINÂMICA ==", f"Anti-Dive: {anti_dive:.1f} %"]

                if align is not None:
                    lines += [
                        "",
                        "== ALINHAMENTO ==",
                        f"Camber: {getattr(align,'camber',0):.2f} deg",
                        f"Caster: {getattr(align,'caster',0):.2f} deg"
                    ]

                self.lbl_3d_res.config(text="\n".join(lines))
                self._set_status("3D calculado com sucesso.")
                self.content_area.select(self.tab_3d)
            else:
                self._set_status("Erro: geometria 3D inválida.")
                self.lbl_3d_res.config(text="Erro: Geometria 3D inválida.")
        except Exception as e:
            self._set_status(f"Erro 3D: {e}")
            messagebox.showerror("Erro 3D", str(e))

    # =========================
    # Visualização 3D (do Código 1)
    # =========================
    def _reset_view(self):
        if hasattr(self, "ax_vis3d"):
            self.ax_vis3d.view_init(elev=20, azim=-60)
            if hasattr(self, "canvas_vis3d"):
                self.canvas_vis3d.draw()

    def _calc_full_3d(self):
        """
        Visualização avançada:
        - Rótulos de texto para cada ponto.
        - Linhas tracejadas para os braços.
        - Linhas de projeção até o chão.
        """
        try:
            wb = self._read("wb")
            bf = self._read("bf")
            rs = self._read("rs")

            geo = models.VehicleGeometry(
                wb, bf, bf,
                self._get_point3d_from_input("Sup In:"), self._get_point3d_from_input("Sup Out:"),
                self._get_point3d_from_input("Inf In:"), self._get_point3d_from_input("Inf Out:"),
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
                    # Modelo(X,Y,Z) -> Matplotlib(X,Z,Y)
                    ax.plot([p1.x, p2.x], [p1.z, p2.z], [p1.y, p2.y],
                            color=color, linestyle=style, linewidth=linewidth)

            def plot_projection(p_name):
                if p_name in pts:
                    p = pts[p_name]
                    ax.plot([p.x, p.x], [p.z, p.z], [p.y, 0],
                            color='gray', linestyle=':', linewidth=0.8, alpha=0.5)

            # Frente Direita
            plot_link("FR Sup In", "FR Sup Out", "blue", "--")
            plot_link("FR Inf In", "FR Inf Out", "blue", "--")
            plot_link("FR Sup Out", "FR Inf Out", "black", "-")
            plot_link("FR Tie In", "FR Tie Out", "green", "--")

            # Frente Esquerda
            plot_link("FL Sup In", "FL Sup Out", "red", "--")
            plot_link("FL Inf In", "FL Inf Out", "red", "--")
            plot_link("FL Sup Out", "FL Inf Out", "black", "-")

            # Pontos + rótulos
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

            self.canvas_vis3d.draw()
            self._set_status("Visualização 3D gerada.")
            self.content_area.select(self.tab_vis3d)

        except Exception as e:
            self._set_status(f"Erro Visual 3D: {e}")
            messagebox.showerror("Erro", str(e))

    # =========================
    # Dinâmica
    # =========================
    def _run_cg_sweep(self):
        try:
            h_ro = (getattr(self.last_2d_results, "h_ro", None)
                    if self.last_2d_results is not None else None)
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
            h_ro = (getattr(self.last_2d_results, "h_ro", None)
                    if self.last_2d_results is not None else None)
            if h_ro is None:
                h_ro = 100.0

            params = models.SuspensionMassScanParameters(
                m_min=self._read("m_min"),
                m_max=self._read("m_max"),
                m_step=self._read("step"),
                h_cg=self._read("hcg"),
                ay=self._read("ay"),
                track=self._read("bf"),
                h_ro=h_ro
            )
            results = list(dynamics.calculate_mass_sweep(params))

            for i in self.tree_mass.get_children():
                self.tree_mass.delete(i)

            for r in results:
                self.tree_mass.insert("", "end", values=(
                    f"{r.mass:.0f}",
                    f"{r.m_roll:.1f}",
                    f"{r.d_fz:.1f}",
                    f"{r.ssf:.3f}",
                    f"{getattr(r, 'ay_crit', 0):.2f}",
                    f"{getattr(r, 'margin', None):.2f}" if getattr(r, 'margin', None) is not None else "--"
                ))

            self._set_status("Varredura de massa concluída.")
            self.content_area.select(self.tab_dyn)
        except Exception as e:
            self._set_status(f"Erro mass sweep: {e}")
            messagebox.showerror("Erro", str(e))

    # =========================
    # Otimização / Estrutura
    # =========================
    def _run_opt(self):
        try:
            results = list(dynamics.calculate_force_vs_angle_sweep(
                models.ForceAngleIteratorParameters(
                    f_load=self._read("load"),
                    angle_sup_base=self._read("ang_sup"),
                    angle_inf_base=self._read("ang_inf"),
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
    # Presets (save/load)
    # =========================
    def _collect_preset(self) -> dict:
        data = {k: e.get() for k, e in self.entries.items()}
        hp = {}
        for lbl, (ex, ey, ez) in self.entries_hp.items():
            hp[lbl] = {"x": ex.get(), "y": ey.get(), "z": ez.get()}
        data["_hardpoints"] = hp
        data["_static"] = int(self.var_static.get())
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

        self.var_static.set(int(data.get("_static", 0)))

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

        hp_defaults = {
            "Sup In:": (450, 420, 200),
            "Sup Out:": (625, 390, 300),
            "Inf In:": (430, 210, 200),
            "Inf Out:": (625, 190, 300)
        }
        for lbl, (x, y, z) in hp_defaults.items():
            ex, ey, ez = self.entries_hp[lbl]
            ex.delete(0, tk.END); ex.insert(0, str(x))
            ey.delete(0, tk.END); ey.insert(0, str(y))
            ez.delete(0, tk.END); ez.insert(0, str(z))

        self.var_static.set(0)
        self._set_status("Defaults restaurados.")


if __name__ == "__main__":
    root = tk.Tk()
    app = App(root)
    root.mainloop()
