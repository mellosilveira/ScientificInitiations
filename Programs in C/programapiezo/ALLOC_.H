/************************************************************************/
double *dalloc(int n)
{
double *x;

  x = (double *) calloc(n,sizeof(double));
  if (x == NULL) {
	 printf("\n\n   Nao ha memoria disponivel.\n\n   Fim de Execucao.\n");
	 exit(0);
  }
  return x;
}
/************************************************************************/
char *challoc(int n)
{
char *x;

  x = (char *) calloc(n,sizeof(char));
  if (x == NULL) {
	 printf("\n\n   Nao ha memoria disponivel.\n\n   Fim de Execucao.\n");
	 exit(0);
  }
  return x;
}
/************************************************************************/
int *ialloc(int n)
{
int *x;

  x = (int *) calloc(n,sizeof(int));
  if (x == NULL) {
	 printf("\n\n   Nao ha memoria disponivel.\n\n   Fim de Execucao.\n");
	 exit(0);
  }
  return x;
}
/************************************************************************/

