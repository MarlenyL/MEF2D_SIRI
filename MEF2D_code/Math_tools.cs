using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

//Se define un arreglo de reales como una lista
using Vector = System.Collections.Generic.List<double>;

//Se define un arreglo de vectores como una matriz
using Matrix = System.Collections.Generic.List<System.Collections.Generic.List<double>>;

namespace MEF1D_code
{
    public static class Math_tools
    {
        //La función recibe:
        //- Una matriz (se asume que será cuadrada)
        //- La dimensión de la matriz
        //La función crea una matriz cuadrada n x n llena de ceros
        public static void zeroes(ref Matrix M, int n)
        {
            //Se crean n filas
            for (int i = 0; i < n; i++)
            {
                //Se crea una fila de n ceros
                Vector auxrow = new Vector();
                
                for(int j =0; j<n;j++){
                    auxrow.Add(0.0);
                }
                
                //Se ingresa la fila en la matriz
                M.Add(auxrow);
            }
        }


        //La función recibe:
        //- Un vector (se asume columna)
        //- La dimensión del vector
        //La función crea un vector n x 1 lleno de ceros
        public static void zeroes(ref Matrix M,int n,int m){
            //Se itera n veces
                        //Se crean n filas
            for (int i = 0; i < n; i++)
            {
                //Se crea una fila de n ceros
                Vector auxrow = new Vector();
                
                for(int j =0; j<m;j++){
                    auxrow.Add(0.0);
                }
                
                //Se ingresa la fila en la matriz
                M.Add(auxrow);
            }
        }

        public static void zeroes(ref Vector v,int n){
            for(int i=0;i<n;i++){
                v.Add(0.0);
            }
        }


        //La función recibe:
        //- Una matriz
        //- Una matriz que será la copia de la primera
        //La función copiará todo el contenido de la primera matriz en
        //la segunda, respetando las posiciones
        public static void copyMatrix(Matrix A, ref Matrix copy){
            //Se inicializa la copia con ceros
            //asegurándose de sus dimensiones
            zeroes(ref copy,A.Count);
            //Se recorre la matriz original
            for(int i=0;i<A.Count;i++)
                for(int j=0;j<A[0].Count;j++)
                {
                    //Se coloca la celda actual de la matriz original
                    //en la misma posición dentro de la copia
                    double aux =A[i][j];
                    copy[i][j] = aux;
                }
        }


        //La función recibe:
        //- Una matriz
        //- Un vector
        //- Un vector para la respuesta
        //La función asume que las dimensiones de la matriz y los vectores son las
        //adecuadas para que la multiplicación sea posible
        public static void productMatrixVector(Matrix A, Vector v, ref Vector R){
            //Se aplica básicamente la formulación que puede
            //consultarse en el siguiente enlace (entrar con cuenta UCA):
            //          https://goo.gl/PEzWWe

            //Se itera una cantidad de veces igual al número de filas de la matriz
            for(int f=0;f<A.Count;f++){
                //Se inicia un acumulador
                double cell = 0.0;
                //Se calcula el valor de la celda de acuerdo a la formulación
                for(int c=0;c<v.Count;c++){
                    double aux =A[f][c];
                    cell += aux*v[c];
                }
                //Se coloca el valor calculado en su celda correspondiente en la respuesta
                R[f] += cell;
            }
        }

        public static float calculateMember(int i,int j,int r,Matrix A,Matrix B){
            float member = 0;
            for(int k=0;k<r;k++)
                member += (float)A[i][k]*(float)B[k][j];
            return member;
        }

        public static Matrix productMatrixMatrix(Matrix A,Matrix B,int n,int r,int m){
            Matrix R = new Matrix();

            Math_tools.zeroes(ref R,n,m);
            for(int i=0;i<n;i++)
                for(int j=0;j<m;j++)
                    R[i][j] = calculateMember(i,j,r,A,B);

            return R;
        }


        //La función recibe:
        //- Un escalar (valor real)
        //- Una matriz
        //- Una matriz para la respuesta
        //La función multiplica cada uno de los elementos de la matriz por el escalar,
        //ubicando los resultados en la matriz de respuesta
        public static void productRealMatrix(double real,Matrix M,ref Matrix R){
            //Se prepara la matriz de respuesta con las mismas dimensiones de la
            //matriz
            zeroes(ref R,M.Count);
            //Se recorre la matriz original
            for(int i=0;i<M.Count;i++)
                for(int j=0;j<M[0].Count;j++){
                    //La celda actual se multiplica por el real, y se almacena
                    //el resultado en la matriz de respuesta
                    double aux = M[i][j];
                    R[i][j] = real*aux;
                }
        }

        //La función recibe:
        //- Una matriz
        //- Un índice de fila i
        //- Un índice de columna j
        //La función elimina en la matriz la fila i, y la columna j
        public static void getMinor(ref Matrix M,int i, int j){
            //Se elimina la fila i
            M.RemoveAt(i);
            //Se recorren las filas restantes
            for(int h=0;h<M.Count;h++)
                //En cada fila se elimina la columna j
                M[h].RemoveAt(j);
        }

        //La función recibe:
        //- Una matriz
        //La función calcula el determinante de la matriz de forma recursiva
        public static double determinant(Matrix M){
            //Caso trivial: si la matriz solo tiene una celda, ese valor es el determinante
            
            if(M.Count == 1)
            {
                return M[0][0];
            }
            else
            {
                //Se implementa la siguiente formulación del siguiente enlace:
                //(Entrar con cuenta UCA)
                //              https://goo.gl/kbWdmu

                //Se inicia un acumulador
                double det=0.0;
                //Se recorre la primera fila
                for(int i=0;i<M[0].Count;i++){
                    //Se obtiene el menor de la posición actual
                    Matrix minor = new Matrix();
                    copyMatrix(M,ref minor);
                    getMinor(ref minor,0,i);

                    //Se calculala contribución de la celda actual al determinante
                    //(valor alternante * celda actual * determinante de menor actual)
                    double aux = M[0][i];
                    det += Math.Pow(-1,i)*aux*determinant(minor);
                }
                return det;
            }
        }


        //La función recibe:
        //- Una matriz
        //- Una matriz que contendrá los cofactores de la primera
        public static void cofactors(Matrix M, ref Matrix Cof){
            //La matriz de cofactores se define así:
            //(Entrar con cuenta UCA)
            //          https://goo.gl/QK7BZo

            //Se prepara la matriz de cofactores para que sea de las mismas
            //dimensiones de la matriz original
            zeroes(ref Cof,M.Count);
            //Se recorre la matriz original
            for(int i=0;i<M.Count;i++){
                for(int j=0;j<M[0].Count;j++){
                    //Se obtiene el menor de la posición actual
                    Matrix minor = new Matrix();
                    copyMatrix(M,ref minor);
                    getMinor(ref minor,i,j);
                    //Se calcula el cofactor de la posición actual
                    //      alternante * determinante del menor de la posición actual
                    Cof[i][j] = Math.Pow(-1,i+j)*determinant(minor);
                }
            }
        }

        //La función recibe:
        //- Una matriz
        //- Una matriz que contendrá a la primera pero transpuesta
        //La función transpone la primera matriz y almacena el resultado en la segunda
        public static void transpose(Matrix M, ref Matrix T){
            //Se prepara la matriz resultante con las mismas dimensiones
            //de la matriz original
            zeroes(ref T,M.Count);
            //Se recorre la matriz original
            for(int i=0;i<M.Count;i++)
                for(int j=0;j<M[0].Count;j++)
                {
                        //La posición actual se almacena en la posición con índices
                        //invertidos de la matriz resultante
                        double aux = M[i][j]; 
                        T[j][i] = aux;
                }
        }
        


        //La función recibe:
        //- Una matriz
        //- Una matriz que contendrá la inversa de la primera matriz
        //La matriz calcula la inversa de la primera matriz, y almacena el resultado
        //en la segunda
        public static void inverseMatrix(Matrix M, ref Matrix Minv){
            //Se utiliza la siguiente fórmula:
            //      (M^-1) = (1/determinant(M))*Adjunta(M)
            //             Adjunta(M) = transpose(Cofactors(M))

            //Se preparan las matrices para la de cofactores y la adjunta
            Matrix Cof = new Matrix();
            Matrix Adj = new Matrix();
            //Se calcula el determinante de la matriz
            double det = determinant(M);
            //Si el determinante es 0, se aborta el programa
            //No puede dividirse entre 0 (matriz no invertible)
            if(det == 0)
            { 
                Console.WriteLine("Determinante Cero");
                Environment.Exit(1);
            }
            
            //Se calcula la matriz de cofactores
            cofactors(M,ref Cof);
            //Se calcula la matriz adjunta
            transpose(Cof,ref Adj);
            //Se aplica la fórmula para la matriz inversa
            productRealMatrix(1/det,Adj,ref Minv);
        }
    };

}