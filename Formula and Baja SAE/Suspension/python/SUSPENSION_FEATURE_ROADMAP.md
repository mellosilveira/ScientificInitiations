# Roadmap técnico de suspensão — MudRunner

## Prioridade 1 — Geometria de direção e curso

### Geometria de Ackermann

Implementar cálculo estático e sweep em função do esterçamento. Entradas mínimas: entre-eixos, bitola dianteira, posição da cremalheira, hardpoints internos e externos das barras de direção, braços de direção e ângulos das rodas. Saídas: ângulo interno ideal, ângulo externo ideal, Ackermann percentual, erro angular por esterçamento, raio de curva e gráfico roda interna × roda externa.

Referência conceitual: Reimpell, Stoll & Betzler, capítulos de direção e geometria de esterçamento; Milliken & Milliken, tratamento de steer geometry e comportamento em curva.

### Bump e rebound

Implementar sweep completo do curso em compressão (bump) e extensão (rebound), preservando comprimentos dos braços e resolvendo a posição da manga. Saídas: camber gain, toe/bump steer, caster, kingpin inclination, scrub radius, trail, track change, wheelbase change, altura do roll center e migração do instant center.

O software já possui uma base de sweep cinemático; deve ser ampliada para resolver todos os hardpoints e não somente exibir a variação de cambagem.

## Prioridade 2 — Métricas de projeto

- bump steer por roda e por eixo;
- motion ratio da mola e do amortecedor;
- wheel rate e ride rate;
- rigidez ao rolamento por eixo e distribuição dianteira/traseira;
- frequência natural, amortecimento crítico e transmissibilidade;
- jounce/rebound travel disponível e verificação de batentes;
- variação de bitola e entre-eixos ao longo do curso;
- anti-dive, anti-squat e anti-lift com convenção de sinais documentada;
- envelope de interferência entre pneu, manga, braços, amortecedor e chassi.

## Prioridade 3 — Correlação e industrialização

- exportação/importação de hardpoints para Adams/Car;
- comparação automática entre curvas do programa, Adams e ensaio físico;
- tolerâncias de fabricação e análise de Monte Carlo;
- versionamento de geometrias e aprovação por revisão;
- sistema formal de unidades;
- testes de regressão com geometrias de referência;
- logs, auditoria e relatórios PDF assinados;
- banco de dados de veículos, subsistemas e configurações.

## Critério de implementação

Cada novo módulo deve conter: entradas validadas, convenção de eixos explícita, equações documentadas, gráfico, relatório técnico, comparação com a configuração anterior, limites de validade e referência bibliográfica.
