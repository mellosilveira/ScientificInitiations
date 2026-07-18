# MudRunner — Baja Suspension Engineering Suite

Aplicação de engenharia da equipe MudRunner para análise geométrica, cinemática, dinâmica e estrutural preliminar de suspensões Baja SAE.

## Execução

```bash
python -m pip install -r requirements.txt
python run.py
```

## Principais recursos

- análise 2D dos centros instantâneos e centros de rolagem;
- análise 3D de kingpin, caster, scrub radius, trail e mecanismos anti;
- alinhamento dos quatro cantos;
- sweep cinemático de cambagem;
- cálculo de barra Panhard e reações nos suportes;
- varreduras de CG e massa;
- triagem estrutural dos braços;
- relatório técnico automático após cada cálculo;
- comparação com a execução anterior da mesma análise;
- exportação do relatório em TXT ou JSON;
- histórico de relatórios durante a sessão.

## Relatórios técnicos

As avaliações usam um índice de 0 a 100 para facilitar priorização. Esse índice é uma ferramenta de triagem, não uma certificação. As faixas precisam ser ajustadas às metas do veículo, pneu, terreno, regulamento e resultados experimentais.

Referências incorporadas ao mecanismo de relatório:

- Reimpell, Stoll & Betzler, *The Automotive Chassis*, 2ª ed., Seção 3.4, pp. 160–173: centros de rolagem;
- Reimpell, Stoll & Betzler, Seção 3.11, pp. 255–264: anti-dive e anti-squat;
- Milliken & Milliken, *Race Car Vehicle Dynamics*: suspensão, transferência de carga e estabilidade;
- MSC Adams/Car Getting Started 2021.0.2: análise e pós-processamento;
- Regulamento Baja SAE Brasil, Emenda 7: integridade de direção e suspensão.

## Limites do protótipo

Antes de uso industrial, recomenda-se adicionar:

- banco de dados versionado por projeto e veículo;
- autenticação, permissões e trilha de auditoria;
- sistema de unidades formal;
- validação de entrada por esquema;
- testes unitários e de regressão com geometrias de referência;
- integração contínua;
- logs estruturados;
- correlação com Adams/Car e ensaios físicos;
- análise estrutural de fadiga, flambagem, juntas e soldas;
- instalador, assinatura e gerenciamento de versões.


## Identidade visual

O logotipo oficial da equipe MudRunner está em `assets/mudrunner_logo.png` e é usado no cabeçalho e no ícone da janela. O carregamento é compatível com execução direta e empacotamento por PyInstaller.

## Próximos módulos recomendados

Consulte `SUSPENSION_FEATURE_ROADMAP.md` para a priorização técnica de Ackermann, bump/rebound, bump steer, motion ratio, rigidez ao rolamento e validação com Adams/Car.
