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

#define  nN  		3    // numero de nós
#define  nDim		2    // número de dimensões
#define  nELE		2    // número de elementos = número de nós - 1
#define  NGL		6    // número de graus de liberdade máximo
#define 	GL			4	  // número de graus de liberdade por elemento
#define  GLS		4    // número de graus de liberdade menos as condições de contorno igual a zero

#define dist1 1e-1 /* stepsize in w*/
#define MAX 30.0		/* max for t */
#define MAX1 30.0		/* max for w */
#define cont2 1.0

#define TOL 1e-2
#define NITER 1000


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
long double gama, beta,f[6],f1[6];
long double a0,a1,a2,a3,a4,a5,a6,a7;
int np_F1,np_F2,np_F3,np_F4,np_F5,np_F6;

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

matriz_xG xG;                 //matriz 3x2 coordenadas dos nós
matriz KEl,Ml;		   //matriz rigidez 6x6 do elemento todo
matriz K,M;						   //matriz massa 6x6 do elemento todo
matriz_K_M C,K_M,C_M;
matriz_K_M inv,M_cc,K_cc,P1;						//matriz amortecimento 4x4 do elemento todo
matriz DV; 					      //NÃO USADO NOS CÁLCULOS matriz 6x6
matriz_nE nE;                 //matriz 2x2 coordenadas dos elementos
matriz_E MatEl;               //matriz 2x4 identificação dos elementos
matriz_suporte M_cc1,e,ei;    //matrizes 4x8  long double para calcular a inversa de M_cc
matriz_K_M invM;					//matriz 4x4  long double para calcular as acelerações inicias

vetor4 fa,P2,P3,P4,P7,P7_ant,P6, fa_ant,delta_fa;                    //vetor [4] produto matriz C-inv(M) com vetor vel
vetor cc;							//vetor [6] condições de contorno
vetor2 aE;							//vetor [2] área dos elementos
vetor2 mE;							//vetor [2] módulo de elasticidade
vetor2 Mo;							//vetor [2] momento de inércia
vetor3 y,y_ant,delta_y;							//vetor [12] variáveis
vetor4 K_Y, C_Y,K_M_Y,acel, auxy2;				//vetor[4] para calcular a aceleracao

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

	printf("***  Solucao de Trelicas planas - MEF  ***");


	strcpy(arqIn,"dados9.inp");
	strcpy(arqOut,"saida.dat");

   l=10;
   gl = GL/2;
   nNs = nN;
   nEl = nNs - 1;
   for(i=0;i<nNs;i++)				/* coordenadas dos nos */
   {
		for(j=0;j<2;j++)
      {
         if(j==0)
         {
      		xG[i][j] = 0;
         }
         if(j==1)
         {
         	xG[i][j] = i*l/(nNs-1);
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

	testaAberturaArqR(arqIn);		/*  abertura do arq. de Entrada */

   fgets(abobrinhas,80,fptrIn);
	for(i=0;i<nEl;i++)      /* area dos elementos */
		{
		fscanf(fptrIn,"%d",&i_aux);
		fscanf(fptrIn,"%Lf",&aE[i]);
		}

	fgets(abobrinhas,80,fptrIn);
	fgets(abobrinhas,80,fptrIn);
	for(i=0;i<nEl;i++)      /* modulo de elasticidade */
		{
		fscanf(fptrIn,"%d",&i_aux);
		fscanf(fptrIn,"%Lf",&mE[i]);
		}
   fgets(abobrinhas,80,fptrIn);
	fgets(abobrinhas,80,fptrIn);
	 for(i=0;i<nEl;i++)      /* modulo de elasticidade */
		{
		fscanf(fptrIn,"%d",&i_aux);
		fscanf(fptrIn,"%Lf",&Mo[i]);
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
   fscanf (fptrIn,"%d\n",&np_F1);  // Componente 11

	for (j=1;j<=np_F1;j++)
	{
	   fscanf (fptrIn,"%Lf %Lf\n",&t_v1[j],&F_v1[j]);
	}
//----------------------------------------------------------------------------//
   fscanf (fptrIn,"%d\n",&np_F2); // Componente 22

	for (j=1;j<=np_F2;j++)
	{
      fscanf (fptrIn,"%Lf %Lf\n",&t_v2[j],&F_v2[j]);
	}
//----------------------------------------------------------------------------//
   fscanf (fptrIn,"%d\n",&np_F3);  // Componente 33

   for (j=1;j<=np_F3;j++)
   {
      fscanf (fptrIn,"%Lf %Lf\n",&t_v3[j],&F_v3[j]);
   }
//----------------------------------------------------------------------------//
// Componentes cisalhantes, as seis componentes da tensão correspondem à
// tensão cisalhante e novamente o tensor de tensõe é simétrico, ou seja,
// os componentes "ij" são iguais aos "ji", por isso, só os "ij" são definidos:


	fscanf (fptrIn,"%d\n",&np_F4);  // Componente 12

	for (j=1;j<=np_F4;j++)
   {
		fscanf (fptrIn,"%Lf %Lf\n",&t_v4[j],&F_v4[j]);
	}
//----------------------------------------------------------------------------//
   fscanf (fptrIn,"%d\n",&np_F5); // Componente 23

	for (j=1;j<=np_F5;j++)
	{
      fscanf (fptrIn,"%Lf %Lf\n",&t_v5[j],&F_v5[j]);
	}
//----------------------------------------------------------------------------//
   fscanf (fptrIn,"%d\n",&np_F6); // Componente 13

	for (j=1;j<=np_F6;j++)
	{
      fscanf (fptrIn,"%Lf %Lf\n",&t_v6[j],&F_v6[j]);
	}



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

void MatrizRigidez(void)
{
/* montagem da matriz global */
		for(i=0;i<N;i++)
			for(j=0;j<N;j++)
				{
			 	K[i][j]=0;
				}

		 for(n=0;n<nEl;n++)
			{

			a=(12*Mo[n]*mE[n]/pow(l,3));
			b=(6*Mo[n]*mE[n]/pow(l,2));
			c=(4*Mo[n]*mE[n]/l);

			KEl[1][1]=a;
			KEl[1][2]=b;
			KEl[1][3]=-a;
			KEl[1][4]=b;
			KEl[2][1]=b;
			KEl[2][2]=c;
			KEl[2][3]=-b;
			KEl[2][4]=c/2;
			KEl[3][1]=-a;
			KEl[3][2]=-b;
			KEl[3][3]=a;
			KEl[3][4]=-b;
			KEl[4][1]=b;
			KEl[4][2]=c/2;
			KEl[4][3]=-b;
			KEl[4][4]=c;

			p= MatEl[n][0];
			q= MatEl[n][1];
			r= MatEl[n][2];
			s= MatEl[n][3];

			K[p][p]=  KEl[1][1] + K[p][p];
			K[p][q]=  KEl[1][2] + K[p][q];
			K[p][r]=  KEl[1][3] + K[p][r];
			K[p][s]=  KEl[1][4] + K[p][s];
			K[q][p]=  KEl[2][1] + K[q][p];
			K[q][q]=  KEl[2][2] + K[q][q];
			K[q][r]=  KEl[2][3] + K[q][r];
			K[q][s]=  KEl[2][4] + K[q][s];
			K[r][p]=  KEl[3][1] + K[r][p];
			K[r][q]=  KEl[3][2] + K[r][q];
			K[r][r]=  KEl[3][3] + K[r][r];
			K[r][s]=  KEl[3][4] + K[r][s];
			K[s][p]=  KEl[4][1] + K[s][p];
			K[s][q]=  KEl[4][2] + K[s][q];
			K[s][r]=  KEl[4][3] + K[s][r];
			K[s][s]=  KEl[4][4] + K[s][s];
			}
}

void MatrizMassa(void)
{
/* montagem da matriz massa*/
		for(i=0;i<N;i++)
			for(j=0;j<N;j++)
				{
			 	M[i][j]=0;
				}

		 for(n=0;n<nEl;n++)
			{

		 	fe=22*l*((aE[n]*dens*l)/420);
			ef=13*l*((aE[n]*dens*l)/420);
			g=3*l*l*((aE[n]*dens*l)/420);
         h1=156*((aE[n]*dens*l)/420);
         nh=54*((aE[n]*dens*l)/420);
         mn=4*l*l*((aE[n]*dens*l)/420);


        	Ml[1][1]=h1;
			Ml[1][2]=fe;
			Ml[1][3]=nh;
			Ml[1][4]=-ef;
			Ml[2][1]=fe;
			Ml[2][2]=mn;
			Ml[2][3]=ef;
			Ml[2][4]=-g;
			Ml[3][1]=nh;
			Ml[3][2]=ef;
			Ml[3][3]=h1;
			Ml[3][4]=-fe;
			Ml[4][1]=-ef;
			Ml[4][2]=-g;
			Ml[4][3]=-fe;
			Ml[4][4]=mn;



			p= MatEl[n][0];
			q= MatEl[n][1];
			r= MatEl[n][2];
			s= MatEl[n][3];

			M[p][p]=  Ml[1][1] + M[p][p];
			M[p][q]=  Ml[1][2] + M[p][q];
			M[p][r]=  Ml[1][3] + M[p][r];
			M[p][s]=  Ml[1][4] + M[p][s];
			M[q][p]=  Ml[2][1] + M[q][p];
			M[q][q]=  Ml[2][2] + M[q][q];
			M[q][r]=  Ml[2][3] + M[q][r];
			M[q][s]=  Ml[2][4] + M[q][s];
			M[r][p]=  Ml[3][1] + M[r][p];
			M[r][q]=  Ml[3][2] + M[r][q];
			M[r][r]=  Ml[3][3] + M[r][r];
			M[r][s]=  Ml[3][4] + M[r][s];
			M[s][p]=  Ml[4][1] + M[s][p];
			M[s][q]=  Ml[4][2] + M[s][q];
			M[s][r]=  Ml[4][3] + M[s][r];
			M[s][s]=  Ml[4][4] + M[s][s];
			}
}

/************************************************************/
 /*********************************************************/
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
void MatrizP2(void)
{
int i,j;
 long double soma,soma1;

     for (j=0; j<N-cont; j++)
     {
       soma = 0.;
		 for (i=0;i<N-cont;i++)
       {
           soma1=(a0*y_ant[i]+a2*y_ant[i+N-cont]+a3*y_ant[i+2*(N-cont)]);
			  soma = soma + M_cc[j][i]*soma1;
		 }
		 P2[j] = soma;
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
			  soma = soma + C[j][i]*(a1*y_ant[i]+a4*y_ant[i+N-cont]+a5*y_ant[i+2*(N-cont)]);
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
            P6[i]=fa[i];
			 	P4[i]=fa[i]+P2[i]+P3[i];

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
			  soma = soma + (C[j][i]*y[i+N-cont]);
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

	for (ii=0;ii<(N-cont-1);ii++)
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

	for (ii=0;ii<(N-cont-1);ii++)
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
      f[0] = carga (t,np_F1,t_v1,F_v1);
      f[1] = carga (t,np_F2,t_v2,F_v2);
      f[2] = carga (t,np_F3,t_v3,F_v3);
      f[3] = carga (t,np_F4,t_v4,F_v4);
      f[4] = carga (t,np_F5,t_v5,F_v5);
      f[5] = carga (t,np_F6,t_v6,F_v6);
      CondicoesContorno1();
      if(t==0)
      {
         for (i=0;i<2*(N-cont);i++)
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
     // if(jp> 0)
     //{
       //if( sqrt((W-0.5)*(W-0.5))<0.0001)
       //{
        	fprintf(output,"%Lf\t %Lf\t %Lf\t %Lf\t %Lf\t %Lf\n",W,t,y[0],y[1],y[2],y[3]);        //}
      //}

      MatrizP1();
      MatrizP2();
      MatrizP3();
      MatrizP4();
      Elim_Gauss();

      for(i=0;i<(N-cont);i++)
      {
        y[i]=P7[i];
      }


      for (i=0;i<(N-cont);i++)
      {
         y[i+2*(N-cont)]=a0*(y[i]-y_ant[i])-a2*y_ant[i+(N-cont)]-a3*y_ant[i+2*(N-cont)];
      }

      for (i=0;i<(N-cont);i++)
      {
         y[i+(N-cont)]=y_ant[i+(N-cont)]+a6*y_ant[i+2*(N-cont)]+a7*y[i+2*(N-cont)];
      }

     t = t+dt;

     for (i=0;i<3*(2);i++)
     {
       y_ant[i]=y[i];		 /* initial position */
     }

   }
 }

  fprintf(frequencia,"%Lf\t %Lf\n",W,z);

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
 MatrizRigidez();
 MatrizMassa();
 CondicoesContorno();
 MatrizC();

  nbif = (int) ((Wf-Wi)/dW)+1+TOL;

  for (jb = 0; jb<=nbif-1; jb++)
 {
  W = Wi +(jb*dW);
  printf("W = %g\n",W);
  if (W!=0) dt = ( long double) ((PI*2./W)/nm);
  else      dt = ( long double) ((PI*2.)/nm);

  dt=0.0001;
  //=nm*np*dt;


   a0=1.0/(beta*dt*dt);
   a1=gama/(beta*dt);
   a2=1.0/(beta*dt);
   a3=(1.0/(2*beta))-1;
   a4=(gama/(beta))-1;
   a5=(dt/2.0)*((gama/(beta))-2);
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





