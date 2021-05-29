//Archivo que contiene el procedimiento principal de la implementación,
//describiendo los pasos a seguir para la aplicación del FEM al problema
//de transferencia de calor.

using System;
using System.Collections.Generic;
//Se define un arreglo de reales como una lista
using Vector = System.Collections.Generic.List<double>;

//Se define un arreglo de vectores como una matriz
using Matrix = System.Collections.Generic.List<System.Collections.Generic.List<double>>;

namespace MEF1D_code
{
    class Program
    {
         //Se preparan dos vectores, uno para contener todas las Ks locales de los elementos de la malla
        //y uno para contener todas las bs locales de dichos elementos
        public static List<Matrix> localKs = new List<Matrix>();
        public static List<Vector> localbs = new List<Vector>();
        static void Main(string[] args)
        {
            //Se preparan también las variables para la K y la b globales, y una para las incógnitas de
            //temperatura, que es donde se almacenará la respuesta.
            Matrix K = new Matrix();
            Vector b = new Vector();
            Vector T = new Vector();

            //Se coloca primero una introducción con todas las características de la implementación
            Console.Write( "IMPLEMENTACIÓN DEL MÉTODO DE LOS ELEMENTOS FINITOS\n"
                + "\t- TRANSFERENCIA DE CALOR\n" + "\t- 1 DIMENSIÓN\n"
                + "\t- FUNCIONES DE FORMA LINEALES\n" + "\t- PESOS DE GALERKIN\n"
                + "*********************************************************************************\n\n");

            //Se crea un objeto mesh, que contendrá toda la información de la malla
            mesh m = new mesh();
            //Se procede a obtener toda la información de la malla y almacenarla en m
            Read_tools.leerMallayCondiciones(ref m);

            //Se procede a crear la K local y la b local de cada elemento, almacenando estas
            //estructuras en los vectores localKs y localbs
            Sel_tools.crearSistemasLocales(ref m,ref localKs,ref localbs);
            //Descomentar la siguiente línea para observar las Ks y bs creadas
            //showKs(localKs); showbs(localbs);

            //Se inicializan con ceros la K global y la b global
            Math_tools.zeroes(ref K,m.getSize((int)sizesE.NODES));
            Math_tools.zeroes(ref b,m.getSize((int)sizesE.NODES));
            
            //Se procede al proceso de ensamblaje
            Sel_tools.ensamblaje(ref m,ref localKs,ref localbs,ref K,ref b);
            
            
            //Descomentar la siguiente línea para observar las estructuras ensambladas
            //showMatrix(K); showVector(b);
            //Se aplican primero las condiciones de contorno de Neumann
            Sel_tools.applyNeumann(ref m,ref b);
            //Descomentar la siguiente línea para observar los cambios en b
            //Sel_tools.showVector(b);
            //Luego se aplican las condiciones de contorno de Dirichlet
            Sel_tools.applyDirichlet(ref m,ref K,ref b);
            //Descomentar la siguiente línea para observar el SEL final luego
            //de los cambios provocados por Dirichlet
            //Sel_tools.showMatrix(K); Sel_tools.showVector(b);

            
            //Se prepara con ceros el vector T que contendrá las respuestas
            Math_tools.zeroes(ref T,b.Count);
            
            //Finalmente, se procede a resolver el SEL
            Sel_tools.calculate(ref K,ref b,ref T);

            //Se informa la respuesta:
            Console.Write("La respuesta es: \n");
            Sel_tools.showVector(T);

        }
    }
}
