/* Runge Kutta for a set of first order differential equations */
/* Este programa resolve as equacoes de estado do Sistema Dinamico*/
/* Massa Mola Amortecedor com rigidez descontínua e escreve os resultados*/
/* num arquivo no seguinte formato: tempo posicao velocidade*/
#include <stdio.h>
#include <math.h>

/* number of first order equations */
#define N 2			
/* stepsize in t*/
#define dist 1e-2 
/* stepsize in w*/
#define dist1 1e-2 
/* max for w */
#define MAX1 30.0		
/* max for t */
#define MAX 40.0		

/*Parametros do Sistema Mecanico*/
double k, c, m, g, w, pi, bratio, wn;   

FILE* output;	
FILE* mola;
FILE* frequencia;

double  f(double x, double y[], int i, double w)
{
	/*rigidez(N/m)*/
	k = 5000; 
	/*massa(kg)*/
	m = 20; 
	/*frequencia natural de vibração*/
	wn = pow((k / m), 0.5); 
	/*razão de amortecimento*/
	bratio = 0.05; 
	/*amortecimento*/
	c = bratio * 2 * m * wn; 
	/* Condição de ressonância */
	//w = wn;

	if (i == 0)
	{
		/* derivative of first equation */
		return(y[1]);
	}			
	if (i == 1)
	{
		/* derivative of second equation */
		return((10 * (sin((w * x))) - (c * y[1]) - (k * y[0])) / m);	
	}
}

/* Algoritmo de Runge Kutta 4a. ordem */
void RungeKutta4(double x, double y[], double step, double w)
{
	double h = step / 2.0,			/* the midpoint */
		t1[N], t2[N], t3[N],		/* temporary storage arrays */
		k1[N], k2[N], k3[N], k4[N];	/* for Runge-Kutta */

	for (int i = 0; i < N; i++)
	{
		t1[i] = y[i] + 0.5 * (k1[i] = step * f(x, y, i, w));
	}
	for (int i = 0; i < N; i++) 
	{
		t2[i] = y[i] + 0.5 * (k2[i] = step * f(x + h, t1, i, w));
	}
	for (int i = 0; i < N; i++) 
	{ 
		t3[i] = y[i] + (k3[i] = step * f(x + h, t2, i, w)); 
	}
	for (int i = 0; i < N; i++) 
	{
		k4[i] = step * f(x + step, t3, i, w);
	}
	for (int i = 0; i < N; i++)
	{
		y[i] += (k1[i] + 2 * k2[i] + 2 * k3[i] + k4[i]) / 6.0;
	}
}

/* Programa Principal */
main()
{
	double t, y[N], z;

	pi = 4 * atan(1);            /*numero pi*/

	output = fopen("LINEAR.dat", "w");			/* external filename */
	mola = fopen("LINEARMOLA.dat", "w");			/* external filename */
	frequencia = fopen("freq.dat", "w");			/* external filename */

	/* initial position */
	y[0] = 0.0;					
	/* initial velocity */
	y[1] = 0.0;					

	for (int jp = 1; jp * dist1 <= MAX1; jp++)
	{
		z = 0.0;
		w = jp * dist1;

		for (int j = 1; j * dist <= MAX; j++)			/* time loop */
		{
			t = j * dist;
			RungeKutta4(t, y, dist, w);

			fprintf(output, "%g\t %g\t %g\t %g\n", w, t, y[0], y[1]);

			if (t > 8)
			{
				if (y[0] > z)
				{
					z = y[0];
				}
			}
		}

		fprintf(frequencia, "%g\t %g\n", w, z);
	}

	fclose(mola);
	fclose(output);
	fclose(frequencia);
}