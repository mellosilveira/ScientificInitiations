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

class App:
    def __init__(self, root):
        self.root = root
        self.root.title("Baja - Suspensão Duplo A - (2D/3D/CG/Esforços)")
        self.root.geometry("1300x900")
        
        self.notebook = ttk.Notebook(root)
        self.notebook.pack(fill="both", expand=True)
        
        # Shared State
        self.last_2d_results = None
        self.last_3d_results = None
        
        # Initialize Tabs
        self._init_tab_2d()
        self._init_tab_3d()
        self._init_tab_baja()
        self._init_tab_opt() # Structural Optimization Tab
        
    def _create_entry(self, parent, label, row, col, default):
        """Helper to create labeled grid entries."""
        ttk.Label(parent, text=label).grid(row=row, column=col, sticky="e", padx=5, pady=2)
        e = ttk.Entry(parent, width=10)
        e.insert(0, str(default))
        e.grid(row=row, column=col+1, sticky="w", padx=5, pady=2)
        return e

    # ==========================================
    # TAB 1: 2D GEOMETRY
    # ==========================================
    def _init_tab_2d(self):
        tab = ttk.Frame(self.notebook)
        self.notebook.add(tab, text="Geometria 2D")
        
        # Input Frame
        f_in = ttk.LabelFrame(tab, text="Coordenadas [mm]")
        f_in.pack(side="left", fill="y", padx=10, pady=10)
        
        self.e_bf = self._create_entry(f_in, "Bitola (bf):", 0, 0, "1250")
        
        ttk.Label(f_in, text="-- Braço Superior --").grid(row=1, column=0, columnspan=2)
        self.e_ux_in = self._create_entry(f_in, "X Interno:", 2, 0, "450")
        self.e_uy_in = self._create_entry(f_in, "Y Interno:", 2, 2, "420")
        self.e_ux_out = self._create_entry(f_in, "X Externo:", 3, 0, "625")
        self.e_uy_out = self._create_entry(f_in, "Y Externo:", 3, 2, "390")
        
        ttk.Label(f_in, text="-- Braço Inferior --").grid(row=4, column=0, columnspan=2)
        self.e_lx_in = self._create_entry(f_in, "X Interno:", 5, 0, "430")
        self.e_ly_in = self._create_entry(f_in, "Y Interno:", 5, 2, "210")
        self.e_lx_out = self._create_entry(f_in, "X Externo:", 6, 0, "625")
        self.e_ly_out = self._create_entry(f_in, "Y Externo:", 6, 2, "190")
        
        ttk.Label(f_in, text="-- Cinemática --").grid(row=7, column=0, columnspan=2)
        self.e_s1 = self._create_entry(f_in, "Curso Ext:", 8, 0, "40")
        self.e_s2 = self._create_entry(f_in, "Curso Int:", 8, 2, "30")
        self.e_cam_o = self._create_entry(f_in, "Camber Ext:", 9, 0, "-1.5")
        self.e_cam_i = self._create_entry(f_in, "Camber Int:", 9, 2, "-4.0")
        
        btn = ttk.Button(f_in, text="Calcular 2D", command=self._calc_2d)
        btn.grid(row=10, column=0, columnspan=4, pady=15)
        
        # Results Frame
        f_res = ttk.Frame(tab)
        f_res.pack(side="right", fill="both", expand=True, padx=10)
        
        self.lbl_2d_res = ttk.Label(f_res, text="Resultados: --", font=("Courier", 10))
        self.lbl_2d_res.pack(pady=10, anchor="w")
        
        # Plot Canvas
        self.fig_2d, self.ax_2d = plt.subplots(figsize=(5,4))
        self.canvas_2d = FigureCanvasTkAgg(self.fig_2d, master=f_res)
        self.canvas_2d.get_tk_widget().pack(fill="both", expand=True)
        
        # Summary Text
        txt = tk.Text(f_res, height=8, bg="#f0f0f0", font=("Arial", 9))
        txt.insert("1.0", ui_strings.SUMMARY_2D)
        txt.config(state="disabled")
        txt.pack(fill="x", pady=5)

    def _calc_2d(self):
        try:
            geo = models.SuspensionGeometry2D(
                track_width=utils.safe_float(self.e_bf.get()),
                upper_in=models.Point2D(utils.safe_float(self.e_ux_in.get()), utils.safe_float(self.e_uy_in.get())),
                upper_out=models.Point2D(utils.safe_float(self.e_ux_out.get()), utils.safe_float(self.e_uy_out.get())),
                lower_in=models.Point2D(utils.safe_float(self.e_lx_in.get()), utils.safe_float(self.e_ly_in.get())),
                lower_out=models.Point2D(utils.safe_float(self.e_lx_out.get()), utils.safe_float(self.e_ly_out.get())),
                s1=utils.safe_float(self.e_s1.get()),
                s2=utils.safe_float(self.e_s2.get()),
                camber_out_deg=utils.safe_float(self.e_cam_o.get()),
                camber_in_deg=utils.safe_float(self.e_cam_i.get())
            )
            
            rc_res = math_2d.calculate_roll_center(geo)
            cam_res = math_2d.calculate_camber_gain(geo)
            
            self.last_2d_results = rc_res
            
            # Update Label
            ic_str = f"({rc_res.ic.x:.1f}, {rc_res.ic.y:.1f})" if rc_res.ic else "Paralelo"
            h_ro_str = f"{rc_res.h_ro:.1f}" if rc_res.h_ro is not None else "--"
            
            res_txt = (f"IC: {ic_str}\n"
                       f"Altura Centro Rolagem: {h_ro_str} mm\n"
                       f"fator-q: {rc_res.q_factor}\n"
                       f"Ângulo Rolagem (dPhi): {cam_res.d_phi_deg:.3f} deg\n"
                       f"Fator Camber (kGamma): {cam_res.k_gamma:.3f}")
            self.lbl_2d_res.config(text=res_txt)
            
            # Plot
            self.ax_2d.clear()
            self.ax_2d.plot([geo.upper_in.x, geo.upper_out.x], [geo.upper_in.y, geo.upper_out.y], 'b-o', label='Superior')
            self.ax_2d.plot([geo.lower_in.x, geo.lower_out.x], [geo.lower_in.y, geo.lower_out.y], 'r-o', label='Inferior')
            if rc_res.ic:
                self.ax_2d.plot(rc_res.ic.x, rc_res.ic.y, 'ks', label='IC')
                if rc_res.h_ro is not None:
                    self.ax_2d.plot(0, rc_res.h_ro, 'm*', markersize=12, label='RC')
                    self.ax_2d.plot([rc_res.ic.x, geo.track_width/2], [rc_res.ic.y, 0], 'g--', alpha=0.5)
            self.ax_2d.grid(True)
            self.ax_2d.legend()
            self.ax_2d.set_title("Geometria Vista Frontal")
            self.canvas_2d.draw()
            
        except Exception as e:
            messagebox.showerror("Erro", str(e))

    # ==========================================
    # TAB 2: 3D FORCES & ANTI-DIVE
    # ==========================================
    def _init_tab_3d(self):
        tab = ttk.Frame(self.notebook)
        self.notebook.add(tab, text="Forças 3D")
        
        f_in = ttk.LabelFrame(tab, text="Geometria 3D & Cargas")
        f_in.pack(side="left", fill="y", padx=10, pady=10)
        
        # Z-Coords
        self.e_z_sup_in = self._create_entry(f_in, "Z Sup Int:", 0, 0, "200")
        self.e_z_sup_out = self._create_entry(f_in, "Z Sup Ext:", 0, 2, "300")
        self.e_z_inf_in = self._create_entry(f_in, "Z Inf Int:", 1, 0, "200")
        self.e_z_inf_out = self._create_entry(f_in, "Z Inf Ext:", 1, 2, "300")
        
        # Forces & Anti-Dive Parameters
        ttk.Label(f_in, text="-- Cargas & Anti-Dive --").grid(row=2, column=0, columnspan=4, pady=5)
        self.e_fx = self._create_entry(f_in, "Fx Pneu [N]:", 3, 0, "1200")
        self.e_cg_h = self._create_entry(f_in, "Altura CG:", 3, 2, "500")
        self.e_wb = self._create_entry(f_in, "Entre-eixos:", 4, 0, "1550")
        
        btn = ttk.Button(f_in, text="Calcular 3D & Anti-Dive", command=self._calc_3d)
        btn.grid(row=5, column=0, columnspan=4, pady=15)
        
        f_res = ttk.Frame(tab)
        f_res.pack(side="right", fill="both", expand=True, padx=10)
        
        self.lbl_3d_res = ttk.Label(f_res, text="Resultados: --", font=("Courier", 10))
        self.lbl_3d_res.pack(pady=10, anchor="w")
        
        txt = tk.Text(f_res, height=10, bg="#f0f0f0", font=("Arial", 9))
        txt.insert("1.0", ui_strings.SUMMARY_3D)
        txt.config(state="disabled")
        txt.pack(fill="x", pady=5)

    def _calc_3d(self):
        try:
            # Reusing X/Y from Tab 1
            geo = models.SuspensionGeometry3D(
                sup_in=models.Point3D(utils.safe_float(self.e_ux_in.get()), utils.safe_float(self.e_uy_in.get()), utils.safe_float(self.e_z_sup_in.get())),
                sup_out=models.Point3D(utils.safe_float(self.e_ux_out.get()), utils.safe_float(self.e_uy_out.get()), utils.safe_float(self.e_z_sup_out.get())),
                inf_in=models.Point3D(utils.safe_float(self.e_lx_in.get()), utils.safe_float(self.e_ly_in.get()), utils.safe_float(self.e_z_inf_in.get())),
                inf_out=models.Point3D(utils.safe_float(self.e_lx_out.get()), utils.safe_float(self.e_ly_out.get()), utils.safe_float(self.e_z_inf_out.get())),
                spindle_sup=models.Point3D(600, 350, 300), 
                spindle_inf=models.Point3D(600, 180, 300),
                toe_front=models.Point3D(600, 180, 350),
                toe_rear=models.Point3D(600, 180, 250),
                stiffness_ratio_sup=1.0, stiffness_ratio_inf=1.0,
                fx_tire=utils.safe_float(self.e_fx.get())
            )
            
            forces = math_3d.calculate_forces(geo)
            align = math_3d.calculate_alignment(geo)
            anti_dive = math_3d.calculate_anti_dive(
                geo.fx_tire, 
                forces.total.fy if forces else 0, 
                utils.safe_float(self.e_cg_h.get()), 
                utils.safe_float(self.e_wb.get())
            )
            
            if forces:
                res = (f"== FORÇAS ==\n"
                       f"Axial Superior: {forces.upper.axial:.1f} N\n"
                       f"Axial Inferior: {forces.lower.axial:.1f} N\n"
                       f"Reação Vertical Total (Fy): {forces.total.fy:.1f} N\n\n"
                       f"== DINÂMICA ==\n"
                       f"Anti-Dive: {anti_dive:.1f} %\n\n"
                       f"== ALINHAMENTO ==\n"
                       f"Camber: {align.camber:.2f} deg\n"
                       f"Caster: {align.caster:.2f} deg")
                self.lbl_3d_res.config(text=res)
            else:
                self.lbl_3d_res.config(text="Erro: Geometria Inválida")
                
        except Exception as e:
            messagebox.showerror("Erro 3D", str(e))

    # ==========================================
    # TAB 3: BAJA ANALYSIS (With Plots)
    # ==========================================
    def _init_tab_baja(self):
        tab = ttk.Frame(self.notebook)
        self.notebook.add(tab, text="Análise Baja")
        
        # Left Panel: Inputs
        f_in = ttk.LabelFrame(tab, text="Parâmetros Simulação")
        f_in.pack(side="left", fill="y", padx=10, pady=10)
        
        self.e_h_min = self._create_entry(f_in, "CG Mín:", 0, 0, "250")
        self.e_h_max = self._create_entry(f_in, "CG Máx:", 1, 0, "450")
        self.e_h_step = self._create_entry(f_in, "Passo CG:", 2, 0, "25")
        self.e_mass = self._create_entry(f_in, "Massa [kg]:", 3, 0, "200")
        self.e_ay = self._create_entry(f_in, "Ay [m/s2]:", 4, 0, "9.8")
        self.e_rs = self._create_entry(f_in, "Scrub [mm]:", 5, 0, "50")
        
        btn = ttk.Button(f_in, text="Rodar Simulação", command=self._run_baja)
        btn.grid(row=6, column=0, columnspan=2, pady=15)
        
        # Right Panel: Notebook for Table/Plot
        res_nb = ttk.Notebook(tab)
        res_nb.pack(side="right", fill="both", expand=True, padx=10, pady=10)
        
        # Sub-tab: Table
        f_table = ttk.Frame(res_nb)
        res_nb.add(f_table, text="Tabela de Dados")
        cols = ("CG", "dFz", "Fz_int", "M_Roll")
        self.tree = ttk.Treeview(f_table, columns=cols, show="headings")
        for c in cols: self.tree.heading(c, text=c); self.tree.column(c, width=80)
        self.tree.pack(fill="both", expand=True)
        
        # Sub-tab: Plot (Restoring Baja.py plot logic)
        f_plot = ttk.Frame(res_nb)
        res_nb.add(f_plot, text="Gráfico de Tendências")
        self.fig_baja, (self.ax_b1, self.ax_b2) = plt.subplots(1, 2, figsize=(8, 4))
        self.canvas_baja = FigureCanvasTkAgg(self.fig_baja, master=f_plot)
        self.canvas_baja.get_tk_widget().pack(fill="both", expand=True)
        
        # Text Summary
        txt = tk.Text(tab, height=6, bg="#f0f0f0", font=("Arial", 9))
        txt.insert("1.0", ui_strings.SUMMARY_BAJA)
        txt.config(state="disabled")
        txt.pack(side="bottom", fill="x", pady=5)

    def _run_baja(self):
        h_ro = self.last_2d_results.h_ro if self.last_2d_results and self.last_2d_results.h_ro else 100.0
        
        params = models.SuspensionIteratorParameters(
            h_min=utils.safe_float(self.e_h_min.get()),
            h_max=utils.safe_float(self.e_h_max.get()),
            h_step=utils.safe_float(self.e_h_step.get()),
            mass=utils.safe_float(self.e_mass.get()),
            ay=utils.safe_float(self.e_ay.get()),
            track=utils.safe_float(self.e_bf.get()),
            h_ro=h_ro,
            scrub_radius=utils.safe_float(self.e_rs.get()), clearance=0.0
        )
        
        # CONSUMING THE GENERATOR: Convert yield stream to list for multi-pass usage
        results = list(dynamics.calculate_baja_sweep(params))
        
        # Update Table
        for i in self.tree.get_children(): self.tree.delete(i)
        for r in results:
            self.tree.insert("", "end", values=(f"{r.h_cg:.1f}", f"{r.d_fz:.1f}", f"{r.fz_int:.1f}", f"{r.m_roll:.1f}"))
            
        # Update Plots
        cgs = [r.h_cg for r in results]
        dfz = [r.d_fz for r in results]
        mroll = [r.m_roll for r in results]
        
        self.ax_b1.clear(); self.ax_b2.clear()
        
        self.ax_b1.plot(cgs, dfz, 'b-o')
        self.ax_b1.set_title("Transf. Carga vs CG")
        self.ax_b1.set_xlabel("Altura CG (mm)"); self.ax_b1.set_ylabel("Delta Fz (N)")
        self.ax_b1.grid(True)
        
        self.ax_b2.plot(cgs, mroll, 'r-o')
        self.ax_b2.set_title("Momento Rolagem vs CG")
        self.ax_b2.set_xlabel("Altura CG (mm)"); self.ax_b2.set_ylabel("Momento (Nm)")
        self.ax_b2.grid(True)
        
        self.fig_baja.tight_layout()
        self.canvas_baja.draw()

    # ==========================================
    # TAB 4: STRUCTURAL OPTIMIZATION (from otimizado2.0.py)
    # ==========================================
    def _init_tab_opt(self):
        tab = ttk.Frame(self.notebook)
        self.notebook.add(tab, text="Otimização Estrutural")
        
        f_in = ttk.LabelFrame(tab, text="Parâmetros de Varredura Angular")
        f_in.pack(side="left", fill="y", padx=10, pady=10)
        
        self.e_load = self._create_entry(f_in, "Carga F [N]:", 0, 0, "3000")
        self.e_ang_sup = self._create_entry(f_in, "Ang Base Sup:", 1, 0, "10")
        self.e_ang_inf = self._create_entry(f_in, "Ang Base Inf:", 1, 2, "20")
        self.e_ksup = self._create_entry(f_in, "Rigidez Sup:", 2, 0, "0.5")
        self.e_kinf = self._create_entry(f_in, "Rigidez Inf:", 2, 2, "0.5")
        self.e_limit = self._create_entry(f_in, "Limite Força:", 3, 0, "8000")
        self.e_amin = self._create_entry(f_in, "Varredura Min:", 4, 0, "0")
        self.e_amax = self._create_entry(f_in, "Varredura Max:", 4, 2, "40")
        self.e_astep = self._create_entry(f_in, "Passo Varredura:", 5, 0, "5")
        
        btn = ttk.Button(f_in, text="Analisar Forças", command=self._run_opt)
        btn.grid(row=6, column=0, columnspan=4, pady=15)
        
        # Output: Notebook with Table and Plot
        res_nb = ttk.Notebook(tab)
        res_nb.pack(side="right", fill="both", expand=True, padx=10, pady=10)
        
        # Table
        f_table = ttk.Frame(res_nb)
        res_nb.add(f_table, text="Tabela de Dados")
        cols = ("dAngle", "F_Sup", "F_Inf", "Status")
        self.tree_opt = ttk.Treeview(f_table, columns=cols, show="headings")
        for c in cols: self.tree_opt.heading(c, text=c); self.tree_opt.column(c, width=80)
        self.tree_opt.pack(fill="both", expand=True)
        
        # Plot
        f_plot = ttk.Frame(res_nb)
        res_nb.add(f_plot, text="Gráfico de Força")
        self.fig_opt, self.ax_opt = plt.subplots(figsize=(5,4))
        self.canvas_opt = FigureCanvasTkAgg(self.fig_opt, master=f_plot)
        self.canvas_opt.get_tk_widget().pack(fill="both", expand=True)

    def _run_opt(self):
        try:
            # CONSUMING THE GENERATOR
            results = list(dynamics.calculate_force_vs_angle_sweep(
                models.ForceAngleIteratorParameters(
                    f_load=utils.safe_float(self.e_load.get()),
                    angle_sup_base=utils.safe_float(self.e_ang_sup.get()),
                    angle_inf_base=utils.safe_float(self.e_ang_inf.get()),
                    k_sup=utils.safe_float(self.e_ksup.get()),
                    k_inf=utils.safe_float(self.e_kinf.get()),
                    limit=utils.safe_float(self.e_limit.get()),
                    ang_min=int(utils.safe_float(self.e_amin.get())),
                    ang_max=int(utils.safe_float(self.e_amax.get())),
                    step=int(utils.safe_float(self.e_astep.get()))
                )
            ))
            
            # Update Table
            for i in self.tree_opt.get_children(): self.tree_opt.delete(i)
            
            angles = []
            f_sup = []
            f_inf = []
            limit = utils.safe_float(self.e_limit.get())
            
            for r in results:
                self.tree_opt.insert("", "end", values=(
                    f"{r.angle_delta}", f"{r.force_sup:.1f}", f"{r.force_inf:.1f}", r.status
                ), tags=('limit' if r.status == "LIMITE" else 'ok',))
                angles.append(r.angle_delta)
                f_sup.append(r.force_sup)
                f_inf.append(r.force_inf)
                
            self.tree_opt.tag_configure('limit', background='#ffcccc')
            
            # Update Plot
            self.ax_opt.clear()
            self.ax_opt.plot(angles, f_sup, label="Braço Superior")
            self.ax_opt.plot(angles, f_inf, label="Braço Inferior")
            self.ax_opt.axhline(limit, color='r', linestyle='--', label="Limite")
            self.ax_opt.set_xlabel("Delta Ângulo (deg)")
            self.ax_opt.set_ylabel("Força Axial (N)")
            self.ax_opt.legend()
            self.ax_opt.grid(True)
            self.canvas_opt.draw()
            
        except Exception as e:
            messagebox.showerror("Erro Opt", str(e))

if __name__ == "__main__":
    root = tk.Tk()
    app = App(root)
    root.mainloop()