"""Relatórios técnicos automáticos para o protótipo de suspensão.

Os limites abaixo são faixas de triagem de engenharia, não critérios universais de
aprovação. Metas do veículo, pneus, terreno, regulamento e validação experimental
sempre prevalecem.
"""
from __future__ import annotations

from dataclasses import dataclass, asdict
from datetime import datetime
from typing import Any, Iterable
import json
import math


REFERENCES = {
    "roll_center": "Reimpell, Stoll & Betzler, The Automotive Chassis, 2ª ed., Seção 3.4, pp. 160–173.",
    "alignment": "Reimpell, Stoll & Betzler, The Automotive Chassis, 2ª ed., geometria e variações de toe/camber, caps. 3 e 4.",
    "anti": "Reimpell, Stoll & Betzler, The Automotive Chassis, 2ª ed., Seção 3.11, pp. 255–264.",
    "vehicle_dynamics": "Milliken & Milliken, Race Car Vehicle Dynamics, capítulos de suspensão, transferência de carga e estabilidade.",
    "validation": "MSC Adams/Car Getting Started 2021.0.2, procedimentos de montagem, análises e pós-processamento.",
    "rules": "Regulamento Administrativo e Técnico Baja SAE Brasil, Emenda 7, requisitos de integridade de direção e suspensão.",
}


@dataclass
class Finding:
    level: str
    title: str
    detail: str
    recommendation: str = ""


@dataclass
class EngineeringReport:
    report_type: str
    title: str
    score: int
    verdict: str
    summary: str
    findings: list[Finding]
    metrics: dict[str, Any]
    references: list[str]
    comparison: str
    created_at: str

    def to_dict(self) -> dict[str, Any]:
        d = asdict(self)
        return d

    def to_text(self) -> str:
        lines = [
            self.title,
            "=" * len(self.title),
            f"Data: {self.created_at}",
            f"Índice de engenharia: {self.score}/100 — {self.verdict}",
            f"Comparação: {self.comparison}",
            "",
            "RESUMO EXECUTIVO",
            self.summary,
            "",
            "CONSTATAÇÕES",
        ]
        for f in self.findings:
            lines.append(f"[{f.level}] {f.title}: {f.detail}")
            if f.recommendation:
                lines.append(f"    Ação recomendada: {f.recommendation}")
        lines += ["", "MÉTRICAS"]
        for k, v in self.metrics.items():
            lines.append(f"- {k}: {v}")
        lines += ["", "REFERÊNCIAS E CRITÉRIO"]
        for ref in self.references:
            lines.append(f"- {ref}")
        lines += [
            "",
            "NOTA DE ENGENHARIA",
            "As faixas são critérios de triagem do protótipo. A aprovação final exige metas do projeto, análise de sensibilidade, verificação estrutural, correlação com Adams/Car e ensaios físicos.",
        ]
        return "\n".join(lines)


class EngineeringReportEngine:
    def __init__(self):
        self.previous_scores: dict[str, int] = {}

    def _finish(self, report_type: str, title: str, score: float, summary: str,
                findings: list[Finding], metrics: dict[str, Any], refs: Iterable[str]) -> EngineeringReport:
        score_i = max(0, min(100, int(round(score))))
        if score_i >= 85:
            verdict = "ROBUSTO PARA A FASE ATUAL"
        elif score_i >= 70:
            verdict = "ADEQUADO, COM PONTOS DE ATENÇÃO"
        elif score_i >= 50:
            verdict = "REVISÃO RECOMENDADA"
        else:
            verdict = "CRÍTICO — REPROJETAR/VALIDAR"
        old = self.previous_scores.get(report_type)
        if old is None:
            comparison = "Primeira avaliação registrada; será usada como referência nas próximas execuções."
        else:
            delta = score_i - old
            if delta > 2:
                comparison = f"Melhorou {delta} pontos em relação ao cálculo anterior ({old}/100)."
            elif delta < -2:
                comparison = f"Piorou {abs(delta)} pontos em relação ao cálculo anterior ({old}/100)."
            else:
                comparison = f"Permaneceu tecnicamente estável em relação ao cálculo anterior ({old}/100)."
        self.previous_scores[report_type] = score_i
        return EngineeringReport(
            report_type, title, score_i, verdict, summary, findings, metrics,
            list(refs), comparison, datetime.now().strftime("%d/%m/%Y %H:%M:%S")
        )

    def report_2d(self, front_rc: float | None, rear_rc: float | None, hcg: float) -> EngineeringReport:
        findings: list[Finding] = []
        score = 100.0
        metrics = {"RC dianteiro [mm]": front_rc, "RC traseiro [mm]": rear_rc, "CG [mm]": hcg}
        for name, rc in (("dianteiro", front_rc), ("traseiro", rear_rc)):
            if rc is None or not math.isfinite(rc):
                score -= 40; findings.append(Finding("CRÍTICO", f"RC {name}", "não foi determinado.", "Revisar paralelismo, hardpoints e convenção de eixos.")); continue
            if rc < 0:
                score -= 28; findings.append(Finding("ALERTA", f"RC {name} negativo", f"{rc:.1f} mm abaixo do solo aumenta o braço geométrico de rolagem.", "Rever inclinação/projeção dos braços e acompanhar migração do RC no curso."))
            elif rc > 0.65 * hcg:
                score -= 20; findings.append(Finding("ALERTA", f"RC {name} elevado", f"{rc:.1f} mm equivale a {100*rc/hcg:.0f}% da altura do CG; pode elevar jacking e respostas abruptas.", "Reduzir e validar em sweep de rolagem."))
            else:
                findings.append(Finding("OK", f"RC {name}", f"{rc:.1f} mm, abaixo do CG e sem inversão de sinal na condição estática."))
        if front_rc is not None and rear_rc is not None:
            delta = rear_rc-front_rc
            if abs(delta) > 150:
                score -= 12; findings.append(Finding("ATENÇÃO", "Inclinação do eixo de rolagem", f"Diferença traseira–dianteira de {delta:+.1f} mm.", "Avaliar distribuição de rigidez de rolagem e comportamento transitório."))
        return self._finish("2d", "Relatório técnico — Geometria 2D e centros de rolagem", score,
            "A avaliação verifica existência, sinal e ordem de grandeza dos centros de rolagem em relação ao CG. Não existe uma altura universalmente ótima; estabilidade ao longo do curso é tão importante quanto o valor estático.", findings, metrics,
            [REFERENCES["roll_center"], REFERENCES["vehicle_dynamics"], REFERENCES["validation"]])

    def report_alignment(self, data: dict[str, Any]) -> EngineeringReport:
        score = 100.0; findings=[]; metrics={}
        for axle in ("FRONT", "REAR"):
            d=data.get(axle) or {}; per=d.get("per_corner", {})
            metrics[f"{axle} camber médio [deg]"]=round(d.get("avg_camber_deg",0),3)
            metrics[f"{axle} caster médio [deg]"]=round(d.get("avg_caster_deg",0),3)
            metrics[f"{axle} toe médio [deg]"]=round(d.get("avg_toe_deg",0),3)
            vals=list(per.values())
            if len(vals)==2:
                for k,label,tol in (("camber_deg","camber",0.5),("caster_deg","caster",1.0),("toe_deg","toe",0.3)):
                    asym=abs(vals[0][k]-vals[1][k]); metrics[f"{axle} assimetria {label} [deg]"]=round(asym,3)
                    if asym>tol:
                        score-=10; findings.append(Finding("ATENÇÃO", f"Assimetria de {label} no {axle}", f"Diferença esquerda–direita de {asym:.3f}°.", "Confirmar simetria geométrica ou justificar por setup intencional."))
            toe=abs(d.get("avg_toe_deg",0)); caster=abs(d.get("avg_caster_deg",0)); cam=abs(d.get("avg_camber_deg",0))
            if toe>1.0: score-=15; findings.append(Finding("ALERTA", f"Toe elevado no {axle}", f"Média absoluta {toe:.2f}° pode aumentar arrasto e instabilidade.", "Revisar tie-rod e convenção de sinal."))
            if caster>15: score-=12; findings.append(Finding("ATENÇÃO", f"Caster extremo no {axle}", f"{caster:.2f}° exige validação de esforço de direção e trail."))
            if cam>6: score-=12; findings.append(Finding("ATENÇÃO", f"Camber estático extremo no {axle}", f"{cam:.2f}° pode reduzir área de contato em linha reta."))
        if not findings: findings.append(Finding("OK", "Consistência de alinhamento", "Não foram detectadas assimetrias ou magnitudes extremas pelos limites de triagem."))
        return self._finish("alignment", "Relatório técnico — Alinhamento", score,
            "O relatório prioriza simetria entre lados, magnitudes não extremas e coerência geométrica. Os alvos finais devem ser definidos com dados de pneu, compliance e missão do Baja.", findings, metrics,
            [REFERENCES["alignment"], REFERENCES["vehicle_dynamics"], REFERENCES["rules"]])

    def report_kinematics(self, results: list[dict[str, Any]], corner: str) -> EngineeringReport:
        findings=[]; score=100.0
        gains=[float(r.get("gain_deg_per_mm",0)) for r in results if math.isfinite(float(r.get("gain_deg_per_mm",0)))]
        cams=[float(r.get("new_camber_deg",0)) for r in results]
        max_gain=max((abs(v) for v in gains), default=0); span=(max(cams)-min(cams)) if cams else 0
        metrics={"Canto":corner,"Pontos":len(results),"Ganho máximo |deg/mm|":round(max_gain,5),"Variação total camber [deg]":round(span,3)}
        if max_gain>0.12: score-=28; findings.append(Finding("ALERTA","Ganho de camber muito agressivo",f"Pico de {max_gain:.4f} °/mm.","Revisar posição dos pivôs e verificar inversões/condições singulares."))
        elif max_gain<0.003: score-=10; findings.append(Finding("ATENÇÃO","Baixo ganho de camber",f"Pico de apenas {max_gain:.4f} °/mm.","Verificar se há compensação suficiente durante rolagem para a meta de pneu."))
        else: findings.append(Finding("OK","Ganho de camber",f"Pico de {max_gain:.4f} °/mm dentro de faixa de triagem moderada."))
        jumps=[abs(cams[i]-cams[i-1]) for i in range(1,len(cams))]
        if jumps and max(jumps)>2.0: score-=25; findings.append(Finding("CRÍTICO","Descontinuidade no sweep",f"Salto máximo de {max(jumps):.2f}° entre passos.","Investigar singularidade, troca de ramo ou erro de solver."))
        return self._finish("kinematics",f"Relatório técnico — Sweep cinemático ({corner})",score,
            "A avaliação verifica suavidade e magnitude do ganho de camber ao longo do curso. A curva completa deve ser confrontada com rolagem prevista e sensibilidade do pneu.",findings,metrics,
            [REFERENCES["alignment"],REFERENCES["roll_center"],REFERENCES["validation"]])

    def report_panhard(self, data: dict[str, Any]) -> EngineeringReport:
        findings=[]; score=100.0; metrics={}
        for axle,d in data.items():
            if not d: score-=35; findings.append(Finding("CRÍTICO",axle,"resultado ausente.")); continue
            ux=abs(float(d["ux"])); ratio=abs(float(d["F_axial"]))/max(abs(float(d["Fy_axle"])),1e-9)
            metrics[f"{axle} |ux|"]=round(ux,4); metrics[f"{axle} amplificação axial"]=round(ratio,3)
            if ux<0.35: score-=25; findings.append(Finding("ALERTA",f"Panhard {axle} pouco lateral",f"|ux|={ux:.3f}, amplificando a força axial em {ratio:.2f}×.","Aumentar componente lateral e verificar embalagem/curso."))
            elif ux<0.7: score-=10; findings.append(Finding("ATENÇÃO",f"Panhard {axle}",f"Amplificação axial {ratio:.2f}×; verificar suportes e soldas."))
            else: findings.append(Finding("OK",f"Panhard {axle}",f"Boa eficiência direcional, |ux|={ux:.3f}."))
        return self._finish("panhard","Relatório técnico — Barra Panhard e reações",score,
            "A análise compara a componente lateral da barra com a força transmitida. Quanto menor a projeção lateral, maior a força axial e a solicitação dos suportes.",findings,metrics,
            [REFERENCES["roll_center"],REFERENCES["vehicle_dynamics"],REFERENCES["rules"]])

    def report_3d(self, result: dict[str, Any], corner: str) -> EngineeringReport:
        m=result["metrics"]; kp=m.kingpin_parameters; ad=m.anti_dive_parameters; aq=m.anti_squat_parameters
        findings=[]; score=100.0
        scrub=abs(float(kp.scrub_radius)); anti_d=abs(float(ad.percentage)); anti_s=abs(float(aq.percentage))
        metrics={"Canto":corner,"Scrub radius [mm]":round(kp.scrub_radius,2),"Mechanical trail [mm]":round(kp.mechanical_trail,2),"Anti-dive [%]":round(ad.percentage,1),"Anti-squat [%]":round(aq.percentage,1)}
        if scrub>100: score-=25; findings.append(Finding("ALERTA","Scrub radius elevado",f"{scrub:.1f} mm pode aumentar kickback e esforço de direção.","Rever KPI, offset da roda e posição do contato."))
        elif scrub>50: score-=10; findings.append(Finding("ATENÇÃO","Scrub radius moderado",f"{scrub:.1f} mm; validar kickback no terreno."))
        else: findings.append(Finding("OK","Scrub radius",f"{scrub:.1f} mm, sem magnitude extrema na triagem."))
        for label,val in (("Anti-dive",anti_d),("Anti-squat",anti_s)):
            if val>120: score-=20; findings.append(Finding("ALERTA",label,f"{val:.1f}% acima de 100% pode gerar reações geométricas agressivas.","Validar forças, conforto, tração e sensibilidade a frenagem/aceleração."))
            elif val>80: score-=8; findings.append(Finding("ATENÇÃO",label,f"{val:.1f}% é elevado e requer validação."))
        st=result.get("steering")
        if st is not None:
            ack=float(st.ackermann_percentage); metrics["Ackermann [%]"]=round(ack,1)
            if ack<50 or ack>150: score-=15; findings.append(Finding("ATENÇÃO","Ackermann fora da faixa de triagem",f"{ack:.1f}%.","Relacionar ao raio de curva, escorregamento dos pneus e objetivo off-road."))
        return self._finish("3d",f"Relatório técnico — Análise 3D ({corner})",score,
            "A avaliação integra direção, kingpin e mecanismos anti. Os valores são indicadores geométricos e não substituem forças de direção, compliance e simulação de veículo completo.",findings,metrics,
            [REFERENCES["anti"],REFERENCES["alignment"],REFERENCES["vehicle_dynamics"],REFERENCES["validation"]])

    def report_sweep(self, kind: str, results: list[Any]) -> EngineeringReport:
        score=90.0; findings=[]; metrics={"Pontos":len(results)}
        if not results: score=20; findings.append(Finding("CRÍTICO","Sweep vazio","Nenhum resultado foi produzido."))
        else:
            findings.append(Finding("OK","Execução do sweep",f"{len(results)} condições avaliadas sem interrupção."))
            if kind=="cg":
                vals=[float(r.d_fz) for r in results]; metrics["ΔFz mínimo/máximo [N]"]=f"{min(vals):.1f} / {max(vals):.1f}"
                findings.append(Finding("INFORMAÇÃO","Tendência esperada","A transferência de carga cresce com a altura do CG; menor CG é favorável à margem de estabilidade."))
            elif kind=="mass":
                margins=[float(r["margin"]) for r in results]; metrics["Margem mínima/máxima"]=f"{min(margins):.2f} / {max(margins):.2f}"
                if min(margins)<0: score-=35; findings.append(Finding("CRÍTICO","Margem negativa",f"Mínimo {min(margins):.2f}.","Rever CG, bitola, massa e condição lateral de projeto."))
        title="CG" if kind=="cg" else "Massa"
        return self._finish(kind,f"Relatório técnico — Sweep de {title}",score,
            "O sweep é uma análise de sensibilidade: identifica tendências e condições críticas, mas depende da validade das hipóteses de carga e do modelo.",findings,metrics,
            [REFERENCES["vehicle_dynamics"],REFERENCES["validation"]])

    def report_structural(self, results: list[dict[str, Any]], limit: float) -> EngineeringReport:
        findings=[]; metrics={"Casos":len(results),"Limite [N]":limit}; score=100.0
        max_force=max((max(abs(float(r["force_sup"])),abs(float(r["force_inf"]))) for r in results), default=0)
        metrics["Força máxima [N]"]=round(max_force,1); utilization=max_force/max(limit,1e-9)
        metrics["Utilização"]=round(utilization,3)
        if utilization>1: score=30; findings.append(Finding("CRÍTICO","Limite excedido",f"Utilização {utilization:.2f}×.","Redimensionar e realizar análise estrutural com fatores de segurança e casos de impacto."))
        elif utilization>0.8: score=65; findings.append(Finding("ATENÇÃO","Baixa reserva",f"Utilização {100*utilization:.0f}%.","Aumentar margem para incertezas, fadiga, soldas e impactos."))
        else: findings.append(Finding("OK","Reserva preliminar",f"Utilização máxima {100*utilization:.0f}% do limite informado."))
        return self._finish("structural","Relatório técnico — Triagem estrutural",score,
            "Esta etapa é apenas uma triagem de esforços nos braços. Aprovação estrutural exige materiais, seções, flambagem, fadiga, juntas, soldas e casos de impacto regulamentares.",findings,metrics,
            [REFERENCES["rules"],REFERENCES["validation"]])
