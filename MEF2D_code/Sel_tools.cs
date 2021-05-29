using System.Collections.Generic;
using System;
//Se define un arreglo de reales como una lista
using Vector = System.Collections.Generic.List<double>;
//Se define un arreglo de vectores como una matriz
using Matrix = System.Collections.Generic.List<System.Collections.Generic.List<double>>;

namespace MEF1D_code
{
    public static class Sel_tools
    {
        //La función recibe:
        //- Una matriz
        //La función muestra en pantalla el contenida de la matrix, fila por fila
        public static void showMatrix(Matrix K){
            for(int i=0;i<K.Count;i++){
                Console.Write("[\t");
                for(int j=0;j<K[i].Count;j++){
                    Console.Write(Math.Round(K[i][j],2)+ "\t");
                }
                Console.Write("]\n");
            }
        }

        //La función recibe un arreglo de matrices, y las muestra
        //en pantalla una por una
        public static void showKs(List<Matrix> Ks){
            for(int i=0;i<Ks.Count;i++){
                Console.Write("K del elemento "+ i+1 + ":\n");
                showMatrix(Ks[i]);
                Console.Write("*************************************\n");
            }
        }

        //La función recibe:
        //- Un vector
        //La función asume que recibe un vector columna y muestra su contenido
        //en pantalla en una sola fila
        public static void showVector(Vector b){
            Console.Write("[\t");
            for(int i=0;i<b.Count;i++){
                Console.Write(Math.Round(b[i],2) + "\t");
            }
            Console.Write("]\n");
        }

        //La función recibe un arreglo de vectores, y los muestra
        //en pantalla uno por uno
        public static void showbs(List<Vector> bs){
            for(int i=0;i<bs.Count;i++){
                Console.Write("b del elemento "+ i+1 + ":\n");
                showVector(bs[i]);
                Console.Write("*************************************\n");
            }
        }


        public static float calculateLocalD(int i,ref mesh m){
            float D,a,b,c,d;

            element e = m.getElement(i);

            node n1 = m.getNode(e.getNode1()-1);
            node n2 = m.getNode(e.getNode2()-1);
            node n3 = m.getNode(e.getNode3()-1);

            a=n2.getX()-n1.getX();b=n2.getY()-n1.getY();
            c=n3.getX()-n1.getX();d=n3.getY()-n1.getY();
            D = a*d - b*c;

            return D;
        }
        public static float calculateMagnitude(float v1, float v2){
            return (float)Math.Sqrt(Math.Pow(v1,2) + Math.Pow(v2,2));
        }

        public static float calculateLocalArea(int i,ref mesh m){
    //Se utiliza la fórmula de Herón
            float A,s,a,b,c;
            element e = m.getElement(i);
            node n1 = m.getNode(e.getNode1()-1);
            node n2 = m.getNode(e.getNode2()-1);
            node n3 = m.getNode(e.getNode3()-1);

            a = calculateMagnitude(n2.getX()-n1.getX(),n2.getY()-n1.getY());
            b = calculateMagnitude(n3.getX()-n2.getX(),n3.getY()-n2.getY());
            c = calculateMagnitude(n3.getX()-n1.getX(),n3.getY()-n1.getY());
            s = (a+b+c)/2;

            A = (float)Math.Sqrt(s*(s-a)*(s-b)*(s-c));
            return A;
        }

        public static void calculateLocalA(int i,ref Matrix A,mesh m){
            element e = m.getElement(i);
            node n1 = m.getNode(e.getNode1()-1);
            node n2 = m.getNode(e.getNode2()-1);
            node n3 = m.getNode(e.getNode3()-1);
            A[0][0] = n3.getY()-n1.getY(); A[0][1] = n1.getY()-n2.getY();
            A[1][0] = n1.getX()-n3.getX(); A[1][1] = n2.getX()-n1.getX();
        }

        public static void calculateB(ref Matrix B){
            B[0][0] = -1; B[0][1] = 1; B[0][2] = 0;
            B[1][0] = -1; B[1][1] = 0; B[1][2] = 1;
        }
        //La función recibe:
        //- Un elemento
        //- El objeto mesh
        //La función construye la matrix local K para el elemento
        //especificado de acuerdo a la formulación del problema
        public static Matrix createLocalK(int element,ref mesh m){
            float D,Ae,k =  m.getParameter((int) parametersE.THERMAL_CONDUCTIVITY);

            Matrix K = new Matrix();
            Matrix A = new Matrix();
            Matrix B = new Matrix();
            Matrix Bt = new Matrix();
            Matrix At = new Matrix();

            D= calculateLocalD(element,ref m);
            Ae = calculateLocalArea(element,ref m);

            Math_tools.zeroes(ref A,2);

            Math_tools.zeroes(ref B,2,3);
            calculateLocalA(element,ref A,m);
            calculateB(ref B);
            Math_tools.transpose(A,ref At);
            Math_tools.transpose(B,ref Bt);
            
            Math_tools.productRealMatrix(k*Ae/(D*D), Math_tools.productMatrixMatrix(Bt,Math_tools.productMatrixMatrix(At,Math_tools.productMatrixMatrix(A,B,2,2,3),2,2,3),3,2,3), ref K);

            return K;
        }

        public static float calculateLocalJ(int i,ref mesh m){
            float J,a,b,c,d;
            element e = m.getElement(i);
            node n1 = m.getNode(e.getNode1()-1);
            node n2 = m.getNode(e.getNode2()-1);
            node n3 = m.getNode(e.getNode3()-1);

            a=n2.getX()-n1.getX();b=n3.getX()-n1.getX();
            c=n2.getY()-n1.getY();d=n3.getY()-n1.getY();
            J = a*d - b*c;

            return J;
        }

        //La función recibe:
        //- Un elemento
        //- El objeto mesh
        //La función construye el vector local b para el elemento
        //especificado de acuerdo a la formulación del problema
        public static Vector createLocalb(int element,ref mesh m){

            Vector b= new Vector();

            float Q = m.getParameter((int) parametersE.HEAT_SOURCE),J,b_i;
            J = calculateLocalJ(element,ref m);

            b_i = Q*J/6;
            b.Add(b_i); b.Add(b_i); b.Add(b_i);

            return b;
        }

        //La función recibe:
        //- El objeto mesh
        //- Un arreglo de matrices
        //- Un arreglo de vectores
        //La función construye una K y una b locales para cada elemento de la malla,
        //y los almacena en su arreglo correspondiente
        public static void crearSistemasLocales(ref mesh m,ref List<Matrix> localKs,ref List<Vector> localbs){
            //Se recorren los elementos
            for(int i=0;i<m.getSize((int)sizesE.ELEMENTS);i++){
                //Por cada elemento, se crea su K y su b
                localKs.Add(createLocalK(i,ref m));
                localbs.Add(createLocalb(i,ref m));
            }
            
        }

        //La función recibe:
        //- El elemento actual
        //- La matriz local K
        //- La matriz global K
        //La función inserta la K local en la K global de acuerdo a los nodos
        //del elemento
        public static void assemblyK(element e,Matrix localK,ref Matrix K){
            
            int index1 = e.getNode1() - 1;
            int index2 = e.getNode2() - 1;
            int index3 = e.getNode3() - 1;

            K[index1][index1] += localK[0][0];
            K[index1][index2] += localK[0][1];
            K[index1][index3] += localK[0][2];
            K[index2][index1] += localK[1][0];
            K[index2][index2] += localK[1][1];
            K[index2][index3] += localK[1][2];
            K[index3][index1] += localK[2][0];
            K[index3][index2] += localK[2][1];
            K[index3][index3] += localK[2][2];
        }

        //La función recibe:
        //- El elemento actual
        //- El vector local b
        //- El vector global b
        //La función inserta la b local en la b global de acuerdo a los nodos
        //del elemento
        public static void assemblyb(element e,Vector localb,ref Vector b){
            //Se determinan los nodos del elemento actual como los índices de la b global
            int index1 = e.getNode1() - 1;
            int index2 = e.getNode2() - 1;
            int index3 = e.getNode3() - 1;

            //Se utilizan los índices para definir las celdas del subvector
            //al que se agrega el vector local del elemento actual
            b[index1] += localb[0];
            b[index2] += localb[1];
            b[index3] += localb[2];
        }

        //La función recibe:
        //- El objeto mesh
        //- El arreglo de Ks locales
        //- El arreglo de bs locales
        //- La matriz K global
        //- El vector b global
        //La función se encarga de ensamblar adecuadamente todos los sistemas locales en
        //la K y la b globales
        public static void ensamblaje(ref mesh m,ref List<Matrix> localKs,ref List<Vector> localbs,ref Matrix K, ref Vector b){
            //Se recorren todos los elementos de la malla, uno por uno
            for(int i=0;i<m.getSize((int) sizesE.ELEMENTS);i++){
                //Se extrae del objeto mesh el elemento actual
                element e = m.getElement(i);
                //Se ensamblan la K y la b del elemento actual en las variables globales
                assemblyK(e,localKs[i],ref K);
                assemblyb(e,localbs[i],ref b);
            }
        }

        //La función recibe:
        //- El objeto mesh
        //- El vector b global
        //La función aplica en la b global las condiciones de Neumann en las
        //posiciones que correspondan
        public static void applyNeumann(ref mesh m,ref Vector b){
            //Se recorren las condiciones de Neumann, una por una
            for(int i=0;i<m.getSize((int) sizesE.NEUMANN);i++){
                //Se extrae la condición de Neumann actual
                condition c = m.getCondition(i,(int) sizesE.NEUMANN);
                //En la posición de b indicada por el nodo de la condición,
                //se agrega el valor indicado por la condición
                b[c.getNode1()-1] += c.getValue();
            }
        }

        //La función recibe:
        //- El objeto mesh
        //- La matriz K global
        //- El vector b global
        //La función aplica en la K y b globales las condiciones de Dirichlet, eliminando
        //las filas correspondientes, y enviando desde el lado izquierdo del SEL al lado
        //derecho los valores de las columnas correspondientes
        public static void applyDirichlet(ref mesh m,ref Matrix K,ref Vector b){
            //Se recorren las condiciones de Dirichlet, una por una
            for(int i=0;i<m.getSize((int) sizesE.DIRICHLET);i++){
                //Se extrae la condición de Dirichlet actual
                condition c = m.getCondition(i,(int) sizesE.DIRICHLET);
                //Se establece el nodo de la condición como el índice
                //para K y b globales donde habrá modificaciones
                int index = c.getNode1()-1;

                //Se elimina la fila correspondiente al nodo de la condición
                K.RemoveAt(index); //Se usa un iterator a la posición inicial, y se
                b.RemoveAt(index); //le agrega la posición de interés

                //Se recorren las filas restantes, una por una, de modo que
                //el dato correspondiente en cada fila a la columna del nodo de la
                //condición, se multiplique por el valor de Dirichlet, y se envíe al
                //lado derecho del SEL con su signo cambiado
                for(int row=0;row<K.Count;row++){
                    //Se extrae el valor ubicado en la columna correspondiente
                    //al nodo de la condición
                    double cell = K[row][index];
                    //Se elimina la columna correspondiente
                    //al nodo de la condición
                    K[row].RemoveAt(index);
                    //El valor extraído se multiplica por el valor de la condición,
                    //se le cambia el signo, y se agrega al lado derecho del SEL
                    b[row] += -1*c.getValue()*cell;
                }
            }
        }

        //La función recibe:
        //- La matriz K global
        //- El vector b global
        //- El vector T que contendrá los valores de las incógnitas
        //La función se encarga de resolver el SEL del problema
        public static void calculate(ref Matrix K, ref Vector b, ref Vector T){
            //Se utiliza lo siguiente:
            //      K*T = b
            // (K^-1)*K*T = (K^-1)*b
            //     I*T = (K^-1)*b
            //      T = (K^-1)*b
            //Se prepara la inversa de K
            Matrix Kinv = new Matrix();
            //Se calcula la inversa de K
            Math_tools.inverseMatrix(K,ref Kinv);
            //Se multiplica la inversa de K por b, y el resultado se almacena en T
            Math_tools.productMatrixVector(Kinv,b,ref T);
        }

    }
}