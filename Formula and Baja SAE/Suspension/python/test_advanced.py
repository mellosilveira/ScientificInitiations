from calculators.advanced_suspension import *
assert len(ackermann_sweep(1500,1300))==15
assert len(bump_rebound_sweep())==25
assert spring_rates(25,.72)['wheel_rate_n_per_mm']>0
assert lateral_load_transfer(260,9.81,450,1300)['total_transfer_n']>0
print('MudRunner v3.0: testes avançados OK')
