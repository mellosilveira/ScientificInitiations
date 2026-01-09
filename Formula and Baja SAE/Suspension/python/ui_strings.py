SUMMARY_2D = """
=== RESUMO GEOMETRIA 2D (REIMPELL) ===
A) Lógica da Vista Frontal:
- IC (Centro Instantâneo): Interseção das linhas dos braços superior e inferior. Determina o ponto de pivô virtual da suspensão nesta vista.
- Centro de Rolagem (Ro): Interseção da linha IC-W (FVSA) com a linha central do veículo (x=0). Este ponto determina o braço de momento geométrico de rolagem.
- Fator-q: Métrica de curvatura de Reimpell. Usado para comparar o quanto o centro de rolagem migra durante o movimento.

B) Ganho de Cambagem:
- dφ: Ângulo de rolagem derivado do curso da suspensão.
- kγ: Fator de cambagem. Valores negativos indicam que a geometria compensa a rolagem (o pneu inclina para dentro da curva), o que melhora a aderência.
"""

SUMMARY_3D = """
=== RESUMO DE FORÇAS 3D ===
A) Distribuição de Forças:
- A força longitudinal (Fx_pneu, ex: Frenagem) é distribuída para os braços com base nas razões de rigidez.
- Efeito Geométrico: Braços inclinados geram forças parasitas Verticais (Fy) e Laterais (Fx) no chassi ao reagir a cargas de frenagem.

B) Anti-Mergulho (Anti-Dive):
- Calculado como (Fy_suspensão / Fz_necessário_para_100%) * 100.
- Determina o quanto a geometria da suspensão resiste ao mergulho da dianteira (nose dip) durante a frenagem, sem comprimir as molas.

C) Alinhamento:
- Camber/Caster/Toe são calculados projetando os pontos 3D nos planos 2D correspondentes.
"""

SUMMARY_BAJA = """
=== RESUMO DINÂMICA BAJA ===
A) Tendências (Variação de CG):
- Aumentar a altura do CG aumenta linearmente a Transferência de Carga (ΔFz) e o Momento de Rolagem.
- Risco Crítico: Se Fz_int (Carga na Roda Interna) cair para perto de zero, a roda interna levanta do chão, causando perda de tração e controle de estabilidade.

B) Momento na Manga (Feedback de Direção):
- Depende da Força Lateral e do Raio de Arrasto (Scrub Radius).
- Grande scrub radius + alta força lateral g = esforço de direção pesado para o piloto.
"""