#define  nGmax  	3    // numero de nós
#define  nElmax  	2
#define  nDim		2    // número de dimensões
#define  nELE		2    // número de elementos
#define  NGL		6    // número de graus de liberdade máximo
#define  GLS		4    // número de graus de liberdade

typedef int matriz_nE [nELE][2];
typedef int matriz_E [nELE][5];
typedef   long double matriz_K_M [GLS][GLS];

typedef  long double matriz [NGL][NGL];
typedef  long double matriz_xG [nGmax][nELE];
typedef  long double matriz_suporte [GLS][8];

typedef  long double vetor [NGL];
typedef  long double vetor4 [GLS];
typedef  long double vetor2 [nELE];
typedef  long double vetor3 [12];

typedef struct
	{  long double x[nGmax];	 long double y[nGmax]; }
nos;

typedef struct
	{  long double i[nElmax];  long double j[nElmax];	}
elementos;

typedef struct
	{  long double E[nElmax];  long double A[nElmax];	 long double L[nElmax]; }
propElemTrelica;

typedef char stringArq [80];
