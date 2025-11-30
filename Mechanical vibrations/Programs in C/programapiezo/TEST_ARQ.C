#include <process.h>

FILE *fptrIn,*fptrOut;



void testaAberturaArqR(stringArq nomeArq)
{
	if( (fptrIn=fopen(nomeArq,"r")) == NULL )
		{
		printf("erro abrindo arquivo entrada!!!");
//		exit(1);
		}
}


void testaAberturaArqW(stringArq nomeArq)
{
	if( (fptrOut=fopen(nomeArq,"w")) == NULL )
		{
		printf("erro abrindo arquivo saida!!!");
//		exit(1);
		}
}
