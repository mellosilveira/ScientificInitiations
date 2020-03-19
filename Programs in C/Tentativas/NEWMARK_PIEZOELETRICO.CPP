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

#define TOL 1e-2
#define NITER 1000

#define  nGmax  	3    // numero de nós
#define  nDim		2    // número de dimensões
#define  nELE		2    // número de elementos
#define  NGL		6    // número de graus de liberdade máximo
#define  GL			4	  // número de graus de liberdade por elemento
#define  GLS		4    // número de graus de liberdade menos as condições de contorno igual a zero

int i,j,k;							//auxiliares para função for
int nNs; 							//número de nós
int nEl;                      //número de elementos
int n;                        //auxilar para função for (M e K)
int p,q,r,s;                  //auxiliares para montar as matrizes M e K completas
int z;                        //auxiliar da função principal para dados de saída
int i_aux;                 //auxiliar para leitura dos dados de entrada                    //densidade
int gl;                       //número de graus de liberdade
int N,N1,N2;                        //N= 2*(nEl+1)*gl
int cont;							//contar condições de contorno iguais a zero
int cont1;
int algo;
int nm,np,nt_Transiente;
int nbif;
int key,kont;
long double dens;
long double aux;
long double a,b,c;
long double d,ef,fe,g;
long double h,h1,nh,mn;				      //auxiliares para cálculo das matrizes M e K completas
long double t,t0,dt;							//tempo
long double dist;						//mesma função que dist1 -> passo
long double Wo,W;
long double Wi,Wf,dW;
long double l; 							//tamanho do elemento analisado
long double PI;
long double tf;
long double gama, beta,f1[6];
long double a0,a1,a2,a3,a4,a5,a6,a7;
long double hs,hp,bs,bp;
long double MoIs,MoIp;
long double aes,aep;
int np_F1,np_F2,np_F3,np_F4,np_F5,np_F6;
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

long double xG[nGmax][nELE];           //matriz 3x2 coordenadas dos nós
long double KEls[GL][GL],KElp[GL][GL];               //matriz rigidez 4x4 de 1 elemento
long double Mls[GL][GL],Mlp[GL][GL];		   			//matriz massa 4x4 de 1 elemento
long double Ks[NGL][NGL],Kp[NGL][NGL],K[NGL][NGL];					//matriz rigidez 6x6 do elemento todo
long double Ms[NGL][NGL],Mp[NGL][NGL],M[NGL][NGL];					//matriz massa 6x6 do elemento todo
long double C[GLS][GLS];					//matriz amortecimento 4x4 do elemento todo aplicado as condições de contorno
long double M_cc[GLS][GLS];				//matriz massa 4x4 do elemento todo aplicado as condições de contorno
long double K_cc[GLS][GLS];            //matriz rigidez 4x4 do elemento todo aplicado as condições de contorno
long double P2a[GLS][GLS];
long double P3a[GLS][GLS];
long double P1[GLS][GLS];              //[P1] = a0*[M] + a1*[C] + [K];
int nE[nELE][2];              			//matriz 2x2 coordenadas dos elementos
int MatEl[nELE][GL];							//matriz 2x4 identificação dos elementos
long double M_m[NGL][NGL];					//matriz 6x6 com a massa do motor

long double fa[GLS];                   //vetor forçamento [4] aplicado as condições de contorno
long double P2[GLS];                   //[P2] = [M] x (a0*[y] + a2*[vel] + a3*[acel])

long double P3[GLS];                   //[P3] = [C]*(a1*[y] + a4*[vel] + a5*[acel])
long double P4[GLS];                   //[P4] = [fa]*sin(W*t) + [P2] + [P3]
long double P6[GLS];
long double P7[GLS];                   //vetor delta y para calcular o resultado
long double P7_ant[GLS];               //valor anterior do vetor P7
long double fa_ant[GLS];               //vetor forçamento 4x4 no instante anterior
long double delta_fa[GLS];             //[delta_fa] = [fa]i - [fa]i-1

long double K_Y[GLS];      		      //vetor produto [K] x [y]
long double C_Y[GLS];    	      	   //vetor produto [C] x [vel]
long double K_M_Y[GLS];             	//[K_C_Y] = [fa] - [C_Y] - [K_Y]
long double acel[GLS];						//vetor aceleração
long double f[NGL]; 							//vetor forçamento do corpo todo
long double cc[NGL],cc_p[nELE];                   //vetor condições de contorno
long double Aes[nELE],Aep[nELE];						//vetor área transversal de todos os elementos
long double mEs[nELE],mEp[nELE];                  //vetor módulo de elasticidade de todos os elementos
long double Mos[nELE],Mop[nELE];                  //vetor momento de inércia de todos os elementos
long double y[12]; 							//vetor variáveis
long double y_ant[12];                 //vetor com valores anteriores das variáveis
long double delta_y[12];               //[delta_y] = [y]i - [y]i-1

elementos el;
propElemTrelica propETP;

stringArq arqIn,arqOut;
char abobrinhas[100];
FILE *output;			/* internal filename */
FILE *mola;
FILE *frequencia;
FILE *matrizes;

void LeituraDados(void)
{
   l=10;
   gl = GL/2;
   nNs = nGmax;
   nEl = nNs-1;
  	for(i=0;i<nNs;i++)				/* coordenadas dos nos */
	{
		for(j=0;j<nDim;j++)
		{
         if(j==0)
         {
         	xG[i][j] = 0;
      	}
			if(j==1)
         {
         	xG[i][j] = i*l/nEl;
      	}
		}
	}

	for(i=0;i<nEl;i++)				/* coordenadas dos elementos */
	{
   	for(j=0;j<2;j++)
		{
			nE[i][j] = i+j+1;
		}
	}
	for(i=0;i<nEl;i++)      /* matriz de identificacao do elemento */
	{
		for(j=0;j<GL;j++)
		{
			MatEl[i][j] = 2*i+j;
		}
	}


	printf("***  Solucao de Trelicas planas - MEF  ***");


	strcpy(arqIn,"dados_piezo.inp");
	strcpy(arqOut,"saida.dat");


	testaAberturaArqR(arqIn);		/*  abertura do arq. de Entrada */
  	fgets(abobrinhas,80,fptrIn);
   fscanf(fptrIn,"%d",&nbp);		/*	numero de vigas piezoeleétrica */
   fgets(abobrinhas,80,fptrIn);
   fgets(abobrinhas,80,fptrIn);
   fscanf(fptrIn,"%Lf %Lf %Lf %Lf\n",&bs,&hs,&bp,&hp);		/*	numero de nos */
   fgets(abobrinhas,80,fptrIn);
 //  fgets(abobrinhas,80,fptrIn);
	for (i=0;i<nEl;i++)						/* Leitura do vetor cc do piezoletrico*/
		{
		fscanf(fptrIn,"%Lf",&cc_p[i]);
     	}
   aes=bs*hs;
   for(i=0;i<nEl;i++)      /* momento de inércia */
		{
		  Aes[i]=aes;
		}
   aep=bp*hp;
   for(i=0;i<nEl;i++)      /* momento de inércia */
   {
      if(cc_p[i]==0)
      {
		  Aep[i]=0;
      }
      else
      {
       Aep[i]=nbp*aep;
      }
   }
	fgets(abobrinhas,80,fptrIn);
	fgets(abobrinhas,80,fptrIn);
	for(i=0;i<nEl;i++)      /* modulo de elasticidade */
		{
		fscanf(fptrIn,"%d",&i_aux);
		fscanf(fptrIn,"%Lf",&mEs[i]);
		}
   fgets(abobrinhas,80,fptrIn);
	fgets(abobrinhas,80,fptrIn);
	for(i=0;i<nEl;i++)      /* modulo de elasticidade */
		{
		fscanf(fptrIn,"%d",&i_aux);
		fscanf(fptrIn,"%Lf",&mEp[i]);
		}
    for(i=0;i<nEl;i++)      /* momento de inércia */
   {
      if(cc_p[i]==0)
      {
		  Aep[i]=0;
      }
      else
      {
       Aep[i]=nbp*aep;
      }
   }
   MoIs=bs*pow(hs,3)/12.0;
	 for(i=0;i<nEl;i++)      /* momento de inércia */
		{
		  Mos[i]=MoIs;
		}
    MoIp=nbp*(bp*pow(hp,3)/12.0+bp*hp*pow((hp+hs)/2,2));

	 for(i=0;i<nEl;i++)      /* momento de inércia */
    {
      if(cc_p[i]==0)
      {
		 Mop[i]=0;
      }
      else
      {
      Mop[i]=MoIp;
      }
    }
   //fgets(abobrinhas,80,fptrIn);
	fgets(abobrinhas,80,fptrIn);
	 for(i=0;i<(nEl+1)*gl;i++)      /* forçamento */
		{
		fscanf(fptrIn,"%Lf",&f1[i]);
		}
  	fgets(abobrinhas,80,fptrIn);
   fgets(abobrinhas,80,fptrIn);
	for (i=0;i<(nEl+1)*gl;i++)						/* Leitura do vetor cc */
		{
		fscanf(fptrIn,"%Lf",&cc[i]);
     	}
   fgets(abobrinhas,80,fptrIn);
   fgets(abobrinhas,80,fptrIn);
   fscanf(fptrIn,"%Lf %d %d %d\n",&t0,&nm,&np,&nt_Transiente);
 /* nm = numero de divisoes por periodo
    np = numero de periodos */
   fgets(abobrinhas,80,fptrIn);
   fscanf(fptrIn,"%Lf %Lf %Lf\n",&Wi,&Wf,&dW);

   fgets(abobrinhas,80,fptrIn);


	fclose(fptrIn);							/* Fechamento do Arquivo de leitura */
}
/*  ==================== Rotinas de execucao =====================*/
/***************************************************************************/
/*double carga (long double t,int np,long double *tm,long double *cm)
{
	double c,a;
	int j,m;

   //j = 0;

	for (m=1;m<=np-1;m++)
	{
		if ((t>=tm[m]) && (t<=tm[m+1]))
		{
			j = m;
		}
	}

	a = (cm[j+1]-cm[j])/(tm[j+1]-tm[j]);
	c = (cm[j]-a*tm[j])+a*t;

	return c;
}
/************************************************/
long double carga (long double t,int np,long double *tm,long double *cm)
{
	long double c,a;
	int j,m;

	for (m=1;m<=np-1;m++)
	{
		if ((t>=tm[m]) && (t<=tm[m+1]))
		{
			j = m;
		}
	}

	a = (cm[j+1]-cm[j])/(tm[j+1]-tm[j]);
	c = (cm[j]-a*tm[j])+a*t;

	return c;
}
/************************************************/
 long double *dalloc(int n)
{
 long double *x;

  x = ( long double *) calloc(n,sizeof( long double));
  if (x == NULL) {
	 printf("\n\n   Nao ha memoria disponivel.\n\n   Fim de Execucao.\n");
	 exit(0);
  }
  return x;
}
/******************************************************/

void MatrizRigidez_estrututa(void)
{
/* montagem da matriz global */
		for(i=0;i<N;i++)
			for(j=0;j<N;j++)
				{
			 	Ks[i][j]=0;
				}

		 for(n=0;n<nEl;n++)
			{
         l=10;

			a=(12*Mos[n]*mEs[n]/pow(l,3));
			b=(6*Mos[n]*mEs[n]/pow(l,2));
			c=(4*Mos[n]*mEs[n]/l);

			KEls[1][1]=a;
			KEls[1][2]=b;
			KEls[1][3]=-a;
			KEls[1][4]=b;
			KEls[2][1]=b;
			KEls[2][2]=c;
			KEls[2][3]=-b;
			KEls[2][4]=c/2;
			KEls[3][1]=-a;
			KEls[3][2]=-b;
			KEls[3][3]=a;
			KEls[3][4]=-b;
			KEls[4][1]=b;
			KEls[4][2]=c/2;
			KEls[4][3]=-b;
			KEls[4][4]=c;

			p= MatEl[n][0];
			q= MatEl[n][1];
			r= MatEl[n][2];
			s= MatEl[n][3];

			Ks[p][p]=  KEls[1][1] + Ks[p][p];
			Ks[p][q]=  KEls[1][2] + Ks[p][q];
			Ks[p][r]=  KEls[1][3] + Ks[p][r];
			Ks[p][s]=  KEls[1][4] + Ks[p][s];
			Ks[q][p]=  KEls[2][1] + Ks[q][p];
			Ks[q][q]=  KEls[2][2] + Ks[q][q];
			Ks[q][r]=  KEls[2][3] + Ks[q][r];
			Ks[q][s]=  KEls[2][4] + Ks[q][s];
			Ks[r][p]=  KEls[3][1] + Ks[r][p];
			Ks[r][q]=  KEls[3][2] + Ks[r][q];
			Ks[r][r]=  KEls[3][3] + Ks[r][r];
			Ks[r][s]=  KEls[3][4] + Ks[r][s];
			Ks[s][p]=  KEls[4][1] + Ks[s][p];
			Ks[s][q]=  KEls[4][2] + Ks[s][q];
			Ks[s][r]=  KEls[4][3] + Ks[s][r];
			Ks[s][s]=  KEls[4][4] + Ks[s][s];
			}
}
/***********************************************************/
void MatrizRigidez_piezo(void)
{
/* montagem da matriz global */
		for(i=0;i<N;i++)
			for(j=0;j<N;j++)
				{
			 	Kp[i][j]=0;
				}

		 for(n=0;n<nEl;n++)
			{
         l=10;

			a=(12*Mop[n]*mEp[n]/pow(l,3));
			b=(6*Mop[n]*mEp[n]/pow(l,2));
			c=(4*Mop[n]*mEp[n]/l);

			KElp[1][1]=a;
			KElp[1][2]=b;
			KElp[1][3]=-a;
			KElp[1][4]=b;
			KElp[2][1]=b;
			KElp[2][2]=c;
			KElp[2][3]=-b;
			KElp[2][4]=c/2;
			KElp[3][1]=-a;
			KElp[3][2]=-b;
			KElp[3][3]=a;
			KElp[3][4]=-b;
			KElp[4][1]=b;
			KElp[4][2]=c/2;
			KElp[4][3]=-b;
			KElp[4][4]=c;

			p= MatEl[n][0];
			q= MatEl[n][1];
			r= MatEl[n][2];
			s= MatEl[n][3];

			Kp[p][p]=  KElp[1][1] + Kp[p][p];
			Kp[p][q]=  KElp[1][2] + Kp[p][q];
			Kp[p][r]=  KElp[1][3] + Kp[p][r];
			Kp[p][s]=  KElp[1][4] + Kp[p][s];
			Kp[q][p]=  KElp[2][1] + Kp[q][p];
			Kp[q][q]=  KElp[2][2] + Kp[q][q];
			Kp[q][r]=  KElp[2][3] + Kp[q][r];
			Kp[q][s]=  KElp[2][4] + Kp[q][s];
			Kp[r][p]=  KElp[3][1] + Kp[r][p];
			Kp[r][q]=  KElp[3][2] + Kp[r][q];
			Kp[r][r]=  KElp[3][3] + Kp[r][r];
			Kp[r][s]=  KElp[3][4] + Kp[r][s];
			Kp[s][p]=  KElp[4][1] + Kp[s][p];
			Kp[s][q]=  KElp[4][2] + Kp[s][q];
			Kp[s][r]=  KElp[4][3] + Kp[s][r];
			Kp[s][s]=  KElp[4][4] + Kp[s][s];
			}
}
/************************************************/
void MatrizMassa_estrutura(void)
{
/* montagem da matriz massa*/
		for(i=0;i<N;i++)
			for(j=0;j<N;j++)
				{
			 	Ms[i][j]=0;
				}

		 for(n=0;n<nEl;n++)
			{

		 	fe=22*l*((Aes[n]*dens*l)/420);
			ef=13*l*((Aes[n]*dens*l)/420);
			g=3*l*l*((Aes[n]*dens*l)/420);
         h1=156*((Aes[n]*dens*l)/420);
         nh=54*((Aes[n]*dens*l)/420);
         mn=4*l*l*((Aes[n]*dens*l)/420);


        	Mls[1][1]=h1;
			Mls[1][2]=fe;
			Mls[1][3]=nh;
			Mls[1][4]=-ef;
			Mls[2][1]=fe;
			Mls[2][2]=mn;
			Mls[2][3]=ef;
			Mls[2][4]=-g;
			Mls[3][1]=nh;
			Mls[3][2]=ef;
			Mls[3][3]=h1;
			Mls[3][4]=-fe;
			Mls[4][1]=-ef;
			Mls[4][2]=-g;
			Mls[4][3]=-fe;
			Mls[4][4]=mn;



			p= MatEl[n][0];
			q= MatEl[n][1];
			r= MatEl[n][2];
			s= MatEl[n][3];

			Ms[p][p]=  Mls[1][1] + Ms[p][p];
			Ms[p][q]=  Mls[1][2] + Ms[p][q];
			Ms[p][r]=  Mls[1][3] + Ms[p][r];
			Ms[p][s]=  Mls[1][4] + Ms[p][s];
			Ms[q][p]=  Mls[2][1] + Ms[q][p];
			Ms[q][q]=  Mls[2][2] + Ms[q][q];
			Ms[q][r]=  Mls[2][3] + Ms[q][r];
			Ms[q][s]=  Mls[2][4] + Ms[q][s];
			Ms[r][p]=  Mls[3][1] + Ms[r][p];
			Ms[r][q]=  Mls[3][2] + Ms[r][q];
			Ms[r][r]=  Mls[3][3] + Ms[r][r];
			Ms[r][s]=  Mls[3][4] + Ms[r][s];
			Ms[s][p]=  Mls[4][1] + Ms[s][p];
			Ms[s][q]=  Mls[4][2] + Ms[s][q];
			Ms[s][r]=  Mls[4][3] + Ms[s][r];
			Ms[s][s]=  Mls[4][4] + Ms[s][s];
			}
}

/************************************************************/
void MatrizMassa_piezo(void)
{
/* montagem da matriz massa*/
		for(i=0;i<N;i++)
			for(j=0;j<N;j++)
				{
			 	Mp[i][j]=0;
				}

		 for(n=0;n<nEl;n++)
			{

		 	fe=22*l*((Aep[n]*dens*l)/420);
			ef=13*l*((Aep[n]*dens*l)/420);
			g=3*l*l*((Aep[n]*dens*l)/420);
         h1=156*((Aep[n]*dens*l)/420);
         nh=54*((Aep[n]*dens*l)/420);
         mn=4*l*l*((Aep[n]*dens*l)/420);


        	Mlp[1][1]=h1;
			Mlp[1][2]=fe;
			Mlp[1][3]=nh;
			Mlp[1][4]=-ef;
			Mlp[2][1]=fe;
			Mlp[2][2]=mn;
			Mlp[2][3]=ef;
			Mlp[2][4]=-g;
			Mlp[3][1]=nh;
			Mlp[3][2]=ef;
			Mlp[3][3]=h1;
			Mlp[3][4]=-fe;
			Mlp[4][1]=-ef;
			Mlp[4][2]=-g;
			Mlp[4][3]=-fe;
			Mlp[4][4]=mn;



			p= MatEl[n][0];
			q= MatEl[n][1];
			r= MatEl[n][2];
			s= MatEl[n][3];

			Mp[p][p]=  Mlp[1][1] + Mp[p][p];
			Mp[p][q]=  Mlp[1][2] + Mp[p][q];
			Mp[p][r]=  Mlp[1][3] + Mp[p][r];
			Mp[p][s]=  Mlp[1][4] + Mp[p][s];
			Mp[q][p]=  Mlp[2][1] + Mp[q][p];
			Mp[q][q]=  Mlp[2][2] + Mp[q][q];
			Mp[q][r]=  Mlp[2][3] + Mp[q][r];
			Mp[q][s]=  Mlp[2][4] + Mp[q][s];
			Mp[r][p]=  Mlp[3][1] + Mp[r][p];
			Mp[r][q]=  Mlp[3][2] + Mp[r][q];
			Mp[r][r]=  Mlp[3][3] + Mp[r][r];
			Mp[r][s]=  Mlp[3][4] + Mp[r][s];
			Mp[s][p]=  Mlp[4][1] + Mp[s][p];
			Mp[s][q]=  Mlp[4][2] + Mp[s][q];
			Mp[s][r]=  Mlp[4][3] + Mp[s][r];
			Mp[s][s]=  Mlp[4][4] + Mp[s][s];
			}
}
/************************************************************/
void CondicoesContorno_piezo(void)
{
  for(i=0;i<N;i++)
  {
		if(cc_p[i]==0)
	  	{
			for(j=0;j<N;j++)
         {
				if(j!=i||j==i)
            {
					Kp[i][j]=0;
               Mp[i][j]=0;
            }
         }
			for(k=0;k<N;k++)
         {
				if(k!=i||k==i)
            {
				  Kp[k][i]=0;
              Mp[k][i]=0;
            }
         }
		}
  }
}
/************************************************************/
void soma_rigidez_massa (void)
{
  	for(i=0;i<N;i++)
   {
			for(j=0;j<N;j++)
			{
            K[i][j]=Ks[i][j]+Kp[i][j];
            M[i][j]=Ms[i][j]+Mp[i][j];
			}
   }
}
/************************************************************/
 void CondicoesContorno(void)
{
  for(i=0;i<N;i++)
		{
		if(cc[i]==0)
			{
			for(j=0;j<N;j++)
				if(j!=i||j==i)
            {
					K[i][j]=0;
               M[i][j]=0;
            }
			for(k=0;k<N;k++)
				if(k!=i||k==i)
            {
				  K[k][i]=0;
              M[k][i]=0;
            }
			}
		}

      algo = 0;
   for(i=0;i<(nEl+1)*gl;i++)
   {
   	cont1 = 0;
   	for(j=0;j<(nEl+1)*gl;j++)
      {
      	if(cc[i]!=0 && cc[j]!=0)
         {
         	M_cc[cont1][algo]=M[j][i];
            K_cc[cont1][algo]=K[j][i];
            cont1=cont1+1;
         }
      }
      if(cc[i]!=0)
      {
      	algo = algo + 1;
      }
   }

}

 /*********************************************************/

/*******************************************/
/*******************************************/
void CondicoesContorno1(void)
{
    	cont1=0;

      for(i=0;i<(nEl+1)*gl;i++)      //preenchendo a última linha da nova matriz até antes da penúltima coluna
      {
        if(cc[i]!=0)
        {
          fa[cont1]=f[i];
          cont1=cont1+1;
        }

      }

}
/************************************************************************/
void MatrizC(void)
{
/* montagem da matriz massa*/
		for(i=0;i<N-cont;i++)
      {
			for(j=0;j<N-cont;j++)
				{
			 	C[i][j]=0.005*M_cc[i][j]+0.005*K_cc[i][j];
				}
      }
}

/*********************************************************/
void MatrizP1(void)
{
/* montagem da matriz massa*/
		for(i=0;i<N-cont;i++)
      {
			for(j=0;j<N-cont;j++)
				{
			 	P1[i][j]=a0*M_cc[i][j]+a1*C[i][j]+K_cc[i][j];
				}
      }
}
/*********************************************************/
void MatrizP2a(void)
{
int i,j;
 long double soma,soma1;

     for (i=0; i<N-cont; i++)
     {

		 for (j=0;j<N-cont;j++)
       {
           P2a[i][j]=a2*M_cc[j][i]+a3*C[i][j];

		 }

     }

}
/******************************************************/

/*********************************************************/
void MatrizP2(void)
{
int i,j;
 long double soma,soma1;

     for (i=0; i<N-cont; i++)
     {
       soma = 0.;
		 for (j=0;j<N-cont;j++)
       {
           soma1=P2a[i][j]*y_ant[j+N-cont];
		 }
		 P2[j] = soma;
     }

}

/***********************************************************************/
/***************************************************/
void MatrizP3a(void)
{
int i,j;
 long double soma,soma1;

     for (i=0; i<N-cont; i++)
     {

		 for (j=0;j<N-cont;j++)
       {
           P3a[i][j]=a4*M_cc[j][i]+a5*C[i][j];

		 }

     }

}

/***********************************************************************/
void MatrizP3(void)
{
int i,j;
 long double soma;

     for (j=0; j<N-cont; j++)
     {
       soma = 0.;
		 for (i=0;i<N-cont;i++)
       {
			  soma = soma + P3a[j][i]*y_ant[i+2*(N-cont)];
       }
		 P3[j] = soma;
     }

}


/***********************************************************************/
void MatrizP4(void)
{
/* montagem da matriz massa*/
		for(i=0;i<N-cont;i++)
      {
            P6[i]=fa[i]-fa_ant[i];
			 	P4[i]=P6[i]+P2[i]+P3[i];

      }
}

/***********************************************************************/
void Matriz_K_Y(void)
{
int i,j;
 long double soma;

     for (j=0; j<N-cont; j++)
     {
       soma = 0.;
		 for (i=0;i<N-cont;i++)
       {
			  soma = soma + (K_cc[j][i]*y[i]);
		 }
		 K_Y[j] = soma;
     }

}

/*****************************************************/
void Matriz_C_Y(void)
{
int i,j;
 long double soma;

     for (j=0; j<N-cont; j++)
     {
       soma = 0.;
		 for (i=0;i<N-cont;i++)
       {
			  soma = soma + (C[j][i]*y[i+(N-cont)]);
		 }
		 C_Y[j] = soma;
     }

}
/***********************************************************************/
void Matriz_K_C_Y(void)
{
/* montagem da matriz massa*/
		for(i=0;i<N-cont;i++)
      {

			 	K_M_Y[i]=fa[i]-C_Y[i]-K_Y[i];

      }
}
/*****************************************************/

/********************************************************/
void Elim_Gauss(void)
{
	int ii,jj,kk;
	 long double pivot,coef,aux;


	/*=====  Triangularizacao (triangular superior)  =====*/

	for (ii=0;ii<(N-cont);ii++)
		{
		pivot=P1[ii][ii];
		for (kk=(ii+1);kk<(N-cont);kk++)
			{
			coef=P1[kk][ii]/pivot;
			for (jj=0;jj<(N-cont);jj++)
				{
				P1[kk][jj]=P1[kk][jj]-(coef*P1[ii][jj]);
				}
                        P4[kk]=P4[kk]-(P4[ii]*coef);
			}
		}


	/*=====  Retrosubstituicao  =====*/

	for(ii=(N-cont)-1;ii>=0;ii--)
		{
		aux=0;
		for(jj=ii+1;jj<(N-cont);jj++)
			{
			aux=aux+P1[ii][jj]*P7[jj];
			}
			P7[ii]=(P4[ii]-aux)/P1[ii][ii];
		}
}

/********************************************************/
void Elim_Gauss1(void)
{
	int ii,jj,kk;
	 long double pivot,coef,aux;


	/*=====  Triangularizacao (triangular superior)  =====*/

	for (ii=0;ii<(N-cont);ii++)
		{
		pivot=M_cc[ii][ii];
		for (kk=(ii+1);kk<(N-cont);kk++)
			{
			coef=M_cc[kk][ii]/pivot;
			for (jj=0;jj<(N-cont);jj++)
				{
				M_cc[kk][jj]=M_cc[kk][jj]-(coef*M_cc[ii][jj]);
				}
                        K_M_Y[kk]=K_M_Y[kk]-(K_M_Y[ii]*coef);
			}
		}


	/*=====  Retrosubstituicao  =====*/

	for(ii=(N-cont)-1;ii>=0;ii--)
		{
		aux=0;
		for(jj=ii+1;jj<(N-cont);jj++)
			{
			aux=aux+M_cc[ii][jj]*acel[jj];
			}
			acel[ii]=(K_M_Y[ii]-aux)/M_cc[ii][ii];
		}
}




/******************************************************/


/******************************************/
void solucao(void)
{
long double z;
int i,jn,jp,cont1;

z=0;
cont1=0;
 for (jp=1;jp<=np-1;jp++)
 {
   for (jn=1; jn<=nm-1;jn++)			/* time loop */
   {
      f[0] =f1[0]*sin(W*t);
      f[1] =f1[1]*sin(W*t);
      f[2] =f1[2]*sin(W*t);
      f[3] =f1[3]*sin(W*t);
      f[4] =f1[4]*sin(W*t);
      f[5] =f1[5]*sin(W*t);
      CondicoesContorno1();
      if(t==0)
      {
         for (i=0;i<3*(N-cont);i++)
         {
            y[i]=y_ant[i]=0.0;				/* initial position */
         }

          Matriz_K_Y();
          Matriz_C_Y();
          Matriz_K_C_Y();
          Elim_Gauss1();

         for (i=0;i<(N-cont);i++)
         {
            y[i+2*(N-cont)]=acel[i];				/* initial position */
         }

          for (i=2*(N-cont);i<3*(N-cont);i++)
         {
            y_ant[i]=y[i];				/* initial position */
         }

     }



    // if(jp> nt_Transiente)
      if(jp> 0)
     {
       if( sqrt((W-0.5)*(W-0.5))<0.0001)
       {
        	fprintf(output,"%Lf\t %Lf\t %Lf\t %Lf\t %Lf\t %Lf\n",W,t,y[0],y[1],y[2],y[3]);
        }
      }

      MatrizP1();
      MatrizP2();
      MatrizP3();
      MatrizP4();
      Elim_Gauss();

     for(i=0;i<(N-cont);i++)
      {
        delta_y[i]=P7[i];
      }


      for (i=0;i<(N-cont);i++)
      {
         delta_y[i+2*(N-cont)]=a1*(P7[i])-a3*y_ant[i+(N-cont)]-a5*y_ant[i+2*(N-cont)];
      }

      for (i=0;i<(N-cont);i++)
      {
         delta_y[i+(N-cont)]=a0*(P7[i])-a2*y_ant[i+(N-cont)]-a4*y_ant[i+2*(N-cont)];
      }

      for (i=0;i<=3*(N-cont);i++)
      {
         y[i]=y_ant[i]+delta_y[i];
      }

     t = t+dt;

     for (i=0;i<3*(N-cont);i++)
     {
       y_ant[i]=y[i];		 /* initial position */
     }

     for (i=0;i<(N-cont);i++)
     {
       fa_ant[i]=fa[i];		 /* initial position */
     }

   }
 }



  fprintf(frequencia,"%Lg\t %Lg\n",W,z);

}


/*******************************************/
/* Programa Principal */

main()
{
 int i,jb,jn,jp;
 PI=4*atan(1);            /*numero PI*/
 beta=1.0/4.0;
 gama=1.0/2.0;
 dens=7850;
 LeituraDados();

 output=fopen("LINEAR.dat", "w");			/* external filename */
 mola=fopen("LINEAR1.dat", "w");			/* external filename */
 frequencia=fopen("freq.dat", "w");
 matrizes = fopen("matrizes.txt","w");

 cont=0;
 N=(nEl+1)*gl; 			//Por que o 2* se no fu não é?
 for(i=0;i<(nEl+1)*gl;i++)
 {
   if(cc[i]==0)
   {
      cont=cont+1;
   }
 }
 N1=3*((nEl+1)*gl-cont);
 N2= (N-cont);



//Matrizes cujos valores são constantes no tempo
 MatrizRigidez_estrututa();
 MatrizRigidez_piezo();
 MatrizMassa_estrutura();
 MatrizMassa_piezo();
 CondicoesContorno_piezo();
 soma_rigidez_massa();
 CondicoesContorno();
 MatrizC();

  nbif = (int) ((Wf-Wi)/dW)+1+TOL;

  for (jb = 0; jb<=nbif-1; jb++)
 {
  W = Wi +(jb*dW);
  printf("W = %g\n",W);
  if (W!=0) dt = ( long double) ((PI*2./W)/nm);
  else      dt = ( long double) ((PI*2.)/nm);

 // dt=0.0001;
  //=nm*np*dt;


   a0=1.0/(beta*dt*dt);
   a1=gama/(beta*dt);
   a2=1.0/(beta*dt);
   a3=gama/(beta);
   a4=1/(2*beta);
   a5=dt*((gama/(2*beta))-1);
   a6=dt*(1-gama);
   a7=gama*dt;

   MatrizP1();
   solucao();
  }

fclose(mola);
fclose(output);
fclose(frequencia);
fclose(matrizes);

}





