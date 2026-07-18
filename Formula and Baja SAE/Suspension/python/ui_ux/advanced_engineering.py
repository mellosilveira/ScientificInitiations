"""Janela de análises avançadas. Pode ser aberta a partir da aplicação principal."""
from __future__ import annotations
import tkinter as tk
from tkinter import ttk, messagebox, filedialog
import json
from calculators.advanced_suspension import *

class AdvancedEngineeringWindow(tk.Toplevel):
    def __init__(self, master, level='3.0'):
        super().__init__(master); self.level=level
        self.title(f'MudRunner — Engenharia Avançada v{level}'); self.geometry('1180x760'); self.minsize(980,650)
        self.protocol('WM_DELETE_WINDOW', self.destroy)
        self.nb=ttk.Notebook(self); self.nb.pack(fill='both',expand=True,padx=10,pady=10)
        self._ackermann(); self._kinematics(); self._rates()
        if level in ('2.5','3.0'): self._dynamics()
        if level=='3.0': self._industrial()
        self.status=tk.StringVar(value='Pronto. Execute uma análise para gerar resultados e parecer técnico.')
        ttk.Label(self,textvariable=self.status,relief='sunken',anchor='w').pack(fill='x')
    def _tab(self,name): f=ttk.Frame(self.nb,padding=12); self.nb.add(f,text=name); return f
    def _field(self,p,label,default,row,col=0):
        ttk.Label(p,text=label).grid(row=row,column=col,sticky='e',padx=5,pady=4); e=ttk.Entry(p,width=12); e.insert(0,str(default)); e.grid(row=row,column=col+1,sticky='w'); return e
    def _text(self,p,row=20): t=tk.Text(p,height=22,wrap='word'); t.grid(row=row,column=0,columnspan=6,sticky='nsew',pady=10); p.rowconfigure(row,weight=1); p.columnconfigure(5,weight=1); return t
    def _show(self,t,obj,assessment):
        t.delete('1.0','end'); t.insert('end',assessment+'\n\n'+json.dumps(obj,indent=2,ensure_ascii=False)); self.status.set(assessment.splitlines()[0])
    def _ackermann(self):
        f=self._tab('Ackermann'); wb=self._field(f,'Entre-eixos [mm]',1500,0); tr=self._field(f,'Bitola [mm]',1300,1); ang=self._field(f,'Ângulo interno máx [°]',35,2); fac=self._field(f,'Fator geométrico real',0.94,3); out=self._text(f,6)
        def run():
            rows=ackermann_sweep(float(wb.get()),float(tr.get()),float(ang.get()),15,float(fac.get())); p=rows[-1]['ackermann_pct']; verdict='FAVORÁVEL' if 85<=p<=110 else 'REVISAR'
            self._show(out,rows,f'PARECER: {verdict} — Ackermann no esterçamento máximo = {p:.1f}%.\nReferência: condição geométrica ideal; validar com hardpoints reais e pneus.')
        ttk.Button(f,text='Calcular Ackermann',command=run).grid(row=4,column=0,columnspan=2,pady=8)
    def _kinematics(self):
        f=self._tab('Bump / Rebound'); tmin=self._field(f,'Rebound [mm]',-60,0); tmax=self._field(f,'Bump [mm]',60,1); cg=self._field(f,'Camber gain [°/mm]',-0.025,2); bs=self._field(f,'Bump steer [°/mm]',0.002,3); mr=self._field(f,'Motion ratio',0.72,4); out=self._text(f,7)
        def run():
            rows=bump_rebound_sweep(float(tmin.get()),float(tmax.get()),25,camber_gain_deg_per_mm=float(cg.get()),bump_steer_deg_per_mm=float(bs.get()),motion_ratio_static=float(mr.get()))
            toe_span=max(r['toe_deg'] for r in rows)-min(r['toe_deg'] for r in rows); verdict='FAVORÁVEL' if toe_span<=0.5 else 'ATENÇÃO'
            self._show(out,rows,f'PARECER: {verdict} — variação total de toe = {toe_span:.3f}°.\nInclui camber, caster, KPI, scrub, trail, bitola, entre-eixos, roll center, motion ratio e curso do amortecedor.')
        ttk.Button(f,text='Executar sweep',command=run).grid(row=5,column=0,columnspan=2,pady=8)
    def _rates(self):
        f=self._tab('Molas / Frequências'); ks=self._field(f,'Mola [N/mm]',25,0); mr=self._field(f,'Motion ratio',0.72,1); kt=self._field(f,'Pneu [N/mm]',180,2); m=self._field(f,'Massa suspensa/canto [kg]',65,3); out=self._text(f,6)
        def run():
            r=spring_rates(float(ks.get()),float(mr.get()),float(kt.get()),float(m.get())); hz=r['natural_frequency_hz']; verdict='COERENTE PARA TRIAGEM OFF-ROAD' if 1.0<=hz<=2.5 else 'REVISAR COM DADOS DE PISTA'
            self._show(out,r,f'PARECER: {verdict} — frequência natural = {hz:.2f} Hz.\nFaixa é critério preliminar, não limite bibliográfico universal.')
        ttk.Button(f,text='Calcular taxas',command=run).grid(row=4,column=0,columnspan=2,pady=8)
    def _dynamics(self):
        f=self._tab('Dinâmica Intermediária'); m=self._field(f,'Massa [kg]',260,0); ay=self._field(f,'Ay [m/s²]',9.81,1); h=self._field(f,'CG [mm]',450,2); tr=self._field(f,'Bitola [mm]',1300,3); rc=self._field(f,'Roll center [mm]',60,4); out=self._text(f,7)
        def run():
            lt=lateral_load_transfer(float(m.get()),float(ay.get()),float(h.get()),float(tr.get())); jf=jacking_force(lt['total_transfer_n'],float(rc.get()),float(tr.get())/2)
            obj={'load_transfer':lt,'jacking_force_n':jf}; ratio=abs(jf)/max(1,lt['total_transfer_n']); verdict='CONTROLADO' if ratio<0.15 else 'ATENÇÃO AO JACKING'
            self._show(out,obj,f'PARECER: {verdict} — razão jacking/transferência = {100*ratio:.1f}%.\nAvaliar em conjunto com distribuição de rigidez ao rolamento e pneus.')
        ttk.Button(f,text='Calcular dinâmica',command=run).grid(row=5,column=0,columnspan=2,pady=8)
    def _industrial(self):
        f=self._tab('Industrial / Tolerâncias'); n=self._field(f,'Amostras',1000,0); tol=self._field(f,'Tolerância hardpoint [mm]',1.5,1); sens=self._field(f,'Sensibilidade [un/mm]',0.08,2); out=self._text(f,6)
        def run():
            r=tolerance_monte_carlo(int(float(n.get())),0,float(tol.get()),float(sens.get())); verdict='PROCESSO POTENCIALMENTE CAPAZ' if r['stdev']<0.1 else 'REVISAR TOLERÂNCIAS'
            self._show(out,r,f'PARECER: {verdict}.\nMonte Carlo preliminar; para liberação industrial incluir distribuição real, correlações, rastreabilidade e estudo MSA.')
        ttk.Button(f,text='Executar Monte Carlo',command=run).grid(row=3,column=0,columnspan=2,pady=8)
