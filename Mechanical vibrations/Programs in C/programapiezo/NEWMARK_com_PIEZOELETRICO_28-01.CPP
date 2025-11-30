#include <stdio.h>
#include <io.h>
#include <conio.h>
#include <string.h>
#include <math.h>
#include <stdlib.h>

//#define N 2*gl			/* number of first order equations */
//#define dist 1e-2 /* stepsize in t*/
#define dist1 1e-1 /* stepsize in w*/
#define MAX 30.0		/* max for t */
#define MAX1 30.0		/* max for w */
#define cont2 1.0

#define TOL 1e-2
#define NITER 1000

#define  nGmax  	3    // numero de nós
#define  nDim		2    // número de dimensões
#define  nELE		2    // número de elementos
#define  NGL		6    // número de graus de liberdade máximo sem o grau de liberdade do potencial
#define  NGL1		3    // número de graus de liberdade máximo dividido por 2
#define  NGL2		9    // número de graus de liberdade máximo+número de graus de liberdade máximo dividido por 2
#define  GL			4	 // número de graus de liberdade por elemento
#define  GL1	  	2	 // número de graus de liberdade por elemento dividido por 2
#define  GLS		7    // número de graus de liberdade menos as condições de contorno igual a zero
#define  GLP    	2    // número de potencial por elemento
#define  GLPT    	3    // número de potencial total
#define  CCT    	6    // número de grau de liberdade desprezando as condições de contorno
int i, j, k;		     //auxiliares para função for
int nNs; 				 //número de nós
int nEl;                 //número de elementos
int n;                   //auxilar para função for (M e K)
int p, q, r, s;          //auxiliares para montar as matrizes M e K completas
int z;                   //auxiliar da função principal para dados de saída
int i_aux;               //auxiliar para leitura dos dados de entrada                    //densidade
int gl;                  //número de graus de liberdade
int N, N2;               //N = (nEl + 1) * gl
int cont;				 //contar condições de contorno iguais a zero
int cont1;
int algo;
int nm, np, nt_Transiente;
int nbif;
int key, kont;
long double dens;
long double aux;
long double a, b, c;
long double d, ef, fe, g;
long double h, h1, nh, mn;				      //auxiliares para cálculo das matrizes M e K completas
long double t, t0, dt;							//tempo
long double dist;						//mesma função que dist1 -> passo
long double Wo, W;
long double Wi, Wf, dW;
long double lp, ls; 							//tamanho do elemento analisado
long double PI;
long double tf;
long double gama, alfa, f1[6];
long double a0, a1, a2, a3, a4, a5, a6, a7;
long double hs, hp, bs, bp;
long double MoIs, MoIp;
long double aes, aep;
long double e31, s33, k33, u1;
int np_F1, np_F2, np_F3, np_F4, np_F5, np_F6;
int nbp;

//matriz massa estrutura para um elemento
long double Mls[GL][GL];

//matriz massa estrutura para n elemento, onde NGL é o numero máximo de grau de liberdades sem considerar os graus e liberdade dos potenciais.
long double Ms[NGL][NGL];

//matriz rigidez estrutura para um elemento
long double KEls[GL][GL];

//matriz rigidez estrutura para n elemento, onde NGL é o numero máximo de grau de liberdades sem considerar os graus e liberdade dos potenciais.
long double Ks[NGL][NGL];

//matriz massa piezo para um elemento
long double Mlp[GL][GL];

//matriz massa estrutura para n elemento, onde NGL é o numero máximo de grau de liberdades sem considerar os graus e liberdade dos potenciais.
long double Mp[NGL][NGL];

//matriz rigidez estrutura para um elemento
long double KElp[GL][GL];

//matriz rigidez estrutura para n elemento, onde NGL é o numero máximo de grau de liberdades sem considerar os graus e liberdade dos potenciais.
long double Kp[NGL][NGL];

//soma da  massa da estrutura com matriz massa do piezo, onde NGL é o numero máximo de grau de liberdades sem considerar os graus e liberdade dos potenciais.
long double M[NGL][NGL];

//soma da  rigidez da estrutura com matriz rigidez do piezo, onde NGL é o numero máximo de grau de liberdades sem considerar os graus e liberdade dos potenciais.
long double K[NGL][NGL];

//Formação da Matriz massa incluindo os graus de liberdade relacionados com os potenciais, onde NGL2 = NGL + NGL/2.
long double M_G[NGL2][NGL2];

//Formação da Matriz RIGIDEZ incluindo os graus de liberdade relacionados com os potenciais, onde NGL2 = NGL + NGL/2.
long double K_G[NGL2][NGL2];

//Formação da Matriz AMORTECIMENTO incluindo os graus de liberdade relacionados com os potenciais, onde NGL2 = NGL + NGL/2.
long double C[NGL2][NGL2];

//Formação da Matriz massa incluindo os graus de liberdade relacionados com os potenciais, e diminuindo o numero e condições de contorno.
long double M_cc[CCT][CCT];

//Formação da Matriz RIGIDEZ incluindo os graus de liberdade relacionados com os potenciais, e diminuindo o numero e condições de contorno.
long double K_cc[CCT][CCT];

//Formação da Matriz AMORTECIMENTO incluindo os graus de liberdade relacionados com os potenciais,e diminuindo o numero e condições de contorno.
long double C_cc[CCT][CCT];

//Formação da Matriz acoplamento u v por elemento
long double K_u_vp[GL][GLP];

//Formação da Matriz acoplamento v  u por elemento
long double K_v_up[GL][GLP];

//Formação da Matriz acoplamento total u v
long double K_u_v[NGL][GLPT];

//Formação da Matriz acoplamento total v u
long double K_v_u[GLPT][NGL];

//Formação da Matriz acoplamento v v por elemento
long double K_v_vp[GLP][GLP];

//Formação da Matriz acoplamento total v v
long double K_v_v[GLPT][GLPT];

long double xG[nGmax][nELE];           //matriz 3x2 coordenadas dos nós

long double P2a[CCT][CCT];
long double P3a[CCT][CCT];
long double P4a[CCT][CCT];
long double P1[CCT][CCT];              //[P1] = a0*[M] + a1*[C] + [K];

int nE[nELE][2];              		   //matriz 2x2 coordenadas dos elementos
int MatEl[nELE][GL];				   //matriz 2x4 identificação dos elementos

long double fa[CCT];                   //vetor forçamento [4] aplicado as condições de contorno
long double P2[CCT];                   //[P2] = [M] x (a0*[y] + a2*[vel] + a3*[acel])

long double P3[CCT];                   //[P3] = [C]*(a1*[y] + a4*[vel] + a5*[acel])
long double P4[CCT];                   //[P4] = [fa]*sin(W*t) + [P2] + [P3]
long double P6[CCT], P6a[CCT];
long double P7[CCT];                   //vetor delta y para calcular o resultado
long double P7_ant[CCT];               //valor anterior do vetor P7
long double fa_ant[CCT];               //vetor forçamento 4x4 no instante anterior
long double delta_fa[CCT];             //[delta_fa] = [fa]i - [fa]i-1

long double K_Y[CCT];      		       //vetor produto [K] x [y]
long double C_Y[CCT];    	      	   //vetor produto [C] x [vel]
long double K_C_Y[CCT];                //[K_C_Y] = [fa] - [C_Y] - [K_Y]
long double acel[CCT];				   //vetor aceleração
long double f[NGL2]; 				   //vetor forçamento do corpo todo
long double cc[NGL2], cc_p[nELE];      //vetor condições de contorno
long double Aes[nELE], Aep[nELE];	   //vetor área transversal de todos os elementos
long double mEs[nELE], mEp[nELE];      //vetor módulo de elasticidade de todos os elementos
long double Mos[nELE], Mop[nELE];      //vetor momento de inércia de todos os elementos
long double y[21]; 					   //vetor variáveis
long double y_ant[21];                 //vetor com valores anteriores das variáveis
long double delta_y[21];               //[delta_y] = [y]i - [y]i-1

char abobrinhas[100];
FILE* output;			/* internal filename */
FILE* mola;
FILE* frequencia;
FILE* matrizes;

void LeituraDados(void)
{
	gl = GL / 2;
	nNs = nGmax;
	nEl = nNs - 1;
	for (i = 0; i < nNs; i++)				/* coordenadas dos nos */
	{
		for (j = 0; j < nDim; j++)
		{
			if (j == 0)
			{
				xG[i][j] = 0;
			}
			if (j == 1)
			{
				xG[i][j] = i * ls / nEl;
			}
		}
	}

	for (i = 0; i < nEl; i++)				/* coordenadas dos elementos */
	{
		for (j = 0; j < 2; j++)
		{
			nE[i][j] = i + j + 1;
		}
	}
	for (i = 0; i < nEl; i++)      /* matriz de identificacao do elemento */
	{
		for (j = 0; j < GL; j++)
		{
			MatEl[i][j] = 2 * i + j;
		}
	}


	printf("***  Solucao de Trelicas planas - MEF  ***");


	strcpy(arqIn, "dados_piezo.inp");
	strcpy(arqOut, "saida.dat");


	testaAberturaArqR(arqIn);		/*  abertura do arq. de Entrada */
	fgets(abobrinhas, 80, fptrIn);
	fscanf(fptrIn, "%d", &nbp);		/*	numero de vigas piezoeleétrica */
	fgets(abobrinhas, 80, fptrIn);
	fgets(abobrinhas, 80, fptrIn);
	fscanf(fptrIn, "%Lf %Lf %Lf\n", &e31, &k33);
	fgets(abobrinhas, 80, fptrIn);
	//fgets(abobrinhas,80,fptrIn);
	fscanf(fptrIn, "%Lf %Lf %Lf %Lf %Lf %Lf\n", &bs, &hs, &bp, &hp, &lp, &ls);
	fgets(abobrinhas, 80, fptrIn);
	//  fgets(abobrinhas,80,fptrIn);
	for (i = 0; i < nEl; i++)						/* Leitura do vetor cc do piezoletrico*/
	{
		fscanf(fptrIn, "%Lf", &cc_p[i]);
	}
	aes = bs * hs;
	for (i = 0; i < nEl; i++)      /*  AREA DA ESTRUTURA*/
	{
		Aes[i] = aes;
	}
	aep = bp * hp;
	for (i = 0; i < nEl; i++)      /* AREA DO PIEZO*/
	{
		if (cc_p[i] == 0)
		{
			Aep[i] = 0;
		}
		else
		{
			Aep[i] = nbp * aep;
		}
	}
	fgets(abobrinhas, 80, fptrIn);
	fgets(abobrinhas, 80, fptrIn);
	for (i = 0; i < nEl; i++)      /* modulo de elasticidade ESTRUTURA */
	{
		fscanf(fptrIn, "%d", &i_aux);
		fscanf(fptrIn, "%Lf", &mEs[i]);
	}
	fgets(abobrinhas, 80, fptrIn);
	fgets(abobrinhas, 80, fptrIn);
	for (i = 0; i < nEl; i++)      /* modulo de elasticidade PIEZO */
	{
		fscanf(fptrIn, "%d", &i_aux);
		fscanf(fptrIn, "%Lf", &mEp[i]);
	}
	for (i = 0; i < nEl; i++)      /* momento de inércia PIEZO */
	{
		if (cc_p[i] == 0)
		{
			Aep[i] = 0;
		}
		else
		{
			Aep[i] = nbp * aep;
		}
	}
	MoIp = nbp * (bp * pow(hp, 3) / 12.0 + bp * hp * pow((hp + hs) / 2, 2));

	for (i = 0; i < nEl; i++)      /* momento de inércia */
	{
		if (cc_p[i] == 0)
		{
			Mop[i] = 0;
		}
		else
		{
			Mop[i] = MoIp;
		}
	}
	/* momento de inércia ESTRUTURA*/
	//MoIs=bs*pow(hs,3)/12.0;
	MoIs = 8e-3;
	for (i = 0; i < nEl; i++)
	{
		Mos[i] = MoIs;
	}

	fgets(abobrinhas, 80, fptrIn);
	fgets(abobrinhas, 80, fptrIn);
	for (i = 0; i < (nEl + 1) * gl; i++)      /* forçamento */
	{
		fscanf(fptrIn, "%Lf", &f1[i]);
	}
	fgets(abobrinhas, 80, fptrIn);
	fgets(abobrinhas, 80, fptrIn);
	for (i = 0; i < ((nEl + 1) * gl + ((nEl + 1) * gl) / 2.0); i++)						/* Leitura do vetor cc */
	{
		fscanf(fptrIn, "%Lf", &cc[i]);
	}
	fgets(abobrinhas, 80, fptrIn);
	fgets(abobrinhas, 80, fptrIn);
	fscanf(fptrIn, "%Lf %d %d %d\n", &t0, &nm, &np, &nt_Transiente);
	/* nm = numero de divisoes por periodo
	   np = numero de periodos */
	fgets(abobrinhas, 80, fptrIn);
	fscanf(fptrIn, "%Lf %Lf %Lf\n", &Wi, &Wf, &dW);

	fgets(abobrinhas, 80, fptrIn);


	fclose(fptrIn);							/* Fechamento do Arquivo de leitura */
}
/*  ==================== Rotinas de execucao =====================*/

/************************************************/

void MatrizRigidez_estrututa(void)
{
	/* montagem da matriz global */
	for (i = 0; i < N; i++)
		for (j = 0; j < N; j++)
		{
			Ks[i][j] = 0;
		}

	for (n = 0; n < nEl; n++)
	{
		a = (12 * Mos[n] * mEs[n] / pow(ls, 3));
		b = (6 * Mos[n] * mEs[n] / pow(ls, 2));
		c = (4 * Mos[n] * mEs[n] / ls);
		d = (6 * Mos[n] * mEs[n] / ls);

		KEls[0][0] = a;
		KEls[0][1] = b;
		KEls[0][2] = -a;
		KEls[0][3] = b;
		KEls[1][0] = b;
		KEls[1][1] = c;
		KEls[1][2] = -b;
		KEls[1][3] = c / 2;
		KEls[2][0] = -a;
		KEls[2][1] = -b;
		KEls[2][2] = a;
		KEls[2][3] = -d;
		KEls[3][0] = b;
		KEls[3][1] = c / 2;
		KEls[3][2] = -d;
		KEls[3][3] = c;
		for (i = 2 * n; i < 2 * n + 4; i++)
		{
			for (j = 2 * n; j < 2 * n + 4; j++)
			{
				Ks[i][j] = Ks[i][j] + KEls[i - 2 * n][j - 2 * n];
			}
		}

	}
}
/***********************************************************/
void MatrizRigidez_piezo(void)
{
	/* montagem da matriz global */
	for (i = 0; i < N; i++)
		for (j = 0; j < N; j++)
		{
			Kp[i][j] = 0;
		}

	for (n = 0; n < nEl; n++)
	{

		a = (12 * Mop[n] * mEp[n] / pow(lp, 3));
		b = (6 * Mop[n] * mEp[n] / pow(lp, 2));
		c = (4 * Mop[n] * mEp[n] / lp);
		d = (6 * Mop[n] * mEp[n] / lp);

		if (cc_p[n] == 0)
		{
			for (i = 0; i < GL; i++)
			{
				for (j = 0; j < GL; j++)
				{
					KElp[i][j] = 0;
				}
			}
		}
		else
		{

			KElp[0][0] = a;
			KElp[0][1] = b;
			KElp[0][2] = -a;
			KElp[0][3] = b;
			KElp[1][0] = b;
			KElp[1][1] = c;
			KElp[1][2] = -b;
			KElp[1][3] = c / 2;
			KElp[2][0] = -a;
			KElp[2][1] = -b;
			KElp[2][2] = a;
			KElp[2][3] = -d;
			KElp[3][0] = b;
			KElp[3][1] = c / 2;
			KElp[3][2] = -d;
			KElp[3][3] = c;
		}

		for (i = 2 * n; i < 2 * n + 4; i++)
		{
			for (j = 2 * n; j < 2 * n + 4; j++)
			{
				Kp[i][j] = Kp[i][j] + KElp[i - 2 * n][j - 2 * n];
			}
		}
	}
}
/************************************************/
void MatrizMassa_estrutura(void)
{
	/* montagem da matriz massa*/
	for (i = 0; i < N; i++)
	{
		for (j = 0; j < N; j++)
		{
			Ms[i][j] = 0;
		}
	}


	for (n = 0; n < nEl; n++)
	{

		fe = 22 * ls * ((Aes[n] * dens * ls) / 420);
		ef = 13 * ls * ((Aes[n] * dens * ls) / 420);
		g = 3 * ls * ls * ((Aes[n] * dens * ls) / 420);
		h1 = 156 * ((Aes[n] * dens * ls) / 420);
		nh = 54 * ((Aes[n] * dens * ls) / 420);
		mn = 4 * ls * ls * ((Aes[n] * dens * ls) / 420);


		Mls[0][0] = h1;
		Mls[0][1] = fe;
		Mls[0][2] = nh;
		Mls[0][3] = -ef;
		Mls[1][0] = fe;
		Mls[1][1] = mn;
		Mls[1][2] = ef;
		Mls[1][3] = -g;
		Mls[2][0] = nh;
		Mls[2][1] = ef;
		Mls[2][2] = h1;
		Mls[2][3] = -fe;
		Mls[3][0] = -ef;
		Mls[3][1] = -g;
		Mls[3][2] = -fe;
		Mls[3][3] = mn;

		for (i = 2 * n; i < 2 * n + 4; i++)
		{
			for (j = 2 * n; j < 2 * n + 4; j++)
			{
				Ms[i][j] = Ms[i][j] + Mls[i - 2 * n][j - 2 * n];
			}
		}

	}
}

/************************************************************/
void MatrizMassa_piezo(void)
{
	/* montagem da matriz massa*/
	for (i = 0; i < N; i++)
		for (j = 0; j < N; j++)
		{
			Mp[i][j] = 0;
		}

	for (n = 0; n < nEl; n++)
	{
		fe = 22 * lp * ((Aep[n] * dens * lp) / 420);
		ef = 13 * lp * ((Aep[n] * dens * lp) / 420);
		g = 3 * lp * lp * ((Aep[n] * dens * lp) / 420);
		h1 = 156 * ((Aep[n] * dens * lp) / 420);
		nh = 54 * ((Aep[n] * dens * lp) / 420);
		mn = 4 * lp * lp * ((Aep[n] * dens * lp) / 420);

		if (cc_p[n] == 0)
		{
			for (i = 0; i < GL; i++)
			{
				for (j = 0; j < GL; j++)
				{
					Mlp[i][j] = 0;
				}
			}
		}
		else
		{
			Mlp[0][0] = h1;
			Mlp[0][1] = fe;
			Mlp[0][2] = nh;
			Mlp[0][3] = -ef;
			Mlp[1][0] = fe;
			Mlp[1][1] = mn;
			Mlp[1][2] = ef;
			Mlp[1][3] = -g;
			Mlp[2][0] = nh;
			Mlp[2][1] = ef;
			Mlp[2][2] = h1;
			Mlp[2][3] = -fe;
			Mlp[3][0] = -ef;
			Mlp[3][1] = -g;
			Mlp[3][2] = -fe;
			Mlp[3][3] = mn;
		}
		for (i = 2 * n; i < 2 * n + 4; i++)
		{
			for (j = 2 * n; j < 2 * n + 4; j++)
			{
				Mp[i][j] = Mp[i][j] + Mlp[i - 2 * n][j - 2 * n];
			}
		}
	}
}
/************************************************************/

/************************************************************/
void soma_rigidez_massa(void)
{
	for (i = 0; i < N; i++)
	{
		for (j = 0; j < N; j++)
		{
			K[i][j] = Ks[i][j] + Kp[i][j];
			M[i][j] = Ms[i][j] + Mp[i][j];
		}
	}
}
/************************************************************/

/*********************************************************/
void Matriz_acoplamento_eletromecanico_U_V(void)
{
	for (i = 0; i < N; i++)
	{
		for (j = 0; j < N / 2.0; j++)
		{
			K_u_v[i][j] = 0;
		}
	}
	for (n = 0; n < nEl; n++)
	{
		u1 = -e31 * bp * lp * (2 * hs * hp + hp * hp) / 2.0;
		if (cc_p[n] == 0)
		{
			for (i = 0; i < N; i++)
			{
				for (j = 0; j < N / 2.0; j++)
				{
					K_u_vp[i][j] = 0;
				}
			}
		}
		else
		{

			K_u_vp[0][0] = 0;
			K_u_vp[0][1] = -u1 * lp;
			K_u_vp[0][2] = 0;
			K_u_vp[0][3] = u1 * lp;
			K_u_vp[1][0] = 0;
			K_u_vp[1][1] = u1 * lp;
			K_u_vp[1][2] = u1 * lp;
			K_u_vp[1][3] = -u1 * lp;
		}


		for (i = 2 * n; i < 2 * n + 4; i++)
		{
			for (j = n; j < n + 2; j++)
			{
				K_u_v[i][j] = K_u_v[i][j] + K_u_vp[i - 2 * n][j - n];
			}
		}


	}

}
/***********************************************************/

void Matriz_acoplamento_eletromecanico_V_U(void)
{
	for (i = 0; i < N; i++)
	{
		for (j = 0; j < N / 2.0; j++)
		{
			K_v_u[j][i] = K_u_v[i][j];
		}
	}

}
/*********************************************************/
void Matriz_acoplamento_eletromecanico_V_V(void)
{
	for (i = 0; i < N / 2.0; i++)
	{
		for (j = 0; j < N / 2; j++)
		{
			K_v_v[i][j] = 0.0;
		}
	}

	for (n = 0; n < nEl; n++)
	{
		u1 = -k33 * Aep[n] * lp / (hp * hp);

		if (cc_p[n] == 0)
		{
			for (i = 0; i < N / 2.0; i++)
			{
				for (j = 0; j < N / 2.0; j++)
				{
					K_v_vp[i][j] = 0;
				}
			}
		}
		else
		{

			K_v_vp[0][0] = u1;
			K_v_vp[0][1] = -u1;
			K_v_vp[1][0] = -u1;
			K_v_vp[1][1] = u1;


		}


		for (i = n; i < n + 2; i++)
		{
			for (j = n; j < n + 2; j++)
			{
				K_v_v[i][j] = K_v_v[i][j] + K_v_vp[i - n][j - n];
			}
		}
	}
}
/***********************************************************/
/*********************************************************/
void Matriz_M_geral(void)
{
	for (i = 0; i < (N + N / 2); i++)
	{
		for (j = 0; j < (N + N / 2); j++)
		{
			M_G[i][j] = 0.0;
		}
	}

	for (i = 0; i < (N + N / 2); i++)
	{
		for (j = 0; j < (N + N / 2); j++)
		{
			if (i < N && j < N)
			{
				M_G[i][j] = M[i][j];
			}
		}
	}
}


/********************************************************/
void Matriz_K_geral(void)
{
	for (i = 0; i < (N + N / 2.0); i++)
	{
		for (j = 0; j < (N + N / 2.0); j++)
		{
			K_G[i][j] = 0.0;
		}
	}

	for (i = 0; i < (N + N / 2.0); i++)
	{
		for (j = 0; j < (N + N / 2.0); j++)
		{
			if (i < N && j < N)
			{
				K_G[i][j] = K[i][j];
			}
			if (i<N && j>N)
			{
				K_G[i][j] = K_u_v[i][j - (N)];
			}
			if (i > (N) && j < (N))
			{
				K_G[i][j] = K_v_u[i - (N)][j];
			}
			if (i > (N) && j > (N))
			{
				K_G[i][j] = K_v_v[i - (N)][j - (N)];
			}
		}
	}
}


/*********************************************************/
/************************************************************************/
void MatrizC(void)
{
	/* montagem da matriz massa*/
	for (i = 0; i < N + N / 2.0; i++)
	{
		for (j = 0; j < N + N / 2.0; j++)
		{
			C[i][j] = 0.0005 * M_G[i][j] + 0.0005 * K_G[i][j];
		}
	}
}
/*********************************************************/
void CondicoesContorno(void)
{
	for (i = 0; i < N + N / 2.0; i++)
	{
		if (cc[i] == 0)
		{
			for (j = 0; j < N + N / 2.0; j++)
				if (j != i || j == i)
				{
					K_G[i][j] = 0;
					M_G[i][j] = 0;
					C[i][j] = 0;
				}
			for (k = 0; k < N + N / 2.0; k++)
				if (k != i || k == i)
				{
					K_G[k][i] = 0;
					M_G[k][i] = 0;
					C[k][j] = 0;
				}
		}
	}

	algo = 0;
	for (i = 0; i < N + N / 2.0; i++)
	{
		cont1 = 0;
		for (j = 0; j < N + N / 2.0; j++)
		{
			if (cc[i] != 0 && cc[j] != 0)
			{
				M_cc[cont1][algo] = M_G[j][i];
				K_cc[cont1][algo] = K_G[j][i];
				C_cc[cont1][algo] = C[j][i];
				cont1 = cont1 + 1;
			}
		}
		if (cc[i] != 0)
		{
			algo = algo + 1;
		}
	}

}

/*********************************************************/

void CondicoesContorno1(void)
{
	cont1 = 0;

	for (i = 0; i < N + N / 2.0; i++)      //preenchendo a última linha da nova matriz até antes da penúltima coluna
	{
		if (cc[i] != 0)
		{
			fa[cont1] = f[i];
			cont1 = cont1 + 1;
		}

	}

}
/*********************************************************/
void MatrizP1(void)
{
	/* montagem da matriz massa*/
	for (i = 0; i < N2; i++)
	{
		for (j = 0; j < N2; j++)
		{
			P1[i][j] = a0 * M_cc[i][j] + a1 * C_cc[i][j] + K_cc[i][j];
		}
	}
}
/*********************************************************/
void MatrizP2a(void)
{
	int i, j;
	long double soma, soma1;

	for (i = 0; i < N2; i++)
	{

		for (j = 0; j < N2; j++)
		{
			P2a[i][j] = a0 * M_cc[i][j] + a1 * C_cc[i][j];

		}

	}

}
/***************************************************/
void MatrizP3a(void)
{
	int i, j;
	long double soma, soma1;

	for (i = 0; i < N2; i++)
	{

		for (j = 0; j < N2; j++)
		{
			P3a[i][j] = a2 * M_cc[i][j] + a4 * C[i][j];

		}

	}

}
/***************************************************/
void MatrizP4a(void)
{
	int i, j;
	long double soma;

	for (i = 0; i < N2; i++)
	{

		for (j = 0; j < N2; j++)
		{
			P4a[i][j] = a3 * M_cc[i][j] + a5 * C[i][j];

		}

	}

}
/******************************************************/
void MatrizP2(void)
{
	int i, j;
	long double soma;

	for (i = 0; i < N2; i++)
	{
		soma = 0.;
		for (j = 0; j < N2; j++)
		{
			soma = soma + P2a[i][j] * y_ant[j];
		}
		P2[i] = soma;
	}

}

/***********************************************************************/


/***********************************************************************/
void MatrizP3(void)
{
	int i, j;
	long double soma;

	for (i = 0; i < N2; i++)
	{
		soma = 0.;
		for (j = 0; j < N2; j++)
		{
			soma = soma + P3a[i][j] * y_ant[j + (N2)];
		}
		P3[i] = soma;
	}

}

/***************************************************/
void MatrizP4(void)
{
	int i, j;
	long double soma;

	for (i = 0; i < N2; i++)
	{
		soma = 0.;
		for (j = 0; j < N2; j++)
		{
			soma = soma + P4a[i][j] * y_ant[j + 2 * (N2)];
		}
		P4[i] = soma;
	}

}




/***********************************************************************/
void MatrizP5(void)
{
	/* montagem da matriz massa*/
	for (i = 0; i < N2; i++)
	{

		P6[i] = fa[i] + P2[i] + P3[i] + P4[i];

	}
}

/***********************************************************************/


/********************************************************/
void Elim_Gauss(void)
{
	int ii, jj, kk;
	long double pivot, coef, aux;


	/*=====  Triangularizacao (triangular superior)  =====*/

	for (ii = 0; ii < (N2 - 1); ii++)
	{
		pivot = P1[ii][ii];
		for (kk = (ii + 1); kk < (N2); kk++)
		{
			coef = P1[kk][ii] / pivot;
			for (jj = 0; jj < (N2); jj++)
			{
				P1[kk][jj] = P1[kk][jj] - (coef * P1[ii][jj]);
			}
			P6[kk] = P6[kk] - (P6[ii] * coef);
		}
	}


	/*=====  Retrosubstituicao  =====*/

	for (ii = (N2)-1; ii >= 0; ii--)
	{
		aux = 0;
		for (jj = ii + 1; jj < (N2); jj++)
		{
			aux = aux + P1[ii][jj] * P7[jj];
		}
		P7[ii] = (P6[ii] - aux) / P1[ii][ii];
	}
}

/******************************************/
void solucao(void)
{
	long double z;
	int i, jn, jp, cont1;

	z = 0;
	cont1 = 0;
	for (jp = 1; jp <= np - 1; jp++)
	{
		for (jn = 1; jn <= nm - 1; jn++)			/* time loop */
		{
			for (i = 0; i < N; i++)
			{
				f[i] = f[i] * sin(W * t);
			}

			CondicoesContorno1();

			// if(jp> nt_Transiente)
			if (jp > 0)
			{
				if (sqrt((W - 0.5) * (W - 0.5)) < 0.0001)
				{
					fprintf(output, "%Lf\t %Lf\t %Lf\t %Lf\t %Lf\t %Lf\t %Lf\t %Lf\n", W, t);
					for (i = 0; i < N; i++)
					{
						fprintf(output, "%Lf\f, ")
					}
					, y[0], y[1], y[2], y[3], y[4], y[5]);
				}
			}

			MatrizP2();
			MatrizP3();
			MatrizP4();
			MatrizP5();
			Elim_Gauss();

			for (i = 0; i < N2; i++)
			{
				y[i] = P7[i];
			}

			for (i = 0; i < N2; i++)
			{
				y[i + 2 * N2] = a0 * (y[i] - y_ant[i]) - a2 * y_ant[i + N2] - a3 * y_ant[i + 2 * N2];
			}

			for (i = 0; i < N2; i++)
			{
				y[i + N2] = y_ant[i + N2] + a6 * y_ant[i + 2 * N2] + a7 * y[i + 2 * N2];
			}



			t = t + dt;

			for (i = 0; i < 3 * N2; i++)
			{
				y_ant[i] = y[i];		 /* initial position */
			}

			for (i = 0; i < N2; i++)
			{
				fa_ant[i] = fa[i];		 /* initial position */
			}

		}
	}

	fprintf(frequencia, "%Lg\t %Lg\n", W, z);
}
/*******************************************/
/* Programa Principal */

main()
{
	int i, jb, jn, jp;
	PI = 4 * atan(1);            /*numero PI*/
	alfa = 1.0 / 4.0;
	gama = 1.0 / 2.0;
	dens = 7850;
	LeituraDados();

	output = fopen("LINEAR.dat", "w");			/* external filename */
	mola = fopen("LINEAR1.dat", "w");			/* external filename */
	frequencia = fopen("freq.dat", "w");
	matrizes = fopen("matrizes.txt", "w");

	cont = 0;
	N = (nEl + 1) * gl; 			//Por que o 2* se no fu não é?
	for (i = 0; i < (N + N / 2.0); i++)
	{
		if (cc[i] == 0)
		{
			cont = cont + 1;
		}
	}
	//N1 = 3 * ((N + N / 2.0) - cont);
	N2 = ((N + N / 2.0) - cont);



	//Matrizes cujos valores são constantes no tempo
	MatrizRigidez_estrututa();
	MatrizRigidez_piezo();
	MatrizMassa_estrutura();
	MatrizMassa_piezo();
	soma_rigidez_massa();
	Matriz_acoplamento_eletromecanico_U_V();
	Matriz_acoplamento_eletromecanico_V_U();
	Matriz_acoplamento_eletromecanico_V_V();
	Matriz_M_geral();
	Matriz_K_geral();
	MatrizC();

	CondicoesContorno();


	nbif = (int)((Wf - Wi) / dW) + 1 + TOL;

	for (jb = 0; jb <= nbif - 1; jb++)
	{
		W = Wi + (jb * dW);
		printf("W = %g\n", W);
		if (W != 0) dt = (long double)((PI * 2. / W) / nm);
		else      dt = (long double)((PI * 2.) / nm);


		if (t == 0)
		{
			for (i = 0; i < 3 * ((N + N / 2.0) - cont); i++)
			{
				y[i] = y_ant[i] = 0.0;				/* initial position */
			}
		}

		a0 = 1.0 / (alfa * dt * dt);
		a1 = gama / (alfa * dt);
		a2 = 1.0 / (alfa * dt);
		a3 = gama / (alfa);
		a4 = 1 / (2 * alfa);
		a5 = dt * ((gama / (2 * alfa)) - 1);
		a6 = dt * (1 - gama);
		a7 = gama * dt;

		MatrizP1();
		MatrizP2a();
		MatrizP3a();
		MatrizP4a();
		solucao();
	}

	fclose(mola);
	fclose(output);
	fclose(frequencia);
	fclose(matrizes);

}





