SUMMARY_2D = """
=== RESUMO GEOMETRIA 2D (REIMPELL) ===
A) Lógica da Vista Frontal:
- IC (Centro Instantâneo): Interseção das linhas dos braços superior e inferior.
- Centro de Rolagem (Ro): Interseção da linha IC-W (FVSA) com a linha central do veículo (x=0).
- Fator-q: Métrica de curvatura de Reimpell (estabilidade do Ro).

B) Ganho de Cambagem:
- dφ: Ângulo de rolagem derivado do curso da suspensão.
- kγ: Fator de cambagem. Negativo = geometria compensa a rolagem.
"""

SUMMARY_3D = """
=== RESUMO DE FORÇAS 3D ===
A) Distribuição de Forças:
- Força longitudinal (Frenagem) distribuída via rigidez relativa.
- Braços inclinados geram forças parasitas (Vertical Fy e Lateral Fx).

B) Anti-Mergulho (Anti-Dive):
- % da transferência de carga suportada pela geometria (braços) e não pelas molas.

C) Alinhamento:
- Camber/Caster/Toe calculados projetando os vetores 3D nos planos ortogonais.
"""

SUMMARY_BAJA = """
=== RESUMO DINÂMICA BAJA ===
A) Varredura de CG:
- Aumentar CG = Maior Transferência de Carga e Momento de Rolagem.
- Cuidado com Fz_int (roda interna) próxima de zero (perda de contato).

B) Varredura de Massa:
- SSF (Static Stability Factor) = (Bitola/2) / h_CG.
- Margem de Segurança = Ay_atual / Ay_crítico.
"""