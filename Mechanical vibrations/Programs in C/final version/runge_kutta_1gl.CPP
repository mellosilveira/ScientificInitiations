#include <stdio.h>
#include <math.h>

/*Number of first order equations*/
#define N 2			
/*Stepsize in t*/
#define dist 1e-2 
/*Stepsize in w*/
#define dist1 1e-2 
/*Max for w */
#define MAX1 30.0		
/*Max for t */
#define MAX 40.0		

/*Mechanical system parameters*/
double k, c, m, force, w, pi, bratio, wn;

FILE* output;
FILE* mola;
FILE* frequencia;

double  f(double x, double y[], int i, double w)
{
	/*Stiffness(N/m)*/
	k = 5000;
	/*Mass(kg)*/
	m = 20;
	/*Natural frequency*/
	wn = pow((k / m), 0.5);
	/*Damping ratio*/
	bratio = 0.05;
	/*Damping*/
	c = bratio * 2 * m * wn;
	/*Force*/
	force = 10;

	if (i == 0)
	{
		/* Derivative of first equation */
		return(y[1]);
	}
	if (i == 1)
	{
		/* derivative of second equation */
		return((force * (sin((w * x))) - (c * y[1]) - (k * y[0])) / m);
	}
}

/* Runge Kutta 4a. order algorithm */
void RungeKutta4(double x, double y[], double step, double w)
{
	double h = step / 2.0,			/* The midpoint */
		/* Temporary storage arrays for Runge-Kutta */
		t1[N], t2[N], t3[N],		
		k1[N], k2[N], k3[N], k4[N];

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

/* Main program */
main()
{
	double t, y[N], z;

	pi = 4 * atan(1);            /*Number pi*/

	output = fopen("LINEAR.dat", "w");				/* External filename */
	mola = fopen("LINEARMOLA.dat", "w");			/* External filename */
	frequencia = fopen("freq.dat", "w");			/* External filename */

	/* Initial position */
	y[0] = 0.0;
	/* Initial velocity */
	y[1] = 0.0;

	for (int jp = 1; jp * dist1 <= MAX1; jp++)
	{
		z = 0.0;
		w = jp * dist1;

		for (int j = 1; j * dist <= MAX; j++)			/* Time loop */
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