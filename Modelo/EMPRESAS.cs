using System;

namespace InventarioSalidas.Modelo
{
    public class EMPRESAS
    {
        public EMPRESAS()
        {
        }

        public string NOMBRE { get; set; }
        public int ID { get; set; }
        public EMPRESAS[] EMPRESA { get; set; }

        public override string ToString()
        {
            return NOMBRE;
        }
    }
}