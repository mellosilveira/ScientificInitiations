/* Runge Kutta for a set of first order differential equations */
/* Este programa resolve as equacoes de estado do Sistema Dinamico*/
/* Massa Mola Amortecedor com rigidez descontínua e escreve os resultados*/
/* num arquivo no seguinte formato: tempo posicao velocidade*/
#include <stdio.h>
#include <math.h>

#define N 4			/* number of first order equations */
#define dist 1e-2 /* stepsize in t*/
#define dist1 1e-1 /* stepsize in w*/
#define MAX 20.0		/* max for t */
#define MAX1 30.0		/* max for w */
#define maximo 1
double k, c, m, g, w, pi, bratio, wn, k1, m1, c1;   /*Parametros do Sistema Mecanico*/

FILE* output;			/* internal filename */
FILE* mola;
FILE* frequencia;

/* Equacao de Estado que se deseja integrar */

double  f(double x, double y[], int i, double w)
{
	k = 5000; /*rigidez(N/m)*/
	k1 = 250; /*rigidez(N/m)*/
	m = 20; /*massa(kg)*/
	m1 = 1; /*massa(kg)*/
	wn = pow((k / m), 0.5); /*frequencia natural de vibração*/
	bratio = 0.05; /*razão de amortecimento*/
	c = bratio * 2 * m * wn; /*amortecimento*/
	c1 = bratio * 2 * m1 * wn; /*amortecimento*/
	/* Condição de ressonância */
	//w = wn;

	if (i == 0) return(y[2]);			/* derivative of first equation */
	if (i == 1) return(y[3]);       /* derivative of first equation */
	if (i == 2) return(((10 * (sin((w * x)))) - ((c + c1) * y[2]) - ((k + k1) * y[0]) + k1 * y[1] + c1 * y[3]) / m);	/* derivative of second equation */
	if (i == 3) return((c1 * (y[2] - y[3])) + ((k1 * (y[0] - y[1]))) / m1);	/* derivative of second equation */
}



/* Algoritmo de Runge Kutta 4a. ordem */

void runge4(double t, double y[], double step, double w)
{
	double h = step / 2.0,			/* the midpoint */
		t1[N], t2[N], t3[N],		/* temporary storage arrays */
		k1[N], k2[N], k3[N], k4[N];	/* for Runge-Kutta */
	int i;

	for (i = 0; i < N; i++) t1[i] = y[i] + 0.5 * (k1[i] = step * f(t, y, i, w));
	for (i = 0; i < N; i++) t2[i] = y[i] + 0.5 * (k2[i] = step * f(t + h, t1, i, w));
	for (i = 0; i < N; i++) t3[i] = y[i] + (k3[i] = step * f(t + h, t2, i, w));
	for (i = 0; i < N; i++) k4[i] = step * f(t + step, t3, i, w);

	for (i = 0; i < N; i++) y[i] += (k1[i] + 2 * k2[i] + 2 * k3[i] + k4[i]) / 6.0;
}

/* Programa Principal */

main()
{
	double t, y[N], z;
	int j, jp;


	/*Definicao dos parametros do sistema mecanico*/

	pi = 4 * atan(1);            /*numero pi*/

	output = fopen("LINEAR.dat", "w");			/* external filename */
	mola = fopen("LINEAR1.dat", "w");			/* external filename */
	frequencia = fopen("freq.dat", "w");			/* external filename */

	y[0] = 0.0;					/* initial position */
	y[1] = 0.0;					/* initial velocity */
	y[2] = 0.0;					/* initial position */
	y[3] = 0.0;

	for (jp = 1; jp * dist1 <= MAX1; jp++)
	{
		z = 0.0;
		w = jp * dist1;
		for (j = 1; j * dist <= MAX; j++)			/* time loop */
		{
			t = j * dist;
			runge4(t, y, dist, w);
			if (sqrt((w - 11) * (w - 11)) < 0.0001)
			{

				fprintf(output, "%g\t %g\t %g\t %g\t %g\t %g\n", w, t, y[0], y[1], y[2], y[3]);

			}
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





