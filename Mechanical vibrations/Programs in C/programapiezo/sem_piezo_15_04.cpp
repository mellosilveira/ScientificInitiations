/* Runge Kutta for a set of first order differential equations */
/* Este programa resolve as equacoes de estado do Sistema Dinamico*/
/* Massa Mola Amortecedor com rigidez descontínua e escreve os resultados*/
/* num arquivo no seguinte formato: tempo posicao velocidade*/

#include <stdio.h>
#include <io.h>
#include <conio.h>
#include <string.h>
#include <math.h>
//#include <alloc.h>
#include <stdlib.h>

//#include "alloc.h"
#include "DefVar19_09.h"				//ALTERAR DEFVAR2.H PARA DEFVAR.H
//#include "MatLib.c"
#include "Test_Arq.c"

//#define N 2*gl			/* number of first order equations */
//#define dist 1e-2 /* stepsize in t*/
#define dist1 1e-1 /* stepsize in w*/
#define MAX 30.0		/* max for t */
#define MAX1 30.0		/* max for w */
#define cont2 1.0
#define TOL 1e-10

#define TOL 1e-2
#define NITER 1000

#define  nGmax  	3    // numero de nós
#define  nDim		2    // número de dimensões
#define  nELE		2    // número de elementos
#define  NGL		6    // número de graus de liberdade máximo sem o grau de liberdade do potencial
#define  NGL1		3    // número de graus de liberdade máximo dividido por 2
#define  NGL2		6    // número de graus de liberdade máximo+número de graus de liberdade máximo dividido por 2
#define 	GL			4	  // número de graus de liberdade por elemento
#define 	GL1	  	2	  // número de graus de liberdade por elemento dividido por 2
#define  GLS		4    // número de graus de liberdade menos as condições de contorno igual a zero

#define  CCT    	4    // número de grau de liberdade desprezando as condições de contorno
int i, j, k;							//auxiliares para função for
int nNs; 							//número de nós
int nEl;                      //número de elementos
int n;                        //auxilar para função for (M e K)
int p, q, r, s;                  //auxiliares para montar as matrizes M e K completas
int z;                        //auxiliar da função principal para dados de saída
int i_aux;                 //auxiliar para leitura dos dados de entrada                    //densidade
int gl;                       //número de graus de liberdade
int N, N2;                        //N= 2*(nEl+1)*gl
int cont;							//contar condições de contorno iguais a zero
int cont1;
int algo;
int nm, np, nt_Transiente;
int nbif;
int key, kont;
long double sd;
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
long double t_v1[100000];
long double F_v1[100000];
long double t_v2[100000];
long double F_v2[100000];
long double t_v3[100000];
long double F_v3[100000];
long double t_v4[100000];
long double F_v4[100000];
long double t_v5[100000];
long double F_v5[100000];
long double t_v6[100000];
long double F_v6[100000];

//matriz massa estrutura para um elemento
long double Mls[GL][GL]; //matriz massa 4x4 de 1 elemento

//matriz massa estrutura para n elemento, onde NGL é o numero máximo de grau de liberdades sem considerar os graus e liberdade dos potenciais.
long double Ms[NGL][NGL];

//matriz rigidez estrutura para um elemento
long double KEls[GL][GL]; //matriz massa 4x4 de 1 elemento

//matriz rigidez estrutura para n elemento, onde NGL é o numero máximo de grau de liberdades sem considerar os graus e liberdade dos potenciais.
long double Ks[NGL][NGL];



//soma da  massa da estrutura com matriz massa do piezo, onde NGL é o numero máximo de grau de liberdades sem considerar os graus e liberdade dos potenciais.
long double M[NGL][NGL];

//soma da  rigidez da estrutura com matriz rigidez do piezo, onde NGL é o numero máximo de grau de liberdades sem considerar os graus e liberdade dos potenciais.
long double K[NGL][NGL];

//Formação da Matriz massa incluindo os graus de liberdade relacionados com os potenciais, onde NGL2=NGL2 +NGL2/2.
long double M_G[NGL2][NGL2];

//Formação da Matriz RIGIDEZ incluindo os graus de liberdade relacionados com os potenciais, onde NGL2=NGL2 +NGL2/2.
long double K_G[NGL2][NGL2];


//Formação da Matriz AMORTECIMENTO incluindo os graus de liberdade relacionados com os potenciais, onde NGL2=NGL2 +NGL2/2.
long double C[NGL2][NGL2];


//Formação da Matriz massa incluindo os graus de liberdade relacionados com os potenciais, e diminuindo o numero e condições de contorno.
long double M_cc[CCT][CCT];

//Formação da Matriz RIGIDEZ incluindo os graus de liberdade relacionados com os potenciais, e diminuindo o numero e condições de contorno.
long double K_cc[CCT][CCT];

//Formação da Matriz AMORTECIMENTO incluindo os graus de liberdade relacionados com os potenciais,e diminuindo o numero e condições de contorno.
long double C_cc[CCT][CCT];



long double xG[nGmax][nELE];           //matriz 3x2 coordenadas dos nós

long double y_equivalente[CCT];

long double P1a[CCT][CCT];
long double P2a[CCT][CCT];
long double P3a[CCT][CCT];
long double P4a[CCT][CCT];
long double PE[CCT][CCT];              //[P1] = a0*[M] + a1*[C] + [K];

//long double P1[3][3]={{1,2,3},{2,-3,2},{3,1,-1}};
//long double P6[3]={{6},{14},{-2}};

int nE[nELE][2];              			//matriz 2x2 coordenadas dos elementos
int MatEl[nELE][GL];							//matriz 2x4 identificação dos elementos

long double fa[CCT];                   //vetor forçamento [4] aplicado as condições de contorno
long double P1[CCT];                   //[P2] = [M] x (a0*[y] + a2*[vel] + a3*[acel])
long double P2[CCT];
long double P3[CCT];                   //[P3] = [C]*(a1*[y] + a4*[vel] + a5*[acel])
long double P4[CCT];                   //[P4] = [fa]*sin(W*t) + [P2] + [P3]
long double P5[CCT], P6a[CCT], P6[CCT];
long double P7[CCT];                   //vetor delta y para calcular o resultado
long double P7_ant[CCT];               //valor anterior do vetor P7
long double fa_ant[CCT];               //vetor forçamento 4x4 no instante anterior
long double delta_fa[CCT];             //[delta_fa] = [fa]i - [fa]i-1
long double P1inv[CCT][CCT];
long double K_Y[CCT];      		      //vetor produto [K] x [y]
long double C_Y[CCT];    	      	   //vetor produto [C] x [vel]
long double K_M_Y[CCT];             	//[K_C_Y] = [fa] - [C_Y] - [K_Y]

long double f[NGL2]; 							//vetor forçamento do corpo todo
long double cc[NGL2], cc_p[nELE];       //vetor condições de contorno
long double Aes[nELE], Aep[nELE];		   //vetor área transversal de todos os elementos
long double mEs[nELE], mEp[nELE];       //vetor módulo de elasticidade de todos os elementos
long double Mos[nELE], Mop[nELE];       //vetor momento de inércia de todos os elementos
							//vetor variáveis
long double y_ant[12];                 //vetor com valores anteriores das variáveis
long double delta_y[4];               //[delta_y] = [y]i - [y]i-1
long double delta_vel[4];
long double delta_acel[4];
long double desl[4]; 							//vetor variáveis
long double desl_ant[4];                 //vetor com valores anteriores das variáveis
long double vel[4];
long double vel_ant[4]; 							//vetor variáveis
long double acel[4];                 //vetor com valores anteriores das variáveis
long double acel_ant[4];
elementos el;
propElemTrelica propETP;

stringArq arqIn, arqOut;
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
	for (i = 0; i < ((nEl + 1) * gl); i++)						/* Leitura do vetor cc */
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

/***********************************************************************/
void MatrizRigidez_estrutura(void)
{
	/* montagem da matriz global */
	for (int i = 0; i < N; i++)
	{
		for (int j = 0; j < N; j++)
		{
			Ks[i][j] = 0;
		}
	}

	for (int n = 0; n < nEl; n++)
	{
		long double constant = Mos[n] * mEs[n] / pow(ls, 3);

		KEls[0][0] = 12 * constant;
		KEls[0][1] = 6 * ls * constant;
		KEls[0][2] = -12 * constant;
		KEls[0][3] = 6 * ls * constant;
		KEls[1][0] = 6 * ls * constant;
		KEls[1][1] = 4 * pow(ls, 2) * constant;
		KEls[1][2] = -(6 * ls * constant);
		KEls[1][3] = 2 * pow(ls, 2) * constant;
		KEls[2][0] = -(12 * constant);
		KEls[2][1] = -(6 * ls * constant);
		KEls[2][2] = 12 * constant;
		KEls[2][3] = -(6 * ls * constant);
		KEls[3][0] = 6 * ls * constant;
		KEls[3][1] = 2 * pow(ls, 2) * constant;
		KEls[3][2] = -(6 * ls * constant);
		KEls[3][3] = 4 * pow(ls, 2) * constant;

		for (int i = 2 * n; i < 2 * n + 4; i++)
		{
			for (int j = 2 * n; j < 2 * n + 4; j++)
			{
				Ks[i][j] += KEls[i - 2 * n][j - 2 * n];
			}
		}
	}
}
/***********************************************************************/

/***********************************************************************/
void MatrizMassa_estrutura(void)
{
	/* montagem da matriz massa*/
	for (int i = 0; i < N; i++)
	{
		for (int j = 0; j < N; j++)
		{
			Ms[i][j] = 0;
		}
	}

	for (n = 0; n < nEl; n++)
	{
		long double constant = Aes[n] * dens * ls / 420;

		Mls[0][0] = 156 * constant;
		Mls[0][1] = 22 * ls * constant;
		Mls[0][2] = 54 * constant;
		Mls[0][3] = -13 * ls * constant;
		Mls[1][0] = 22 * ls * constant;
		Mls[1][1] = 4 * pow(ls, 2) * constant;
		Mls[1][2] = 13 * ls * constant;
		Mls[1][3] = -3 * pow(ls, 2) * constant;
		Mls[2][0] = 54 * constant;
		Mls[2][1] = 13 * ls * constant;
		Mls[2][2] = 156 * constant;
		Mls[2][3] = -22 * ls * constant;
		Mls[3][0] = -13 * ls * constant;
		Mls[3][1] = -3 * pow(ls, 2) * constant;
		Mls[3][2] = -22 * ls * constant;
		Mls[3][3] = 4 * pow(ls, 2) * constant;

		for (int i = 2 * n; i < 2 * n + 4; i++)
		{
			for (int j = 2 * n; j < 2 * n + 4; j++)
			{
				Ms[i][j] += Mls[i - 2 * n][j - 2 * n];
			}
		}
	}
}
/***********************************************************************/

/***********************************************************************/
void soma_rigidez_massa(void)
{
	for (int i = 0; i < N; i++)
	{
		for (int j = 0; j < N; j++)
		{
			K[i][j] = Ks[i][j];
			M[i][j] = Ms[i][j];
		}
	}
}

/***********************************************************************/
void Matriz_M_geral(void)
{
	for (int i = 0; i < (N); i++)
	{
		for (int j = 0; j < (N); j++)
		{

			M_G[i][j] = M[i][j];

		}
	}
}
/***********************************************************************/
void Matriz_K_geral(void)
{
	for (int i = 0; i < (N); i++)
	{
		for (int j = 0; j < (N); j++)
		{
			K_G[i][j] = K[i][j];
		}
	}
}
/***********************************************************************/
void MatrizC(void)
{
	/* montagem da matriz massa*/
	for (int i = 0; i < N; i++)
	{
		for (int j = 0; j < N; j++)
		{
			C[i][j] = 0.0005 * M_G[i][j] + 0.0005 * K_G[i][j];
		}
	}
}
/***********************************************************************/
void CondicoesContorno(void)
{
	for (int i = 0; i < N; i++)
	{
		if (cc[i] == 0)
		{
			for (int j = 0; j < N; j++)
			{
				if (j != i || j == i)
				{
					K_G[i][j] = 0;
					M_G[i][j] = 0;
					C[i][j] = 0;
				}
			}
			for (int k = 0; k < N; k++)
			{
				if (k != i || k == i)
				{
					K_G[k][i] = 0;
					M_G[k][i] = 0;
					C[k][j] = 0;
				}
			}
		}
	}

	int count1 = 0, count2 = 0;

	for (int i = 0; i < N; i++)
	{
		count2 = 0;

		if (cc[i] == true)
		{
			for (int j = 0; j < N; j++)
			{
				if (cc[j] == true)
				{
					M_cc[count1][count2] = M_G[j][i];
					K_cc[count1][count2] = K_G[j][i];
					C_cc[count1][count2] = C[j][i];

					count2 = count2 + 1;
				}
			}

			count1 += 1;
		}
	}
}
/***********************************************************************/
void CondicoesContorno1(void)
{
	int count1 = 0;

	for (i = 0; i < N; i++)
	{
		if (cc[i] == true)
		{
			fa[count1] = f[i];
			count1 += 1;
		}
	}
}
/***********************************************************************/
void RigidezEquivalente(void)
{
	for (int i = 0; i < N2; i++)
	{
		for (int j = 0; j < N2; j++)
		{
			PE[i][j] = a0 * M_cc[i][j] + a1 * C_cc[i][j] + K_cc[i][j];
		}
	}
}
/***********************************************************************/
void MatrizP1a(void)
{
	int i, j;
	long double soma, soma1;

	for (i = 0; i < N2; i++)
	{
		for (j = 0; j < N2; j++)
		{
			P1a[i][j] = a2 * M_cc[i][j] + a3 * C_cc[i][j];
		}
	}
}
/***********************************************************************/
void MatrizP2a(void)
{
	int i, j;
	long double soma, soma1;

	for (i = 0; i < N2; i++)
	{
		for (j = 0; j < N2; j++)
		{
			P2a[i][j] = a4 * M_cc[i][j] + a5 * C_cc[i][j];
		}
	}
}
/***********************************************************************/
void MatrizP4a(void)
{
	int i, j;
	long double soma;

	for (i = 0; i < N2; i++)
	{
		for (j = 0; j < N2; j++)
		{
			P4a[i][j] = a3 * M_cc[i][j] + a5 * C_cc[i][j];
		}
	}
}
/***********************************************************************/
void MatrizP1(void)
{
	long double soma;

	for (int i = 0; i < N2; i++)
	{
		soma = 0;

		for (int j = 0; j < N2; j++)
		{
			soma += P1a[i][j] * vel_ant[j];
		}

		P1[i] = soma;
	}
}
/***********************************************************************/
void MatrizP2(void)
{
	long double soma;

	for (int i = 0; i < N2; i++)
	{
		soma = 0;

		for (int j = 0; j < N2; j++)
		{
			soma += P2a[i][j] * acel_ant[j];
		}

		P2[i] = soma;
	}
}
/***********************************************************************/
void MatrizP4(void)
{
	long double soma;

	for (int i = 0; i < N2; i++)
	{
		soma = 0;

		for (int j = 0; j < N2; j++)
		{
			soma += P4a[i][j] * y_ant[j + 2 * (N2)];
		}

		P4[i] = soma;
	}
}
/***********************************************************************/
void MatrizP5(void)
{
	for (int i = 0; i < N2; i++)
	{
		P5[i] = (fa[i] - fa_ant[i]) + P1[i] + P2[i];
	}
}
/***********************************************************************/
void Elim_Gauss1(void)
{

	long double pivot, p, aux;
	int i, j, k, l;

	for (int i = 0; i < N2; i++)
	{
		for (int j = 0; j < N2; j++)
		{
			if (i == j)
			{
				P1inv[i][j] = 1;
			}
			else
			{
				P1inv[i][j] = 0;
			}
		}
	}

	// Triangularization
	for (int i = 0; i < N2; i++)
	{
		pivot = PE[i][i];

		for (int l = 0; l < N2; l++)
		{
			PE[i][l] = PE[i][l] / pivot;
			P1inv[i][l] = P1inv[i][l] / pivot;
		}

		for (int k = i + 1; k < N2; k++)
		{
			p = PE[k][i];

			for (int j = 0; j < N2; j++)
			{
				PE[k][j] = PE[k][j] - (p * PE[i][j]);
				P1inv[k][j] = P1inv[k][j] - (p * P1inv[i][j]);
			}
		}
	}

	// Retrosubstitution
	for (int i = N2 - 1; i >= 0; i--)
	{
		for (int k = i - 1; k >= 0; k--)
		{
			p = PE[k][i];

			for (int j = N2 - 1; j >= 0; j--)
			{
				PE[k][j] = PE[k][j] - (p * PE[i][j]);
				P1inv[k][j] = P1inv[k][j] - (p * P1inv[i][j]);
			}
		}
	}

	for (int i = 0; i < N2; i++)
	{
		for (int j = 0; j < N2; j++)
		{
			aux += P1inv[i][j] * P5[j];
		}

		y_equivalente[i] = aux;
	}
}
/***********************************************************************/
void Elim_Gauss(void)
{
	int ii, jj, kk;
	long double pivot, coef, aux;


	/*=====  Triangularizacao (triangular superior)  =====*/

	for (ii = 0; ii < (N2 - 1); ii++)
	{
		pivot = PE[ii][ii];
		for (kk = (ii + 1); kk < (N2); kk++)
		{
			coef = PE[kk][ii] / pivot;
			for (jj = 0; jj < (N2); jj++)
			{
				PE[kk][jj] = PE[kk][jj] - (coef * PE[ii][jj]);
			}
			P5[kk] = P5[kk] - (P5[ii] * coef);
			if (abs(P5[kk]) < 1e-10)
			{
				P5[kk] = 0;
			}

		}
	}


	/*=====  Retrosubstituicao  =====*/

	for (ii = (N2)-1; ii >= 0; ii--)
	{
		aux = 0;
		for (jj = ii + 1; jj < (N2); jj++)
		{
			aux = aux + PE[ii][jj] * P7[jj];
		}
		P7[ii] = (P5[ii] - aux) / PE[ii][ii];
	}
}

/**************************************************************/
void solucao(void)
{
	long double z = 0;
	int cont1 = 0;

	for (int jp = 0; jp < np; jp++)
	{
		for (int jn = 0; jn < nm; jn++)
		{
			for (int i = 0; i < N; i++)
			{
				f[i] = f1[i] * sin(W * t);
			}

			if (jp == 1 && jn == 32)
			{
				sd = 0;
			}
			CondicoesContorno1();

			if (jp > 10)
			{
				if (W == 0.5)
				{
					fprintf(output, "%Lf\t %Lf\t %Lf\t %Lf\t %Lf\t %Lf\t %Lf\t %Lf\t %Lf\t %Lf\t %Lf\t %Lf\t %Lf\t %Lf\t", W, t, desl[0], desl[1], desl[2], desl[3], vel[0], vel[1], vel[2], vel[3], acel[0], acel[1], acel[2], acel[3]);

					fprintf(output, "\n");
				}
			}
			RigidezEquivalente();
			MatrizP1();
			MatrizP2();
			MatrizP5();
			Elim_Gauss();

			for (i = 0; i < (N - cont); i++)
			{
				delta_y[i] = P7[i];
			}


			for (i = 0; i < (N - cont); i++)
			{
				delta_acel[i] = a0 * (delta_y[i]) - a2 * vel_ant[i] - a4 * acel_ant[i];
			}

			for (i = 0; i < (N - cont); i++)
			{
				delta_vel[i] = a1 * (delta_y[i]) - a3 * vel_ant[i] - a5 * acel_ant[i];
			}

			for (i = 0; i <= (N - cont); i++)
			{
				desl[i] = desl_ant[i] + delta_y[i];
				vel[i] = vel_ant[i] + delta_vel[i];
				acel[i] = acel_ant[i] + delta_acel[i];
			}



			t = t + dt;

			if (t > 16.58)
			{
				sd = 0;
			}

			for (i = 0; i < N2; i++)
			{
				desl_ant[i] = desl[i];
				vel_ant[i] = vel[i];
				acel_ant[i] = acel[i];
				fa_ant[i] = fa[i];
			}


		}
	}

	fprintf(frequencia, "%Lg\t %Lg\n", W, z);
}

/***********************************************************************/
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
	frequencia = fopen("freq.dat", "w");

	cont = 0;
	N = (nEl + 1) * gl;

	for (i = 0; i < (N); i++)
	{
		if (cc[i] == 0)
		{
			cont += 1;
		}
	}

	N2 = ((N)-cont);

	int nbif = (int)((Wf - Wi) / dW) + 1;

	for (jb = 0; jb < nbif; jb++)
	{
		W = Wi + (jb * dW);
		printf("W = %g\n", W);

		if (W != 0)
		{
			dt = (long double)((PI * 2 / W) / nm);
		}
		else
		{
			dt = (long double)(PI * 2 / nm);
		}

		if (t == 0)
		{
			for (i = 0; i < ((N)-cont); i++)
			{
				desl[i] = desl_ant[i] = 0;
				vel[i] = vel_ant[i] = 0;
				acel[i] = acel_ant[i] = 0;
			}
			for (i = 0; i < ((N)-cont); i++)
			{
				fa_ant[i] = 0;
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

		// Matrizes Massa, Rigidez e Amortecimento (C)
		MatrizRigidez_estrutura();
		MatrizMassa_estrutura();
		soma_rigidez_massa();

		Matriz_M_geral();
		Matriz_K_geral();
		MatrizC();
		CondicoesContorno();
		MatrizP1a();
		MatrizP2a();


		// P1[4][4]={{1,2,3},{2,-3,2},{3,1,-1}};

		solucao();
	}

	fclose(output);
	fclose(frequencia);
}