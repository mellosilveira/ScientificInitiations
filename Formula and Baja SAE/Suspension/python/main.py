import tkinter as tk
from tkinter import ttk, messagebox
import matplotlib.pyplot as plt
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg

# Import domain modules
import models
import math_2d
import math_3d
import dynamics
import utils
import ui_strings

class ScrollableFrame(ttk.Frame):
    """
    A frame containing a canvas and scrollbar to allow scrolling of inputs.
    """
    def __init__(self, container, *args, **kwargs):
        super().__init__(container, *args, **kwargs)
        canvas = tk.Canvas(self, width=380) # Fixed width for sidebar
        scrollbar = ttk.Scrollbar(self, orient="vertical", command=canvas.yview)
        self.scrollable_window = ttk.Frame(canvas)

        self.scrollable_window.bind(
            "<Configure>",
            lambda e: canvas.configure(scrollregion=canvas.bbox("all"))
        )

        canvas.create_window((0, 0), window=self.scrollable_window, anchor="nw")
        canvas.configure(yscrollcommand=scrollbar.set)

        canvas.pack(side="left", fill="both", expand=True)
        scrollbar.pack(side="right", fill="y")
        
        # Bind mouse wheel for scrolling
        canvas.bind_all("<MouseWheel>", lambda event: canvas.yview_scroll(int(-1*(event.delta/120)), "units"))

class App:
    def __init__(self, root):
        self.root = root
        self.root.title("Baja - Suspensão Duplo A - (2D/3D/CG/Esforços)")
        self.root.geometry("1920x1080")
        
        # Configure Style
        style = ttk.Style()
        style.theme_use('clam')
        style.configure("TLabelFrame", font=("Arial", 10, "bold"))
        style.configure("TButton", font=("Arial", 9, "bold"))
        style.configure("Action.TButton", foreground="blue") # Style for action buttons
        
        # Main Layout (PanedWindow)
        self.paned = tk.PanedWindow(root, orient="horizontal", sashwidth=5)
        self.paned.pack(fill="both", expand=True)
        
        # 1. Sidebar (Inputs Only)
        self.sidebar_container = ScrollableFrame(self.paned)
        self.paned.add(self.sidebar_container, minsize=400, width=420)
        
        # 2. Content Area (Results & Actions)
        self.content_area = ttk.Notebook(self.paned)
        self.paned.add(self.content_area, minsize=600)
        
        # Shared State
        self.last_2d_results = None
        self.last_3d_results = None
        self.entries_hp = {} # Store hardpoint entries
        
        # Build UI
        self._init_sidebar_inputs(self.sidebar_container.scrollable_window)
        self._init_result_tabs()
        
    def _create_entry(self, parent, label, row, col, default, width=8):
        """Helper to create labeled grid entries."""
        ttk.Label(parent, text=label).grid(row=row, column=col, sticky="e", padx=2, pady=2)
        e = ttk.Entry(parent, width=width)
        e.insert(0, str(default))
        e.grid(row=row, column=col+1, sticky="w", padx=2, pady=2)
        return e

    # ==========================================================
    # SIDEBAR: INPUTS ONLY (No Buttons Here)
    # ==========================================================
    def _init_sidebar_inputs(self, parent):
        # --- BLOCK 1: GEOMETRY (HARDPOINTS) ---
        f_geo = ttk.LabelFrame(parent, text="1. Geometria (Hardpoints) [mm]")
        f_geo.pack(fill="x", padx=5, pady=5)
        
        self.e_bf = self._create_entry(f_geo, "Bitola (bf):", 0, 0, "1250", width=12)
        
        # Headers X, Y, Z
        ttk.Label(f_geo, text="X").grid(row=1, column=1)
        ttk.Label(f_geo, text="Y").grid(row=1, column=2)
        ttk.Label(f_geo, text="Z").grid(row=1, column=3)
        
        # Hardpoints Rows
        labels = ["Sup In:", "Sup Out:", "Inf In:", "Inf Out:"]
        defaults = [
            ("450", "420", "200"), # Sup In
            ("625", "390", "300"), # Sup Out
            ("430", "210", "200"), # Inf In
            ("625", "190", "300")  # Inf Out
        ]
        
        for i, (lbl, (dx, dy, dz)) in enumerate(zip(labels, defaults)):
            row = i + 2
            ttk.Label(f_geo, text=lbl).grid(row=row, column=0, sticky="e")
            ex = ttk.Entry(f_geo, width=7); ex.insert(0, dx); ex.grid(row=row, column=1)
            ey = ttk.Entry(f_geo, width=7); ey.insert(0, dy); ey.grid(row=row, column=2)
            ez = ttk.Entry(f_geo, width=7); ez.insert(0, dz); ez.grid(row=row, column=3)
            self.entries_hp[lbl] = (ex, ey, ez)

        # --- BLOCK 2: KINEMATICS & ALIGNMENT ---
        f_kin = ttk.LabelFrame(parent, text="2. Cinemática & Alinhamento")
        f_kin.pack(fill="x", padx=5, pady=5)
        
        self.e_s1 = self._create_entry(f_kin, "Curso Ext (mm):", 0, 0, "40")
        self.e_s2 = self._create_entry(f_kin, "Curso Int (mm):", 0, 2, "30")
        self.e_cam_o = self._create_entry(f_kin, "Camber Ext (°):", 1, 0, "-1.5")
        self.e_cam_i = self._create_entry(f_kin, "Camber Int (°):", 1, 2, "-4.0")
        
        # Auxiliary 3D Points
        ttk.Label(f_kin, text="-- Auxiliares 3D --").grid(row=2, column=0, columnspan=4, pady=5)
        self.e_spindle_sup_z = self._create_entry(f_kin, "Spindle Sup Z:", 3, 0, "300")
        self.e_spindle_inf_z = self._create_entry(f_kin, "Spindle Inf Z:", 3, 2, "300")
        self.e_toe_f_x = self._create_entry(f_kin, "Toe Front X:", 4, 0, "600")
        self.e_toe_r_x = self._create_entry(f_kin, "Toe Rear X:", 4, 2, "600")
        
        # --- BLOCK 3: DYNAMICS (BAJA) ---
        f_dyn = ttk.LabelFrame(parent, text="3. Dinâmica Veicular (Baja)")
        f_dyn.pack(fill="x", padx=5, pady=5)
        
        self.e_mass = self._create_entry(f_dyn, "Massa (kg):", 0, 0, "200")
        self.e_hcg = self._create_entry(f_dyn, "Altura CG (mm):", 0, 2, "450")
        self.e_ay = self._create_entry(f_dyn, "Ay (m/s²):", 1, 0, "9.8")
        self.e_wb = self._create_entry(f_dyn, "Entre-eixos (mm):", 1, 2, "1550")
        self.e_fx = self._create_entry(f_dyn, "Fx Pneu (N):", 2, 0, "1200")
        self.e_rs = self._create_entry(f_dyn, "Scrub (mm):", 2, 2, "50")
        
        ttk.Label(f_dyn, text="-- Varredura (Simulação) --").grid(row=3, column=0, columnspan=4, pady=5)
        self.e_h_min = self._create_entry(f_dyn, "CG Min:", 4, 0, "250")
        self.e_h_max = self._create_entry(f_dyn, "CG Max:", 4, 2, "650")
        self.e_m_min = self._create_entry(f_dyn, "Massa Min:", 5, 0, "180")
        self.e_m_max = self._create_entry(f_dyn, "Massa Max:", 5, 2, "240")
        self.e_step = self._create_entry(f_dyn, "Passo:", 6, 0, "25")

        # --- BLOCK 4: STRUCTURAL OPTIMIZATION ---
        f_opt = ttk.LabelFrame(parent, text="4. Otimização Estrutural")
        f_opt.pack(fill="x", padx=5, pady=5)
        
        self.e_load = self._create_entry(f_opt, "Carga F [N]:", 0, 0, "3000")
        self.var_static = tk.IntVar()
        ttk.Checkbutton(f_opt, text="+ Estática (mg/4)", variable=self.var_static).grid(row=0, column=2, columnspan=2)
        
        self.e_ang_sup = self._create_entry(f_opt, "Ang Base Sup:", 2, 0, "10")
        self.e_ang_inf = self._create_entry(f_opt, "Ang Base Inf:", 2, 2, "20")
        self.e_limit = self._create_entry(f_opt, "Limite (N):", 3, 0, "8000")
        self.e_ksup = self._create_entry(f_opt, "K Sup:", 3, 2, "0.5")
        
        self.e_amin = self._create_entry(f_opt, "Varredura Min:", 4, 0, "0")
        self.e_amax = self._create_entry(f_opt, "Varredura Max:", 4, 2, "40")

    # ==========================================================
    # MAIN AREA: RESULTS TABS & ACTIONS
    # ==========================================================
    def _init_result_tabs(self):
        # Tab 1: 2D Results
        self.tab_2d = ttk.Frame(self.content_area)
        self.content_area.add(self.tab_2d, text="Resultados 2D")
        self._setup_res_2d(self.tab_2d)
        
        # Tab 2: 3D Results
        self.tab_3d = ttk.Frame(self.content_area)
        self.content_area.add(self.tab_3d, text="Resultados 3D")
        self._setup_res_3d(self.tab_3d)
        
        # Tab 3: Dynamics
        self.tab_dyn = ttk.Frame(self.content_area)
        self.content_area.add(self.tab_dyn, text="Dinâmica Veicular")
        self._setup_res_dyn(self.tab_dyn)
        
        # Tab 4: Optimization
        self.tab_opt = ttk.Frame(self.content_area)
        self.content_area.add(self.tab_opt, text="Otimização")
        self._setup_res_opt(self.tab_opt)

    def _setup_res_2d(self, parent):
        # --- ACTION FRAME ---
        f_ctrl = ttk.Frame(parent)
        f_ctrl.pack(fill='x', padx=10, pady=10)
        
        ttk.Button(f_ctrl, text="CALCULAR GEOMETRIA 2D", style="Action.TButton", command=self._calc_2d).pack(fill='x', ipady=5)
        
        # Results Label
        self.lbl_2d_res = ttk.Label(parent, text="Clique em calcular...", font=("Courier", 11), justify="left")
        self.lbl_2d_res.pack(pady=5, padx=10, anchor="w")
        
        # Plot
        self.fig_2d, self.ax_2d = plt.subplots(figsize=(6,5))
        self.canvas_2d = FigureCanvasTkAgg(self.fig_2d, master=parent)
        self.canvas_2d.get_tk_widget().pack(fill="both", expand=True, padx=10, pady=5)
        
        # Summary
        txt = tk.Text(parent, height=6, bg="#f0f0f0", font=("Arial", 9))
        txt.insert("1.0", ui_strings.SUMMARY_2D)
        txt.config(state="disabled")
        txt.pack(fill="x", side="bottom")

    def _setup_res_3d(self, parent):
        # --- ACTION FRAME ---
        f_ctrl = ttk.Frame(parent)
        f_ctrl.pack(fill='x', padx=10, pady=10)
        
        ttk.Button(f_ctrl, text="CALCULAR FORÇAS 3D & ANTI-DIVE", style="Action.TButton", 
                   command=self._calc_3d).pack(fill='x', ipady=5)

        # Results Label
        self.lbl_3d_res = ttk.Label(parent, text="Clique em calcular...", font=("Courier", 11), justify="left")
        self.lbl_3d_res.pack(pady=10, padx=10, anchor="w")
        
        txt = tk.Text(parent, height=10, bg="#f0f0f0", font=("Arial", 9))
        txt.insert("1.0", ui_strings.SUMMARY_3D)
        txt.config(state="disabled")
        txt.pack(fill="x", side="bottom")

    def _setup_res_dyn(self, parent):
        nb = ttk.Notebook(parent)
        nb.pack(fill="both", expand=True)
        
        # --- Sub-tab: CG Sweep ---
        f_cg = ttk.Frame(nb)
        nb.add(f_cg, text="Varredura de CG")
        
        # Action Button CG
        f_ctrl_cg = ttk.Frame(f_cg)
        f_ctrl_cg.pack(fill='x', padx=10, pady=5)
        ttk.Button(f_ctrl_cg, text="RODAR SIMULAÇÃO DE CG", style="Action.TButton",
                   command=self._run_cg_sweep).pack(fill='x', ipady=3)
        
        self.fig_cg, (self.ax_cg1, self.ax_cg2) = plt.subplots(1, 2, figsize=(8, 3.5))
        self.canvas_cg = FigureCanvasTkAgg(self.fig_cg, master=f_cg)
        self.canvas_cg.get_tk_widget().pack(fill="both", expand=True)
        
        cols_cg = ("CG", "dFz", "Fz_int", "M_Roll")
        self.tree_cg = ttk.Treeview(f_cg, columns=cols_cg, show="headings", height=5)
        for c in cols_cg: self.tree_cg.heading(c, text=c); self.tree_cg.column(c, width=80)
        self.tree_cg.pack(fill="x")

        # --- Sub-tab: Mass Sweep ---
        f_mass = ttk.Frame(nb)
        nb.add(f_mass, text="Varredura de Massa")
        
        # Action Button Mass
        f_ctrl_m = ttk.Frame(f_mass)
        f_ctrl_m.pack(fill='x', padx=10, pady=5)
        ttk.Button(f_ctrl_m, text="RODAR SIMULAÇÃO DE MASSA", style="Action.TButton",
                   command=self._run_mass_sweep).pack(fill='x', ipady=3)

        cols_m = ("Massa", "M_Roll", "dFz", "SSF", "Ay_Crit", "Margem")
        self.tree_mass = ttk.Treeview(f_mass, columns=cols_m, show="headings")
        for c in cols_m: self.tree_mass.heading(c, text=c); self.tree_mass.column(c, width=90)
        self.tree_mass.pack(fill="both", expand=True)

        # Summary
        txt = tk.Text(parent, height=5, bg="#f0f0f0", font=("Arial", 9))
        txt.insert("1.0", ui_strings.SUMMARY_BAJA)
        txt.config(state="disabled")
        txt.pack(fill="x", side="bottom")

    def _setup_res_opt(self, parent):
        # --- ACTION FRAME ---
        f_ctrl = ttk.Frame(parent)
        f_ctrl.pack(fill='x', padx=10, pady=10)
        
        col1 = ttk.Frame(f_ctrl)
        col1.pack(side="left", fill="x", expand=True, padx=2)
        col2 = ttk.Frame(f_ctrl)
        col2.pack(side="left", fill="x", expand=True, padx=2)
        
        # Button 1: Auto Calculate Load
        ttk.Button(col1, text="1. Auto Calc Carga (via Dinâmica)", 
                   command=self._calc_load_from_dynamics).pack(fill='x')
        
        # Button 2: Run Optimization
        ttk.Button(col2, text="2. ANALISAR ESTRUTURA", style="Action.TButton",
                   command=self._run_opt).pack(fill='x')

        # Split: Table and Graph
        paned = tk.PanedWindow(parent, orient="vertical")
        paned.pack(fill="both", expand=True)
        
        # Table
        f_tab = ttk.Frame(paned)
        cols = ("dAngle", "F_Sup", "F_Inf", "Status")
        self.tree_opt = ttk.Treeview(f_tab, columns=cols, show="headings")
        for c in cols: self.tree_opt.heading(c, text=c); self.tree_opt.column(c, width=80)
        self.tree_opt.pack(fill="both", expand=True)
        paned.add(f_tab, minsize=150)
        
        # Graph
        f_graph = ttk.Frame(paned)
        self.fig_opt, self.ax_opt = plt.subplots(figsize=(5,4))
        self.canvas_opt = FigureCanvasTkAgg(self.fig_opt, master=f_graph)
        self.canvas_opt.get_tk_widget().pack(fill="both", expand=True)
        paned.add(f_graph)

    # ==========================================================
    # LOGIC: DATA GATHERING & EXECUTION
    # ==========================================================
    
    def _get_point3d_from_input(self, key):
        """Helper to read X,Y,Z from the unified entries dictionary."""
        entries = self.entries_hp[key] # Tuple (ex, ey, ez)
        return models.Point3D(
            utils.safe_float(entries[0].get()),
            utils.safe_float(entries[1].get()),
            utils.safe_float(entries[2].get())
        )

    def _calc_2d(self):
        """Logic for 2D Calculation Button."""
        try:
            geo2d = models.SuspensionGeometry2D(
                track_width=utils.safe_float(self.e_bf.get()),
                upper_in=models.Point2D(utils.safe_float(self.entries_hp["Sup In:"][0].get()), utils.safe_float(self.entries_hp["Sup In:"][1].get())),
                upper_out=models.Point2D(utils.safe_float(self.entries_hp["Sup Out:"][0].get()), utils.safe_float(self.entries_hp["Sup Out:"][1].get())),
                lower_in=models.Point2D(utils.safe_float(self.entries_hp["Inf In:"][0].get()), utils.safe_float(self.entries_hp["Inf In:"][1].get())),
                lower_out=models.Point2D(utils.safe_float(self.entries_hp["Inf Out:"][0].get()), utils.safe_float(self.entries_hp["Inf Out:"][1].get())),
                s1=utils.safe_float(self.e_s1.get()),
                s2=utils.safe_float(self.e_s2.get()),
                camber_out_deg=utils.safe_float(self.e_cam_o.get()),
                camber_in_deg=utils.safe_float(self.e_cam_i.get())
            )
            
            rc_res = math_2d.calculate_roll_center(geo2d)
            cam_res = math_2d.calculate_camber_gain(geo2d)
            self.last_2d_results = rc_res
            
            # Update UI
            ic_str = f"({rc_res.ic.x:.1f}, {rc_res.ic.y:.1f})" if rc_res.ic else "Paralelo"
            h_ro_str = f"{rc_res.h_ro:.1f}" if rc_res.h_ro is not None else "--"
            self.lbl_2d_res.config(text=(
                f"IC: {ic_str} mm\n"
                f"Altura RC (h_Ro): {h_ro_str} mm\n"
                f"fator-q: {rc_res.q_factor}\n"
                f"Rolagem (dPhi): {cam_res.d_phi_deg:.3f} deg\n"
                f"Fator Camber (kGamma): {cam_res.k_gamma:.3f}"
            ))
            
            # Plot
            self.ax_2d.clear()
            self.ax_2d.plot([geo2d.upper_in.x, geo2d.upper_out.x], [geo2d.upper_in.y, geo2d.upper_out.y], 'b-o', label='Superior')
            self.ax_2d.plot([geo2d.lower_in.x, geo2d.lower_out.x], [geo2d.lower_in.y, geo2d.lower_out.y], 'r-o', label='Inferior')
            if rc_res.ic:
                self.ax_2d.plot(rc_res.ic.x, rc_res.ic.y, 'ks', label='IC')
                if rc_res.h_ro is not None:
                    self.ax_2d.plot(0, rc_res.h_ro, 'm*', markersize=12, label='RC')
                    self.ax_2d.plot([rc_res.ic.x, geo2d.track_width/2], [rc_res.ic.y, 0], 'g--', alpha=0.5)
            self.ax_2d.grid(True); self.ax_2d.legend(); self.ax_2d.set_title("Geometria Vista Frontal")
            self.canvas_2d.draw()
            
        except Exception as e:
            messagebox.showerror("Erro 2D", str(e))

    def _calc_3d(self):
        """Logic for 3D Calculation Button."""
        try:
            # Reusing Inputs from Sidebar (2D inputs + 3D specifics)
            geo3d = models.SuspensionGeometry3D(
                sup_in=self._get_point3d_from_input("Sup In:"),
                sup_out=self._get_point3d_from_input("Sup Out:"),
                inf_in=self._get_point3d_from_input("Inf In:"),
                inf_out=self._get_point3d_from_input("Inf Out:"),
                spindle_sup=models.Point3D(utils.safe_float(self.entries_hp["Sup Out:"][0].get()), utils.safe_float(self.entries_hp["Sup Out:"][1].get()), utils.safe_float(self.e_spindle_sup_z.get())),
                spindle_inf=models.Point3D(utils.safe_float(self.entries_hp["Inf Out:"][0].get()), utils.safe_float(self.entries_hp["Inf Out:"][1].get()), utils.safe_float(self.e_spindle_inf_z.get())),
                toe_front=models.Point3D(utils.safe_float(self.e_toe_f_x.get()), 0, 0),
                toe_rear=models.Point3D(utils.safe_float(self.e_toe_r_x.get()), 0, 0),
                stiffness_ratio_sup=1.0, stiffness_ratio_inf=1.0,
                fx_tire=utils.safe_float(self.e_fx.get())
            )

            forces = math_3d.calculate_forces(geo3d)
            align = math_3d.calculate_alignment(geo3d)
            anti_dive = math_3d.calculate_anti_dive(
                geo3d.fx_tire, 
                forces.total.fy if forces else 0, 
                utils.safe_float(self.e_hcg.get()), 
                utils.safe_float(self.e_wb.get())
            )
            
            if forces:
                self.lbl_3d_res.config(text=(
                    f"== FORÇAS AXIAIS ==\n"
                    f"Sup: {forces.upper.axial:.1f} N\n"
                    f"Inf: {forces.lower.axial:.1f} N\n\n"
                    f"== DINÂMICA ==\n"
                    f"Anti-Dive: {anti_dive:.1f} %\n\n"
                    f"== ALINHAMENTO ==\n"
                    f"Camber: {align.camber:.2f} deg\n"
                    f"Caster: {align.caster:.2f} deg"
                ))
            else:
                self.lbl_3d_res.config(text="Erro Geometria 3D")
                
        except Exception as e:
            messagebox.showerror("Erro 3D", str(e))

    def _run_cg_sweep(self):
        # Uses last calculated RC or default 100
        h_ro = self.last_2d_results.h_ro if self.last_2d_results and self.last_2d_results.h_ro else 100.0
        
        params = models.SuspensionCGScanParameters(
            h_min=utils.safe_float(self.e_h_min.get()),
            h_max=utils.safe_float(self.e_h_max.get()),
            h_step=utils.safe_float(self.e_step.get()),
            mass=utils.safe_float(self.e_mass.get()),
            ay=utils.safe_float(self.e_ay.get()),
            track=utils.safe_float(self.e_bf.get()),
            h_ro=h_ro, scrub_radius=utils.safe_float(self.e_rs.get()), clearance=0.0
        )
        
        results = list(dynamics.calculate_baja_cg_sweep(params))
        
        # Update Table
        for i in self.tree_cg.get_children(): self.tree_cg.delete(i)
        for r in results:
            self.tree_cg.insert("", "end", values=(f"{r.h_cg:.1f}", f"{r.d_fz:.1f}", f"{r.fz_int:.1f}", f"{r.m_roll:.1f}"))
            
        # Update Plots
        cgs = [r.h_cg for r in results]
        self.ax_cg1.clear(); self.ax_cg2.clear()
        self.ax_cg1.plot(cgs, [r.d_fz for r in results], 'b-o'); self.ax_cg1.set_title("Transf. Carga vs CG")
        self.ax_cg1.grid(True)
        self.ax_cg2.plot(cgs, [r.m_roll for r in results], 'r-o'); self.ax_cg2.set_title("Momento Rolagem vs CG")
        self.ax_cg2.grid(True)
        self.canvas_cg.draw()

    def _run_mass_sweep(self):
        h_ro = self.last_2d_results.h_ro if self.last_2d_results and self.last_2d_results.h_ro else 100.0
        params = models.SuspensionMassScanParameters(
            m_min=utils.safe_float(self.e_m_min.get()),
            m_max=utils.safe_float(self.e_m_max.get()),
            m_step=utils.safe_float(self.e_step.get()), # Using the same step entry for simplicity
            h_cg=utils.safe_float(self.e_hcg.get()),
            ay=utils.safe_float(self.e_ay.get()),
            track=utils.safe_float(self.e_bf.get()),
            h_ro=h_ro
        )
        
        results = list(dynamics.calculate_mass_sweep(params))
        
        for i in self.tree_mass.get_children(): self.tree_mass.delete(i)
        for r in results:
            self.tree_mass.insert("", "end", values=(
                f"{r.mass:.0f}", f"{r.m_roll:.1f}", f"{r.d_fz:.1f}", 
                f"{r.ssf:.3f}", f"{r.ay_crit:.2f}", f"{r.margin:.2f}" if r.margin else "--"
            ))

    def _calc_load_from_dynamics(self):
        try:
            m = utils.safe_float(self.e_mass.get())
            ay = utils.safe_float(self.e_ay.get())
            track = utils.safe_float(self.e_bf.get())
            h_cg = utils.safe_float(self.e_hcg.get())
            h_ro = self.last_2d_results.h_ro if self.last_2d_results and self.last_2d_results.h_ro else 100.0
            
            comps = dynamics.calculate_load_transfer_components(m, ay, track, h_cg, h_ro)
            
            f_wheel = comps.d_fz_total / 2.0
            if self.var_static.get():
                f_wheel += (m * 9.81) / 4.0
            
            self.e_load.delete(0, tk.END)
            self.e_load.insert(0, f"{f_wheel:.1f}")
            messagebox.showinfo("Calc Carga", f"Carga calculada: {f_wheel:.1f} N\n(Geo: {comps.d_fz_geo:.0f} N, Elást: {comps.d_fz_el:.0f} N)")
            
        except Exception as e:
            messagebox.showerror("Erro", str(e))

    def _run_opt(self):
        try:
            results = list(dynamics.calculate_force_vs_angle_sweep(
                models.ForceAngleIteratorParameters(
                    f_load=utils.safe_float(self.e_load.get()),
                    angle_sup_base=utils.safe_float(self.e_ang_sup.get()),
                    angle_inf_base=utils.safe_float(self.e_ang_inf.get()),
                    k_sup=utils.safe_float(self.e_ksup.get()),
                    k_inf=1.0 - utils.safe_float(self.e_ksup.get()), 
                    limit=utils.safe_float(self.e_limit.get()),
                    ang_min=int(utils.safe_float(self.e_amin.get())),
                    ang_max=int(utils.safe_float(self.e_amax.get())),
                    step=int(5)
                )
            ))
            
            for i in self.tree_opt.get_children(): self.tree_opt.delete(i)
            angles, f_sup, f_inf = [], [], []
            limit = utils.safe_float(self.e_limit.get())
            
            for r in results:
                self.tree_opt.insert("", "end", values=(
                    f"{r.angle_delta}", f"{r.force_sup:.1f}", f"{r.force_inf:.1f}", r.status
                ), tags=('limit' if r.status == "LIMITE" else 'ok',))
                angles.append(r.angle_delta); f_sup.append(r.force_sup); f_inf.append(r.force_inf)
                
            self.tree_opt.tag_configure('limit', background='#ffcccc')
            
            self.ax_opt.clear()
            self.ax_opt.plot(angles, f_sup, label="Sup"); self.ax_opt.plot(angles, f_inf, label="Inf")
            self.ax_opt.axhline(limit, color='r', linestyle='--')
            self.ax_opt.legend(); self.ax_opt.grid(True)
            self.canvas_opt.draw()
            
        except Exception as e:
            messagebox.showerror("Erro Opt", str(e))

if __name__ == "__main__":
    root = tk.Tk()
    app = App(root)
    root.mainloop()